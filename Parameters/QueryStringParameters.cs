namespace WebApiTraductorPMV.Parameters;

public class QueryStringParameters
{
    const int _maxPageSize = 1000;

    /// <summary>
    /// Numero de la pagina a mostrar, empieza desde 0
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// IP del panel
    /// </summary>
    public string IP { get; set; } = "";

    private int _pageSize = 10;

    /// <summary>
    /// Tamaño de la paginación, por defecto 10
    /// </summary>
    public int PageSize
    {
        get
        {
            return _pageSize;
        }
        set
        {
            _pageSize = (value > _maxPageSize) ? _maxPageSize : value;
        }
    }
}
