using WebApiTraductorPMV.Dtos;
using WebApiTraductorPMV.Enums;

namespace WebApiTraductorPMV.Services
{
    public interface IStatusService
    {
        (EnumAPIResponse response, StatusPanelDTO? status) GetStatus(string ip);

        EnumAPIResponse RestartPanel(string ip);
    }
}