using NewDrink2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace NewDrink2
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ////定時排程
            //Timer t = new Timer(3600000 * 60 * 1);
            //t.Elapsed += new ElapsedEventHandler(t_Elapsed);
            //t.AutoReset = true;
            //t.Enabled = true;
        }

        //private void t_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    NewDrinkDB db = new NewDrinkDB();

        //    //看CreateBuyOrder_LeaderOrder的截止日
        //    //如果為有效截止日則到CreateBuyOrder_LeaderOrder將此訂單結束
        //    var NullTime = Convert.ToDateTime("2000/1/1 12:00:00");
        //    var NowTime = DateTime.Now;
        //    var AllEndOrder = db.CreateBuyOrder_LeaderOrders.Where(m => m.EndThisTime > NullTime && (m.CanOrNotOrder == 0 || m.CanOrNotOrder == 3) && m.CheckEnd == true);
        //    if (AllEndOrder.FirstOrDefault() != null)
        //    {
        //        foreach (var item in AllEndOrder)
        //        {
        //            if (item.EndThisTime < NowTime)
        //            {
        //                item.CanOrNotOrder = 2;
        //            }
        //            db.SaveChanges();

        //            //如果過期了自動寄出通知
        //            //寄出結束通知
        //            Models.AnyDMorBuy.CheckCanSend_members(item.OrderID, "結束");

        //        }
        //    }

        //    db.Dispose();
        //}

    }
}
