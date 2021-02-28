// using System;
// using System.Collections.Generic;
// using System.Text;
// using System.Threading;
// using System.Threading.Channels;
// using System.Threading.Tasks;
// using CloudNative.CloudEvents;
// using Microsoft.Azure.Amqp.Serialization;
// using Microsoft.Azure.ServiceBus;
// using Microsoft.Azure.ServiceBus.Core;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using Newtonsoft.Json;
// using Weikio.EventFramework.Abstractions;
//
// namespace Weikio.EventFramework.EventGateway.AzureServiceBus
// {
//     public class IncomingAzureServiceBusChannel : IIncomingChannel
//     {
//         public string Name { get; }
//         public Task Send(CloudEvent cloudEvent)
//         {
//             throw new NotImplementedException();
//         }
//
//         public ChannelWriter<CloudEvent> Writer { get; }
//         public ChannelReader<CloudEvent> Reader { get; }
//         public int ReaderCount { get; set; }
//
//         public IncomingAzureServiceBusChannel(Channel<CloudEvent> channel)
//         {
//             Writer = channel.Writer;
//             Reader = channel.Reader;
//             ReaderCount = 1;
//         }
//     }
//     
//     
//
//     public class MyIncomingAzureServiceBusChannel : IIMyChannel
//     {
//         private readonly IServiceProvider _serviceProvider;
//         private readonly List<IncomingAzureServiceBusHostedService> _readers = new List<IncomingAzureServiceBusHostedService>();
//
//         public MyIncomingAzureServiceBusChannel(IServiceProvider serviceProvider)
//         {
//             _serviceProvider = serviceProvider;
//         }
//
//         public async Task Start()
//         {
//             var inst = (IncomingAzureServiceBusHostedService) ActivatorUtilities.CreateInstance(_serviceProvider, typeof(IncomingAzureServiceBusHostedService));
//             await inst.StartAsync(new CancellationToken());
//             
//             _readers.Add(inst);
//         }
//
//         public async Task Stop()
//         {
//             foreach (var reader in _readers)
//             {
//                 await reader.StopAsync(new CancellationToken());
//             }
//         }
//     }
//
//     public class IncomingAzureServiceBusHostedService : IHostedService
//     {
//         private readonly ILogger<IncomingAzureServiceBusHostedService> _logger;
//         private AzureServiceBusOptions _options;
//         private ChannelWriter<CloudEvent> _target;
//
//         public IncomingAzureServiceBusHostedService(ILogger<IncomingAzureServiceBusHostedService> logger)
//         {
//             _logger = logger;
//         }
//
//         public void Initialize(AzureServiceBusOptions options, ChannelWriter<CloudEvent> writer)
//         {
//             _options = options;
//             _target = writer;
//         }
//
//         public async Task StartAsync(CancellationToken cancellationToken)
//         {
//             var receiver = new MessageReceiver(_options.ConnectionString, _options.QueueName, ReceiveMode.PeekLock);
//
//             // This is the host application's cancellation token
//             cancellationToken.Register(() => receiver.CloseAsync().Wait(cancellationToken));
//
//             while (!cancellationToken.IsCancellationRequested)
//             {
//                 try
//                 {
//                     var message = await receiver.ReceiveAsync();
//
//                     if (message == null)
//                     {
//                         continue;
//                     }
//
//                     var notificationBodyJson = Encoding.UTF8.GetString(message.Body);
//                     var cloudEvent = notificationBodyJson.ToCloudEvent();
//
//                     await _target.WriteAsync(cloudEvent, cancellationToken);
//
//                     try
//                     {
//                         await receiver.CompleteAsync(message.SystemProperties.LockToken);
//                     }
//                     catch (Exception e)
//                     {
//                         _logger.LogError(e, "Handled message, but failed to complete it. Lock timed out? Allow to fail.");
//                     }
//                 }
//                 catch (ServiceBusException e)
//                 {
//                     if (!e.IsTransient)
//                     {
//                         _logger.LogError(e, "Error processing the message. Try again and eventually deadletter.");
//                     }
//                     else
//                     {
//                         _logger.LogError(e, "Service bus error, try again shortly.");
//                     }
//                 }
//                 catch (Exception e)
//                 {
//                     _logger.LogError(e, "Failed to handle incoming message");
//                 }
//             }
//
//             _logger.LogInformation("Exiting message handling loop.");
//         }
//
//         public Task StopAsync(CancellationToken cancellationToken)
//         {
//             return Task.CompletedTask;
//         }
//     }
// }
