using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApiTraductorPMV.Dtos;
using WebApiTraductorPMV.Enums;
using WebApiTraductorPMV.Parameters;
using WebApiTraductorPMV.Services;

namespace WebApiTraductorPMV.Controllers;

/// <summary>
/// Endpoint para manipular los mensajes con horarios guardados en el panel
/// </summary>
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
//[Authorize(Roles = "admin")]
[Authorize(Policy = "RequireReaderRole")]
[Authorize(Policy = "RequireWriteRole")]
[Consumes("application/json")]
[Produces("application/json")]
public class SchedulesController : ControllerBase
{
    private readonly ILogger<SchedulesController> _log;
    private readonly ISchedulesService _schedulesService;

    public SchedulesController(ILogger<SchedulesController> log, ISchedulesService schedulesService)
    {
        _log = log;
        _schedulesService = schedulesService;
    }

    // GET: api/Schedules/192.168.0.5
    /// <summary>
    /// Obtiene la lista de mensajes con horario guardados en la memoria del panel
    /// </summary>
    /// <param name="parameters">Objeto que contiene las variables de la paginación</param>
    /// <returns>Retorna una lista de mensajes si el status es 200 OK</returns>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(PaginatorModel<List<ScheduleMessageDTO>>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public IActionResult Get([FromQuery] QueryStringParameters parameters)
    {
        _log.LogDebug("ip:{ip}", parameters.IP);

        var (response, messages) = _schedulesService.GetScheduleMessages(parameters);
        if (response != Enums.EnumAPIResponse.OK)
        {
            return BadRequest(new { message = response.ToString() });
        }

        var pagination = new PaginatorModel<List<ScheduleMessageDTO>>();

        if (messages != null)
        {
            pagination.PageSize = messages.PageSize;
            pagination.Page = messages.CurrentPage;
            pagination.TotalCount = messages.TotalCount;
            pagination.Data = messages;

            return Ok(pagination);
        }

        return Ok(pagination);
    }

    /// <summary>
    /// Obtiene un mensaje especifico de la memoria del panel
    /// </summary>
    /// <param name="ip">Dirección IP del panel</param>
    /// <param name="id">Id unico que identifica el mensaje programado</param>
    /// <returns>Retorna un objeto con la información del mensaje programado</returns>
    [HttpGet("{ip}/{id}")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ScheduleMessageDTO))]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public IActionResult Get(string ip, string id)
    {
        _log.LogDebug("ip:{ip}, id:{id}", ip, id);

        var (response, message) = _schedulesService.GetScheduleMessage(ip, id);
        if (response != Enums.EnumAPIResponse.OK)
        {
            return BadRequest(new { message = response.ToString() });
        }

        return Ok(message);
    }


    // POST api/Schedules
    /// <summary>
    /// Permite guardar un mensaje con horario en el panel
    /// </summary>
    /// <param name="ip">Dirección IP del panel</param>
    /// <param name="value">Objeto con la información relevante para la creación del mensaje</param>
    /// <returns>Retorna el status 200 Ok si el mensaje se guarda sin problemas</returns>
    [HttpPost("{ip}")]
    [Authorize(Policy = "RequireWriteRole")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public IActionResult Post(string ip, [FromBody] ScheduleAddMessageModel value)
    {
        if (value == null)
        {
            _log.LogError("error, value is null");
            return BadRequest(new { message = EnumAPIResponse.ERROR_WRONG_DATA.ToString() });
        }
        if (!ModelState.IsValid)
        {
            _log.LogError("error, model is not valid");
            return BadRequest(new { message = EnumAPIResponse.ERROR_INVALID_MODEL.ToString() });
        }

        var result = _schedulesService.SetScheduleMessage(ip, value);
        if (result != Enums.EnumAPIResponse.OK)
        {
            return BadRequest(new { message = result.ToString() });
        }
        return Ok();
    }

    /// <summary>
    /// Permite editar un mensaje con horario de la memoria del panel
    /// </summary>
    /// <param name="ip">Dirección IP del panel</param>
    /// <param name="value">Objeto con la información necesaria para editar el horario del mensaje</param>
    /// <returns>Retorna el status 200 Ok si el mensaje se guarda sin problemas</returns>
    [HttpPut("{ip}")]
    [Authorize(Policy = "RequireWriteRole")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public IActionResult Put(string ip, [FromBody] ScheduleEditMessageModel value)
    {
        if (value == null)
        {
            _log.LogError("error, value is null");
            return BadRequest(new { message = EnumAPIResponse.ERROR_WRONG_DATA.ToString() });
        }
        if (!ModelState.IsValid)
        {
            _log.LogError("error, model is not valid");
            return BadRequest(new { message = EnumAPIResponse.ERROR_INVALID_MODEL.ToString() });
        }

        var result = _schedulesService.UpdateScheduleMessage(ip, value);
        if (result != Enums.EnumAPIResponse.OK)
        {
            return BadRequest(new { message = result.ToString() });
        }
        return Ok();
    }


    /// <summary>
    /// Permite eliminar un mensaje con horario, solo borra el horario no el mensaje
    /// </summary>
    /// <param name="ip">Dirección IP del panel</param>
    /// <param name="id">Id unico que identifica el mensaje con horario</param>
    /// <returns>Retorna el status 200 Ok si el mensaje se guarda sin problemas</returns>
    [HttpDelete("{ip}/{id}")]
    [Authorize(Policy = "RequireWriteRole")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public IActionResult Delete(string ip, string id)
    {
        var result = _schedulesService.DeleteScheduleMessage(ip, id);
        if (result != Enums.EnumAPIResponse.OK)
        {
            return BadRequest(new { message = result.ToString() });
        }
        return Ok();
    }

}
