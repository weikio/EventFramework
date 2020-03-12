using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Weikio.AspNetCore.StartupTasks;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Router
{
    public class RouteInitializationStartupTask : IStartupTask
    {
        private readonly IEnumerable<ICloudEventRoute> _routes;
        private readonly RouteInitializer _initializer;

        public RouteInitializationStartupTask(IEnumerable<ICloudEventRoute> routes, RouteInitializer initializer)
        {
            _routes = routes;
            _initializer = initializer;
        }

        public Task Execute(CancellationToken cancellationToken)
        {
            foreach (var route in _routes)
            {
                _initializer.Initialize(route);
            }
            
            return Task.CompletedTask;
        }
    }
}
