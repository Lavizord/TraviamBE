using System.Data.SQLite;
using System.Data;
using Newtonsoft.Json;

namespace Traviam.Utils;

public static class ConnectionStrings
{
    //public const string CONNECTION_STRING = "Data Source=C:/Projetos/Traviam/Database/TraviamDB.db; UseUTF16Encoding=True";

    public static string GetDBConnString()
    {
        string path = Directory.GetCurrentDirectory();
        return String.Format(@"Data Source={0}\Database\TraviamDB.db; UseUTF16Encoding=True", path);
    }
}