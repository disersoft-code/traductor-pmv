using System.ComponentModel.DataAnnotations;

namespace WebApiTraductorPMV.Dtos;

/// <summary>
/// Clase que permite guardar un mensaje con horario
/// </summary>
public class ScheduleAddMessageModel
{
    /// <summary>
    /// Fecha en formato Epoch timestamp (numero de segundos desde 1970) en formato GTM
    /// </summary>
    [Required]
    public UInt32 Date { get; set; }

    /// <summary>
    /// Numero de fila donde se ubica el mensaje, entre 1 y 255, numeral 5.6.8.2 NTCIP 1203 v03.05
    /// </summary>
    [Required]
    public int MessageNumber { get; set; }
}

