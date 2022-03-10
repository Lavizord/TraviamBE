using Traviam.Utils;

namespace Traviam.GameLogic;

public class Evento
{
    private static string CONNECTION_STRING = Utils.ConnectionStrings.GetDBConnString();
    public int id { get; set; }
    public string tipo { get; set; }
    

    public bool LoadFromId(int idEvento)
    {   
        string where = String.Format(@"Eventos.Id={0}", idEvento);
        return true;
    }
}
