using SnmpSharpNet;
using System.Collections;
using WebApiTraductorPMV.Dtos;
using WebApiTraductorPMV.Enums;
using WebApiTraductorPMV.Parameters;
using WebApiTraductorPMV.Utils;

namespace WebApiTraductorPMV.Services;

public class SchedulesService : ISchedulesService
{
    private readonly ILogger<SchedulesService> _log;
    private readonly ICommonCode _commonCode;

    public SchedulesService(ILogger<SchedulesService> log, ICommonCode commonCode)
    {
        _log = log;
        _commonCode = commonCode;
    }

    /// <summary>
    /// Busca y retorna todos los mensajes con horario que existen en el panel
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns>Retorna una dupla de valores, el primero una enumeración donde OK es exitoso y diferente a eso es un error, la segunda el objeto con lo solicitado</returns>
    public (EnumAPIResponse response, PagedList<ScheduleMessageDTO>? messages) GetScheduleMessages(QueryStringParameters parameters)
    {
        if (Global.IsIpAddressValid(parameters.IP) == false)
        {
            _log.LogError("error, invalid ip address");
            return (EnumAPIResponse.ERROR_INVALID_MODEL, null);
        }


        return FillScheduleArrayNew(parameters);

    }

    /// <summary>
    /// Función que permite guardar un mensaje con horario
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="trama">Objeto con los datos necesarios para el mensaje</param>
    /// <returns>Retorna una enumeración con las diferentes posibles respuestas</returns>
    public EnumAPIResponse SetScheduleMessage(string ip, ScheduleAddMessageModel trama)
    {
        if (Global.IsIpAddressValid(ip) == false)
        {
            _log.LogError("error, invalid ip address");
            return EnumAPIResponse.ERROR_INVALID_MODEL;
        }

        return SaveScheduleMessageOnMemory(ip, trama);

    }

    public (EnumAPIResponse response, ScheduleMessageDTO? message) GetScheduleMessage(string ip, string id)
    {
        if (Global.IsIpAddressValid(ip) == false || string.IsNullOrWhiteSpace(id))
        {
            _log.LogError("error, invalid ip address");
            return (EnumAPIResponse.ERROR_INVALID_MODEL, null);
        }

        var lista = id.Split(".");
        if (lista.Length < 3)
        {
            _log.LogError("error, invalid id");
            return (EnumAPIResponse.ERROR_INVALID_MODEL, null);
        }

        return FillSchedule(ip, Convert.ToInt32(lista[0]), Convert.ToInt32(lista[1]), Convert.ToInt32(lista[2]), Convert.ToInt32(lista[3]));
    }


    public EnumAPIResponse UpdateScheduleMessage(string ip, ScheduleEditMessageModel trama)
    {
        if (Global.IsIpAddressValid(ip) == false || string.IsNullOrWhiteSpace(trama.Id))
        {
            _log.LogError("error, invalid ip address");
            return EnumAPIResponse.ERROR_INVALID_MODEL;
        }

        var lista = trama.Id.Split(".");
        if (lista.Length < 3)
        {
            _log.LogError("error, invalid id");
            return EnumAPIResponse.ERROR_INVALID_MODEL;
        }

        return EditScheduleMessageOnMemory(ip, trama.Date, trama.MessageNumber, Convert.ToInt32(lista[0]), Convert.ToInt32(lista[1]), Convert.ToInt32(lista[2]), Convert.ToInt32(lista[3]), true);
    }

    public EnumAPIResponse DeleteScheduleMessage(string ip, string id)
    {
        if (Global.IsIpAddressValid(ip) == false || string.IsNullOrWhiteSpace(id))
        {
            _log.LogError("error, invalid ip address");
            return EnumAPIResponse.ERROR_INVALID_MODEL;
        }

        var lista = id.Split(".");
        if (lista.Length < 3)
        {
            _log.LogError("error, invalid id");
            return EnumAPIResponse.ERROR_INVALID_MODEL;
        }

        return DeleteScheduleMessageOnMemory(ip, Convert.ToInt32(lista[0]), Convert.ToInt32(lista[1]), Convert.ToInt32(lista[2]), Convert.ToInt32(lista[3]));
    }

    /// <summary>
    /// Función que permite guardar un mensaje con horario
    /// </summary>
    /// <param name="ip">IP del panel led</param>
    /// <param name="trama">Objeto con los datos necesarios para el mensaje</param>
    /// <returns>Retorna una enumeración con las diferentes posibles respuestas</returns>
    private EnumAPIResponse SaveScheduleMessageOnMemory(string ip, ScheduleAddMessageModel trama)
    {
        var (response, item) = GetNextFreePosition(ip);
        if (response != EnumAPIResponse.OK)
        {
            return response;
        }

        if (item == null)
        {
            return EnumAPIResponse.EXCEPTION;
        }

        return EditScheduleMessageOnMemory(ip, trama.Date, trama.MessageNumber, item.TimeBaseSchedule, item.DayPlan, item.DayPlantEvent, item.Action, true);
    }

    private (EnumAPIResponse response, ScheduleMessageDTO? message) FillSchedule(string ip, int timeBaseScheduleNumber, int dayPlanNumber, int dayPlanEventNumber, int actionIndex)
    {
        _log.LogInformation("FillSchedule, ip:{ip}, timeBaseScheduleNumber:{timeBaseScheduleNumber}, dayPlanNumber:{dayPlanNumber}, dayPlanEventNumber:{dayPlanEventNumber}, actionIndex:{actionIndex}", ip, timeBaseScheduleNumber, dayPlanNumber, dayPlanEventNumber, actionIndex);


        var pdu = new Pdu(PduType.Get);
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.6.3.3.2.1.2.{timeBaseScheduleNumber}");//timeBaseScheduleMonth
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.6.3.3.2.1.4.{timeBaseScheduleNumber}");//timeBaseScheduleDate
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.6.3.3.2.1.3.{timeBaseScheduleNumber}");//timeBaseScheduleDay
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.6.3.3.2.1.5.{timeBaseScheduleNumber}");//timeBaseScheduleDayPlan
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.6.3.3.5.1.3.{dayPlanNumber}.{dayPlanEventNumber}");//dayPlanHour
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.6.3.3.5.1.4.{dayPlanNumber}.{dayPlanEventNumber}");//dayPlanMinute
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.6.3.3.5.1.5.{dayPlanNumber}.{dayPlanEventNumber}");//dayPlanActionNumberOID
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.8.2.1.2.{actionIndex}");//dmsActionMsgCode


        var (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("Error on FillScheduleArray, get time base");
            return (response, null);
        }

        var monthAux = Convert.ToInt32(snmpPacket?.Pdu.VbList[0].Value.ToString());
        var dateAux = Convert.ToInt32(snmpPacket?.Pdu.VbList[1].Value.ToString());
        var dayAux = Convert.ToInt32(snmpPacket?.Pdu.VbList[2].Value.ToString());
        var dayPlan = Convert.ToInt32(snmpPacket?.Pdu.VbList[3].Value.ToString());


        _log.LogDebug("Entrie:{index}, monthAux:{monthAux}, dateAux:{dateAux}, dayAux:{dayAux}, dayPlan:{dayPlan}", timeBaseScheduleNumber, monthAux, dateAux, dayAux, dayPlan);

        var monthByteArray = new BitArray(new int[] { monthAux });
        var dateByteArray = new BitArray(new int[] { dateAux });

        int[] bitsMonth = monthByteArray.Cast<bool>().Select(bit => bit ? 1 : 0).ToArray();

        int[] bitsDate = monthByteArray.Cast<bool>().Select(bit => bit ? 1 : 0).ToArray();

        if ((bitsMonth[0] == 0 && bitsMonth[13] == 0 && bitsMonth[14] == 0 && bitsMonth[15] == 0 && bitsDate[0] == 0 && dayPlan >= 1 && dayPlan <= 32 && dayPlan == dayPlanNumber) == false)
        {
            return (EnumAPIResponse.WRONG_SCHEDULE_ID, null);
        }



        var month = Math.Log10(monthAux) / Math.Log10(2);
        var date = Math.Log10(dateAux) / Math.Log10(2);
        var day = Math.Log10(dayAux) / Math.Log10(2);


        _log.LogDebug("Month:{month}, date:{date}, day:{day}, dayPlan:{dayPlan}", month, date, day, dayPlan);


        //pdu = new Pdu(PduType.Get);
        //pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.6.3.3.5.1.3.{dayPlan}.{dayPlanEventNumber}");//dayPlanHour
        //pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.6.3.3.5.1.4.{dayPlan}.{dayPlanEventNumber}");//dayPlanMinute
        //pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.6.3.3.5.1.5.{dayPlan}.{dayPlanEventNumber}");//dayPlanActionNumberOID

        //(response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
        //if (response != EnumAPIResponse.OK)
        //{
        //    _log.LogError("Error on FillScheduleArray, get day plan");
        //    return (response, null);
        //}

        var hour = Convert.ToInt32(snmpPacket?.Pdu.VbList[4].Value.ToString());
        var minute = Convert.ToInt32(snmpPacket?.Pdu.VbList[5].Value.ToString());
        var objectIdentifier = snmpPacket?.Pdu.VbList[6].Value.ToString();
        var lista = objectIdentifier?.Split('.');
        var actionOID = Convert.ToInt32(lista?[lista.Length - 1]);


        if ((hour >= 0 && hour < 24 && minute >= 0 && minute < 60 && actionOID > 0 && actionOID == actionIndex) == false)
        {
            return (EnumAPIResponse.WRONG_SCHEDULE_ID, null);
        }

        var dateOffSet = new DateTimeOffset(DateTime.Now.Year, (int)month, (int)date, hour, minute, 0, TimeSpan.Zero);

        _log.LogDebug("dayPlan:{dayPlan}, dayPlanEvent:{dayPlanEvent} hour:{hour}, minute:{minute}: actionOID{actionOID}", dayPlan, dayPlanEventNumber, hour, minute, actionOID);

        //pdu = new Pdu(PduType.Get);
        //pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.8.2.1.2.{actionOID}");//dmsActionMsgCode
        //(response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
        //if (response != EnumAPIResponse.OK)
        //{
        //    _log.LogError("Error on FillScheduleArray, get dmsActionMsgCode");
        //    return (response, null);
        //}

        _log.LogDebug("dmsActionMsgCode:{dmsActionMsgCode}", snmpPacket?.Pdu.VbList[7].Value.ToString());

        var actionMsgCodeByteArray = Global.HexToByteArray(snmpPacket?.Pdu.VbList[7].Value.ToString());
        byte memoryType;
        short messageNumber;

        if (actionMsgCodeByteArray == null)
        {
            return (EnumAPIResponse.EXCEPTION, null);
        }

        using (var ms = new MemoryStream(actionMsgCodeByteArray))
        {
            using var br = new BinaryReader(ms);
            memoryType = br.ReadByte();
            var data = br.ReadBytes(2);
            Array.Reverse(data);
            messageNumber = BitConverter.ToInt16(data, 0);

        }

        var messageSchedule = new ScheduleMessageDTO
        {
            TimeBaseScheduleNumber = timeBaseScheduleNumber,
            DayPlanNumber = dayPlan,
            DayPlanEventNumber = dayPlanEventNumber,
            ActionIndex = actionOID,
            MessageNumber = messageNumber,
            Date = (uint)dateOffSet.ToUnixTimeSeconds(),
            DateGMT = dateOffSet.ToString("yyyy/MM/dd HH:mm"),
            LocalTime = dateOffSet.ToLocalTime().ToString("yyyy/MM/dd HH:mm"),
            Id = $"{timeBaseScheduleNumber}.{dayPlan}.{dayPlanEventNumber}.{actionOID}"

        };

        if (messageNumber > 0)
        {
            var (result, message) = _commonCode.FillMessage(ip, messageNumber, EnumMemoryType.Changeable);
            if (result == EnumAPIResponse.OK)
            {
                messageSchedule.MessageObject = message;
                messageSchedule.Message = message?.Message;
            }
            else
            {
                _log.LogError("error to get message");
            }

        }
        else
        {
            _log.LogError("messageNumber is 0");
        }

        return (EnumAPIResponse.OK, messageSchedule);


    }

    private EnumAPIResponse EditScheduleMessageOnMemory(string ip, uint date, int messageNumber, int timeBaseScheduleNumber, int dayPlanNumber, int dayPlanEventNumber, int actionIndex, bool activateSchedule = true)
    {
        var fecha = DateTimeOffset.FromUnixTimeSeconds(date);
        //if (fecha.DateTime < DateTime.Now)
        //{
        //    _log.LogError("wrong datetime:{date}", fecha.DateTime);
        //    return EnumAPIResponse.WRONG_DATE_TIME;
        //}


        var pdu = new Pdu(PduType.Get);
        //Message Memory Type Parameter
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.5.8.1.1.3.{messageNumber}"); // numero de memoria
        ////Message Number Parameter
        pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.5.8.1.2.3.{messageNumber}"); // numero de mensaje

        var (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("SaveScheduleMessageOnMemory, Error on get data");
            return response;
        }

        byte[] idDelMensaje = new byte[5];
        idDelMensaje[0] = Convert.ToByte(snmpPacket?.Pdu.VbList[0].Value.ToString());
        idDelMensaje[1] = 0X00;
        idDelMensaje[2] = Convert.ToByte(snmpPacket?.Pdu.VbList[1].Value.ToString());
        idDelMensaje[3] = 0X00;
        idDelMensaje[4] = 0X00;

        var diaDeLasemana = Convert.ToInt32(Math.Pow(2, (int)fecha.LocalDateTime.DayOfWeek + 1));
        var numeroDeMes = Convert.ToInt32(Math.Pow(2, (int)fecha.LocalDateTime.Month));
        var diaDelMes = Convert.ToInt32(Math.Pow(2, (int)fecha.LocalDateTime.Day));
        

        pdu = new Pdu(PduType.Set);
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.3.8.2.1.2.{actionIndex}"), new OctetString(idDelMensaje));//dmsactioncode
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.6.3.3.5.1.3.{dayPlanNumber}.{dayPlanEventNumber}"), new Integer32(fecha.LocalDateTime.Hour));//hora
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.6.3.3.5.1.4.{dayPlanNumber}.{dayPlanEventNumber}"), new Integer32(fecha.LocalDateTime.Minute));//minuto
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.6.3.3.5.1.5.{dayPlanNumber}.{dayPlanEventNumber}"), new Oid($"1.3.6.1.4.1.1206.4.2.3.8.2.1.1.{actionIndex}"));//oid del indice de la accion
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.6.3.3.2.1.3.{timeBaseScheduleNumber}"), new Integer32(diaDeLasemana));//dia de la semana
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.6.3.3.2.1.4.{timeBaseScheduleNumber}"), new Integer32(diaDelMes));//dia del mes
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.6.3.3.2.1.2.{timeBaseScheduleNumber}"), new Integer32(numeroDeMes));//mes
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.6.3.3.2.1.5.{timeBaseScheduleNumber}"), new Integer32(dayPlanNumber));//numero del plan

        (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("SaveScheduleMessageOnMemory, Error on save data");
            return response;
        }

        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[0].Oid, snmpPacket?.Pdu[0].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[1].Oid, snmpPacket?.Pdu[1].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[2].Oid, snmpPacket?.Pdu[2].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[3].Oid, snmpPacket?.Pdu[3].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[4].Oid, snmpPacket?.Pdu[4].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[5].Oid, snmpPacket?.Pdu[5].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[6].Oid, snmpPacket?.Pdu[6].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[7].Oid, snmpPacket?.Pdu[7].Value);

        _log.LogDebug("SaveScheduleMessageOnMemory, ok");

        if (activateSchedule)
        {
            //0x00, 0x01, 0xff, 0x06, 0x00, 0x01, 0x00, 0x00, 0xa9, 0x9a, 0x0a, 0x31
            string[] dir = ip.Split('.');
            byte[] activation = new byte[12];
            activation[0] = 0xff;
            activation[1] = 0xff;
            activation[2] = 0xff;
            activation[3] = 6;
            activation[4] = 0;
            activation[5] = 1;
            activation[6] = 0;
            activation[7] = 0;
            activation[8] = Convert.ToByte(dir[0]);
            activation[9] = Convert.ToByte(dir[1]);
            activation[10] = Convert.ToByte(dir[2]);
            activation[11] = Convert.ToByte(dir[3]);

            pdu = new Pdu(PduType.Set);
            pdu.VbList.Add(new Oid("1.3.6.1.4.1.1206.4.2.3.6.3.0"), new OctetString(activation));
            (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
            if (response != EnumAPIResponse.OK)
            {
                _log.LogError("SaveScheduleMessageOnMemory, Error on save data");
                return response;
            }


            pdu = new Pdu(PduType.Get);
            pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.9.7.1.0");
            pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.6.25.0");
            (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
            if (response != EnumAPIResponse.OK)
            {
                _log.LogError("SaveScheduleMessageOnMemory, Error on save data");
                return response;
            }

            var shortErrorStatus = snmpPacket?.Pdu.VbList[0].Value.ToString();
            var dmsActivateMessageState = snmpPacket?.Pdu.VbList[1].Value.ToString();




            _log.LogDebug("SaveScheduleMessageOnMemory, activate Schedule ok, {shortErrorStatus}, {dmsActivateMessageState}", shortErrorStatus, dmsActivateMessageState);
        }

        return EnumAPIResponse.OK;
    }

    private (EnumAPIResponse response, ItemsScheduleMessage? item) GetNextFreePosition(string ip)
    {
        var pdu = new Pdu(PduType.Get);
        //Maximum Number of Time Base Schedule Entries Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.6.3.3.1.0");

        var (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("Error on FillScheduleArray, get initial data");
            return (response, null);
        }

        var maxTimeBaseScheduleEntries = Convert.ToInt32(snmpPacket?.Pdu.VbList[0].Value.ToString());
        var dayPlan = 0;

        _log.LogInformation("maxTimeBaseScheduleEntries:{maxTimeBaseScheduleEntries}", maxTimeBaseScheduleEntries);

        for (int i = 1; i <= maxTimeBaseScheduleEntries; i++)
        {
            
            var actionOID = i;

            pdu = new Pdu(PduType.Get);
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.6.3.3.2.1.2.{i}");//timeBaseScheduleMonth

            (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
            if (response != EnumAPIResponse.OK)
            {
                _log.LogError("Error on FillScheduleArray, get time base");
                return (response, null);
            }

            var monthAux = Convert.ToInt32(snmpPacket?.Pdu.VbList[0].Value.ToString());
            if (monthAux == 0)
            {
                dayPlan = i;
                _log.LogInformation("dayPlan empty:{dayPlan}", dayPlan);
                break;
            }
        }


        if (dayPlan == 0)
        {
            _log.LogError("limit exceeded schedule items, dayplan");
            return (EnumAPIResponse.LIMIT_EXCEEDED_SCHEDULE_ITEMS, null);
        }



        var item = new ItemsScheduleMessage
        {
            TimeBaseSchedule = dayPlan,
            DayPlan = dayPlan,
            DayPlantEvent = 1,
            Action = dayPlan
        };

        _log.LogInformation("after, timeBaseSchedule:{tbs}, dayPlan:{dp}, dayPlantEvent:{dpe}, action:{a}", item.TimeBaseSchedule, item.DayPlan, item.DayPlantEvent, item.Action);

        return (EnumAPIResponse.OK, item);
    }

    private EnumAPIResponse DeleteScheduleMessageOnMemory(string ip, int timeBaseScheduleNumber, int dayPlanNumber, int dayPlanEventNumber, int actionIndex)
    {
        byte[] idDelMensaje = new byte[5];
        idDelMensaje[0] = 0;
        idDelMensaje[1] = 0;
        idDelMensaje[2] = 0;
        idDelMensaje[3] = 0;
        idDelMensaje[4] = 0;

        var diaDeLasemana = 0;
        var numeroDeMes = 0;
        var diaDelMes = 0;

        var pdu = new Pdu(PduType.Set);
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.3.8.2.1.2.{actionIndex}"), new OctetString(idDelMensaje));//dmsactioncode
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.6.3.3.5.1.3.{dayPlanNumber}.{dayPlanEventNumber}"), new Integer32(0));//hora
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.6.3.3.5.1.4.{dayPlanNumber}.{dayPlanEventNumber}"), new Integer32(0));//minuto
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.6.3.3.5.1.5.{dayPlanNumber}.{dayPlanEventNumber}"), new Oid($"1.3.6.1.4.1.1206.4.2.3.8.2.1.1.0"));//oid del indice de la accion
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.6.3.3.2.1.3.{timeBaseScheduleNumber}"), new Integer32(diaDeLasemana));//dia de la semana
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.6.3.3.2.1.4.{timeBaseScheduleNumber}"), new Integer32(diaDelMes));//dia del mes
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.6.3.3.2.1.2.{timeBaseScheduleNumber}"), new Integer32(numeroDeMes));//mes
        pdu.VbList.Add(new Oid($"1.3.6.1.4.1.1206.4.2.6.3.3.2.1.5.{timeBaseScheduleNumber}"), new Integer32(0));//numero del plan


        var (response, snmpPacket) = _commonCode.SNMPRequest(ip, pdu);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("SaveScheduleMessageOnMemory, Error on save data");
            return response;
        }

        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[0].Oid, snmpPacket?.Pdu[0].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[1].Oid, snmpPacket?.Pdu[1].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[2].Oid, snmpPacket?.Pdu[2].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[3].Oid, snmpPacket?.Pdu[3].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[4].Oid, snmpPacket?.Pdu[4].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[5].Oid, snmpPacket?.Pdu[5].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[6].Oid, snmpPacket?.Pdu[6].Value);
        _log.LogDebug("Agent response {Oid}: {value}", snmpPacket?.Pdu[7].Oid, snmpPacket?.Pdu[7].Value);

        _log.LogDebug("SaveScheduleMessageOnMemory, ok");

        return EnumAPIResponse.OK;
    }

    private (EnumAPIResponse response, PagedList<ScheduleMessageDTO>? messages) FillScheduleArrayNew(QueryStringParameters parameters)
    {
        _log.LogDebug("FillScheduleArray ip:{ip}", parameters.IP);

        // 4.2.3.4 Defining a Schedule, NTCIP 1203 v03.05
        var pdu = new Pdu(PduType.Get);
        //Maximum Number of Time Base Schedule Entries Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.6.3.3.1.0");
        // 2.4.4.1 Maximum Number of Day Plans—Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.6.3.3.3.0");
        // 2.4.4.2 Maximum Number of Day Plan Events—Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.6.3.3.4.0");//maxDayPlanEvents
        // 5.9.1 Action Table Entries Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.3.8.1.0");//numActionTableEntries
        // 2.4.4.5 Schedule Status Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.6.3.3.7.0");//Schedule Status Parameter
        // 2.4.4.4 Day Plan Status Parameter
        pdu.VbList.Add("1.3.6.1.4.1.1206.4.2.6.3.3.6.0");//Day Plan Status Parameter

        var (response, snmpPacket) = _commonCode.SNMPRequest(parameters.IP, pdu);
        if (response != EnumAPIResponse.OK)
        {
            _log.LogError("Error on FillScheduleArray, get initial data");
            return (response, null);
        }

        var maxTimeBaseScheduleEntries = Convert.ToInt32(snmpPacket?.Pdu.VbList[0].Value.ToString());
        var maxDayPlans = Convert.ToInt32(snmpPacket?.Pdu.VbList[1].Value.ToString());
        var maxDayPlanEvents = Convert.ToInt32(snmpPacket?.Pdu.VbList[2].Value.ToString());
        var numActionTableEntries = Convert.ToInt32(snmpPacket?.Pdu.VbList[3].Value.ToString());
        var timeBaseScheduleTableStatus = Convert.ToInt32(snmpPacket?.Pdu.VbList[4].Value.ToString());
        var dayPlanStatus = Convert.ToInt32(snmpPacket?.Pdu.VbList[5].Value.ToString());

        _log.LogDebug("maxTimeBaseScheduleEntries:{maxTimeBaseScheduleEntries}, maxDayPlans:{maxDayPlans}, maxDayPlanEvents:{maxDayPlanEvents}, numActionTableEntries:{numActionTableEntries}, timeBaseScheduleTableStatus:{timeBaseScheduleTableStatus}, dayPlanStatus:{dayPlanStatus}", maxTimeBaseScheduleEntries, maxDayPlans, maxDayPlanEvents, numActionTableEntries, timeBaseScheduleTableStatus, dayPlanStatus);


        var listSchedule = new List<ScheduleMessageDTO>();
        var indexSchedule = 1;

        for (int i = 1; i <= maxTimeBaseScheduleEntries; i++)
        {
            var dayPlan = i;
            var index = 1;
            var actionOID = i;

            pdu = new Pdu(PduType.Get);
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.6.3.3.2.1.2.{i}");//timeBaseScheduleMonth

            (response, snmpPacket) = _commonCode.SNMPRequest(parameters.IP, pdu);
            if (response != EnumAPIResponse.OK)
            {
                _log.LogError("Error on FillScheduleArray, get time base");
                return (response, null);
            }

            var monthAux = Convert.ToInt32(snmpPacket?.Pdu.VbList[0].Value.ToString());
            if (monthAux == 0)
            {
                _log.LogInformation("dayPlan empty:{dayPlan}", dayPlan);
                continue;
            }
            
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.6.3.3.2.1.4.{i}");//timeBaseScheduleDate
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.6.3.3.2.1.3.{i}");//timeBaseScheduleDay
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.6.3.3.2.1.5.{i}");//timeBaseScheduleDayPlan
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.6.3.3.5.1.3.{dayPlan}.{index}");//dayPlanHour
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.6.3.3.5.1.4.{dayPlan}.{index}");//dayPlanMinute
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.6.3.3.5.1.5.{dayPlan}.{index}");//dayPlanActionNumberOID
            pdu.VbList.Add($"1.3.6.1.4.1.1206.4.2.3.8.2.1.2.{actionOID}");//dmsActionMsgCode


            (response, snmpPacket) = _commonCode.SNMPRequest(parameters.IP, pdu);
            if (response != EnumAPIResponse.OK)
            {
                _log.LogError("Error on FillScheduleArray, get time base");
                return (response, null);
            }

            monthAux = Convert.ToInt32(snmpPacket?.Pdu.VbList[0].Value.ToString());
            var dateAux = Convert.ToInt32(snmpPacket?.Pdu.VbList[1].Value.ToString());
            var dayAux = Convert.ToInt32(snmpPacket?.Pdu.VbList[2].Value.ToString());
            dayPlan = Convert.ToInt32(snmpPacket?.Pdu.VbList[3].Value.ToString());


            _log.LogDebug("Entrie:{index}, monthAux:{monthAux}, dateAux:{dateAux}, dayAux:{dayAux}, dayPlan:{dayPlan}", i, monthAux, dateAux, dayAux, dayPlan);

            var monthByteArray = new BitArray(new int[] { monthAux });
            var dateByteArray = new BitArray(new int[] { dateAux });

            int[] bitsMonth = monthByteArray.Cast<bool>().Select(bit => bit ? 1 : 0).ToArray();

            int[] bitsDate = dateByteArray.Cast<bool>().Select(bit => bit ? 1 : 0).ToArray();

            if (bitsMonth[0] == 0 && bitsMonth[13] == 0 && bitsMonth[14] == 0 && bitsMonth[15] == 0 && bitsDate[0] == 0 && dayPlan >= 1 && dayPlan <= maxDayPlans)
            {


                var month = Math.Log10(monthAux) / Math.Log10(2);
                var date = Math.Log10(dateAux) / Math.Log10(2);
                var day = Math.Log10(dayAux) / Math.Log10(2);


                _log.LogDebug("Month:{month}, date:{date}, day:{day}, dayPlan:{dayPlan}", month, date, day, dayPlan);

                var hour = Convert.ToInt32(snmpPacket?.Pdu.VbList[4].Value.ToString());
                var minute = Convert.ToInt32(snmpPacket?.Pdu.VbList[5].Value.ToString());
                var objectIdentifier = snmpPacket?.Pdu.VbList[6].Value.ToString();
                var lista = objectIdentifier?.Split('.');
                actionOID = Convert.ToInt32(lista?[lista.Length - 1]);


                if ((hour >= 0 && hour < 24 && minute >= 0 && minute < 60 && actionOID > 0) == false)
                {
                    break;
                }

                var fecha = new DateTime(DateTime.Now.Year, (int)month, (int)date, hour, minute, 0);

                _log.LogDebug("dayPlan:{dayPlan}, dayPlanEvent:{dayPlanEvent} hour:{hour}, minute:{minute}: actionOID{actionOID}", dayPlan, index, hour, minute, actionOID);


                _log.LogDebug("dmsActionMsgCode:{dmsActionMsgCode}", snmpPacket?.Pdu.VbList[7].Value.ToString());

                var actionMsgCodeByteArray = Global.HexToByteArray(snmpPacket?.Pdu.VbList[7].Value.ToString());
                byte memoryType;
                short messageNumber;

                if (actionMsgCodeByteArray == null)
                {
                    return (EnumAPIResponse.EXCEPTION, null);
                }

                using (var ms = new MemoryStream(actionMsgCodeByteArray))
                {
                    using var br = new BinaryReader(ms);
                    memoryType = br.ReadByte();
                    var data = br.ReadBytes(2);
                    Array.Reverse(data);
                    messageNumber = BitConverter.ToInt16(data, 0);

                }

                if (memoryType == 0 || messageNumber == 0)
                {
                    break;
                }

                var messageSchedule = new ScheduleMessageDTO
                {
                    TimeBaseScheduleNumber = i,
                    DayPlanNumber = dayPlan,
                    DayPlanEventNumber = index,
                    ActionIndex = actionOID,
                    MessageNumber = messageNumber,
                    Date = (uint)Global.ConvertToUnixTimestamp(fecha.ToUniversalTime()),
                    DateGMT = fecha.ToUniversalTime().ToString("yyyy/MM/dd HH:mm"),
                    LocalTime = fecha.ToString("yyyy/MM/dd HH:mm"),
                    Id = $"{i}.{dayPlan}.{index}.{actionOID}",
                    Index = indexSchedule

                };

                indexSchedule++;

                if (messageNumber > 0)
                {
                    var (result, message) = _commonCode.FillMessage(parameters.IP, messageNumber, EnumMemoryType.Changeable);
                    if (result == EnumAPIResponse.OK)
                    {
                        messageSchedule.MessageObject = message;
                        messageSchedule.Message = message?.Message;
                    }
                    else
                    {
                        _log.LogError("error to get message");
                    }

                }
                else
                {
                    _log.LogError("messageNumber is 0");
                }

                listSchedule.Add(messageSchedule);




            }
            //else
            //{
            //    break;
            //}
        }


        var totalMessages = listSchedule.Count;

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


        var pageListSchedule = new List<ScheduleMessageDTO>();
        for (int i = offset; i < limit; i++)
        {
            pageListSchedule.Add(listSchedule[i]);
        }

        return (EnumAPIResponse.OK, PagedList<ScheduleMessageDTO>.ToPagedList(pageListSchedule, totalMessages, parameters.PageNumber, parameters.PageSize));
    }

}
