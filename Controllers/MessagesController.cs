using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApiTraductorPMV.Dtos;
using WebApiTraductorPMV.Enums;
using WebApiTraductorPMV.Parameters;
using WebApiTraductorPMV.Services;

namespace WebApiTraductorPMV.Controllers;

/// <summary>
/// Endpoint para manipular los mensajes guardados en el panel
/// </summary>
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
//[Authorize(Roles = "admin")]
//[Authorize(Roles = "admin")]
[Authorize(Policy = "RequireReaderRole")]
[Authorize(Policy = "RequireWriteRole")]
[Consumes("application/json")]
[Produces("application/json")]
public class MessagesController : ControllerBase
{
    private readonly ILogger<MessagesController> _log;
    private readonly IConfiguration _config;
    private readonly IMessagesService _messagesService;

    public MessagesController(ILogger<MessagesController> log, IConfiguration config, IMessagesService messagesService)
    {
        _log = log;
        _config = config;
        _messagesService = messagesService;
    }


    // GET: api/Messages/192.168.0.5
    /// <summary>
    /// Obtiene la lista de mensajes guardados en la memoria del panel 
    /// </summary>
    /// <param name="parameters">Datos de consulta como lo son la ip y la paginación de los mensajes</param>
    /// <returns>Retorna una lista de mensajes si el status es 200 OK</returns>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(PaginatorModel<List<MessageDTO>>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public IActionResult Get([FromQuery] QueryStringParameters parameters)
    {
        var nameApp = _config.GetValue<string>("NameApp");
        _log.LogDebug("NameApp:{NameApp}, ip:{ip}", nameApp, parameters.IP);

        var (response, messages) = _messagesService.GetMessages(parameters);
        if (response != Enums.EnumAPIResponse.OK)
        {
            return BadRequest(new { message = response.ToString() });
        }

        var pagination = new PaginatorModel<List<MessageDTO>>();

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

    // GET api/Messages/192.168.0.5/1
    /// <summary>
    /// Obtiene un mensaje especifico de la memoria del panel
    /// </summary>
    /// <param name="ip">Dirección ip del panel</param>
    /// <param name="id">Numero del mensaje</param>
    /// <returns>Retorna el mensaje si el status es 200 OK</returns>
    [HttpGet("{ip}/{id}")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(MessageDTO))]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public IActionResult Get(string ip, int id)
    {
        _log.LogDebug("ip:{ip}, id:{id}", ip, id);

        var (response, message) = _messagesService.GetMessage(ip, id, EnumMemoryType.Changeable);
        if (response != Enums.EnumAPIResponse.OK)
        {
            return BadRequest(new { message = response.ToString() });
        }

        return Ok(message);
    }

    // POST api/Messages
    /// <summary>
    /// Permite editar un mensaje en el panel, los paneles tienen espacios de memoria de 250 a 512
    /// </summary>
    /// <param name="ip">Dirección IP del panel</param>
    /// <param name="value">Objeto con la información relevante para la creaoción del mensaje</param>
    /// <returns>Retorna el status 200 Ok si el mensaje se edita sin problemas</returns>
    [HttpPost("{ip}")]
    [Authorize(Policy = "RequireWriteRole")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public IActionResult Post(string ip, [FromBody] DynamicMessageSign value)
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

        if (value.Pages == null && value.MultiString == null)
        {
            _log.LogError("error, model is not valid, no message");
            return BadRequest(new { message = EnumAPIResponse.ERROR_INVALID_MODEL.ToString() });
        }



        var result = _messagesService.SetMessage(ip, value);
        if (result != Enums.EnumAPIResponse.OK)
        {
            return BadRequest(new { message = result.ToString() });
        }
        return Ok();
    }


    /// <summary>
    /// Permite vaciar un mensaje, los mensajes no se pueden borrar.
    /// </summary>
    /// <param name="ip">Dirección IP del panel</param>
    /// <param name="id">Numero de la posición del mensaje</param>
    /// <returns>Retorna el status 200 Ok si el mensaje se vacia sin problemas</returns>
    [HttpDelete("{ip}/{id}")]
    [Authorize(Policy = "RequireWriteRole")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public IActionResult Delete(string ip, int id)
    {
        _log.LogDebug("ip:{ip}, id:{id}", ip, id);

        var response = _messagesService.DeleteMessage(ip, id);
        if (response != Enums.EnumAPIResponse.OK)
        {
            return BadRequest(new { message = response.ToString() });
        }

        return Ok();
    }

    /// <summary>
    /// Permite activar o desactivar (mostrar o quitar) el mensaje del panel
    /// </summary>
    /// <param name="ip">Dirección IP del panel</param>
    /// <param name="value">Objeto con la información necesaria para activar o desactivar el mensaje</param>
    /// <returns>Retorna el status 200 Ok si el mensaje se activa sin problemas</returns>
    [HttpPut("{ip}")]
    [Authorize(Policy = "RequireWriteRole")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public IActionResult Put(string ip, [FromBody] ActivateMessage value)
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

        var result = _messagesService.ActivateMessage(ip, value);
        if (result != Enums.EnumAPIResponse.OK)
        {
            return BadRequest(new { message = result.ToString() });
        }
        return Ok();
    }



}
