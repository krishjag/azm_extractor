using AzureMonitor.Extractor;

namespace AzureMonitor.Scanner.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int counter = 0;
            int iterations = Convert.ToInt32(_config["NumberOfIterations"]);
            while (!stoppingToken.IsCancellationRequested && counter < iterations)
            {
                counter++;
                Extractor.Extractor.Extract(_logger, _config, stoppingToken);

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await Task.Delay(1000 * Convert.ToInt32(_config["IterationIntervalInSeconds"]), stoppingToken);
            }

            _logger.LogInformation("Process completed.");
            Environment.Exit(0);
        }
    }
}