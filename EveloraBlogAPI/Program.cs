

// using Microsoft.EntityFrameworkCore;
// using EveloraBlogAPI.Data;
// using Microsoft.Extensions.FileProviders;

// var builder = WebApplication.CreateBuilder(args);

// // Add services to the container.
// builder.Services.AddControllers();

// // Add PostgreSQL database context
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// // Add CORS
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowReactApp",
//         builder => builder
//             .WithOrigins("http://localhost:5173", "http://localhost:5174") // Your React app URLs
//             .AllowAnyMethod()
//             .AllowAnyHeader()
//             .AllowCredentials());
// });

// // Add Swagger
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// app.UseHttpsRedirection();

// // Configure static files for serving images
// app.UseStaticFiles();
// app.UseStaticFiles(new StaticFileOptions
// {
//     FileProvider = new PhysicalFileProvider(
//         Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
//     RequestPath = ""
// });

// // Ensure wwwroot/images/blogs directory exists
// var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "blogs");
// if (!Directory.Exists(uploadsDir))
// {
//     Directory.CreateDirectory(uploadsDir);
// }

// app.UseCors("AllowReactApp");
// app.UseAuthorization();
// app.MapControllers();

// // Apply migrations automatically in development
// if (app.Environment.IsDevelopment())
// {
//     using (var scope = app.Services.CreateScope())
//     {
//         var services = scope.ServiceProvider;
//         try
//         {
//             var context = services.GetRequiredService<ApplicationDbContext>();
            
//             // Create database if it doesn't exist
//             context.Database.EnsureCreated();
            
//             // Apply any pending migrations
//             if (context.Database.GetPendingMigrations().Any())
//             {
//                 context.Database.Migrate();
//             }
            
//             // Seed initial data
//             DbInitializer.Initialize(context);
//         }
//         catch (Exception ex)
//         {
//             var logger = services.GetRequiredService<ILogger<Program>>();
//             logger.LogError(ex, "An error occurred while migrating or seeding the database.");
//         }
//     }
// }

// app.Run();

using Microsoft.EntityFrameworkCore;
using EveloraBlogAPI.Data;
using Microsoft.Extensions.FileProviders;
using EveloraBlogAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add PostgreSQL database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add repositories
builder.Services.AddScoped<IBlogRepository, BlogRepository>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder
            .WithOrigins("http://localhost:5173", "http://localhost:5174") // Your React app URLs
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Configure static files for serving images
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = ""
});

// Ensure wwwroot/images/blogs directory exists
var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "blogs");
if (!Directory.Exists(uploadsDir))
{
    Directory.CreateDirectory(uploadsDir);
}

app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers();

// Initialize and migrate database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Attempting to ensure database is created...");
        // Create database if it doesn't exist
        context.Database.EnsureCreated();
        
        logger.LogInformation("Checking for pending migrations...");
        // Check if there are any pending migrations
        if (context.Database.GetPendingMigrations().Any())
        {
            logger.LogInformation("Applying pending migrations...");
            context.Database.Migrate();
        }
        
        logger.LogInformation("Initializing database with seed data...");
        // Seed initial data
        DbInitializer.Initialize(context);
        logger.LogInformation("Database initialization complete.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while setting up the database.");
    }
}

app.Run();




