// using System;
// using System.Collections.Generic;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
//
// namespace Weikio.EventFramework.Files
// {
//     public class EventSourceHostingService : IHostedService, IDisposable
//     {
//         private readonly ILogger<EventSourceHostingService> _logger;
//         private readonly IServiceProvider _serviceProvider;
//         private Func<Task> _start;
//         private Func<Task> _stop;
//         private Func<List<object>> _run;
//         private Action _dispose;
//
//         public EventSourceHostingService(ILogger<EventSourceHostingService> logger, IServiceProvider serviceProvider)
//         {
//             _logger = logger;
//             _serviceProvider = serviceProvider;
//         }
//
//         public void Initialize(Func<Task> start = null, Func<Task> stop = null, Func<List<object>> run = null, Action dispose = null)
//         {
//             _logger.LogDebug("Initializing event source host");
//             _start = start;
//             _stop = stop;
//             _run = run;
//             _dispose = dispose;
//             
//             IsInitialized = true;
//         }
//
//         public bool IsInitialized { get; set; }
//
//         public Task StartAsync(CancellationToken cancellationToken)
//         {
//             if (_start != null)
//             {
//                 return _start();
//             }
//
//             return Task.CompletedTask;
//         }
//
//         public Task StopAsync(CancellationToken cancellationToken)
//         {
//             if (_stop != null)
//             {
//                 return _stop();
//             }
//
//             return Task.CompletedTask;
//         }
//
//         public void Dispose()
//         {
//             _dispose?.Invoke();
//         }
//     }
// }
