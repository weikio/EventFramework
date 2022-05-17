# Event Framework

[![Nuget Version](https://img.shields.io/nuget/v/Weikio.EventFramework.AspNetCore.svg?style=flat&label=Weikio.EventFramework.AspNetCore)](https://www.nuget.org/packages/Weikio.EventFramework.AspNetCore/)

Event Framework is an Open Source framework for creating, receiving, sending and handling CloudEvents in .NET.

![Logo](https://docs.weik.io/eventframework/2020-05-13-19-24-22.png)

# Main Features

1. Create CloudEvents using CloudEventCreator
2. Send & Receive CloudEvents using Channels and Event Sources
3. Build Event Flows

# Examples

### Creating CloudEvents

```
var obj = new CustomerCreated(Guid.NewGuid(), "Test", "Customer");

// Object to event
var cloudEvent = CloudEventCreator.Create(obj);

// Object to event customization
var cloudEventCustomName = CloudEventCreator.Create(obj, eventTypeName: "custom-name");

// Using instance of CloudEventCreator
var creator = new CloudEventCreator();
```

### Sending CloudEvents

Event Framework uses Channels when transporting and transforming events from a source to an endpoint. Channels have adapters, components and endpoints (+ interceptors) which are used to process an event.

Here's an example where a channel is created with a single HTTP endpoint. Every object sent to this channel is transformed to CloudEvent and then delivered using HTTP:

```
var channel = await CloudEventsChannelBuilder.From("myHttpChannel")
    .Http("https://webhook.site/3bdf5c39-065b-48f8-8356-511b284de874")
    .Build(serviceProvider);

await channel.Send(new CustomerCreatedEvent() { Age = 50, Name = "Test User" });
```

### Receiving CloudEvents

Event Framework supports Event Sources. An event source can be used to receive events (for example: HTTP, Azure Service Bus) but an event source can also poll and watch changes happening in some other system (like local file system).

Here's an example where HTTP and Azure Service Bus are used to receive events in ASP.NET Core and then logged:

```
services.AddEventFramework()
    .AddChannel(CloudEventsChannelBuilder.From("logChannel")
        .Logger())
    .AddHttpCloudEventSource("events")
    .AddAzureServiceBusCloudEventSource(
        "Endpoint=sb://sb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YDcvmuL4=",
        "myqueue");

services.Configure<DefaultChannelOptions>(options => options.DefaultChannelName = "logChannel");
```

### Building Event Flows

Event Sources and Channels can be combined into Event Flows. Event Flows also support branching and subflows.

Here's an example where an event source is used to track file changes in a local file system and then all the created files are reported using HTTP:

```
var flow = EventFlowBuilder.From<FileSystemEventSource>(options =>
    {
        options.Configuration = new FileSystemEventSourceConfiguration() { Folder = @"c:\\temp\\myfiles", Filter = "*.bin" };
    })
    .Filter(ev => ev.Type == "FileCreatedEvent" ? Filter.Continue : Filter.Skip)
    .Http("https://webhook.site/3bdf5c39-065b-48f8-8356-511b284de874");

services.AddEventFramework()
    .AddEventFlow(flow);
```

## Project Home

Please visit the project homesite at https://weik.io/eventframework for more details.

## Source code

Source code for Event Framework is available from [GitHub](https://github.com/weikio/EventFramework).

## Support & Build by

Event Framework is build by [Adafy](https://adafy.com). Adafy also provides commercial support for the framework.

![Adafy Logo](docs/Adafy_logo_256.png)

Adafy is a Finnish software development house, focusing on Microsoft technologies.

## License

Event Framework is available as an open source, apache2-licensed project. 