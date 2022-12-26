namespace WebApiTraductorPMV.Dtos;

/// <summary>
/// Clase que permite al usuario obtener el token para acceder a los diferentes endpoints del web api
/// </summary>
public class UserTokenDTO
{
    /// <summary>
    /// Texto que permite al usuario consumir los diferentes endpoints
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Fecha la cual el token expira
    /// </summary>
    public DateTime Expiration { get; set; }

    public UserTokenDTO()
    {

    }

    public UserTokenDTO(string token, DateTime expiration)
    {
        Token = token;
        Expiration = expiration;
    }
}
