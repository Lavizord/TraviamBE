using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.Linq;
using System.Data;
using System.Data.SQLite;
using Newtonsoft.Json;
using System.Text.Json;
using Swashbuckle.AspNetCore.Annotations;

using Traviam.Utils;
using Traviam.GameLogic;

namespace Traviam.Controllers;

[ApiController]
[Route("[controller]")]
public class MapaController : ControllerBase
{
    private string CONNECTION_STRING = Utils.ConnectionStrings.GetDBConnString();


    private readonly ILogger<MapaController> _logger;


    public MapaController(ILogger<MapaController> logger)
    {
        _logger = logger;
    }
   


    [HttpPost("Cria")]
    [SwaggerOperation(Summary = "Cria mapa inicial do Jogo.", Description = "Cria mapa com X = Y = GridSize passado. Tem de ser executado uma vez no inicio de cada jogo antes da criação de jogadores.")]   
    public IActionResult  Cria(int gridSize)
    {
        Int32 numTotalTiles = gridSize * gridSize;
        string getGeradorTiles  = 
                @"
                    SELECT PercentTiles, TipoTile, NumTrigo, NumMadeira, NumPedra 
                    FROM GeradorTiles
                    ORDER BY PercentTiles
                ";

        List<string[]> tilesGerados = new List<string[]>();
        
        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            SQLiteCommand command = new SQLiteCommand(getGeradorTiles, connection);
            connection.Open();
            SQLiteDataReader reader = command.ExecuteReader();
            
            while(reader.Read())                              //Percorre as colunas da Tabela
            {
               GeraTile( tilesGerados, reader, numTotalTiles); //Gera Lista de Tiles.
            }
            reader.Close();
        }

        var rnd = new Random();
        var randomTiles = tilesGerados.OrderBy(item => rnd.Next()).ToList();
        int countX, countY, id, idT,xMax, yMax;
        id = 1;
        idT= 0;
        xMax = gridSize ;
        yMax = gridSize ;

        for ( countX = 0; countX < xMax; countX++ ) {
            for (  countY = 0; countY < yMax; countY++ ) {
                
                string insTileQuery = String.Format(
                        @"INSERT INTO TilesMapa (id, TipoTile, PosX, PosY, NumTTrigo, NumTMadeira, NumTPedra, DtUltAct, DtAtribuicao)
                                VALUES ({0}, '{1}', {2}, {3}, {4}, {5}, {6}, DATETIME(), DATETIME())
                        ", id, randomTiles[idT][0], countX.ToString(), countY.ToString(), randomTiles[idT][1], randomTiles[idT][2], randomTiles[idT][3]
                        ); 
                id++;
                idT++;
                
                //Console.WriteLine(insTileQuery); // CONSOLE LOG DA QUERY GERADA
                
                using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
                {
                    connection.Open();
                    SQLiteCommand sql_cmd = connection.CreateCommand();
                    sql_cmd.CommandText = insTileQuery;
                    sql_cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }
        return Ok();
    }


    [HttpGet("")]
    [Produces("application/json")]
    [SwaggerOperation(Summary = "Retorna JSON de tiles do mapa.", Description = "Retorna uma lista completa de tiles do Mapa.")]   
    public IActionResult Get()
    {
         string query  = 
                    @"
                        SELECT id, TipoTile, PlayerId, PosX, PosY, NumTTrigo, NumTPedra, NumTMadeira 
                        FROM TilesMapa
                        ORDER by PosY DESC,posx asc 
                    ";

        string map = String.Empty;
        //Console.WriteLine(query);
        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            SQLiteCommand command = new SQLiteCommand(query, connection);
            connection.Open();
            SQLiteDataReader reader = command.ExecuteReader();
            if(!reader.HasRows)
            {
                return NotFound();
            }
            JsonUtils jsonUtils = new JsonUtils();

            var myData = new
            {
                xMax = GameLogic.Mapa.GetMaxCoordenadas("x") + 1,
                yMax = GameLogic.Mapa.GetMaxCoordenadas("y") + 1, 
                mapa = jsonUtils.sqlDatoToJson(reader)
            };
            return new OkObjectResult(myData);
        }
    }


    [SwaggerOperation(Summary = "Faz delete ao Mapa, aos Jogadores e aos Edificios.")]   
    [HttpDelete("LimpaJogo")]
    public IActionResult  LimpaJogo()
    {
        string  query = 
                @"
                    DELETE FROM TilesMapa;
                    DELETE FROM Players;
                    DELETE FROM TropasMovimento;
                    DELETE FROM Edificios;
                ";

        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            SQLiteCommand sql_cmd = connection.CreateCommand();
            sql_cmd.CommandText = query;
            sql_cmd.ExecuteNonQuery();
            connection.Close();
        }
        return Ok();
    }


    public static void GeraTile(List<string[]> tilesGerados, IDataReader dataReader, Int32 numTotalTiles)
    {
        Int32 nTilesGerar = (Int32)(numTotalTiles * dataReader.GetDecimal(0));

        while(nTilesGerar > 0)
        {
            List<string> tile = new List<string>(); // { dataReader.GetString(1), dataReader.GetString(2), dataReader.GetString(3), dataReader.GetString(4)};
            int i;
            for ( i = 1; i < 5; i++ ) {
                tile.Add(dataReader.GetValue(i).ToString());
            }
            tilesGerados.Add(tile.ToArray());

            nTilesGerar--;
        }                                   
    }


    internal class Mapa
    {
        public int xMax { get; set; }
        public int yMax { get; set; }
        public string mapa { get; set; }
    }

}
