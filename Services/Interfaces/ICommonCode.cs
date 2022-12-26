using SnmpSharpNet;
using WebApiTraductorPMV.Dtos;
using WebApiTraductorPMV.Enums;

namespace WebApiTraductorPMV.Services
{
    public interface ICommonCode
    {
        (EnumAPIResponse response, SnmpV1Packet? snmpPacket) SNMPRequest(string ip, Pdu pdu);
        (EnumAPIResponse response, MessageDTO? message) FillMessage(string ip, int id, EnumMemoryType memoryType, bool fillDinamycMessage = false);
        EnumAPIResponse CheckMessageId(string ip, int id);
    }
}