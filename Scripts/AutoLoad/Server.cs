using Godot;
using Godot.Collections;

using NightFallAuthenticationServer.Scripts.Singleton;

namespace NightFallAuthenticationServer.Scripts.AutoLoad
{
    public sealed class Server : Node
    {

        private static Server singleton;
        public static Server Singleton => singleton;
        private NetworkedMultiplayerENet serverPeer;
        private Dictionary<int, Gateway> _gateways;
        private Dictionary<int, GameServer> _gameServers;


        public Server()
        {
            singleton = this;

            serverPeer = new NetworkedMultiplayerENet();

            _gateways = new Dictionary<int, Gateway>();
            _gameServers = new Dictionary<int, GameServer>();
        }

        public override void _EnterTree()
        {
            SetupDTLS();
            ValidateEnvironmentVariables();
            GetTree().Connect("network_peer_connected", this, nameof(PeerConnected));
            GetTree().Connect("network_peer_disconnected", this, nameof(PeerDisconnected));
        }

        public override void _Ready()
        {
            var port = Configuration.Singleton.GetPort(defaultPort: 4444);
            var numberOfGameWorlds = Configuration.Singleton.GetMaxGameWorlds(defaultMaxGameWorlds: 1);
            var numberOfGateways = Configuration.Singleton.GetMaxGateways(defaultMaxGateways: 1);
            serverPeer.CreateServer(port, numberOfGameWorlds + numberOfGateways);
            GetTree().NetworkPeer = serverPeer;
        }

        public int GetRpcSenderId()
        {
            return GetTree().GetRpcSenderId();
        }

        public void DisconnectPeer(int id, bool now = false)
        {
            serverPeer.DisconnectPeer(id, now);
        }

        public void AddGateway(int id)
        {
            _gateways.Add(id, new Gateway());
        }

        public void AddGameServer(int id)
        {
            _gameServers.Add(id, new GameServer());
        }

        /// Loads and sets certificate and key for ENet connection.
        private void SetupDTLS()
        {

            serverPeer.DtlsVerify = false;
            serverPeer.UseDtls = true;
            var pathToDTLS = "user://DTLS";
            if (DirExists(pathToDTLS))
            {
                var x509Cert = new X509Certificate();
                var certificateFile = pathToDTLS.PlusFile("basic.crt");
                var error = x509Cert.Load(certificateFile);
                if (error != Error.Ok)
                {
                    Logger.Error($"Could not load certificate file {ProjectSettings.GlobalizePath(certificateFile)}. Error code: {error}");
                    GetTree().Quit(-(int)error);
                    return;
                }
                serverPeer.SetDtlsCertificate(x509Cert);

                var cryptoKey = new CryptoKey();
                var keyFile = pathToDTLS.PlusFile("basic.key");
                error = cryptoKey.Load(keyFile);
                if (error != Error.Ok)
                {
                    Logger.Error($"Could not load key file {ProjectSettings.GlobalizePath(keyFile)}. Error code: {error}");
                    GetTree().Quit(-(int)error);
                    return;
                }
                serverPeer.SetDtlsKey(cryptoKey);
            }
            else
            {
                Logger.Error($"Directory {ProjectSettings.GlobalizePath(pathToDTLS)} doesn't exist!. Abording");
                GetTree().Quit(-(int)Error.FileBadPath);
            }

        }

        private bool DirExists(string path)
        {
            var dir = new Directory();
            return dir.DirExists(path);
        }
        
        private void ValidateEnvironmentVariables()
        {
            var environmentVariables = new[] { "GATEWAY_TOKEN", "GAME_SERVER_TOKEN" };
            foreach (var environmentVariable in environmentVariables)
            {
                if (OS.GetEnvironment(environmentVariable).Length == 0)
                {
                    Logger.Error($"Environment varianle {environmentVariable} is not set. Abording...");
                    GetTree().Quit(-(int)Error.PrinterOnFire);
                }
            }
        }

        private void PeerConnected(int id)
        {
            Logger.Server.Info($"Peer {id} has connected");
        }

        private void PeerDisconnected(int id)
        {
            Logger.Server.Info($"Peer {id} has diconnected");
        }
    }
}