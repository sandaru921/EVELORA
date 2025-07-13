using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.Backend.Models;
using Microsoft.OpenApi.Models;
using AssessmentPlatform.Backend.Service;
using AssessmentPlatform.Backend.Authorization; // Add this for custom auth
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
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
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var roleClaim = context.Principal?.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            if (!string.IsNullOrEmpty(roleClaim))
            {
                context.Principal.AddIdentity(new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, roleClaim) }));
            }
            return Task.CompletedTask;
        }
    };
});

;

// Register Authorization Policies for Permissions 
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanEditQuiz", policy =>
        policy.Requirements.Add(new PermissionRequirement("EditQuiz")));

    options.AddPolicy("CanDeleteQuiz", policy =>
        policy.Requirements.Add(new PermissionRequirement("DeleteQuiz")));

    options.AddPolicy("CanCreateQuestion", policy =>
        policy.Requirements.Add(new PermissionRequirement("CreateQuestion")));

    // Add more policies as needed
});

// Register custom permission handler
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

// Add CORS (adjust origin as needed for frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins("http://localhost:5173")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UserService>();

builder.Services.AddControllers();

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
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();