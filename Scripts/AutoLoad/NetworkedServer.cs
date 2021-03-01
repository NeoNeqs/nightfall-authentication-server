using Godot.Collections;
using NightFallServersUtils.Scripts.AutoLoad;

namespace NightFallAuthenticationServer.Scripts.AutoLoad
{
    public class NetworkedServer : DefaultNetworkedServer
    {
        private static NetworkedServer _singleton;
        public static NetworkedServer Singleton => _singleton;
        private Dictionary<int, Gateway> _gateways;
        private Dictionary<int, GameServer> _gameServers;

        private NetworkedServer()
        {
            _singleton = this;
            _gateways = new Dictionary<int, Gateway>();
            _gameServers = new Dictionary<int, GameServer>();
        }

        public void AddGateway(int peerId)
        {
            _gateways.Add(peerId, new Gateway());
        }

        public void AddGameServer(int peerId)
        {
            _gameServers.Add(peerId, new GameServer());
        }
    }
}