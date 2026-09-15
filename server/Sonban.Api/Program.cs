using System.Text.Json.Serialization;
using NLog;
using Sonban.Api.Classes;
using Sonban.Api.Repository;
using Sonban.Api.Services;

namespace Sonban.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
            logger.Debug("Logger initialized.");
            builder.Services.AddTransient<NLog.ILogger>(_ => logger);
            builder.Services.AddTransient<ILoggerFactory>(_ => new NLog.Extensions.Logging.NLogLoggerFactory());
            builder.Services.AddTransient(provider => provider.GetService<ILoggerFactory>().CreateLogger("Generic"));

            ConfigureServices(builder.Services, builder.Configuration);

            builder.Services.AddControllers(config => {
                // config.Filters.Add(typeof(AuthCookieCheckFilterAttribute));
            }).AddJsonOptions(x => {
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            app.UseCors(policy => {
                policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin()
                    //.AllowCredentials()
                    //.WithOrigins(allowedHosts)
                    ;
            });

            app.UseMiddleware<ErrorHandlingMiddleware>();

            if (app.Environment.IsDevelopment()) {
                app.UseSwagger();
                app.UseSwaggerUI(c => {
                    c.DefaultModelsExpandDepth(-1);
                    // c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");
                });
            }
            else {
                app.UseHttpsRedirection();
            }

            app.UseAuthorization();

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.MapFallbackToFile("index.html");

            app.MapControllers();

            var appServiceUrl = builder.Configuration.GetValue<string>("appServiceUrl");
            app.Run(appServiceUrl);
        }

        /// <summary>
        /// Custom services registration
        /// </summary>
        /// <param name="services">Services collection</param>
        /// <param name="configuration">Configuration & parameters</param>
        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration) {

            services.AddSingleton<ISettingsProvider>(_ => new SettingsProvider(configuration));

            services.AddTransient<IUsersService, UsersService>();
            services.AddTransient<IUsersRepository, UsersRepository>();
            services.AddTransient<IMcpTokenRepository, McpTokenRepository>();
            services.AddSingleton<IAccessTokenLastUsedUpdater, AccessTokenLastUsedUpdater>();

            services.AddTransient<IAccountsRepository, AccountsRepository>();
            services.AddTransient<IAccountsService, AccountsService>();

            services.AddTransient<IProductsRepository, ProductsRepository>();
            services.AddTransient<IProductsService, ProductsService>();

            services.AddTransient<IBooksRepository, BooksRepository>();
            services.AddTransient<IBooksService, BooksService>();
            
            services.AddTransient<IAudioBooksRepository, AudioBooksRepository>();
            services.AddTransient<IAudioBooksService, AudioBooksService>();

            services.AddTransient<IMcpRepository, McpRepository>();
            services.AddTransient<IMcpService, McpService>();

            services.AddTransient<IEmailService, EmailService>();


            //services.AddSingleton<ISystemAdminService>(provider => new SystemAdminService(
            //    provider.GetService<ISystemAdminRepository>()
            //));
            //services.AddSingleton<ISystemAdminRepository>(provider => new SystemAdminRepository(
            //    provider.GetService<ISettingsProvider>()
            //));
        }
    }
}
