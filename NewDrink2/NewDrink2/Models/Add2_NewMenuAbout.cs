using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NewDrink2.Models
{
    public class Add2_NewMenuAbout
    {
        //取出建立飲料細節的ViewModel
        public static Add2_Drinks_detail Add2_NewDrinkDetails(int ID)
        {
            NewDrinkDB db = new NewDrinkDB();
            Menu MenuName = db.Menus.Find(ID);

            //取出店家尺寸的ViewModel
            IList<Add2_Drinks_Size> mySize = Add2_NewMenuAbout.Add2mySize(ID);

            //取出店家尺寸的ViewModel (下拉選單)
            List<SelectListItem> mySizeD = ManagerModels.mySize(ID);

            //取出飲料甜度的ViewModel
            IList<SelectListItem> mySweet = ManagerModels.mySweet(ID);

            //取出飲料冰度的ViewModel
            IList<SelectListItem> myIce = ManagerModels.myIce(ID);

            //取出飲料配料的ViewModel
            IList<SelectListItem> myAddItem = ManagerModels.myAddItem(ID);

            db.Dispose();

            return new Add2_Drinks_detail() {
                DrinkName = "",
                Add2_Drinks_Sizes = mySize,
                SizeType = mySizeD,
                Sweet = mySweet,
                IceHot = myIce,
                AddItem = myAddItem,
                Bathus = "",
                DrinkPrice = 0,
                SizeTypeM = ""
            };

        }

        //取出飲料尺寸
        public static List<Add2_Drinks_Size> Add2mySize(int id)
        {
            NewDrinkDB db = new NewDrinkDB();

            //先從店家尺寸表取SizeID
            var query = db.MenuSizes.Where(m => m.MenuID == id);

            //再以Size到尺寸總表取Size名字
            List<Add2_Drinks_Size> mySize = new List<Add2_Drinks_Size>();
            foreach (var item in query)
            {
                var query2 = db.SizeTypes.Find(item.SizeID);
                mySize.Add(new Add2_Drinks_Size()
                {
                    SizeID = item.SizeID,
                    SizeName = query2.SizeName,
                    Price = 0
                });
            }

            db.Dispose();
            return mySize;
        }

        //取出編輯加料
        public static List<AddItemCreate> Add2myAddItem(int MenuID)
        {
            NewDrinkDB db = new NewDrinkDB();
            Menu menu = db.Menus.Find(MenuID);
            var result2 = new List<AddItemCreate>();
            var pickItem = Models.ManagerModels.PickItemDetailPartial(MenuID);

            foreach (var item in pickItem)
            {
                result2.Add(new AddItemCreate()
                {
                    ID = item.ID,
                    ItemName = item.ItemName,
                    LPrice = item.LPrice,
                    MPrice = item.MPrice,
                    SPrice = item.SPrice,
                });
            }

            db.Dispose();
            return result2;
        }

        //以MenuID取出編輯飲料配料Model
        public static AddItemCreateEdit PickAddItemCreateEdit_UseMenuID(int MenuID)
        {
            NewDrinkDB db = new NewDrinkDB();
            Menu menu = db.Menus.Find(MenuID);
            var result2 = new List<AddCreateEditOther>();
            var pickItem = Models.ManagerModels.PickItemDetailPartial(MenuID);

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

        


        //取出建立飲料動態細節的ViewModel(編輯菜單甜.冰.加料.尺寸)
        public static Add2_MEnuSis Add2_MEnuSisEntryR(int ID)
        {

            //取出店家尺寸的ViewModel
            var size = Models.ManagerModels.MenuSIS_GetSize(ID);

            //取出飲料甜度的ViewModel
            var sweet = Models.ManagerModels.MenuSIS_GetSweet(ID);

            //取出飲料冰度的ViewModel
            var Ice = Models.ManagerModels.MenuSIS_GetIce(ID);

            //取出飲料配料的ViewModel
            var myAddItem = Add2myAddItem(ID);



            return new Add2_MEnuSis()
            {
                MenuID = ID,
                Size = size,
                Sweet = sweet,
                IceHot = Ice,
                AddItemCreate = myAddItem
            };

        }


        //將甜度,冰度,尺寸加入陣列
        public static List<int> CheckCheckboxNotEmpty2(Add2_MEnuSis newM, string type)
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

        //取出使用者輸入的飲料設定
        //取出輸入的甜度
        public static IList<SelectListItem> TakeDrinksSweet( IList<SelectListItem> sweet, int MenuID )
        {
            IList<SelectListItem> mySweet = ManagerModels.mySweet(MenuID);
            foreach (var item in sweet)
            {
                foreach (var sweetitem in mySweet)
                {
                    if (item.Value == sweetitem.Value)
                    {
                        sweetitem.Selected = true;
                    }
                }
            }

            return mySweet;
        }

        //取出輸入的冰度
        public static IList<SelectListItem> TakeDrinksIce(IList<SelectListItem> ice, int MenuID)
        {
            IList<SelectListItem> myice = ManagerModels.myIce(MenuID);
            foreach (var item in ice)
            {
                foreach (var iceitem in myice)
                {
                    if (item.Value == iceitem.Value)
                    {
                        iceitem.Selected = true;
                    }
                }
            }

            return myice;
        }

        //取出輸入的配料
        public static IList<SelectListItem> TakeDrinksAddItem(IList<SelectListItem> AddItem, int MenuID)
        {
            IList<SelectListItem> myAddItem= ManagerModels.myAddItem(MenuID);
            if (AddItem != null)
            {
                foreach (var item in AddItem)
                {
                    foreach (var Additem in myAddItem)
                    {
                        if (item.Value == Additem.Value)
                        {
                            Additem.Selected = true;
                        }
                    }
                }
            }

            return myAddItem;
        }

        //取出輸入的尺寸
        public static IList<Add2_Drinks_Size> TakeDrinksSize2(IEnumerable<Add2_Drinks_Size> Size, int MenuID)
        {
            IList<Add2_Drinks_Size> mySize = Add2_NewMenuAbout.Add2mySize(MenuID);
            if (Size != null)
            {
                foreach (var item in Size)
                {
                    foreach (var sizeitem in mySize)
                    {
                        if (item.SizeID == sizeitem.SizeID)
                        {
                            sizeitem.Price = item.Price;
                        }
                    }
                }
            }

            return mySize;
        }



        //取出輸入的尺寸
        public static IList<Add2_Drinks_Size> TakeDrinksSize(IList<Add2_Drinks_Size> Size, int MenuID)
        {
            IList<Add2_Drinks_Size> mySize = Add2_NewMenuAbout.Add2mySize(MenuID);
            if (Size != null)
            {
                foreach (var item in Size)
                {
                    foreach (var sizeitem in mySize)
                    {
                        if (item.SizeID == sizeitem.SizeID)
                        {
                            sizeitem.Price = item.Price;
                        }
                    }
                }
            }

            return mySize;
        }

        //返回編輯視圖(添加飲料時使用)
        public static List<Add2_Drinks_detail> BackDetails( int MenuID,IList<Add2_Drinks_detail> Add2_Drinks_details)
        {
            var result = new List<Add2_Drinks_detail>() { };
            //回傳使用者剛剛輸入的資料
            foreach (var item in Add2_Drinks_details)
            {
                //取出使用者有勾選的甜度.冰度.加料.尺寸價錢
                var sw = Models.Add2_NewMenuAbout.TakeDrinksSweet(item.Sweet, MenuID);
                var ic = Models.Add2_NewMenuAbout.TakeDrinksIce(item.IceHot, MenuID);
                var ad = Models.Add2_NewMenuAbout.TakeDrinksAddItem(item.AddItem, MenuID);
                var siz = Models.Add2_NewMenuAbout.TakeDrinksSize2(item.Add2_Drinks_Sizes, MenuID);
                var siz2 = ManagerModels.mySize(MenuID);

                result.Add(new Add2_Drinks_detail()
                {
                    DrinkName = item.DrinkName,
                    Bathus = item.Bathus,
                    Sweet = sw,
                    IceHot = ic,
                    AddItem = ad,
                    Add2_Drinks_Sizes = siz,
                    SizeType = siz2,
                    DrinkPrice = 0,
                    SizeTypeM = ""
                });
            }
            return result;
        }


        //寫入店家甜度(在新增飲料編輯時使用)
        public static bool EditSweet(int MenuID, Add2_MEnuSis AddType)
        {
            NewDrinkDB db = new NewDrinkDB();

            var Nsis = new MenuSISEdit() {
                MenuID = MenuID,
                Sweet = AddType.Sweet,
                SweetName = AddType.SweetName
            };
            var SweetCheck = Models.ManagerModels.CheckCheckboxNotEmpty_MenuSIS(Nsis, "sweet");

            //判斷甜度.只填一個或都填的情況/////////////////////////////////////  
            //檢查未勾選的,從店家甜度表及飲料甜度表刪除
            var SweetFind = db.MenuSweets.Where(m => m.MenuID == Nsis.MenuID);
            var NoSweet = Models.ManagerModels.CheckCheckboxNotEmpty_MenuSIS(Nsis, "sweetNo");
            bool AreCheck = true;
            bool AreFalse = false;
            if (SweetCheck != null || SweetCheck.Count > 0)
            {
                //檢查勾選的,從店家甜度表新增                
                foreach (var itemsweet in SweetCheck)
                {
                    int newitem = 0;
                    //檢查新勾的有哪些
                    foreach (var item in SweetFind)
                    {
                        if (itemsweet == item.SweetID)
                        {
                            AreCheck = true;
                            break;
                        }
                        else
                        {
                            //比對找到新勾選的項目
                            AreCheck = false;
                            newitem = itemsweet;
                        }
                    }
                    //加入新勾選的項目
                    if (!AreCheck)
                    {
                        db.MenuSweets.Add(new MenuSweet()
                        {
                            MenuID = Nsis.MenuID,
                            SweetID = newitem
                        });
                    }
                }
            }
            foreach (var itemNo in NoSweet)
            {
                //檢查以前勾的有哪些
                foreach (var item in SweetFind)
                {
                    if (itemNo == item.SweetID)
                    {
                        AreFalse = true;
                        break;
                    }
                }
                //刪除不勾選的項目
                if (AreFalse)
                {
                    var deleteS = db.MenuSweets.Where(m => m.MenuID == Nsis.MenuID && m.SweetID == itemNo);
                    var deleteST = db.SizeTables.Where(m => m.MenuID == Nsis.MenuID);
                    foreach (var item in deleteST)
                    {
                        var deleteSweetTable = db.SweetTables.Where(m => m.SizePID == item.ID && m.SweetID == itemNo);
                        db.SweetTables.RemoveRange(deleteSweetTable);
                    }
                    db.MenuSweets.RemoveRange(deleteS);
                }
            }
            //新增甜度
            if (Nsis.SweetName != null)
            {
                //若新增,則新增甜度表種類   
                //判斷是否有重複的名字
                var NewSweetType = new List<SweetType>();
                foreach (var item in Nsis.SweetName)
                {
                    var NoRepeat = db.SweetTypes.Where(m => m.SweetName == item.SweetName);
                    if (NoRepeat.FirstOrDefault() == null && !String.IsNullOrEmpty(item.SweetName))
                    {
                        NewSweetType.Add(new SweetType()
                        {
                            SweetName = item.SweetName
                        });
                    }
                }
                if (NewSweetType != null && NewSweetType.Count != 0)
                {
                    db.SweetTypes.AddRange(NewSweetType);
                    db.SaveChanges();
                }
                //取出甜度ID,寫入店家甜度表
                var NewMenuSweet = new List<MenuSweet>();
                foreach (var item in Nsis.SweetName)
                {
                    if (!String.IsNullOrEmpty(item.SweetName))
                    {
                        int SweetID = Models.ManagerModels.SweetID(item.SweetName, "sweet");
                        NewMenuSweet.Add(new MenuSweet()
                        {
                            MenuID = Nsis.MenuID,
                            SweetID = SweetID
                        });
                    }                   
                }
                if (NewMenuSweet != null && NewMenuSweet.Count != 0)
                {
                    db.MenuSweets.AddRange(NewMenuSweet);
                }               
            }

            db.SaveChanges();
            db.Dispose();
            return true;

        }

        //寫入店家冰度(在新增飲料編輯時使用)
        public static bool EditIce(int MenuID, Add2_MEnuSis AddType)
        {
            NewDrinkDB db = new NewDrinkDB();

            var Nsis = new MenuSISEdit()
            {
                MenuID = MenuID,
                IceHot = AddType.IceHot,
                IceName = AddType.IceName
            };
            var IceCheck = Models.ManagerModels.CheckCheckboxNotEmpty_MenuSIS(Nsis, "ice");
            //檢查未勾選的,從店家冰度表及飲料冰度表刪除////////////////////////////////////////
            var IceFind = db.MenuIces.Where(m => m.MenuID == Nsis.MenuID);
            var NoIce = Models.ManagerModels.CheckCheckboxNotEmpty_MenuSIS(Nsis, "iceNo");
            bool AreCheck = true;
            bool AreFalse = false;
            if (IceCheck != null || IceCheck.Count > 0)
            {
                //檢查勾選的,從店家冰度表新增                
                foreach (var itemice in IceCheck)
                {
                    int newitem = 0;
                    //檢查新勾的有哪些
                    foreach (var item in IceFind)
                    {
                        if (itemice == item.IceID)
                        {
                            AreCheck = true;
                            break;
                        }
                        else
                        {
                            //比對找到新勾選的項目
                            AreCheck = false;
                            newitem = itemice;
                        }
                    }
                    //加入新勾選的項目
                    if (!AreCheck)
                    {
                        db.MenuIces.Add(new MenuIce()
                        {
                            MenuID = Nsis.MenuID,
                            IceID = newitem
                        });
                    }
                }
            }
            foreach (var itemNo in NoIce)
            {
                //檢查以前勾的有哪些
                foreach (var item in IceFind)
                {
                    if (itemNo == item.IceID)
                    {
                        AreFalse = true;
                        break;
                    }
                }
                //刪除不勾選的項目
                if (AreFalse)
                {
                    var deleteI = db.MenuIces.Where(m => m.MenuID == Nsis.MenuID && m.IceID == itemNo);
                    var deleteIT = db.SizeTables.Where(m => m.MenuID == Nsis.MenuID);
                    foreach (var item in deleteIT)
                    {
                        var deleteIceTable = db.IceTables.Where(m => m.SizePID == item.ID && m.IceID == itemNo);
                        db.IceTables.RemoveRange(deleteIceTable);
                    }
                    db.MenuIces.RemoveRange(deleteI);
                }
            }
            //新增冰度
            if (Nsis.IceName != null)
            {
                //若新增,則新增冰度表種類   
                //判斷是否有重複的名字
                var NewIceType = new List<IceType>();
                foreach (var item in Nsis.IceName)
                {
                    var NoRepeat = db.IceTypes.Where(m => m.IceName == item.IceName);
                    if (NoRepeat.FirstOrDefault() == null && !String.IsNullOrEmpty(item.IceName))
                    {
                        NewIceType.Add(new IceType()
                        {
                            IceName = item.IceName
                        });
                    }
                }
                if (NewIceType != null && NewIceType.Count != 0)
                {
                    db.IceTypes.AddRange(NewIceType);
                    db.SaveChanges();
                }
                //取出冰度ID,寫入店家冰度表
                var NewMenuIce = new List<MenuIce>();
                foreach (var item in Nsis.IceName)
                {
                    if (!String.IsNullOrEmpty(item.IceName))
                    {
                        int IceID = Models.ManagerModels.SweetID(item.IceName, "ice");
                        NewMenuIce.Add(new MenuIce()
                        {
                            MenuID = Nsis.MenuID,
                            IceID = IceID
                        });
                    }                   
                }
                if (NewMenuIce != null && NewMenuIce.Count != 0)
                {
                    db.MenuIces.AddRange(NewMenuIce);
                }               
            }

            db.SaveChanges();
            db.Dispose();
            return true;
        }

        //判斷加料金額不為負值
        public static int MyMenuAdditemEdit_PriceNoSmall2(IList<AddItemCreate> Nitem)
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

        //將加料資料判斷,並寫進資料庫
        public static bool EditAdditem(int MenuID, Add2_MEnuSis AddType)
        {
            NewDrinkDB db = new NewDrinkDB();

            var Dele = new List<int>();

            //先取出原有的所有配料
            var olditem = db.AddItemTypePrices.Where(m => m.MenuID == MenuID);
            if (olditem.FirstOrDefault() != null)
            {
                //如果AddType.AddItemCreate = null就將配料全刪除
                if (AddType.AddItemCreate == null)
                {
                    foreach (var item in olditem)
                    {
                        Dele.Add(item.ID);
                        //刪除價錢表
                        var DeAddTP = db.AddItemTypePrices.Where(m => m.ItemID == item.ItemID && m.MenuID == MenuID);
                        foreach (var item2 in DeAddTP)
                        {
                            //刪除飲料加料價表
                            var DeAddTable = db.AddItemTables.Where(m => m.ItemIDPriceID == item2.ID);
                            db.AddItemTables.RemoveRange(DeAddTable);
                        }
                        db.AddItemTypePrices.RemoveRange(DeAddTP);
                        db.SaveChanges();
                    }
                    db.Dispose();
                    return true;
                }


                foreach (var item in olditem)
                {
                    var T = true;
                    foreach (var itemAdd in AddType.AddItemCreate)
                    {
                        if (item.ItemID == itemAdd.ID)
                        {
                            T = true;
                            break;
                        }
                        else
                        {
                            T = false;
                        }
                    }
                    if (T == false)
                    {
                        Dele.Add(item.ID);
                        //刪除價錢表
                        var DeAddTP = db.AddItemTypePrices.Where(m => m.ItemID == item.ItemID && m.MenuID == MenuID);
                        foreach (var item2 in DeAddTP)
                        {
                            //刪除飲料加料價表
                            var DeAddTable = db.AddItemTables.Where(m => m.ItemIDPriceID == item2.ID);
                            db.AddItemTables.RemoveRange(DeAddTable);
                        }
                        db.AddItemTypePrices.RemoveRange(DeAddTP);
                        db.SaveChanges();
                    }
                }
            }


            //再用要更新的配料ID去找看看哪一筆不在了
            //編輯以itemID更新舊資料
            //判斷更新
            foreach (var item in AddType.AddItemCreate)
            {
                if (item.ID != 0)
                {
                    //更新配料名(種類表)
                    var UpAddT = db.AddItemTypes.Find(item.ID);
                    if (UpAddT != null)
                    {
                        UpAddT.ItemName = item.ItemName;
                    }
                    //更新價錢(價錢表)
                    var UpAddTP = db.AddItemTypePrices.Where(m => m.MenuID == MenuID && m.ItemID == item.ID);
                    if (UpAddTP != null)
                    {
                        var UpAddTPL = UpAddTP.Where(m => m.SizeNumber == 0);
                        foreach (var LSize in UpAddTPL)
                        {
                            LSize.ItemPrice = item.LPrice;
                        }

                        var UpAddTPM = UpAddTP.Where(m => m.SizeNumber == 1);
                        foreach (var MSize in UpAddTPM)
                        {
                            MSize.ItemPrice = item.MPrice;
                        }

                        var UpAddTPS = UpAddTP.Where(m => m.SizeNumber == 2);
                        foreach (var SSize in UpAddTPS)
                        {
                            SSize.ItemPrice = item.SPrice;
                        }
                    }
                    db.SaveChanges();

                }
            }





            //新增(2種狀況)
            //已有的種類(判斷不得重複) 
            //新的種類:新增加料種類表,寫入新的配料價表
            if (AddType.AddItemCreate != null)
            {
                foreach (var item in AddType.AddItemCreate)
                {
                    if (item.ID == 0)
                    {
                        //新增加料種類表
                        var query = db.AddItemTypes.Add(new AddItemType() { ItemName = item.ItemName });
                        db.SaveChanges();
                        int ItemID = 0;
                        var CheckID = db.AddItemTypes.Where(m => m.ItemName == item.ItemName);
                        foreach (var itemid in CheckID)
                        {
                            ItemID = itemid.ID;
                        }

                        //寫入新的配料價表
                        var AdditemPL = db.AddItemTypePrices.Add(new AddItemTypePrice() { ItemID = ItemID, SizeNumber = 0, ItemPrice = item.LPrice, MenuID = MenuID });
                        var AdditemPM = db.AddItemTypePrices.Add(new AddItemTypePrice() { ItemID = ItemID, SizeNumber = 1, ItemPrice = item.MPrice, MenuID = MenuID });
                        var AdditemPS = db.AddItemTypePrices.Add(new AddItemTypePrice() { ItemID = ItemID, SizeNumber = 2, ItemPrice = item.SPrice, MenuID = MenuID });

                        db.SaveChanges();
                    }
                }
            }

            db.Dispose();
            return true;
        }

        //寫入店家尺寸(在新增飲料編輯時使用)
        public static bool EditSize(int MenuID, Add2_MEnuSis AddType)
        {
            NewDrinkDB db = new NewDrinkDB();

            var Nsis = new MenuSISEdit()
            {
                MenuID = MenuID,
                Size = AddType.Size,
                SizeName = AddType.SizeName
            };
            var SizeCheck = Models.ManagerModels.CheckCheckboxNotEmpty_MenuSIS(Nsis, "size");
            //檢查未勾選的,從店家尺寸表及飲料尺寸價表刪除////////////////////////////////////////
            var SizeFind = db.MenuSizes.Where(m => m.MenuID == Nsis.MenuID);
            var NoSize = Models.ManagerModels.CheckCheckboxNotEmpty_MenuSIS(Nsis, "sizeNo");
            bool AreCheck = true;
            bool AreFalse = false;
            if (SizeCheck != null || SizeCheck.Count > 0)
            {
                //檢查勾選的,從店家尺寸表新增                
                foreach (var itemsize in SizeCheck)
                {
                    int newitem = 0;
                    //檢查新勾的有哪些
                    foreach (var item in SizeFind)
                    {
                        if (itemsize == item.SizeID)
                        {
                            AreCheck = true;
                            break;
                        }
                        else
                        {
                            //比對找到新勾選的項目
                            AreCheck = false;
                            newitem = itemsize;
                        }
                    }
                    //加入新勾選的項目
                    if (!AreCheck)
                    {
                        db.MenuSizes.Add(new MenuSize()
                        {
                            MenuID = Nsis.MenuID,
                            SizeID = newitem
                        });
                    }
                }
            }
            foreach (var itemNo in NoSize)
            {
                //檢查以前勾的有哪些
                foreach (var item in SizeFind)
                {
                    if (itemNo == item.SizeID)
                    {
                        AreFalse = true;
                        break;
                    }
                }
                //刪除不勾選的項目
                if (AreFalse)
                {
                    var deleteS = db.MenuSizes.Where(m => m.MenuID == Nsis.MenuID && m.SizeID == itemNo);
                    db.MenuSizes.RemoveRange(deleteS);

                    var deleteST = db.SizeTables.Where(m => m.MenuID == Nsis.MenuID && m.SizeID == itemNo);
                    db.SizeTables.RemoveRange(deleteST);
                }
            }
            //新增尺寸
            if (Nsis.SizeName != null)
            {
                var NewSizeType = new List<SizeType>();
                foreach (var item in Nsis.SizeName)
                {
                    var NoRepeat = db.SizeTypes.Where(m => m.SizeName == item.SizeName);
                    if (NoRepeat.FirstOrDefault() == null && !String.IsNullOrEmpty(item.SizeName))
                    {
                        NewSizeType.Add(new SizeType()
                        {
                            SizeName = item.SizeName
                        });
                    }
                }
                if (NewSizeType != null && NewSizeType.Count != 0)
                {
                    db.SizeTypes.AddRange(NewSizeType);
                    db.SaveChanges();
                }
                //取出尺寸ID,寫入店家尺寸表
                var NewMenuSize = new List<MenuSize>();
                foreach (var item in Nsis.SizeName)
                {
                    if (!String.IsNullOrEmpty(item.SizeName))
                    {
                        int SizeID = Models.ManagerModels.SweetID(item.SizeName, "size");
                        NewMenuSize.Add(new MenuSize()
                        {
                            MenuID = Nsis.MenuID,
                            SizeID = SizeID
                        });
                    }                   
                }
                if (NewMenuSize != null && NewMenuSize.Count != 0)
                {
                    db.MenuSizes.AddRange(NewMenuSize);
                }              
            }

            db.SaveChanges();
            db.Dispose();
            return true;
        }

        //判斷飲料金額不為負值
        public static int DrinkPriceIsNotSmall(Add2_DrinksView DrinkD)
        {
            int Price = 0;
            foreach (var item in DrinkD.Add2_Drinks_details)
            {
                //if (item.Add2_Drinks_Sizes != null)
                //{
                //    foreach (var itemP in item.Add2_Drinks_Sizes)
                //    {
                //        if (itemP.Price < 0)
                //        {
                //            Price = -1;
                //            break;
                //        }
                //        Price = 1;
                //    }
                //}
                if (item.DrinkPrice < 0)
                {
                    Price = -1;
                    break;
                }
                Price = 1;
            }

            return Price;
        }

        //將甜度,冰度,加料加入陣列(新增飲品用)
        public static List<int> AddDrink_CheckboxNotEmpty2(Add2_Drinks_detail details, string type)
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
        public static int PickDrinkID_UseModel2(Add2_Drinks_detail detail, int MenuID, string DrinkType)
        {
            NewDrinkDB db = new NewDrinkDB();
            int DrinkID = 0;
            var query = db.MenuDrinks.Where(m => m.MenuID == MenuID && m.DrinkType == DrinkType && m.DrinkName == detail.DrinkName).Select(m => new { ID = m.ID });
            foreach (var item in query)
            {
                DrinkID = item.ID;
            }

            db.Dispose();
            return DrinkID;
        }

        //依照DrinkID,再用DrinkID.SizeID.Price取SizePID
        public static int PickSizePID_UseDidSidPrice2(Add2_Drinks_detail detail, int DrinkID)
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






    }
}