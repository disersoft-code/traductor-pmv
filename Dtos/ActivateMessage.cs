using System.ComponentModel.DataAnnotations;

namespace WebApiTraductorPMV.Dtos;

public class ActivateMessage
{
    /// <summary>
    /// Posición donde se va a guardar el mensaje (1 a 128)
    /// </summary>
    [Required]
    [Range(1, 512)]
    public int MessageNumber { get; set; }

    /// <summary>
    /// Si esta en true el mensaje se desplegara de forma inmediata, si es false solo guarda el mensaje
    /// </summary>
    [Required]
    public bool Activate { get; set; }

}
