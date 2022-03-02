using System.Data.SQLite;

using Traviam.Utils;

namespace Traviam.GameLogic;

public static class Mapa
{
    private static string CONNECTION_STRING = Utils.ConnectionStrings.GetDBConnString();

    public static int GetMaxCoordenadas(string coordenada) 
    {
        Int32 max = 0;
        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            SQLiteCommand sql_cmd = connection.CreateCommand();
            sql_cmd.CommandText = String.Format(@" SELECT max(Pos{0}) FROM TilesMapa ", coordenada);
            
            SQLiteDataReader reader = sql_cmd.ExecuteReader();
            while (reader.Read())
            {
                max = (Int32)(reader.GetDouble(0));
                break; // (if you only want the first item returned)
            }
            reader.Close();
        }
        return max; 
    }
}

