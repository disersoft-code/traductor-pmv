using WebApiTraductorPMV.Dtos;
using WebApiTraductorPMV.Enums;
using WebApiTraductorPMV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApiTraductorPMV.Controllers;

/// <summary>
/// Endpoint para validar y obtener el token de acceso al resto de endpoints
/// </summary>
[Route("api/[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _log;
    private readonly IAccountService _accountService;

    public AccountController(ILogger<AccountController> log, IAccountService accountService)
    {
        _log = log;
        _accountService = accountService;
    }

    /// <summary>
    /// Permite validar un usuario para obtener el token de acceso
    /// </summary>
    /// <param name="user">Objeto que contiene el email y la contraseña del usuario</param>
    /// <returns>Retorna un objeto token si el status es 200 Ok</returns>
    [HttpPost]
    [AllowAnonymous]
    [Route("login")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(UserTokenDTO))]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public IActionResult Login([FromBody] UserLogin user)
    {
        if (user == null)
        {
            _log.LogError("error, user is null");
            return BadRequest(new { message = EnumAPIResponse.ERROR_WRONG_DATA.ToString() });
        }
        if (!ModelState.IsValid)
        {
            _log.LogError("error, model is not valid");
            return BadRequest(new { message = EnumAPIResponse.ERROR_INVALID_MODEL.ToString() });
        }

        var token = _accountService.Login(user);
        if (token == null)
        {
            _log.LogWarning("error, invalid login attempt");
            return BadRequest(new { message = EnumAPIResponse.INAVLID_LOGIN_ATTEMPT.ToString() });
        }

        _log.LogInformation("login ok");
        return Ok(token);
    }
}
