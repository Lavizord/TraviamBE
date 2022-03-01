using System.Data.SQLite;
using System.Data;
using Newtonsoft.Json;

namespace Traviam.Utils;

public static class DbHelper
{
    public static int SafeGetInt(this SQLiteDataReader reader, int colIndex)
    {
        if(!reader.IsDBNull(colIndex))
            return (Int32)(reader.GetDouble(colIndex));
        return 0;
    }

    public static string SafeGetString(this SQLiteDataReader reader, int colIndex)
    {
    if(!reader.IsDBNull(colIndex))
        return reader.GetString(colIndex);
    return string.Empty;
    }


}