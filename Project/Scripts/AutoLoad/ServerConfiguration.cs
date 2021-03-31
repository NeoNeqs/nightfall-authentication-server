using Godot;

using ServersUtils.Configurations;

using SharedUtils.Common;

namespace AuthenticationServer.Scripts.AutoLoad
{
    public sealed class ServerConfiguration : StandartServerConfiguration
    {
        private static ServerConfiguration _singleton;
        public static ServerConfiguration Singleton => _singleton;


        public ServerConfiguration() : base()
        {
            _singleton = this;
        }

        public override void _EnterTree()
        {
            var error = LoadConfiguration();
            if (error != ErrorCode.Ok)
            {
                //GD.LogError($"Could not load configuration file {Path}. Error code: {error}");
            }
        }

        public int GetMaxGateways(int defaultMaxGateways)
        {
            return GetValue<int>("NETWORKING", "max_gateways", defaultMaxGateways);
        }

        public int GetMaxGameWorlds(int defaultMaxGameWorlds)
        {
            return GetValue<int>("NETWORKING", "max_game_worlds", defaultMaxGameWorlds);
        }

        public override void _ExitTree()
        {
            var error = SaveConfiguration();
            if (error != ErrorCode.Ok)
            {
                //GD.LogError($"Could not save configuration file. Error code: {error}");
            }
        }
    }
}