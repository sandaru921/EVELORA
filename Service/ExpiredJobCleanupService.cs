using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AssessmentPlatform.Backend.Data;
using Azure.Storage.Blobs;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AssessmentPlatform.Backend.Services
{
    public class ExpiredJobCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiredJobCleanupService> _logger;
        private readonly string _connectionString = "DefaultEndpointsProtocol=https;AccountName=storage1evelora;AccountKey=ToyWFg2+nWEc5C9P9Ekrk+rifpXx4dvpnkm1tmUHSK3g/PO06yuhpQ9A2TSzMHrqcL3dWOAphkBt+AStk0smYQ==;EndpointSuffix=core.windows.net";
        private readonly string _containerName = "jobs";
        private readonly TimeSpan _interval = TimeSpan.FromHours(24); // Run every 24 hours

        public ExpiredJobCleanupService(IServiceProvider serviceProvider, ILogger<ExpiredJobCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Expired Job Cleanup Service started at {Time}", DateTime.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DeleteExpiredJobsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while deleting expired jobs: {Message}", ex.Message);
                }

                // Wait for the next interval
                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Expired Job Cleanup Service stopped at {Time}", DateTime.UtcNow);
        }

        private async Task DeleteExpiredJobsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            // Get current date (midnight UTC) for comparison
            var currentDate = DateTime.UtcNow.Date;

            // Find jobs where ExpiringDate is in the past
            var expiredJobs = dbContext.Jobs
                .Where(j => j.ExpiringDate < currentDate)
                .ToList();

            if (!expiredJobs.Any())
            {
                _logger.LogInformation("No expired jobs found at {Time}", DateTime.UtcNow);
                return;
            }

            _logger.LogInformation("Found {Count} expired jobs to delete at {Time}", expiredJobs.Count, DateTime.UtcNow);

            // Delete associated JobQuizzes
            var jobIds = expiredJobs.Select(j => j.Id).ToList();
            var jobQuizzes = dbContext.JobQuizzes.Where(jq => jobIds.Contains(jq.JobId));
            dbContext.JobQuizzes.RemoveRange(jobQuizzes);
            _logger.LogInformation("Removed {Count} JobQuizzes for expired jobs", jobQuizzes.Count());

            // Delete associated blobs
            foreach (var job in expiredJobs)
            {
                if (!string.IsNullOrEmpty(job.ImageUrl))
                {
                    try
                    {
                        var blobName = Path.GetFileName(new Uri(job.ImageUrl).AbsolutePath);
                        var blobClient = containerClient.GetBlobClient(blobName);
                        await blobClient.DeleteIfExistsAsync();
                        _logger.LogInformation("Deleted blob {BlobName} for JobId: {JobId}", blobName, job.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deleting blob for JobId: {JobId}: {Message}", job.Id, ex.Message);
                    }
                }
            }

            // Delete the jobs
            dbContext.Jobs.RemoveRange(expiredJobs);

            // Save changes
            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully deleted {Count} expired jobs at {Time}", expiredJobs.Count, DateTime.UtcNow);
        }
    }
}