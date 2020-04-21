using Microsoft.Extensions.DependencyInjection;

namespace Weikio.EventFramework.Abstractions.DependencyInjection
{
    public interface IEventFrameworkBuilder
    {
        IServiceCollection Services { get; }
    }
}
