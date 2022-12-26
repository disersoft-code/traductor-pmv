namespace WebApiTraductorPMV.Dtos;

public class PaginatorModel<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public T? Data { get; set; }

}
