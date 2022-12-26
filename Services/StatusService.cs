using SnmpSharpNet;
using System.Diagnostics;
using System.Net;
using WebApiTraductorPMV.Dtos;
using WebApiTraductorPMV.Enums;
using WebApiTraductorPMV.Utils;

namespace WebApiTraductorPMV.Services;

public class StatusService : IStatusService
{
    private readonly ILogger<StatusService> _log;
    private readonly ICommonCode _commonCode;

    public StatusService(ILogger<StatusService> log, ICommonCode commonCode)
    {
        _log = log;
        _commonCode = commonCode;
    }
    /// <summary>
    /// Obtiene el status del panel led
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <returns>Retorna una dupla de valores, el primero una enumeración donde OK es exitoso y diferente a eso es un error, la segunda el objeto con lo solicitado</returns>
    public (EnumAPIResponse response, StatusPanelDTO? status) GetStatus(string ip)
    {
        if (Global.IsIpAddressValid(ip) == false)
        {
            _log.LogError("error, invalid ip address");
            return (EnumAPIResponse.ERROR_INVALID_MODEL, null);
        }

        return FillStatus(ip);

    }

    /// <summary>
    /// Obtiene el status del panel
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <returns>Retorna una dupla de valores, el primero una enumeración donde OK es exitoso y diferente a eso es un error, la segunda el objeto con lo solicitado</returns>
    private (EnumAPIResponse response, StatusPanelDTO? status) FillStatus(string ip)
    {
        _log.LogDebug("FillStatus, ip:{ip}", ip);

        var (response, message) = _commonCode.FillMessage(ip, 1, EnumMemoryType.CurrentBuffer);

        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("Error to fill status");
            return (response, null);
        }

        var pdu = new Pdu(PduType.Get);
        //Current date;
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.6.3.1.0");
        //Sign Height in Pixels Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.2.3.0");
        //Sign Width in Pixels Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.2.4.0");//Ancho del letrero en pixeles
                                                       //Number of Fonts Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.3.1.0");//Numero maximo de fuentes a almasenar
                                                       //Maximum Number of Pages Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.4.15.0");//Numero max de paguinas en el mensaje
                                                        //Maximum MULTI String Length Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.4.16.0");//Longuitud Max de la cadena MULTI
                                                        //Number of Permanent Messages Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.5.1.0");//Numero de mensajes permanentes
                                                       //Number of Changeable Messages Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.5.2.0");//Numero de mensajes modificables
                                                       //Maximum Number of Changeable Messages Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.5.3.0");//Numero max de mensajes modificables
                                                       //Maximum Number of Graphics Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.10.1.0");//Numero max de graficos
                                                        //Number of Graphics Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.10.2.0");//Numero de graficos almasenado
                                                        //Illumination Control Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.7.1.0");//Control de brillo
                                                       //Status of Illumination Brightness Level Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.7.5.0");//Nivel de brillo actual
                                                       //Illumination Manual Level Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.7.6.0");//Nivel de breillo en control manual

        var (result, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);

        if (result != EnumAPIResponse.OK)
        {
            return (result, null);
        }

        var status = new StatusPanelDTO
        {
            CurrentMessage = message,
            CurrentDate = Convert.ToInt32(snmpPacket?.Pdu.VbList[0].Value.ToString()),
            CurrentDateGTM = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(snmpPacket?.Pdu.VbList[0].Value.ToString())).ToString("yyyy/MM/dd HH:mm:ss"),
            LocalTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(snmpPacket?.Pdu.VbList[0].Value.ToString())).ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss"),
            SignHeightInPixels = Convert.ToInt32(snmpPacket?.Pdu.VbList[1].Value.ToString()),
            SignWidthInPixels = Convert.ToInt32(snmpPacket?.Pdu.VbList[2].Value.ToString()),
            NumberFonts = Convert.ToInt32(snmpPacket?.Pdu.VbList[3].Value.ToString()),
            MaximumNumberPages = Convert.ToInt32(snmpPacket?.Pdu.VbList[4].Value.ToString()),
            MaximumMultiStringLength = Convert.ToInt32(snmpPacket?.Pdu.VbList[5].Value.ToString()),
            NumberPermanentMessages = Convert.ToInt32(snmpPacket?.Pdu.VbList[6].Value.ToString()),
            NumberChangeableMessages = Convert.ToInt32(snmpPacket?.Pdu.VbList[7].Value.ToString()),
            MaximumNumberChangeableMessages = Convert.ToInt32(snmpPacket?.Pdu.VbList[8].Value.ToString()),
            MaximumNumberGraphics = Convert.ToInt32(snmpPacket?.Pdu.VbList[9].Value.ToString()),
            NumberGraphics = Convert.ToInt32(snmpPacket?.Pdu.VbList[10].Value.ToString()),
            IlluminationControlNumber = Convert.ToInt32(snmpPacket?.Pdu.VbList[11].Value.ToString()),
            StatusIlluminationBrightnessLevel = Convert.ToInt32(snmpPacket?.Pdu.VbList[12].Value.ToString()),
            IlluminationManualLevel = Convert.ToInt32(snmpPacket?.Pdu.VbList[13].Value.ToString())
        };

        return (EnumAPIResponse.OK, status);
    }


    public EnumAPIResponse RestartPanel(string ip)
    {
        if (Global.IsIpAddressValid(ip) == false)
        {
            _log.LogError("error, invalid ip address");
            return EnumAPIResponse.ERROR_INVALID_MODEL;
        }


        var pdu = new Pdu(PduType.Set);
        // 5.7.2 Software Reset Parameter
        pdu.VbList.Add(new Oid("1.3.6.1.4.1.1206.4.2.3.6.2.0"), new Integer32(1));

        var (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("Error on RestartPanel");
            return response;
        }

        // Everything is ok. Agent will return the new value for the OID we changed
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[0].Oid, snmpPacket?.Pdu[0].Value);

        return EnumAPIResponse.OK;


    }

}
