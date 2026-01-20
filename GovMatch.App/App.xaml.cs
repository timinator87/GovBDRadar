using System.IO;
using System.Windows;
using GovMatch.App.Services;
using GovMatch.App.ViewModels;
using GovMatch.Core.Interfaces;
using GovMatch.Core.Services;
using GovMatch.Data;
using GovMatch.Data.Repositories;
using GovMatch.Data.Services;
using GovMatch.Integrations.SamGov.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace GovMatch.App;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GovMatch");

        Directory.CreateDirectory(appDataPath);
        var dbPath = Path.Combine(appDataPath, "govmatch.db");

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton(sp =>
                {
                    var dbContext = new DatabaseContext(dbPath);
                    dbContext.InitializeDatabaseAsync().Wait();
                    return dbContext;
                });

                services.AddSingleton<OpportunityRepository>();
                services.AddSingleton<MatchResultRepository>();
                services.AddSingleton<CompanyProfileRepository>();
                services.AddSingleton<UserActionRepository>();
                services.AddSingleton<RetrievalRuleRepository>();
                services.AddSingleton<AppSettingsRepository>();

                services.AddSingleton(sp =>
                {
                    var settingsRepo = sp.GetRequiredService<AppSettingsRepository>();
                    var settings = settingsRepo.GetSettingsAsync().Result;
                    var dbContext = sp.GetRequiredService<DatabaseContext>();
                    return new RateLimiter(dbContext, settings.DailyRateLimitTier);
                });

                services.AddSingleton(sp =>
                {
                    var settingsRepo = sp.GetRequiredService<AppSettingsRepository>();
                    var settings = settingsRepo.GetSettingsAsync().Result;
                    var apiKey = !string.IsNullOrWhiteSpace(settings.EncryptedApiKey)
                        ? SecureStorageService.DecryptString(settings.EncryptedApiKey)
                        : "";
                    return new SamGovApiClient(apiKey, settings.Environment);
                });

                services.AddSingleton<IScoringEngine>(sp =>
                {
                    var settingsRepo = sp.GetRequiredService<AppSettingsRepository>();
                    var settings = settingsRepo.GetSettingsAsync().Result;
                    return new TfIdfScoringEngine(settings.StructuredWeight, settings.TextWeight, settings.AgencyWeight);
                });

                services.AddSingleton<SyncService>();
                services.AddSingleton<ExportService>();

                services.AddSingleton<InboxViewModel>();
                services.AddSingleton<CompanyProfileViewModel>();
                services.AddSingleton<RetrievalRulesViewModel>();
                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<SyncLogsViewModel>();
                services.AddSingleton<MainViewModel>();

                services.AddSingleton<MainWindow>();

                services.AddQuartz(q =>
                {
                    q.UseMicrosoftDependencyInjectionJobFactory();

                    var jobKey = new JobKey("SyncJob");
                    q.AddJob<SyncJob>(opts => opts.WithIdentity(jobKey));

                    q.AddTrigger(opts => opts
                        .ForJob(jobKey)
                        .WithIdentity("SyncJob-trigger")
                        .WithCronSchedule("0 0 6 * * ?"));
                });

                services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            })
            .Build();

        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
