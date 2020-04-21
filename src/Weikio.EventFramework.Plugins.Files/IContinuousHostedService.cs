using System.Threading;
using System.Threading.Tasks;

namespace Weikio.EventFramework.Files
{
    public interface IContinuousHostedService
    {
        Task StartAsync(CancellationToken stoppingToken);
        Task StopAsync(CancellationToken stoppingToken);
        void Dispose();
    }
}