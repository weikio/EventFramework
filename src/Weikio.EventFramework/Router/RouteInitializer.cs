using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Router
{
    public class RouteInitializer
    {
        private readonly ICloudEventRouteCollection _routeCollection;

        public RouteInitializer(ICloudEventRouteCollection routeCollection)
        {
            _routeCollection = routeCollection;
        }

        public void Initialize(ICloudEventRoute route)
        {
            _routeCollection.Add(route);
        }
    }
}
