using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using backendChatApplication.Models;
using backendChatApplication.Services;
using backendChatApplication;
using System.Security.Claims;
using backendChatApplcation.Services;
using Microsoft.AspNetCore.Http.Connections;
using backendChatApplication.Hubs;

namespace backendChatApplcation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
            builder.Logging.AddConsole();

            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            Configure(app);

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DBConn");

            services.AddDbContext<chatDataContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));

            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials()
                           .WithOrigins("http://localhost:5180"); 
                });
            });



            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                         NameClaimType = ClaimTypes.Email
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var token = context.Request.Query["access_token"];
                            if(!string.IsNullOrEmpty(token))
                            {
                                context.Token = token;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });



            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });
            services.AddTransient<chatHub>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<IchatServices,chatServices>();
            services.AddScoped<IUserServices, UserServices>();
            services.AddScoped<IFileService, fileServices>();
        }

        private static void Configure(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors("CorsPolicy");

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHub<chatHub>("/chathub");



           /* app.MapHub<chatHub>("/chathub", options =>
            {
                options.Transports =
                    HttpTransportType.WebSockets |
                    HttpTransportType.LongPolling;
            }*/

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<chatDataContext>();
                context.Database.Migrate();
                context.SeedData();
            }
        }
    }
}
