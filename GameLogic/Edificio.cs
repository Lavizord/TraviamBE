using System.Data.SQLite;

using Traviam.Utils;

namespace Traviam.GameLogic;
public class Edificio 
{ 

    private const string CONNECTION_STRING = ConnectionStrings.CONNECTION_STRING;
    public Int32 id { get; set; }
    public string nome { get; set; }
    public string descricao { get; set; } 
    public Int32 level { get; set; } 
    public Int32 popmax { get; set; }
    public Int32 hp { get; set; } 
    public Int32 custotrigo { get; set; } 
    public Int32 customadeira { get; set; } 
    public Int32 custopedra { get; set; } 
    public Int32 output { get; set; } 
    public Int32 playerid { get; set; } 
    public Int32 tileid { get; set; } 
    public DateTime DtUpgrade { get; set; } 
    public DateTime DtCriacao { get; set; } 
    public bool isBuilding { get; set; }

    public void LoadFrom(string tipoLoad, string fieldProcurar) //Edificio.LoadFrom("id", 2)
    {
        //NOTA: Apenas preparado para dar load pelo Id do Edificio.
        //      acrescentar maneira de dar load por outras vars.
        string query  =  String.Format(
        @"
            SELECT Edificios.Id, Edificios.Nome, Edificios.Descricao, Edificios.DtCriacao, 
                    Edificios.Level, EdificiosLevel.PopMax, Edificios.Hp, Edificios.DtUpgrade,
                    Edificios.CustoTrigo, Edificios.CustoMadeira, Edificios.CustoPedra, EdificiosLevel.Output,
                    Edificios.PlayerId, Edificios.TileID, Edificios.isBuilding
            FROM Edificios 
            LEFT JOIN EdificiosLevel on EdificiosLevel.Nome=Edificios.Nome and EdificiosLevel.Level=Edificios.Level 
            WHERE {0}={1}
        ", tipoLoad, fieldProcurar);

        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            SQLiteCommand command = new SQLiteCommand(query, connection);
            connection.Open();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                this.id = (Int32)(reader.GetDouble(0));
                this.nome = reader.GetValue(1).ToString();
                this.descricao = reader.GetValue(2).ToString();
                this.level = (Int32)(reader.GetDouble(4));
                this.popmax = (Int32)(reader.GetDouble(5));
                this.hp = (Int32)(reader.GetDouble(6));
                this.DtUpgrade = (DateTime)(reader.GetDateTime(7)); 
                this.custotrigo = (Int32)(reader.GetDouble(8));
                this.customadeira = (Int32)(reader.GetDouble(9));
                this.custopedra = (Int32)(reader.GetDouble(10));
                this.output = (Int32)(reader.GetDouble(11));
                this.playerid = (Int32)(reader.GetDouble(12));
                this.tileid = (Int32)(reader.GetDouble(13));
                this.isBuilding = (reader.GetBoolean(14));
                break; // (if you only want the first item returned)
            }
            reader.Close();
        }
    }

    public void CriaEdificio(string nome, int tileID, int pId)
    {
        this.id = GeraId();   
        this.level = 1; 
        
        nome = '"'+nome+'"';

        string query  =  String.Format(
        @"
            SELECT nome, hp, UpTimeMin, CustoTrigo, 
                   CustoMadeira, CustoPedra, Output, PopMax, Descricao, DATETIME() 
            FROM EdificiosLevel 
            WHERE nome={0} and level = 1
        ", nome);

        Console.WriteLine(query);

        string jsonData = String.Empty;

        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            SQLiteCommand command = new SQLiteCommand(query, connection);
            connection.Open();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                this.nome       = reader.GetValue(0).ToString();
                this.hp         = (Int32)(reader.GetDouble(1));
                this.custotrigo = (Int32)(reader.GetDouble(3));
                this.customadeira = (Int32)(reader.GetDouble(4));
                this.custopedra = (Int32)(reader.GetDouble(5));   
                this.output     = DbHelper.SafeGetInt(reader, 6);
                Console.WriteLine(output);

                this.popmax     = DbHelper.SafeGetInt(reader, 7);
                 Console.WriteLine(this.popmax);
                this.descricao  = reader.GetValue(8).ToString();
                this.playerid   = pId;
                this.tileid     = tileID;
                 Console.WriteLine(this.tileid);
                this.DtCriacao = (DateTime)(reader.GetDateTime(9)); 
                this.DtUpgrade = (DateTime)(reader.GetDateTime(9));

                break; // (if you only want the first item returned)
            }
            reader.Close();
        }
        this.Grava();  
    }

    private void Grava()
    {
        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            SQLiteCommand sql_cmd = connection.CreateCommand();
            //sql_cmd.CommandText = query;
            sql_cmd.CommandText = @"INSERT INTO edificios 
                                    (id, nome, descricao, level, hp,
                                    CustoTrigo, CustoMadeira, CustoPedra, PlayerId, TileID, DtUpgrade, 
                                    DtCriacao, isBuilding)
                                    VALUES 
                                    (@Id, @nome, @descricao, @level, @hp,
                                    @CustoTrigo, @CustoMadeira, @CustoPedra, @PlayerId,
                                    @TileID, @DtUpgrade, @DtCriacao, @isBuilding)";

            sql_cmd.Parameters.AddWithValue("@Id", this.id);
            sql_cmd.Parameters.AddWithValue("@nome", this.nome);
            sql_cmd.Parameters.AddWithValue("@descricao", this.descricao);
            sql_cmd.Parameters.AddWithValue("@level", this.level);
            sql_cmd.Parameters.AddWithValue("@hp", this.hp);
            sql_cmd.Parameters.AddWithValue("@custoTrigo", this.custotrigo);
            sql_cmd.Parameters.AddWithValue("@CustoMadeira", this.customadeira);
            sql_cmd.Parameters.AddWithValue("@CustoPedra", this.custopedra);
            sql_cmd.Parameters.AddWithValue("@PlayerId", this.playerid);
            sql_cmd.Parameters.AddWithValue("@TileId", this.tileid);
            sql_cmd.Parameters.AddWithValue("@DtUpgrade", this.DtUpgrade);
            sql_cmd.Parameters.AddWithValue("@DtCriacao", this.DtCriacao);
            sql_cmd.Parameters.AddWithValue("isBuilding", this.isBuilding);

            sql_cmd.ExecuteNonQuery();
            connection.Close();
        }

    }

    private void Update()
    {
        string query  =  String.Format(
                @"
                ", this.id, this.nome, this.descricao, this.level, this.popmax, this.hp, this.custotrigo,
                this.customadeira, this.custopedra, this.playerid, this.tileid);

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

    private static int GeraId()
    {
        string cntQuery  = 
                @"
                    SELECT COUNT(id) FROM Edificios
                ";

        Int32 id = 0;
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
        //Console.WriteLine(id);
        return id;
    }


    /*
    SELECT id,Nome,Descricao,Level,PopMax,Hp,UpTime,CustoTrigo,CustoMadeira,CustoPedra,EdificiosLevel.Output,PlayerId,TileID  
        FROM Edificios   
        LEFT JOIN EdificiosLevel 
            ON EdificiosLevel.Nome=Edificios.Nome and EdificiosLevel.Level=Edificios.Level    
    */

}
