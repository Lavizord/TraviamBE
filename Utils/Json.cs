using System.Data.SQLite;
using System.Data;
using Newtonsoft.Json;

namespace Traviam.Utils;

public class JsonUtils
{
    public String sqlDatoToJson(SQLiteDataReader dataReader)
    {
        var dataTable = new DataTable();
        dataTable.Load(dataReader);
        string JSONString = string.Empty;
        JSONString = JsonConvert.SerializeObject(dataTable);
        return JSONString;
    }
}