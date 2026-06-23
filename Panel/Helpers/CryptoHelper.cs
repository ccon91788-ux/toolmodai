using System.Security.Cryptography;
using System.Text;

namespace Panel.Helpers;

/// <summary>
/// Mã hoá/giải mã TripleDES + MD5 key "ud" — cùng thuật toán với AutoLoginCL phía client.
/// </summary>
public static class CryptoHelper
{
    private const string Key = "ud";
    private static byte[]? _keyCache;

    private static byte[] GetKeyHash()
    {
        return _keyCache ??= MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Key));
    }

    /// <summary>Mã hoá password → base64 (gửi qua command line args).</summary>
    public static string EncryptPassword(string plainText)
    {
        try
        {
            using var des = new TripleDESCryptoServiceProvider
            {
                Key = GetKeyHash(),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = des.CreateEncryptor().TransformFinalBlock(data, 0, data.Length);
            return Convert.ToBase64String(encrypted);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>Giải mã base64 → plain text.</summary>
    public static string DecryptPassword(string encrypted)
    {
        if (string.IsNullOrEmpty(encrypted)) return string.Empty;

        try
        {
            byte[] data = Convert.FromBase64String(encrypted);
            using var des = new TripleDESCryptoServiceProvider
            {
                Key = GetKeyHash(),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            byte[] decrypted = des.CreateDecryptor().TransformFinalBlock(data, 0, data.Length);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            return encrypted; // fallback for legacy plaintext passwords
        }
    }
}

