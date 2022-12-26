using WebApiTraductorPMV.Dtos;
using WebApiTraductorPMV.Enums;
using WebApiTraductorPMV.Parameters;

namespace WebApiTraductorPMV.Services
{
    public interface IGraphicsService
    {
        (EnumAPIResponse response, GraphicDTO? graphic) GetGraphic(string ip, int id);
        (EnumAPIResponse response, PagedList<GraphicDTO>? graphics) GetGraphics(QueryStringParameters parameters);
        EnumAPIResponse SetGraphic(string ip, Graphic trama);
    }
}