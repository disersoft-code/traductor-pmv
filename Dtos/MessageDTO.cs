namespace WebApiTraductorPMV.Dtos;

/// <summary>
/// Clase que contiene la información de un mensaje guardado en un espacio de memoria del panel
/// </summary>
public class MessageDTO
{
    public MessageDTO()
    {

    }

    public MessageDTO(int messageNumber, string text)
    {
        MessageNumber = messageNumber;
        MultiString = text;
    }

    /// <summary>
    /// Numero de fila donde se ubica el mensaje, entre 1 y 255, numeral 5.6.8.2 NTCIP 1203 v03.05
    /// </summary>
    public int MessageNumber { get; set; }

    /// <summary>
    /// Mensaje en formato multi string, numeral 5.6.8.3 NTCIP 1203 v03.05
    /// </summary>
    public string? MultiString { get; set; }

    /// <summary>
    /// Texto con el que se nombra el mensaje, numeral 5.6.8.4 NTCIP 1203 v03.05
    /// </summary>
    public string? OwnerParameter { get; set; }

    /// <summary>
    /// CRC-16 creado a partir del MultiStrin + BeaconParameter + PixelServiceParameter, numeral 5.6.8.5 NTCIP 1203 v03.05
    /// </summary>
    public int CRCParameter { get; set; }

    /// <summary>
    /// Indica si el beacon esta activado (1), numeral  5.6.8.6 NTCIP 1203 v03.05
    /// </summary>
    public byte BeaconParameter { get; set; }

    /// <summary>
    ///  Indica si el pixel service esta activo (1), numeral 5.6.8.7 NTCIP 1203 v03.05
    /// </summary>
    public byte PixelServiceParameter { get; set; }

    /// <summary>
    /// Indica el run time priority asignado al mensaje, 1 el mas bajo, 255 el mas alto, numeral 5.6.8.8 NTCIP 1203 v03.05
    /// </summary>
    public byte RunTimePriorityParameter { get; set; }

    /// <summary>
    /// Indica el estado actual del mensaje, 1 - notUsed, 2 - modifying, 3 - validating, 4 - valid, 5 - error, 6 - modifyReq, 7 - validateReq, 8 - notUsedReq, numeral 5.6.8.9 NTCIP 1203 v03.05
    /// </summary>
    public byte StatusParameterNumber { get; set; }

    /// <summary>
    /// Indica el estado actual del mensaje en formato de texto
    /// </summary>
    public string StatusParameter { 
        get {
            return StatusParameterNumber switch
            {
                1 => "notused",
                2 => "modifying",
                3 => "validating",
                4 => "valid",
                5 => "error",
                6 => "modifyReq",
                7 => "validateReq",
                8 => "notUsedReq",
                _ => "unkwon",
            };
        } 
    }

    /// <summary>
    /// Indica el tipo de memoria utilizada para guardar el mensaje, 1 - other, 2 - permanent, 3 - changeable, 4 - volatile, 5 - currentBuffer, 6 - schedule, 7 - blank, numeral 5.6.8.1 NTCIP 1203 v03.05
    /// </summary>
    public byte MemoryTypeParameterNumber { get; set; }

    /// <summary>
    /// Indica el tipo de memoria utilizada para guardar el mensaje en formato de texto
    /// </summary>
    public string MemoryTypeParameter { 
        get {
            return MemoryTypeParameterNumber switch
            {
                1 => "other",
                2 => "permanent",
                3 => "changeable",
                4 => "volatile",
                5 => "currentBuffer",
                6 => "schedule",
                7 => "blank",
                _ => "unkwon",
            };
        }
    }

    public string? Message { get; set; }
    public DynamicMessageSign? DynamicMessage { get; set; }
    public bool IsActive { get; set; }


}

