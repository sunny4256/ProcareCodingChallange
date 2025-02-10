//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Procare Software, LLC">
//     Copyright © 2021-2025 Procare Software, LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Procare.AddressValidation.Tester;

using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;
using Serilog;

internal sealed class Program
{
    private static async Task Main(string[] args)
    {

        IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddLogging((logger) =>
                {
                    var serilogLogger = new LoggerConfiguration()
                        .MinimumLevel.Information()
                        .WriteTo.Console(
                            outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}",
                            formatProvider: CultureInfo.InvariantCulture)
                        .WriteTo.File(
                            path: "logs/app-.txt",
                            rollingInterval: RollingInterval.Minute,
                            outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}",
                            formatProvider: CultureInfo.InvariantCulture,
                            fileSizeLimitBytes: 10 * 1024 * 1024,
                            retainedFileCountLimit: 7,
                            rollOnFileSizeLimit: true)
                        .CreateLogger();

                    logger.ClearProviders();
                    logger.AddSerilog(serilogLogger);
                });

                services.AddHttpClient(AddressValidationService.HttpClientName, client =>
                {
                    client.BaseAddress = new("https://addresses.dev-procarepay.com");
                })

                // https://learn.microsoft.com/en-us/dotnet/core/resilience/?tabs=dotnet-cli
                // https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience?tabs=dotnet-cli

                // Add a resilience pipeline to the named client. This pipeline will be used for all requests made by the client. 
                // This is the recommended approach.
    
                .AddResilienceHandler("custom", pipeline =>
                {
                    // this is the retry policy. It will retry 3 times with exponential backoff and jitter.
                    _ = pipeline.AddRetry(new HttpRetryStrategyOptions
                    {
                        MaxRetryAttempts = 3,
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true,
                        Delay = TimeSpan.FromMilliseconds(100),

                        // Refer :  https://www.pollydocs.org/strategies/#fault-handling
                        // The predicate to determine whether the request should be retried. If not specified, the default is to retry on all transient exceptions.
                        ShouldHandle = args => args.Outcome switch
                        {
                            { Exception: HttpRequestException } => PredicateResult.True(),
                            { Exception: TimeoutRejectedException } => PredicateResult.True(),

                             { Result: HttpResponseMessage response }
                                when (int)response.StatusCode >= 500 && (int)response.StatusCode < 600
                                => PredicateResult.True(),
                            _ => PredicateResult.False(),
                        },
                    });
                    // this is the timeout policy. It will timeout after 750 milli seconds.
                    pipeline.AddTimeout(TimeSpan.FromMilliseconds(750));
                     
                    // this is the circuit breaker policy. It will break the circuit if 50% of the requests fail with a 5xx status code 
                    // within a 1 minute window and will stay open for 30 seconds.
                    pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                    {
                        FailureRatio = 0.5,
                        SamplingDuration = TimeSpan.FromMinutes(1),
                        MinimumThroughput = 10,
                        BreakDuration = TimeSpan.FromSeconds(30),
                    });
                });

                services.AddTransient<AddressValidationService>();

                services.AddHostedService((services) => new ConsoleRunner(
                    services.GetRequiredKeyedService<ILogger<ConsoleRunner>>(null),
                    services.GetRequiredService<IHostApplicationLifetime>(),
                    services.GetRequiredKeyedService<AddressValidationService>(null),
                    args));
            });

        await hostBuilder.RunConsoleAsync().ConfigureAwait(false);
    }
}
