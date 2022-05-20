using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddUserSecrets<Program>()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables();

var configuration = builder.Build();

var uris = configuration.GetSection("uris").Get<string[]>();
var frequency = configuration.GetValue<int>("frequency");
var location = configuration.GetValue<string>("location");
var connectionstring = configuration.GetValue<string>("connectionstring");

Console.WriteLine($"Starting availability monitoring of {uris.Length} endpoints every {frequency} seconds...");

var client = new HttpClient();
var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
telemetryConfiguration.ConnectionString = connectionstring;
var telemetryClient = new TelemetryClient(telemetryConfiguration);

while (true)
{
    Console.WriteLine($"Endpoint availability check {DateTimeOffset.UtcNow}");

    foreach (var uri in uris)
    {
        var start = DateTimeOffset.UtcNow;
        var hostname = new Uri(uri).Host;

        try
        {
            var response = await client.GetAsync(uri);
            var end = DateTimeOffset.UtcNow;

            telemetryClient.TrackAvailability(
                name: hostname,
                timeStamp: start,
                duration: end - start,
                runLocation: location,
                success: response.IsSuccessStatusCode,
                properties: new Dictionary<string, string>(), metrics: new Dictionary<string, double>());
        }
        catch (Exception ex)
        {
            var end = DateTimeOffset.UtcNow;
            telemetryClient.TrackAvailability(
                name: hostname,
                timeStamp: start,
                duration: end - start,
                runLocation: location,
                success: false,
                message: ex.Message);
        }
    }

    telemetryClient.Flush();
    await Task.Delay(TimeSpan.FromSeconds(frequency));
}