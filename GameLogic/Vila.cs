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

    /*
        TODO: Necessário adicionar a info dos edificios, com as devidas coordenadas 
              de forma a que Miguel possa fazer load dessa info para o ecrã 
    */


    public string UpdateVila()
    {
        // Buscar Info da Vila.
        // Buscar Edificios da Vila -> Atualizar os que já foram construidos.
        // Nota: Necessário fazer uma lista de eventos que vão ser resolvidos no UpdateVila.
        //       É preciso criar uma Tabela de Eventos na base de dados.
        //       Nessa tabela vão ter que ser registados os eventos com hora de inicio e hora de fim.
        //       -Ataques       -Retorno de Tropa      -Mercador   
        //
        //       Base de dados deve conter informação para ser processada e resultar num  ou vários outputs por tipo de evento.

        Console.WriteLine("-> GameLogic.Vila: Inicia UpdateVila");
        this.AdicionaRecurso();
        this.UpdateDados();
        Console.WriteLine("-> GameLogic.Vila: Após Adiciona Recurso");
        return GetVilaData(x, y);
    } 

 /* Daqui para baixo sao funcoes antigas, estão funcionais mas tem de ser adaptadas á nova arquitetura*/

    public bool LoadFromId(int id) //Carrega Objeto com dados da BD já existentes
    {
        string where = String.Format("id = {0}", id);
        return DbHelper.SafeLoadVila(this, where);
    }

    public bool LoadFromXY(int x, int y) //Carrega Objeto com dados da BD já existentes
    {
        string where = String.Format("PosX = {0} AND PosY = {1}", x, y);
        return DbHelper.SafeLoadVila(this, where);
    }

    public bool LoadRandomTipo(string tipo, bool ocupado=false) //Carrega com dados de um tile aleatório. default carrega um tile sem ser de um jogador. 
    {
        string where = String.Empty;
        if(ocupado){
            where  = String.Format(@" TipoTile = '{0}' AND PlayerId IS NULL
                        LIMIT 1", tipo);
        } else {
            where  = String.Format(@" TipoTile = '{0}' AND PlayerId IS NOT NULL
                        LIMIT 1", tipo);
        }
        return DbHelper.SafeLoadVila(this, where);
    }

    public void UpdateDados() //Faz o update da vila, tendo sido o obj previament carregado.
    {
        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            SQLiteCommand sql_cmd = connection.CreateCommand();
            sql_cmd.CommandText = @" UPDATE TilesMapa 
                                     SET TipoTile = @tipotile,
                                         PlayerId = @playerid,
                                         posx = @x,
                                         posy = @y,
                                         madeira = @madeira,
                                         pedra = @pedra,
                                         trigo = @trigo,
                                         numTTrigo = @ntt,
                                         numtpedra = @ntp,
                                         numtmadeira = @ntm,
                                         DtUltAct=DATETIME()
                                     WHERE id=@Id ;";
                                     
            sql_cmd.Parameters.AddWithValue("@tipotile", this.tipo);
            sql_cmd.Parameters.AddWithValue("@playerid", this.PlayerID);
            sql_cmd.Parameters.AddWithValue("@x", this.x);
            sql_cmd.Parameters.AddWithValue("@y", this.y);
            sql_cmd.Parameters.AddWithValue("@madeira", this.madeira);
            sql_cmd.Parameters.AddWithValue("@pedra", this.pedra);
            sql_cmd.Parameters.AddWithValue("@trigo", this.trigo);
            sql_cmd.Parameters.AddWithValue("@ntt", this.nTilesTrigo);
            sql_cmd.Parameters.AddWithValue("@ntp", this.nTilesPedra);
            sql_cmd.Parameters.AddWithValue("@ntm", this.nTilesMadeira);
            sql_cmd.Parameters.AddWithValue("@id", this.id);
            sql_cmd.Prepare();
            sql_cmd.ExecuteNonQuery();
            connection.Close();
        }
    }

    private void AdicionaRecurso()
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
            ) as SumMadeira", this.id);

        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            SQLiteCommand command = new SQLiteCommand(querySomasRecursos, connection);
            connection.Open();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int addTrigo = (Int32) reader.GetDouble(0);                
                int addPedra = (Int32) reader.GetDouble(1);
                int addMadeira = (Int32) reader.GetDouble(2);
                this.madeira = this.madeira + addMadeira;
                this.pedra = this.pedra + addPedra;
                this.trigo = this.trigo + addTrigo;

                break; // (if you only want the first item returned)
            }
            reader.Close();
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

