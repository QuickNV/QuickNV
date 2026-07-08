using Microsoft.Extensions.Primitives;
using Quick.EntityFrameworkCore.Plus;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace QuickNV.Core.Web;

public class ReverseProxyManager : IProxyConfigProvider
{
    private class InMemoryConfig : IProxyConfig
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public IReadOnlyList<RouteConfig> Routes { get; }

        public IReadOnlyList<ClusterConfig> Clusters { get; }

        public IChangeToken ChangeToken { get; }

        public InMemoryConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            Routes = routes;
            Clusters = clusters;
            ChangeToken = new CancellationChangeToken(_cts.Token);
        }

        internal void SignalChange()
        {
            _cts.Cancel();
        }
    }
    private Dictionary<string, RouteConfig> routeDict = new Dictionary<string, RouteConfig>();
    private Dictionary<string, ClusterConfig> clusterDict = new Dictionary<string, ClusterConfig>();
    private volatile InMemoryConfig _config;
    public IProxyConfig GetConfig() => _config;
    public static ReverseProxyManager Instance { get; } = new ReverseProxyManager();

    public ReverseProxyManager()
    {
        _config = new InMemoryConfig([], []);
    }

    private void Update()
    {
        var oldConfig = _config;
        _config = new InMemoryConfig(routeDict.Values.ToArray(), clusterDict.Values.ToArray());
        oldConfig.SignalChange();
    }

    public IReverseProxyBuilder Load(IReverseProxyBuilder builder)
    {
        foreach (var mediaServer in ConfigDbContext.CacheContext.Query<Model.MediaServer>())
            AddMediaServer(mediaServer);
        builder.Services.AddSingleton<IProxyConfigProvider>(this);
        return builder;
    }

    public string GetProxyMediaServerWsPath(Model.MediaServer mediaServer, string path)
    {
        return $"/MediaServer/{mediaServer.Id}/ws{path}";
    }

    public void AddMediaServer(Model.MediaServer mediaServer)
    {
        AddRule(GetProxyMediaServerWsPath(mediaServer, "/"), mediaServer.GetWsUrlForProxy("/"));
    }

    public void RemoveMediaServer(Model.MediaServer mediaServer)
    {
        RemoveRule(GetProxyMediaServerWsPath(mediaServer, "/"));
    }

    public void AddRule(string path, string url)
    {
        string routeMatchPath = path;
        string transformPathRemovePrefix = path;
        if (path.EndsWith("/"))
        {
            routeMatchPath = path + "{**catch-all}";
            transformPathRemovePrefix = transformPathRemovePrefix.Substring(0, transformPathRemovePrefix.Length - 1);
        }

        lock (routeDict)
        {
            routeDict[path] = new RouteConfig()
            {
                RouteId = "route" + path,
                ClusterId = "cluster" + path,
                Match = new RouteMatch { Path = routeMatchPath }
            }.WithTransformPathRemovePrefix(transformPathRemovePrefix);

            clusterDict[path] = new ClusterConfig()
            {
                ClusterId = "cluster" + path,
                Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                        {
                            { "destination1", new DestinationConfig() { Address = url } }
                        }
            };
            Update();
        }
    }

    public void RemoveRule(string path)
    {
        lock (routeDict)
        {
            if (routeDict.ContainsKey(path))
                routeDict.Remove(path);
            if (clusterDict.ContainsKey(path))
                clusterDict.Remove(path);
            Update();
        }
    }
}
