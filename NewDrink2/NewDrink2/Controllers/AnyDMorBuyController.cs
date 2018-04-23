using NewDrink2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace NewDrink2.Controllers
{
    [Authorize]
    public class AnyDMorBuyController : Controller
    {
        private NewDrinkDB db = new NewDrinkDB();

        // GET: AnyDMorBuy/AllDM
        public ActionResult AllDM(string SearchString)
        {
            //將以開放的菜單show出
            var result = db.Menus.Where(m => m.Open == true);

            if (!String.IsNullOrEmpty(SearchString))
            {
                result = result.Where(m => m.MenuName.Contains(SearchString));
            }

            return View(result.ToList());
        }

        //GET: AnyDMorBuy/_DMDetailLayout
        public ActionResult _DMDetailLayout(int id, string BuyCheck)
        {
            //以菜單ID取出名稱.
            Menu query = db.Menus.Find(id);
            if (query == null)
            {
                return HttpNotFound();
            }

            //取出三張表作組合
            //Menus(菜單總表)
            //MenuDrink(飲料類名表)
            //SizeTable(飲料尺寸價表)
            var result = Models.AnyDMorBuy.BuyDMDetailView_UseMenuID(id);

            //取出飲料type
            var type = result.DMDrinksDetailView.GroupBy(o => o.DrinkType);
            List<string> FirstType = new List<string>();
            foreach (var item in type)
            {
                FirstType.Add(item.Key);
            }

            ViewBag.OrderID = BuyCheck;
            ViewBag.Tyep = FirstType;
            return PartialView(result);
        }

        // GET: AnyDMorBuy/DMDetail
        public ActionResult DMDetail(int? id, string BuyCheck, string UserSearch)
        {
            //以菜單ID取出名稱.
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Menu query = db.Menus.Find(id);
            if (query == null)
            {
                return HttpNotFound();
            }
            int ID = id ?? 0;

            //取出三張表作組合
            //Menus(菜單總表)
            //MenuDrink(飲料類名表)
            //SizeTable(飲料尺寸價表)
            var result = Models.AnyDMorBuy.BuyDMDetailView_UseMenuID(ID);

            //取出飲料type
            var type = result.DMDrinksDetailView.GroupBy(o => o.DrinkType);
            List<string> FirstType = new List<string>();
            foreach (var item in type)
            {
                FirstType.Add(item.Key);
            }

            if (BuyCheck != null)
            {
                //取出使用者ID
                string UserMail = Helper.GetUserMail();
                int UserID = Models.AnyDMorBuy.GetUserID_UseEmail(UserMail);
                //查看使用者選幾個商品
                int checkID = Int32.Parse(BuyCheck);
                var TotBuy = db.CreateBuyOrder_Details.Where(m => m.OrderID == checkID && m.UserID == UserID && m.MenuID == id);
                ViewBag.TotBuy = TotBuy.Count();
                ViewBag.OrderID = BuyCheck;
                //取出團長ID
                var Lead = db.CreateBuyOrder_LeaderOrders.Find(checkID);
                ViewBag.LeaderID = Lead.UserID;
                ViewBag.UserID = UserID;
            }

            ViewBag.Tyep = FirstType;
            ViewBag.UserSearch = UserSearch;
            return View(result);
        }

        //GET: AnyDMorBuy/_DMDetailPartial
        public ActionResult _DMDetailPartial(int id, string SearchString, string BuyCheck, string UserSearch)
        {
            var query = db.MenuDrinks.Where(m => m.MenuID == id);
            if (String.IsNullOrWhiteSpace(SearchString))
            {
                var TheType = query.GroupBy(o => o.DrinkType);
                foreach (var item in TheType)
                {
                    SearchString = item.Key;
                    break;
                }
            }
            var type = db.MenuDrinks.Where(m => m.MenuID == id && m.DrinkType == SearchString);

            if (!String.IsNullOrEmpty(UserSearch))
            {
                type = db.MenuDrinks.Where(m => m.MenuID == id && m.DrinkName.Contains(UserSearch));
            }

            List<DMDrinksDetailView> result = new List<DMDrinksDetailView>();
            foreach (var item in type)
            {                
                List<DMDrinksPriceSize> price = new List<DMDrinksPriceSize>();
                var sizeP = db.SizeTables.Where(m => m.MenuID == id && m.DrinkID == item.ID);
                foreach (var Pitem in sizeP)
                {
                    //這理暫時設定 SizePID為搜索主鍵, IceID = 5 時為熱飲
                    var hotIce = db.IceTables.Where(m => m.SizePID == Pitem.ID);
                    int HotID = 0;
                    foreach (var HotItem in hotIce)
                    {
                        if (HotItem.IceID == 5 && hotIce != null && hotIce.Count() == 1)
                        {
                            HotID = 1;
                        }
                    }

                    var SizeName = db.SizeTypes.Find(Pitem.SizeID);
                    string SName = SizeName.SimpleName;
                    if (SizeName.SimpleName == "NoName" || String.IsNullOrEmpty(SizeName.SimpleName))
                    {
                        SName = SizeName.SizeName;
                    }

                    price.Add(new DMDrinksPriceSize()
                    {
                        DrinkPrice = Pitem.Price,
                        DrinkSize = SizeName.SizeName,
                        SimpleName = SName,
                        HotOrIce = HotID
                    });
                }
                result.Add(new DMDrinksDetailView()
                {
                    DrinkID = item.ID,
                    DrinkType = item.DrinkType,
                    DrinkName = item.DrinkName,
                    Bathus = item.Bathus,
                    DMDrinksPriceSize = price
                });
            }

            if (BuyCheck != null)
            {               
                ViewBag.OrderID = BuyCheck;
            }

            ViewBag.MenuID = id;
            return PartialView("_DMDetailPartial", result);
        }

        //GET: AnyDMorBuy/DrinkDetail
        public ActionResult DrinkDetail(int MenuID, int DrinkID, string BuyCheck)
        {
            //依照MenuID,DrinkID取資訊
            /*需要使用的資訊
             * MenuID
             * DrinkID
             * DrinkName
             * SizePID(Select)
             * SweetID(Select)
             * IceID(Select)
             * AdditemPID(checkbox)
             */
            if (BuyCheck != null)
            {
                ViewBag.OrderID = BuyCheck;
            }
            var result = Models.AnyDMorBuy.GetBuyDMDrinkView_MenuIDdrinkID(MenuID, DrinkID);
            return View(result);
        }

        //GET: AnyDMorBuy/_HaveItemLimitPartial
        public ActionResult _HaveItemLimitPartial(int SizeP, int MenuID, int DrinkID)
        {
            if (SizeP != 0)
            {
                var result = Models.AnyDMorBuy.DrinkAddItem(SizeP);
                return PartialView("_HaveItemLimitPartial", result);
            }

            var result2 = Models.AnyDMorBuy.DrinkAddItem_UseMenuIDdrinkID(MenuID, DrinkID);
            return PartialView("_HaveItemLimitPartial", result2);
        }

        //GET: AnyDMorBuy/_DrinkSweetPartial
        public ActionResult _DrinkSweetPartial(int SizeP, int MenuID, int DrinkID)
        {
            if (SizeP != 0)
            {
                var result = Models.AnyDMorBuy.DrinkSweet(SizeP);
                return PartialView("_DrinkSweetPartial", result);
            }

            var result2 = Models.AnyDMorBuy.DrinkSweet_UseMenuIDdrinkID(MenuID, DrinkID);
            return PartialView("_DrinkSweetPartial", result2);
        }

        //GET: AnyDMorBuy/_DrinkIcePartial
        public ActionResult _DrinkIcePartial(int SizeP, int MenuID, int DrinkID)
        {
            if (SizeP != 0)
            {
                var result = Models.AnyDMorBuy.DrinkIce(SizeP);
                return PartialView("_DrinkIcePartial", result);
            }

            var result2 = Models.AnyDMorBuy.DrinkIce_UseMenuIDdrinkID(MenuID, DrinkID);
            return PartialView("_DrinkIcePartial", result2);
        }

        //GET: AnyDMorBuy/_DrinkItemPartial
        public ActionResult _DrinkItemPartial(int SizeP, int MenuID, int DrinkID)
        {
            if (SizeP != 0)
            {
                var result = Models.AnyDMorBuy.DrinkAddItem(SizeP);
                return PartialView("_DrinkItemPartial", result);
            }

            var result2 = Models.AnyDMorBuy.DrinkAddItem_UseMenuIDdrinkID(MenuID, DrinkID);
            return PartialView("_DrinkItemPartial", result2);
        }

        //GET: AnyDMorBuy/_TotalMoneyPartial
        public ActionResult _TotalMoneyPartial(int SizeP, string AdditemP, int Quantity, int MenuID, int DrinkID)
        {
            //計算價錢
            if (SizeP == 0)
            {
                SizeP = Models.AnyDMorBuy.PickSizePid_UseMenuIDdrinkID(MenuID, DrinkID);
            }
            var SizePrice = db.SizeTables.Find(SizeP);
            int Total = SizePrice.Price;
           
            if (AdditemP != null)
            {
                List<int> additemP = new List<int>();
                var add = AdditemP.Trim(',');
                string[] pickadd = add.Split(new char[] {','});

                foreach (var item in pickadd)
                {
                    if (item == "undefinde" || String.IsNullOrEmpty(item))
                    {
                        continue;
                    }
                    int num = Int32.Parse(item);
                    var query = db.AddItemTypePrices.Find(num);
                    if (query != null)
                    {
                        additemP.Add(query.ItemPrice);
                    }                   
                }
                foreach (var item in additemP)
                {
                    Total = Total + item;
                }
            }
            Total = Total * Quantity;
                       
            ViewBag.Total = Total;
            return PartialView("_TotalMoneyPartial");
        }

        //POST: AnyDMorBuy/DrinkDetail
        [HttpPost]
        public ActionResult DrinkDetail(BuyDMDrinkView Buy, string BuyCheck)
        {
            //加入超過三樣配料,返回視圖
            int addTot = Models.AnyDMorBuy.GetAddItemInt(Buy).Count;

            if (!ModelState.IsValid || addTot > 3)
            {
                Buy.SizeType = Models.AnyDMorBuy.DrinkSize(Buy.MenuID, Buy.DrinkID);

                if (addTot > 3)
                {
                    ViewBag.ErrorMessage = "配料最多只能選三樣喔!";
                }
                return View(Buy);
            }

            //取出使用者ID
            string UserMail = Helper.GetUserMail();
            int UserID = Models.AnyDMorBuy.GetUserID_UseEmail(UserMail);
            //取出時間
            DateTime time = DateTime.Now;

            //正確金額在這裡做計算
            //寫入兩個表
            //下訂單用總表(CreateBuyOrder_LeaderOrder)
            //訂單用明細(CreateBuyOrder_Detail)

            //(第一次下訂單時)
            //先寫入下訂單用總表(取OrderID)////////////////////////////////////////////////////
            int OrderID = 0;
            if (BuyCheck == null || String.IsNullOrEmpty(BuyCheck))
            {
                db.CreateBuyOrder_LeaderOrders.Add(new CreateBuyOrder_LeaderOrder()
                {
                    UserID = UserID,
                    MenuID = Buy.MenuID,
                    CreateTime = time,
                    TotalCount = 0,
                    CanOrNotOrder = 0
                });
                db.SaveChanges();

                var newTime = time.ToString();
                var Order = db.CreateBuyOrder_LeaderOrders.Where(m => m.MenuID == Buy.MenuID && m.UserID == UserID).Select(m => new { ID = m.OrderID, Time = m.CreateTime });
                if (Order == null)
                {
                    Buy.SizeType = Models.AnyDMorBuy.DrinkSize(Buy.MenuID, Buy.DrinkID);
                    if (addTot > 3)
                    {
                        ViewBag.ErrorMessage = "菜單建置時發生錯誤,請重新下訂.";
                    }
                    return View(Buy);
                }

                //取出OrderID
                foreach (var item in Order)
                {
                    if (item.Time.ToString() == newTime)
                    {
                        OrderID = item.ID;
                    }
                }
            }
            else
            {
                int myNowID = Int32.Parse(BuyCheck);
                OrderID = myNowID;
            }
                        
            //寫入訂單用明細(取DetailID)////////////////////////////////////////////////////////////////////
            int SizePID = Int32.Parse(Buy.SizeTypeM);
            int SweetID = Models.AnyDMorBuy.GetSweetIceInt(Buy, "sweet");
            int IceID = Models.AnyDMorBuy.GetSweetIceInt(Buy, "ice");
            int Add01 = Models.AnyDMorBuy.GetOnlyAdditemInt(Buy, 1);
            int Add02 = Models.AnyDMorBuy.GetOnlyAdditemInt(Buy, 2);
            int Add03 = Models.AnyDMorBuy.GetOnlyAdditemInt(Buy, 3);
            int SCount = Models.AnyDMorBuy.GetSCount(Buy);

            db.CreateBuyOrder_Details.Add(new CreateBuyOrder_Detail()
            {
                OrderID = OrderID,
                MenuID = Buy.MenuID,
                UserID = UserID,
                DrinkID = Buy.DrinkID,
                SizePID = SizePID,
                SweetID = SweetID,
                IceID = IceID,
                Additem01PID = Add01,
                Additem02PID = Add02,
                Additem03PID = Add03,
                Quantity = Buy.Quantity,
                SCount = SCount,
            });           

            //更新總價格
            var ChangeTot = db.CreateBuyOrder_LeaderOrders.Find(OrderID);
            ChangeTot.TotalCount = ChangeTot.TotalCount + SCount;
            db.SaveChanges();

            //繼續選購
            return RedirectToAction("DMDetail", "AnyDMorBuy", new { id = Buy.MenuID, BuyCheck = OrderID.ToString() });
        }

        //GET: AnyDMorBuy/AllOrderView
        public ActionResult AllOrderView()
        {
            //取出使用者ID
            string UserMail = Helper.GetUserMail();
            int UserID = Models.AnyDMorBuy.GetUserID_UseEmail(UserMail);

            //確認截止訂單(將過期的訂單結止)
            Models.HomeModels.HaveEndOrder();

            //找出自己未完成的訂單
            //之後寄Mail給自己的也可以看到
            var myAllOrder = Models.AnyDMorBuy.GetAllOrdering_UseOrderID(UserID, 0, 3);

            //由於加入了回覆不訂購的要素,所以要將自己回覆不訂購的訂單藏起來
            //先找出自己不回覆的訂單(到時到前台則不顯示)
            var MySotpOrder = db.CreateBuyOrder_MemberOrders.Where(m => m.UserID == UserID && m.Together == false);
            foreach (var item in MySotpOrder)
            {
                //var not = true;//(表示訂單存活)
                foreach (var myAll in myAllOrder)
                {
                    if (item.OrderID == myAll.OrderID)
                    {
                        //找到死亡的訂單移除
                        myAllOrder.Remove(myAll);
                        break;
                    }

                }
            }

            return View(myAllOrder);
        }

        //GET: AnyDMorBuy/OrderingDetail
        public ActionResult OrderingDetail(string OrderID)
        {
            if (String.IsNullOrEmpty(OrderID))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //先判斷訂單是否已經結束
            //如果結束則返回已讀訊息
            int orderid = Int32.Parse(OrderID);
            var check = db.CreateBuyOrder_LeaderOrders.Find(orderid);
            if (check.CanOrNotOrder == 1 || check.CanOrNotOrder == 2)
            {
                //將訂單已讀
                var Mail = Helper.GetUserMail();
                var UID = AnyDMorBuy.GetUserID_UseEmail(Mail);
                var qu = db.SendMessageViews.Where(m => m.OrderID == orderid && m.UserID == UID);
                foreach (var item in qu)
                {
                    item.ReadOrNot = true;
                }
                db.SaveChanges();
                TempData["message"] = "這筆訂單已經結束訂購囉!";
                return RedirectToAction("MessageView_Read", "Home");
            }

            var result = Models.AnyDMorBuy.GetOrderingDetail_UseOrderID(OrderID);
            ViewBag.OrderID = OrderID;
            return View(result);
        }

        //GET: AnyDMorBuy/OrderingMemDetail
        public ActionResult OrderingMemDetail(string OrderID)
        {
            if (String.IsNullOrEmpty(OrderID))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.OrderID = OrderID;

            //看此訂單有沒有成員資料
            int ordid = Int32.Parse(OrderID);
            var HaveMem = db.CreateBuyOrder_MemberOrders.Where(m => m.OrderID == ordid);
            ViewBag.HaveMem = 0;
            if (HaveMem.FirstOrDefault() != null)
            {
                ViewBag.HaveMem = 1;
            }

            var result = Models.AnyDMorBuy.GetOrderingDetail_UseOrderID(OrderID);
            return View(result);
        }

        //GET: AnyDMorBuy/OrderingMemResponse
        public ActionResult OrderingMemResponse(int OrderID)
        {
            var result = Models.AnyDMorBuy.MyMem_Response(OrderID);
            return PartialView(result);
        }


        //GET: AnyDMorBuy/OverTheOrder
        public ActionResult OverTheOrder(int OrderID)
        {
            var query = db.CreateBuyOrder_LeaderOrders.Find(OrderID);
            if (query == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //寫入結帳或取消訂單時間
            var time = DateTime.Now;       

            query.CanOrNotOrder = 2;
            query.EndThisTime = time;
            db.SaveChanges();

            //寄出結束通知
            Models.AnyDMorBuy.CheckCanSend_members(OrderID, "結束");

            //返回歷史訂單
            return RedirectToAction("AllOrderView", "AnyDMorBuy");
        }

        //GET: AnyDMorBuy/BackMenuDetail_Buy
        public ActionResult BackMenuDetail_Buy(int BuyCheck)
        {
            var query = db.CreateBuyOrder_LeaderOrders.Find(BuyCheck);
            return RedirectToAction("DMDetail", "AnyDMorBuy", new { id = query.MenuID, BuyCheck = BuyCheck.ToString() });
        }

        //GET: AnyDMorBuy/MenuDetail_ToHistory
        public ActionResult MenuDetail_ToHistory(int BuyCheck)
        {
            var time = DateTime.Now;
            var query = db.CreateBuyOrder_LeaderOrders.Find(BuyCheck);
            query.CanOrNotOrder = 1;
            query.EndThisTime = time;
            db.SaveChanges();
            //導向選購訂單頁
            //向團員寄出取消通知(取出這份訂單有邀請的人)
            Models.AnyDMorBuy.CheckCanSend_members(BuyCheck, "取消");


            return RedirectToAction("AllOrderView", "AnyDMorBuy");
        }

        //直接揪團(先給一組orderID)
        //GET: AnyDMorBuy/SendMessage_GetMenu
        public ActionResult SendMessage_GetMenu(int MenuID)
        {
            if (MenuID == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //取出使用者ID
            string UserMail = Helper.GetUserMail();
            int UserID = Models.AnyDMorBuy.GetUserID_UseEmail(UserMail);
            //取出時間
            DateTime time = DateTime.Now;

            //先寫入下訂單用總表(取OrderID)////////////////////////////////////////////////////
            //先取orderID(建立訂單)
            db.CreateBuyOrder_LeaderOrders.Add(new CreateBuyOrder_LeaderOrder()
            {
                UserID = UserID,
                MenuID = MenuID,
                CreateTime = time,
                TotalCount = 0,
                CanOrNotOrder = 0
            });
            db.SaveChanges();

            int OrderID = 0;
            var newTime = time.ToString();
            var Order = db.CreateBuyOrder_LeaderOrders.Where(m => m.MenuID == MenuID && m.UserID == UserID).Select(m => new { ID = m.OrderID, Time = m.CreateTime });
            if (Order == null)
            {
                ViewBag.ErrorMessage = "菜單建置時發生錯誤,請重新下訂.";
                return RedirectToAction("DMDetail", "AnyDMorBuy", new { id = MenuID });
            }
            foreach (var item in Order)
            {
                if (item.Time.ToString() == newTime)
                {
                    OrderID = item.ID;
                }
            }

            //寄Mail
            ViewBag.DetailID = OrderID;
            return RedirectToAction("SendMyMessage", "AnyDMorBuy", new { BuyCheck = OrderID });
        }

        //寄信團員選擇視圖
        public ActionResult _ChangeSendMemberPartial(ChangeMem MyMember)
        {
            
            //取得團員,但不顯示自己及管理者
            var MyID = Helper.GetUserMail();
            var Myid = AnyDMorBuy.GetUserID_UseEmail(MyID);

            var Member = db.Users.Where(m => m.ID != Myid && m.Identity != 1);

            if (MyMember.Member != null)
            {
                //文字不見了要補上
                foreach (var item in MyMember.Member)
                {
                    int Num = Int32.Parse(item.Value);
                    foreach (var userN in Member)
                    {
                        if (userN.ID == Num)
                        {
                            item.Text = userN.Name;
                            break;
                        }
                    }
                }

                ChangeMem Check2 = MyMember;
                return PartialView("_ChangeSendMemberPartial", Check2);
            }


            var result = new List<SelectListItem>();
            foreach (var item in Member)
            {
                result.Add(
                    new SelectListItem { Text = item.Name, Value = item.ID.ToString(), Selected = true }
                );
            }
           
            var Check = new ChangeMem() { Member = result };

            return PartialView("_ChangeSendMemberPartial", Check);
        }

        //後來糾團(用原本的orderID)
        //GET: AnyDMorBuy/SendMyMessage
        public ActionResult SendMyMessage(int BuyCheck)
        {
            ViewBag.DetailID = BuyCheck;

            //先去LeaderSendMessage找看看有沒有寄過
            var result = db.SendMessageViews.Where(m => m.OrderID == BuyCheck);
            if (result.FirstOrDefault() != null)
            {
                var Nresult = new EmailModelView();
                foreach (var item in result)
                {
                    Nresult = new EmailModelView()
                    {
                        Subject = item.SentSubject,
                        Body = item.SentBody,
                        CheckEnd = true
                    };
                    break;
                }
                return View(Nresult);
            }

            return View();
        }

        //POST: AnyDMorBuy/SendMyMessage
        [HttpPost]
        public ActionResult SendMyMessage(EmailModelView MailC, string YearDate2)
        {
            NewDrinkDB dbR = new NewDrinkDB();
            //接收Model
            //接收使用者OrderID
            if (!ModelState.IsValid || String.IsNullOrEmpty(YearDate2))
            {
                ViewBag.DetailID = MailC.OrderID;
                if (String.IsNullOrEmpty(YearDate2))
                {
                    ViewBag.Message = "時間欄位未填.";
                }                
                return View(MailC);
            }
            //勾選系統關團的狀況
            ////有勾未填時間
            //if (String.IsNullOrEmpty(MailC.EndOverTime) && MailC.CheckEnd == true)
            //{
            //    ViewBag.Message = "您尚未填寫關團時間.";
            //    ViewBag.DetailID = MailC.OrderID;
            //    return View(MailC);
            //}
            ////填時間未勾
            //if (!String.IsNullOrEmpty(MailC.EndOverTime) && MailC.CheckEnd == false)
            //{
            //    ViewBag.Message = "請勾選系統自動關團.";
            //    ViewBag.DetailID = MailC.OrderID;
            //    return View(MailC);
            //}

            string EtimeEnd = null;
            string MyTime = null;
            var time2 = DateTime.Now;
            var InputTime = Convert.ToDateTime(YearDate2);
            //判斷時間是否非法
            if (time2 > InputTime)
            {
                ViewBag.DetailID = MailC.OrderID;
                ViewBag.Message = "時間不可以是過去式.";
                return View(MailC);
            }
            EtimeEnd = "截止日期: " + YearDate2;
            MyTime = YearDate2;


            //判斷時間格式
            //須判斷: 月份,日期
            //"", "NaN-NaN-NaN" 
            //int Hour = Int32.Parse(hours);
            //if (String.IsNullOrEmpty(YearDate) || YearDate == "NaN-NaN-NaN")
            //{                   
            //    var time2 = DateTime.Now;               
            //    time2 = time2.AddHours(Hour);
            //    EtimeEnd = "截止日期: " + time2;
            //    MyTime = time2.ToString();
            //}

            //if (!String.IsNullOrEmpty(YearDate))
            //{
            //    DateTime ETimeM = Convert.ToDateTime(YearDate);
            //    var time2 = DateTime.Now;

            //    int EMon = Int32.Parse(ETimeM.ToString("MM"));
            //    int NMon = Int32.Parse(time2.ToString("MM"));
            //    //月份小
            //    if (EMon < NMon)
            //    {
            //        //回傳錯誤
            //        ViewBag.Message = "時間不可以是過去式.";
            //        ViewBag.DetailID = MailC.OrderID;
            //        ViewBag.Member = MailC.Member;
            //        return View(MailC);
            //    }
            //    //月份同
            //    if (EMon == NMon)
            //    {
            //        int EDate = Int32.Parse(ETimeM.ToString("dd"));
            //        int NDate = Int32.Parse(time2.ToString("dd"));
            //        //日期小
            //        if (EDate < NDate)
            //        {
            //            //回傳錯誤
            //            ViewBag.Message = "時間不可以是過去式.";
            //            ViewBag.DetailID = MailC.OrderID;
            //            ViewBag.Member = MailC.Member;
            //            return View(MailC);
            //        }
            //    }
            //    //[月份大],[月份同,日期大](直接加輸入的小時)
            //    if (EMon > NMon)
            //    {
            //        DateTime ETime = Convert.ToDateTime(YearDate);
            //        ETime = ETime.AddHours(Hour);
            //        MyTime = ETime.ToString();
            //        EtimeEnd = "截止日期: " + ETime;
            //    }

            //}

            ////取出所有有勾選的成員(不寄給自己)
            var PickM = new ChangeMem() { Member = MailC.Member };
            var AllMember = Models.AnyDMorBuy.CheckMem(PickM);

            if (AllMember == null || AllMember.Count() == 0)
            {
                ViewBag.DetailID = MailC.OrderID;
                ViewBag.EmptyM = "請至少選擇一位成員.";
                return View(MailC);
            }

            var time = DateTime.Now;
            ////取出使用者ID
            string UserMail = Helper.GetUserMail();
            int UserID = Models.AnyDMorBuy.GetUserID_UseEmail(UserMail);

            //這裡要先判斷使用者寄信的狀態,及時間(0為無寄信, 1為寄信中或上次寄信錯誤)
            //若為1則返回錯誤訊息(限制5分鐘後才可再寄信)
            var CheckYesSend = dbR.Users.Find(UserID);

            //判斷時間
            bool CheckThisTime = Models.AnyDMorBuy.CheckCanSend_Message(time, CheckYesSend.SendTime);

            if (CheckYesSend.SendOrNot == 1 && CheckThisTime == true)
            {
                ViewBag.DetailID = MailC.OrderID;
                TempData["message"] = "Error";
                ViewBag.Error = "Error";
                return View(MailC);
            }
            else
            {
                //若為0寄信,並更改寄信狀態,紀錄時間
                CheckYesSend.SendOrNot = 1;
                CheckYesSend.SendTime = time;
                dbR.SaveChanges();
            }

            //截止日期
            DateTime ETime2 = Convert.ToDateTime(MyTime);

            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            //到時要循環寄出////////////////////////////////////////////////////////////////////////////////////////////
            //還要判斷是否重複寄出了
            //若重覆了則決定要updata時間還是讓使用者重建菜單

            //var AllMember = db.Users.Where(m => m.ID != UserID);
            //循環寄出
            foreach (var item in AllMember)
            {
                NewDrinkDB dbM = new NewDrinkDB();
                //取出成員資料
                var MemList = dbM.Users.Find(item);

                //http://172.16.8.4:42118
                //http://172.16.8.59:52118
                //作Guid加解密(作唯一鍵值)
                string plainText = MemList.ID.ToString() + MailC.OrderID.ToString();
                Guid userGuid = System.Guid.NewGuid();
                var dataAndGuid = Models.AnyDMorBuy.GuidwithOrderLogin(userGuid, plainText);

                string SendLink = "http://172.16.8.4:42118/Account/Get_MyOrder?ID=" + dataAndGuid;
                string NewBody = MailC.Body + "菜單連結: http://172.16.8.4:42118/Account/Get_MyOrder?ID=" + dataAndGuid;

                if (!String.IsNullOrEmpty(EtimeEnd) && MailC.CheckEnd == true)
                {
                    NewBody = NewBody + EtimeEnd;
                }

                //團員mail
                string to = MemList.Email.ToString();
                //團長mail
                string from = UserMail;                

                string subject = MailC.Subject;                
                MailMessage mailMessage = new MailMessage(from, to);
                mailMessage.Subject = subject;
                mailMessage.Body = NewBody;
                //string smtpServer = "mail.bingotimes.com.tw";
                string smtpServer = "smtp.gmail.com";
                //string smtpServer = "127.0.0.1";
                SmtpClient client = new SmtpClient(smtpServer, 25);
                //client.EnableSsl = true;

                //寄之前暫停1秒
                int millsecondsTimeout = 1000;
                System.Threading.Thread.Sleep(millsecondsTimeout);

                try
                {
                    client.Send(mailMessage);
                }
                catch (Exception e)
                {
                    ViewBag.DetailID = MailC.OrderID;
                    TempData["message"] = "Error";
                    ViewBag.Error = "Error";
                    return View(MailC);
                }
                               
                //還要判斷是否重複寄出了
                //若重覆了則決定要updata時間還是讓使用者重建菜單
                var CheckRepeat = db.SendMessageViews.Where(m => m.UserID == item && m.OrderID == MailC.OrderID && m.SentUser == UserID);
                if (CheckRepeat.FirstOrDefault() != null)
                {
                    //已經寄過了,更新時間及link
                    foreach (var update in CheckRepeat)
                    {
                        update.SendTime = time;
                        update.SentAlink = SendLink;
                        update.SentBody = MailC.Body;
                        update.SentSubject = MailC.Subject;
                        update.ReadOrNot = false;
                        db.SaveChanges();
                    }
                }
                else
                {
                    //第一次寄
                    //寫入資料庫(到時ID要改成其他人對應的ID)
                    dbM.SendMessageViews.Add(new SendMessageView()
                    {
                        ID = dataAndGuid,
                        OrderID = MailC.OrderID,
                        UserID = MemList.ID,
                        SendTime = time,
                        SentUser = UserID,
                        SentBody = MailC.Body,
                        SentAlink = SendLink,
                        SentSubject = MailC.Subject,
                        ReadOrNot = false,
                        EndOverTime = ETime2
                    });
                    dbM.SaveChanges();
                }
            }//End foreach.

            //循環結束 ///////////////////////////////////////////////////////////////////////////////////////////
           
            //這邊團長自己的也要判斷(link寫團長自己的ID)
            var CheckLeader = dbR.LeaderSendMessages.Where(m => m.SentUser == UserID && m.OrderID == MailC.OrderID);
            //取出MenuID
            var MenuID = dbR.CreateBuyOrder_LeaderOrders.Find(MailC.OrderID);
            //作Token加解密
            string plainTextU = UserID.ToString() + MailC.OrderID.ToString();
            Guid leaderGuid = System.Guid.NewGuid();
            var dataAndGuidU = Models.AnyDMorBuy.GuidwithOrderLogin(leaderGuid, plainTextU);
            //string LeaderID = PasswordUtility.AESEncryptor(UserID.ToString(), aes.Key, aes.IV);
            string LeaderLink = "http://172.16.8.59:52118/Account/Get_LeaderMyOrder?ID=" + dataAndGuidU;
            

            //寫入資料庫
            if (CheckLeader.FirstOrDefault() != null)
            {
                //已經寄過了,更新時間及link
                foreach (var item in CheckLeader)
                {
                    //item.ID = dataAndGuidU;
                    item.Subject = MailC.Subject;
                    item.Body = MailC.Body;
                    //item.Alink = LeaderLink;
                    item.SendTime = time;
                    item.EndOverTime = ETime2;
                }

                //寫入成員表(這裡要判斷不要重覆寫入)
                var AddmyAllMember = dbR.CreateBuyOrder_MemberOrders.Where(m => m.OrderID == MailC.OrderID);
                var AddmyAllMember2 = new List<CreateBuyOrder_MemberOrder>();
                foreach (var item in AllMember)
                {
                    var HaveV = true;
                    foreach (var memC in AddmyAllMember)
                    {
                        if (item == memC.UserID)
                        {
                            //依狀況更新
                            memC.Together = true;
                            dbR.SaveChanges();
                            break;
                        }
                        HaveV = false;
                    }
                    if (HaveV == false)
                    {
                        //新增
                        AddmyAllMember2.Add(new CreateBuyOrder_MemberOrder()
                        {
                            UserID = item,
                            OrderID = MailC.OrderID,
                            Together = true
                        });
                    }
                }
                dbR.CreateBuyOrder_MemberOrders.AddRange(AddmyAllMember2);

            }
            else
            {
                dbR.LeaderSendMessages.Add(new LeaderSendMessage()
                {
                    ID = dataAndGuidU,
                    SentUser = UserID,
                    OrderID = MailC.OrderID,
                    Subject = MailC.Subject,
                    Body = MailC.Body,
                    Alink = LeaderLink,
                    SendTime = time,
                    EndOverTime = ETime2
                });

                //寫入成員表(第一次寫入)
                var AddmyAllMember = new List<CreateBuyOrder_MemberOrder>() { };
                foreach (var item in AllMember)
                {
                    AddmyAllMember.Add(new CreateBuyOrder_MemberOrder()
                    {
                        UserID = item,
                        OrderID = MailC.OrderID,
                        Together = true
                    });
                }
                dbR.CreateBuyOrder_MemberOrders.AddRange(AddmyAllMember);

            }

            //將團長的訂單填入截止日期
            if (!String.IsNullOrEmpty(MyTime))
            {
                NewDrinkDB dbO = new NewDrinkDB();
                var OrderEnd = dbO.CreateBuyOrder_LeaderOrders.Find(MailC.OrderID);
                OrderEnd.EndThisTime = ETime2;
                OrderEnd.CheckEnd = MailC.CheckEnd;
                dbO.SaveChanges();
            }

            //寄完所有信件後,將狀態改成0
            CheckYesSend.SendOrNot = 0;

            dbR.SaveChanges();

            //轉向已送出訊息檢視
            return RedirectToAction("MessageView_Sent", "Home");
        }

        //GET: AnyDMorBuy/_EditOrDeletePartial
        public ActionResult _EditOrDeletePartial(int DetailID)
        {
            ViewBag.DetailID = DetailID;
            return PartialView("_EditOrDeletePartial");
        }

        //GET: AnyDMorBuy/DeleteDetail
        public ActionResult DeleteDetail(int DetailID)
        {
            var query = db.CreateBuyOrder_Details.Find(DetailID);
            if (query == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int ID = query.OrderID;
            db.CreateBuyOrder_Details.Remove(query);

            //修改總價錢
            var TotCount = db.CreateBuyOrder_LeaderOrders.Find(query.OrderID);
            TotCount.TotalCount = TotCount.TotalCount - query.SCount;

            db.SaveChanges();
            return RedirectToAction("OrderingDetail", "AnyDMorBuy", new { OrderID = ID.ToString() });
        }

        //GET: AnyDMorBuy/EditDrinkDetail
        public ActionResult EditDrinkDetail(int DetailID)
        {
            var query = db.CreateBuyOrder_Details.Find(DetailID);
            if (query == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //以DetailID來編輯
            //使用BuyDMDrinkView Model視圖
            var result = Models.AnyDMorBuy.GetDrinkDetail_EditDrink(DetailID);
            ViewBag.DetailID = DetailID;
            return View(result);
        }

        //GET: AnyDMorBuy/_DrinkSweetPartial_Edit
        public ActionResult _DrinkSweetPartial_Edit(int DetailID)
        {
            var query = db.CreateBuyOrder_Details.Find(DetailID);
            if (query == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var SweetN = db.SweetTypes.Find(query.SweetID);
            var result = Models.AnyDMorBuy.DrinkSweet(query.SizePID);
            foreach (var item in result.SweetType)
            {
                if (item.Text == SweetN.SweetName)
                {
                    item.Selected = true;
                }
            }
            return PartialView("_DrinkSweetPartial", result);
        }

        //GET: AnyDMorBuy/_DrinkIcePartial_Edit
        public ActionResult _DrinkIcePartial_Edit(int DetailID)
        {
            var query = db.CreateBuyOrder_Details.Find(DetailID);
            if (query == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var IceN = db.IceTypes.Find(query.IceID);
            var result = Models.AnyDMorBuy.DrinkIce(query.SizePID);
            foreach (var item in result.IceType)
            {
                if (item.Text == IceN.IceName)
                {
                    item.Selected = true;
                }
            }

            return PartialView("_DrinkIcePartial", result);
        }

        //GET: AnyDMorBuy/_DrinkItemPartial_Edit
        public ActionResult _DrinkItemPartial_Edit(int DetailID)
        {
            var query = db.CreateBuyOrder_Details.Find(DetailID);
            if (query == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var result = Models.AnyDMorBuy.DrinkAddItem(query.SizePID);
            foreach (var item in result.AddItem)
            {
                if (item.Value == query.Additem01PID.ToString() || item.Value == query.Additem02PID.ToString() || item.Value == query.Additem03PID.ToString())
                {
                    item.Selected = true;
                }
            }

            return PartialView("_DrinkItemPartial", result);
        }

        //POST: AnyDMorBuy/EditDrinkDetail
        [HttpPost]
        public ActionResult EditDrinkDetail(BuyDMDrinkView Buy, string DetailID)
        {
            int ordid = Int32.Parse(DetailID);
            var OrderID = db.CreateBuyOrder_Details.Find(ordid);

            //加入超過三樣配料,返回視圖
            int addTot = Models.AnyDMorBuy.GetAddItemInt(Buy).Count;

            if (!ModelState.IsValid || addTot > 3)
            {
                Buy.SizeType = Models.AnyDMorBuy.DrinkSize(OrderID.MenuID, OrderID.DrinkID);
                //到時要寫配料的選擇超過三樣時的返回視圖

                if (addTot > 3)
                {
                    ViewBag.ErrorMessage = "配料最多只能選三樣喔!";
                }
                ViewBag.DetailID = ordid;
                return View(Buy);
            }

            //更新訂單用明細
            int SizePID = Int32.Parse(Buy.SizeTypeM);
            int SweetID = Models.AnyDMorBuy.GetSweetIceInt(Buy, "sweet");
            int IceID = Models.AnyDMorBuy.GetSweetIceInt(Buy, "ice");
            int Add01 = Models.AnyDMorBuy.GetOnlyAdditemInt(Buy, 1);
            int Add02 = Models.AnyDMorBuy.GetOnlyAdditemInt(Buy, 2);
            int Add03 = Models.AnyDMorBuy.GetOnlyAdditemInt(Buy, 3);
            int SCount = Models.AnyDMorBuy.GetSCount(Buy);

            var result = db.Database.ExecuteSqlCommand(@"UPDATE createbuyorder_detail SET SizePID = '" + SizePID + "', SweetID = '" + SweetID + "', IceID = '" + IceID +
                                                        "', Additem01PID = '" + Add01 + "', Additem02PID = '" + Add02 + "', Additem03PID = '" + Add03 + "', Quantity = '" + Buy.Quantity +
                                                        "', SCount = '" + SCount + "' Where DetailID = '" + ordid + "';");

            //釋放資料(若不釋放則有資料會取未更新時的那筆)
            db.Dispose();

            //更新總價格
            NewDrinkDB db2 = new NewDrinkDB();
            var OrderID2 = db2.CreateBuyOrder_Details.Find(ordid);
            var TotCount = db2.CreateBuyOrder_LeaderOrders.Find(OrderID.OrderID);
            var AllTotCount2 = db2.CreateBuyOrder_Details.Where(m => m.OrderID == TotCount.OrderID);
            int tot = 0;
            foreach (var item in AllTotCount2)
            {
                tot = tot + item.SCount;
            }
            TotCount.TotalCount = tot;

            db2.SaveChanges();
            db2.Dispose();
            //返回訂單檢視界面
            return RedirectToAction("OrderingDetail", "AnyDMorBuy", new { OrderID = OrderID.OrderID.ToString() });
        }

        //GET: AnyDMorBuy/AllOrderHistoryView
        public ActionResult AllOrderHistoryView()
        {
            //取出使用者ID
            string UserMail = Helper.GetUserMail();
            int UserID = Models.AnyDMorBuy.GetUserID_UseEmail(UserMail);

            //找出自己完成的訂單
            //之後寄Mail給自己的也可以看到
            var myAllOrder = Models.AnyDMorBuy.GetAllOrdering_UseOrderID(UserID, 1, 2);
            return View(myAllOrder);
        }

        //GET: AnyDMorBuy/OrderingDetail
        public ActionResult OrderingDetail_History(string OrderID)
        {
            if (String.IsNullOrEmpty(OrderID))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var result = Models.AnyDMorBuy.GetOrderingDetail_UseOrderID(OrderID);
            ViewBag.OrderID = OrderID;
            return View(result);
        }

        //GET: AnyDMorBuy/OrderingMemDetail
        public ActionResult OrderingMemDetail_History(string OrderID)
        {
            if (String.IsNullOrEmpty(OrderID))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.OrderID = OrderID;

            //看此訂單有沒有成員資料
            int ordid = Int32.Parse(OrderID);
            var HaveMem = db.CreateBuyOrder_MemberOrders.Where(m => m.OrderID == ordid);
            ViewBag.HaveMem = 0;
            if (HaveMem.FirstOrDefault() != null)
            {
                ViewBag.HaveMem = 1;
            }

            var result = Models.AnyDMorBuy.GetOrderingDetail_UseOrderID(OrderID);
            return View(result);
        }

        //關閉連線
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}