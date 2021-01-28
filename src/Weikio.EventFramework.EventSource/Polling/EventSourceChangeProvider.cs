using Microsoft.Extensions.Primitives;

namespace Weikio.EventFramework.EventSource.Polling
{
    public class EventSourceChangeProvider
    {
        public EventSourceChangeProvider(EventSourceChangeToken changeToken)
        {
            _changeToken = changeToken;
        }

        private readonly EventSourceChangeToken _changeToken;

        public IChangeToken GetChangeToken()
        {
            if (_changeToken.TokenSource.IsCancellationRequested)
            {
                _changeToken.Initialize();

                return new CancellationChangeToken(_changeToken.TokenSource.Token);
            }

            return new CancellationChangeToken(_changeToken.TokenSource.Token);
        }
    }
}