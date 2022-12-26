using WebApiTraductorPMV.Dtos;
using WebApiTraductorPMV.Enums;
using WebApiTraductorPMV.Parameters;

namespace WebApiTraductorPMV.Services
{
    public interface ISchedulesService
    {
        (EnumAPIResponse response, PagedList<ScheduleMessageDTO>? messages) GetScheduleMessages(QueryStringParameters parameters);
        EnumAPIResponse SetScheduleMessage(string ip, ScheduleAddMessageModel trama);
        (EnumAPIResponse response, ScheduleMessageDTO? message) GetScheduleMessage(string ip, string id);
        EnumAPIResponse UpdateScheduleMessage(string ip, ScheduleEditMessageModel trama);
        EnumAPIResponse DeleteScheduleMessage(string ip, string id);
    }
}