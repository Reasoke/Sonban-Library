using System;
using System.IO;
using System.Security.Cryptography;

namespace Sonban.Common.Classes;

public static class CryptoHelper
{
    #region Encryption

    private static readonly byte[] aesKey = "63P5RtO7BZU+jhjD3tQbO8qU2ab486vb".FromBase64Bytes();
    private static readonly byte[] aesIv = "MZRNmm5AHcM=".FromBase64Bytes();

    public static string Encrypt(this string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return null;
        byte[] encrypted;
        using (var alg = TripleDES.Create())
        {
            alg.Key = aesKey;
            alg.IV = aesIv;
            var encryptor = alg.CreateEncryptor(alg.Key, alg.IV);
            using (var msEncrypt = new MemoryStream())
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using (var swEncrypt = new StreamWriter(csEncrypt))
                    swEncrypt.Write(plainText);
                encrypted = msEncrypt.ToArray();
            }
        }

        return encrypted.ToBase64();
    }

    public static string Decrypt(this string cipherText)
    {
        if (cipherText == null || cipherText.Length <= 0)
            return null;
        using var alg = TripleDES.Create();
        alg.Key = aesKey;
        alg.IV = aesIv;
        var decryptor = alg.CreateDecryptor(alg.Key, alg.IV);
        using var msDecrypt = new MemoryStream(cipherText.FromBase64Bytes());
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        var plaintext = srDecrypt.ReadToEnd();
        return plaintext;
    }

    #endregion

    public static string GetHash(this string value)
    {
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(value);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        return Convert.ToHexString(hashBytes);
    }

    #region string to byte and back extension methods

    private static byte[] FromBase64Bytes(this string str)
    {
        return string.IsNullOrEmpty(str) ? null : Convert.FromBase64String(str);
    }

    private static string ToBase64(this byte[] data)
    {
        if (data == null || data.Length == 0) return null;
        return Convert.ToBase64String(data);
    }

    #endregion
}