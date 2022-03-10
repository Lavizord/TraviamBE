using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.Linq;
using System.Data;
using System.Data.SQLite;
using System.Text.Json;
using Swashbuckle.AspNetCore.Annotations;

using Traviam.GameLogic;
using Traviam.Utils;

namespace Traviam.Controllers;

[ApiController]
[Route("[controller]")]
public class EdificiosController : ControllerBase
{
    private string CONNECTION_STRING = Utils.ConnectionStrings.GetDBConnString();
    

    private Edificio edificio;
    private Vila vila;

    private readonly ILogger<EdificiosController> _logger;


    public EdificiosController(ILogger<EdificiosController> logger)
    {
        _logger = logger;
    }


    [HttpGet("")]
    [Produces("application/json")]
    [SwaggerOperation(Summary = "Retorna JSON com info do Edificio.", Description = "Com base no ID fornecido retorna info do edificio.")]
    public IActionResult Get(int id)
    {  
        // TODO: Atualizar o edificios? (tempo de construção)
        edificio = new Edificio();
        edificio.LoadFromId(id);
        return Ok(JsonSerializer.Serialize(edificio));
    }


    [HttpPatch("Upgrade")]
    [SwaggerOperation(Summary = "Realiza um upgrade ao Edificio.", Description = "Inicia o Upgrade de um edificio com base no id passado. Deduz os recursos a vila.")]
    public IActionResult Upgrade(int id)
    {
        edificio = new Edificio();
        edificio.LoadFromId(id);
        if(edificio.Upgrade()){
            return Ok();
        }
        return BadRequest() ;
    }


    [HttpPost("Cria")]
    [Produces("application/json")]
    [SwaggerOperation(Summary = "Inicia a criação de um novo edificio.", Description = "Com base nos dados pasados incia a construção de um novo edificio lvl1.")]
    public IActionResult Cria(int tileId, int playerID, String nome, int x, int y)
    {
        edificio = new Edificio();
        vila = new Vila();
        if(vila.LoadFromId(tileId) == false){
            return NotFound();
        }

        vila.UpdateVila();
        
        if( vila.HasResourcesToEdificio(edificio)){
            vila.SubtraiCustoEdi(edificio);
            edificio.CriaEdificio(nome, tileId, x, y, 1, playerID, flagGrava:false);
            edificio.isBuilding = true;
            edificio.DtUpgrade = DateTime.Now.AddMinutes(edificio.level * 10); 
            edificio.UpdateDados();
            return Ok(JsonSerializer.Serialize(edificio));
        }   
        return BadRequest();     
    }
}
