using System.Data.SQLite;
using System.Data;
using Newtonsoft.Json;

using Traviam.GameLogic;

namespace Traviam.Utils;

public static class DbHelper
{

    private static string CONNECTION_STRING = Utils.ConnectionStrings.GetDBConnString();

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

    public static DateTime SafeGetDatetime(this SQLiteDataReader reader, int colIndex)
    {
        if(!reader.IsDBNull(colIndex))
            return reader.GetDateTime(colIndex);
        return DateTime.UnixEpoch;
    }
    
    public static List<Edificio> GetPossiveisEdificios(int vilaId, int posVilaX, int posVilaY)
    {
        Console.WriteLine();
        Console.WriteLine("--> DbHelper.GetPossiveisEdificios");
        Console.WriteLine();
        List<Edificio> lista = new List<Edificio>();   
        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            Console.WriteLine();
            Console.WriteLine("----> USING sliteconnection");
            Console.WriteLine();
            
            string query  =  
                @"
                SELECT  EdificiosLevel.PopMax, EdificiosLevel.Output, 
                        EdificiosLevel.CustoMadeira, EdificiosLevel.CustoPedra,
                        EdificiosLevel.CustoTrigo, EdificiosLevel.Level,
                        EdificiosLevel.Hp, EdificiosLevel.UpTimeMin, 
                        EdificiosLevel.Nome, EdificiosLevel.Descricao
                    FROM EdificiosLevel 
                    Left JOIN TileSlots 
                        ON TileSlots.NomeEdificio = EdificiosLevel.Nome
                    INNER JOIN TilesMapa
                        ON TilesMapa.TipoTile = TileSlots.TipoTile
                    Where 
                        TileSlots.PosX = @x AND TileSlots.PosY = @y
                        AND level = 1 AND TilesMapa.Id = @vilaid
                ";

            connection.Open();
            SQLiteCommand command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@x", posVilaX);
            command.Parameters.AddWithValue("@y", posVilaY);
            command.Parameters.AddWithValue("@vilaid", vilaId);
            command.Prepare();
            SQLiteDataReader reader = command.ExecuteReader();
            Console.WriteLine("      Query Returnou Valores? "+reader.HasRows.ToString());
            if (!reader.HasRows)
            {
                Console.WriteLine(":::: Não é possível criar edificios.");
                return lista; // Vai vazia, sem ações do jogador possiveis.
            }
            while (reader.Read())
            {
                Edificio edificio = new Edificio();
                edificio.CriaEdificio(DbHelper.SafeGetString(reader, 8), vilaId, posVilaX, posVilaY, flagGrava:false);
                Console.WriteLine(":::: Adicionao um edificio á lista.");
                lista.Add(edificio);
            }
            reader.Close();
        }
        Console.WriteLine(":::: Retorno final da lista de edificios.");
        return lista;
    }
}