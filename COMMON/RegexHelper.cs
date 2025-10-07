using System.Net.Mail;
using System.Text.RegularExpressions;

namespace COMMON;

public static class RegexHelper
{
    #region Елхат анықтау   +IsEmail(string email)

    static public bool IsEmail(string email)
    {
        try
        {
            var m = new MailAddress(email);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    #endregion

    #region Латын әріпін анықтау +IsLatinString(string str)

    public static bool IsLatinString(string str)
    {
        return Regex.IsMatch(str, @"^[a-zA-Z0-9-]+$");
    }

    #endregion

    #region Жалғаным екенін анықтау + IsUrl(string str)

    public static bool IsUrl(string addressString)
    {
        Uri result = null;
        return Uri.TryCreate(addressString, UriKind.RelativeOrAbsolute, out result);
    }

    #endregion

    #region Кілт тізбегін анықтау +IsLocalString(string str)

    public static bool IsLocalString(string str)
    {
        if (str.Length < 4) return false;
        if (!str.Substring(0, 3).Equals("ls_")) return false;
        return Regex.IsMatch(str, @"^[a-zA-Z0-9_]+$");
    }

    #endregion

    static public bool IsPhoneNumber(string phone, out string phoneNumber)
    {
        phoneNumber = string.Empty;
    
        // 移除所有非数字和非+号的字符（包括空格、特殊字符等）
        phone = Regex.Replace(phone ?? "", @"[^\d+]", "").Trim();

        if (string.IsNullOrEmpty(phone))
            return false;

        // 处理以1开头且长度为11位的号码（修正：应该是+1而不是+86）
        if (phone.StartsWith("1") && phone.Length == 11)
        {
            phoneNumber = "+" + phone;
            return true;
        }

        // 处理以7开头且长度为11位的号码（俄罗斯号码带国家代码）
        if (phone.StartsWith("7") && phone.Length == 11)
        {
            phoneNumber = "+" + phone;
            return true;
        }

        // 处理以7开头且长度为10位的号码（俄罗斯号码去掉国家代码）
        if (phone.StartsWith("7") && phone.Length == 10)
        {
            phoneNumber = "+7" + phone;
            return true;
        }

        // 处理以77开头且长度为11位的号码（哈萨克斯坦号码）
        // 注意：这个条件放在7开头11位之后，避免冲突
        if (phone.StartsWith("77") && phone.Length == 11)
        {
            phoneNumber = "+" + phone;
            return true;
        }

        // 处理以87开头且长度为11位的号码（转换为+7格式）
        if (phone.StartsWith("87") && phone.Length == 11)
        {
            phoneNumber = "+7" + phone.Substring(1);
            return true;
        }

        // 处理已经带有国家代码的完整号码
        if ((phone.StartsWith("+861") && phone.Length == 14) || 
            (phone.StartsWith("+77") && phone.Length == 12))
        {
            phoneNumber = phone;
            return true;
        }

        // 其他所有号码（包括9开头或任何其他格式）直接返回true，不做格式化
        if (phone.Length > 0)
        {
            phoneNumber = phone.StartsWith("+") ? phone : "+" + phone;
            return true;
        }

        return false;
    }

    #region Түс мәнінің дұрыстығын анықтау   +IsHexColorString(string str)

    static public bool IsHexColorString(string str)
    {
        return Regex.IsMatch(str, @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$");
    }

    #endregion
}