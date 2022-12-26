using System.ComponentModel.DataAnnotations;
using WebApiTraductorPMV.Utils;

namespace WebApiTraductorPMV.Dtos;

public class PanelRequest
{
    [IpAddress]
    [Required]
    public string? IP { get; set; }
    public int NumeroDeMensaje { get; set; }
}
