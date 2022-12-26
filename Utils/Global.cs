using System.Text.RegularExpressions;
using WebApiTraductorPMV.Dtos;

namespace WebApiTraductorPMV.Utils;

public static class Global
{
    /// <summary>
    /// Valida si una cadena de texto es una ip valida
    /// </summary>
    /// <param name="ipAddress">Dirección IP a validar</param>
    /// <returns>True si la cadena de texto es una IP valida</returns>
    public static bool IsIpAddressValid(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return false;

        string[] values = ipAddress.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

        byte ipByteValue;
        foreach (string token in values)
        {
            if (!byte.TryParse(token, out ipByteValue))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Función que convierte una cadena de texto a un array de bytes
    /// </summary>
    /// <param name="hex">Cadena de texto en formato hexadecimal</param>
    /// <returns>Array de bytes</returns>
    public static byte[]? HexToByteArray(string? hex)
    {
        try
        {
            if(hex != null)
            {
                hex = hex.Replace(" ", "");
                int NumberChars = hex.Length;
                byte[] bytes = new byte[NumberChars / 2];
                for (int i = 0; i < NumberChars; i += 2)
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                return bytes;

            }
        }
        catch (Exception)
        {
            
        }

        return null;
    }

    public static string MultiStringToString(string? multistring)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(multistring))
            {
                return "";
            }

            var lines = Regex.Split(multistring, @"(\[.+?\])");
            string msg = "";
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line) && !line.Contains('['))
                {
                    if (msg.Length > 0)
                    {
                        msg += $" {line}";
                    }
                    else
                    {
                        msg += line;
                    }
                }
            }
            return msg;
        }
        catch (Exception)
        {
            return "";
        }
    }

    public static DynamicMessageSign? MultiStringToDynamicMessageSign(string? multistring)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(multistring))
            {
                return null;
            }

            var dynamicMsg = new DynamicMessageSign();
            var newPage = new Page();
            Line? newLine = null;

            var lines = Regex.Split(multistring, @"(\[.+?\])").Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.Contains("[np]"))
                {
                    dynamicMsg.Pages.Add(newPage);
                    newPage = new Page();
                    continue;
                }

                if (line.Contains("[pt"))
                {
                    newPage.PageTime = Convert.ToInt32(line.Replace("[pt", "").Replace("o", "").Replace("]", ""));
                    continue;
                }

                if (line.Contains("[pb"))
                {
                    var aux = line.Replace("[pb", "").Replace("]", "").Split(",");
                    newPage.PageBackgroundColor = new int[] { Convert.ToInt32(aux[0]), Convert.ToInt32(aux[1]), Convert.ToInt32(aux[2]) };
                    continue;
                }

                if (line.Contains("[sc"))
                {
                    newPage.SpacingCharacter = Convert.ToInt32(line.Replace("[sc", "").Replace("]", ""));
                    continue;
                }

                if (line.Contains("[/sc]"))
                {
                    continue;
                }


                if (line.Contains("[jp"))
                {
                    newPage.JustificationPage = Convert.ToInt32(line.Replace("[jp", "").Replace("]", ""));
                    continue;
                }

                if (line.Contains("[fo"))
                {
                    newPage.Font = Convert.ToInt32(line.Replace("[fo", "").Replace("]", ""));
                    continue;
                }

                if (line.Contains("[tr"))
                {
                    var aux = line.Replace("[tr", "").Replace("]", "").Split(",");
                    newPage.TextRectangle = new int[] { Convert.ToInt32(aux[0]), Convert.ToInt32(aux[1]), Convert.ToInt32(aux[2]), Convert.ToInt32(aux[3]) };
                    continue;
                }

                if (line.Contains("[g"))
                {
                    var aux = line.Replace("[g", "").Replace("]", "").Split(",");
                    newPage.Graphic = new int[] { Convert.ToInt32(aux[0]), Convert.ToInt32(aux[1]), Convert.ToInt32(aux[2]) };
                    continue;
                }

                if (line.Contains("[cf"))
                {
                    var aux = line.Replace("[cf", "").Replace("]", "").Split(",");
                    newPage.ColorForeground = new int[] { Convert.ToInt32(aux[0]), Convert.ToInt32(aux[1]), Convert.ToInt32(aux[2]) };
                    continue;
                }

                if (line.Contains("[nl"))
                {
                    newLine = new Line();
                    newLine.NewLine = Convert.ToInt32(line.Replace("[nl", "").Replace("]", ""));
                    continue;
                }

                if (line.Contains("[jl"))
                {
                    if (newLine == null)
                    {
                        newLine = new Line();
                    }
                    newLine.JustificationLine = Convert.ToInt32(line.Replace("[jl", "").Replace("]", ""));
                    continue;
                }

                if (newLine == null)
                {
                    newLine = new Line();
                }
                newLine.Text = line;
                newPage.Lines.Add(newLine);
            }

            dynamicMsg.Pages.Add(newPage);

            return dynamicMsg;
        }
        catch (Exception)
        {
            //_log.LogError(ex, "Error on MultiStringToString");
        }

        return null;
    }

    public static DateTime ConvertFromUnixTimestamp(double timestamp)
    {
        var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return origin.AddSeconds(timestamp);
    }

    public static double ConvertToUnixTimestamp(DateTime date)
    {
        var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan diff = date.ToUniversalTime() - origin;
        return Math.Floor(diff.TotalSeconds);
    }


}
