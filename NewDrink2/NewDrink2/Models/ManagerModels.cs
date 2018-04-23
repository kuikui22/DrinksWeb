using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NewDrink2.Models
{
    public class ManagerModels
    {
        public static NewMenu GetFakeMenus()
        {
            return new NewMenu()
            {
                MenuName = "",
                MenuPhone = "",
                Addr = "",
                Open = false,
                Sweet = FirstGetSweet(),
                IceHot = FirstGetIce(),
                Size = FirstGetSize(),
                AddItemCreate = new List<AddItemCreate>()
                {
                    new AddItemCreate()
                    {
                        ItemName = "",
                        LPrice = 0,
                        MPrice = 0,
                        SPrice = 0
                    }
                }
            };
        }

        public static NewMenu GetFakeMenus2()
        {
            return new NewMenu()
            {
                MenuName = "",
                MenuPhone = "",
                Addr = "",
                Open = false,
                Sweet = FirstGetSweet(),
                IceHot = FirstGetIce(),
                Size = FirstGetSize()               
            };
        }

        //取得錯誤返回值(NewMenu)
        public static NewMenu ErrorBackMenu(NewMenu menu)
        {
            menu.Sweet = FirstGetSweet();
            menu.Size =  FirstGetSize();
            menu.IceHot = FirstGetIce();
            return menu;
        }

        //取得目前現有甜度
        public static IList<SelectListItem> FirstGetSweet()
        {
            NewDrinkDB db = new NewDrinkDB();
            var query = db.SweetTypes.ToList();

            var result = new List<SelectListItem>();
            foreach (var item in query)
            {
                result.Add(
                    new SelectListItem { Text = item.SweetName, Value = item.ID.ToString()}
                );
            }

            db.Dispose();
            return result;
        }

        //取得目前現有冰度
        public static IList<SelectListItem> FirstGetIce()
        {
            NewDrinkDB db = new NewDrinkDB();
            var query = db.IceTypes.ToList();

            var result = new List<SelectListItem>();
            foreach (var item in query)
            {
                result.Add(
                    new SelectListItem { Text = item.IceName, Value = item.ID.ToString()}
                );
            }

            db.Dispose();
            return result;
        }

        //取得目前現有尺寸
        public static IList<SelectListItem> FirstGetSize()
        {
            NewDrinkDB db = new NewDrinkDB();
            var query = db.SizeTypes.ToList();

            var result = new List<SelectListItem>();
            foreach (var item in query)
            {
                result.Add(
                    new SelectListItem { Text = item.SizeName, Value = item.ID.ToString() }
                );
            }

            db.Dispose();
            return result;
        }

        //判斷加料金額不為負值
        public static int PriceIsNotSmall(NewMenu newM)
        {
            int Price = 0;
            foreach (var item in newM.AddItemCreate)
            {
                if (item.LPrice < 0 || item.MPrice < 0 || item.SPrice < 0)
                {
                    Price = -1;
                    break;
                }
                Price = 1;
            }

            return Price;
        }

        //將甜度,冰度,尺寸加入陣列
        public static List<int> CheckCheckboxNotEmpty(NewMenu newM, string type)
        {
            var addItem = new List<int>();
            switch (type)
            {
                case "sweet":
                    foreach (var item in newM.Sweet)
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
                    break;
                case "ice":
                    foreach (var item in newM.IceHot)
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
                    break;
                case "size":
                    foreach (var item in newM.Size)
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
                    break;
                default:
                    break;
            }

            return addItem;
        }

        //以菜單名取出菜單ID
        public static int MenuID(string MenuName)
        {
            NewDrinkDB db = new NewDrinkDB();
            int MenuID = 0;
            var query = db.Menus.Where(m => m.MenuName == MenuName).Select(m => new { ID = m.ID });
            foreach (var item in query)
            {
                MenuID = item.ID;
            }

            db.Dispose();
            return MenuID;
        }

        //以甜度名取出甜度ID(冰度,尺寸,加料也在這取)
        public static int SweetID(string Name, string type)
        {
            NewDrinkDB db = new NewDrinkDB();
            int MenuID = 0;
            switch (type)
            {
                case "sweet":
                    var query = db.SweetTypes.Where(m => m.SweetName == Name).Select(m => new { ID = m.ID });
                    foreach (var item in query)
                    {
                        MenuID = item.ID;
                    }
                    break;
                case "ice":
                    var query2 = db.IceTypes.Where(m => m.IceName == Name).Select(m => new { ID = m.ID });
                    foreach (var item in query2)
                    {
                        MenuID = item.ID;
                    }
                    break;
                case "size":
                    var query3 = db.SizeTypes.Where(m => m.SizeName == Name).Select(m => new { ID = m.ID });
                    foreach (var item in query3)
                    {
                        MenuID = item.ID;
                    }
                    break;
                case "additem":
                    var query4 = db.AddItemTypes.Where(m => m.ItemName == Name).Select(m => new { ID = m.ID });
                    foreach (var item in query4)
                    {
                        MenuID = item.ID;
                    }
                    break;
                default:
                    break;
            }
            db.Dispose();
            return MenuID;
        }

        //依MenuID取出加料價表的ID
        public static List<int> PickAllItemPrice(int MenuID)
        {
            NewDrinkDB db = new NewDrinkDB();
            var result = new List<int>();
            //取出全價表
            var query = db.AddItemTypePrices.Where(m => m.MenuID == MenuID);
            foreach (var item in query)
            {
                result.Add(item.ID);
            }

            db.Dispose();
            return result;
        }

        //取出建立飲料細節的ViewModel
        public static AddDrinkDetails CurrentAddDrinkDetails(int ID)
        {
            NewDrinkDB db = new NewDrinkDB();
            Menu MenuName = db.Menus.Find(ID);

            //取出飲料尺寸的ViewModel
            List<SelectListItem> mySize = ManagerModels.mySize(ID);

            //取出飲料甜度的ViewModel
            IList<SelectListItem> mySweet = ManagerModels.mySweet(ID);

            //取出飲料冰度的ViewModel
            IList<SelectListItem> myIce = ManagerModels.myIce(ID);

            //取出飲料配料的ViewModel
            IList<SelectListItem> myAddItem = ManagerModels.myAddItem(ID);
            
            db.Dispose();
            return new AddDrinkDetails()
            {
                MenuID = MenuName.ID,
                MenuName = MenuName.MenuName,
                DrinkDetails = new List<DrinkDetails>()
                {
                    new DrinkDetails()
                    {
                        DrinkType = "",
                        DrinkName = "",
                        SizeType = mySize,
                        Sweet = mySweet,
                        IceHot = myIce,
                        AddItem = myAddItem,
                        DrinkPrice = 0
                    }
                }
            };
        }

        //取出飲料尺寸的ViewModel
        public static List<SelectListItem> mySize(int id)
        {
            NewDrinkDB db = new NewDrinkDB();

            //先從店家尺寸表取SizeID
            var query = db.MenuSizes.Where(m => m.MenuID == id);

            //再以Size到尺寸總表取Size名字
            List<SelectListItem> mySize = new List<SelectListItem>();
            foreach (var item in query)
            {
                var query2 = db.SizeTypes.Find(item.SizeID);
                mySize.Add(new SelectListItem()
                {
                    Text = item.SizeName,
                    Value = item.SizeID.ToString(),
                });
            }

            db.Dispose();
            return mySize;
        }

        //取出飲料甜度的ViewModel(checkbox)
        public static IList<SelectListItem> mySweet(int id)
        {
            NewDrinkDB db = new NewDrinkDB();

            //先從店家甜度表取SweetID
            var query = db.MenuSweets.Where(m => m.MenuID == id);

            var result = new List<SelectListItem>();
            foreach (var item in query)
            {
                var query2 = db.SweetTypes.Find(item.SweetID);
                result.Add(
                    new SelectListItem { Text = item.SweetName, Value = item.SweetID.ToString(), Selected = true }
                );
            }

            db.Dispose();
            return result;
        }

        //取出飲料冰度的ViewModel(checkbox)
        public static IList<SelectListItem> myIce(int id)
        {
            NewDrinkDB db = new NewDrinkDB();

            //先從店家冰度表取IceID
            var query = db.MenuIces.Where(m => m.MenuID == id);

            var result = new List<SelectListItem>();
            foreach (var item in query)
            {
                var query2 = db.IceTypes.Find(item.IceID);
                result.Add(
                    new SelectListItem { Text = item.IceName, Value = item.IceID.ToString(), Selected = true }
                );
            }

            db.Dispose();
            return result;
        }

        //取出飲料加料價的ViewModel(checkbox)
        public static IList<SelectListItem> myAddItem(int id)
        {
            NewDrinkDB db = new NewDrinkDB();

            //先從店家加料價表取AdditemID
            var query = db.AddItemTypePrices.Where(m => m.MenuID == id);

            var result = new List<SelectListItem>();
            foreach (var item in query)
            {
                string Size = "";
                if (item.SizeNumber == 0)
                {
                    Size = "大";
                }
                if (item.SizeNumber == 1)
                {
                    Size = "中";
                }
                if (item.SizeNumber == 2)
                {
                    Size = "小";
                }

                var query2 = db.AddItemTypes.Find(item.ItemID);
                result.Add(
                    new SelectListItem { Text = query2.ItemName + "(" + Size + ") $" + item.ItemPrice, Value = item.ID.ToString() }
                );
            }

            db.Dispose();
            return result;
        }

        //取出動態新增飲料細節的ViewModel
        public static DrinkDetails PartialAddDrinkDetails(int ID)
        {
            //取出飲料尺寸的ViewModel
            List<SelectListItem> mySize = ManagerModels.mySize(ID);

            //取出飲料甜度的ViewModel
            IList<SelectListItem> mySweet = ManagerModels.mySweet(ID);

            //取出飲料冰度的ViewModel
            IList<SelectListItem> myIce = ManagerModels.myIce(ID);

            //取出飲料冰度的ViewModel
            IList<SelectListItem> myAddItem = ManagerModels.myAddItem(ID);

            return new DrinkDetails() { SizeType = mySize, Sweet = mySweet, IceHot = myIce, AddItem = myAddItem, DrinkPrice = 0 };
        }

        //判斷飲料金額不為負值
        public static int DrinkPriceIsNotSmall(AddDrinkDetails addDks)
        {
            int Price = 0;
            foreach (var item in addDks.DrinkDetails)
            {
                if (item.DrinkPrice < 0)
                {
                    Price = -1;
                    break;
                }
                Price = 1;
            }

            return Price;
        }

        //取出返回動態新增飲料細節的ViewModel
        public static AddDrinkDetails BackAddDrinkDetails(AddDrinkDetails addDks)
        {
            //取出飲料尺寸的ViewModel
            List<SelectListItem> mySize = ManagerModels.mySize(addDks.MenuID);

            //取出飲料甜度的ViewModel
            IList<SelectListItem> mySweet = ManagerModels.mySweet(addDks.MenuID);

            //取出飲料冰度的ViewModel
            IList<SelectListItem> myIce = ManagerModels.myIce(addDks.MenuID);

            //取出飲料冰度的ViewModel
            IList<SelectListItem> myAddItem = ManagerModels.myAddItem(addDks.MenuID);

            foreach (var item in addDks.DrinkDetails)
            {
                item.SizeType = mySize;
                item.Sweet = mySweet;
                item.IceHot = myIce;
                item.AddItem = myAddItem;
            }

            return addDks;
        }

        //將甜度,冰度,加料加入陣列(新增飲品用)
        public static List<int> AddDrink_CheckboxNotEmpty(DrinkDetails details, string type)
        {
            var addItem = new List<int>();
            switch (type)
            {
                case "sweet":
                    foreach (var item in details.Sweet)
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
                    break;
                case "ice":
                    foreach (var item in details.IceHot)
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
                    break;
                case "additem":
                    if (details.AddItem != null)
                    {
                        foreach (var item in details.AddItem)
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
                    }
                    break;
                default:
                    break;
            }

            return addItem;
        }

        //依照MenuID.DrinkType.DrinkName取出飲料ID
        public static int PickDrinkID_UseModel(DrinkDetails detail, int MenuID)
        {
            NewDrinkDB db = new NewDrinkDB();
            int DrinkID = 0;
            var query = db.MenuDrinks.Where(m => m.MenuID == MenuID && m.DrinkType == detail.DrinkType && m.DrinkName == detail.DrinkName).Select(m => new { ID = m.ID });
            foreach (var item in query)
            {
                DrinkID = item.ID;
            }

            db.Dispose();
            return DrinkID;
        }

        //依照DrinkID,再用DrinkID.SizeID.Price取SizePID
        public static int PickSizePID_UseDidSidPrice(DrinkDetails detail, int DrinkID)
        {
            NewDrinkDB db = new NewDrinkDB();
            int SizePID = 0;
            int SizeID = Int32.Parse(detail.SizeTypeM);
            var query = db.SizeTables.Where(m => m.DrinkID == DrinkID && m.SizeID == SizeID && m.Price == detail.DrinkPrice).Select(m => new { ID = m.ID });

            foreach (var item in query)
            {
                SizePID = item.ID;
            }

            db.Dispose();
            return SizePID;
        }

        //創飲料建檢視視圖
        public static IList<MenuDrinkNameType> GetMenuDrinkView(int MenuID, string MenuName)
        {
            NewDrinkDB db = new NewDrinkDB();
            var DetailResult = new List<MenuDrinkNameType>();
            //取出飲料名稱.種類
            var query = db.MenuDrinks.Where(m => m.MenuID == MenuID);
            foreach (var drinkNameT in query)
            {
                //取出飲料大小.單價
                var query2 = db.SizeTables.Where(m => m.DrinkID == drinkNameT.ID);
                foreach (var sizePrice in query2)
                {
                    var Size = db.SizeTypes.Find(sizePrice.SizeID);
                    //取出飲料甜度總數
                    var SweetTot = db.SweetTables.Where(m => m.SizePID == sizePrice.ID);
                    //取出飲料冰度總數
                    var IceTot = db.IceTables.Where(m => m.SizePID == sizePrice.ID);
                    //取出飲料加料總數
                    var AddItemTot = db.AddItemTables.Where(m => m.SizePID == sizePrice.ID);
                    //寫入視圖
                    DetailResult.Add(new MenuDrinkNameType()
                    {
                        MenuID = MenuID,
                        DrinkID = drinkNameT.ID,
                        DrinkType = drinkNameT.DrinkType,
                        DrinkName = drinkNameT.DrinkName,
                        SizePID = sizePrice.ID,
                        SizeName = Size.SizeName,
                        DrinkPrice = sizePrice.Price,
                        CanUseSweet = SweetTot.Count(),
                        CanUseIce = IceTot.Count(),
                        CanUseAddItem = AddItemTot.Count(),
                        DrinkSort = sizePrice.DrinkSort
                    });
                }
            }//End main foreach.

            db.Dispose();
            return DetailResult;
        }

        //飲料編輯的ViewModel
        public static DrinkDetails GetEditDrinkDetail_UseSizePID(int SizePID)
        {
            NewDrinkDB db = new NewDrinkDB();
            SizeTable query = db.SizeTables.Find(SizePID);

            //取出飲料種類.名稱
            MenuDrink drinkNameT = db.MenuDrinks.Find(query.DrinkID);

            //分別取出尺寸,甜度,冰度,加料
            var size = ManagerModels.mySize(query.MenuID);
            var sweet = ManagerModels.AllreadyMySweet(SizePID);
            var ice = ManagerModels.AllreadyMyIce(SizePID);
            var additem = ManagerModels.AllreadyMyAddItem(SizePID);

            DrinkDetails result = new DrinkDetails()
            {
                ID = SizePID,
                DrinkType = drinkNameT.DrinkType,
                DrinkName = drinkNameT.DrinkName,
                DrinkPrice = query.Price,
                SizeType = size,
                SizeTypeM = query.SizeID.ToString(),
                Sweet = sweet,
                IceHot = ice,
                AddItem = additem
            };

            db.Dispose();
            return result;
        }

        //取出已勾選的飲料甜度的ViewModel(checkbox)
        public static IList<SelectListItem> AllreadyMySweet(int SizePID)
        {
            NewDrinkDB db = new NewDrinkDB();

            SizeTable Sizequery = db.SizeTables.Find(SizePID);
            //先從店家甜度表取SweetID
            var query = db.MenuSweets.Where(m => m.MenuID == Sizequery.MenuID);

            //查看勾選了哪些項目
            var sweetTable = db.SweetTables.Where(m => m.SizePID == SizePID);

            var result = new List<SelectListItem>();
            foreach (var item in query)
            {
                //從SweetTable將已勾選的項目勾選
                var query2 = db.SweetTypes.Find(item.SweetID);
                bool AreCheck = false;

                foreach (var checkSweet in sweetTable)
                {
                    if (checkSweet.SweetID == item.SweetID)
                    {
                        AreCheck = true;
                    }
                }

                if (AreCheck)
                {
                    result.Add(
                        new SelectListItem { Text = item.SweetName, Value = item.SweetID.ToString(), Selected = true }
                    );
                }
                else
                {
                    result.Add(
                        new SelectListItem { Text = query2.SweetName, Value = item.SweetID.ToString(), Selected = false }
                    );
                }
            }

            db.Dispose();
            return result;
        }

        //取出已勾選的飲料冰度的ViewModel(checkbox)
        public static IList<SelectListItem> AllreadyMyIce(int SizePID)
        {
            NewDrinkDB db = new NewDrinkDB();

            SizeTable Sizequery = db.SizeTables.Find(SizePID);
            //先從店家冰度表取IceID
            var query = db.MenuIces.Where(m => m.MenuID == Sizequery.MenuID);

            //查看勾選了哪些項目
            var iceTable = db.IceTables.Where(m => m.SizePID == SizePID);

            var result = new List<SelectListItem>();
            foreach (var item in query)
            {
                //從iceTable將已勾選的項目勾選
                var query2 = db.IceTypes.Find(item.IceID);
                bool AreCheck = false;

                foreach (var checkIce in iceTable)
                {
                    if (checkIce.IceID == item.IceID)
                    {
                        AreCheck = true;
                    }
                }

                if (AreCheck)
                {
                    result.Add(
                        new SelectListItem { Text = item.IceName, Value = item.IceID.ToString(), Selected = true }
                    );
                }
                else
                {
                    result.Add(
                        new SelectListItem { Text = query2.IceName, Value = item.IceID.ToString(), Selected = false }
                    );
                }               
            }

            db.Dispose();
            return result;
        }

        //取出已勾選的飲料加料價的ViewModel(checkbox)
        public static IList<SelectListItem> AllreadyMyAddItem(int SizePID)
        {
            NewDrinkDB db = new NewDrinkDB();

            SizeTable Sizequery = db.SizeTables.Find(SizePID);
            //先從店家加料價表取AdditemID
            var query = db.AddItemTypePrices.Where(m => m.MenuID == Sizequery.MenuID);

            //查看勾選了哪些項目
            var additemTable = db.AddItemTables.Where(m => m.SizePID == SizePID);

            var result = new List<SelectListItem>();
            foreach (var item in query)
            {
                //從additemTable將已勾選的項目勾選
                string Size = "";
                if (item.SizeNumber == 0)
                {
                    Size = "大";
                }
                if (item.SizeNumber == 1)
                {
                    Size = "中";
                }
                if (item.SizeNumber == 2)
                {
                    Size = "小";
                }

                var query2 = db.AddItemTypes.Find(item.ItemID);
                bool AreCheck = false;

                foreach (var checkadd in additemTable)
                {
                    if (checkadd.ItemIDPriceID == item.ID)
                    {
                        AreCheck = true;
                    }
                }

                if (AreCheck)
                {
                    result.Add(
                        new SelectListItem { Text = query2.ItemName + "(" + Size + ") $" + item.ItemPrice, Value = item.ID.ToString(), Selected = true }
                    );
                }
                else
                {
                    result.Add(
                        new SelectListItem { Text = query2.ItemName + "(" + Size + ") $" + item.ItemPrice, Value = item.ID.ToString(), Selected = false }
                    );
                }
            }

            db.Dispose();
            return result;
        }

        //將甜度,冰度,尺寸加入陣列(飲料編輯用)
        public static List<int> CheckEditbox(DrinkDetails EditD, string type)
        {
            var addItem = new List<int>();
            switch (type)
            {
                case "sweetCheck":
                    foreach (var item in EditD.Sweet)
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
                    break;
                case "sweetFalse":
                    foreach (var item in EditD.Sweet)
                    {
                        if (!item.Selected)
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
                    break;
                case "iceCheck":
                    foreach (var item in EditD.IceHot)
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
                    break;
                case "iceFalse":
                    foreach (var item in EditD.IceHot)
                    {
                        if (!item.Selected)
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
                    break;
                case "AdditemCheck":
                    if (EditD.AddItem != null)
                    {
                        foreach (var item in EditD.AddItem)
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
                    }
                    break;
                case "AdditemFalse":
                    foreach (var item in EditD.AddItem)
                    {
                        if (!item.Selected)
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
                    break;
                default:
                    break;
            }

            return addItem;
        }

        //取出總菜單配料檢視部份視圖
        public static List<AddItemCreate> PickItemDetailPartial(int MenuID)
        {
            NewDrinkDB db = new NewDrinkDB();
            var result = db.AddItemTypePrices.Where(m => m.MenuID == MenuID);

            var AddView = new List<AddItemCreate>();
            int Num = 0;

            foreach (var item in result)
            {
                //避免重覆取出
                if (item.ItemID == Num)
                {
                    continue;
                }

                Num = item.ItemID;
                //取出配料價表 0大, 1中, 2小
                //取出配料名                
                var query = db.AddItemTypes.Find(item.ItemID);
                var AddPL = db.AddItemTypePrices.Where(m => m.MenuID == MenuID && m.ItemID == query.ID && m.SizeNumber == 0);
                var AddPM = db.AddItemTypePrices.Where(m => m.MenuID == MenuID && m.ItemID == query.ID && m.SizeNumber == 1);
                var AddPS = db.AddItemTypePrices.Where(m => m.MenuID == MenuID && m.ItemID == query.ID && m.SizeNumber == 2);

                foreach (var PriceL in AddPL)
                {
                    foreach (var PriceM in AddPM)
                    {
                        foreach (var PriceS in AddPS)
                        {
                            AddView.Add(new AddItemCreate()
                            {
                                ID = item.ItemID,
                                ItemName = query.ItemName,
                                LPrice = PriceL.ItemPrice,
                                MPrice = PriceM.ItemPrice,
                                SPrice = PriceS.ItemPrice
                            });
                        }//End PriceS Froeach.
                    }//End PriceM Froeach.
                }//End PriceL Froeach.
            }

            db.Dispose();
            return AddView;
        }

        //以MenuID取出甜度名稱
        public static List<SweetType> PickMenuSweet_UseMenuID(int MenuID)
        {
            NewDrinkDB db = new NewDrinkDB();
            var result = db.MenuSweets.Where(m => m.MenuID == MenuID);
            var SweetT = new List<SweetType>();

            foreach (var item in result)
            {
                var query = db.SweetTypes.Find(item.SweetID);
                SweetT.Add(new SweetType()
                {
                    SweetName = query.SweetName
                });
            }

            db.Dispose();
            return SweetT;
        }

        //以MenuID取出冰度名稱
        public static List<IceType> PickMenuIce_UseMenuID(int MenuID)
        {
            NewDrinkDB db = new NewDrinkDB();
            var result = db.MenuIces.Where(m => m.MenuID == MenuID);
            var IceT = new List<IceType>();

            foreach (var item in result)
            {
                var query = db.IceTypes.Find(item.IceID);
                IceT.Add(new IceType()
                {
                    IceName = query.IceName
                });
            }

            db.Dispose();
            return IceT;
        }

        //以MenuID取出飲料大小名稱
        public static List<SizeType> PickMenuSize_UseMenuID(int MenuID)
        {
            NewDrinkDB db = new NewDrinkDB();
            var result = db.MenuSizes.Where(m => m.MenuID == MenuID);
            var SizeT = new List<SizeType>();

            foreach (var item in result)
            {
                var query = db.SizeTypes.Find(item.SizeID);
                SizeT.Add(new SizeType()
                {
                    SizeName = query.SizeName
                });
            }

            db.Dispose();
            return SizeT;
        }

        //以MenuID取出編輯飲料配料Model
        public static AddItemCreateEdit PickAddItemCreateEdit_UseMenuID(int MenuID)
        {
            NewDrinkDB db = new NewDrinkDB();
            Menu menu = db.Menus.Find(MenuID);
            var result2 = new List<AddCreateEditOther>();
            var pickItem = PickItemDetailPartial(MenuID);

            foreach (var item in pickItem)
            {
                result2.Add(new AddCreateEditOther()
                {
                    ItemID = item.ID,
                    ItemName = item.ItemName,
                    LPrice = item.LPrice,
                    MPrice = item.MPrice,
                    SPrice = item.SPrice,
                    deleteOrNot = false
                });
            }

            db.Dispose();
            return new AddItemCreateEdit()
            {
                MenuID = MenuID,
                MenuName = menu.MenuName,
                AddCreateEditOther = result2
            };
        }

        //判斷加料金額不為負值
        public static int MyMenuAdditemEdit_PriceNoSmall(IList<AddCreateEditOther> Nitem)
        {
            int Price = 0;

            //判斷資料價錢是否為負
            foreach (var item in Nitem)
            {
                if (item.LPrice < 0 || item.MPrice < 0 || item.SPrice < 0)
                {
                    Price = -1;
                    break;
                }
            }

            return Price;
        }

        //判斷舊配料資料是否刪除(true刪除, false不刪除)
        public static List<int> MyMenuAdditemEdit_DeleOrUpdate(IList<AddCreateEditOther> Nitem, string type)
        {
            var addItem = new List<int>();
            switch (type)
            {
                case "DeleteItem":
                    foreach (var item in Nitem)
                    {
                        if (item.deleteOrNot)
                        {
                            addItem.Add(item.ItemID);
                        }
                    }
                    break;
                case "UpdateItem":
                    foreach (var item in Nitem)
                    {
                        if (!item.deleteOrNot)
                        {
                            addItem.Add(item.ItemID);
                        }
                    }
                    break;
                default:
                    break;
            }
            return addItem;
        }

        //取得編輯店家甜度.冰度.尺寸的視圖
        public static MenuSISEdit GetFakeMenuSISEdit_View(int MenuID)
        {
            //取甜度
            var Sweet = MenuSIS_GetSweet(MenuID);
            //取冰度
            var Ice = MenuSIS_GetIce(MenuID);
            //取尺寸
            var Size = MenuSIS_GetSize(MenuID);

            return new MenuSISEdit()
            {
                MenuID = MenuID,
                Sweet = Sweet,
                IceHot = Ice,
                Size = Size
            };
        }

        //取得錯誤而返回的視圖
        public static MenuSISEdit ErrorBackMenu_MenuSIS(MenuSISEdit Nsis)
        {
            Nsis.Sweet = MenuSIS_GetSweet(Nsis.MenuID);
            Nsis.IceHot = MenuSIS_GetIce(Nsis.MenuID);
            Nsis.Size = MenuSIS_GetSize(Nsis.MenuID);

            return Nsis;
        }

        //先拿到全部甜度,再判斷哪些已經勾過了(編輯店家甜度.冰度.尺寸的視圖用)
        public static IList<SelectListItem> MenuSIS_GetSweet(int MenuID)
        {
            NewDrinkDB db = new NewDrinkDB();
            //拿到全部甜度
            var AllSweet = db.SweetTypes.ToList();
            //拿到勾選的項目
            var MenuSweet = db.MenuSweets.Where(m => m.MenuID == MenuID);

            var result = new List<SelectListItem>();
            foreach (var item in AllSweet)
            {
                bool AreCheck = false;
                foreach (var itemSweet in MenuSweet)
                {
                    if (itemSweet.SweetID == item.ID)
                    {
                        AreCheck = true;
                    }
                }

                if (AreCheck)
                {
                    //如果甜度已勾選擇啟用勾選時在Menusweets的名字
                    var NameS = db.MenuSweets.Where(m => m.MenuID == MenuID && m.SweetID == item.ID);
                    foreach (var NameSitem in NameS)
                    {
                        item.SweetName = NameSitem.SweetName;
                    }
                    result.Add(
                        new SelectListItem { Text = item.SweetName, Value = item.ID.ToString(), Selected = true }
                    );
                }
                else
                {
                    result.Add(
                        new SelectListItem { Text = item.SweetName, Value = item.ID.ToString(), Selected = false }
                    );
                }
            }
            db.Dispose();
            return result;
        }

        //先拿到全部冰度,再判斷哪些已經勾過了(編輯店家甜度.冰度.尺寸的視圖用)
        public static IList<SelectListItem> MenuSIS_GetIce(int MenuID)
        {
            NewDrinkDB db = new NewDrinkDB();
            //拿到全部甜度
            var AllIce = db.IceTypes.ToList();
            //拿到勾選的項目
            var MenuIce = db.MenuIces.Where(m => m.MenuID == MenuID);

            var result = new List<SelectListItem>();
            foreach (var item in AllIce)
            {
                bool AreCheck = false;
                foreach (var itemIce in MenuIce)
                {
                    if (itemIce.IceID == item.ID)
                    {
                        AreCheck = true;
                    }
                }

                if (AreCheck)
                {
                    //如果冰度已勾選擇啟用勾選時在Menuices的名字
                    var NameI = db.MenuIces.Where(m => m.MenuID == MenuID && m.IceID == item.ID);
                    foreach (var NameIitem in NameI)
                    {
                        item.IceName = NameIitem.IceName;
                    }
                    result.Add(
                        new SelectListItem { Text = item.IceName, Value = item.ID.ToString(), Selected = true }
                    );
                }
                else
                {
                    result.Add(
                        new SelectListItem { Text = item.IceName, Value = item.ID.ToString(), Selected = false }
                    );
                }
            }
            db.Dispose();
            return result;
        }

        //先拿到全部尺寸,再判斷哪些已經勾過了(編輯店家甜度.冰度.尺寸的視圖用)
        public static IList<SelectListItem> MenuSIS_GetSize(int MenuID)
        {
            NewDrinkDB db = new NewDrinkDB();
            //拿到全部甜度
            var AllSize = db.SizeTypes.ToList();
            //拿到勾選的項目
            var MenuSize = db.MenuSizes.Where(m => m.MenuID == MenuID);

            var result = new List<SelectListItem>();
            foreach (var item in AllSize)
            {
                bool AreCheck = false;
                foreach (var itemSize in MenuSize)
                {
                    if (itemSize.SizeID == item.ID)
                    {
                        AreCheck = true;
                    }
                }

                if (AreCheck)
                {
                    //如果尺寸已勾選擇啟用勾選時在Menuices的名字
                    var NameI = db.MenuSizes.Where(m => m.MenuID == MenuID && m.SizeID == item.ID);
                    foreach (var NameSitem in NameI)
                    {
                        item.SizeName = NameSitem.SizeName;
                    }
                    result.Add(
                        new SelectListItem { Text = item.SizeName, Value = item.ID.ToString(), Selected = true }
                    );
                }
                else
                {
                    result.Add(
                        new SelectListItem { Text = item.SizeName, Value = item.ID.ToString(), Selected = false }
                    );
                }
            }
            db.Dispose();
            return result;
        }

        //將甜度,冰度,尺寸加入陣列(編輯店家甜度.冰度.尺寸的視圖用,含勾選及不勾選)
        public static List<int> CheckCheckboxNotEmpty_MenuSIS(MenuSISEdit Nsis, string type)
        {
            var addItem = new List<int>();
            switch (type)
            {
                case "sweet":
                    foreach (var item in Nsis.Sweet)
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
                    break;
                case "sweetNo":
                    foreach (var item in Nsis.Sweet)
                    {
                        if (!item.Selected)
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
                    break;
                case "ice":
                    foreach (var item in Nsis.IceHot)
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
                    break;
                case "iceNo":
                    foreach (var item in Nsis.IceHot)
                    {
                        if (!item.Selected)
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
                    break;
                case "size":
                    foreach (var item in Nsis.Size)
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
                    break;
                case "sizeNo":
                    foreach (var item in Nsis.Size)
                    {
                        if (!item.Selected)
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
                    break;
                default:
                    break;
            }

            return addItem;
        }

        //將甜度加入陣列(編輯店家甜度的視圖用,含勾選及不勾選)
        public static List<int> CheckCheckboxNotEmpty_MenuSISsweet(MenuAdd_NewSweet Nsis, string type)
        {
            var addItem = new List<int>();
            switch (type)
            {
                case "sweet":
                    foreach (var item in Nsis.Sweet)
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
                    break;
                case "sweetNo":
                    foreach (var item in Nsis.Sweet)
                    {
                        if (!item.Selected)
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
                    break;              
                default:
                    break;
            }

            return addItem;
        }

        //將甜度,冰度,尺寸加入陣列(編輯店家甜度.冰度.尺寸的視圖用,含勾選及不勾選)
        public static List<int> CheckCheckboxNotEmpty_MenuSIIce(MenuAdd_NewIce Nsis, string type)
        {
            var addItem = new List<int>();
            switch (type)
            {                
                case "ice":
                    foreach (var item in Nsis.IceHot)
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
                    break;
                case "iceNo":
                    foreach (var item in Nsis.IceHot)
                    {
                        if (!item.Selected)
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
                    break;               
                default:
                    break;
            }

            return addItem;
        }

        //將尺寸加入陣列(編輯店家尺寸的視圖用,含勾選及不勾選)
        public static List<int> CheckCheckboxNotEmpty_MenuSISize(MenuAdd_NewSize Nsis, string type)
        {
            var addItem = new List<int>();
            switch (type)
            {              
                case "size":
                    foreach (var item in Nsis.Size)
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
                    break;
                case "sizeNo":
                    foreach (var item in Nsis.Size)
                    {
                        if (!item.Selected)
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
                    break;
                default:
                    break;
            }

            return addItem;
        }


    }
}