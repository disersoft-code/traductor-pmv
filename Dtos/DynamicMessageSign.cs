using System.ComponentModel.DataAnnotations;

namespace WebApiTraductorPMV.Dtos;

/// <summary>
/// Clase que permite guardar y/o activar un mensaje en el panel
/// </summary>
public class DynamicMessageSign
{
    public DynamicMessageSign()
    {
        Pages = new List<Page>();
    }
    /// <summary>
    /// Nombre que se le da al mensaje
    /// </summary>
    [Required]
    public string? MessageOwner { get; set; }

    /// <summary>
    /// Posición donde se va a guardar el mensaje (1 a 128)
    /// </summary>
    [Required]
    [Range(1, 512)]
    public int MessageNumber { get; set; }

    /// <summary>
    /// Array de mensajes que componen el objeto, si se utiliza no se debe llenar la propiedad MultiString
    /// </summary>
    public List<Page> Pages { get; set; }

    /// <summary>
    /// Si esta en true el mensaje se desplegara de forma inmediata, si es false solo guarda el mensaje
    /// </summary>
    [Required]
    public bool ActivateMessage { get; set; }

    /// <summary>
    /// Cadena de texto con el formato multistring que utiliza el panel para darle formato a los mensajes, si se utiliza no se debe llenar la propiedad Messages 
    /// </summary>
    public string? MultiString { get; set; }
}

/// <summary>
/// Clase que permite configurar paginas en un mensaje
/// </summary>
public class Page
{
    public Page()
    {
        Lines = new List<Line>();
    }
    /// <summary>
    /// Tiempo en pantalla del mensaje, decima parte de un segundo, 1 seg == 10, numeral 6.4.16 NTCIP 1203 v03.05
    /// </summary>
    [Required]
    public int PageTime { get; set; }

    /// <summary>
    /// Color de fondo de la pagina, array de enteros con el formato RGB, numeral 6.4.2 NTCIP 1203 v03.05
    /// </summary>
    [Required]
    public int[]? PageBackgroundColor { get; set; }

    /// <summary>
    /// Espacio entre caracteres numero entre 0 y 99, numeral 6.4.17 NTCIP 1203 v03.05
    /// </summary>
    [Required]
    public int SpacingCharacter { get; set; }

    /// <summary>
    /// Justificación de la pagina, 1 - other, 2 - top, 3 - middle, 4 - bottom, numeral 6.4.11 NTCIP 1203 v03.05
    /// </summary>
    [Required]
    public int JustificationPage { get; set; }

    /// <summary>
    /// Numero de la fuente a utilizar numero entre 1 y 255, numeral 6.4.7 NTCIP 1203 v03.05
    /// </summary>
    [Required] 
    public int Font { get; set; }

    /// <summary>
    /// Rectangulo donde se dibujara el texto, posee 4 enteros en el formato [x,y,w,h], numeral 6.4.18 NTCIP 1203 v03.05
    /// </summary>
    [Required] 
    public int[]? TextRectangle { get; set; }

    /// <summary>
    /// Color de la fuente, array de enteros con el formato RGB, numeral 6.4.3 NTCIP 1203 v03.05
    /// </summary>
    [Required] 
    public int[]? ColorForeground { get; set; }

    /// <summary>
    /// Listado de textos a mostrar
    /// </summary>
    [Required] 
    public List<Line> Lines { get; set; }

    /// <summary>
    /// Array que permite mostrar un grafico en el formato [n,x,y], numeral 6.4.8 NTCIP 1203 v03.05
    /// </summary>
    public int[]? Graphic { get; set; }
}

/// <summary>
/// Clase que permite configurar una pagina
/// </summary>
public class Line
{
    /// <summary>
    /// Nueva linea, define el espacio en pixeles entre 2 lineas, numeral 6.4.14 NTCIP 1203 v03.05
    /// </summary>
    [Required]
    public int NewLine { get; set; }

    /// <summary>
    /// Texto a mostrar
    /// </summary>
    [Required]
    public string? Text { get; set; }

    /// <summary>
    /// Justificación de linea, 1 - other, 2 - left, 3 - center, 4 - right, 5 - full
    /// </summary>
    [Required]
    public int JustificationLine { get; set; }
}

