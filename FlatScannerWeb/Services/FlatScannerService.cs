using FlatScannerWeb.DataAccess;
using FlatScannerWeb.Entities;
using FlatScannerWeb.Helpers;
using FlatScannerWeb.Providers;
using FlatScannerWeb.Providers.Factory;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace FlatScannerWeb.Services;

public class FlatScannerService : BackgroundService
{
    private readonly IProviderFactory _providerFactory;
    private readonly IDatabase _database;
    private readonly IMailService _mailService;
    private readonly MailRecipientsOptions _mailRecipientsOptions;
    private readonly ILogger<FlatScannerService> _logger;
    private readonly IEnumerable<IFlatProvider> _providers;

    private ConcurrentBag<Exception> _scanExceptions = new();

    public FlatScannerService(
        IProviderFactory providerFactory,
        IDatabase database,
        IMailService mailService,
        IOptions<MailRecipientsOptions> mailRecipientsOptions,
        ILogger<FlatScannerService> logger)
    {
        _providerFactory = providerFactory;
        _database = database;
        _mailService = mailService;
        _mailRecipientsOptions = mailRecipientsOptions.Value;
        _logger = logger;

        _providers = _providerFactory.CreateProviders();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background service is starting.");

        // Timer to scan flats and notify about new flats published
        var scanFlatsTimer = new Timer(
            async _ => await SafeTimerCallbackInvoke(CheckFlats),
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(Constants.ScanInternalMinutes));

        // Timer to check if any exceptions occured while scanning flats and notify dev team about them
        var checkExceptionTimer = new Timer(
            async _ => await SafeTimerCallbackInvoke(CheckExceptions),
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(Constants.ExceptionInternalMinutes));

        // Just a loop for service to stay alive
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("Background service is stopping.");
    }

    private async Task CheckFlats()
    {
        try
        {
            await CheckNewFlatsAndNotify();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "General exception occured.");
            await _mailService.SendEmail(_mailRecipientsOptions.DevTeamRecipients, "FlatScanner - General exception occured", ex.ToString());
        }

        _logger.LogInformation($"Waiting {Constants.ScanInternalMinutes} minutes for next flats scan.");
    }

    private async Task CheckNewFlatsAndNotify()
    {
        _logger.LogInformation("Scanning started.");

        var scanTask = GetFlats();
        var getExistingFlatsTask = _database.GetFlats();

        await Task.WhenAll(scanTask, getExistingFlatsTask);

        var scannedFlats = scanTask.Result;
        var existingFlats = getExistingFlatsTask.Result;

        var newFlats = scannedFlats.ExceptBy(existingFlats.Select(p => p.Id), p => p.Id);

        if (newFlats.Any())
        {
            _logger.LogInformation($"Found {newFlats.Count()} new flats, sending mail and saving new flats into database...");

            await SendNewFlatsMail(newFlats);
            await _database.SaveFlats(newFlats);
        }

        _logger.LogInformation("Scanning stopped.");
    }

    private async Task<IEnumerable<FlatEntity>> GetFlats()
    {
        var tasks = _providers.Select(GetProviderFlats);
        var results = await Task.WhenAll(tasks);
        return results.SelectMany(x => x);
    }

    private async Task<IEnumerable<FlatEntity>> GetProviderFlats(IFlatProvider provider)
    {
        IEnumerable<FlatEntity> result = new List<FlatEntity>();

        try
        {
            result = await provider.GetFlats();

            _logger.LogInformation($"Provider {provider.GetType().Name} found {result.Count()} flats.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception occured while checking {provider.GetType().Name} provider flats.");

            _scanExceptions.Add(ex);
        }

        return result;
    }

    private async Task CheckExceptions()
    {
        _logger.LogInformation("Checking exceptions to be sent to the dev team...");

        if (_scanExceptions.Any())
        {
            _logger.LogInformation($"{_scanExceptions.Count} exception occured, sending mail...");

            await SendExceptionsMail(_scanExceptions);

            _scanExceptions.Clear();
        }
        else
        {
            _logger.LogInformation("No exceptions found, be proud.");
        }

        _logger.LogInformation($"Waiting {Constants.ExceptionInternalMinutes} minutes for next checking for exceptions.");
    }

    private async Task SendNewFlatsMail(IEnumerable<FlatEntity> newFlats)
    {
        var body = MailComposerHelper.CreateNewFlatsMailBody(newFlats);

        Directory.CreateDirectory("test_files");

        await Task.WhenAll(
            File.WriteAllTextAsync("test_files/mail.html", body),
            _mailService.SendEmail(_mailRecipientsOptions.NotificationRecipients, "FlatScanner - Nova stanovanja na voljo", body));
    }

    private async Task SendExceptionsMail(ConcurrentBag<Exception> exceptions)
    {
        var body = MailComposerHelper.CreateExceptionsMailBody(exceptions);
        await _mailService.SendEmail(_mailRecipientsOptions.DevTeamRecipients, "FlatScanner - Porocilo o napakah", body);
    }

    private async Task SafeTimerCallbackInvoke(Func<Task> func)
    {
        try
        {
            await func();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occured inside timer callback which should terminate application, check logs...");
        }
    }
}
