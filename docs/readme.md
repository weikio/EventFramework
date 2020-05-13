---
title: Introduction
description: Event Framework is a framework for creating, receiving, sending and handling CloudEvents in .NET.
order: 0
---

## Introduction 
![Logo](2020-05-13-19-24-22.png){.float-right}

[CloudEvents](https://cloudevents.io/){target="_blank"} are a specification which describe event data in a common way. **Event Framework** is a framework for creating, receiving, sending and handling **CloudEvents in .NET**.

Event Framework is available for .NET Core 3.1. Some parts of it are available as .NET Standard 2.0 libraries. The sweet spot for using Event Framework is an ASP.NET Core 3.1 based application.

Event Framework is part of the Weik.io platform. Weik.io provides open source integration, eventing and automation frameworks for .NET applications. Event Framework is the "eventing" part of the Weik.io platform.

## Core Features

The core features of the Event Framework are:

* Create CloudEvents from .NET objects
* Send and receive CloudEvents
* Handle CloudEvents
* Monitor and automatically publish CloudEvents

Each of these these core features are available as separate Nuget packages. The following sections describe these core features in more detail.

##### CloudEvent Creation

Use Event Framework to create CloudEvents from .NET objects. Event Framework can convert a single object or a collection of objects:

```csharp
services.AddEventFramework()
    .AddCloudEventCreator();
...
var cloudEvent = _cloudEventCreator.CreateCloudEvent(obj);
```

Configuration can be used to define the event's subject, topic and other information.

View the [quick start of using Event Framework to create CloudEvents](/creation/quickstart.html) or browse through the [full documentation](/creation/introduction.html).

##### CloudEvent Gateway

Use Event Framework to host CloudEvents Gateway. Gateways allow Event Framework to receive and to send CloudEvents:

```csharp
services.AddEventFramework()
    .AddHttpGateway();
```

Event Framework supports the following types of gateways:
* Local (in-memory)
* HTTP
* Azure Service Bus

Single application can contain multiple gateways, so it is possible for example to receive messages through Azure Service Bus and through HTTP web requests. 

View the [quick start of using Event Framework gateways to send and to receive CloudEvents](/gateway/quickstart.html) or browse through the [full documentation](/gateway/introduction.html).

##### CloudEvent Aggregator

Use Event Framework to handle CloudEvents. CloudEvent Aggregator allows your application to handle CloudEvents. With the event aggregator your application can handle different types of events through handlers:

```csharp
services.AddEventFramework()
    .AddHandler<FileCreatedHandler>();
```

Multiple handlers can work with a single CloudEvent and your application can handle messages coming from outside of your system or from inside.

View the [quick start of creating CloudEvent handler](/aggregator/quickstart.html) or browse through the [full documentation](/aggregator/introduction.html).

##### CloudEvent Sources

Use Event Framework Sources to watch and to publish CloudEvents based on changes happening in other systems. Event source is an ideal solution for a scenario where you want to track changes in other systems, for example in an external database or in CosmosDB: 

```csharp
services.AddEventFramework()
    .AddSource<FileEventSource>(source =>
    {
        source.Folder = @"c:\temp";
        source.Filter = "*.exe";
    })
```

Add and configure Event source into your application and then use handlers to react to the changes. Or just publish the CloudEvents into an another gateway.

Use plugins to add different CloudEvent sources easily.

View the [quick start of using a CloudEvent source](/sources/quickstart.html) or browse through the [full documentation](/sources/introduction.html).

## Source code

Source code for Event Framework is available from [GitHub](https://github.com/weikio/EventFramework){target="_blank"}.

## Support

Commercial support for Event Framework is available.

## License

Event Framework is available as an open source, MIT-licensed project. 