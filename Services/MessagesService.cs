using SnmpSharpNet;
using System.Net;
using WebApiTraductorPMV.Dtos;
using WebApiTraductorPMV.Enums;
using WebApiTraductorPMV.Parameters;
using WebApiTraductorPMV.Utils;

namespace WebApiTraductorPMV.Services;

/// <summary>
/// Clase que permite obtener y/o modificar los mensajes que contienen los paneles led
/// </summary>
public class MessagesService : IMessagesService
{
    private readonly ILogger<MessagesService> _log;
    private readonly ICommonCode _commonCode;

    public MessagesService(ILogger<MessagesService> log, ICommonCode commonCode)
    {
        _log = log;
        _commonCode = commonCode;
    }

    /// <summary>
    /// Permite guardar el mensaje en la memoria del panel
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="trama">Clase con toda la información necesaria para crear o actualizar el mensaje</param>
    /// <returns>Retorna una enumeración con las diferentes posibles respuestas</returns>
    public EnumAPIResponse SetMessage(string ip, DynamicMessageSign trama)
    {
        if (Global.IsIpAddressValid(ip) == false)
        {
            _log.LogError("error, invalid ip address");
            return EnumAPIResponse.ERROR_INVALID_MODEL;
        }


        return SaveMessageOnMemory(ip, trama);

    }

    /// <summary>
    /// Obtiene un listado de mensajes que tiene la tarjeta en memoria
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns>Retorna una dupla de valores, el primero una enumeración donde OK es exitoso y diferente a eso es un error, la segunda el objeto con lo solicitado</returns>
    public (EnumAPIResponse response, PagedList<MessageDTO>? messages) GetMessages(QueryStringParameters parameters)
    {
        if (Global.IsIpAddressValid(parameters.IP) == false)
        {
            _log.LogError("error, invalid ip address");
            return (EnumAPIResponse.ERROR_INVALID_MODEL, null);
        }

        return FillMessagesArray(parameters.IP, parameters);
    }

    /// <summary>
    /// Obtiene un mensaje especifico del panel
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="id">Numero del mensaje</param>
    /// <param name="memoryType">Tipo de memoria donde esta guardado el mensaje</param>
    /// <returns>Retorna una dupla de valores, el primero una enumeración donde OK es exitoso y diferente a eso es un error, la segunda el objeto con lo solicitado</returns>
    public (EnumAPIResponse response, MessageDTO? message) GetMessage(string ip, int id, EnumMemoryType memoryType)
    {
        if (Global.IsIpAddressValid(ip) == false)
        {
            _log.LogError("error, invalid ip address");
            return (EnumAPIResponse.ERROR_INVALID_MODEL, null);
        }

        var (responseStatus, status) = FillStatus(ip);
        if (responseStatus != EnumAPIResponse.OK)
        {
            return (responseStatus, null);
        }


        var (response, message) = _commonCode.FillMessage(ip, id, memoryType, true);
        if (response != EnumAPIResponse.OK)
        {
            return (response, null);
        }

        message.IsActive = (message?.OwnerParameter == status?.CurrentMessage?.OwnerParameter);

        return (response, message);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public EnumAPIResponse DeleteMessage(string ip, int id)
    {
        if (Global.IsIpAddressValid(ip) == false)
        {
            _log.LogError("error, invalid ip address");
            return EnumAPIResponse.ERROR_INVALID_MODEL;
        }


        return DeleteMessageOnMemory(ip, id);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="trama"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public EnumAPIResponse ActivateMessage(string ip, ActivateMessage trama)
    {
        if (Global.IsIpAddressValid(ip) == false)
        {
            _log.LogError("error, invalid ip address");
            return EnumAPIResponse.ERROR_INVALID_MODEL;
        }

        return ActivateMessageOnMemory(ip, trama);
    }



    /// <summary>
    /// Función que permite guardar un mensaje en la memoria del panel
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="trama">Objeto con la información del mensaje</param>
    /// <param name="blankMessage"></param>
    /// <returns>Retorna una enumeración con las diferentes posibles respuestas</returns>
    private EnumAPIResponse SaveMessageOnMemory(string ip, DynamicMessageSign trama, bool blankMessage = false)
    {
        var result = _commonCode.CheckMessageId(ip, trama.MessageNumber);
        if (result != EnumAPIResponse.OK)
        {
            _log.LogError("error message wrong number:{id}", trama.MessageNumber);
            return result;
        }

        string msgMulti = "";

        if (trama.Pages != null && trama.Pages.Count() > 0)
        {
            foreach (var page in trama.Pages)
            {
                if (msgMulti.Length > 0)
                {
                    msgMulti += "[np]";
                }

                if (page.Graphic[0] == 0 && page.Graphic[1] == 0 && page.Graphic[2] == 0)
                {
                    //msgMulti += $"[pt{msg.PageTime}o][pb{string.Join(",", msg.ColorPage)}][sc{msg.SpacingCharacter}][jp{msg.JustificationPage}][fo{msg.Font}][tr{string.Join(",", msg.TextRectangle)}][cf{string.Join(",", msg.ColorForeground)}]";
                    msgMulti += $"[pt{page.PageTime}o][pb{string.Join(",", page.PageBackgroundColor)}][jp{page.JustificationPage}][fo{page.Font}][tr{string.Join(",", page.TextRectangle)}][cf{string.Join(",", page.ColorForeground)}]";
                }
                else
                {
                    //msgMulti += $"[pt{msg.PageTime}o][pb{string.Join(",", msg.ColorPage)}][sc{msg.SpacingCharacter}][jp{msg.JustificationPage}][fo{msg.Font}][tr{string.Join(",", msg.TextRectangle)}][g{string.Join(",", msg.Graphic)}][cf{string.Join(",", msg.ColorForeground)}]";
                    msgMulti += $"[pt{page.PageTime}o][pb{string.Join(",", page.PageBackgroundColor)}][jp{page.JustificationPage}][fo{page.Font}][tr{string.Join(",", page.TextRectangle)}][g{string.Join(",", page.Graphic)}][cf{string.Join(",", page.ColorForeground)}]";
                }

                foreach (var line in page.Lines)
                {
                    msgMulti += $"[nl{line.NewLine}][jl{line.JustificationLine}][sc{page.SpacingCharacter}]{line.Text}[/sc]";
                }
            }
        }
        else
        {
            if (trama.MultiString != null && trama.MultiString.Length > 0)
            {
                msgMulti = trama.MultiString;
            }
        }

        _log.LogDebug("msgMulti:{msgMulti}", msgMulti);
        //msgMulti = "[pt200o][pb0,0,0][sc1][jp0][fo50][tr49,1,144,48][g1,1,1][cf255,255,0][nl1][jl2]AAAA[nl1][jl3]BBB[nl1][jl2]CCC[np][pt60o][pb0,0,0][sc1][jp2][fo51][tr49,1,144,48][g2,1,1][cf0,255,0][nl1][jl3]ENCIENDA[nl1][jl3]LUCES[nl1][jl2]:)[np][pt100o][pb0,0,0][sc1][jp2][fo50][tr49,1,144,48][g3,1,1][cf255,0,0][nl1][jl3]MAS[nl1][jl3]ADELANTE";
        //msgMulti = "[pt30o5]THIS IS[np][pt20o10]A TEST";

        if (msgMulti.Length == 0 && blankMessage == false)
        {
            _log.LogError("error msgMulti is null");
            return EnumAPIResponse.ERROR_INVALID_MODEL;
        }

        var message = new OctetString(msgMulti);
        var statusParameterStepOne = EnumStatusParameter.modifyReq;
        var statusParameterStepTwo = EnumStatusParameter.validateReq;
        if (msgMulti.Length == 0)
        {
            statusParameterStepOne = EnumStatusParameter.modifyReq;
            statusParameterStepTwo = EnumStatusParameter.validateReq;
        }

        result = StepOneSaveMessage(ip, trama, statusParameterStepOne);
        if (result != EnumAPIResponse.OK)
        {
            _log.LogError("error on step one");
            return result;
        }

        result = StepTwoSaveMessage(ip, trama, message, statusParameterStepTwo);
        if (result != EnumAPIResponse.OK)
        {
            _log.LogError("error on step two");
            return result;
        }

        result = StepThreeSaveMessage(ip, trama, message);
        if (result != EnumAPIResponse.OK)
        {
            _log.LogError("error on step three");
            return result;
        }

        _log.LogInformation("save message ok");
        return EnumAPIResponse.OK;
    }

    /// <summary>
    /// Devuelve un array con los mensajes que tiene el panel
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="parameters"></param>
    /// <returns>Retorna una dupla de valores, el primero una enumeración donde OK es exitoso y diferente a eso es un error, la segunda el objeto con lo solicitado</returns>
    private (EnumAPIResponse response, PagedList<MessageDTO>? messages) FillMessagesArray(string ip, QueryStringParameters parameters)
    {
        _log.LogDebug("FillMessagesArray:{ip}", ip);


        var (responseStatus, status) = FillStatus(ip);
        if (responseStatus != EnumAPIResponse.OK)
        {
            return (responseStatus, null);
        }

        var pdu = new Pdu(PduType.Get);
        //Maximum Number of Changeable Messages Parameter;
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.5.3.0");

        var (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);

        if (response != EnumAPIResponse.OK || snmpPacket == null)
        {
            return (response, null);
        }

        int totalMessages = Convert.ToInt32(snmpPacket.Pdu.VbList[0].Value.ToString());
        _log.LogDebug("totalMessages:{totalMessages}", totalMessages);

        if (parameters.PageSize > totalMessages)
        {
            parameters.PageSize = totalMessages;
        }


        if (parameters.PageSize == -1)
        {
            parameters.PageSize = totalMessages;
            parameters.PageNumber = 0;
        }

        //int offset = (parameters.PageNumber - 1) * parameters.PageSize;
        int offset = parameters.PageNumber * parameters.PageSize;
        int limit = offset + parameters.PageSize;
        if (limit > totalMessages)
        {
            limit = totalMessages;
        }


        var list = new List<MessageDTO>();
        for (int i = offset; i < limit; i++)
        {
            var (result, message) = _commonCode.FillMessage(ip, i + 1, EnumMemoryType.Changeable);
            if (result != EnumAPIResponse.OK)
            {
                return (result, null);
            }

            if (message != null)
            {
                message.IsActive = (message.OwnerParameter == status?.CurrentMessage?.OwnerParameter);
                list.Add(message);
            }
        }

        return (EnumAPIResponse.OK, PagedList<MessageDTO>.ToPagedList(list, totalMessages, parameters.PageNumber, parameters.PageSize));
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

    private EnumAPIResponse DeleteMessageOnMemory(string ip, int id)
    {

        var blankMsg = new DynamicMessageSign
        {
            MessageNumber = id,
            MultiString = "[pb0,0,0][tr0,0,0,0][g0,0,0][cf0,0,0]",
            MessageOwner = id.ToString()
        };


        return SaveMessageOnMemory(ip, blankMsg, true);



        //var result = CheckMessageId(ip, id);
        //if (result != EnumAPIResponse.OK)
        //{
        //    _log.LogError("error message wrong number:{id}", id);
        //    return result;
        //}


        //result = StepOneSaveMessage(ip, trama);
        //if (result != EnumAPIResponse.OK)
        //{
        //    _log.LogError("error on step one");
        //    return result;
        //}

        //result = StepTwoSaveMessage(ip, trama, message);
        //if (result != EnumAPIResponse.OK)
        //{
        //    _log.LogError("error on step two");
        //    return result;
        //}

        //result = StepThreeSaveMessage(ip, trama, message);
        //if (result != EnumAPIResponse.OK)
        //{
        //    _log.LogError("error on step three");
        //    return result;
        //}

        //_log.LogInformation("save message ok");
        //return EnumAPIResponse.OK;
    }

    /// <summary>
    /// Paso 1 para guardar el mensaje en la memoria del panel
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="trama">Objeto que tiene los datos del mensaje a guardar</param>
    /// <param name="statusParameter"></param>
    /// <returns>Retorna una enumeración con las diferentes posibles respuestas</returns>
    private EnumAPIResponse StepOneSaveMessage(string ip, DynamicMessageSign trama, EnumStatusParameter statusParameter)
    {
        var pdu = new Pdu(PduType.Set);
        //Message Status Parameter
        pdu.VbList.Add(new Oid("1.3.6.1.4.1.1206.4.2.3.5.8.1.9.3." + trama.MessageNumber), new Integer32(Convert.ToInt32(statusParameter)));

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
    /// Paso 2 para guardar el mensaje en la memoria del panel
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="trama">Objeto que tiene los datos del mensaje a guardar</param>
    /// <param name="message">Mensaje en formato multistring a guardar</param>
    /// <param name="statusParameter"></param>
    /// <returns>Retorna una enumeración con las diferentes posibles respuestas</returns>
    private EnumAPIResponse StepTwoSaveMessage(string ip, DynamicMessageSign trama, OctetString message, EnumStatusParameter statusParameter)
    {
        var pdu = new Pdu(PduType.Set);
        //Message MULTI String Parameter
        pdu.VbList.Add(new Oid("1.3.6.1.4.1.1206.4.2.3.5.8.1.3.3." + trama.MessageNumber), message);
        //Message Owner Parameter
        pdu.VbList.Add(new Oid("1.3.6.1.4.1.1206.4.2.3.5.8.1.4.3." + trama.MessageNumber), new OctetString(trama.MessageOwner));
        //Message Status Parameter
        //pdu.VbList.Add(new Oid("1.3.6.1.4.1.1206.4.2.3.5.8.1.9.3." + trama.MessageNumber), new Integer32(Convert.ToInt32(EnumStatusParameter.validateReq)));
        pdu.VbList.Add(new Oid("1.3.6.1.4.1.1206.4.2.3.5.8.1.9.3." + trama.MessageNumber), new Integer32(Convert.ToInt32(statusParameter)));

        var (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("Error on StepTwoSaveMessage");
            return response;
        }

        // Everything is ok. Agent will return the new value for the OID we changed
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[0].Oid, snmpPacket?.Pdu[0].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[1].Oid, snmpPacket?.Pdu[1].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[0].Oid, snmpPacket?.Pdu[2].Value);

        return EnumAPIResponse.OK;

    }

    /// <summary>
    /// Paso 3 para guardar el mensaje en la memoria del panel
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="trama">Objeto que tiene los datos del mensaje a guardar</param>
    /// <param name="message">Mensaje en formato multistring a guardar</param>
    /// <returns>Retorna una enumeración con las diferentes posibles respuestas</returns>
    private EnumAPIResponse StepThreeSaveMessage(string ip, DynamicMessageSign trama, OctetString message)
    {
        var pdu = new Pdu(PduType.Get);
        //Message Memory Type Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.5.8.1.1.3." + trama.MessageNumber);
        //Message Number Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.5.8.1.2.3." + trama.MessageNumber);
        //Message CRC Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.5.8.1.5.3." + trama.MessageNumber);
        //Message MULTI String Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.5.8.1.3.3." + trama.MessageNumber);
        //Message Owner Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.5.8.1.4.3." + trama.MessageNumber);
        //Message Status Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.5.8.1.9.3." + trama.MessageNumber);
        //Validate Message Error Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.5.9.0");
        //MULTI Syntax Error Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.6.18.0");
        //Position of MULTI Syntax Error Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.6.19.0");


        var (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("Error on StepThreeSaveMessage");
            return response;
        }

        var tipoDeMemoria = snmpPacket?.Pdu.VbList[0].Value.ToString();
        _log.LogDebug("Tipo de Memoria:{tipoDeMemoria}", tipoDeMemoria);
        var numDeMen = snmpPacket?.Pdu.VbList[1].Value.ToString();
        _log.LogDebug("Numero de Mensaje:{numDeMen}", numDeMen);
        var crc = snmpPacket?.Pdu.VbList[2].Value.ToString();

        _log.LogDebug("CRC:{crc}", crc);
        _log.LogDebug("Mensaje: {value}", snmpPacket?.Pdu.VbList[3].Value);
        _log.LogDebug("Mensaje del propietario: {value}", snmpPacket?.Pdu.VbList[4].Value);
        _log.LogDebug("Validacion: {value}", snmpPacket?.Pdu.VbList[5].Value);

        var validateMessageError = Convert.ToInt32(snmpPacket?.Pdu.VbList[6].Value.ToString());
        var sintaxisError = Convert.ToInt32(snmpPacket?.Pdu.VbList[7].Value.ToString());
        var positionMULTISyntaxError = Convert.ToInt32(snmpPacket?.Pdu.VbList[8].Value.ToString());

        _log.LogDebug("validateMessageError:{validateMessageError}, sintaxisError:{sintaxisError}, positionMULTISyntaxError:{positionMULTISyntaxError}", validateMessageError, sintaxisError, positionMULTISyntaxError);

        if (validateMessageError != 2)
        {
            return CheckMessageError(validateMessageError, sintaxisError, positionMULTISyntaxError);
        }

        if (trama.ActivateMessage == false)
        {
            _log.LogDebug("not activate message");
            return EnumAPIResponse.OK;
        }

        string[] dir = ip.Split('.');

        Int32 crccode = (Convert.ToInt32(crc));
        Int16 tipoDeMemoriacode = (Convert.ToInt16(tipoDeMemoria));
        Int32 numDeMencode = (Convert.ToInt32(numDeMen));
        Int32 dir1 = (Convert.ToInt32(dir[0]));
        Int32 dir2 = (Convert.ToInt32(dir[1]));
        Int32 dir3 = (Convert.ToInt32(dir[2]));
        Int32 dir4 = (Convert.ToInt32(dir[3]));


        string hexCrc = crccode.ToString("X4");
        string hexTipo = tipoDeMemoriacode.ToString("X");
        string hexNumero = numDeMencode.ToString("X4");
        string di1 = dir1.ToString("X");
        string di2 = dir2.ToString("X");
        string di3 = dir3.ToString("X");
        string di4 = dir4.ToString("X");
        string w = message.ToString();


        byte numMeria = Convert.ToByte(tipoDeMemoria, 16);
        byte numDMen1 = Convert.ToByte(hexNumero.Substring(0, 2), 16);
        byte numDMen2 = Convert.ToByte(hexNumero.Substring(2, 2), 16);
        byte bcrc1 = Convert.ToByte(hexCrc.Substring(0, 2), 16);
        byte bcrc2 = Convert.ToByte(hexCrc.Substring(2, 2), 16);
        byte d1 = Convert.ToByte(di1, 16);
        byte d2 = Convert.ToByte(di2, 16);
        byte d3 = Convert.ToByte(di3, 16);
        byte d4 = Convert.ToByte(di4, 16);

        var idMensaje = ("Id Mensaje: 0" + hexTipo + hexNumero + hexCrc);
        _log.LogDebug("idMensaje:{idMensaje}", idMensaje);

        var bytes = new byte[12];
        bytes[0] = 0xff;
        bytes[1] = 0xff;
        bytes[2] = 0xff;
        bytes[3] = numMeria;
        bytes[4] = numDMen1;
        bytes[5] = numDMen2;
        bytes[6] = bcrc1;
        bytes[7] = bcrc2;
        bytes[8] = d1;
        bytes[9] = d2;
        bytes[10] = d3;
        bytes[11] = d4;

        var act = new OctetString(bytes);
        _log.LogDebug("act:{act}", act);

        pdu = new Pdu(PduType.Set);
        //Activate Message Parameter
        pdu.VbList.Add(new Oid("1.3.6.1.4.1.1206.4.2.3.6.3.0"), act);

        (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("Error on Activate Message Parameter");
            return response;
        }

        // Everything is ok. Agent will return the new value for the OID we changed
        _log.LogDebug("Agent response {oid}: {value}", snmpPacket?.Pdu[0].Oid, snmpPacket?.Pdu[0].Value);
        _log.LogDebug("Mensaje ACTIVADO");

        return EnumAPIResponse.OK;

    }

    /// <summary>
    /// Procesa el error y retorna una enumeración con el error devuelto por el panel
    /// </summary>
    /// <param name="validateMessageError">numero del error de validación</param>
    /// <param name="sintaxisError">numero de error de sintaxis</param>
    /// <param name="positionMULTISyntaxError">numero de la posición del error</param>
    /// <returns>Retorna una enumeración con las diferentes posibles respuestas</returns>
    private EnumAPIResponse CheckMessageError(int validateMessageError, int sintaxisError, int positionMULTISyntaxError)
    {

        switch (validateMessageError)
        {
            case 1:
                _log.LogError("CheckMessageError, other");
                return EnumAPIResponse.MESSAGE_ERROR_OTHER;
            case 3:
                _log.LogError("CheckMessageError, beacons");
                return EnumAPIResponse.MESSAGE_ERROR_BEACONS;
            case 4:
                _log.LogError("CheckMessageError, pixelService");
                return EnumAPIResponse.MESSAGE_ERROR_PIXEL_SERVICE;
            case 5:
                _log.LogError("CheckMessageError, syntaxMULTI");

                switch (sintaxisError)
                {
                    case 1:
                        _log.LogError("CheckMessageError, syntaxMULTI, other, position:{position}", positionMULTISyntaxError);
                        return EnumAPIResponse.MESSAGE_ERROR_SYNTAXMULTI_OTHER;

                    case 3:
                        _log.LogError("CheckMessageError, syntaxMULTI, unsupportedTag, position:{position}", positionMULTISyntaxError);
                        return EnumAPIResponse.MESSAGE_ERROR_SYNTAXMULTI_UNSUPPORTED_TAG;
                    case 4:
                        _log.LogError("CheckMessageError, syntaxMULTI, unsupportedTagValue, position:{position}", positionMULTISyntaxError);
                        return EnumAPIResponse.MESSAGE_ERROR_SYNTAXMULTI_UNSUPPORTED_TAG_VALUE;
                    case 5:
                        _log.LogError("CheckMessageError, syntaxMULTI, textTooBig, position:{position}", positionMULTISyntaxError);
                        return EnumAPIResponse.MESSAGE_ERROR_SYNTAXMULTI_TEXT_TOO_BIG;
                    case 6:
                        _log.LogError("CheckMessageError, syntaxMULTI, fontNotDefined, position:{position}", positionMULTISyntaxError);
                        return EnumAPIResponse.MESSAGE_ERROR_SYNTAXMULTI_FONT_NOT_DEFINED;
                    case 7:
                        _log.LogError("CheckMessageError, syntaxMULTI, characterNotDefined, position:{position}", positionMULTISyntaxError);
                        return EnumAPIResponse.MESSAGE_ERROR_SYNTAXMULTI_CHARACTER_NOT_DEFINED;
                    case 8:
                        _log.LogError("CheckMessageError, syntaxMULTI, fieldDeviceNotExist, position:{position}", positionMULTISyntaxError);
                        return EnumAPIResponse.MESSAGE_ERROR_SYNTAXMULTI_FIELD_DEVICE_NOT_EXIST;
                    case 9:
                        _log.LogError("CheckMessageError, syntaxMULTI, fieldDeviceError, position:{position}", positionMULTISyntaxError);
                        return EnumAPIResponse.MESSAGE_ERROR_SYNTAXMULTI_FIELD_DEVICE_ERROR;
                    case 10:
                        _log.LogError("CheckMessageError, syntaxMULTI, flashRegionError, position:{position}", positionMULTISyntaxError);
                        return EnumAPIResponse.MESSAGE_ERROR_SYNTAXMULTI_FLASH_REGION_ERROR;
                    case 11:
                        _log.LogError("CheckMessageError, syntaxMULTI, tagConflict, position:{position}", positionMULTISyntaxError);
                        return EnumAPIResponse.MESSAGE_ERROR_SYNTAXMULTI_TAG_CONFLICT;
                    case 12:
                        _log.LogError("CheckMessageError, syntaxMULTI, tooManyPages, position:{position}", positionMULTISyntaxError);
                        return EnumAPIResponse.MESSAGE_ERROR_SYNTAXMULTI_TOO_MANY_PAGES;
                    case 13:
                        _log.LogError("CheckMessageError, syntaxMULTI, fontVersionID, position:{position}", positionMULTISyntaxError);
                        return EnumAPIResponse.MESSAGE_ERROR_SYNTAXMULTI_FONT_VERSION_ID;
                    case 14:
                        _log.LogError("CheckMessageError, syntaxMULTI, graphicID, position:{position}", positionMULTISyntaxError);
                        return EnumAPIResponse.MESSAGE_ERROR_SYNTAXMULTI_GRAPHIC_ID;
                    case 15:
                        _log.LogError("CheckMessageError, syntaxMULTI, graphicNotDefined, position:{position}", positionMULTISyntaxError);
                        return EnumAPIResponse.MESSAGE_ERROR_SYNTAXMULTI_GRAPHIC_NOT_DEFINED;
                }
                break;
        }

        _log.LogError("CheckMessageError, unknow error validateMessageError:{validateMessageError}, sintaxisError:{sintaxisError}", validateMessageError, sintaxisError);
        return EnumAPIResponse.EXCEPTION;
    }


    private EnumAPIResponse ActivateMessageOnMemory(string ip, ActivateMessage trama)
    {
        var (response, message) = _commonCode.FillMessage(ip, trama.MessageNumber, EnumMemoryType.Changeable, true);
        if (response != EnumAPIResponse.OK)
        {
            return response;
        }

        if (trama.Activate == false)
        {
            return DeactivateMessage(ip);
        }


        var octetString = new OctetString(message.MultiString);
        message.DynamicMessage.ActivateMessage = trama.Activate;

        var result = StepThreeSaveMessage(ip, message.DynamicMessage, octetString);
        if (result != EnumAPIResponse.OK)
        {
            _log.LogError("error on step three");
            return result;
        }

        _log.LogInformation("save message ok");
        return EnumAPIResponse.OK;
    }


    private EnumAPIResponse DeactivateMessage(string ip)
    {
        var tipoDeMemoria = "7";
        var numDeMen = "1";
        _log.LogDebug("Numero de Mensaje:{numDeMen}", numDeMen);
        var crc = "0";

        string[] dir = ip.Split('.');

        Int32 crccode = (Convert.ToInt32(crc));
        Int16 tipoDeMemoriacode = (Convert.ToInt16(tipoDeMemoria));
        Int32 numDeMencode = (Convert.ToInt32(numDeMen));
        Int32 dir1 = (Convert.ToInt32(dir[0]));
        Int32 dir2 = (Convert.ToInt32(dir[1]));
        Int32 dir3 = (Convert.ToInt32(dir[2]));
        Int32 dir4 = (Convert.ToInt32(dir[3]));

        var message = new OctetString("");

        string hexCrc = crccode.ToString("X4");
        string hexTipo = tipoDeMemoriacode.ToString("X");
        string hexNumero = numDeMencode.ToString("X4");
        string di1 = dir1.ToString("X");
        string di2 = dir2.ToString("X");
        string di3 = dir3.ToString("X");
        string di4 = dir4.ToString("X");
        string w = message.ToString();


        byte numMeria = Convert.ToByte(tipoDeMemoria, 16);
        byte numDMen1 = Convert.ToByte(hexNumero.Substring(0, 2), 16);
        byte numDMen2 = Convert.ToByte(hexNumero.Substring(2, 2), 16);
        byte bcrc1 = Convert.ToByte(hexCrc.Substring(0, 2), 16);
        byte bcrc2 = Convert.ToByte(hexCrc.Substring(2, 2), 16);
        byte d1 = Convert.ToByte(di1, 16);
        byte d2 = Convert.ToByte(di2, 16);
        byte d3 = Convert.ToByte(di3, 16);
        byte d4 = Convert.ToByte(di4, 16);

        var idMensaje = ("Id Mensaje: 0" + hexTipo + hexNumero + hexCrc);
        _log.LogDebug("idMensaje:{idMensaje}", idMensaje);

        var bytes = new byte[12];
        bytes[0] = 0xff;
        bytes[1] = 0xff;
        bytes[2] = 0xff;
        bytes[3] = numMeria;
        bytes[4] = numDMen1;
        bytes[5] = numDMen2;
        bytes[6] = bcrc1;
        bytes[7] = bcrc2;
        bytes[8] = d1;
        bytes[9] = d2;
        bytes[10] = d3;
        bytes[11] = d4;

        var act = new OctetString(bytes);
        _log.LogDebug("act:{act}", act);

        var pdu = new Pdu(PduType.Set);
        //Activate Message Parameter
        pdu.VbList.Add(new Oid("1.3.6.1.4.1.1206.4.2.3.6.3.0"), act);

        var (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("Error on Activate Message Parameter");
            return response;
        }

        // Everything is ok. Agent will return the new value for the OID we changed
        _log.LogDebug("Agent response {oid}: {value}", snmpPacket?.Pdu[0].Oid, snmpPacket?.Pdu[0].Value);
        _log.LogDebug("Mensaje Desactivado");

        return EnumAPIResponse.OK;

    }


}