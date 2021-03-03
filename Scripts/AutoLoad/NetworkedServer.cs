using Godot;
using Godot.Collections;

using ServersUtils.Scripts.Logging;
using ServersUtils.Scripts.Services;


namespace AuthenticationServer.Scripts.AutoLoad
{
    public class NetworkedServer : NetworkedServerService
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
            var error = SetupDTLS("user://DTLS/");
            if (error != Error.Ok)
            {
                ServerLogger.GetLogger().Error($"An error has occurred while setting up DTLS. Error code: {error}");
            }
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
            ServerLogger.GetLogger().Info($"Peer {id} connected");
        }

        private void PeerDisconnected(int id)
        {
            ServerLogger.GetLogger().Info($"Peer {id} disconnected");
        }

        protected override string GetCryptoKeyName()
        {
            return "ag.key";
        }

        protected override string GetCertificateName()
        {
            return "ag.crt";
        }
    }
}