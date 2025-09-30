using System.Security.Cryptography;
using System.Text;
namespace Delivera.Helpers;

public class CodeGeneratorHelper
{
    public static string ShortCodeFromGuid(Guid guid, int length = 8)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(guid.ToString()));
        return BitConverter.ToString(hash).Replace("-", "").Substring(0, length);
    }

    const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public static string Base62Encode(Guid guid, int length = 10)
    {
        var value = BitConverter.ToUInt64(guid.ToByteArray(), 0); // take 64 bits
        var result = new StringBuilder();
        while (value > 0 && result.Length < length)
        {
            result.Insert(0, Alphabet[(int)(value % 62)]);
            value /= 62;
        }
        return result.ToString().PadLeft(length, '0');
    }
}