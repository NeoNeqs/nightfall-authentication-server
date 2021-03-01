using NightFallServersUtils.Scripts.AutoLoad;

namespace NightFallAuthenticationServer.Scripts.AutoLoad
{
    public sealed class ServerConfiguration : DefaultServerConfiguration
    {
        private static ServerConfiguration _singleton;
        public static ServerConfiguration Singleton => _singleton;
        public ServerConfiguration()
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