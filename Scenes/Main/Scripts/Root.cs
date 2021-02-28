using Godot;
using AuthenticationServer.Scripts.AutoLoad;

public class Root : Node
{
    [Remote]
    private void AuthenticateGateway(string token)
    {
        if (token != OS.GetEnvironment("GATEWAY_TOKEN"))
        {
            Server.Singleton.DisconnectPeer(Server.Singleton.GetRpcSenderId(), true);
        }
        else
        {
            Server.Singleton.AddGateway(Server.Singleton.GetRpcSenderId());
        }
    }

    [Remote]
    private void AuthenticateGameServer(string token)
    {
        if (token != OS.GetEnvironment("GAME_SERVER_TOKEN"))
        {
            Server.Singleton.DisconnectPeer(Server.Singleton.GetRpcSenderId(), true);
        }
        else
        {
            Server.Singleton.AddGameServer(Server.Singleton.GetRpcSenderId());
        }
    }
}
