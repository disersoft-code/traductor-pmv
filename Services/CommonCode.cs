using SnmpSharpNet;
using System.Net;
using WebApiTraductorPMV.Dtos;
using WebApiTraductorPMV.Enums;
using WebApiTraductorPMV.Utils;

namespace WebApiTraductorPMV.Services
{
    public class CommonCode : ICommonCode
    {
        private readonly ILogger<CommonCode> _log;

        public CommonCode(ILogger<CommonCode> log)
        {
            _log = log;
        }


        /// <summary>
        /// Funcion que permite solcitar información a un ip por medio del protocolo SNMP
        /// </summary>
        /// <param name="ip">IP del panel led</param>
        /// <param name="pdu">Objeto que contiene los datos solicitados al panel</param>
        /// <returns>Retorna una dupla de valores, el primero una enumeración donde OK es exitoso y diferente a eso es un error, la segunda el objeto con lo solicitado</returns>
        public (EnumAPIResponse response, SnmpV1Packet? snmpPacket) SNMPRequest(string ip, Pdu pdu)
        {
            try
            {
                var agent = new IpAddress(ip);
                var target = new UdpTarget((IPAddress)agent, 161, 9520, 1);
                var community = new OctetString("public");
                var param = new AgentParameters(community)
                {
                    Version = SnmpVersion.Ver1
                };

                var result = (SnmpV1Packet)target.Request(pdu, param);
                if (result == null)
                {
                    _log.LogError("No response received from SNMP agent.");
                    return (EnumAPIResponse.NO_RESPONSE_RECEIVED_FROM_SNMP_AGENT, null);
                }

                if (result.Pdu.ErrorStatus != 0)
                {

                    _log.LogError("Error in SNMP reply. Error {status} index {index}", result.Pdu.ErrorStatus, result.Pdu.ErrorIndex);
                    return (EnumAPIResponse.ERROR_IN_SNMP_REPLY, null);
                }

                target.Close();

                return (EnumAPIResponse.OK, result);


            }
            catch (SnmpNetworkException ex)
            {
                _log.LogError(ex, "Error SnmpNetworkException");
                return (EnumAPIResponse.NETWORK_EXCEPTION, null);
            }
            catch (SnmpException ex)
            {
                _log.LogError(ex, "Error SnmpException");
                if (ex.ErrorCode == 13)
                {
                    return (EnumAPIResponse.NETWORK_EXCEPTION, null);
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error Exception");
            }

            return (EnumAPIResponse.EXCEPTION, null);
        }

        /// <summary>
        /// Funcion que permite obtener un mensaje especifico del panel
        /// </summary>
        /// <param name="ip">IP del panel led</param>
        /// <param name="id">Numero del mensaje</param>
        /// <param name="memoryType">Tipo de memoria donde esta el mensaje</param>
        /// <param name="fillDinamycMessage"></param>
        /// <returns>Retorna una dupla de valores, el primero una enumeración donde OK es exitoso y diferente a eso es un error, la segunda el objeto con lo solicitado</returns>
        public (EnumAPIResponse response, MessageDTO? message) FillMessage(string ip, int id, EnumMemoryType memoryType, bool fillDinamycMessage = false)
        {
            _log.LogDebug("FillMessage, ip:{ip}, id:{id}, memoryType:{memoryType}", ip, id, memoryType);

            var result = CheckMessageId(ip, id);
            if (result != EnumAPIResponse.OK)
            {
                _log.LogError("error message wrong number:{id}", id);
                return (result, null);
            }

            var pdu = new Pdu(PduType.Get);
            //Message MULTI String Parameter
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.5.8.1.3.{(int)memoryType}.{id}");
            //Message Owner Parameter
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.5.8.1.4.{(int)memoryType}.{id}");
            //Message CRC Parameter
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.5.8.1.5.{(int)memoryType}.{id}");
            //Message Beacon Parameter
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.5.8.1.6.{(int)memoryType}.{id}");
            //Message Pixel Service Parameter
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.5.8.1.7.{(int)memoryType}.{id}");
            //Message Run Time Priority Parameter
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.5.8.1.8.{(int)memoryType}.{id}");
            //Message Status Parameter
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.5.8.1.9.{(int)memoryType}.{id}");
            //Message Memory Type Parameter
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.5.8.1.1.{(int)memoryType}.{id}");

            var (response, snmpPacket) = SNMPRequest(ip, pdu);

            if (response != EnumAPIResponse.OK)
            {
                return (response, null);
            }

            _log.LogDebug("Message index:{index}, text:{text}", id, snmpPacket?.Pdu.VbList[0].Value.ToString());

            var msg = new MessageDTO
            {
                MessageNumber = id,
                MultiString = snmpPacket?.Pdu.VbList[0].Value.ToString(),
                OwnerParameter = snmpPacket?.Pdu.VbList[1].Value.ToString(),
                CRCParameter = Convert.ToInt32(snmpPacket?.Pdu.VbList[2].Value.ToString()),
                BeaconParameter = Convert.ToByte(snmpPacket?.Pdu.VbList[3].Value.ToString()),
                PixelServiceParameter = Convert.ToByte(snmpPacket?.Pdu.VbList[4].Value.ToString()),
                RunTimePriorityParameter = Convert.ToByte(snmpPacket?.Pdu.VbList[5].Value.ToString()),
                StatusParameterNumber = Convert.ToByte(snmpPacket?.Pdu.VbList[6].Value.ToString()),
                MemoryTypeParameterNumber = Convert.ToByte(snmpPacket?.Pdu.VbList[7].Value.ToString()),
                Message = Global.MultiStringToString(snmpPacket?.Pdu.VbList[0].Value.ToString())

            };

            if (fillDinamycMessage)
            {
                msg.DynamicMessage = Global.MultiStringToDynamicMessageSign(snmpPacket?.Pdu.VbList[0].Value.ToString());
                if (msg.DynamicMessage != null)
                {
                    msg.DynamicMessage.MessageNumber = id;
                    msg.DynamicMessage.MultiString = msg.MultiString;
                    msg.DynamicMessage.MessageOwner = msg.OwnerParameter;
                }
            }


            return (EnumAPIResponse.OK, msg);
        }

        /// <summary>
        /// Funcion verifica si el numero de mensaje es valido
        /// </summary>
        /// <param name="ip">IP del panel led</param>
        /// <param name="id">Numero del mensaje</param>
        /// <returns>Retorna una enumeración con las diferentes posibles respuestas</returns>
        public EnumAPIResponse CheckMessageId(string ip, int id)
        {
            var pdu = new Pdu(PduType.Get);
            //Maximum Number of Changeable Messages Parameter
            pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.5.3.0");

            var (response, snmpPacket) = SNMPRequest(ip, pdu);

            if (response != EnumAPIResponse.OK)
            {
                return response;
            }

            int numMsg = Convert.ToInt32(snmpPacket?.Pdu.VbList[0].Value.ToString());
            _log.LogDebug("numMsg:{numMsg}, msg request:{id}", numMsg, id);

            if ((id > 0 && id <= numMsg) == false)
            {
                _log.LogError("wrong message id");
                return EnumAPIResponse.WRONG_MESSAGE_ID;
            }

            return EnumAPIResponse.OK;

        }

    }
}
