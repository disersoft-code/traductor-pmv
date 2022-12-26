using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApiTraductorPMV.Dtos;
using WebApiTraductorPMV.Enums;
using WebApiTraductorPMV.Parameters;
using WebApiTraductorPMV.Services;

namespace WebApiTraductorPMV.Controllers;

/// <summary>
/// Endpoint para obtener el listado de graficos del panel
/// </summary>
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
//[Authorize(Roles = "admin")]
[Authorize(Policy = "RequireReaderRole")]
[Consumes("application/json")]
[Produces("application/json")]
public class GraphicsController : ControllerBase
{
    private readonly ILogger<GraphicsController> _log;
    private readonly IGraphicsService _graphicsService;

    public GraphicsController(ILogger<GraphicsController> log, IGraphicsService graphicsService)
    {
        _log = log;
        _graphicsService = graphicsService;
    }


    // GET: api/<GraphicController>
    /// <summary>
    /// Obtiene información de las imagenes almacenadas
    /// </summary>
    /// <param name="parameters">Objeto que permite hacer la paginación del listado de items</param>
    /// <returns>Retorna un objeto status con la información del panel si la respuesta es 200 OK</returns>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(PaginatorModel<List<Dtos.GraphicDTO>>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public IActionResult Get([FromQuery] QueryStringParameters parameters)
    {

        _log.LogDebug("Get ip:{ip}", parameters.IP);
        var (response, graphics) = _graphicsService.GetGraphics(parameters);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("error to get graphic:{response}", response);
            return BadRequest(new { message = response.ToString() });
        }

        var pagination = new PaginatorModel<List<Dtos.GraphicDTO>>();

        if (graphics != null)
        {
            pagination.PageSize = graphics.PageSize;
            pagination.Page = graphics.CurrentPage;
            pagination.TotalCount = graphics.TotalCount;
            pagination.Data = graphics;

            return Ok(pagination);
        }

        return Ok(pagination);
    }

    // GET api//192.168.0.5/1
    /// <summary>
    /// Obtiene informacion de un grafico especifico de la memoria del panel
    /// </summary>
    /// <param name="ip">Dirección ip del panel</param>
    /// <param name="id">Numero del grafico</param>
    /// <returns>Retorna el mensaje si el status es 200 OK</returns>
    [HttpGet("{ip}/{id}")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(GraphicDTO))]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public IActionResult Get(string ip, int id)
    {
        _log.LogDebug("ip:{ip}, id:{id}", ip, id);

        var (response, graphic) = _graphicsService.GetGraphic(ip, id);
        if (response != Enums.EnumAPIResponse.OK)
        {
            return BadRequest(new { message = response.ToString() });
        }

        return Ok(graphic);
    }

    // POST api/Messages
    ///// <summary>
    ///// Permite guardar una imagen en el panel
    ///// </summary>
    ///// <param name="ip">Dirección IP del panel</param>
    ///// <param name="value">Objeto con la información relevante para la creaoción del mensaje</param>
    ///// <returns>Retorna el status 200 Ok si el mensaje se guarda sin problemas</returns>
    //[HttpPost("{ip}")]
    //[SwaggerResponse(StatusCodes.Status200OK)]
    //[SwaggerResponse(StatusCodes.Status400BadRequest)]
    //[SwaggerResponse(StatusCodes.Status500InternalServerError)]
    //public IActionResult Post(string ip, [FromBody] Graphic value)
    //{
    //    var result = _graphicsService.SetGraphic(ip, value);
    //    if (result != Enums.EnumAPIResponse.OK)
    //    {
    //        return BadRequest(new { message = result.ToString() });
    //    }
    //    return Ok();
    //}
}