using System.Security.Cryptography;
using System.Text;

namespace COMMON;

public static class RandomHelper
{
    //Function to get random number string
    public static string GetNumberString(int length)
    {
        var sb = new StringBuilder();
        var random = new Random();

        for (var i = 0; i < length; i++)
        {
            sb.Append(random.Next() % 10);
        }

        return sb.ToString();
    }

    // public static string GenerateUniqueId(int length)
    // {
    //     const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    //     var random = new Random();
    //     return new string(Enumerable.Repeat(chars, length)
    //         .Select(s => s[random.Next(s.Length)]).ToArray());
    // }

    public static string GenerateUniqueId(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var charArray = chars.ToCharArray();
        var id = new char[length];

        using (var rng = RandomNumberGenerator.Create())
        {
            var bytes = new byte[length];
            rng.GetBytes(bytes);

            for (var i = 0; i < length; i++)
            {
                id[i] = charArray[bytes[i] % charArray.Length];
            }
        }

        return new string(id);
    }
}