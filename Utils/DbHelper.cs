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
                edificio.CriaEdificio(DbHelper.SafeGetString(reader, 8), vilaId, posVilaX, posVilaY, 1, flagGrava:false);
                Console.WriteLine(":::: Adicionao um edificio á lista.");
                lista.Add(edificio);
            }
            reader.Close();
        }
        Console.WriteLine(":::: Retorno final da lista de edificios.");
        return lista;
    }

    public static List<Vila> GetVilasJogador(int playerid)
    {
        Console.WriteLine();
        Console.WriteLine("--> DbHelper.GetVilasJogador");
        List<Vila> lista = new List<Vila>();   
        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            Console.WriteLine("----> USING sliteconnection");
            
            string query  =  @" SELECT id
                                FROM TilesMapa
                                WHERE PlayerID = @playerid
                            ";

            connection.Open();
            SQLiteCommand command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@playerid", playerid);
            command.Prepare();
            SQLiteDataReader reader = command.ExecuteReader();

            Console.WriteLine("      Query Returnou Valores? "+reader.HasRows.ToString());

            if (!reader.HasRows)
            {
                Console.WriteLine(":::: Não Existem aldeias de Jogador");
                return lista; 
            }
            Vila vila = new Vila();
            while (reader.Read())
            {
                vila.LoadFromId(reader.GetInt32(0));
            }
            reader.Close();
            vila.UpdateVila();
            Console.WriteLine(":::: Adicionao uma vila á lista.");
            lista.Add(vila);
        }
        Console.WriteLine(":::: Retorno final da lista de vilas.");
        return lista;
    }

    public static List<Edificio> GetEdificiosVila(int vilaid) //TODO: Adaptar este código para os edificios
    {
        Console.WriteLine();
        Console.WriteLine("--> DbHelper.GetEdificiosVila");
        Console.WriteLine();
        List<Edificio> lista = new List<Edificio>();   
        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            Console.WriteLine();
            Console.WriteLine("----> USING sliteconnection");
            Console.WriteLine();
            
            string query  =  @" SELECT id
                                FROM TilesMapa
                                WHERE PlayerID = @playerid
                            ";

            connection.Open();
            SQLiteCommand command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@playerid", vilaid);
            command.Prepare();
            SQLiteDataReader reader = command.ExecuteReader();
            Console.WriteLine("      Query Returnou Valores? "+reader.HasRows.ToString());
            if (!reader.HasRows)
            {
                Console.WriteLine(":::: Não Existem aldeias de Jogador");
                return lista; 
            }
            while (reader.Read())
            {
                //Vila vila = new Vila();
                //vila.LoadFromId(reader.GetInt32(0));
                //vila.UpdateVila();
                //Console.WriteLine(":::: Adicionao uma vila á lista.");
                //lista.Add(vila);
            }
            reader.Close();
        }
        Console.WriteLine(":::: Retorno final da lista de vilas.");
        return lista;
    }

    public static bool SafeLoadPlayer(Player player, string where)
    {
        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            string query = String.Format(@"SELECT 
                                id, nome, DtCriacao, CapitalX, CapitalY
                            FROM Players 
                            WHERE {0};", where);
            connection.Open();
            SQLiteCommand command = new SQLiteCommand(query, connection);
            command.Prepare();
            SQLiteDataReader reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                return false;
            }                            
            while (reader.Read())
            {
                player.id = DbHelper.SafeGetInt(reader, 0);
                player.nome = DbHelper.SafeGetString(reader, 1);
                player.CapitalX = DbHelper.SafeGetInt(reader, 3);
                player.CapitalY = DbHelper.SafeGetInt(reader, 4);
                player.DtCriacao = DbHelper.SafeGetDatetime(reader, 2); 
                break; // (if you only want the first item returned)
            }
            reader.Close();
        }
        return true;
    }

    public static bool SafeLoadVila(Vila vila, string where)
    {
        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            string query = String.Format(@"
                            SELECT id, TipoTile, PlayerId, PosX, PosY, 
                                Madeira, Pedra, Trigo, DtUltAct, DtAtribuicao,
                                NumTTrigo, NumTPedra, NumTMadeira 
                            FROM TilesMapa 
                            WHERE {0};", where);
            connection.Open();
            SQLiteCommand command = new SQLiteCommand(query, connection);
            command.Prepare();
            SQLiteDataReader reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                return false;
            }  
            while (reader.Read())
            {
                vila.id      =  DbHelper.SafeGetInt(reader, 0);
                vila.tipo    = DbHelper.SafeGetString(reader, 1);
                vila.PlayerID = DbHelper.SafeGetInt(reader, 2);
                vila.x       =  DbHelper.SafeGetInt(reader, 3);
                vila.y       =  DbHelper.SafeGetInt(reader, 4); 
                vila.madeira =  DbHelper.SafeGetInt(reader, 5);
                vila.pedra   =  DbHelper.SafeGetInt(reader, 6);
                vila.trigo   =  DbHelper.SafeGetInt(reader, 7);
                vila.nTilesTrigo =  DbHelper.SafeGetInt(reader, 10);
                vila.nTilesMadeira =  DbHelper.SafeGetInt(reader, 11);
                vila.nTilesPedra =  DbHelper.SafeGetInt(reader, 12);
                vila.DtUltAct =  DbHelper.SafeGetDatetime(reader, 8);
                vila.DtAtribuicao = DbHelper.SafeGetDatetime(reader, 9);
                break; // (if you only want the first item returned)
            }
            reader.Close();
        }
        return true;
    }

    public static bool SafeLoadEdificio(Edificio edificio, string where)
    {
        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            string query = String.Format(@"
                SELECT Edificios.Id, Edificios.Nome, Edificios.Descricao, Edificios.DtCriacao, 
                        Edificios.Level, EdificiosLevel.PopMax, Edificios.Hp, Edificios.DtUpgrade,
                        Edificios.CustoTrigo, Edificios.CustoMadeira, Edificios.CustoPedra, EdificiosLevel.Output,
                        Edificios.PlayerId, Edificios.TileID, Edificios.isBuilding, Edificios.PosTileX, Edificios.PosTileY
                FROM Edificios 
                LEFT JOIN EdificiosLevel on EdificiosLevel.Nome=Edificios.Nome and EdificiosLevel.Level=Edificios.Level 
                WHERE {0};", where);
            Console.WriteLine(query);
            connection.Open();
            SQLiteCommand command = new SQLiteCommand(query, connection);
            command.Prepare();
            SQLiteDataReader reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                Console.WriteLine("SEM LINHAS NA QUERY AO EDIFICIO");
                return false;
            }     
            while (reader.Read())
            {
                edificio.id = DbHelper.SafeGetInt(reader, 0);
                edificio.nome = DbHelper.SafeGetString(reader, 1);
                edificio.descricao = DbHelper.SafeGetString(reader, 2);
                edificio.level = DbHelper.SafeGetInt(reader, 4);
                edificio.popmax = DbHelper.SafeGetInt(reader, 5);
                edificio.hp = DbHelper.SafeGetInt(reader, 6);
                edificio.DtUpgrade = DbHelper.SafeGetDatetime(reader ,7); 
                edificio.custotrigo = DbHelper.SafeGetInt(reader, 8);
                edificio.customadeira = DbHelper.SafeGetInt(reader, 9);
                edificio.custopedra = DbHelper.SafeGetInt(reader, 10);
                edificio.output = DbHelper.SafeGetInt(reader, 11);
                edificio.playerid = DbHelper.SafeGetInt(reader, 12);
                edificio.tileid = DbHelper.SafeGetInt(reader, 13);
                edificio.isBuilding = (reader.GetBoolean(14));
                edificio.posVilaX = DbHelper.SafeGetInt(reader, 15);
                edificio.posVilaY = DbHelper.SafeGetInt(reader, 16);
                break; // (if you only want the first item returned)
            }
            reader.Close();
        }
        return true;
    }
}