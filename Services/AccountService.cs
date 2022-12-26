using WebApiTraductorPMV.Controllers;
using WebApiTraductorPMV.Dtos;
using WebApiTraductorPMV.Utils;

namespace WebApiTraductorPMV.Services;

/// <summary>
/// Clase que permite verificar si un usuario puede consumir el API Rest
/// </summary>
public class AccountService : IAccountService
{
    private readonly IConfiguration _conf;
    private readonly ILogger<AccountController> _log;

    public AccountService(IConfiguration conf, ILogger<AccountController> log)
    {
        _conf = conf;
        _log = log;
    }

    /// <summary>
    /// Permite verificar si un usuario existe y puede utilizar el API Rest
    /// </summary>
    /// <param name="model">Clase que contiene el email y contraseña del usuario</param>
    /// <returns></returns>
    public UserTokenDTO? Login(UserLogin model)
    {
        _log.LogDebug("login {email}", model.Email);

        var users = _conf.GetSection("ApiUsers").Get<UserModel[]>();
        if (users != null)
        {
            var account = users.Where(x => x.Email == model.Email).FirstOrDefault();
            //var aux = BCrypt.Net.BCrypt.HashPassword(model.Password);
            if (account != null && BCrypt.Net.BCrypt.Verify(model.Password, account.Password))
            {
                if (account.Roles != null)
                {
                    return JwtConfigurator.BuildToken(account, account.Roles, _conf);
                }
            }
            else
            {
                _log.LogWarning("account not found or wrong password");
            }
        }
        else
        {
            _log.LogError("ApiUsers is null");
        }

        return default;
    }

}
