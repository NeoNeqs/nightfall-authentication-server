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

        private NetworkedServer() : base()
        {
            _singleton = this;
            _gateways = new Dictionary<int, Gateway>();
            _gameServers = new Dictionary<int, GameServer>();
        }

        public override void _EnterTree()
        {
            base._EnterTree();
            GetTree().Connect("network_peer_connected", this, nameof(PeerConnected));            
            GetTree().Connect("network_peer_disconnected", this, nameof(PeerDisconnected));
        }

        public override void _Ready()
        {
            var port = ServerConfiguration.Singleton.GetPort(4444);
            var maxGameWorlds = ServerConfiguration.Singleton.GetMaxGameWorlds(3);
            var maxGateways = ServerConfiguration.Singleton.GetMaxGateways(1);
            CreateServer(port, maxClients: maxGameWorlds + maxGateways);
        }
        public void AddGateway(int peerId)
        {
            _gateways.Add(peerId, new Gateway());
        }

        public void AddGameServer(int peerId)
        {
            _gameServers.Add(peerId, new GameServer());
        }

        private void PeerConnected(int id)
        {
            Logger.Server.Info($"Peer {id} connected");
        }

        private void PeerDisconnected(int id)
        {
            Logger.Server.Info($"Peer {id} disconnected");
        }
    }
}