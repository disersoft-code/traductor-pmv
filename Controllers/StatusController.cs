using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApiTraductorPMV.Dtos;
using WebApiTraductorPMV.Enums;
using WebApiTraductorPMV.Services;

namespace WebApiTraductorPMV.Controllers;

/// <summary>
/// Endpoint para obtener información del panel
/// </summary>
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
//[Authorize(Roles = "admin")]
[Authorize(Policy = "RequireReaderRole")]
[Consumes("application/json")]
[Produces("application/json")]
public class StatusController : ControllerBase
{
    private readonly ILogger<StatusController> _log;
    private readonly IStatusService _statusService;

    public StatusController(ILogger<StatusController> log, IStatusService statusService)
    {
        _log = log;
        _statusService = statusService;
    }


    // GET: api/Status
    /// <summary>
    /// Obtiene información relevante del funcionamiento del panel
    /// </summary>
    /// <param name="ip">Dirección IP del panel</param>
    /// <returns>Retorna un objeto status con la información del panel si la respuesta es 200 OK</returns>
    [HttpGet("{ip}")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(Dtos.StatusPanelDTO))]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public IActionResult Get(string ip)
    {
        _log.LogDebug("Get ip:{ip}", ip);
        var (response, status) = _statusService.GetStatus(ip);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("error to get status:{response}", response);
            return BadRequest(new { message = response.ToString() });
        }

        _log.LogDebug("get status ok");
        return Ok(status);
    }

    // POST api/Status
    /// <summary>
    /// Permite resetear el hardware del panel
    /// </summary>
    /// <param name="ip">Dirección IP del panel</param>
    /// <returns>Retorna el status 200 Ok si el mensaje se edita sin problemas</returns>
    [HttpPost("{ip}")]
    [Authorize(Policy = "RequireWriteRole")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public IActionResult Post(string ip)
    {

        var result = _statusService.RestartPanel(ip);
        if (result != Enums.EnumAPIResponse.OK)
        {
            return BadRequest(new { message = result.ToString() });
        }
        return Ok();
    }


}