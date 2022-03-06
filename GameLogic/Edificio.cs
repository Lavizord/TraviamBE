using System.Data.SQLite;
using System.Text.Json.Serialization;

using Traviam.Utils;

namespace Traviam.GameLogic;
public class Edificio 
{ 

    private static string CONNECTION_STRING = Utils.ConnectionStrings.GetDBConnString();
    public Int32 id { get; set; }
    public string nome { get; set; }
    public string descricao { get; set; } 
    public Int32 level { get; set; } 
    public Int32 popmax { get; set; }   // não está a ser usado 
    public Int32 hp { get; set; } 
    public Int32 custotrigo { get; set; } 
    public Int32 customadeira { get; set; } 
    public Int32 custopedra { get; set; } 
    public Int32 output { get; set; } 
    public Int32 playerid { get; set; } 
    public Int32 tileid { get; set; } 
    public bool isBuilding { get; set; }
    
    public Int32 posVilaX { get; set; }
    public Int32 posVilaY { get; set; }

    public DateTime DtUpgrade { get; set; } 

    [JsonIgnore] public DateTime DtCriacao { get; set; } 

    public bool LoadFromId(int id) 
    {
        string where = String.Format(@"Edificios.Id={0}", id);
        return DbHelper.SafeLoadEdificio(this, where); 
    }

    public bool LoadFromIdXY(int id, int x, int y) 
    {
        string where = String.Format(@"Edificios.TileID={0} AND 
                                        edificios.PosTileX = {1} AND 
                                        edificios.PosTileY = {2}", id,x,y);
        return DbHelper.SafeLoadEdificio(this, where); 
    }

    public void CriaEdificio(string nome, int tileID, int posX, int posY, int level, int playerid=0, bool flagGrava=true)
    {
        this.id = GeraId();   
        this.level = level; 
        
        nome = '"'+nome+'"';

        string query  =  String.Format(
        @"
            SELECT nome, hp, UpTimeMin, CustoTrigo, 
                   CustoMadeira, CustoPedra, Output, PopMax, Descricao, DATETIME() 
            FROM EdificiosLevel 
            WHERE nome={0} and level = {1}
        ", nome, this.level);

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
                this.popmax     = DbHelper.SafeGetInt(reader, 7);
                Console.WriteLine(this.popmax);

                this.output     = DbHelper.SafeGetInt(reader, 6);
                Console.WriteLine(this.output);
                
                this.descricao  = reader.GetValue(8).ToString();
                this.playerid   = playerid;
                this.tileid     = tileID;
                 Console.WriteLine(this.tileid);
                this.DtCriacao = (DateTime)(reader.GetDateTime(9)); 
                this.DtUpgrade = (DateTime)(reader.GetDateTime(9));
                this.posVilaX = posX;
                this.posVilaY = posY;
                Console.WriteLine(":::: Foi CRIADO um edificios.");
                break; // (if you only want the first item returned)
            }
            reader.Close();
        }
        if (flagGrava)
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
                                    DtCriacao, isBuilding, PosTileX, PosTileY)
                                    VALUES 
                                    (@Id, @nome, @descricao, @level, @hp,
                                    @CustoTrigo, @CustoMadeira, @CustoPedra, @PlayerId,
                                    @TileID, @DtUpgrade, @DtCriacao, @isBuilding, @x, @y)";

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
            sql_cmd.Parameters.AddWithValue("@isBuilding", this.isBuilding);
            sql_cmd.Parameters.AddWithValue("@x", this.posVilaX);
            sql_cmd.Parameters.AddWithValue("@y", this.posVilaY);

            sql_cmd.ExecuteNonQuery();
            connection.Close();
        }

    }

    private void UpdateDados()
    {
        using (SQLiteConnection connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            SQLiteCommand sql_cmd = connection.CreateCommand();
            sql_cmd.CommandText = @" UPDATE Edificios SET 
                                    Nome=@nome, Descricao=@desc, level=@level, hp=@hp, 
                                    DtUpgrade=@dtup, custoTrigo=@ctrigo, custoMadeira = @cmadeira,
                                    custoPedra = @cpedra, isBuilding=@isbuilding, PlayerId=@pid, 
                                    PosTileX =@x, PosTileY = @y
                                    WHERE id=@id";

            sql_cmd.Parameters.AddWithValue("@id", this.id);
            sql_cmd.Parameters.AddWithValue("@nome", this.nome);
            sql_cmd.Parameters.AddWithValue("@desc", this.descricao);
            sql_cmd.Parameters.AddWithValue("@level", this.level);
            sql_cmd.Parameters.AddWithValue("@hp", this.hp);
            sql_cmd.Parameters.AddWithValue("@dtup", this.DtUpgrade);
            sql_cmd.Parameters.AddWithValue("@ctrigo", this.custotrigo);
            sql_cmd.Parameters.AddWithValue("@cmadeira", this.customadeira);
            sql_cmd.Parameters.AddWithValue("@cpedra", this.custopedra);
            sql_cmd.Parameters.AddWithValue("@isbuilding", this.isBuilding);
            sql_cmd.Parameters.AddWithValue("@pid", this.playerid);
            sql_cmd.Parameters.AddWithValue("@x", this.posVilaX);
            sql_cmd.Parameters.AddWithValue("@y", this.posVilaY);
            sql_cmd.Prepare();
            sql_cmd.ExecuteNonQuery();
            connection.Close();
        }
    }

    public bool Upgrade() // Verfifica se vila tem recursos suficiente para fazer upgrade do edificio.
    {
        Vila vila = new Vila();
        vila.LoadFromId(this.tileid);
        Edificio edificio = new Edificio();
        int upgradeLvl = this.level + 1;
        edificio.CriaEdificio(this.nome, this.tileid, this.posVilaX, this.posVilaY,  upgradeLvl, this.playerid, flagGrava:false);
        Console.WriteLine("Edificio criado no upgrade: {0}, level:{1}, custoMadeira: {2}, custoPedra: {3}, , custoTrigo: {4}",
                                                 edificio.nome, edificio.level, edificio.customadeira, edificio.custopedra, edificio.custotrigo);
        if(vila.madeira - edificio.customadeira > 0 && vila.pedra - edificio.custopedra > 0 && vila.trigo - edificio.custotrigo > 0 ){
            Console.WriteLine("Recursos Suficientes para Upgrade!");
            edificio.DtUpgrade = DateTime.Now.AddMinutes(edificio.level*10);
            edificio.isBuilding = true;
            Console.WriteLine("Data de fim de Ugrade: {0}, isBuilding:", edificio.DtUpgrade, edificio.isBuilding);
            edificio.id = this.id;
            edificio.UpdateDados();
            return true;
        }
        return false;
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


}
