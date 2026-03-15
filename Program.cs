
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PRM393_Travel_Planner_BE.Models;
using PRM393_Travel_Planner_BE.Repositories.Implementations;
using PRM393_Travel_Planner_BE.Repositories.Interfaces;
using PRM393_Travel_Planner_BE.Services.Implementations;
using PRM393_Travel_Planner_BE.Services.Interfaces;
using TravelApp.API.API.Middleware;
using TravelApp.API.Application.Interfaces;
using TravelApp.API.Application.Services;

namespace PRM393_Travel_Planner_BE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Bật legacy timestamp behavior cho Npgsql để fix lỗi xung đột DateTime khi từ SQL Server sang PostgreSQL
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var builder = WebApplication.CreateBuilder(args);

            // 1. Cấu hình Database (Hỗ trợ cả Railway URL và Local Connection String)
            var databaseUrl = Environment.GetEnvironmentVariable("MY_CUSTOM_DB_URL")
               ?? Environment.GetEnvironmentVariable("DATABASE_URL");
            string connectionString;

            if (!string.IsNullOrEmpty(databaseUrl))
            {
                // Logic cho môi trường Production (Railway)
                connectionString = ConvertRailwayUrlToConnectionString(databaseUrl);
            }
            else
            {
                // Logic cho môi trường Local
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
            }

            // 2. Use PostgreSQL
            builder.Services.AddDbContext<Prm393TravelPlannerContext>(options =>
                options.UseNpgsql(connectionString));

            // ── JWT Authentication ────────────────────────────────────────────────────────
            var jwtSecret = builder.Configuration["Jwt:Secret"]!;

            builder.Services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero   // không cho phép trễ
                };
            });

            builder.Services.AddAuthorization();

            // ── Dependency Injection ──────────────────────────────────────────────────────
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            builder.Services.AddScoped<ITripRepository, TripRepository>();
            builder.Services.AddScoped<ITripDayRepository, TripDayRepository>();
            builder.Services.AddScoped<ITripActivityRepository, TripActivityRepository>();
            builder.Services.AddScoped<IChecklistRepository, ChecklistRepository>();
            builder.Services.AddScoped<IChecklistItemRepository, ChecklistItemRepository>();

            // ── Services ──────────────────────────────────────────────────────────────────
            builder.Services.AddScoped<ITripService, TripService>();
            builder.Services.AddScoped<ITripDayService, TripDayService>();
            builder.Services.AddScoped<ITripActivityService, TripActivityService>();
            builder.Services.AddScoped<IChecklistService, ChecklistService>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            // ── CORS ──────────────────────────────────────────────────────────────────────
            builder.Services.AddCors(opt =>
                opt.AddDefaultPolicy(policy =>
                    policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["*"])
                          .AllowAnyHeader()
                          .AllowAnyMethod()));

            // ── Swagger ───────────────────────────────────────────────────────────────────
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new OpenApiInfo { Title = "TravelApp API", Version = "v1" });

                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập JWT token. Ví dụ: Bearer {token}"
                });

                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {{
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }});
            });

            builder.Services.AddControllers();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<Prm393TravelPlannerContext>();
                db.Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ExceptionMiddleware>();

            //app.UseHttpsRedirection();

            app.UseCors();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.MapGet("/", () => Results.Redirect("/swagger"));

            var connectionString1 = builder.Services.BuildServiceProvider().GetRequiredService<Prm393TravelPlannerContext>()
                       .Database.GetConnectionString();
            Console.WriteLine($"--- ĐANG KẾT NỐI TỚI: {connectionString1} ---");

            app.Run();
        }

        private static string ConvertRailwayUrlToConnectionString(string databaseUrl)
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':');
            if (userInfo.Length != 2) throw new Exception("DATABASE_URL không hợp lệ");

            return $"Host={uri.Host};" +
                   $"Port={uri.Port};" +
                   $"Database={uri.AbsolutePath.TrimStart('/')};" +
                   $"Username={userInfo[0]};" +
                   $"Password={userInfo[1]};" +
                   $"Ssl Mode=Require;Trust Server Certificate=true;";
        }
    }
}
