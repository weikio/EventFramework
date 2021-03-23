using System.Threading.Tasks;

namespace Weikio.EventFramework.Channels
{
    public interface IIMyChannel 
    {
        Task Start();
        Task Stop();
    }
}
