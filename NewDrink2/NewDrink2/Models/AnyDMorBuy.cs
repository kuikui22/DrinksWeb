using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace NewDrink2.Models
{
    public class AnyDMorBuy
    {
        //取出選購菜單檢視View
        public static DMDetailsView BuyDMDetailView_UseMenuID(int MenuID)
        {
            NewDrinkDB db = new NewDrinkDB();            
            var MenuName = db.Menus.Find(MenuID);
            List<DMDrinksDetailView> drinkModel = GetDrinksSizePrice(MenuID);

            var result = new DMDetailsView()
            {
                MenuID = MenuID,
                MenuName = MenuName.MenuName,
                MenuPhon = MenuName.MenuPhone,
                OrderCondition = MenuName.OrderConditions,
                DMDrinksDetailView = drinkModel
            };

            db.Dispose();
            return result;
        }

        //取出飲料價表
        public static List<DMDrinksDetailView> GetDrinksSizePrice(int MenuID)
        {
            NewDrinkDB db = new NewDrinkDB();
            List<DMDrinksDetailView> result = new List<DMDrinksDetailView>();
            var DrinkName = db.MenuDrinks.Where(m => m.MenuID == MenuID);

            foreach (var item in DrinkName)
            {
                List<DMDrinksPriceSize> price = new List<DMDrinksPriceSize>();
                var sizeP = db.SizeTables.Where(m => m.MenuID == MenuID && m.DrinkID == item.ID);
                foreach (var Pitem in sizeP)
                {
                    var SizeName = db.SizeTypes.Find(Pitem.SizeID);
                    price.Add(new DMDrinksPriceSize()
                    {
                        DrinkPrice = Pitem.Price,
                        DrinkSize = SizeName.SizeName
                    });
                }
                result.Add(new DMDrinksDetailView()
                {
                    DrinkID = item.ID,
                    DrinkType = item.DrinkType,
                    DrinkName = item.DrinkName,
                    DMDrinksPriceSize = price
                });
            }

            db.Dispose();
            return result;
        }

        //取出購買飲料細項ViewModel
        public static BuyDMDrinkView GetBuyDMDrinkView_MenuIDdrinkID(int MenuID, int DrinkID)
        {
            NewDrinkDB db = new NewDrinkDB();
            var DrinkName = db.MenuDrinks.Find(DrinkID);

            var mySize = DrinkSize(MenuID, DrinkID);

            var result = new BuyDMDrinkView()
            {
                MenuID = MenuID,
                DrinkID = DrinkID,
                DrinkName = DrinkName.DrinkName,
                Bathus = DrinkName.Bathus,
                SizeType = mySize,
                Quantity = 1
            };

            db.Dispose();
            return result;
        }

        //依照DrinkID,MenuID取出飲料大小
        public static List<SelectListItem> DrinkSize(int MenuID, int DrinkID)
        {
            NewDrinkDB db = new NewDrinkDB();
            List<SelectListItem> mySize = new List<SelectListItem>();
            var result = db.SizeTables.Where(m => m.DrinkID == DrinkID && m.MenuID == MenuID);

            foreach (var item in result)
            {
                var SizeName = db.SizeTypes.Find(item.SizeID);
                var NameS = db.MenuSizes.Where(m => m.MenuID == MenuID && m.SizeID == item.SizeID);
                string NS = "";
                foreach (var NaSitem in NameS)
                {
                    NS = NaSitem.SizeName;
                }
                mySize.Add(new SelectListItem(){Text = NS, Value = item.ID.ToString()});
            }

            db.Dispose();
            return mySize;
        }
      
        //依照MenuID, DrinkID取出飲料甜度(SizePID == 0)
        public static BuyDMDrinkSweet DrinkSweet_UseMenuIDdrinkID(int MenuID, int DrinkID)
        {
            NewDrinkDB db = new NewDrinkDB();
            List<SelectListItem> mySweet = new List<SelectListItem>();
            var result = db.SizeTables.Where(m => m.MenuID == MenuID && m.DrinkID == DrinkID);

            foreach (var item in result)
            {
                var SweetID = db.SweetTables.Where(m => m.SizePID == item.ID);
                foreach (var Sweetitem in SweetID)
                {
                    var SweetN = db.SweetTypes.Find(Sweetitem.SweetID);
                    var NameS = db.MenuSweets.Where(m => m.MenuID == MenuID && m.SweetID == Sweetitem.SweetID);
                    string NS = "";
                    foreach (var NaSitem in NameS)
                    {
                        NS = NaSitem.SweetName;
                    }
                    mySweet.Add(new SelectListItem() { Text = NS, Value = SweetN.ID.ToString() });
                }
                break;
            }

            db.Dispose();
            return new BuyDMDrinkSweet() { SweetType = mySweet };
        }

        //依照SizePID取出飲料甜度
        public static BuyDMDrinkSweet DrinkSweet(int SizePID)
        {
            NewDrinkDB db = new NewDrinkDB();
            List<SelectListItem> mySweet = new List<SelectListItem>();
            var result = db.SweetTables.Where(m => m.SizePID == SizePID);

            foreach (var item in result)
            {
                var SweetN = db.SweetTypes.Find(item.SweetID);
                var menuID = db.SizeTables.Find(SizePID);
                var NameS = db.MenuSweets.Where(m => m.MenuID == menuID.MenuID && m.SweetID == item.SweetID);
                string NS = "";
                foreach (var NaSitem in NameS)
                {
                    NS = NaSitem.SweetName;
                }
                mySweet.Add(new SelectListItem() { Text = NS, Value = SweetN.ID.ToString() });
            }

            db.Dispose();
            return new BuyDMDrinkSweet() { SweetType = mySweet };
        }

        //依照MenuID, DrinkID取出飲料冰度(SizePID == 0)
        public static BuyDMDrinkIce DrinkIce_UseMenuIDdrinkID(int MenuID, int DrinkID)
        {
            NewDrinkDB db = new NewDrinkDB();
            List<SelectListItem> myIce = new List<SelectListItem>();
            var result = db.SizeTables.Where(m => m.MenuID == MenuID && m.DrinkID == DrinkID);

            foreach (var item in result)
            {
                var IceID = db.IceTables.Where(m => m.SizePID == item.ID);
                foreach (var Iceitem in IceID)
                {
                    var NameS = db.MenuIces.Where(m => m.MenuID == MenuID && m.IceID == Iceitem.IceID);
                    string NS = "";
                    foreach (var NaSitem in NameS)
                    {
                        NS = NaSitem.IceName;
                    }
                    var IceN = db.IceTypes.Find(Iceitem.IceID);
                    myIce.Add(new SelectListItem() { Text = NS, Value = IceN.ID.ToString() });
                }
                break;
            }

            db.Dispose();
            return new BuyDMDrinkIce() { IceType = myIce };
        }

        //依照SizePID取出飲料冰度
        public static BuyDMDrinkIce DrinkIce(int SizePID)
        {
            NewDrinkDB db = new NewDrinkDB();
            List<SelectListItem> myIce = new List<SelectListItem>();
            var result = db.IceTables.Where(m => m.SizePID == SizePID);

            foreach (var item in result)
            {
                var IceN = db.IceTypes.Find(item.IceID);
                var menuID = db.SizeTables.Find(SizePID);
                var NameS = db.MenuIces.Where(m => m.MenuID == menuID.MenuID && m.IceID == item.IceID);
                string NS = "";
                foreach (var NaSitem in NameS)
                {
                    NS = NaSitem.IceName;
                }
                myIce.Add(new SelectListItem() { Text = NS, Value = IceN.ID.ToString() });
            }

            db.Dispose();
            return new BuyDMDrinkIce() { IceType = myIce };
        }

        //依照MenuID, DrinkID取出飲料配料(SizePID == 0)
        public static BuyDMDrinkAdditem DrinkAddItem_UseMenuIDdrinkID(int MenuID, int DrinkID)
        {
            NewDrinkDB db = new NewDrinkDB();
            List<SelectListItem> myItem = new List<SelectListItem>();
            var result = db.SizeTables.Where(m => m.MenuID == MenuID && m.DrinkID == DrinkID);

            foreach (var item in result)
            {
                var AdditemPID = db.AddItemTables.Where(m => m.SizePID == item.ID);
                foreach (var additem in AdditemPID)
                {
                    var AdditemP = db.AddItemTypePrices.Find(additem.ItemIDPriceID);
                    var AddN = db.AddItemTypes.Find(AdditemP.ItemID);
                    myItem.Add(new SelectListItem() { Text = AddN.ItemName + " $" + AdditemP.ItemPrice, Value = AdditemP.ID.ToString() });
                }
                break;
            }

            db.Dispose();
            return new BuyDMDrinkAdditem() { AddItem = myItem };
        }

        //依照SizePID取出飲料配料
        public static BuyDMDrinkAdditem DrinkAddItem(int SizePID)
        {
            NewDrinkDB db = new NewDrinkDB();
            List<SelectListItem> myItem = new List<SelectListItem>();
            var result = db.AddItemTables.Where(m => m.SizePID == SizePID);

            foreach (var additem in result)
            {
                var AdditemP = db.AddItemTypePrices.Find(additem.ItemIDPriceID);
                var AddN = db.AddItemTypes.Find(AdditemP.ItemID);
                myItem.Add(new SelectListItem() { Text = AddN.ItemName + " $" + AdditemP.ItemPrice, Value = AdditemP.ID.ToString() });
            }

            db.Dispose();
            return new BuyDMDrinkAdditem() { AddItem = myItem };
        }

        //依照MenuID, DrinkID取出SizePID (SizePID == 0)
        public static int PickSizePid_UseMenuIDdrinkID(int MenuID, int DrinkID)
        {
            NewDrinkDB db = new NewDrinkDB();
            int SizePID = 0;
            var result = db.SizeTables.Where(m => m.MenuID == MenuID && m.DrinkID == DrinkID);
            foreach (var item in result)
            {
                //取找到的第一個
                SizePID = item.ID;
                break;
            }

            db.Dispose();
            return SizePID;
        }

        //以使用者Email取出使用者ID
        public static int GetUserID_UseEmail(string UserMail)
        {
            NewDrinkDB db = new NewDrinkDB();
            int UserID = 0;
            var query = db.Users.Where(m => m.Email == UserMail);
            foreach (var item in query)
            {
                UserID = item.ID;
            }

            db.Dispose();
            return UserID;
        }

        //取出訂單甜度.冰度
        public static int GetSweetIceInt(BuyDMDrinkView Buy, string type)
        {
            int ID = 0;
            switch (type)
            {
                case "sweet":
                    foreach (var item in Buy.SweetTypeM)
                    {
                        ID = Int32.Parse(item.SweetTypeM);
                    }
                    break;
                case "ice":
                    foreach (var item in Buy.IceTypeM)
                    {
                        ID = Int32.Parse(item.IceTypeM);
                    }
                    break;
                default:
                    break;
            }
            return ID;
        }

        //取出訂單配料
        public static List<int> GetAddItemInt(BuyDMDrinkView Buy)
        {
            List<int> ID = new List<int>();

            if (Buy.AddItemType == null)
            {
                return ID;
            }

            foreach (var item in Buy.AddItemType)
            {
                foreach (var additem in item.AddItem)
                {
                    if (additem.Selected)
                    {
                        int id = Int32.Parse(additem.Value);
                        ID.Add(id);
                    }
                }
            }
            return ID;
        }

        //依照給的model取出1-3的配料
        public static int GetOnlyAdditemInt(BuyDMDrinkView Buy, int num)
        {
            var Tot = GetAddItemInt(Buy);
            int ID = 0;

            switch (num)
            {
                case 1:
                    for (int i = 0; i < Tot.Count; i++)
                    {
                        if (i == 0)
                        {
                            ID = Tot[i];
                            break;
                        }
                    }
                    break;
                case 2:
                    for (int i = 0; i < Tot.Count; i++)
                    {
                        if (i == 1)
                        {
                            ID = Tot[i];
                            break;
                        }
                    }
                    break;
                case 3:
                    for (int i = 0; i < Tot.Count; i++)
                    {
                        if (i == 2)
                        {
                            ID = Tot[i];
                            break;
                        }
                    }
                    break;
                default:
                    break;
            }
            return ID;
        }

        //依照給的model取出計算小計
        public static int GetSCount(BuyDMDrinkView Buy)
        {
            NewDrinkDB db = new NewDrinkDB();
            //要SizeID
            //加料價ID
            //數量
            int SizePrice = Int32.Parse(Buy.SizeTypeM);
            var SizeP = db.SizeTables.Find(SizePrice);
            int SCount = SizeP.Price;
            var Add = GetAddItemInt(Buy);

            foreach (var item in Add)
            {
                var AddPrice = db.AddItemTypePrices.Find(item);
                SCount = SCount + AddPrice.ItemPrice;
            }
            SCount = SCount * Buy.Quantity;

            db.Dispose();
            return SCount;
        }

        //取得訂單總覽用的視圖
        public static IList<AllOrder_View> GetAllOrdering_UseOrderID(int UserID, int Finish, int Finish2)
        {
            //0: 選購中 , 1: 取消訂單   2: 結束訂購 3: 跟團中
            NewDrinkDB db = new NewDrinkDB();
            //找出自己未完成的訂單
            //之後寄Mail給自己的也可以看到
            //取出資料
            var myAllOrder = db.CreateBuyOrder_LeaderOrders.Where(m => m.UserID == UserID && (m.CanOrNotOrder == Finish || m.CanOrNotOrder == Finish2));
            var result = new List<AllOrder_View>();

            //如果自己的開團紀錄超過10筆則刪除超過的舊紀錄(依照結束時間判斷)
            if (myAllOrder.Count() > 10 && (Finish == 1 || Finish == 2) || (Finish2 == 1 || Finish2 == 2))
            {
                //依時間遞增排序
                myAllOrder = myAllOrder.OrderBy(m => m.UserID).ThenBy(m => m.EndThisTime);
                while (myAllOrder.Count() > 10)
                {
                    foreach (var item in myAllOrder)
                    {
                        //刪除開團訂單
                        var delete = db.CreateBuyOrder_LeaderOrders.Find(item.OrderID);
                        db.CreateBuyOrder_LeaderOrders.Remove(delete);

                        //刪除開團詳細
                        var delete2 = db.CreateBuyOrder_Details.Where(m => m.OrderID == item.OrderID);
                        db.CreateBuyOrder_Details.RemoveRange(delete2);
                        db.SaveChanges();
                        break;
                    }
                }
            }
          
            foreach (var item in myAllOrder)
            {
                //看自己有沒有在自己開的團下單
                var MyDoOrder = db.CreateBuyOrder_Details.Where(m => m.UserID == UserID && m.OrderID == item.OrderID);
                var HaveDrink = 0;
                if (MyDoOrder.FirstOrDefault() != null)
                {
                    HaveDrink = 1;
                }

                var MenuName = db.Menus.Find(item.MenuID);
                var UserName = db.Users.Find(item.UserID);
                result.Add(new AllOrder_View()
                {
                    OrderID = item.OrderID,
                    MenuName = MenuName.MenuName,
                    UserName = UserName.Name,
                    CreateTime = item.CreateTime,
                    CanOrder = item.CanOrNotOrder,
                    HaveDrink = HaveDrink
                });
            }

            //加入寄Mail給自己的(揪團)
            //先看自己的糾團訊息內的orderID(取出邀請的orderID)
            //已orderID看DetailID裡有沒有自己的下單紀錄(有則取出,做記號)
            //即使沒有下單紀錄也要顯示出來
            if (Finish == 0 || Finish2 == 0)
            {
                //從團員名單中找出自己被邀請的訂單
                //var Checkmyorder = db.SendMessageViews.Where(m => m.UserID == UserID);
                var Checkmyorder = db.CreateBuyOrder_MemberOrders.Where(m => m.UserID == UserID);
                if (Checkmyorder.FirstOrDefault() != null)
                {
                    foreach (var item in Checkmyorder)
                    {                        
                        //檢查自己是否有下單
                        var DoOrder = db.CreateBuyOrder_Details.Where(m => m.UserID == UserID && m.OrderID == item.OrderID);
                        //且訂單尚未取消或結束
                        var NoEnd = db.CreateBuyOrder_LeaderOrders.Where(m => m.OrderID == item.OrderID && (m.CanOrNotOrder == 0 || m.CanOrNotOrder == 3));
                        //這裡要判斷要不要做記號(表示有下單)
                        if (/*DoOrder.FirstOrDefault() != null &&*/ NoEnd.FirstOrDefault() != null)
                        {
                            var AboutOrder = db.CreateBuyOrder_LeaderOrders.Find(item.OrderID);
                            var MenuName = db.Menus.Find(AboutOrder.MenuID);
                            var UserName = db.Users.Find(AboutOrder.UserID);
                            var HaveDrink = 0;
                            if (DoOrder.FirstOrDefault() != null)
                            {
                                HaveDrink = 1;
                            }

                            result.Add(new AllOrder_View()
                            {
                                OrderID = item.OrderID,
                                MenuName = MenuName.MenuName,
                                UserName = UserName.Name,
                                CreateTime = AboutOrder.CreateTime,
                                CanOrder = 3,
                                HaveDrink = HaveDrink
                            });
                        }
                    }
                }
            }
            if ((Finish == 1 || Finish == 2) || (Finish2 == 1 || Finish2 == 2))
            {
                //先看結束的orderID裡有沒有自己的訂單
                //即使沒有下單紀錄也要顯示出來

                var CheckHistory = db.CreateBuyOrder_LeaderOrders.Where(m => (m.CanOrNotOrder == 1 || m.CanOrNotOrder == 2) && m.UserID != UserID);
                foreach (var itemID in CheckHistory)
                {
                    //用orderID去DetailID找看看自己有沒有下訂單(有則取出)
                    //var CheckDetail = db.CreateBuyOrder_Details.Where(m => m.UserID == UserID && m.OrderID == itemID.OrderID);
                    var Checkmyorder = db.CreateBuyOrder_MemberOrders.Where(m => m.UserID == UserID && m.OrderID == itemID.OrderID);

                    //if (CheckDetail.FirstOrDefault() != null)
                    //{
                    //    var MenuName = db.Menus.Find(itemID.MenuID);
                    //    var UserName = db.Users.Find(itemID.UserID);
                    //    result.Add(new AllOrder_View()
                    //    {
                    //        OrderID = itemID.OrderID,
                    //        MenuName = MenuName.MenuName,
                    //        UserName = UserName.Name,
                    //        CreateTime = itemID.CreateTime,
                    //        CanOrder = itemID.CanOrNotOrder
                    //    });
                    //}

                    if (Checkmyorder.FirstOrDefault() != null)
                    {
                        var MenuName = db.Menus.Find(itemID.MenuID);
                        var UserName = db.Users.Find(itemID.UserID);
                        result.Add(new AllOrder_View()
                        {
                            OrderID = itemID.OrderID,
                            MenuName = MenuName.MenuName,
                            UserName = UserName.Name,
                            CreateTime = itemID.CreateTime,
                            CanOrder = itemID.CanOrNotOrder
                        });
                    }

                }
            }

            db.Dispose();
            result = result.OrderByDescending(m => m.CreateTime).ToList();
            return result;
        }

        //取得訂單詳情的視圖
        public static OrderingDetailView GetOrderingDetail_UseOrderID(string OrderID)
        {
            NewDrinkDB db = new NewDrinkDB();
            var Mail = Helper.GetUserMail();
            int userID = Models.AnyDMorBuy.GetUserID_UseEmail(Mail);
            int orderid = Int32.Parse(OrderID);
            //將通知已讀掉
            var IduMessage = db.SendMessageViews.Where(m => m.OrderID == orderid && m.UserID == userID);
            if (IduMessage != null)
            {
                foreach (var item in IduMessage)
                {
                    item.ReadOrNot = true;
                }
                db.SaveChanges();
            }

            List<OrderingDetailView_Detail> Details = new List<OrderingDetailView_Detail>();
            //取出資料
            CreateBuyOrder_LeaderOrder AllCount = db.CreateBuyOrder_LeaderOrders.Find(orderid);
            var order = db.CreateBuyOrder_Details.Where(m => m.OrderID == orderid);
            int AllQuantity = 0;

            foreach (var item in order)
            {
                AllQuantity = AllQuantity + item.Quantity;
                var UserN = db.Users.Find(item.UserID);
                var DrinkN = db.MenuDrinks.Find(item.DrinkID);
                var SizeP = db.SizeTables.Find(item.SizePID);
                var Size = db.SizeTypes.Find(SizeP.SizeID);
                var Sweet = db.SweetTypes.Find(item.SweetID);
                var Ice = db.IceTypes.Find(item.IceID);
                //取出各項名稱
                //尺寸
                var SNsize = db.MenuSizes.Where(m => m.MenuID == item.MenuID && m.SizeID == SizeP.SizeID);
                string SNsz = "";
                foreach (var SNszitem in SNsize)
                {
                    SNsz = SNszitem.SizeName;
                    break;
                }
                //甜度
                var SNsweet = db.MenuSweets.Where(m => m.MenuID == item.MenuID && m.SweetID == item.SweetID);
                string SNsw = "";
                foreach (var SNszitem in SNsweet)
                {
                    SNsw = SNszitem.SweetName;
                    break;
                }
                //冰度
                var SNice = db.MenuIces.Where(m => m.MenuID == item.MenuID && m.IceID == item.IceID);
                string SNi = "";
                foreach (var SNszitem in SNice)
                {
                    SNi = SNszitem.IceName;
                    break;
                }

                //去除頭尾的"."
                var itemT = Additem01_03N(item.Additem01PID) + " ." + Additem01_03N(item.Additem02PID) + " ." + Additem01_03N(item.Additem03PID);
                itemT = itemT.Trim();
                itemT = itemT.Trim('.');               

                Details.Add(new OrderingDetailView_Detail()
                {
                    DetailID = item.DetailID,
                    UserID = item.UserID,
                    UserName = UserN.Name,
                    DrinkName = DrinkN.DrinkName,
                    DrinkSize = SNsz,
                    DrinkSweet = SNsw,
                    DrinkIce = SNi,
                    DrinkAdditem = itemT,
                    SCount = item.SCount,
                    SQuantity = item.Quantity
                });
            }

            var result = new OrderingDetailView()
            {
                OrderID = orderid,
                AllCount = AllCount.TotalCount,
                AllQuantity = AllQuantity,
                OrderingDetailView_Detail = Details
            };

            db.Dispose();
            return result;
        }

        //取出飲料配料的名稱
        public static string Additem01_03N(int addID)
        {
            NewDrinkDB db = new NewDrinkDB();
            string addN = "";
            if (addID != 0)
            {
                var addName = db.AddItemTypePrices.Find(addID);
                var additemName = db.AddItemTypes.Find(addName.ItemID);
                addN = additemName.ItemName;
            }
            
            db.Dispose();
            return addN;
        }

        //取出飲料細節編輯的視圖
        public static BuyDMDrinkView GetDrinkDetail_EditDrink(int DetailID)
        {
            NewDrinkDB db = new NewDrinkDB();
            var DrinkDetail = db.CreateBuyOrder_Details.Find(DetailID);
            var DrinkN = db.MenuDrinks.Find(DrinkDetail.DrinkID);
            var SizeNT = db.SizeTables.Find(DrinkDetail.SizePID);
            var SizeN = db.SizeTypes.Find(SizeNT.SizeID);
            var mySize = DrinkSize(DrinkDetail.MenuID, DrinkDetail.DrinkID);
            foreach (var item in mySize)
            {
                if (item.Text == SizeN.SizeName && item.Value == DrinkDetail.SizePID.ToString())
                {
                    item.Selected = true;
                }
            }

            var result = new BuyDMDrinkView()
            {
                MenuID = DrinkDetail.MenuID,
                DrinkID = DrinkDetail.DrinkID,
                DrinkName = DrinkN.DrinkName,
                Bathus = DrinkN.Bathus,
                SizeType = mySize,
                SizeTypeM = DrinkDetail.SizePID.ToString(),
                Quantity = DrinkDetail.Quantity
            };

            db.Dispose();
            return result;
        }

        //用OrderID取出團長ID
        public static int GetLeaderID_UseOrderID(string OrderID)
        {
            NewDrinkDB db = new NewDrinkDB();
            int orderid = Int32.Parse(OrderID);
            var query = db.CreateBuyOrder_LeaderOrders.Find(orderid);

            db.Dispose();
            return query.UserID;
        }

        //作Guid加解密
        public static string GuidwithOrderLogin(Guid guid, string plainText)
        {
            byte[] data = ASCIIEncoding.ASCII.GetBytes(plainText + guid.ToString());
            byte[] result;
            SHA512Managed sha = new SHA512Managed();
            result = sha.ComputeHash(data);
            return Convert.ToBase64String(result);
        }

        //取出已勾選的成員
        public static List<int> CheckMem(ChangeMem Nsis)
        {
            var addItem = new List<int>();

            foreach (var item in Nsis.Member)
            {
                if (item.Selected)
                {
                    //加到陣列
                    var DD = Int32.Parse(item.Value);
                    addItem.Add(DD);
                }
                else
                {
                    //跳過
                    continue;
                }
            }

            return addItem;
        }

        //判斷寄信時間
        public static bool CheckCanSend_Message(DateTime NowTime, DateTime CheckTime)
        {
            string NTime = NowTime.ToString("yyyyMMdd");
            string LTime = CheckTime.ToString("yyyyMMdd");

            //判斷是否同一天
            if (NTime != LTime)
            {
                return false;
            }

            //先判斷上下午
            string W = NowTime.ToString("tt");
            string N = CheckTime.ToString("tt");

            string ss = NowTime.ToString("HH");
            string ee = CheckTime.ToString("HH");

            if (W == "下午" && ee == "00")
            {
                ee = "22";
            }
            if (W == "上午" && ee == "00")
            {
                ee = "12";
            }

            if (ss != ee)
            {
                return false;
            }

            //判斷分鐘
            string NowM = NowTime.ToString("mm");
            string ChkM = CheckTime.ToString("mm");

            int Ntime = Int32.Parse(NowM);
            int Ctime = Int32.Parse(ChkM);

            if (Ntime != Ctime && Ntime - Ctime >= 5)
            {
                return false;
            }

            if (Ntime == Ctime || Ntime - Ctime <= 5)
            {
                return true;
            }

            return true;

        }

        //取出以寄出邀請團員的資料
        public static bool CheckCanSend_members(int OrderID, string Type)
        {
            //先取出有寄出邀請的團員
            NewDrinkDB db = new NewDrinkDB();
            var Check = db.CreateBuyOrder_MemberOrders.Where(m => m.OrderID == OrderID);

            //先判斷有沒寄過結束通知
            var HaveEndMessage = db.OrderEndingSends.Where(m => m.OrderID == OrderID);

            var UserMail = Helper.GetUserMail();
            var UserName = Helper.GetUserName();
            int TypeNum = 0;
            if (Type == "取消")
            {
                TypeNum = 0;
            }
            if (Type == "結束")
            {
                TypeNum = 1;
            }

            if (Check.FirstOrDefault() != null && HaveEndMessage.FirstOrDefault() == null)
            {
                //寄出郵件
                foreach (var item in Check)
                {
                    //取出團員及訂單資訊
                    //寄出的內容: 團長名在X年X月X日結束或取消了這筆訂單.
                    var reMem = db.Users.Find(item.UserID);
                    var time = DateTime.Now;
                    var MenuCon = db.LeaderSendMessages.Where(m => m.OrderID == OrderID);

                    string sendTime = null;
                    string subject2 = null;
                    foreach (var Menu in MenuCon)
                    {
                        sendTime = Menu.SendTime.ToString();
                        subject2 = Menu.Subject;
                    }

                    //團員mail
                    string to = reMem.Email.ToString();
                    //團長mail
                    string from = UserMail;
                    string NewBody = UserName + Type + "了於" + sendTime + "寄出,主旨為" + subject2 + "的訂單.";
                    string subject = subject2 + "訂單已" + Type;
                    MailMessage mailMessage = new MailMessage(from, to);
                    mailMessage.Subject = subject;
                    mailMessage.Body = NewBody;
                    string smtpServer = "mail.bingotimes.com.tw";
                    //string smtpServer = "127.0.0.1";
                    SmtpClient client = new SmtpClient(smtpServer, 25);

                    //寄之前暫停1秒
                    int millsecondsTimeout = 1000;
                    System.Threading.Thread.Sleep(millsecondsTimeout);

                    try
                    {
                        client.Send(mailMessage);
                    }
                    catch (Exception e)
                    {
                        
                    }

                    var EndTime = db.CreateBuyOrder_LeaderOrders.Find(OrderID);

                    //寫入資料庫
                    var SendEnd = db.OrderEndingSends.Add(new OrderEndingSend()
                    {
                        OrderID = item.OrderID,
                        EndOrCancel = TypeNum,
                        ToUser = reMem.ID,
                        ReadOrNot = false,
                        EndOverTime = EndTime.EndThisTime
                    });
                    db.SaveChanges();

                }
            }

            return true;

        }

        //區別有沒有下訂單及尚未回覆或不訂
        public static List<MyMember_Response> MyMem_Response(int OrderID)
        {
            //先在團員名單中找出團員
            NewDrinkDB db = new NewDrinkDB();
            var AllMem = db.CreateBuyOrder_MemberOrders.Where(m => m.OrderID == OrderID);
            List<MyMember_Response> result = new List<MyMember_Response>();

            if (AllMem.FirstOrDefault() != null)
            {
                //依照團員的狀態去顯示團員是否不訂飲料
                //0 -> 不訂
                //1 -> 尚未表態或是有訂
                foreach (var item in AllMem)
                {
                    //取出團員名稱
                    var UserName = db.Users.Find(item.UserID);

                    //如果狀態為0,則直接寫入(不訂飲料)
                    if (item.Together == false)
                    {
                        result.Add(new MyMember_Response()
                        {
                            MemID = item.MemID,
                            OrderID = item.OrderID,
                            UserID = item.UserID,
                            Together = item.Together,
                            UserName = UserName.Name
                        });
                    }//End of Second IF.

                    //如果狀態為1,則判斷是否有商品訂單,若沒有則直接寫入(尚未回覆)
                    if (item.Together == true)
                    {
                        var HaveOrder = db.CreateBuyOrder_Details.Where(m => m.UserID == item.UserID && m.OrderID == OrderID);
                        if (HaveOrder.FirstOrDefault() == null)
                        {
                            result.Add(new MyMember_Response()
                            {
                                MemID = item.MemID,
                                OrderID = item.OrderID,
                                UserID = item.UserID,
                                Together = item.Together,
                                UserName = UserName.Name
                            });
                        }
                    }//End of Second IF.

                }//End of Foreach IF.
            }//End of Main IF.

            //因該要另做一個視圖將未訂,及不訂的放一起

            return result;
        }


    }
}