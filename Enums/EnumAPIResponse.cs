﻿namespace WebApiTraductorPMV.Enums;

public enum EnumAPIResponse
{
    OK = 0,
    NAME_ALREDY_EXIST,
    ERROR_WRONG_DATA,
    ERROR_INVALID_MODEL,
    ERROR_EMAIL_ALREADY_EXIST,
    INAVLID_LOGIN_ATTEMPT,
    NETWORK_EXCEPTION,
    NO_RESPONSE_RECEIVED_FROM_SNMP_AGENT,
    ERROR_IN_SNMP_REPLY,
    WRONG_MESSAGE_ID,
    WRONG_FONT_ID,
    WRONG_GRAPHIC_ID,
    WRONG_GRAPHIC_PAGE,
    WRONG_MESSAGE_PAGE,
    MESSAGE_ERROR_OTHER,
    MESSAGE_ERROR_BEACONS,
    MESSAGE_ERROR_PIXEL_SERVICE,
    MESSAGE_ERROR_SYNTAXMULTI_OTHER,
    MESSAGE_ERROR_SYNTAXMULTI_UNSUPPORTED_TAG,
    MESSAGE_ERROR_SYNTAXMULTI_UNSUPPORTED_TAG_VALUE,
    MESSAGE_ERROR_SYNTAXMULTI_TEXT_TOO_BIG,
    MESSAGE_ERROR_SYNTAXMULTI_FONT_NOT_DEFINED,
    MESSAGE_ERROR_SYNTAXMULTI_CHARACTER_NOT_DEFINED,
    MESSAGE_ERROR_SYNTAXMULTI_FIELD_DEVICE_NOT_EXIST,
    MESSAGE_ERROR_SYNTAXMULTI_FIELD_DEVICE_ERROR,
    MESSAGE_ERROR_SYNTAXMULTI_FLASH_REGION_ERROR,
    MESSAGE_ERROR_SYNTAXMULTI_TAG_CONFLICT,
    MESSAGE_ERROR_SYNTAXMULTI_TOO_MANY_PAGES,
    MESSAGE_ERROR_SYNTAXMULTI_FONT_VERSION_ID,
    MESSAGE_ERROR_SYNTAXMULTI_GRAPHIC_ID,
    MESSAGE_ERROR_SYNTAXMULTI_GRAPHIC_NOT_DEFINED,
    WRONG_DATE_TIME,
    WRONG_SCHEDULE_ID,
    LIMIT_EXCEEDED_SCHEDULE_ITEMS,
    EXCEPTION = 106


}
