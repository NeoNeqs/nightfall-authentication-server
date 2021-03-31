using Godot;
using Godot.Collections;
using SharedUtils.Common;

namespace AuthenticationServer.Scripts.AutoLoad
{
    public class DataBase : GodotSingleton<DataBase>
    {
        [Signal]
        public delegate void Connected(bool success);

        [Signal]
        public delegate void FindUserResult(int gatewayId, int clientId, bool exists);

        //private readonly MySQL mySQL;

        public DataBase()
        {
            _singleton = this;
            //mySQL = new MySQL();
        }

        public override void _Ready()
        {
            //mySQL.SetCredentials(host: "localhost", username: "root", password: "");

            //GD.LogInfo("Starting database thread...");
            // Seems like it is better to deffer call since `connect_to_database` creates a thread which halts the main thread for a 2-3 seconds
            //mySQL.CallDeferred("connect_to_database", this, nameof(DataBaseConnected));

            // It's ok to set the schema here since the call will be queued and sent only after connection was successful.
           // mySQL.SetSchema("nightfall");
        }

        public void FindUser(int gatewayId, int clinetId, string login, string password)
        {
            string query = "SELECT * FROM users WHERE login=? AND password=?";
            //mySQL.ExecutePreparedSelectQuery(query, new Array { login, password }, this, nameof(FindUserResult), new Array { gatewayId, clinetId });
        }

        private void FindUserResults(bool success, int rows, Array data)
        {
#if DEBUG
            if (data.Count != 2)
            {
                //GD.PrintErr("Argument `data` must have `Count` = 2");
            }
            if (!(data[0] is int))
            {
               // GD.PrintErr("data[0] must be an int");
            }
            if (!(data[1] is int))
            {
               // GD.PrintErr("data[1] must be an int");
            }
#endif
            int gatewayId = (int)data[0];
            int clientId = (int)data[1];

            EmitSignal(nameof(FindUserResult), gatewayId, clientId, success && rows == 1);
        }

        private void DataBaseConnected(bool success, Array data)
        {
           // GD.LogInfo("Database thread is ready!");
            EmitSignal(nameof(Connected), success);
            if (success)
            {
              //  GD.LogInfo("Connected to database.");
            }
            else
            {
                //GD.LogError("Connection to database failed.");
            }
        }

        public override void _ExitTree()
        {
            //mySQL.Free();
        }
    }
}
