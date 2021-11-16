using System;
using System.Threading.Tasks;

namespace Weikio.EventFramework.IntegrationFlow
{
    public interface IBuilder<T> where T : class
    {
        Task<T> Build(IServiceProvider serviceProvider);
    }
}
