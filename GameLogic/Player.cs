using System.Data.SQLite;
using System.Text.Json.Serialization;

using Traviam.Utils;

namespace Traviam.GameLogic;

public class Player
{
    public Int32 id { get; set; }
    public Int32 CapitalX { get; set; }
    public Int32 CapitalY { get; set; }
    public string nome { get; set; }
    [JsonIgnore] public DateTime DtCriacao { get; set; }
    public List<Vila> vilas { get; set; }
    
    private string CONNECTION_STRING = Utils.ConnectionStrings.GetDBConnString();

    public bool LoadFromNome(string nome) //Carrega Objeto com dados da BD já existentes
    {   
        string where = String.Format("nome = {0}", nome);
        return DbHelper.SafeLoadPlayer(this, where);
    }
   
    public bool LoadFromId(int id)//Carrega Objeto com dados da BD já existentes
    {   
        string where = String.Format("id = {0}", id);
        return DbHelper.SafeLoadPlayer(this, where);
    }

    public void GeraCoordenada(float seed, string coordenada)//Altera CapitalX e Y do objeto para umas Novas
    {
        Random random = new Random();
        double centerX, radius, theta, r;
        float divisionvalue = 0f;
        centerX = 0;
    
        divisionvalue = id / seed;
        radius = Math.Ceiling(divisionvalue);
        
        r = radius * Math.Sqrt(random.NextDouble());
        theta = random.NextDouble() * 2 * Math.PI;

        if (coordenada == "x")
        {
            this.CapitalX = (Int32)Math.Ceiling(centerX + r * Math.Cos(theta));
            this.CapitalX = CapitalX + ((Mapa.GetMaxCoordenadas("x") + 1) / 2 );
            
        } else 
        {
            this.CapitalY = (Int32)Math.Ceiling(centerX + r * Math.Sin(theta));
            this.CapitalY = CapitalY + ((Mapa.GetMaxCoordenadas("y ") + 1) / 2 );
        }      
    }

    public void Init(string nome) //Altera ID do objeto para um novo disponível na BD.
    {
        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {   
            this.id = 0;
            string cntQuery  = @" SELECT COUNT(id) FROM PLAYERS ";
            SQLiteCommand command = new SQLiteCommand(cntQuery, connection);
            connection.Open();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                this.id = DbHelper.SafeGetInt(reader, 0);
                break; // (if you only want the first item returned)
            }
            reader.Close();
        }
        this.id ++;
        this.nome = nome;
    }

    public void GravaDados() //Faz o insert inicial dos dados no jogador, tendo sido o obj previament criado.
    {
        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            SQLiteCommand sql_cmd = connection.CreateCommand();
            sql_cmd.CommandText = @" INSERT INTO Players (id, Nome, DtCriacao, CapitalX, CapitalY)
                                    VALUES (@Id, @nome, DATETIME(), @x, @y) ";
            sql_cmd.Parameters.AddWithValue("@Id", this.id);
            sql_cmd.Parameters.AddWithValue("@nome", this.nome);
            sql_cmd.Parameters.AddWithValue("@x", this.CapitalX);
            sql_cmd.Parameters.AddWithValue("@y", this.CapitalY);
            Console.WriteLine(sql_cmd.CommandText);
            sql_cmd.ExecuteNonQuery();
            connection.Close();
        }
    }

    public void UpdateDados() //Faz o update dos dados jogador, tendo sido o obj previament carregado.
    {
        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            SQLiteCommand sql_cmd = connection.CreateCommand();
            sql_cmd.CommandText = @" UPDATE Players SET 
                                    id=@id, Nome=@nome, DtCriacao=@dtcriacao, CapitalX=@capitalx, CapitalY=@capitaly";
            sql_cmd.Parameters.AddWithValue("@id", this.id);
            sql_cmd.Parameters.AddWithValue("@nome", this.nome);
            sql_cmd.Parameters.AddWithValue("@dtcriacao", this.DtCriacao);
            sql_cmd.Parameters.AddWithValue("@capitalx", this.CapitalX);
            sql_cmd.Parameters.AddWithValue("@capitaly", this.CapitalY);
            sql_cmd.ExecuteNonQuery();
            connection.Close();
        }
    }

    public void AtribuiTile() //Atribui tile ao Player
    {
        Console.WriteLine("::ATRIBUI TILE AO JOGADOR");
        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            // Vamos ter que guardar o tipo do tile que vamos substituir. 
            Vila tileAntigo = new Vila();
            tileAntigo.LoadFromXY(this.CapitalX, this.CapitalY);
            if(tileAntigo.tipo != "standard")
            {
                Console.WriteLine(":::::::::::: TILE SWITCH");
                Vila tileNovo =  new Vila();
                tileNovo.LoadRandomTipo(tileAntigo.tipo);
                tileNovo.tipo = tileAntigo.tipo;
                tileNovo.nTilesMadeira = tileAntigo.nTilesMadeira;
                tileNovo.nTilesPedra = tileAntigo.nTilesPedra;
                tileNovo.nTilesTrigo = tileAntigo.nTilesTrigo;
                tileNovo.UpdateDados();                
            }
            //Agora vamos dar o tile ao jogador.
            SQLiteCommand sql_cmd = connection.CreateCommand();
            //sql_cmd.CommandText = query;
            sql_cmd.CommandText = @" 
                    UPDATE TilesMapa 
                    SET PlayerId = @id, Madeira = 1000, Pedra = 1000, Trigo = 1000, 
                        DtAtribuicao = DATETIME(), 
                        tipotile = 'standard', numttrigo=3, numtmadeira=3, numtpedra=3
                    WHERE PosX = @x AND PosY = @y ";

            sql_cmd.Parameters.AddWithValue("@Id", this.id);
            sql_cmd.Parameters.AddWithValue("@x", this.CapitalX);
            sql_cmd.Parameters.AddWithValue("@y", this.CapitalY);
            sql_cmd.ExecuteNonQuery();
            connection.Close();
        }
    }
    
    /*      NOVO CÓDIGO RELACIONADO COM VILAS   */
    
    public void LoadVilas() 
    {
        this.vilas = DbHelper.GetVilasJogador(this.id);
    }
}

