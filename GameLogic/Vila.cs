using System.Data.SQLite;
using System.Text.Json.Serialization;

using Traviam.Utils;

namespace Traviam.GameLogic;

public class Vila
{
    private static string CONNECTION_STRING = Utils.ConnectionStrings.GetDBConnString();
    
    public int id = 0;
    public string tipo = String.Empty;
    public int PlayerID = 0;
    public int x = 0;
    public int y = 0;
    public int madeira = 0;
    public int pedra = 0;
    public int trigo = 0;
    public int nTilesTrigo = 0;
    public int nTilesMadeira = 0;
    public int nTilesPedra = 0;
    [JsonIgnore] public DateTime DtAtribuicao;
    [JsonIgnore] public DateTime DtUltAct;

    /*
        TODO / NOTAS
        -Esta class foi adaptada do código do endpoint.
        -Os métodos estão a fazer alterações diretamente na base de dados.
        -Após testes, qundo tiver oportunidade alterar para = ao Gamelogic.Player    
    */


    public string UpdateVila(int x, int y)
    {
        // Buscar Info da Vila.
        // Buscar Edificios da Vila
        // Nota: Necessário fazer uma lista de eventos que vão ser resolvidos no UpdateVila.
        //       É preciso criar uma Tabela de Eventos na base de dados.
        //       Nessa tabela vão ter que ser registados os eventos com hora de inicio e hora de fim.
        //       -Ataques       -Retorno de Tropa      -Mercador   
        //
        //       Base de dados deve conter informação para ser processada e resultar num  ou vários outputs por tipo de evento.

        Console.WriteLine("-> GameLogic.Vila: Inicia UpdateVila");
        AdicionaRecurso(id);
        Console.WriteLine("-> GameLogic.Vila: Após Adiciona Recurso");
        return GetVilaData(x, y);
    } 


    public void LoadFrom(string tipoLoad, string fieldProcurar)
    {
        //NOTA: Apenas preparado para dar load pelo nome.
        //      acrescentar maneira de dar load por outras vars.
        string query  =  String.Format(
        @"
            SELECT id, TipoTile, PlayerId, PosX, PosY, Madeira, Pedra, Trigo, DtUltAct, DtAtrbuicao,
                    NumTTrigo, NumTPedra, NumTMadeira 
            FROM TilesMapa 
            WHERE {0}={1}
        ", tipoLoad, fieldProcurar);

        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            SQLiteCommand command = new SQLiteCommand(query, connection);
            connection.Open();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                this.id      = (Int32)(reader.GetDouble(0));
                this.tipo    = reader.GetString(1);
                this.PlayerID = (Int32)(reader.GetDouble(2));
                this.x       = (Int32)(reader.GetDouble(3));
                this.y       = (Int32)(reader.GetDouble(4)); 
                this.madeira = (Int32)(reader.GetDouble(5));
                this.pedra   = (Int32)(reader.GetDouble(6));
                this.trigo   = (Int32)(reader.GetDouble(7));
                this.nTilesTrigo = (Int32)(reader.GetDouble(8));
                this.nTilesMadeira = (Int32)(reader.GetDouble(9));
                this.nTilesPedra = (Int32)(reader.GetDouble(10));
                this.DtUltAct = (DateTime)(reader.GetDateTime(8));
                this.DtAtribuicao = (DateTime)(reader.GetDateTime(9));
                break; // (if you only want the first item returned)
            }
            reader.Close();
        }
    }

     public void LoadFromXY(int x, int y)
    {
        string query  = String.Format(
    @"
        SELECT id, TipoTile, PlayerId, PosX, PosY, 
               Madeira, Pedra, Trigo, DtUltAct, DtAtribuicao,
               NumTTrigo, NumTPedra, NumTMadeira 
        FROM TilesMapa 
        WHERE PosX = {0} AND PosY = {1}
    ", x, y);

        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            SQLiteCommand command = new SQLiteCommand(query, connection);
            connection.Open();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id      = (Int32)(reader.GetDouble(0));
                tipo    = reader.GetString(1);
                PlayerID = (Int32)(reader.GetDouble(2));
                x       = (Int32)(reader.GetDouble(3));
                y       = (Int32)(reader.GetDouble(4)); 
                madeira = (Int32)(reader.GetDouble(5));
                pedra   = (Int32)(reader.GetDouble(6));
                trigo   = (Int32)(reader.GetDouble(7));
                nTilesTrigo = (Int32)(reader.GetDouble(10));
                nTilesMadeira = (Int32)(reader.GetDouble(11));
                nTilesPedra = (Int32)(reader.GetDouble(12));
                DtUltAct = (DateTime)(reader.GetDateTime(8));
                DtAtribuicao = (DateTime)(reader.GetDateTime(9));
                break; // (if you only want the first item returned)
            }
            reader.Close();
        }
    }


    /* Daqui para baixo sao funcoes antigas, estão funcionais mas tem de ser adaptadas á nova arquitetura*/

    
    private void AdicionaRecurso(int idTile)
    {   
        string updateQuery = String.Empty;
        string querySomasRecursos = String.Format(
            @"
            SELECT (
                SELECT SUM(EdificiosLevel.Output) *
                    (CASE 
                        WHEN Edificios.DtCriacao > TilesMapa.DtUltAct THEN ((strftime('%s', DATETIME()) - strftime('%s',  Edificios.DtCriacao))/60)
                        WHEN Edificios.DtCriacao <= TilesMapa.DtUltAct THEN ((strftime('%s', DATETIME()) - strftime('%s',  TilesMapa.DtUltAct))/60)
                    END) 
                FROM Edificios
                LEFT JOIN EdificiosLevel 
                    ON EdificiosLevel.Nome = Edificios.Nome AND Edificios.Level = EdificiosLevel.Level
                INNER JOIN TilesMapa
                    ON Edificios.TileID = TilesMapa.Id
                WHERE 
                    Edificios.isBuilding = 0 AND
                    TilesMapa.ID = {0} AND
                    Edificios.Nome = 'Quinta' 
            ) AS SumTrigo, 
            (
                SELECT SUM(EdificiosLevel.Output) *
                    (CASE 
                        WHEN Edificios.DtCriacao > TilesMapa.DtUltAct THEN ((strftime('%s', DATETIME()) - strftime('%s',  Edificios.DtCriacao))/60)
                        WHEN Edificios.DtCriacao <= TilesMapa.DtUltAct THEN ((strftime('%s', DATETIME()) - strftime('%s',  TilesMapa.DtUltAct))/60)
                    END) 
                FROM Edificios
                LEFT JOIN EdificiosLevel 
                    ON EdificiosLevel.Nome = Edificios.Nome AND Edificios.Level = EdificiosLevel.Level
                INNER JOIN TilesMapa
                    ON Edificios.TileID = TilesMapa.Id
                WHERE 
                    Edificios.isBuilding = 0 AND
                    TilesMapa.ID = {0} AND
                    Edificios.Nome = 'Pedreira' 

            ) as SumPedra, (
                SELECT SUM(EdificiosLevel.Output) *
                    (CASE 
                        WHEN Edificios.DtCriacao > TilesMapa.DtUltAct THEN ((strftime('%s', DATETIME()) - strftime('%s',  Edificios.DtCriacao))/60)
                        WHEN Edificios.DtCriacao <= TilesMapa.DtUltAct THEN ((strftime('%s', DATETIME()) - strftime('%s',  TilesMapa.DtUltAct))/60)
                    END) 
                FROM Edificios
                LEFT JOIN EdificiosLevel 
                    ON EdificiosLevel.Nome = Edificios.Nome AND Edificios.Level = EdificiosLevel.Level
                INNER JOIN TilesMapa
                    ON Edificios.TileID = TilesMapa.Id
                WHERE 
                    Edificios.isBuilding = 0 AND
                    TilesMapa.ID = {0} AND
                    Edificios.Nome = 'Floresta'
            ) as SumMadeira", idTile);
        Console.WriteLine("Query Recursos Atualizar:");
        Console.WriteLine(querySomasRecursos);

        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            SQLiteCommand command = new SQLiteCommand(querySomasRecursos, connection);
            connection.Open();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int addTrigo = (Int32) reader.GetDouble(0);
                Console.WriteLine(String.Format("Trigo a adicionar a vila: {0}",addTrigo));
                
                int addPedra = (Int32) reader.GetDouble(1);
                Console.WriteLine(String.Format("Pedra a adicionar a vila: {0}",addPedra));

                int addMadeira = (Int32) reader.GetDouble(2);
                Console.WriteLine(String.Format("Madeira a adicionar a vila: {0}",addMadeira));

                this.madeira = this.madeira + addMadeira;
                this.pedra = this.pedra + addPedra;
                this.trigo = this.trigo + addTrigo;

                /*
                updateQuery = String.Format(
                        @"
                            UPDATE TilesMapa
                            SET Madeira={0}, Pedra={1}, Trigo={2}, DtUltAct=DATETIME()
                            WHERE id={3}
                        ", (trigo + addTrigo), (pedra + addPedra), (madeira + addMadeira), idTile);
                */
                break; // (if you only want the first item returned)
            }
            reader.Close();

            command = connection.CreateCommand();
            command.CommandText = 
                        @" UPDATE TilesMapa
                            SET Madeira=@Madeira, Pedra=@Pedra, Trigo=@Trigo, DtUltAct=DATETIME()
                            WHERE id=@Id";

            command.Parameters.AddWithValue("@Id", this.id);
            command.Parameters.AddWithValue("@Madeira", this.madeira);
            command.Parameters.AddWithValue("@Pedra", this.pedra);
            command.Parameters.AddWithValue("@Trigo", this.trigo);
            command.ExecuteNonQuery();
            connection.Close();
        }
    }


    private static int GetValorAdicionar(int idTile) //TODO - Colocar parte código do AdicionaRecursos
    {

        return 0;
    }


    private static int GetRecursosVila(int idTile, string recurso)
    {
        Int32 valorRecurso = 0;
        string idQuery  = String.Format(
                        @"
                            SELECT {0} 
                            FROM TilesMapa
                            WHERE Id = {1} 
                        ", recurso, idTile);

        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            SQLiteCommand command = new SQLiteCommand(idQuery, connection);
            connection.Open();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                valorRecurso = (Int32)(reader.GetDouble(0));
                break; // (if you only want the first item returned)
            }
            reader.Close();
        }

        return valorRecurso;        
    }
   

    private string GetVilaData(int x, int y)
    {
        string query  =  String.Format(
        @"
            SELECT id, tipoTile, PlayerID, Madeira, Pedra, Trigo, DtUltAct, DtAtribuicao, NumTTrigo, NumTPedra, NumTmadeira
            FROM TilesMapa 
            WHERE PosX = {0} AND PosY = {1}
        ", x, y);
        string jsonData = String.Empty;

        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            SQLiteCommand command = new SQLiteCommand(query, connection);
            connection.Open();
            SQLiteDataReader reader = command.ExecuteReader();
            JsonUtils jsonUtils = new JsonUtils();
            return jsonData = jsonUtils.sqlDatoToJson(reader);
        }
    }  

}

