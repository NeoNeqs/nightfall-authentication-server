using Godot;
using Godot.Collections;

using SharedUtils.Networking;

using ServersUtils.Services;
using ServersUtils.Generators;
using SharedUtils.Services.Validators;
using SharedUtils.Common;

namespace AuthenticationServer.Scripts.AutoLoad
{
    public class InternalNetwork : NetworkedServer<InternalNetwork>
    {
        private InternalNetwork() : base()
        {
            _singleton = this;
        }

        public override void _EnterTree() => base._EnterTree();

        public override async void _Ready()
        {
            SetProcess(false);
            _ = DataBase.Singleton.Connect(nameof(DataBase.FindUserResult), this, nameof(DataBaseFindUserResult));

            int port = ServerConfiguration.Singleton.GetPort(4444);
            int maxGameWorlds = ServerConfiguration.Singleton.GetMaxGameWorlds(3);
            int maxGateways = ServerConfiguration.Singleton.GetMaxGateways(1);

            // Wait for database before accepting any connections.
            var ret = await ToSignal(DataBase.Singleton, nameof(DataBase.Connected));
            GD.Print(ret);

            _ = CreateServer(port, maxClients: maxGameWorlds + maxGateways);
            base._Ready();
            SetProcess(true);
        }

        protected override void OnPacketReceived(PacketType packetType, params object[] args)
        {
            {
                var packetArgsCountValidator = new PacketArgsCountValidator();

                if (packetArgsCountValidator.Validate(packetType, args.Length) != ErrorCode.Ok) return;
            }

            switch (packetType)
            {
                case PacketType.GatewayServerAuth:
                    {
                        if (!(args[0] is string token)) return;
                        if (SubServersContainer.Singleton.GatewayExists(RpcSenderId)) return;

                        if (token != OS.GetEnvironment("GATEWAY_TOKEN"))
                        {
                            //GD.LogWarn($"Peer {RpcSenderId} sent invalid gateway token.");
                            DisconnectPeer(RpcSenderId, now: true);
                            return;
                        }

                        //GD.LogInfo("Gateway has been authenticated.");
                        RemoveTimerForPeer(RpcSenderId);

                        SubServersContainer.Singleton.AddGateway(RpcSenderId);
                        break;
                    }
                case PacketType.GameWorldServerAuth:
                    {
                        if (!(args[0] is string token)) return;
                        if (SubServersContainer.Singleton.GameWorldExists(RpcSenderId)) return;

                        if (token != OS.GetEnvironment("GAME_SERVER_TOKEN"))
                        {
                            //GD.LogWarn($"Peer {RpcSenderId} sent invalid game server token.");
                            DisconnectPeer(RpcSenderId, true);
                            return;
                        }

                        //GD.LogInfo("Game Server has been authenticated.");
                        RemoveTimerForPeer(RpcSenderId);

                        SubServersContainer.Singleton.AddGameWorld(RpcSenderId);

                        break;
                    }
                case PacketType.GatewayServerAuthForward:
                    {
                        if (!(args[0] is int clientId)) return;
                        if (!(args[1] is string login)) return;
                        if (!(args[2] is string password)) return;

                        DataBase.Singleton.FindUser(RpcSenderId, clientId, login, password); //gateway id too
                        break;
                    }
                default:
                    break;
            }
        }

        private void DataBaseFindUserResult(int gatewayId, int clientId, bool exists)
        {
            // TODO: instead of sending bool to a client a status code should be sent (correct, wrong username/password, no game worlds, internal error ("please try again later" or sth))
            int optimalGameWorldId = SubServersContainer.Singleton.OptimalGameWorldId;

            string token = exists ? TokenGenerator.GetUniqueKey(64) : "";
            string ipAddressOfOptimalGameWorld = exists && optimalGameWorldId != -1 ? GetIpAddressOfPeer(optimalGameWorldId) : "";

            SendTokenToGateway(gatewayId, clientId, ipAddressOfOptimalGameWorld, token);

            // empty token means client sent wrong credentials and there is no sense in seding empty one.
            if (token.Length != 0 && optimalGameWorldId != -1)
            {
                SendTokenToGameWorld(optimalGameWorldId, token);
            }
        }

        /// <summary>
        ///     Sends a temporary token to Gateway Server which is going to forward it to the Client. 
        ///     The lifetime of the token is managed by appropraite Game World Server 
        ///     (this implies that one of the Game World Server must receive the token).
        /// </summary>
        /// <param name="gatewayId">Id of the gateway that issued <see cref="PacketType.GatewayServerAuthForward"/></param>
        /// <param name="clientId">Id of the client that sent login credentials to Gateway Server</param>
        /// <param name="ipAddressOfOptimalGameWorld">Ip address of a Game World Server that is least crowded</param>
        /// <param name="token">64 length string. See <see cref="TokenGenerator"/> to make one</param>
        private void SendTokenToGateway(int gatewayId, int clientId, string ipAddressOfOptimalGameWorld, string token)
        {
            Send(gatewayId, PacketType.AuthenticationServerAuthResponse, clientId, ipAddressOfOptimalGameWorld, token);
        }

        /// <summary>
        ///     Sends a token to Game World Server with id <paramref name="gameWorldId"/>.
        ///     That Game World Server manages the lifetime of that token.
        /// </summary>
        /// <param name="gameWorldId">If of a Game World Server</param>
        /// <param name="token">64 length string. See <see cref="TokenGenerator"/> to make one</param>
        private void SendTokenToGameWorld(int gameWorldId, string token)
        {
            Send(gameWorldId, PacketType.AuthenticationServerSendToken, token);
        }

        private void SpawnTimerForPeer(int peerId)
        {
            var timer = new Timer
            {
                Name = peerId.ToString(),
                WaitTime = (int)ProjectSettings.GetSetting("network/limits/tcp/connect_timeout_seconds"),
                Autostart = true,
                OneShot = true
            };

            _ = timer.Connect("timeout", this, nameof(TimerTimeout), new Array { peerId }, (int)ConnectFlags.Oneshot);
            AddChild(timer);
        }

        private void RemoveTimerForPeer(int peerId)
        {
            GetNodeOrNull(peerId.ToString())?.QueueFree();
        }

        private void TimerTimeout(int peerId)
        {
            if (!SubServersContainer.Singleton.Exists(peerId))
            {
                DisconnectPeer(peerId);
            }

            RemoveTimerForPeer(peerId);
        }

        protected override void PeerConnected(int id)
        {
            //GD.LogInfo($"Peer {id} connected");
            SpawnTimerForPeer(id);
        }

        protected override void PeerDisconnected(int id)
        {
            //GD.LogInfo($"Peer {id} disconnected");
            RemoveTimerForPeer(id);

            _ = SubServersContainer.Singleton.Remove(id);
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