using System;

using Gateways = System.Collections.Generic.Dictionary<int, AuthenticationServer.Gateway>;
using GameWorlds = System.Collections.Generic.Dictionary<int, AuthenticationServer.GameWorld>;


namespace AuthenticationServer
{
    public sealed class SubServersContainer
    {
        private static readonly Lazy<SubServersContainer> lazy = new Lazy<SubServersContainer>(() => new SubServersContainer());

        public static SubServersContainer Singleton
        {
            get => lazy.Value;
        }

        private readonly Gateways gateways;
        private readonly GameWorlds gameWorlds;

        public int OptimalGameWorldId 
        {
            get => optimalGameWorldId;
        }

        private int optimalGameWorldId = -1;

        private SubServersContainer()
        {
            gateways = new Gateways();
            gameWorlds = new GameWorlds();
        }

        public void AddGateway(int peerId)
        {
            gateways.Add(peerId, new Gateway()); 
        }

        public void AddGameWorld(int peerId)
        {
            // Newly added GameWorld is always the most optimal
            optimalGameWorldId = peerId;

            gameWorlds.Add(peerId, new GameWorld());
        }

        public bool GatewayExists(int peerId)
        {
            return gateways.ContainsKey(peerId);
        }

        public bool GameWorldExists(int peerId)
        {
            return gateways.ContainsKey(peerId);
        }

        public bool RemoveGateway(int peerId)
        {
            return gateways.Remove(peerId);
        }

        public bool RemoveGameWorld(int peerId)
        {
            return gameWorlds.Remove(peerId);
        }

        // TODO: :)
        public void IncrementGameWorldPlayerCount()
        {

        }
        
        public bool Remove(int peerId)
        {
            return RemoveGateway(peerId) || RemoveGameWorld(peerId);
        }
        public bool Exists(int peerId)
        {
            return GatewayExists(peerId) || GameWorldExists(peerId);
        }
    }
}
