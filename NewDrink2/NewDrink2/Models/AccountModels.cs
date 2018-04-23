using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace NewDrink2.Models
{
    public class AccountModels
    {
        private static AesCryptoServiceProvider aes = new AesCryptoServiceProvider();

        //解密密碼,判斷使用者
        public static string UserPsd(string UserEMail)
        {
            string Psd = "";
            NewDrinkDB db = new NewDrinkDB();

            var queryLinQ = db.Users.Where((m) => m.Email == UserEMail).Select(m => new { Psd = m.Password });
            foreach (var item in queryLinQ)
            {
                Psd = item.Psd;
            }

            if (queryLinQ.FirstOrDefault() == null)
            {
                //如果找不到使用者則回傳
                Psd = null;
                return Psd;
            }

            //解密密碼
            Psd = PasswordUtility.AESDecryptor(Psd, aes.Key, aes.IV);

            db.Dispose();

            return Psd;
        }

        //取出使用者的ID
        public static int UserID(string UserEMail)
        {
            int ID = 0;
            NewDrinkDB db = new NewDrinkDB();
            var queryLinQ = db.Users.Where((m) => m.Email == UserEMail).Select(m => new { ID = m.ID });
            foreach (var item in queryLinQ)
            {
                ID = item.ID;
            }
            db.Dispose();

            return ID;
        }

    }
}