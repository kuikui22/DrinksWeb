using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace NewDrink2.Models
{
    //密碼加解密
    public class PasswordUtility
    {
        public static string AESEncryptor(string plainText, byte[] Key, byte[] IV)
        {
            Key = Encoding.ASCII.GetBytes("1234567812345678");
            IV = Encoding.ASCII.GetBytes("8765432187654321");
            byte[] data = ASCIIEncoding.ASCII.GetBytes(plainText);
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            string encryptedString = Convert.ToBase64String(aes.CreateEncryptor(Key, IV).TransformFinalBlock(data, 0, data.Length));
            return encryptedString;
        }

        internal static string AESDecryptor(string psd, object key, object iV)
        {
            throw new NotImplementedException();
        }

        public static string AESDecryptor(string encryptedString, byte[] Key, byte[] IV)
        {
            Key = Encoding.ASCII.GetBytes("1234567812345678");
            IV = Encoding.ASCII.GetBytes("8765432187654321");
            byte[] data = Convert.FromBase64String(encryptedString);
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            string decryptedString =
                ASCIIEncoding.ASCII.GetString(aes.CreateDecryptor(Key, IV).TransformFinalBlock(data, 0, data.Length));
            return decryptedString;
        }
    }
}