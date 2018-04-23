using NewDrink2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NewDrink2.Controllers
{
    public class TypeChangeManagerController : Controller
    {
        private NewDrinkDB db = new NewDrinkDB();

        // GET: TypeChangeManager
        public ActionResult Index()
        {
            return View();
        }
        //GET: Manager/_Add2_NewDrinks
        public ActionResult _Add2_NewDrinks(int id)
        {
            Menu query = db.Menus.Find(id);
            if (query == null)
            {
                return HttpNotFound();
            }

            //先依照菜單設定的資料取出視圖
            //用菜單ID取出菜單名及菜單ViewModel
            Add2_Drinks_detail resultDetail = Models.Add2_NewMenuAbout.Add2_NewDrinkDetails(id);
            Add2_DrinksView result = new Add2_DrinksView()
            {
                MenuID = id,
                MenuName = query.MenuName,
                DrinkType = "",
                Add2_Drinks_details = new List<Add2_Drinks_detail>() {
                    resultDetail
                }
            };

            return View(result);
        }

        //GET: Manager/_Add2_NewDrinksEntry
        public ActionResult _Add2_NewDrinksEntry(int id)
        {
            var result = Models.Add2_NewMenuAbout.Add2_NewDrinkDetails(id);
            return PartialView("_DrinkDetailsEntry2", result);
        }

        //GET: Manager/_DrinkDetailsEntry
        public ActionResult _DrinkDetailsEntryRow2(int id)
        {
            var result = Models.ManagerModels.PartialAddDrinkDetails(id);
            var result2 = Models.Add2_NewMenuAbout.Add2_NewDrinkDetails(id);
            return PartialView("_DrinkDetailsEntry2", result2);
        }

        //GET: Manager/_Add2ButtonMenu
        public ActionResult _Add2ButtonMenu(int MenuID, List<AddItemCreate> AddItemcreats, string PriceSmall, string Additem)
        {
            //要先取出菜單的設定
            var result = Models.Add2_NewMenuAbout.Add2_MEnuSisEntryR(MenuID);
            if (AddItemcreats != null)
            {
                foreach (var item in AddItemcreats)
                {
                    result.AddItemCreate.Add(new AddItemCreate()
                    {
                        ItemName = item.ItemName,
                        LPrice = item.LPrice,
                        MPrice = item.MPrice,
                        SPrice = item.SPrice
                    });
                }
            }

            if (!String.IsNullOrEmpty(PriceSmall))
            {
                ViewBag.PriceSmall = PriceSmall;
            }
            if (!String.IsNullOrEmpty(Additem))
            {
                ViewBag.Additem = Additem;
            }

            return PartialView("_Add2ButtonMenu", result);
        }

        //POST: Manager/_Add2ButtonMenu
        [HttpPost]
        public ActionResult _Add2ButtonMenu2(Add2_MEnuSis Type)
        {
            var result = Models.Add2_NewMenuAbout.Add2_MEnuSisEntryR(22);            
            return PartialView("_Add2ButtonMenu", result);
        }

        //POST: Manager/_Add2_NewDrinks
        [HttpPost]
        public ActionResult _Add2_NewDrinks(Add2_DrinksView DrinkD, Add2_Drinks_Size Add2_Drinks_Sizes, Add2_MEnuSis AddType, string AddSend,string SweetSend, string IceSend, string SizeSend, string saveAdd)
        {
            //先判斷按的按鈕

            //按下加料按鈕
            if (AddSend != null)
            {
                //判斷價錢不得為負
                int Price = 0;
                if (AddType.AddItemCreate != null)
                {
                    Price = Models.Add2_NewMenuAbout.MyMenuAdditemEdit_PriceNoSmall2(AddType.AddItemCreate);
                }
                //判斷輸入的值不得為空
                string Addmodelstate = "No";
                if (AddType.AddItemCreate != null)
                {
                    foreach (var item in AddType.AddItemCreate)
                    {
                        if (String.IsNullOrEmpty(item.ItemName))
                        {
                            Addmodelstate = null;
                        }
                    }
                }                

                if (String.IsNullOrEmpty(Addmodelstate) || Price < 0)
                {
                    if (Price < 0)
                    {
                        ViewBag.PriceSmall = "加料價錢不得為負數.";                       
                    }

                    if (String.IsNullOrEmpty(Addmodelstate))
                    {
                        ViewBag.Additem = "加料名稱不得為空.";
                    }

                    ViewBag.AddItemcreats = AddType.AddItemCreate;
                    var result = Models.Add2_NewMenuAbout.BackDetails(DrinkD.MenuID, DrinkD.Add2_Drinks_details);
                    //回傳使用者剛剛輸入的資料
                    DrinkD.Add2_Drinks_details = result;
                    return View(DrinkD);
                }
                //同菜單不能建立重覆的種類價表
                if (AddType.AddItemCreate != null)
                {
                    //判斷是否有重複的種類
                    foreach (var item in AddType.AddItemCreate)
                    {
                        if (item.ID == 0)
                        {
                            var query = db.AddItemTypes.Where(m => m.ItemName == item.ItemName);

                            if (query.FirstOrDefault() != null)
                            {
                                foreach (var queryItem in query)
                                {
                                    var query2 = db.AddItemTypePrices.Where(m => m.MenuID == DrinkD.MenuID && m.ItemID == queryItem.ID);
                                    if (query2.FirstOrDefault() != null)
                                    {
                                        ViewBag.PriceSmall = "您建立了重覆的種類,請檢查後再做新增.";
                                        ViewBag.AddItemcreats = AddType.AddItemCreate;
                                        DrinkD.Add2_Drinks_details = Models.Add2_NewMenuAbout.BackDetails(DrinkD.MenuID, DrinkD.Add2_Drinks_details);
                                        return View(DrinkD);
                                    }
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }//End if

                //存入資料(寫入菜單)
                Models.Add2_NewMenuAbout.EditAdditem(DrinkD.MenuID, AddType);
                //返回視圖
                DrinkD.Add2_Drinks_details = Models.Add2_NewMenuAbout.BackDetails(DrinkD.MenuID, DrinkD.Add2_Drinks_details);
                return View(DrinkD);
            }

            //按下甜度按鈕
            if (SweetSend != null)
            {
                //判斷
                //判斷甜度勾選及動態欄其一必填
                //判斷甜度.兩個都沒有選的情況
                var SweetCheck = Models.Add2_NewMenuAbout.CheckCheckboxNotEmpty2(AddType, "sweet");
                if ((AddType.SweetName == null || AddType.SweetName.Count == 0) && (SweetCheck == null || SweetCheck.Count == 0))
                {
                    ViewBag.SweetEmpty = "甜度欄位尚未填寫,請填寫完畢再儲存.";
                    //var Menu = Models.ManagerModels.ErrorBackMenu(newM);
                    var result = Models.Add2_NewMenuAbout.BackDetails(DrinkD.MenuID, DrinkD.Add2_Drinks_details);
                    //回傳使用者剛剛輸入的資料
                    DrinkD.Add2_Drinks_details = result;
                    return View(DrinkD);
                }


                //存入資料(寫入菜單)
                Models.Add2_NewMenuAbout.EditSweet(DrinkD.MenuID, AddType);
                //返回視圖
                DrinkD.Add2_Drinks_details = Models.Add2_NewMenuAbout.BackDetails(DrinkD.MenuID, DrinkD.Add2_Drinks_details);
                return View(DrinkD);
            }
            //按下冰度按鈕
            if (IceSend != null)
            {
                //判斷冰度.兩個都沒有選的情況
                var IceCheck = Models.Add2_NewMenuAbout.CheckCheckboxNotEmpty2(AddType, "ice");
                if ((AddType.IceName == null || AddType.IceName.Count == 0) && (IceCheck == null || IceCheck.Count == 0))
                {
                    ViewBag.IceEmpty = "冰度欄位尚未填寫,請填寫完畢再儲存.";
                    var result = Models.Add2_NewMenuAbout.BackDetails(DrinkD.MenuID, DrinkD.Add2_Drinks_details);
                    //回傳使用者剛剛輸入的資料
                    DrinkD.Add2_Drinks_details = result;
                    return View(DrinkD);
                }
                //存入資料(寫入菜單)
                Models.Add2_NewMenuAbout.EditIce(DrinkD.MenuID, AddType);
                //返回視圖
                DrinkD.Add2_Drinks_details = Models.Add2_NewMenuAbout.BackDetails(DrinkD.MenuID, DrinkD.Add2_Drinks_details);
                return View(DrinkD);
            }
            //按下尺寸按鈕
            if (SizeSend != null)
            {
                //判斷尺寸.兩個都沒有選的情況
                var SizeCheck = Models.Add2_NewMenuAbout.CheckCheckboxNotEmpty2(AddType, "size");
                if ((AddType.SizeName == null || AddType.SizeName.Count == 0) && (SizeCheck == null || SizeCheck.Count == 0))
                {
                    ViewBag.SizeEmpty = "尺寸欄位尚未填寫,請填寫完畢再儲存.";
                    var result = Models.Add2_NewMenuAbout.BackDetails(DrinkD.MenuID, DrinkD.Add2_Drinks_details);
                    //回傳使用者剛剛輸入的資料
                    DrinkD.Add2_Drinks_details = result;
                    return View(DrinkD);
                }

                //存入資料(寫入菜單)
                Models.Add2_NewMenuAbout.EditSize(DrinkD.MenuID, AddType);
                //返回視圖
                DrinkD.Add2_Drinks_details = Models.Add2_NewMenuAbout.BackDetails(DrinkD.MenuID, DrinkD.Add2_Drinks_details);
                return View(DrinkD);
            }
            //按下送出按鈕
            if (saveAdd != null)
            {
                //判斷新增飲料欄位不得刪除全部,導致此欄位null
                if (DrinkD.Add2_Drinks_details == null || DrinkD.Add2_Drinks_details.Count == 0)
                {
                    ViewBag.EmptyError = "您沒有新增任何飲品.";
                    //返回視圖
                    //DrinkD.Add2_Drinks_details = Models.Add2_NewMenuAbout.BackDetails(DrinkD.MenuID, DrinkD.Add2_Drinks_details);
                    return View(DrinkD);
                }
                //飲品金額則以不為負數為基準
                int Price = Models.Add2_NewMenuAbout.DrinkPriceIsNotSmall(DrinkD);
                if (!ModelState.IsValid || Price < 0)
                {
                    if (Price < 0)
                    {
                        ViewBag.PriceSmall = "價錢不得為負值.";
                    }
                    //返回視圖
                    DrinkD.Add2_Drinks_details = Models.Add2_NewMenuAbout.BackDetails(DrinkD.MenuID, DrinkD.Add2_Drinks_details);
                    return View(DrinkD);
                }

                //判斷勾選甜度.冰度.欄位必須勾選一項(由於是多項,所以必須一樣樣判斷)
                //加料由於有可加與不可加所以就不判斷
                foreach (var item in DrinkD.Add2_Drinks_details)
                {
                    var CheckSweet = Models.Add2_NewMenuAbout.AddDrink_CheckboxNotEmpty2(item, "sweet");
                    var CheckIce = Models.Add2_NewMenuAbout.AddDrink_CheckboxNotEmpty2(item, "ice");
                    if ((CheckSweet == null || CheckSweet.Count == 0) || (CheckIce == null || CheckIce.Count == 0))
                    {
                        ViewBag.EmptyError = "請將欄位填寫完全,甜度及冰度欄位請至少勾選一項.";
                        //返回視圖
                        DrinkD.Add2_Drinks_details = Models.Add2_NewMenuAbout.BackDetails(DrinkD.MenuID, DrinkD.Add2_Drinks_details);
                        return View(DrinkD);
                    }
                }
                //判斷完後分別寫入四個表////////////////////////////////////////
                //飲料名及種類表(要先寫入資料庫才能取DrinkID)
                //飲料尺寸價表(取得DrinkID寫入資料庫,再取得SizePID)
                //
                //--依照DrinkID,再用DrinkID.SizeID.Price取SizePID------
                //這裡先不判斷使用者若重覆建立飲料名.種類.大小.價錢一樣,但冷冰度不一樣的資料(使用者不使用修改[這樣會造成有兩筆一樣但ID不一樣的資料])
                //飲料甜度表
                //飲料冰度表            
                //飲料加料表(要先判斷是否null,若不為null則寫入)
                ////////////////////////////////////////////////////////////////

                //飲料名及種類表(要先判斷同一菜單是否已有同名.同種類飲料)       
                foreach (var item in DrinkD.Add2_Drinks_details)
                {
                    DrinkD.DrinkType = DrinkD.DrinkType.Trim();
                    var query = db.MenuDrinks.Where(m => m.MenuID == DrinkD.MenuID && m.DrinkType == DrinkD.DrinkType && m.DrinkName == item.DrinkName);
                    if (query.FirstOrDefault() != null)
                    {
                        continue;
                    }
                    else
                    {
                        var NewDrinks = new MenuDrink()
                        {
                            MenuID = DrinkD.MenuID,
                            DrinkType = DrinkD.DrinkType,
                            DrinkName = item.DrinkName
                        };
                        //NewDrinks.Add(new MenuDrink()
                        //{
                        //    MenuID = addDks.MenuID,
                        //    DrinkType = item.DrinkType,
                        //    DrinkName = item.DrinkName
                        //});
                        db.MenuDrinks.Add(NewDrinks);
                        db.SaveChanges();
                    }
                }
                //依照MenuID.DrinkType.DrinkName取出飲料ID/////////////////////////////////
                var NewSizeP = new List<SizeTable>();
                foreach (var item in DrinkD.Add2_Drinks_details)
                {
                    int DrinkID = Models.Add2_NewMenuAbout.PickDrinkID_UseModel2(item, DrinkD.MenuID, DrinkD.DrinkType);
                    //飲料尺寸價表
                    int SizeID = Int32.Parse(item.SizeTypeM);
                    //由於加入了排序的要素所以要先判斷所寫入的這個項目是第幾個
                    //一個一個分別寫入
                    var CheckHave = db.SizeTables.Where(m => m.MenuID == DrinkD.MenuID);
                    //若此菜單找不到飲料則起始值為1
                    if (CheckHave.FirstOrDefault() == null)
                    {
                        var NSizeTable = new SizeTable()
                        {
                            DrinkID = DrinkID,
                            SizeID = SizeID,
                            Price = item.DrinkPrice,
                            MenuID = DrinkD.MenuID,
                            DrinkSort = 1
                        };
                        db.SizeTables.Add(NSizeTable);
                        db.SaveChanges();
                    }
                    //若有找到飲料則以最後一個值的sort加1
                    if (CheckHave.FirstOrDefault() != null)
                    {
                        //先找到最後一個飲料的sort值
                        int sortNum = 0;
                        CheckHave = CheckHave.OrderByDescending(m => m.DrinkSort);
                        foreach (var CHitem in CheckHave)
                        {
                            sortNum = CHitem.DrinkSort;
                            break;
                        }
                        sortNum = sortNum + 1;
                        //要先判斷第一個要寫入的值有沒有先寫入了(當菜單為空白時要判斷)
                        var HaveSet = db.SizeTables.Where(m => m.DrinkID == DrinkID && m.SizeID == m.SizeID && m.Price == item.DrinkPrice && m.MenuID == DrinkD.MenuID && sortNum == 1);
                        if (HaveSet.FirstOrDefault() != null)
                        {
                            continue;
                        }
                        else
                        {
                            var NSizeTable = new SizeTable()
                            {
                                DrinkID = DrinkID,
                                SizeID = SizeID,
                                Price = item.DrinkPrice,
                                MenuID = DrinkD.MenuID,
                                DrinkSort = sortNum
                            };
                            db.SizeTables.Add(NSizeTable);
                            db.SaveChanges();
                        }
                        
                    }
                    //NewSizeP.Add(new SizeTable()
                    //{
                    //    DrinkID = DrinkID,
                    //    SizeID = SizeID,
                    //    Price = item.DrinkPrice,
                    //    MenuID = DrinkD.MenuID
                    //});
                }
                //db.SizeTables.AddRange(NewSizeP);
                //db.SaveChanges();

                //依照DrinkID,再用DrinkID.SizeID.Price取SizePID//////////////////////////
                var NewSweetTable = new List<SweetTable>();
                var NewIceTable = new List<IceTable>();
                var NewAddItemTable = new List<AddItemTable>();
                foreach (var item in DrinkD.Add2_Drinks_details)
                {
                    int DrinkID = Models.Add2_NewMenuAbout.PickDrinkID_UseModel2(item, DrinkD.MenuID, DrinkD.DrinkType);
                    int SizePID = Models.Add2_NewMenuAbout.PickSizePID_UseDidSidPrice2(item, DrinkID);
                    var CheckSweet = Models.Add2_NewMenuAbout.AddDrink_CheckboxNotEmpty2(item, "sweet");
                    var CheckIce = Models.Add2_NewMenuAbout.AddDrink_CheckboxNotEmpty2(item, "ice");
                    var CheckAddItem = Models.Add2_NewMenuAbout.AddDrink_CheckboxNotEmpty2(item, "additem");

                    //飲料甜度表
                    foreach (var sweetitem in CheckSweet)
                    {
                        NewSweetTable.Add(new SweetTable()
                        {
                            SizePID = SizePID,
                            SweetID = sweetitem
                        });
                    }
                    //飲料冰度表 
                    foreach (var iceitem in CheckIce)
                    {
                        NewIceTable.Add(new IceTable()
                        {
                            SizePID = SizePID,
                            IceID = iceitem
                        });
                    }
                    //飲料加料表(要先判斷是否null,若不為null則寫入)
                    if (NewAddItemTable != null || NewAddItemTable.Count != 0)
                    {
                        foreach (var additem in CheckAddItem)
                        {
                            NewAddItemTable.Add(new AddItemTable()
                            {
                                SizePID = SizePID,
                                ItemIDPriceID = additem
                            });
                        }
                    }
                    db.SweetTables.AddRange(NewSweetTable);
                    db.IceTables.AddRange(NewIceTable);
                    if (NewAddItemTable != null || NewAddItemTable.Count != 0)
                    {
                        db.AddItemTables.AddRange(NewAddItemTable);
                    }
                    db.SaveChanges();
                }

                //寫完依menuID回到飲料詳細介面
                return RedirectToAction("MyMenuDrinkDetail", "Manager", new { id = DrinkD.MenuID });
            }

            //寫完依menuID回到飲料詳細介面
            return RedirectToAction("MyMenuDrinkDetail", "Manager", new { id = DrinkD.MenuID });
        }





        //關閉連線
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}