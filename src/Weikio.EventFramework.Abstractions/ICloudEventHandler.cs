using System.Threading.Tasks;

namespace Weikio.EventFramework.Abstractions
{
    public interface ICloudEventHandler
    {
        Task CanHandle(ICloudEventContext cloudEventContext);
        
        Task Handle(ICloudEventContext cloudEventContext);
    }
    
    public interface ICloudEventHandler<TCloudEventDataType>
    {
        
    }
}
