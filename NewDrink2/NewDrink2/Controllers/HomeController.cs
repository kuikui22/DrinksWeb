using NewDrink2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Security.Cryptography;
using System.Net;

namespace NewDrink2.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private NewDrinkDB db = new NewDrinkDB();

        //GET: ManagerHome
        public ActionResult ManagerHome()
        {
            return View();
        }

        //GET: Home
        public ActionResult Index()
        {
            string UserMail = Models.Helper.GetUserName();
            if (String.IsNullOrWhiteSpace(UserMail))
            {
                Session.RemoveAll();
                FormsAuthentication.SignOut();
                return RedirectToAction("Index", "Account");
            }

            string BuyDrink = Helper.GetUserBuyDrink();
            string OrderSet = Helper.GetUserOrderSet();
            string Message = Helper.GetUserMessage();
            string Callnotice = Helper.GetUserCallnotice();
            string ChangePsd = Helper.GetUserChangePsd();
            string[] UserLimit = { BuyDrink, OrderSet, Message, Callnotice, ChangePsd };
            ViewBag.UserLimit = UserLimit;

            //確認截止訂單(將過期的訂單結止)
            Models.HomeModels.HaveEndOrder();

            var result = db.IndexImages.Find(1);
            return View(result);
        }

        //GET: Home/ChangePsd
        public ActionResult ChangePsd()
        {
            string BuyDrink = Helper.GetUserBuyDrink();
            string OrderSet = Helper.GetUserOrderSet();
            string Message = Helper.GetUserMessage();
            string Callnotice = Helper.GetUserCallnotice();
            string ChangePsd = Helper.GetUserChangePsd();
            string[] UserLimit = { BuyDrink, OrderSet, Message, Callnotice, ChangePsd };
            ViewBag.UserLimit = UserLimit;
            return View();
        }

        //POST: Home/ChangePsd
        [HttpPost]
        public ActionResult ChangePsd(ChangePsdView chang)
        {
            if (!ModelState.IsValid)
            {
                return View(chang);
            }
            string UserMail = Helper.GetUserMail();
            int UserID = Models.AccountModels.UserID(UserMail);

            //先確認舊密碼是否正確
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();

            NewDrinkDB db = new NewDrinkDB();

            User query = db.Users.Find(UserID);
            string Psd = PasswordUtility.AESDecryptor(query.Password, aes.Key, aes.IV);
            if (Psd != chang.OldPassword)
            {
                ViewBag.Error = "密碼輸入錯誤";
                return View(chang);
            }

            string NewPsd = PasswordUtility.AESEncryptor(chang.Password, aes.Key, aes.IV);
            var result = db.Database.ExecuteSqlCommand(@"UPDATE users SET Password = '" + NewPsd + "', ConfirmPsd = '" + NewPsd + "' Where ID = '" + UserID + "';");

            //關閉連線
            db.Dispose();

            string BuyDrink = Helper.GetUserBuyDrink();
            string OrderSet = Helper.GetUserOrderSet();
            string Message = Helper.GetUserMessage();
            string Callnotice = Helper.GetUserCallnotice();
            string ChangePsd = Helper.GetUserChangePsd();
            string[] UserLimit = { BuyDrink, OrderSet, Message, Callnotice, ChangePsd };
            ViewBag.UserLimit = UserLimit;
            TempData["message"] = "success";

            return RedirectToAction("Index", "Home");
        }

        //GET: Home/MessageView_New
        public ActionResult MessageView_New()
        {
            //取出使用者ID
            string UserMail = Helper.GetUserMail();
            int UserID = Models.AnyDMorBuy.GetUserID_UseEmail(UserMail);
            var time = DateTime.Now;

            //確認截止訂單(將過期的訂單結止)
            Models.HomeModels.HaveEndOrder();

            var resultCheck = db.SendMessageViews.Where(m => m.UserID == UserID && m.ReadOrNot == false);
            var EndresultAndresult = new List<SendMessageView>();

            //判斷是否有資料超過三天,超過則刪除
            foreach (var item in resultCheck)
            {
                string NowDay = time.ToString("yyyyMM");
                string SendDay = item.SendTime.ToString("yyyyMM");

                //如果不同年月則刪除
                if (NowDay != SendDay)
                {
                    var De = db.SendMessageViews.Find(item.ID);
                    db.SendMessageViews.Remove(De);
                    db.SaveChanges();
                }

                //如果超過三天則刪除
                string NowDay2 = time.ToString("dd");
                string SendDay2 = item.SendTime.ToString("dd");
                int nowD = Int32.Parse(NowDay2);
                int SedD = Int32.Parse(SendDay2);

                if (nowD - SedD >= 3)
                {
                    var De = db.SendMessageViews.Find(item.ID);
                    db.SendMessageViews.Remove(De);
                    db.SaveChanges();
                }
            }
            var db2 = new NewDrinkDB();
            var result = db2.SendMessageViews.Where(m => m.UserID == UserID && m.ReadOrNot == false);

            if (result.Count() > 0)
            {
                //由於加入無有效日期的要素,將未設定定時關閉的訂單的有效日期隱藏
                foreach (var item in result)
                {
                    var MyStopOrder = db.CreateBuyOrder_LeaderOrders.Find(item.OrderID);                    
                    if (MyStopOrder.CheckEnd == false)
                    {
                        var str = Convert.ToDateTime("1999-01-01 00:00:00");
                        item.EndOverTime = str;
                    }

                    EndresultAndresult.Add(new SendMessageView()
                    {
                        ID = item.ID,
                        UserID = item.UserID,
                        OrderID = item.OrderID,
                        SentSubject = item.SentSubject,
                        SentBody = item.SentBody,
                        SentUser = item.SentUser,
                        SentAlink = item.SentAlink,
                        SendTime = item.SendTime,
                        EndOverTime = item.EndOverTime,
                        ReadOrNot = item.ReadOrNot
                    });

                }

                //將資料遞減排序
                result = result.OrderBy(m => m.UserID).ThenByDescending(m => m.SendTime);
            }

            //將取消通知取出
            var EndMessage = db.OrderEndingSends.Where(m => m.ToUser == UserID && m.ReadOrNot == false);
            var Endresult = new List<SendMessageView>();
            if (EndMessage.FirstOrDefault() != null)
            {
                //判斷是否有資料超過三天,超過則刪除
                foreach (var item in Endresult)
                {
                    string NowDay = time.ToString("yyyyMM");
                    string SendDay = item.SendTime.ToString("yyyyMM");

                    //如果不同年月則刪除
                    if (NowDay != SendDay)
                    {
                        var De = db.OrderEndingSends.Find(item.ID);
                        db.OrderEndingSends.Remove(De);
                        db.SaveChanges();
                    }

                    //如果超過三天則刪除
                    string NowDay2 = time.ToString("dd");
                    string SendDay2 = item.SendTime.ToString("dd");
                    int nowD = Int32.Parse(NowDay2);
                    int SedD = Int32.Parse(SendDay2);

                    if (nowD - SedD >= 3)
                    {
                        var De = db.OrderEndingSends.Find(item.ID);
                        db.OrderEndingSends.Remove(De);
                        db.SaveChanges();
                    }
                }

                foreach (var item in EndMessage)
                {
                    var EndOrCancel = "";
                    if (item.EndOrCancel == 0)
                    {
                        EndOrCancel = "取消";
                    }
                    if (item.EndOrCancel == 1)
                    {
                        EndOrCancel = "結束";
                    }

                    var id = item.ID.ToString();
                    var EndConbin = db2.LeaderSendMessages.Where(m => m.OrderID == item.OrderID);
                    foreach (var Enditem in EndConbin)
                    {
                        var LeaderN = db2.Users.Find(Enditem.SentUser);
                        var Msg = LeaderN.Name + "於" + Enditem.EndOverTime + EndOrCancel + "了訂單.";
                        EndresultAndresult.Add(new SendMessageView()
                        {
                            ID = id,
                            OrderID = item.OrderID,
                            SentSubject = Enditem.Subject,
                            SentBody = Msg,
                            SentAlink = EndOrCancel,
                            SendTime = Enditem.EndOverTime,
                            ReadOrNot = item.ReadOrNot
                        });
                    }

                }

            }

            if (EndMessage.FirstOrDefault() == null && result.FirstOrDefault() == null)
            {
                ViewBag.NoMsg = "目前尚無任何訊息.";
            }

            EndresultAndresult = EndresultAndresult.OrderByDescending(m => m.SendTime).ToList();

            return View(EndresultAndresult);
        }

        //GET: Home/MessageView_Read
        public ActionResult MessageView_Read()
        {
            //取出使用者ID
            string UserMail = Helper.GetUserMail();
            int UserID = Models.AnyDMorBuy.GetUserID_UseEmail(UserMail);

            //確認截止訂單(將過期的訂單結止)
            Models.HomeModels.HaveEndOrder();

            var result = db.SendMessageViews.Where(m => m.UserID == UserID && m.ReadOrNot == true);
            var EndresultAndresult = new List<SendMessageView>();

            result = result.OrderBy(m => m.UserID).ThenBy(m => m.SendTime);
            if (result.Count() > 0)
            {
                //當取出資料大逾4筆時刪掉最舊的那個
                while (result.Count() > 4)
                {
                    foreach (var item in result)
                    {
                        var dele = db.SendMessageViews.Find(item.ID);
                        db.SendMessageViews.Remove(dele);
                        db.SaveChanges();
                        break;
                    }
                }

                //由於加入了回覆不訂購的要素,所以要將自己回覆不訂購的訂單藏起來
                //先找出自己不回覆的訂單(到時到前台則不顯示)
                var MySotpOrder = db.CreateBuyOrder_MemberOrders.Where(m => m.UserID == UserID && m.Together == false);
                foreach (var item in MySotpOrder)
                {
                    foreach (var resultitem in result)
                    {
                        if (item.OrderID == resultitem.OrderID)
                        {
                            //找到死亡的訂單移除連結
                            resultitem.SentAlink = "";
                            break;
                        }
                    }
                }

                //由於加入無有效日期的要素,將未設定定時關閉的訂單的有效日期隱藏
                foreach (var item in result)
                {
                    var MyStopOrder = db.CreateBuyOrder_LeaderOrders.Find(item.OrderID);
                    if (MyStopOrder.CheckEnd == false)
                    {
                        var str = Convert.ToDateTime("1999-01-01 00:00:00");
                        item.EndOverTime = str;
                    }

                    EndresultAndresult.Add(new SendMessageView()
                    {
                        ID = item.ID,
                        UserID = item.UserID,
                        OrderID = item.OrderID,
                        SentSubject = item.SentSubject,
                        SentBody = item.SentBody,
                        SentUser = item.SentUser,
                        SentAlink = item.SentAlink,
                        SendTime = item.SendTime,
                        EndOverTime = item.EndOverTime,
                        ReadOrNot = item.ReadOrNot
                    });
                }


                //將資料遞減排序
                result = result.OrderBy(m => m.UserID).ThenByDescending(m => m.SendTime);
            }

            //將取消通知取出
            var EndMessage = db.OrderEndingSends.Where(m => m.ToUser == UserID && m.ReadOrNot == true);
            EndMessage = EndMessage.OrderBy(m => m.EndOverTime);
            if (EndMessage.FirstOrDefault() != null)
            {
                //當取出資料大逾4筆時刪掉最舊的那個
                while (EndMessage.Count() > 4)
                {
                    foreach (var item in EndMessage)
                    {
                        var dele = db.OrderEndingSends.Find(item.ID);
                        db.OrderEndingSends.Remove(dele);
                        db.SaveChanges();
                        break;
                    }
                }

                foreach (var item in EndMessage)
                {
                    var EndOrCancel = "";
                    if (item.EndOrCancel == 0)
                    {
                        EndOrCancel = "取消";
                    }
                    if (item.EndOrCancel == 1)
                    {
                        EndOrCancel = "結束";
                    }

                    var id = item.ID.ToString();
                    var EndConbin = db.LeaderSendMessages.Where(m => m.OrderID == item.OrderID);
                    foreach (var Enditem in EndConbin)
                    {                       
                        var LeaderN = db.Users.Find(Enditem.SentUser);
                        var Msg = LeaderN.Name + "於" + Enditem.EndOverTime + EndOrCancel + "了訂單.";
                        EndresultAndresult.Add(new SendMessageView()
                        {
                            ID = id,
                            OrderID = item.OrderID,
                            SentSubject = Enditem.Subject,
                            SentBody = Msg,
                            SentAlink = EndOrCancel,
                            SendTime = Enditem.EndOverTime,
                            ReadOrNot = item.ReadOrNot
                        });
                    }
                }
            }//EndIF

            if (EndMessage.FirstOrDefault() == null && result.FirstOrDefault() == null)
            {
                ViewBag.NoMsg = "目前尚無任何訊息.";
            }

            EndresultAndresult = EndresultAndresult.OrderByDescending(m => m.SendTime).ToList();
            return View(EndresultAndresult);
        }

        //GET: Home/MessageView_Sent
        public ActionResult MessageView_Sent()
        {
            //取出使用者ID
            string UserMail = Helper.GetUserMail();
            int UserID = Models.AnyDMorBuy.GetUserID_UseEmail(UserMail);

            //確認截止訂單(將過期的訂單結止)
            Models.HomeModels.HaveEndOrder();

            //當取出資料大逾10筆時刪掉最舊的那個
            var result = db.LeaderSendMessages.Where(m => m.SentUser == UserID);
            result = result.OrderBy(m => m.SentUser).ThenBy(m => m.SendTime);
            if (result.FirstOrDefault() != null)
            {
                if (result.Count() > 0)
                {
                    while (result.Count() > 10)
                    {
                        foreach (var item in result)
                        {
                            var del = db.LeaderSendMessages.Find(item.ID);
                            db.LeaderSendMessages.Remove(del);
                            db.SaveChanges();
                            break;
                        }
                    }

                    //由於加入無有效日期的要素,將未設定定時關閉的訂單的有效日期隱藏
                    foreach (var item in result)
                    {
                        var MyStopOrder = db.CreateBuyOrder_LeaderOrders.Find(item.OrderID);
                        if (MyStopOrder.CheckEnd == false)
                        {
                            var str = Convert.ToDateTime("1999-01-01 00:00:00");
                            item.EndOverTime = str;
                        }
                    }

                    //將資料遞減排序
                    result = result.OrderBy(m => m.SentUser).ThenByDescending(m => m.SendTime);
                }
            }           
            else
            {
                ViewBag.NoMsg = "目前尚無寄送任何訊息.";
            }
            
            return View(result);

        }

        //GET: Home/CheckRead_Button
        public ActionResult CheckRead_Button(string checkID)
        {
            var query = db.SendMessageViews.Find(checkID);
            if (query == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            query.ReadOrNot = true;

            db.SaveChanges();
            return RedirectToAction("MessageView_New", "Home");
        }

        //GET: Home/CheckNotTogether_Button
        public ActionResult CheckNotTogether_Button(string checkID)
        {
            var query = db.SendMessageViews.Find(checkID);
            if (query == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }            
            //將訊息更改為已讀
            query.ReadOrNot = true;

            //修改成員狀態Together = false
            //取出使用者ID
            string UserMail = Helper.GetUserMail();
            var UserID = Models.AnyDMorBuy.GetUserID_UseEmail(UserMail);            
            var MemCon = db.CreateBuyOrder_MemberOrders.Where(m => m.OrderID == query.OrderID && m.UserID == UserID);
            foreach (var item in MemCon)
            {
                item.Together = false;
            }

            db.SaveChanges();
            
            return RedirectToAction("MessageView_New", "Home");
        }

        //GET: Home/CheckNotTogether_DetailDM
        public ActionResult CheckNotTogether_DetailDM(string OrderID)
        {
            //取出使用者ID
            string UserMail = Helper.GetUserMail();
            var UserID = Models.AnyDMorBuy.GetUserID_UseEmail(UserMail);
            int orderID = Int32.Parse(OrderID);
            //用訂單尋找使用者的訊息並將訊息更改為已讀
            var MemMessage = db.SendMessageViews.Where(m => m.OrderID == orderID && m.UserID == UserID);
            foreach (var item in MemMessage)
            {
                item.ReadOrNot = true;
            }

            //修改成員狀態Together = false
            var MemCon = db.CreateBuyOrder_MemberOrders.Where(m => m.OrderID == orderID && m.UserID == UserID);
            foreach (var item in MemCon)
            {
                item.Together = false;
            }

            db.SaveChanges();

            return RedirectToAction("MessageView_New", "Home");
        }


        //GET: Home/SettingHome
        public ActionResult SettingHome()
        {
            string UserMail = Models.Helper.GetUserName();
            if (String.IsNullOrWhiteSpace(UserMail))
            {
                Session.RemoveAll();
                FormsAuthentication.SignOut();
                return RedirectToAction("Index", "Account");
            }

            string BuyDrink = Helper.GetUserBuyDrink();
            string OrderSet = Helper.GetUserOrderSet();
            string Message = Helper.GetUserMessage();
            string Callnotice = Helper.GetUserCallnotice();
            string ChangePsd = Helper.GetUserChangePsd();
            string[] UserLimit = { BuyDrink, OrderSet, Message, Callnotice, ChangePsd };
            ViewBag.UserLimit = UserLimit;

            return View();
        }

        //GET: Home/SettingHome
        public ActionResult _MenuHomePartial()
        {
            //將以開放的菜單show出
            var result = db.Menus.Where(m => m.Open == true);
            return PartialView("_MenuHomePartial", result.ToList());
        }

        //GET: Home/CheckEndReadBtn
        public ActionResult CheckEndReadBtn(int id)
        {
            var query = db.OrderEndingSends.Find(id);
            if (query == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            query.ReadOrNot = true;
            db.SaveChanges();

            return RedirectToAction("MessageView_New", "Home");
        }






        //關閉連線
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }


    }
}