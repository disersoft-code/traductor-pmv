using System.ComponentModel.DataAnnotations;

namespace WebApiTraductorPMV.Dtos;

/// <summary>
/// Clase que permite al usuario validar su email y contraseña
/// </summary>
public class UserLogin
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [MinLength(8)]
    public string? Password { get; set; }

}
