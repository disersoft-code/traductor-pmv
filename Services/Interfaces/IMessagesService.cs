using WebApiTraductorPMV.Dtos;
using WebApiTraductorPMV.Enums;
using WebApiTraductorPMV.Parameters;

namespace WebApiTraductorPMV.Services;

public interface IMessagesService
{
    (EnumAPIResponse response, PagedList<MessageDTO>? messages) GetMessages(QueryStringParameters parameters);
    EnumAPIResponse SetMessage(string ip, DynamicMessageSign trama);
    (EnumAPIResponse response, MessageDTO? message) GetMessage(string ip, int id, EnumMemoryType memoryType);
    EnumAPIResponse DeleteMessage(string ip, int id);
    EnumAPIResponse ActivateMessage(string ip, ActivateMessage trama);
}