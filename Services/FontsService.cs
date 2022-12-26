using SnmpSharpNet;
using System.Net;
using WebApiTraductorPMV.Dtos;
using WebApiTraductorPMV.Enums;
using WebApiTraductorPMV.Parameters;
using WebApiTraductorPMV.Utils;

namespace WebApiTraductorPMV.Services;

public class FontsService : IFontsService
{
    private readonly ILogger<FontsService> _log;
    private readonly ICommonCode _commonCode;

    public FontsService(ILogger<FontsService> log, ICommonCode commonCode)
    {
        _log = log;
        _commonCode = commonCode;
    }


    /// <summary>
    /// Obtiene información de las fuentes almacenadas en el panel
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns>Retorna una dupla de valores, el primero una enumeración donde OK es exitoso y diferente a eso es un error, la segunda el objeto con lo solicitado</returns>
    public (EnumAPIResponse response, PagedList<Font>? fonts) GetFonts(QueryStringParameters parameters)
    {
        if (Global.IsIpAddressValid(parameters.IP) == false)
        {
            _log.LogError("error, invalid ip address");
            return (EnumAPIResponse.ERROR_INVALID_MODEL, null);
        }

        return FillFonts(parameters);

    }

    /// <summary>
    /// Obtiene una fuente especifica del panel
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="id">Numero de funete</param>
    /// <returns>Retorna una dupla de valores, el primero una enumeración donde OK es exitoso y diferente a eso es un error, la segunda el objeto con lo solicitado</returns>
    public (EnumAPIResponse response, Font? font) GetFont(string ip, int id)
    {
        if (Global.IsIpAddressValid(ip) == false)
        {
            _log.LogError("error, invalid ip address");
            return (EnumAPIResponse.ERROR_INVALID_MODEL, null);
        }
        return FillFont(ip, id);

    }

    /// <summary>
    /// Obtiene información de las fuentes almacenadas en el panel
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns>Retorna una dupla de valores, el primero una enumeración donde OK es exitoso y diferente a eso es un error, la segunda el objeto con lo solicitado</returns>
    private (EnumAPIResponse response, PagedList<Font>? fonts) FillFonts(QueryStringParameters parameters)
    {
        _log.LogDebug("FillFont, ip:{ip}", parameters.IP);

        var pdu = new Pdu(PduType.Get);
        //Number of Fonts Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.3.1.0");
        //Maximum Characters per Font Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.3.3.0");
        //Default Font Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.4.5.0");

        var (response, snmpPacket) = _commonCode.SNMPRequest(parameters.IP, pdu);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("Error on FillScheduleArray, get initial data");
            return (response, null);
        }

        var numberOfFonts = Convert.ToInt32(snmpPacket?.Pdu.VbList[0].Value.ToString());
        //var maximumCharactersPerFont = Convert.ToInt32(snmpPacket?.Pdu.VbList[1].Value.ToString());
        //var defaultFontParameter = Convert.ToInt32(snmpPacket?.Pdu.VbList[2].Value.ToString());


        if (parameters.PageSize > numberOfFonts)
        {
            parameters.PageSize = numberOfFonts;
        }

        if (parameters.PageSize == -1)
        {
            parameters.PageSize = numberOfFonts;
            parameters.PageNumber = 0;
        }

        //int offset = (parameters.PageNumber - 1) * parameters.PageSize;
        int offset = parameters.PageNumber * parameters.PageSize;
        int limit = offset + parameters.PageSize;
        if (limit > numberOfFonts)
        {
            limit = numberOfFonts;
        }

        var listfont = new List<Font>();

        for (int i = offset; i < limit; i++)
        {
            pdu = new Pdu(PduType.Get);
            //index
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.3.2.1.1.{i + 1}");
            //Font Number Parameter
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.3.2.1.2.{i + 1}");
            //Font Name Parameter
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.3.2.1.3.{i + 1}");
            //Font Height Parameterss
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.3.2.1.4.{i + 1}");
            //Font Version ID Parameter
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.3.2.1.7.{i + 1}");
            //Font Status Parameter
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.3.2.1.8.{i + 1}");

            (response, snmpPacket) = _commonCode.SNMPRequest(parameters.IP, pdu);
            if (response != EnumAPIResponse.OK)
            {
                _log.LogError("Error on FillScheduleArray, get initial data");
                return (response, null);
            }
            var index = Convert.ToInt32(snmpPacket?.Pdu.VbList[0].Value.ToString());
            var fontNumberParameter = Convert.ToInt32(snmpPacket?.Pdu.VbList[1].Value.ToString());
            var fontNameParameter = snmpPacket?.Pdu.VbList[2].Value.ToString();
            var fontHeightParameterss = Convert.ToInt32(snmpPacket?.Pdu.VbList[3].Value.ToString());
            var fontVersionIDParameter = snmpPacket?.Pdu.VbList[4].Value.ToString();
            var fontStatusParameter = Convert.ToInt32(snmpPacket?.Pdu.VbList[5].Value.ToString());
            var f = new Font
            {
                Index = index,
                Number = fontNumberParameter,
                Name = fontNameParameter,
                Height = fontHeightParameterss,
                VersionID = fontVersionIDParameter,
                Status = fontStatusParameter
            };
            listfont.Add(f);
        }

        return (EnumAPIResponse.OK, PagedList<Font>.ToPagedList(listfont, numberOfFonts, parameters.PageNumber, parameters.PageSize));
    }

    /// <summary>
    /// Funcion que permite obtener una fuente especifica del panel
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="id">Numero de la fuente</param>
    /// <returns>Retorna una dupla de valores, el primero una enumeración donde OK es exitoso y diferente a eso es un error, la segunda el objeto con lo solicitado</returns>
    private (EnumAPIResponse response, Font? font) FillFont(string ip, int id)
    {
        _log.LogDebug("FillMessage, ip:{ip}, id:{id}", ip, id);

        var result = CheckFontId(ip, id);
        if (result != EnumAPIResponse.OK)
        {
            _log.LogError("error message wrong number:{id}", id);
            return (result, null);
        }

        var pdu = new Pdu(PduType.Get);
        //index
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.3.2.1.1.{id}");
        //Font Number Parameter
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.3.2.1.2.{id}");
        //Font Name Parameter
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.3.2.1.3.{id}");
        //Font Height Parameterss
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.3.2.1.4.{id}");
        //Font Version ID Parameter
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.3.2.1.7.{id}");
        //Font Status Parameter
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.3.2.1.8.{id}");

        var (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("Error on FillScheduleArray, get initial data");
            return (response, null);
        }

        var index = Convert.ToInt32(snmpPacket?.Pdu.VbList[0].Value.ToString());
        var fontNumberParameter = Convert.ToInt32(snmpPacket?.Pdu.VbList[1].Value.ToString());
        var fontNameParameter = snmpPacket?.Pdu.VbList[2].Value.ToString();
        var fontHeightParameterss = Convert.ToInt32(snmpPacket?.Pdu.VbList[3].Value.ToString());
        var fontVersionIDParameter = snmpPacket?.Pdu.VbList[4].Value.ToString();
        var fontStatusParameter = Convert.ToInt32(snmpPacket?.Pdu.VbList[5].Value.ToString());
        var font = new Font
        {
            Index = index,
            Number = fontNumberParameter,
            Name = fontNameParameter,
            Height = fontHeightParameterss,
            VersionID = fontVersionIDParameter,
            Status = fontStatusParameter
        };
        return (EnumAPIResponse.OK, font);
    }

    /// <summary>
    /// Funcion verifica si el numero de fuente es valido
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="id">Numero del mensaje</param>
    /// <returns>Retorna una enumeración con las diferentes posibles respuestas</returns>
    private EnumAPIResponse CheckFontId(string ip, int id)
    {
        var pdu = new Pdu(PduType.Get);
        //Number of Fonts Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.3.1.0");

        var (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);

        if (response != EnumAPIResponse.OK)
        {
            return response;
        }

        int numFont = Convert.ToInt32(snmpPacket?.Pdu.VbList[0].Value.ToString());
        _log.LogDebug("numFont:{numFont}, Font request:{id}", numFont, id);

        if ((id > 0 && id <= numFont) == false)
        {
            _log.LogError("wrong font id");
            return EnumAPIResponse.WRONG_FONT_ID;
        }

        return EnumAPIResponse.OK;
    }
}
