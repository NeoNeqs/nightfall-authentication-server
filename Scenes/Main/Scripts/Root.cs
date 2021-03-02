using Godot;

using AuthenticationServer.Scripts.AutoLoad;

namespace AuthenticationServer.Scenes.Main.Scritps
{
    public class Root : Node
    {
        [Remote]
        private void AuthenticateGateway(string token)
        {
            if (token != OS.GetEnvironment("GATEWAY_TOKEN"))
            {
                NetworkedServer.Singleton.DisconnectPeer(NetworkedServer.Singleton.GetRpcSenderId(), true);
            }
            else
            {
                NetworkedServer.Singleton.AddGateway(NetworkedServer.Singleton.GetRpcSenderId());
            }
        }

        [Remote]
        private void AuthenticateGameServer(string token)
        {
            if (token != OS.GetEnvironment("GAME_SERVER_TOKEN"))
            {
                NetworkedServer.Singleton.DisconnectPeer(NetworkedServer.Singleton.GetRpcSenderId(), true);
            }
            else
            {
                NetworkedServer.Singleton.AddGameServer(NetworkedServer.Singleton.GetRpcSenderId());
            }
        }
    }
}