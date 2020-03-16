using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using ImpromptuInterface;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventAggregator;

namespace Weikio.EventFramework.AspNetCore.Extensions
{
    public static class EventFrameworkEventAggregatorExtensions
    {
        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, ICloudEventHandler handler)
        {
            return builder;
        }
        
        public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder) where THandlerType : class
        {
            builder.Services.AddTransient<THandlerType>();

            builder.Services.AddOptions<HandlerOptions>(typeof(THandlerType).FullName)
                .Configure<IServiceProvider>((options, provider) =>
                {
                    options.HandlerFactory = provider.GetRequiredService<THandlerType>;
                })

            ;
            // builder.Services.Configure<HandlerOptions>(typeof(THandlerType).FullName,  (provider, options) =>
            // {
            //     options.HandlerFactory = () =>
            //     {
            //
            //     };
            // });
            
            // builder.Services.AddTransient(provider =>
            // {
            //     var service = provider.GetRequiredService<THandlerType>();
            //     ICloudEventHandler result = service.ActLike();
            //
            //     return result;
            // });
            //
            

            return builder;
        }
        
        public static IEventFrameworkBuilder AddHandler<TCloudEventDataType>(this IEventFrameworkBuilder builder, ICloudEventHandler<TCloudEventDataType> handler)
        {
            return builder;
        }

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, string eventType, Func<ICloudEventContext, Task> handle)
        {
            return builder;
        }
        
        public static IEventFrameworkBuilder AddHandler<TCloudEventDataType>(this IEventFrameworkBuilder builder, string eventType, Func<ICloudEventContext<TCloudEventDataType>, Task> handle)
        {
            return builder;
        }

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, string eventType, Func<IServiceProvider, ICloudEventContext, Task> handle)
        {
            return builder;
        }

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Predicate<ICloudEventContext> canHandle, Func<ICloudEventContext, Task> handle)
        {
            return builder;
        }
        
        public static IEventFrameworkBuilder AddHandler<TCloudEventDataType>(this IEventFrameworkBuilder builder, Predicate<ICloudEventContext<TCloudEventDataType>> canHandle, Func<ICloudEventContext<TCloudEventDataType>, Task> handle)
        {
            return builder;
        }

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Predicate<ICloudEventContext> canHandle, Func<IServiceProvider, ICloudEventContext, Task> handle)
        {
            return builder;
        }  
        
        public static IEventFrameworkBuilder AddHandler<TCloudEventDataType>(this IEventFrameworkBuilder builder, Predicate<CloudEvent> canHandle, Func<IServiceProvider, ICloudEventContext<TCloudEventDataType>, Task> handle)
        {
            return builder;
        }  

    }
}
