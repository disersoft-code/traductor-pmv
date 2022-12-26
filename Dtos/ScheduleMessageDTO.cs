namespace WebApiTraductorPMV.Dtos;

/// <summary>
/// Clase que contiene la información de los mensajes con horario que tiene el panel
/// </summary>
public class ScheduleMessageDTO
{
    /// <summary>
    /// Consecutivo utilizado para el listado de mensajes con horario
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Id unico que identifica el mensaje con horario
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Fecha en formato Epoch timestamp (numero de segundos desde 1970) en formato GTM
    /// </summary>
    public UInt32 Date { get; set; }

    public string? DateGMT { get; set; }
    public string? LocalTime { get; set; }

    /// <summary>
    /// Numero entre 1 y 255, numeral 2.4.4.3.1 NTCIP 1201 v03.15
    /// </summary>
    public int DayPlanNumber { get; set; }

    /// <summary>
    /// Numero entre 1 y 255, numeral 2.4.4.3.2 NTCIP 1201 v03.15
    /// </summary>
    public int DayPlanEventNumber { get; set; }

    /// <summary>
    /// Numero de fila donde se ubica el mensaje, entre 1 y 255, numeral 5.6.8.2 NTCIP 1203 v03.05
    /// </summary>
    public int MessageNumber { get; set; }

    /// <summary>
    /// Numero entre 1 y 255, numeral 5.9.2.1 NTCIP 1203 v03.05
    /// </summary>
    public int ActionIndex { get; set; }

    /// <summary>
    /// Numero entre 1 y 65535, numeral 2.4.3.2.1 NTCIP 1201 v03.15
    /// </summary>
    public int TimeBaseScheduleNumber { get; set; }

    /// <summary>
    /// Mensaje asociado al horario
    /// </summary>
    public MessageDTO? MessageObject { get; set; }

    public string? Message { get; set; }



}
