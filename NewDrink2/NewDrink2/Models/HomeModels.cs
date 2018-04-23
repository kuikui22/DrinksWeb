using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewDrink2.Models
{
    public class HomeModels
    {
        public static int GetHowManyMessage_UseUserEmail(string Mail)
        {
            NewDrinkDB db = new NewDrinkDB();

            var UserID = db.Users.Where(m => m.Email == Mail).Select(m => new { id = m.ID });
            int Userid = 0;
            foreach (var item in UserID)
            {
                Userid = item.id;
            }

            //判斷是否有資料超過三天,超過則刪除
            var time = DateTime.Now;
            var resultCheck = db.SendMessageViews.Where(m => m.UserID == Userid && m.ReadOrNot == false);
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
            var Message = db2.SendMessageViews.Where(m => m.UserID == Userid && m.ReadOrNot == false);
            int Mess = Message.Count();

            db.Dispose();
            return Mess;
        }

        //將結束未讀的訂單取出
        public static int GetHowManyEndMessage_UseUserEmail(string Mail)
        {
            NewDrinkDB db = new NewDrinkDB();

            var UserID = db.Users.Where(m => m.Email == Mail).Select(m => new { id = m.ID });
            int Userid = 0;
            foreach (var item in UserID)
            {
                Userid = item.id;
            }

            //判斷是否有資料超過三天,超過則刪除
            var time = DateTime.Now;
            var resultCheck = db.OrderEndingSends.Where(m => m.ToUser == Userid && m.ReadOrNot == false);
            foreach (var item in resultCheck)
            {
                string NowDay = time.ToString("yyyyMM");
                string SendDay = item.EndOverTime.ToString("yyyyMM");

                //如果不同年月則刪除
                if (NowDay != SendDay)
                {
                    var De = db.OrderEndingSends.Find(item.ID);
                    db.OrderEndingSends.Remove(De);
                    db.SaveChanges();
                }

                //如果超過三天則刪除
                string NowDay2 = time.ToString("dd");
                string SendDay2 = item.EndOverTime.ToString("dd");
                int nowD = Int32.Parse(NowDay2);
                int SedD = Int32.Parse(SendDay2);

                if (nowD - SedD >= 3)
                {
                    var De = db.OrderEndingSends.Find(item.ID);
                    db.OrderEndingSends.Remove(De);
                    db.SaveChanges();
                }

            }
            var db2 = new NewDrinkDB();
            var Message = db2.OrderEndingSends.Where(m => m.ToUser == Userid && m.ReadOrNot == false);
            int Mess = Message.Count();

            db.Dispose();
            return Mess;

        }


        //檢查有沒有訂單到期了,並將訂單結束
        public static bool HaveEndOrder() {
            NewDrinkDB db = new NewDrinkDB();

            //看CreateBuyOrder_LeaderOrder的截止日
            //如果為有效截止日則到CreateBuyOrder_LeaderOrder將此訂單結束
            var NullTime = Convert.ToDateTime("2000/1/1 12:00:00");
            var NowTime = DateTime.Now;
            var AllEndOrder = db.CreateBuyOrder_LeaderOrders.Where(m => m.EndThisTime > NullTime && (m.CanOrNotOrder == 0 || m.CanOrNotOrder == 3) && m.CheckEnd == true);
            if (AllEndOrder.FirstOrDefault() != null)
            {
                foreach (var item in AllEndOrder)
                {
                    if (item.EndThisTime < NowTime)
                    {
                        item.CanOrNotOrder = 2;

                        //如果過期了自動寄出通知
                        //寄出結束通知
                        Models.AnyDMorBuy.CheckCanSend_members(item.OrderID, "結束");
                    }
                    db.SaveChanges();
                }
            }

            db.Dispose();
            return true;
        }






    }
}