using FlatScannerWeb.DataAccess;
using FlatScannerWeb.Entities;
using FlatScannerWeb.Providers;
using FlatScannerWeb.Providers.Factory;
using FlatScannerWeb.Services;

namespace FlatScannerWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders()
                .AddConsole(c => c.TimestampFormat = "[dd.MM.yy HH:mm:ss:fff] ")
                .AddProvider(new StaticLoggerProvider());

            var services = builder.Services;
            
            var appSettings = builder.Configuration.Get<AppSettings>() ?? new();
            services.AddSingleton(appSettings);

            services.Configure<MailSmtpOptions>(o =>
            {
                o.Enabled = appSettings.Mail.Smtp.Enabled;
                o.Name = appSettings.Mail.Smtp.Name;
                o.Mail = appSettings.Mail.Smtp.Mail;
                o.Password = appSettings.Mail.Smtp.Password;
            });

            services.Configure<MailRecipientsOptions>(o =>
            {
                o.NotificationRecipients = appSettings.Mail.NotificationRecipients;
                o.DevTeamRecipients = appSettings.Mail.DevTeamRecipients;
            });

            services.AddHttpClient();

            services.AddTransient<IProviderFactory, ProviderFactory>();
            services.AddTransient<IFlatProvider, NepremicnineProvider>();
            services.AddTransient<IFlatProvider, BolhaProvider>();
            services.AddTransient<IFlatProvider, Century21Provider>();
            services.AddTransient<IFlatProvider, Nep24Provider>();
            services.AddTransient<IFlatProvider, DoDomaProvider>();
            services.AddTransient<IFlatProvider, GaleaProvider>();
            services.AddTransient<IFlatProvider, RandomFlatProvider>();

            services.AddTransient<IMailService, MailService>();

            services.AddSingleton<IDatabase, InMemoryDatabase>();

            services.AddHostedService<FlatScannerService>();

            var app = builder.Build();

            app.MapGet("/", () => $"Scheduler working... {Environment.NewLine}" +
            $"Service output (last {Constants.OldLogsTimeoutHours} hours):{Environment.NewLine}{StaticLogger.GetLogs()}");

            app.Run();
        }
    }
}