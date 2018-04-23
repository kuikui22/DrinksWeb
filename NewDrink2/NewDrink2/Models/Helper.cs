using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace NewDrink2.Models
{
    public class Helper
    {
        //取得使用者姓名
        public static string GetUserName()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                FormsIdentity id = HttpContext.Current.User.Identity as FormsIdentity;
                FormsAuthenticationTicket ticket = id.Ticket;
                var userInfo = id.Ticket.Name;
                return userInfo;
            }

            return "";
        }

        //取得使用者Email
        public static string GetUserMail()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                FormsIdentity id = HttpContext.Current.User.Identity as FormsIdentity;
                FormsAuthenticationTicket ticket = id.Ticket;
                string[] name = id.Ticket.UserData.Split(new char[] { ',' });
                return name[0];
            }

            return "";
        }

        //取得使用者身分成員/管理者
        public static string GetUserIdentity()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                FormsIdentity id = HttpContext.Current.User.Identity as FormsIdentity;
                FormsAuthenticationTicket ticket = id.Ticket;
                string[] name = id.Ticket.UserData.Split(new char[] { ',' });
                return name[1];
            }

            return "";
        }

        public static string GetUserIdentity2()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                FormsIdentity id = HttpContext.Current.User.Identity as FormsIdentity;
                FormsAuthenticationTicket ticket = id.Ticket;
                string[] name = id.Ticket.UserData.Split(new char[] { ',' });
                return name[2];
            }

            return "";
        }

        //取得使用者權限
        public static string GetUserBuyDrink()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                FormsIdentity id = HttpContext.Current.User.Identity as FormsIdentity;
                FormsAuthenticationTicket ticket = id.Ticket;
                string[] name = id.Ticket.UserData.Split(new char[] { ',' });
                return name[3];
            }

            return "";
        }

        public static string GetUserOrderSet()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                FormsIdentity id = HttpContext.Current.User.Identity as FormsIdentity;
                FormsAuthenticationTicket ticket = id.Ticket;
                string[] name = id.Ticket.UserData.Split(new char[] { ',' });
                return name[4];
            }

            return "";
        }

        public static string GetUserMessage()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                FormsIdentity id = HttpContext.Current.User.Identity as FormsIdentity;
                FormsAuthenticationTicket ticket = id.Ticket;
                string[] name = id.Ticket.UserData.Split(new char[] { ',' });
                return name[5];
            }

            return "";
        }

        public static string GetUserCallnotice()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                FormsIdentity id = HttpContext.Current.User.Identity as FormsIdentity;
                FormsAuthenticationTicket ticket = id.Ticket;
                string[] name = id.Ticket.UserData.Split(new char[] { ',' });
                return name[6];
            }

            return "";
        }

        public static string GetUserChangePsd()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                FormsIdentity id = HttpContext.Current.User.Identity as FormsIdentity;
                FormsAuthenticationTicket ticket = id.Ticket;
                string[] name = id.Ticket.UserData.Split(new char[] { ',' });
                return name[7];
            }

            return "";
        }

        public static string GetUserMyUserSet()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                FormsIdentity id = HttpContext.Current.User.Identity as FormsIdentity;
                FormsAuthenticationTicket ticket = id.Ticket;
                string[] name = id.Ticket.UserData.Split(new char[] { ',' });
                return name[8];
            }

            return "";
        }

        public static string GetUserMyMenuSet()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                FormsIdentity id = HttpContext.Current.User.Identity as FormsIdentity;
                FormsAuthenticationTicket ticket = id.Ticket;
                string[] name = id.Ticket.UserData.Split(new char[] { ',' });
                return name[9];
            }

            return "";
        }

    }
}