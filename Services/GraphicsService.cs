using SnmpSharpNet;
using System.Net;
using WebApiTraductorPMV.Dtos;
using WebApiTraductorPMV.Enums;
using WebApiTraductorPMV.Parameters;
using WebApiTraductorPMV.Utils;

namespace WebApiTraductorPMV.Services;

public class GraphicsService : IGraphicsService
{
    private readonly ILogger<GraphicsService> _log;
    private readonly ICommonCode _commonCode;

    public GraphicsService(ILogger<GraphicsService> log, ICommonCode commonCode)
    {
        _log = log;
        _commonCode = commonCode;
    }

    /// <summary>
    /// Obtiene información de los graficos almacenadas en el panel
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns>Retorna una dupla de valores, el primero una enumeración donde OK es exitoso y diferente a eso es un error, la segunda el objeto con lo solicitado</returns>
    public (EnumAPIResponse response, PagedList<GraphicDTO>? graphics) GetGraphics(QueryStringParameters parameters)
    {
        if (Global.IsIpAddressValid(parameters.IP) == false)
        {
            _log.LogError("error, invalid ip address");
            return (EnumAPIResponse.ERROR_INVALID_MODEL, null);
        }

        return FillGraphics(parameters);

    }

    /// <summary>
    /// Obtiene un grafico especifico del panel
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="id">Numero del grafico</param>
    /// <returns>Retorna una dupla de valores, el primero una enumeración donde OK es exitoso y diferente a eso es un error, la segunda el objeto con lo solicitado</returns>
    public (EnumAPIResponse response, GraphicDTO? graphic) GetGraphic(string ip, int id)
    {
        if (Global.IsIpAddressValid(ip) == false)
        {
            _log.LogError("error, invalid ip address");
            return (EnumAPIResponse.ERROR_INVALID_MODEL, null);
        }
        return FillGraphic(ip, id);

    }

    /// <summary>
    /// Función que permite guardar un grafico
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="trama">Objeto con los datos necesarios para el grafico</param>
    /// <returns>Retorna una enumeración con las diferentes posibles respuestas</returns>
    public EnumAPIResponse SetGraphic(string ip, Graphic trama)
    {
        if (Global.IsIpAddressValid(ip) == false)
        {
            _log.LogError("error, invalid ip address");
            return EnumAPIResponse.ERROR_INVALID_MODEL;
        }

        return SaveGraphic(ip, trama);

    }

    /// <summary>
    /// Obtiene información de las imagenes almacenadas en el panel
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns>Retorna una dupla de valores, el primero una enumeración donde OK es exitoso y diferente a eso es un error, la segunda el objeto con lo solicitado</returns>
    private (EnumAPIResponse response, PagedList<GraphicDTO>? graphic) FillGraphics(QueryStringParameters parameters)
    {
        _log.LogDebug("FillGraphic, ip:{ip}", parameters.IP);

        var pdu = new Pdu(PduType.Get);
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.10.1.0");//Maximum Number of Graphics
        var (response, snmpPacket) = _commonCode.SNMPRequest(parameters.IP, pdu);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("Error on FillScheduleArray, get initial data");
            return (response, null);
        }
        var maxNumberOfGraphics = Convert.ToInt32(snmpPacket?.Pdu.VbList[0].Value.ToString());
        if (parameters.PageSize > maxNumberOfGraphics)
        {
            parameters.PageSize = maxNumberOfGraphics;
        }

        if (parameters.PageSize == -1)
        {
            parameters.PageSize = maxNumberOfGraphics;
            parameters.PageNumber = 0;
        }


        //int offset = (parameters.PageNumber - 1) * parameters.PageSize;
        int offset = parameters.PageNumber * parameters.PageSize;
        int limit = offset + parameters.PageSize;
        if (limit > maxNumberOfGraphics)
        {
            limit = maxNumberOfGraphics;
        }

        var listGraphics = new List<GraphicDTO>();

        for (int i = offset; i < limit; i++)
        {
            _log.LogInformation($"start graphic number:{i + 1}...");

            pdu = new Pdu(PduType.Get);
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.10.6.1.2.{i + 1}");//Graphic Number
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.10.6.1.3.{i + 1}");//Graphic Name
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.10.6.1.4.{i + 1}");//Graphic Height
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.10.6.1.5.{i + 1}");//Graphic Width
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.10.6.1.6.{i + 1}");//Graphic Type
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.10.6.1.7.{i + 1}");//Graphic ID Parameter
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.10.6.1.8.{i + 1}");//Graphic Transparent Enabled
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.10.6.1.9.{i + 1}");//Graphic Transparent Color
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.10.6.1.10.{i + 1}");//Graphic Status

            (response, snmpPacket) = _commonCode.SNMPRequest(parameters.IP, pdu);
            if (response != EnumAPIResponse.OK)
            {
                _log.LogError("Error on FillScheduleArray, get initial data");
                return (response, null);
            }

            _log.LogInformation($"end graphic number:{i + 1}");

            var graphicNumber = Convert.ToInt32(snmpPacket?.Pdu.VbList[0].Value.ToString());
            var graphicName = snmpPacket?.Pdu.VbList[1].Value.ToString();
            var graphicHeight = Convert.ToInt32(snmpPacket?.Pdu.VbList[2].Value.ToString());
            var graphicWidth = Convert.ToInt32(snmpPacket?.Pdu.VbList[3].Value.ToString());
            var graphicType = Convert.ToInt32(snmpPacket?.Pdu.VbList[4].Value.ToString());
            var graphicID = Convert.ToInt32(snmpPacket?.Pdu.VbList[5].Value.ToString());
            var graphicTransparentEnabled = Convert.ToInt32(snmpPacket?.Pdu.VbList[6].Value.ToString());
            var graphicTransparentColor = snmpPacket?.Pdu.VbList[7].Value.ToString();
            var graphicStatus = Convert.ToInt32(snmpPacket?.Pdu.VbList[8].Value.ToString());
            var graphics = new GraphicDTO
            {
                Number = graphicNumber,
                Name = graphicName,
                Height = graphicHeight,
                Width = graphicWidth,
                Type = graphicType,
                ID = graphicID,
                TransparentEnabled = graphicTransparentEnabled,
                TransparentColor = graphicTransparentColor,
                Status = graphicStatus
            };

            listGraphics.Add(graphics);
        }

        return (EnumAPIResponse.OK, PagedList<GraphicDTO>.ToPagedList(listGraphics, maxNumberOfGraphics, parameters.PageNumber, parameters.PageSize));
    }

    /// <summary>
    /// Funcion que permite obtener informacion de un grafico especifico del panel
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="id">Numero del grafico</param>
    /// <returns>Retorna una dupla de valores, el primero una enumeración donde OK es exitoso y diferente a eso es un error, la segunda el objeto con lo solicitado</returns>
    private (EnumAPIResponse response, GraphicDTO? graphic) FillGraphic(string ip, int id)
    {
        _log.LogDebug("FillMessage, ip:{ip}, id:{id}", ip, id);

        var result = CheckGraphicId(ip, id);
        if (result != EnumAPIResponse.OK)
        {
            _log.LogError("error message wrong number:{id}", id);
            return (result, null);
        }

        var pdu = new Pdu(PduType.Get);
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.10.6.1.2.{id}");//Graphic Number
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.10.6.1.3.{id}");//Graphic Name
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.10.6.1.4.{id}");//Graphic Height
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.10.6.1.5.{id}");//Graphic Width
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.10.6.1.6.{id}");//Graphic Type
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.10.6.1.7.{id}");//Graphic ID Parameter
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.10.6.1.8.{id}");//Graphic Transparent Enabled
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.10.6.1.9.{id}");//Graphic Transparent Color
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.10.6.1.10.{id}");//Graphic Status

        var (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("Error on FillScheduleArray, get initial data");
            return (response, null);
        }
        var graphicNumber = Convert.ToInt32(snmpPacket?.Pdu.VbList[0].Value.ToString());
        var graphicName = snmpPacket?.Pdu.VbList[1].Value.ToString();
        var graphicHeight = Convert.ToInt32(snmpPacket?.Pdu.VbList[2].Value.ToString());
        var graphicWidth = Convert.ToInt32(snmpPacket?.Pdu.VbList[3].Value.ToString());
        var graphicType = Convert.ToInt32(snmpPacket?.Pdu.VbList[4].Value.ToString());
        var graphicID = Convert.ToInt32(snmpPacket?.Pdu.VbList[5].Value.ToString());
        var graphicTransparentEnabled = Convert.ToInt32(snmpPacket?.Pdu.VbList[6].Value.ToString());
        var graphicTransparentColor = snmpPacket?.Pdu.VbList[7].Value.ToString();
        var graphicStatus = Convert.ToInt32(snmpPacket?.Pdu.VbList[8].Value.ToString());

        var graphic = new GraphicDTO
        {
            Number = graphicNumber,
            Name = graphicName,
            Height = graphicHeight,
            Width = graphicWidth,
            Type = graphicType,
            ID = graphicID,
            TransparentEnabled = graphicTransparentEnabled,
            TransparentColor = graphicTransparentColor,
            Status = graphicStatus
        };

        return (EnumAPIResponse.OK, graphic);
    }

    /// <summary>
    /// Paso 1 para guardar una imagen en la memoria del panel
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="trama">Objeto que tiene los datos de la imagen a guardar</param>
    /// <returns>Retorna una enumeración con las diferentes posibles respuestas</returns>
    private EnumAPIResponse StepOneSavegraphic(string ip, Graphic trama)
    {
        var pdu = new Pdu(PduType.Set);
        //Message Status Parameter
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.3.10.6.1.10.{trama.Number}"), new Integer32(7));//Graphic Status

        var (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("Error on StepOneSaveMessage");
            return response;
        }

        // Everything is ok. Agent will return the new value for the OID we changed
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[0].Oid, snmpPacket?.Pdu[0].Value);

        return EnumAPIResponse.OK;

    }

    /// <summary>
    /// Paso 2 para guardar una imagen en la memoria del panel
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="trama">Objeto que tiene los datos del grafico a guardar</param>
    /// <returns>Retorna una enumeración con las diferentes posibles respuestas</returns>
    private EnumAPIResponse StepTwoSavegraphic(string ip, Graphic trama)
    {
        var number = trama.Number;
        var height = trama.Height;
        var width = trama.Width;
        var type = trama.Type;
        var transparent = trama.TransparentEnabled;
        var color = trama.TransparentColor;
        byte color1 = Convert.ToByte(color);

        var pdu = new Pdu(PduType.Set);
        //Message Status Parameter
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.3.10.6.1.2.{number}"), new Integer32(Convert.ToInt32(number)));//Graphic number
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.3.10.6.1.3.{number}"), new OctetString(trama.Name));//Graphic name
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.3.10.6.1.4.{number}"), new Integer32(Convert.ToInt32(height)));//Graphic Height
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.3.10.6.1.5.{number}"), new Integer32(Convert.ToInt32(width)));//Graphic Width
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.3.10.6.1.6.{number}"), new Integer32(Convert.ToInt32(type)));//Graphic Type
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.3.10.6.1.8.{number}"), new Integer32(Convert.ToInt32(transparent)));//Graphic Transparent Enabled
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.3.10.6.1.9.{number}"), new OctetString(color1));//Graphic Transparent Color

        var (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("Error on StepOneSaveMessage");
            return response;
        }

        // Everything is ok. Agent will return the new value for the OID we changed
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[0].Oid, snmpPacket?.Pdu[0].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[1].Oid, snmpPacket?.Pdu[1].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[2].Oid, snmpPacket?.Pdu[2].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[3].Oid, snmpPacket?.Pdu[3].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[4].Oid, snmpPacket?.Pdu[4].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[5].Oid, snmpPacket?.Pdu[5].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[6].Oid, snmpPacket?.Pdu[6].Value);
        return EnumAPIResponse.OK;

    }

    /// <summary>
    /// Paso 3 para guardar una imagen en la memoria del panel
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="trama">Objeto que tiene los datos del grafico a guardar</param>
    /// <returns>Retorna una enumeración con las diferentes posibles respuestas</returns>
    private EnumAPIResponse StepThreeSavegraphic(string ip, Graphic trama)
    {
        string? BMP = new string(trama.BMP);


        byte[]? newBytes = Convert.FromBase64String(BMP);
        var cont = newBytes.Length;
        int j = 0;
        byte[] new1 = new byte[1020];
        byte[] new2 = new byte[1020];
        byte[] new3 = new byte[1020];
        byte[] new4 = new byte[1020];
        byte[] new5 = new byte[1020];
        byte[] new6 = new byte[1020];

        for (int i = 1078; i < 2098 && 2098 < cont; i++)
        {
            new1[j] = newBytes[i];
            j++;
        }
        j = 0;
        for (int i = 2098; i < 3118 && 3118 < cont; i++)
        {
            new2[j] = newBytes[i];
            j++;
        }
        j = 0;
        for (int i = 3118; i < 4138 && 4138 < cont; i++)
        {
            new3[j] = newBytes[i];
            j++;
        }
        j = 0;
        for (int i = 4138; i < 5158 && 5158 < cont; i++)
        {
            new4[j] = newBytes[i];
            j++;
        }
        j = 0;
        for (int i = 5158; i < 6178 && 6178 < cont; i++)
        {
            new5[j] = newBytes[i];
            j++;
        }
        j = 0;
        for (int i = 6178; i < 6178 && 6178 < cont; i++)
        {
            new6[j] = newBytes[i];
            j++;
        }

        OctetString part1 = new OctetString(new1);
        OctetString part2 = new OctetString(new2);
        OctetString part3 = new OctetString(new3);
        OctetString part4 = new OctetString(new4);
        OctetString part5 = new OctetString(new5);
        OctetString part6 = new OctetString(new6);
        OctetString[] new7 = new OctetString[6];
        new7[0] = part1;
        new7[1] = part2;
        new7[2] = part3;
        new7[3] = part4;
        new7[4] = part5;
        new7[5] = part6;
        for (int i = 1; i <= 6; i++)
        {
            var pdu = new Pdu(PduType.Set);
            //Message Status Parameter
            pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.3.10.7.1.3.{trama.Number}.{i}"), new7[i - 1]);//BMP

            var (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
            if (response != EnumAPIResponse.OK)
            {
                _log.LogError("Error on StepOneSaveMessage");
                return response;
            }

            // Everything is ok. Agent will return the new value for the OID we changed
            _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[0].Oid, snmpPacket?.Pdu[0].Value);

        }
        return EnumAPIResponse.OK;
    }

    /// <summary>
    /// Paso 4 para guardar una imagen en la memoria del panel
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="trama">Objeto que tiene los datos del grafico a guardar</param>
    /// <returns>Retorna una enumeración con las diferentes posibles respuestas</returns>
    private EnumAPIResponse StepFourSavegraphic(string ip, Graphic trama)
    {
        var pdu = new Pdu(PduType.Set);
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.3.10.6.1.10.{trama.Number}"), new Integer32(8));//Graphic Status

        var (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("Error on StepOneSaveMessage");
            return response;
        }

        // Everything is ok. Agent will return the new value for the OID we changed
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[0].Oid, snmpPacket?.Pdu[0].Value);

        return EnumAPIResponse.OK;

    }

    /// <summary>
    /// Función que permite guardar una imagen 
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="trama">Objeto con los datos necesarios para el grafico</param>
    /// <returns>Retorna una enumeración con las diferentes posibles respuestas</returns>
    private EnumAPIResponse SaveGraphic(string ip, Graphic trama)
    {
        var result = StepOneSavegraphic(ip, trama);
        if (result != EnumAPIResponse.OK)
        {
            _log.LogError("error on step one");
            return result;
        }

        result = StepTwoSavegraphic(ip, trama);
        if (result != EnumAPIResponse.OK)
        {
            _log.LogError("error on step two");
            return result;
        }

        result = StepThreeSavegraphic(ip, trama);
        if (result != EnumAPIResponse.OK)
        {
            _log.LogError("error on step three");
            return result;
        }

        result = StepFourSavegraphic(ip, trama);
        if (result != EnumAPIResponse.OK)
        {
            _log.LogError("error on step three");
            return result;
        }

        _log.LogInformation("save graphic ok");
        return EnumAPIResponse.OK;
    }

    /// <summary>
    /// Funcion verifica si el numero de grafico es valido
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="id">Numero del mensaje</param>
    /// <returns>Retorna una enumeración con las diferentes posibles respuestas</returns>
    private EnumAPIResponse CheckGraphicId(string ip, int id)
    {
        var pdu = new Pdu(PduType.Get);
        //Maximum Number of Graphics
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.10.1.0");

        var (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);

        if (response != EnumAPIResponse.OK)
        {
            return response;
        }

        int numGraphic = Convert.ToInt32(snmpPacket?.Pdu.VbList[0].Value.ToString());
        _log.LogDebug("numGraphic:{numGraphic}, Graphic request:{id}", numGraphic, id);

        if ((id > 0 && id <= numGraphic) == false)
        {
            _log.LogError("wrong graphic id");
            return EnumAPIResponse.WRONG_GRAPHIC_ID;
        }

        return EnumAPIResponse.OK;
    }




}
