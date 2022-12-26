namespace WebApiTraductorPMV.Dtos;

/// <summary>
/// Clase que permite obtener el estado general del panel
/// </summary>
public class StatusPanelDTO
{
    /// <summary>
    /// Fecha en formato Epoch timestamp (numero de segundos desde 1970) en formato GTM
    /// </summary>
    public int CurrentDate { get; set; }

    public string? CurrentDateGTM { get; set; }

    public string? LocalTime { get; set; }

    /// <summary>
    /// Indica el numero de filas de pixeles para el letrero, numeral 5.3.3 NTCIP 1203 v03.05
    /// </summary>
    public int SignHeightInPixels { get; set; }

    /// <summary>
    /// Indica el numero de columnas de pixeles para el letrero, numeral 5.3.4 NTCIP 1203 v03.05
    /// </summary>
    public int SignWidthInPixels { get; set; }

    /// <summary>
    /// Indica el numero maximo de fuentes que el letrero puede almacenar, numeral 5.4.1 NTCIP 1203 v03.05
    /// </summary>
    public int NumberFonts { get; set; }

    /// <summary>
    /// Indica el numero maximo de paginas permitidas en el multi string, numeral 5.5.24 NTCIP 1203 v03.05
    /// </summary>
    public int MaximumNumberPages { get; set; }

    /// <summary>
    /// Indica el numero maximo de bytes permitidos en el multi string, numeral 5.5.25 NTCIP 1203 v03.05
    /// </summary>
    public int MaximumMultiStringLength { get; set; }

    /// <summary>
    /// Indica el numero actual de mensajes guardados en una memoria no volatil, no modificable, numeral 5.6.1 NTCIP 1203 v03.05 
    /// </summary>
    public int NumberPermanentMessages { get; set; }

    /// <summary>
    /// Indica el numero actual de mensajes validos guardados en una memoria no volatil, modificable, numeral 5.6.2 NTCIP 1203 v03.05 
    /// </summary>
    public int NumberChangeableMessages { get; set; }

    /// <summary>
    /// Indica el numero maximo de mensajes que se pueden guardar en una memoria no volatil, modificable, numeral 5.6.3 NTCIP 1203 v03.05 
    /// </summary>
    public int MaximumNumberChangeableMessages { get; set; }

    /// <summary>
    /// Indica el numero maximo de graficos que el letrero puede almacenar, numeral 5.12.1 NTCIP 1203 v03.05
    /// </summary>
    public int MaximumNumberGraphics { get; set; }

    /// <summary>
    /// Indica el numero de graficos que actualmente estan almacenados, numeral 5.12.2 NTCIP 1203 v03.05
    /// </summary>
    public int NumberGraphics { get; set; }

    /// <summary>
    /// Indica el metodo usado para seleccionar el nivel del brillo, numeral 5.8.1 NTCIP 1203 v03.05
    /// </summary>
    public int IlluminationControlNumber { get; set; }

    /// <summary>
    /// Indica el nivel actual de brillo del dispositivo, numeral 5.8.5 NTCIP 1203 v03.05
    /// </summary>
    public int StatusIlluminationBrightnessLevel { get; set; }

    /// <summary>
    /// Indica el valor deseado el nivel de brillo como un valor, numeral 5.8.6 NTCIP 1203 v03.05
    /// </summary>
    public int IlluminationManualLevel { get; set; }

    /// <summary>
    /// Indica el metodo usado para seleccionar el nivel del brillo en texto, numeral 5.8.1 NTCIP 1203 v03.05
    /// </summary>
    public string IlluminationControl
    {
        get
        {
            return IlluminationControlNumber switch
            {
                1 => "other",
                2 => "photocell",
                3 => "timer",
                4 => "manual",
                5 => "manualDirect",
                6 => "manualIndexed",
                _ => "unkwon",
            };

        }
    }

    /// <summary>
    /// Mensaje que esta actualmente activo
    /// </summary>
    public MessageDTO? CurrentMessage { get; set; }

    

}
