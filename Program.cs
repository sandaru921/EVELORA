using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.Backend.Models;
using Microsoft.OpenApi.Models;
using AssessmentPlatform.Backend.Services;
using AssessmentPlatform.Backend.Repositories;
using AssessmentPlatform.Backend.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Azure.Storage.Blobs;
using DotNetEnv;
using System.Text;

// Load .env
DotNetEnv.Env.Load();

// Environment variables
var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
var azureBlobConnectionString = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING");
var blobJobContainerName = Environment.GetEnvironmentVariable("BLOB_JOB_CONTAINER_NAME");
var blobBlogContainerName = Environment.GetEnvironmentVariable("BLOB_BLOG_CONTAINER_NAME");

var jwtSettings = new JwtSettings
{
    SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "fallbackSecretKey",
    Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
    Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
    ExpireMinutes = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRE_MINUTES"), out var exp) ? exp : 60
};

var builder = WebApplication.CreateBuilder(args);

// Optional: inject env vars into config if needed
builder.Configuration["ConnectionStrings:DefaultConnection"] = dbConnectionString ?? "";
builder.Configuration["AzureBlobStorage:ConnectionString"] = azureBlobConnectionString ?? "";
builder.Configuration["AzureBlobStorage:JobContainerName"] = blobJobContainerName ?? "jobs";
builder.Configuration["AzureBlobStorage:BlogContainerName"] = blobBlogContainerName ?? "blogs";

// Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// JWT
builder.Services.Configure<JwtSettings>(options =>
{
    options.SecretKey = jwtSettings.SecretKey;
    options.Issuer = jwtSettings.Issuer;
    options.Audience = jwtSettings.Audience;
    options.ExpireMinutes = jwtSettings.ExpireMinutes;
});
var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanEditQuiz", policy => policy.Requirements.Add(new PermissionRequirement("EditQuiz")));
    options.AddPolicy("CanDeleteQuiz", policy => policy.Requirements.Add(new PermissionRequirement("DeleteQuiz")));
    options.AddPolicy("CanCreateQuestion", policy => policy.Requirements.Add(new PermissionRequirement("CreateQuestion")));
});
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

// CORS
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowFrontend", policy =>
//     {
//         policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
//               .AllowAnyHeader()
//               .AllowAnyMethod()
//               .AllowCredentials();
//     });
// });
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy

            .WithOrigins("http://localhost:5173", "https://localhost:5173", "http://localhost:5174") // Your React dev server
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});

// DB + Blob
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dbConnectionString));
builder.Services.AddSingleton(x => new BlobServiceClient(azureBlobConnectionString));

// Services + Repos
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IBlogRepository, BlogRepository>();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AssessmentPlatform API", Version = "v1" });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token **_only_**",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, new string[] { } }
    });
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// DB Ensure + Migration
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        logger.LogInformation("Ensuring database is created...");
        context.Database.EnsureCreated();

        var pendingMigrations = context.Database.GetPendingMigrations();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying pending migrations...");
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database migration.");
    }
}

app.Run();
