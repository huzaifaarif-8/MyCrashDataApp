using CrashDataApp.Data;
using CrashDataApp.Models;
using CrashDataApp.Repositories;
using CrashDataApp.Services;
using CrashDataApp.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting CrashDataApp");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) =>
        lc.ReadFrom.Configuration(ctx.Configuration));

    builder.Services.AddControllers(options =>
        options.Filters.Add<ValidationFilter>());
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Enter your JWT token"
        });
        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddDbContext<CrashContext>(options =>
        options.UseSqlite(
            builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=crashes.db"));

    builder.Services.AddScoped<ICrashRepository, CrashRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();

    builder.Services.AddScoped<ICrashService, CrashService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IAuthService, AuthService>();

    builder.Services.AddSingleton<DapperContext>();
    builder.Services.AddScoped<AnalyticsRepository>();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
            };
        });

    builder.Services.AddCors(options =>
        options.AddPolicy("Angular", policy =>
            policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()));

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<CrashContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        CsvImporter.SeedIfEmpty(context);

        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL
            )");

        if (!context.Users.Any())
        {
            var username = config["DefaultAdmin:Username"] ?? "admin";
            var password = config["DefaultAdmin:Password"] ?? "Admin@123";
            context.Users.Add(new AppUser
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            });
            context.SaveChanges();
            Log.Information("Default admin user {Username} created", username);
        }

        var analytics = scope.ServiceProvider.GetRequiredService<AnalyticsRepository>();
        await analytics.InitializeSchemaAsync();

        var operatorStats = context.Crashes
            .Where(c => c.Operator != null)
            .GroupBy(c => c.Operator!)
            .Select(g => new OperatorStat
            {
                OperatorName = g.Key,
                TotalCrashes = g.Count(),
                TotalFatalities = g.Sum(c => c.Fatalities ?? 0),
                TotalAboard = g.Sum(c => c.Aboard ?? 0),
                FirstCrashYear = g.Min(c => c.Year),
                LastCrashYear = g.Max(c => c.Year)
            })
            .ToList();

        await analytics.UpsertOperatorStatsAsync(operatorStats);
        Log.Information("Analytics DB seeded with {Count} operator records via Dapper", operatorStats.Count);
    }

    app.UseRouting();

    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.Use(async (ctx, next) =>
    {
        if (ctx.Request.Method == "OPTIONS")
        {
            ctx.Response.Headers.Append("Access-Control-Allow-Private-Network", "true");
        }
        await next();
    });

    app.UseCors("Angular");

    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Crash Data API v1"));

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers().RequireCors("Angular");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
