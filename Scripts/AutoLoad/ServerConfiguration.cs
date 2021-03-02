using ServersUtils.Scripts.Configurations;

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

        public int GetMaxGateways(int defaultMaxGateways)
        {
            return GetValue<int>("NETWORKING", "max_gateways", defaultMaxGateways);
        }

        public int GetMaxGameWorlds(int defaultMaxGameWorlds)
        {
            return GetValue<int>("NETWORKING", "max_game_worlds", defaultMaxGameWorlds);
        }
    }
}