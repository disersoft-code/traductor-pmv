using System.ComponentModel.DataAnnotations;

namespace WebApiTraductorPMV.Dtos;
public class Graphic
{
    [Required]
    public int? Number { get; set; }

    public string? Name { get; set; }

    public int? Height { get; set; }

    public int? Width { get; set; }

    public int? Type { get; set; }

    public int? TransparentEnabled { get; set; }

    public string? TransparentColor { get; set; }

    public string? BMP { get; set; }
}

