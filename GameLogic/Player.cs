using System.Data.SQLite;

using Traviam.Utils;

namespace Traviam.GameLogic;

public class Player
{
    public Int32 id { get; set; }
    public Int32 CapitalX { get; set; }
    public Int32 CapitalY { get; set; }
    public string nome { get; set; } = String.Empty;
    public DateTime DtCriacao { get; set; }

    private const string CONNECTION_STRING = "Data Source=C:/Projetos/Traviam/Database/TraviamDB.db; UseUTF16Encoding=True";

    public void LoadFrom(string tipoLoad, string fieldProcurar)
    {
        //NOTA: Apenas preparado para dar load pelo nome.
        //      acrescentar maneira de dar load por outras vars.
        string query  =  String.Format(
        @"
            SELECT id, nome, DtCriacao, CapitalX, CapitalY
            FROM Players 
            WHERE {0}={1}
        ", tipoLoad, fieldProcurar);
        string jsonData = String.Empty;

        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            SQLiteCommand command = new SQLiteCommand(query, connection);
            connection.Open();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = (Int32)(reader.GetDouble(0));
                CapitalX = (Int32)(reader.GetDouble(3));
                CapitalY = (Int32)(reader.GetDouble(4));
                nome = reader.GetValue(1).ToString();
                DtCriacao = (DateTime)(reader.GetDateTime(2)); 
                break; // (if you only want the first item returned)
            }

            reader.Close();
        }
    }

    public void GeraCoordenada(float seed, string coordenada)
    {
        Random random = new Random();
        double centerX, radius, theta, r;
        centerX = 0;
    
        //Console.WriteLine(seed);
        float teste = 0f;
        teste = id / seed;
        //Console.WriteLine(teste);
        
        radius = Math.Ceiling(teste);
        //Console.WriteLine(radius);
        
        r = radius * Math.Sqrt(random.NextDouble());
        theta = random.NextDouble() * 2 * Math.PI;

        if (coordenada == "x")
        {
            CapitalX = (Int32)Math.Ceiling(centerX + r * Math.Cos(theta));
            CapitalX = CapitalX + ((GetMaxCoordenadas("x") + 1) / 2 );
            
        } else 
        {
            CapitalY = (Int32)Math.Ceiling(centerX + r * Math.Sin(theta));
            CapitalY = CapitalY + ((GetMaxCoordenadas("y") + 1) / 2 );
        }      
    }
    
    public void GeraId()
    {
        string cntQuery  = 
                @"
                    SELECT COUNT(id) FROM PLAYERS
                ";

        id = 0;
        Console.WriteLine(cntQuery);

        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            SQLiteCommand command = new SQLiteCommand(cntQuery, connection);
            connection.Open();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = (Int32)(reader.GetDouble(0));
                break; // (if you only want the first item returned)
            }
            reader.Close();
        }
        id ++;
        Console.WriteLine(id);
    }

    public void Grava()
    {
        
        string query  =  String.Format(
                @"
                    INSERT INTO Players (id, Nome, DtCriacao, CapitalX, CapitalY)
                            VALUES  ({0}, {0}, DATETIME(), {2}, {3})", id, nome, CapitalX, CapitalY);

        Console.WriteLine(query);

        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            SQLiteCommand sql_cmd = connection.CreateCommand();
            sql_cmd.CommandText = query;
            sql_cmd.ExecuteNonQuery();
            connection.Close();
        }
    }

    public void AtribuiTile()
    {
        //Atribui tile ao Player
        string query  =  String.Format(
                @"
                    UPDATE TilesMapa 
                    SET PlayerId = {0}, Madeira = 1000, Pedra = 1000, Trigo = 1000, DtAtribuicao = DATETIME()  
                    WHERE PosX = {1} AND PosY = {2}
                ", id, CapitalX, CapitalY);
        
        //Console.WriteLine(query);
        
        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            SQLiteCommand sql_cmd = connection.CreateCommand();
            sql_cmd.CommandText = query;
            sql_cmd.ExecuteNonQuery();
            connection.Close();
        }
    }
    
    public void AtribuiEdificios() //TODO: fazer método
    {

    }

    public int GetMaxCoordenadas(string coordenada)
    {
       string maxQuery  = String.Format(
                    @"
                        SELECT max(Pos{0})  FROM TilesMapa
                    ", coordenada);

        Console.WriteLine(maxQuery);
        Int32 max = 0;

        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            SQLiteCommand command = new SQLiteCommand(maxQuery, connection);
            connection.Open();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                max = (Int32)(reader.GetDouble(0));
                break; // (if you only want the first item returned)
            }
            reader.Close();
        }
        Console.WriteLine(max);
        return max; 
    }
}

