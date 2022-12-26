namespace WebApiTraductorPMV.Dtos;

/// <summary>
/// Permite obtener informacion de las fuentes
/// </summary>
public class FontDTO
{
    /// <summary>
    /// Indica el número máximo de fuentes que puede almacenar el letrero, numeral 5.4.1 NTCIP 1203 v03.05
    /// </summary>
    public int numberOfFonts { get; set; }

    /// <summary>
    /// Indica el número máximo de caracteres que se puede almacenar en cada fuente, numeral 5.4.3 NTCIP 1203 v03.05
    /// </summary>
    public int maximumCharactersPerFont { get; set; }

    /// <summary>
    /// Indica el número de fuente predeterminado para un mensaje, numeral 5.5.7 NTCIP 1203 v03.05
    /// </summary>
    public int defaultFontParameter { get; set; }

    public List<Font>? Fonts { get; set; }

}


public class Font
{
    public int Index { get; set; }
    public int Number { get; set; }
    public string? Name { get; set; }
    public int Height { get; set; }
    public string? VersionID { get; set; }
    public int Status { get; set; }
}

