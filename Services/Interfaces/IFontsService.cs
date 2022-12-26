using WebApiTraductorPMV.Dtos;
using WebApiTraductorPMV.Enums;
using WebApiTraductorPMV.Parameters;

namespace WebApiTraductorPMV.Services
{
    public interface IFontsService
    {
        (EnumAPIResponse response, Font? font) GetFont(string ip, int id);
        (EnumAPIResponse response, PagedList<Font>? fonts) GetFonts(QueryStringParameters parameters);
    }
}