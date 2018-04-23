using NewDrink2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace NewDrink2.Controllers
{
    public class AccountController : Controller
    {
        private NewDrinkDB db = new NewDrinkDB();

        // GET: Account
        public ActionResult Index(string NotUsingMessage)
        {
            if (!String.IsNullOrEmpty(NotUsingMessage))
            {
                ViewBag.NotUsingMessage = NotUsingMessage;
            }

            Session.RemoveAll();
            FormsAuthentication.SignOut();

            return View();
        }

        // POST: Account
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Index(LoginViewModel peo)
        {
            if (!ModelState.IsValid)
            {
                return View(peo);
            }

            //解密密碼,判斷使用者
            string Psd = Models.AccountModels.UserPsd(peo.Email);

            if (String.IsNullOrWhiteSpace(Psd))
            {
                ViewBag.Error = "輸入錯誤,查無此帳號!";
                return View(peo);
            }
            else if (Psd != peo.Password)
            {
                ViewBag.Error = "密碼輸入錯誤";
                return View(peo);
            }

            //先做session清除
            Session.RemoveAll();
            FormsAuthentication.SignOut();
            LogOff();

            //將使用者的EMail,Name,Identity,權限存進Sessiion中
            //存權限的順序依照資料庫
            int UserID = Models.AccountModels.UserID(peo.Email);
            User query = db.Users.Find(UserID);
            Identity query2 = db.Identities.Find(UserID);
            UserCanDo query3 = db.UserCanDoes.Find(UserID);

            string userdata = query.Email + "," + query.Identity + "," + query2.Identity2 + "," + query3.BuyDrink + "," + query3.OrderSet + "," + query3.Message + "," + query3.Callnotice + "," + query3.ChangePsd + "," + query3.MyUserSet + "," + query3.MyMenuSet;
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1,
                      query.Name,
                      DateTime.Now,
                      DateTime.Now.AddMinutes(30),
                      true,
                      userdata,
                      FormsAuthentication.FormsCookiePath);

            string encTicket = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            cookie.HttpOnly = true;
            Response.Cookies.Add(cookie);

            //判斷使用者為會員或管理者
            if (query.Identity == 1)
            {
                //進入首頁
                return RedirectToAction("ManagerHome", "Home");
            }

            //進入首頁
            return RedirectToAction("Index", "Home");
        }

        //登出
        [HttpPost]
        public ActionResult LogOff()
        {
            Session.Abandon();

            FormsAuthentication.SignOut();
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            cookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie);

            Session.RemoveAll();
            Session.Abandon();

            return RedirectToAction("Index", "Account");
        }

        //揪團成員循著網址找到此方法
        //GET: Account/Get_MyOrder
        public ActionResult Get_MyOrder(string ID)
        {
            if (String.IsNullOrEmpty(ID))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //確認截止訂單(將過期的訂單結止)
            Models.HomeModels.HaveEndOrder();

            //先將空白字服替換成+符
            ID = ID.Replace(" ", "+");

            //以Guid取出資料
            var Check = db.SendMessageViews.Find(ID);
            if (Check == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //取出使用者ID及orderID
            var User = db.Users.Find(Check.UserID);
            var Order = db.CreateBuyOrder_LeaderOrders.Find(Check.OrderID);

            if (User == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //判斷訂單是否已經結束
            if (Order == null || Order.CanOrNotOrder == 1 || Order.CanOrNotOrder == 2)
            {
                //將訂單設為已讀
                Check.ReadOrNot = true;
                db.SaveChanges();
                TempData["message"] = "這筆訂單已經結束訂購囉!";
                return RedirectToAction("MessageView_Read", "Home");
            }

            //將使用者的EMail,Name,Identity,權限存進Sessiion中
            //存權限的順序依照資料庫
            Identity query2 = db.Identities.Find(User.ID);
            UserCanDo query3 = db.UserCanDoes.Find(User.ID);
            string userdata = User.Email + "," + User.Identity + "," + query2.Identity2 + "," + query3.BuyDrink + "," + query3.OrderSet + "," + query3.Message + "," + query3.Callnotice + "," + query3.ChangePsd + "," + query3.MyUserSet + "," + query3.MyMenuSet;
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1,
                      User.Name,
                      DateTime.Now,
                      DateTime.Now.AddMinutes(30),
                      true,
                      userdata,
                      FormsAuthentication.FormsCookiePath);

            string encTicket = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            cookie.HttpOnly = true;
            Response.Cookies.Add(cookie);

            //將資料設為已讀
            ////有效期限為兩小時
            //var time = DateTime.Now;            

            //DateTime start = Convert.ToDateTime(Check.SendTime);
            //DateTime end = Convert.ToDateTime(time);

            ////判斷是否同一天
            //string sDay = start.ToString("yyyyMMdd");
            //string eDay = end.ToString("yyyyMMdd");
            //if (sDay != eDay)
            //{
            //    Check.ReadOrNot = true;
            //    db.SaveChanges();

            //    TempData["message"] = "這筆邀請已經過期囉!";
            //    return RedirectToAction("MessageView_Read", "Home");
            //}

            ////先判斷上下午
            //string W = start.ToString("tt");
            //string N = end.ToString("tt");

            //string ss = start.ToString("HH");
            //string ee = end.ToString("HH");

            //if (W == "下午" && ss == "00")
            //{
            //    ss = "22";
            //}
            //if (W == "上午" && ss == "00")
            //{
            //    ss = "12";
            //}

            //int stime = Int32.Parse(ss);
            //int etime = Int32.Parse(ee);

            //var dateH = etime - stime;

            ////TimeSpan ts = end.Subtract(start);
            ////var dateH = ts.Hours;   

            //if (dateH >= 0 && (dateH < 2 || dateH == 0))
            //{
            //    Check.ReadOrNot = true;
            //    db.SaveChanges();
            //}
            //else
            //{
            //    Check.ReadOrNot = true;
            //    TempData["message"] = "這筆邀請已經超過2小時囉!";
            //    return RedirectToAction("MessageView_Read", "Home");
            //}

            Check.ReadOrNot = true;
            db.SaveChanges();

            //導入加點飲料介面
            return RedirectToAction("DMDetail", "AnyDMorBuy", new { id = Order.MenuID, BuyCheck = Check.OrderID });
        }

        //團長循著網址找到此方法
        //GET: Account/Get_MyOrder
        public ActionResult Get_LeaderMyOrder(string ID)
        {
            if (String.IsNullOrEmpty(ID))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //先將空白字服替換成+符
            ID = ID.Replace(" ", "+");

            //以Guid取出資料
            var Check = db.LeaderSendMessages.Find(ID);
            if (Check == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //取出使用者ID及orderID
            var User = db.Users.Find(Check.SentUser);
            var Order = db.CreateBuyOrder_LeaderOrders.Find(Check.OrderID);

            if (User == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //判斷訂單是否已經結束
            if (Order == null || Order.CanOrNotOrder == 1 || Order.CanOrNotOrder == 2)
            {
                TempData["message"] = "這筆訂單已經結束訂購囉!";
                return RedirectToAction("MessageView_Sent", "Home");              
            }

            //將使用者的EMail,Name,Identity,權限存進Sessiion中
            //存權限的順序依照資料庫
            Identity query2 = db.Identities.Find(User.ID);
            UserCanDo query3 = db.UserCanDoes.Find(User.ID);
            string userdata = User.Email + "," + User.Identity + "," + query2.Identity2 + "," + query3.BuyDrink + "," + query3.OrderSet + "," + query3.Message + "," + query3.Callnotice + "," + query3.ChangePsd + "," + query3.MyUserSet + "," + query3.MyMenuSet;
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1,
                      User.Name,
                      DateTime.Now,
                      DateTime.Now.AddMinutes(30),
                      true,
                      userdata,
                      FormsAuthentication.FormsCookiePath);

            string encTicket = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            cookie.HttpOnly = true;
            Response.Cookies.Add(cookie);

            //導入加點飲料介面
            return RedirectToAction("DMDetail", "AnyDMorBuy", new { id = Order.MenuID, BuyCheck = Check.OrderID });
        }





        //關閉連線
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}