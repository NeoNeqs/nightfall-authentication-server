using Godot;


namespace NightFallServersUtils.Scripts.AutoLoad
{
    public class NetworkedServerService : Node
    {
        private readonly NetworkedMultiplayerENet serverPeer;


        protected NetworkedServerService()
        {
            serverPeer = new NetworkedMultiplayerENet();
        }

        public override void _EnterTree()
        {
            SetupDTLS();
            //ValidateEnvironmentVariables();
        }

        protected void CreateServer(int port, int maxClients)
        {
            serverPeer.CreateServer(port, maxClients);
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

        /// Loads and sets certificate and key for ENet connection.
        protected void SetupDTLS()
        {
            serverPeer.DtlsVerify = false;
            serverPeer.UseDtls = true;
            var pathToDTLS = "user://DTLS";
            if (DirExists(pathToDTLS))
            {
                var x509Cert = new X509Certificate();
                var certificateFile = pathToDTLS.PlusFile("main.crt");
                var error = x509Cert.Load(certificateFile);
                if (error != Error.Ok)
                {
                    //Logger.Server.Error($"Could not load certificate file {ProjectSettings.GlobalizePath(certificateFile)}. Error code: {error}");
                    GetTree().Quit(-(int)error);
                    return;
                }
                serverPeer.SetDtlsCertificate(x509Cert);

                var cryptoKey = new CryptoKey();
                var keyFile = pathToDTLS.PlusFile("main.key");
                error = cryptoKey.Load(keyFile);
                if (error != Error.Ok)
                {
                   // Logger.Server.Error($"Could not load key file {ProjectSettings.GlobalizePath(keyFile)}. Error code: {error}");
                    GetTree().Quit(-(int)error);
                    return;
                }
                serverPeer.SetDtlsKey(cryptoKey);
            }
            else
            {
               // Logger.Server.Error($"Directory {ProjectSettings.GlobalizePath(pathToDTLS)} doesn't exist!. Abording");
                GetTree().Quit(-(int)Error.FileBadPath);
            }
        }

        private bool DirExists(string path)
        {
            var dir = new Directory();
            return dir.DirExists(path);
        }

       //protected void ValidateEnvironmentVariables()
       //{
       //    var environmentVariables = new[] { "GATEWAY_TOKEN", "GAME_SERVER_TOKEN" };
       //    foreach (var environmentVariable in environmentVariables)
       //    {
       //        if (OS.GetEnvironment(environmentVariable).Length == 0)
       //        {
       //            Logger.Server.Error($"Environment varianle {environmentVariable} is not set. Abording...");
       //            GetTree().Quit(-(int)Error.PrinterOnFire);
       //        }
       //    }
       //}
    }
}