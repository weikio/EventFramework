using Microsoft.Extensions.DependencyInjection;

namespace Weikio.EventFramework.Abstractions
{
    public interface IEventFrameworkBuilder
    {
        IServiceCollection Services { get; }
    }
}