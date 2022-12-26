using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApiTraductorPMV.Dtos;
using WebApiTraductorPMV.Enums;
using WebApiTraductorPMV.Parameters;
using WebApiTraductorPMV.Services;

namespace WebApiTraductorPMV.Controllers;

/// <summary>
/// Endpoint para obtener el listado de fuentes que tiene el panel
/// </summary>
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
//[Authorize(Roles = "admin")]
[Authorize(Policy = "RequireReaderRole")]
[Consumes("application/json")]
[Produces("application/json")]
public class FontsController : ControllerBase
{
    private readonly ILogger<FontsController> _log;
    private readonly IFontsService _fontsService;

    public FontsController(ILogger<FontsController> log, IFontsService fontsService)
    {
        _log = log;
        _fontsService = fontsService;
    }


    // GET: api/Fonts
    /// <summary>
    /// Obtiene información de las fuentes almacenadas en el panel
    /// </summary>
    /// <param name="parameters">Objeto que permite hacer la paginación del listado de items</param>
    /// <returns>Retorna un objeto status con la información del panel si la respuesta es 200 OK</returns>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(PaginatorModel<List<Dtos.Font>>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public IActionResult Get([FromQuery] QueryStringParameters parameters)
    {
        _log.LogDebug("Get ip:{ip}", parameters.IP);
        var (response, fonts) = _fontsService.GetFonts(parameters);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("error to get font:{response}", response);
            return BadRequest(new { message = response.ToString() });
        }

        var pagination = new PaginatorModel<List<Dtos.Font>>();

        if (fonts != null)
        {
            pagination.PageSize = fonts.PageSize;
            pagination.Page = fonts.CurrentPage;
            pagination.TotalCount = fonts.TotalCount;
            pagination.Data = fonts;

            return Ok(pagination);
        }

        return Ok(pagination);
    }

    // GET api/Messages/192.168.0.5/1
    /// <summary>
    /// Obtiene una fuente especifica de la memoria del panel
    /// </summary>
    /// <param name="ip">Dirección ip del panel</param>
    /// <param name="id">Numero de fuente</param>
    /// <returns>Retorna el mensaje si el status es 200 OK</returns>
    [HttpGet("{ip}/{id}")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(Font))]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public IActionResult Get(string ip, int id)
    {
        _log.LogDebug("ip:{ip}, id:{id}", ip, id);

        var (response, font) = _fontsService.GetFont(ip, id);
        if (response != Enums.EnumAPIResponse.OK)
        {
            return BadRequest(new { message = response.ToString() });
        }

        return Ok(font);
    }

}
