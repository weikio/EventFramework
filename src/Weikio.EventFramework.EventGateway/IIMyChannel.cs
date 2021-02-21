using System.Threading.Tasks;

namespace Weikio.EventFramework.EventGateway
{
    public interface IIMyChannel 
    {
        Task Start();
        Task Stop();
    }
}
