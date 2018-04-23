﻿using NewDrink2.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

namespace NewDrink2.Controllers
{
    [Authorize]
    public class ManagerController : Controller
    {
        private NewDrinkDB db = new NewDrinkDB();
        private AesCryptoServiceProvider aes = new AesCryptoServiceProvider();

        /*********** 成員管理 ************/

        //GET: Manager/MyUser
        public ActionResult MyUser(int? page, string sortOrder, string SearchString, string currentFilter)
        {           
            string User = Helper.GetUserMail();

            //不顯示自己
            var qyery = db.Users.Where(m => m.Email != User);

            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            //搜尋使用者
            if (SearchString != null)
            {
                page = 1;
            }
            else
            {
                SearchString = currentFilter;
            }
            ViewBag.CurrentFilter = SearchString;

            if (!String.IsNullOrEmpty(SearchString))
            {
                qyery = qyery.Where(s => s.Name.Contains(SearchString));
            }

            //排序
            switch (sortOrder)
            {
                case "name_desc":
                    qyery = qyery.OrderByDescending(s => s.Name);
                    break;
                default:
                    qyery = qyery.OrderBy(s => s.ID);
                    break;
            }
            qyery = qyery.OrderBy(s => s.Name);
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(qyery.ToPagedList(pageNumber, pageSize));
        }

        // GET: Manager/AddUser
        public ActionResult AddUser()
        {
            return View();
        }

        //POST: Manager/AddUser
        [HttpPost]
        public ActionResult AddUser(User add)
        {
            if (!ModelState.IsValid)
            {
                return View(add);
            }

            //防止使用者重複註冊(同Email不得重複註冊)
            var queryLINQ = db.Users.Where(m => m.Email == add.Email);
            if (queryLINQ.FirstOrDefault() != null)
            {
                ViewBag.Error = "您使用的Email已註冊過!";
                return View(add);
            }

            //防止使用者名稱重複(揪團時易造成混淆)
            var queryLINQ2 = db.Users.Where(m => m.Name == add.Name);
            if (queryLINQ2.FirstOrDefault() != null)
            {
                ViewBag.Error = "您使用的姓名已重覆!";
                return View(add);
            }


            //加密密碼                    
            add.Password = PasswordUtility.AESEncryptor(add.Password, aes.Key, aes.IV);
            add.ConfirmPsd = PasswordUtility.AESEncryptor(add.ConfirmPsd, aes.Key, aes.IV);

            //設定使用者身分(2是成員)
            add.Identity = 2;
            //存入資料庫
            db.Users.Add(add);
            db.SaveChanges();

            //存入使用者身分及權限設定
            int UserID = Models.AccountModels.UserID(add.Email);
            int Identity = 2;
            int Identity2 = 0;
            var result = db.Database.ExecuteSqlCommand(@"INSERT INTO Identities(ID, Identity1, Identity2)VALUES(" + UserID + ", " + Identity + ", " + Identity2 + ")");
            var result2 = db.Database.ExecuteSqlCommand(@"INSERT INTO UserCanDoes(ID, BuyDrink, OrderSet, Message, Callnotice, ChangePsd, MyUserSet, MyMenuSet)VALUES(" + UserID + ", 1, 1, 1, 1, 1, 0, 0)");

            //返回使用者管理介面
            return RedirectToAction("MyUser", "Manager");
        }

        //GET: Manager/MyUserEdit
        public ActionResult MyUserEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User query = db.Users.Find(id);
            if (query == null)
            {
                return HttpNotFound();
            }

            return View(query);
        }

        //POST: Manager/MyUserEdit
        [HttpPost]
        public ActionResult MyUserEdit(int ID, User chang, UserCanDo cando)
        {
            if (String.IsNullOrEmpty(chang.Name))
            {
                ViewBag.NameError = "姓名是必要項.";
                return View(chang);
            }

            //之後要回來判斷如果都沒有勾選的話

            User query = db.Users.Find(ID);
            query.Name = chang.Name;
            if (chang.Identity == 1)
            {
                query.Identity = 1;
            }
            else if (chang.Identity == 2)
            {
                query.Identity = 2;
            }

            Identity query2 = db.Identities.Find(ID);
            query2.Identity1 = 2;

            if (chang.Identity != 1)
            {
                chang.Identity = 0;
            }

            query2.Identity2 = chang.Identity;

            UserCanDo query3 = db.UserCanDoes.Find(ID);
            query3.BuyDrink = cando.BuyDrink;
            query3.Callnotice = cando.Callnotice;
            query3.ChangePsd = cando.ChangePsd;
            query3.Message = cando.Message;
            query3.OrderSet = cando.OrderSet;
            query3.MyMenuSet = cando.MyMenuSet;
            query3.MyUserSet = cando.MyUserSet;

            db.SaveChanges();

            return RedirectToAction("MyUserDetails", "Manager", new { id = ID });
        }

        //GET: Manager/_ChangeIdentityPartial
        public ActionResult _ChangeIdentityPartial(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Identity query = db.Identities.Find(ID);
            if (query == null)
            {
                return HttpNotFound();
            }
           
            return PartialView("_ChangeIdentityPartial", query);
        }

        //GET: Manager/_ChangeCanDoPartial
        public ActionResult _ChangeCanDoPartial(int? ID, string Can)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserCanDo query = db.UserCanDoes.Find(ID);
            if (query == null)
            {
                return HttpNotFound();
            }

            ViewBag.Identity = Can;
            return PartialView("_ChangeCanDoPartial", query);
        }

        //GET: Manager/MyUserDetails
        public ActionResult MyUserDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User query = db.Users.Find(id);
            if (query == null)
            {
                return HttpNotFound();
            }

            return View(query);
        }

        //GET: Manager/_DetailsIdentityPartial
        public ActionResult _DetailsIdentityPartial(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Identity query = db.Identities.Find(ID);
            if (query == null)
            {
                return HttpNotFound();
            }

            return PartialView("_DetailsIdentityPartial", query);
        }

        //GET: Manager/_DetailsCanDoPartial
        public ActionResult _DetailsCanDoPartial(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserCanDo query = db.UserCanDoes.Find(ID);
            if (query == null)
            {
                return HttpNotFound();
            }

            return PartialView("_DetailsCanDoPartial", query);
        }

        //GET: Manager/MyUserDelete
        public ActionResult MyUserDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User query = db.Users.Find(id);
            if (query == null)
            {
                return HttpNotFound();
            }

            return View(query);
        }

        //POST: Manager/MyUserDelete
        [HttpPost]
        public ActionResult MyUserDelete(int id)
        {
            User query = db.Users.Find(id);
            if (query == null)
            {
                return HttpNotFound();
            }
            db.Users.Remove(query);

            Identity query2 = db.Identities.Find(id);
            if (query2 != null)
            {
                db.Identities.Remove(query2);
            }

            UserCanDo query3 = db.UserCanDoes.Find(id);
            if (query3 != null)
            {
                db.UserCanDoes.Remove(query3);
            }

            //刪除使用者要連使用者的訂購資料一起刪除
            //刪除訂單
            var deleteOrderC = db.CreateBuyOrder_LeaderOrders.Where(m => m.UserID == id);
            if (deleteOrderC.FirstOrDefault() != null)
            {
                db.CreateBuyOrder_LeaderOrders.RemoveRange(deleteOrderC);
            }

            //刪除訊息
            var deleteMessageM = db.SendMessageViews.Where(m => m.UserID == id);
            if (deleteMessageM.FirstOrDefault() != null)
            {
                db.SendMessageViews.RemoveRange(deleteMessageM);
            }

            //刪除團長訊息
            var deleteLeaderMess = db.LeaderSendMessages.Where(m => m.SentUser == id);
            if (deleteLeaderMess.FirstOrDefault() != null)
            {
                db.LeaderSendMessages.RemoveRange(deleteLeaderMess);
            }

            db.SaveChanges();

            return RedirectToAction("MyUser", "Manager");
        }

        /************* 菜單管理 *************/

        //GET: Manager/MyMenu
        public ActionResult MyMenu(int? page)
        {
            var qyery = db.Menus.OrderBy(s => s.ID);

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(qyery.ToPagedList(pageNumber, pageSize));
        }

        //GET: Manager/AddMenu
        public ActionResult AddMenu()
        {
            var result = Models.ManagerModels.GetFakeMenus2();
            return View(result);
        }

        //POST: Manager/AddMenu
        //新版
        [HttpPost]
        public ActionResult AddMenu(NewMenu newM, HttpPostedFileBase file)
        {
            int Price = 0;
            //判斷加料欄位null
            if (newM.AddItemCreate != null /*|| newM.AddItemCreate.Count != 0*/)
            {
                Price = Models.ManagerModels.PriceIsNotSmall(newM);
            }
            //加料金額則以不為負數為基準
            if (!ModelState.IsValid || Price < 0)
            {
                if (Price < 0)
                {
                    ViewBag.PriceSmall = "價錢不得為負值.";
                }

                ViewBag.EmptyError = "若有新增空白欄,請填寫完畢再儲存.";
                //判斷是哪個項目空值,並彈出視窗
                //加料
                if (newM.AddItemCreate != null)
                {
                    foreach (var item in newM.AddItemCreate)
                    {
                        if (String.IsNullOrEmpty(item.ItemName) || item.ItemName == null )
                        {
                            ViewBag.AddItem = "未填寫";
                            break;
                        }
                    }
                }
                //甜度
                if (newM.SweetName != null)
                {
                    foreach (var item in newM.SweetName)
                    {
                        if (String.IsNullOrEmpty(item.SweetName) || item.SweetName == null)
                        {
                            ViewBag.SweetName = "未填寫";
                            break;
                        }
                    }
                }
                //冰度
                if (newM.IceName != null)
                {
                    foreach (var item in newM.IceName)
                    {
                        if (String.IsNullOrEmpty(item.IceName) || item.IceName == null)
                        {
                            ViewBag.IceName = "未填寫";
                            break;
                        }
                    }
                }
                //尺寸
                if (newM.SizeName != null)
                {
                    foreach (var item in newM.SizeName)
                    {
                        if (String.IsNullOrEmpty(item.SizeName) || item.SizeName == null)
                        {
                            ViewBag.SizeName = "未填寫";
                            break;
                        }
                    }
                }

                var Menu = Models.ManagerModels.ErrorBackMenu(newM);
                return View(Menu);
            }

            //先寫入主Menu,取ID(要判斷Menu名不重複)
            var query = db.Menus.Where(m => m.MenuName == newM.MenuName);
            if (query.FirstOrDefault() != null)
            {
                ViewBag.EmptyError = "您已有相同名稱的菜單,請勿重覆建立.";
                var Menu = Models.ManagerModels.ErrorBackMenu(newM);
                return View(Menu);
            }

            //上傳圖片
            string noImage = "19.jpg";
            if (file != null)
            {
                if (file.ContentLength > 0 && file.ContentLength <= 1024 * 1024)
                {
                    //允許的圖片格式
                    var allowedExtensions = new[] { ".png", ".gif", ".jpg", ".jpeg" };
                    var extension = Path.GetExtension(file.FileName);
                    if (!allowedExtensions.Contains(extension))
                    {
                        //格式不正確
                        ViewBag.Message = "圖片格式錯誤.";
                        var Menu = Models.ManagerModels.ErrorBackMenu(newM);
                        return View(Menu);
                    }

                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/MenuImage"), fileName);
                    file.SaveAs(path);
                    noImage = fileName;
                }
                else
                {
                    //超出尺寸
                    ViewBag.Message = "圖片尺寸不能超過1024KB.";
                    var Menu = Models.ManagerModels.ErrorBackMenu(newM);
                    return View(Menu);
                }
            }
            DateTime time = DateTime.Now;
            if (String.IsNullOrEmpty(newM.OrderConditions))
            {
                newM.OrderConditions = "無";
            }
            var MainMenu = new Menu()
            {
                CreateTime = time,
                MenuName = newM.MenuName,
                MenuPhone = newM.MenuPhone,
                Addr = newM.Addr,
                Open = false,
                ImageName = noImage,
                OrderConditions = newM.OrderConditions
            };
            db.Menus.Add(MainMenu);
            db.SaveChanges();
            int MenuID = Models.ManagerModels.MenuID(newM.MenuName);

            //若甜度未填則使用預設甜度
            //判斷甜度.兩個都沒有選的情況
            var SweetCheck = Models.ManagerModels.CheckCheckboxNotEmpty(newM, "sweet");
            if ((newM.SweetName == null || newM.SweetName.Count == 0) && (SweetCheck == null || SweetCheck.Count == 0))
            {
                //給預設甜度
                var NewMenuSweet = new List<MenuSweet>();
                for (int i = 1; i <= 4; i++)
                {
                    var NameS = db.SweetTypes.Find(i);
                    NewMenuSweet.Add(new MenuSweet()
                    {
                        MenuID = MenuID,
                        SweetID = i,
                        SweetName = NameS.SweetName
                    });
                }
                db.MenuSweets.AddRange(NewMenuSweet);
            }

            //若冰度未填則使用預設冰度
            //判斷冰度.兩個都沒有選的情況
            var IceCheck = Models.ManagerModels.CheckCheckboxNotEmpty(newM, "ice");
            if ((newM.IceName == null || newM.IceName.Count == 0) && (IceCheck == null || IceCheck.Count == 0))
            {
                var NewMenuIce = new List<MenuIce>();
                for (int i = 1; i <= 5; i++)
                {
                    var NameS = db.IceTypes.Find(i);
                    NewMenuIce.Add(new MenuIce()
                    {
                        MenuID = MenuID,
                        IceID = i,
                        IceName = NameS.IceName
                    });
                }
                db.MenuIces.AddRange(NewMenuIce);
            }

            //若尺寸度未填則使用預設尺寸
            //判斷尺寸.兩個都沒有選的情況
            var SizeCheck = Models.ManagerModels.CheckCheckboxNotEmpty(newM, "size");
            if ((newM.SizeName == null || newM.SizeName.Count == 0) && (SizeCheck == null || SizeCheck.Count == 0))
            {
                var NewMenuSize = new List<MenuSize>();
                for (int i = 1; i <= 2; i++)
                {
                    var NameS = db.SizeTypes.Find(i);
                    NewMenuSize.Add(new MenuSize()
                    {
                        MenuID = MenuID,
                        SizeID = i,
                        SizeName = NameS.SizeName
                    });
                }
                db.MenuSizes.AddRange(NewMenuSize);
            }

            //若加料未填則使用使其空值
            //寫入加料表,加料價表,店家加料表
            //先寫入加料表取ID,避免重覆名字
            if (newM.AddItemCreate != null)
            {
                var NewItem = new List<AddItemType>();
                foreach (var item in newM.AddItemCreate)
                {
                    var NoItemRepeat = db.AddItemTypes.Where(m => m.ItemName == item.ItemName);
                    if (NoItemRepeat.FirstOrDefault() == null)
                    {
                        NewItem.Add(new AddItemType()
                        {
                            ItemName = item.ItemName
                        });
                    }
                }
                if (NewItem != null || NewItem.Count != 0)
                {
                    db.AddItemTypes.AddRange(NewItem);
                    db.SaveChanges();
                }
                //寫入加料價表
                var NewItemPrice = new List<AddItemTypePrice>();
                foreach (var item in newM.AddItemCreate)
                {
                    int AddItemID = Models.ManagerModels.SweetID(item.ItemName, "additem");
                    NewItemPrice.Add(new AddItemTypePrice()
                    {
                        ItemID = AddItemID,
                        MenuID = MenuID,
                        SizeNumber = 0,
                        ItemPrice = item.LPrice
                    });
                    NewItemPrice.Add(new AddItemTypePrice()
                    {
                        ItemID = AddItemID,
                        MenuID = MenuID,
                        SizeNumber = 1,
                        ItemPrice = item.MPrice
                    });
                    NewItemPrice.Add(new AddItemTypePrice()
                    {
                        ItemID = AddItemID,
                        MenuID = MenuID,
                        SizeNumber = 2,
                        ItemPrice = item.SPrice
                    });
                }
                db.AddItemTypePrices.AddRange(NewItemPrice);
                db.SaveChanges();
            }


            //寫入店家加料表
            var NewMenuAddItem = new List<MenuAddItem>();
            var ItemPriceID = Models.ManagerModels.PickAllItemPrice(MenuID);
            foreach (var item in ItemPriceID)
            {
                NewMenuAddItem.Add(new MenuAddItem()
                {
                    MenuID = MenuID,
                    ItemIDPriceID = item
                });
            }
            db.MenuAddItems.AddRange(NewMenuAddItem);

            //判斷甜度.只填一個或都填的情況/////////////////////////////////////                       
            if (SweetCheck != null || SweetCheck.Count > 0)
            {
                //若勾選,則填入店家甜度表
                var NewMenuSweet = new List<MenuSweet>();
                foreach (var item in SweetCheck)
                {
                    var NameS = db.SweetTypes.Find(item);
                    NewMenuSweet.Add(new MenuSweet()
                    {
                        MenuID = MenuID,
                        SweetID = item,
                        SweetName = NameS.SweetName
                    });
                }
                db.MenuSweets.AddRange(NewMenuSweet);
            }
            if (newM.SweetName != null)
            {
                //若新增,則新增甜度表種類   
                //判斷是否有重複的名字
                var NewSweetType = new List<SweetType>();
                foreach (var item in newM.SweetName)
                {
                    var NoRepeat = db.SweetTypes.Where(m => m.SweetName == item.SweetName);
                    if (NoRepeat.FirstOrDefault() == null)
                    {
                        NewSweetType.Add(new SweetType()
                        {
                            SweetName = item.SweetName
                        });
                    }
                }
                if (NewSweetType != null || NewSweetType.Count != 0)
                {
                    db.SweetTypes.AddRange(NewSweetType);
                    db.SaveChanges();
                }
                //取出甜度ID,寫入店家甜度表
                var NewMenuSweet = new List<MenuSweet>();
                foreach (var item in newM.SweetName)
                {
                    int SweetID = Models.ManagerModels.SweetID(item.SweetName, "sweet");
                    var NameS = db.SweetTypes.Find(SweetID);
                    NewMenuSweet.Add(new MenuSweet()
                    {
                        MenuID = MenuID,
                        SweetID = SweetID,
                        SweetName = NameS.SweetName
                    });
                }
                db.MenuSweets.AddRange(NewMenuSweet);
            }

            //判斷冰度.只填一個或都填的情況///////////////////////////////////                       
            if (IceCheck != null || IceCheck.Count > 0)
            {
                //若勾選,則填入店家冰度表
                var NewMenuIce = new List<MenuIce>();
                foreach (var item in IceCheck)
                {
                    var NameS = db.IceTypes.Find(item);
                    NewMenuIce.Add(new MenuIce()
                    {
                        MenuID = MenuID,
                        IceID = item,
                        IceName = NameS.IceName
                    });
                }
                db.MenuIces.AddRange(NewMenuIce);
            }
            if (newM.IceName != null)
            {
                //若新增,則新增冰度表種類   
                //判斷是否有重複的名字
                var NewIceType = new List<IceType>();
                foreach (var item in newM.IceName)
                {
                    var NoRepeat = db.IceTypes.Where(m => m.IceName == item.IceName);
                    if (NoRepeat.FirstOrDefault() == null)
                    {
                        NewIceType.Add(new IceType()
                        {
                            IceName = item.IceName
                        });
                    }
                }
                if (NewIceType != null || NewIceType.Count != 0)
                {
                    db.IceTypes.AddRange(NewIceType);
                    db.SaveChanges();
                }
                //取出冰度ID,寫入店家冰度表
                var NewMenuIce = new List<MenuIce>();
                foreach (var item in newM.IceName)
                {
                    int IceID = Models.ManagerModels.SweetID(item.IceName, "ice");
                    var NameS = db.IceTypes.Find(IceID);
                    NewMenuIce.Add(new MenuIce()
                    {
                        MenuID = MenuID,
                        IceID = IceID,
                        IceName = NameS.IceName
                    });
                }
                db.MenuIces.AddRange(NewMenuIce);
            }

            //判斷尺寸.只填一個或都填的情況///////////////////////////////////  
            if (SizeCheck != null || SizeCheck.Count > 0)
            {
                //若勾選,則填入店家尺寸表
                var NewMenuSize = new List<MenuSize>();
                foreach (var item in SizeCheck)
                {
                    var NameS = db.SizeTypes.Find(item);
                    NewMenuSize.Add(new MenuSize()
                    {
                        MenuID = MenuID,
                        SizeID = item,
                        SizeName = NameS.SizeName
                    });
                }
                db.MenuSizes.AddRange(NewMenuSize);
            }
            if (newM.SizeName != null)
            {
                //若新增,則新增尺寸表種類   
                //判斷是否有重複的名字
                var NewSizeType = new List<SizeType>();
                foreach (var item in newM.SizeName)
                {
                    var NoRepeat = db.SizeTypes.Where(m => m.SizeName == item.SizeName);
                    if (NoRepeat.FirstOrDefault() == null)
                    {
                        NewSizeType.Add(new SizeType()
                        {
                            SizeName = item.SizeName
                        });
                    }
                }
                if (NewSizeType != null || NewSizeType.Count != 0)
                {
                    db.SizeTypes.AddRange(NewSizeType);
                    db.SaveChanges();
                }
                //取出尺寸ID,寫入店家尺寸表
                var NewMenuSize = new List<MenuSize>();
                foreach (var item in newM.SizeName)
                {
                    int SizeID = Models.ManagerModels.SweetID(item.SizeName, "size");
                    var NameS = db.SizeTypes.Find(SizeID);
                    NewMenuSize.Add(new MenuSize()
                    {
                        MenuID = MenuID,
                        SizeID = SizeID,
                        SizeName = NameS.SizeName
                    });
                }
                db.MenuSizes.AddRange(NewMenuSize);
            }
            //存入資料庫
            db.SaveChanges();

            //依照menuID進到新增飲料介面
            //return RedirectToAction("AddDrinks", "Manager", new { id = MenuID });
            //return RedirectToAction("Add_NewDrinks", "Manager", new { id = MenuID });
            return RedirectToAction("_Add2_NewDrinks", "TypeChangeManager", new { id = MenuID });
            //return RedirectToAction("_Add2_NewDrinks", "Manager", new { id = MenuID });
        }


        //原版
        //[HttpPost]
        //public ActionResult AddMenu(NewMenu newM, HttpPostedFileBase file)
        //{
        //    //判斷加料欄位不得刪除全部,導致此欄位null
        //    if (newM.AddItemCreate == null || newM.AddItemCreate.Count == 0)
        //    {
        //        ViewBag.EmptyError = "加料欄位尚未填寫,請填寫完畢再儲存.";
        //        var Menu = Models.ManagerModels.ErrorBackMenu(newM);
        //        return View(Menu);
        //    }

        //    //加料金額則以不為負數為基準
        //    int Price = Models.ManagerModels.PriceIsNotSmall(newM);

        //    if (!ModelState.IsValid || Price < 0)
        //    {
        //        if (Price < 0)
        //        {
        //            ViewBag.PriceSmall = "價錢不得為負值.";
        //        }

        //        ViewBag.EmptyError = "若有新增空白欄,請填寫完畢再儲存.";

        //        var Menu = Models.ManagerModels.ErrorBackMenu(newM);
        //        return View(Menu);
        //    }

        //    //分別判斷甜度,冰度,大小勾選及動態欄其一必填
        //    //先判斷是否有動態欄位,若無則檢查是否有勾選
        //    //判斷甜度.兩個都沒有選的情況
        //    var SweetCheck = Models.ManagerModels.CheckCheckboxNotEmpty(newM, "sweet");
        //    if ((newM.SweetName == null || newM.SweetName.Count == 0) && (SweetCheck == null || SweetCheck.Count == 0))
        //    {
        //        ViewBag.EmptyError = "甜度欄位尚未填寫,請填寫完畢再儲存.";
        //        var Menu = Models.ManagerModels.ErrorBackMenu(newM);
        //        return View(Menu);
        //    }

        //    //判斷冰度.兩個都沒有選的情況
        //    var IceCheck = Models.ManagerModels.CheckCheckboxNotEmpty(newM, "ice");
        //    if ((newM.IceName == null || newM.IceName.Count == 0) && (IceCheck == null || IceCheck.Count == 0))
        //    {
        //        ViewBag.EmptyError = "冰度欄位尚未填寫,請填寫完畢再儲存.";
        //        var Menu = Models.ManagerModels.ErrorBackMenu(newM);
        //        return View(Menu);
        //    }

        //    //判斷尺寸.兩個都沒有選的情況
        //    var SizeCheck = Models.ManagerModels.CheckCheckboxNotEmpty(newM, "size");
        //    if ((newM.SizeName == null || newM.SizeName.Count == 0) && (SizeCheck == null || SizeCheck.Count == 0))
        //    {
        //        ViewBag.EmptyError = "尺寸欄位尚未填寫,請填寫完畢再儲存.";
        //        var Menu = Models.ManagerModels.ErrorBackMenu(newM);
        //        return View(Menu);
        //    }

        //    //先寫入主Menu,取ID(要判斷Menu名不重複)
        //    var query = db.Menus.Where(m => m.MenuName == newM.MenuName);
        //    if (query.FirstOrDefault() != null)
        //    {
        //        ViewBag.EmptyError = "您已有相同名稱的菜單,請勿重覆建立.";
        //        var Menu = Models.ManagerModels.ErrorBackMenu(newM);
        //        return View(Menu);
        //    }

        //    //上傳圖片
        //    string noImage = "19.jpg";
        //    if (file != null)
        //    {
        //        if (file.ContentLength > 0 && file.ContentLength <= 1024 * 1024)
        //        {
        //            //允許的圖片格式
        //            var allowedExtensions = new[] { ".png", ".gif", ".jpg", ".jpeg" };
        //            var extension = Path.GetExtension(file.FileName);
        //            if (!allowedExtensions.Contains(extension))
        //            {
        //                //格式不正確
        //                ViewBag.Message = "圖片格式錯誤.";
        //                var Menu = Models.ManagerModels.ErrorBackMenu(newM);
        //                return View(Menu);
        //            }

        //            var fileName = Path.GetFileName(file.FileName);
        //            var path = Path.Combine(Server.MapPath("~/MenuImage"), fileName);
        //            file.SaveAs(path);
        //            noImage = fileName;
        //        }
        //        else
        //        {
        //            //超出尺寸
        //            ViewBag.Message = "圖片尺寸不能超過1024KB.";
        //            var Menu = Models.ManagerModels.ErrorBackMenu(newM);
        //            return View(Menu);
        //        }
        //    }

        //    DateTime time = DateTime.Now;
        //    var MainMenu = new Menu()
        //    {
        //        CreateTime = time,
        //        MenuName = newM.MenuName,
        //        MenuPhone = newM.MenuPhone,
        //        Addr = newM.Addr,
        //        Open = false,
        //        ImageName = noImage,
        //        OrderConditions = newM.OrderConditions
        //    };
        //    db.Menus.Add(MainMenu);
        //    db.SaveChanges();
        //    int MenuID = Models.ManagerModels.MenuID(newM.MenuName);

        //    //寫入加料表,加料價表,店家加料表
        //    //先寫入加料表取ID,避免重覆名字
        //    var NewItem = new List<AddItemType>();
        //    foreach (var item in newM.AddItemCreate)
        //    {
        //        var NoItemRepeat = db.AddItemTypes.Where(m => m.ItemName == item.ItemName);                
        //        if (NoItemRepeat.FirstOrDefault() == null)
        //        {
        //            NewItem.Add(new AddItemType()
        //            {
        //                ItemName = item.ItemName
        //            });
        //        }                             
        //    }
        //    if (NewItem != null || NewItem.Count != 0)
        //    {
        //        db.AddItemTypes.AddRange(NewItem);
        //        db.SaveChanges();
        //    }
        //    //寫入加料價表
        //    var NewItemPrice = new List<AddItemTypePrice>();
        //    foreach (var item in newM.AddItemCreate)
        //    {
        //        int AddItemID = Models.ManagerModels.SweetID(item.ItemName, "additem");
        //        NewItemPrice.Add(new AddItemTypePrice()
        //        {
        //            ItemID = AddItemID,
        //            MenuID = MenuID,
        //            SizeNumber = 0,
        //            ItemPrice = item.LPrice
        //        });
        //        NewItemPrice.Add(new AddItemTypePrice()
        //        {
        //            ItemID = AddItemID,
        //            MenuID = MenuID,
        //            SizeNumber = 1,
        //            ItemPrice = item.MPrice
        //        });
        //        NewItemPrice.Add(new AddItemTypePrice()
        //        {
        //            ItemID = AddItemID,
        //            MenuID = MenuID,
        //            SizeNumber = 2,
        //            ItemPrice = item.SPrice
        //        });
        //    }
        //    db.AddItemTypePrices.AddRange(NewItemPrice);
        //    db.SaveChanges();
        //    //寫入店家加料表
        //    var NewMenuAddItem = new List<MenuAddItem>();
        //    var ItemPriceID = Models.ManagerModels.PickAllItemPrice(MenuID);
        //    foreach (var item in ItemPriceID)
        //    {
        //        NewMenuAddItem.Add(new MenuAddItem()
        //        {
        //            MenuID = MenuID,
        //            ItemIDPriceID = item
        //        });
        //    }
        //    db.MenuAddItems.AddRange(NewMenuAddItem);

        //    //判斷甜度.只填一個或都填的情況/////////////////////////////////////                       
        //    if (SweetCheck != null || SweetCheck.Count > 0)
        //    {
        //        //若勾選,則填入店家甜度表
        //        var NewMenuSweet = new List<MenuSweet>();
        //        foreach (var item in SweetCheck)
        //        {
        //            NewMenuSweet.Add(new MenuSweet()
        //            {
        //                MenuID = MenuID,
        //                SweetID = item
        //            });
        //        }
        //        db.MenuSweets.AddRange(NewMenuSweet);
        //    }
        //    if (newM.SweetName != null)
        //    {
        //        //若新增,則新增甜度表種類   
        //        //判斷是否有重複的名字
        //        var NewSweetType = new List<SweetType>();
        //        foreach (var item in newM.SweetName)
        //        {
        //            var NoRepeat = db.SweetTypes.Where(m => m.SweetName == item.SweetName);
        //            if (NoRepeat.FirstOrDefault() == null)
        //            {
        //                NewSweetType.Add(new SweetType()
        //                {
        //                    SweetName = item.SweetName
        //                });
        //            }                    
        //        }
        //        if (NewSweetType != null || NewSweetType.Count != 0)
        //        {
        //            db.SweetTypes.AddRange(NewSweetType);
        //            db.SaveChanges();
        //        }
        //        //取出甜度ID,寫入店家甜度表
        //        var NewMenuSweet = new List<MenuSweet>();
        //        foreach (var item in newM.SweetName)
        //        {
        //            int SweetID = Models.ManagerModels.SweetID(item.SweetName, "sweet");
        //            NewMenuSweet.Add(new MenuSweet()
        //            {
        //                MenuID = MenuID,
        //                SweetID = SweetID
        //            });
        //        }
        //        db.MenuSweets.AddRange(NewMenuSweet);
        //    }

        //    //判斷冰度.只填一個或都填的情況///////////////////////////////////                       
        //    if (IceCheck != null || IceCheck.Count > 0)
        //    {
        //        //若勾選,則填入店家冰度表
        //        var NewMenuIce = new List<MenuIce>();
        //        foreach (var item in IceCheck)
        //        {
        //            NewMenuIce.Add(new MenuIce()
        //            {
        //                MenuID = MenuID,
        //                IceID = item
        //            });
        //        }
        //        db.MenuIces.AddRange(NewMenuIce);
        //    }
        //    if (newM.IceName != null)
        //    {
        //        //若新增,則新增冰度表種類   
        //        //判斷是否有重複的名字
        //        var NewIceType = new List<IceType>();
        //        foreach (var item in newM.IceName)
        //        {
        //            var NoRepeat = db.IceTypes.Where(m => m.IceName == item.IceName);
        //            if (NoRepeat.FirstOrDefault() == null)
        //            {
        //                NewIceType.Add(new IceType()
        //                {
        //                    IceName = item.IceName
        //                });
        //            }                   
        //        }
        //        if (NewIceType != null || NewIceType.Count != 0)
        //        {
        //            db.IceTypes.AddRange(NewIceType);
        //            db.SaveChanges();
        //        }
        //        //取出冰度ID,寫入店家冰度表
        //        var NewMenuIce = new List<MenuIce>();
        //        foreach (var item in newM.IceName)
        //        {
        //            int IceID = Models.ManagerModels.SweetID(item.IceName, "ice");
        //            NewMenuIce.Add(new MenuIce()
        //            {
        //                MenuID = MenuID,
        //                IceID = IceID
        //            });
        //        }
        //        db.MenuIces.AddRange(NewMenuIce);
        //    }

        //    //判斷尺寸.只填一個或都填的情況///////////////////////////////////  
        //    if (SizeCheck != null || SizeCheck.Count > 0)
        //    {
        //        //若勾選,則填入店家尺寸表
        //        var NewMenuSize = new List<MenuSize>();
        //        foreach (var item in SizeCheck)
        //        {
        //            NewMenuSize.Add(new MenuSize()
        //            {
        //                MenuID = MenuID,
        //                SizeID = item
        //            });
        //        }
        //        db.MenuSizes.AddRange(NewMenuSize);
        //    }
        //    if (newM.SizeName != null)
        //    {
        //        //若新增,則新增尺寸表種類   
        //        //判斷是否有重複的名字
        //        var NewSizeType = new List<SizeType>();
        //        foreach (var item in newM.SizeName)
        //        {
        //            var NoRepeat = db.SizeTypes.Where(m => m.SizeName == item.SizeName);
        //            if (NoRepeat.FirstOrDefault() == null)
        //            {
        //                NewSizeType.Add(new SizeType()
        //                {
        //                    SizeName = item.SizeName
        //                });
        //            }                   
        //        }
        //        if (NewSizeType != null || NewSizeType.Count != 0)
        //        {
        //            db.SizeTypes.AddRange(NewSizeType);
        //            db.SaveChanges();
        //        }
        //        //取出尺寸ID,寫入店家尺寸表
        //        var NewMenuSize = new List<MenuSize>();
        //        foreach (var item in newM.SizeName)
        //        {
        //            int SizeID = Models.ManagerModels.SweetID(item.SizeName, "size");
        //            NewMenuSize.Add(new MenuSize()
        //            {
        //                MenuID = MenuID,
        //                SizeID = SizeID
        //            });
        //        }
        //        db.MenuSizes.AddRange(NewMenuSize);
        //    }
        //    //存入資料庫
        //    db.SaveChanges();

        //    //依照menuID進到新增飲料介面
        //    return RedirectToAction("AddDrinks", "Manager", new { id = MenuID });
        //}

        //GET: Manager/_ItemTypeEntry
        public ActionResult _ItemTypeEntryRow()
        {
            var result = new AddItemCreate()
            {
                ItemName = "",
                LPrice = 0,
                MPrice = 0,
                SPrice = 0
            };
            return PartialView("_ItemTypeEntry", result);
        }       

        //GET: Manager/_SweetTypeEntry
        public ActionResult _SweetTypeEntryRow()
        {
            return PartialView("_SweetTypeEntry");
        }

        //GET: Manager/_IceTypeEntry
        public ActionResult _IceTypeEntryRow()
        {
            return PartialView("_IceTypeEntry");
        }

        //GET: Manager/_SizeTypeEntry
        public ActionResult _SizeTypeEntryRow()
        {
            return PartialView("_SizeTypeEntry");
        }

        //GET: Manager/MyMenuDelete
        public ActionResult MyMenuDelete(int id)
        {
            Menu query = db.Menus.Find(id);
            if (query == null)
            {
                return HttpNotFound();
            }

            //刪除關連項:
            /////針對整體菜單///////////
            //主菜單 Menu
            //加料價 AddItemTypePrice
            //店家甜度表 MenuSweet
            //店家冰度表 MenuIce
            //店家尺寸表 MenuSize
            //店家加料表 MenuAddItem
            //
            ////針對飲料///////////////
            //刪除時要注意順序,"飲料尺寸表"關係到其餘相關表格
            //
            //飲料名及種類表 MenuDrink
            //飲料甜度表 SweetTable
            //飲料冰度表 IceTable
            //飲料尺寸價表 SizeTable
            //飲料加料表 AddItemTable
            //
            ///////////////////////////////

            //主菜單 Menu
            db.Menus.Remove(query);

            //加料價
            var AddItemP = db.AddItemTypePrices.Where(m => m.MenuID == query.ID);
            db.AddItemTypePrices.RemoveRange(AddItemP);

            //店家甜度表
            var Menusweet = db.MenuSweets.Where(m => m.MenuID == query.ID);
            db.MenuSweets.RemoveRange(Menusweet);

            //店家冰度表 
            var Menuice = db.MenuIces.Where(m => m.MenuID == query.ID);
            db.MenuIces.RemoveRange(Menuice);

            //店家尺寸表 
            var Menusize = db.MenuSizes.Where(m => m.MenuID == query.ID);
            db.MenuSizes.RemoveRange(Menusize);

            //店家加料表 
            var Menuadditem = db.MenuAddItems.Where(m => m.MenuID == query.ID);
            db.MenuAddItems.RemoveRange(Menuadditem);

            //飲料名及種類表
            var drinkNameT = db.MenuDrinks.Where(m => m.MenuID == query.ID);
            db.MenuDrinks.RemoveRange(drinkNameT);
            
            //飲料尺寸價表
            var drinkSizeP = db.SizeTables.Where(m => m.MenuID == query.ID);
            foreach (var item in drinkSizeP)
            {
                //飲料甜度表
                var drinkSweet = db.SweetTables.Where(m => m.SizePID == item.ID);
                db.SweetTables.RemoveRange(drinkSweet);

                //飲料冰度表
                var drinkIce = db.IceTables.Where(m => m.SizePID == item.ID);
                db.IceTables.RemoveRange(drinkIce);

                //飲料加料表
                var drinkAdditem = db.AddItemTables.Where(m => m.SizePID == item.ID);
                db.AddItemTables.RemoveRange(drinkAdditem);
            }
            db.SizeTables.RemoveRange(drinkSizeP);

            //針對相關訂單的刪除
            //團長創建訂單
            //飲料細項訂單

            var deleteOrder = db.CreateBuyOrder_LeaderOrders.Where(m => m.MenuID == id);
            if (deleteOrder.FirstOrDefault() != null)
            {
                db.CreateBuyOrder_LeaderOrders.RemoveRange(deleteOrder);
            }

            var deleteDetail = db.CreateBuyOrder_Details.Where(m => m.MenuID == id);
            if (deleteDetail.FirstOrDefault() != null)
            {
                db.CreateBuyOrder_Details.RemoveRange(deleteDetail);
            }

            db.SaveChanges();

            return RedirectToAction("MyMenu", "Manager");
        }

        //GET: Manager/AddDrinks
        public ActionResult AddDrinks(int id)
        {
            Menu query = db.Menus.Find(id);
            if (query == null)
            {
                return HttpNotFound();
            }
            //用菜單ID取出菜單名及菜單ViewModel
            AddDrinkDetails result = Models.ManagerModels.CurrentAddDrinkDetails(id);

            return View(result);
        }

        //GET: Manager/_DrinkDetailsEntry
        public ActionResult _DrinkDetailsEntryRow(int id)
        {
            var result = Models.ManagerModels.PartialAddDrinkDetails(id);
            return PartialView("_DrinkDetailsEntry", result);
        }

        //POST: Manager/AddDrinks
        [HttpPost]
        public ActionResult AddDrinks(AddDrinkDetails addDks)
        {
            //判斷新增飲料欄位不得刪除全部,導致此欄位null
            if (addDks.DrinkDetails == null || addDks.DrinkDetails.Count == 0)
            {
                ViewBag.EmptyError = "您沒有新增任何飲品.";
                return View(addDks);
            }
            //飲品金額則以不為負數為基準
            int Price = Models.ManagerModels.DrinkPriceIsNotSmall(addDks);
            if (!ModelState.IsValid || Price < 0)
            {
                if (Price < 0)
                {
                    ViewBag.PriceSmall = "價錢不得為負值.";
                }
                var result = Models.ManagerModels.BackAddDrinkDetails(addDks);
                return View(addDks);
            }

            //判斷勾選甜度.冰度.欄位必須勾選一項(由於是多項,所以必須一樣樣判斷)
            //加料由於有可加與不可加所以就不判斷
            foreach (var item in addDks.DrinkDetails)
            {
                var CheckSweet = Models.ManagerModels.AddDrink_CheckboxNotEmpty(item, "sweet");
                var CheckIce = Models.ManagerModels.AddDrink_CheckboxNotEmpty(item, "ice");
                if ((CheckSweet == null || CheckSweet.Count == 0) || (CheckIce == null || CheckIce.Count == 0))
                {
                    ViewBag.EmptyError = "請將欄位填寫完全,甜度及冰度欄位請至少勾選一項.";
                    var result = Models.ManagerModels.BackAddDrinkDetails(addDks);
                    return View(addDks);
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
            foreach (var item in addDks.DrinkDetails)
            {
                var query = db.MenuDrinks.Where(m => m.MenuID == addDks.MenuID && m.DrinkType == item.DrinkType && m.DrinkName == item.DrinkName);
                if (query.FirstOrDefault() != null)
                {
                    continue;
                }
                else
                {
                    var NewDrinks = new MenuDrink()
                    {
                        MenuID = addDks.MenuID,
                        DrinkType = item.DrinkType,
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
            foreach (var item in addDks.DrinkDetails)
            {
                int DrinkID = Models.ManagerModels.PickDrinkID_UseModel(item, addDks.MenuID);
                //飲料尺寸價表
                int SizeID = Int32.Parse(item.SizeTypeM);
                NewSizeP.Add(new SizeTable()
                {
                    DrinkID = DrinkID,
                    SizeID = SizeID,
                    Price = item.DrinkPrice,
                    MenuID = addDks.MenuID
                });
            }
            db.SizeTables.AddRange(NewSizeP);
            db.SaveChanges();

            //依照DrinkID,再用DrinkID.SizeID.Price取SizePID//////////////////////////
            var NewSweetTable = new List<SweetTable>();
            var NewIceTable = new List<IceTable>();
            var NewAddItemTable = new List<AddItemTable>();
            foreach (var item in addDks.DrinkDetails)
            {
                int DrinkID = Models.ManagerModels.PickDrinkID_UseModel(item, addDks.MenuID);
                int SizePID = Models.ManagerModels.PickSizePID_UseDidSidPrice(item, DrinkID);
                var CheckSweet = Models.ManagerModels.AddDrink_CheckboxNotEmpty(item, "sweet");
                var CheckIce = Models.ManagerModels.AddDrink_CheckboxNotEmpty(item, "ice");
                var CheckAddItem = Models.ManagerModels.AddDrink_CheckboxNotEmpty(item, "additem");

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
            }
            db.SweetTables.AddRange(NewSweetTable);
            db.IceTables.AddRange(NewIceTable);
            if (NewAddItemTable != null || NewAddItemTable.Count != 0)
            {
                db.AddItemTables.AddRange(NewAddItemTable);
            }
            db.SaveChanges();

            //寫完依menuID回到飲料詳細介面
            return RedirectToAction("MyMenuDrinkDetail", "Manager", new { id = addDks.MenuID });
        }

        //GET: Manager/MyMenuDrinkDetail
        public ActionResult MyMenuDrinkDetail(int? id, int? page, string SearchString)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Menu query = db.Menus.Find(id);
            if (query == null)
            {
                return HttpNotFound();
            }
            ViewBag.Page = page;
            ViewBag.SearchString = SearchString;
            return View(query);
        }

        //GET: Manager/_DrinkDetailPartial
        public ActionResult _DrinkDetailPartial(int id, int? page, string SearchString)
        {
            Menu query = db.Menus.Find(id);
            var result = Models.ManagerModels.GetMenuDrinkView(query.ID, query.MenuName);

            if (!String.IsNullOrEmpty(SearchString))
            {
                result.Where(m => m.DrinkName.Contains(SearchString) || m.DrinkType.Contains(SearchString));
            }

            var result2 = result.OrderBy(m => m.DrinkSort);
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.Page = page;
            ViewBag.SearchString = SearchString;
            return PartialView("_DrinkDetailPartial", result2.ToPagedList(pageNumber, pageSize));
        }

        //GET: Manager/_ChangeSort
        public ActionResult _ChangeSort(int Drinkid, int sort, int menuID, int? page, string SearchString)
        {
            //取出要改變的飲料的id以及向下移或上移
            var mydrink = db.SizeTables.Find(Drinkid);
            int CanC = mydrink.DrinkSort;
            //下移
            if (sort == 1)
            {               
                var findnx = db.SizeTables.Where(m => m.MenuID == menuID && m.DrinkSort > CanC);
                //要先判斷自己不是最後一個
                if (findnx.FirstOrDefault() == null)
                {
                    //自己是最後一個,不動作
                }
                if (findnx.FirstOrDefault() != null)
                {
                    //改變自己的編號並與自己的上一個互換編號
                    int myNewNum = CanC + 1;
                    mydrink.DrinkSort = myNewNum;
                    //找出離自己編號最近的下一號(因為有可能下一個被刪出了)
                    var findnx2 = findnx.OrderBy(m => m.DrinkSort);
                    foreach (var item in findnx2)
                    {
                        item.DrinkSort = item.DrinkSort - 1;
                        break;
                    }
                    db.SaveChanges();
                }
            }
            //上移
            if (sort == -1)
            {                                
                //要先判斷自己不是第一個
                if (CanC -1 <= 0)
                {
                    //自己是第一個,不動作
                }
                if ((CanC -1) > 0)
                {
                    //改變自己的編號並與自己的上一個互換編號
                    int myNewNum = CanC - 1;
                    mydrink.DrinkSort = myNewNum;
                    //找出離自己編號最近的上一號(因為有可能上一個被刪出了)
                    var findup = db.SizeTables.Where(m => m.MenuID == menuID && m.DrinkSort <= myNewNum);
                    if (findup.FirstOrDefault() != null)
                    {
                        findup = findup.OrderByDescending(m => m.DrinkSort);
                        foreach (var item in findup)
                        {
                            item.DrinkSort = item.DrinkSort + 1;
                            break;
                        }
                    }
                    db.SaveChanges();
                }
            }
            //之後取出目前的頁數及menuid
            //回傳視圖
            Menu query = db.Menus.Find(menuID);
            var result = Models.ManagerModels.GetMenuDrinkView(query.ID, query.MenuName);

            if (!String.IsNullOrEmpty(SearchString))
            {
                result.Where(m => m.DrinkName.Contains(SearchString) || m.DrinkType.Contains(SearchString));
            }
            var result2 = result.OrderBy(m => m.DrinkSort);
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.Page = page;
            return PartialView("_DrinkDetailPartial", result2.ToPagedList(pageNumber, pageSize));
        }

        //GET: Manager/_ChangeSortAny
        public ActionResult _ChangeSortAny(int Drinkid, int sort, int menuID, int? page)
        {
            //要先有menuid, Drinkid, sortID, page
            ViewBag.DrinkID = 0;
            ViewBag.NowSort = 0;
            ViewBag.MenuID = menuID;
            if (Drinkid != 0 || sort != 0)
            {
                ViewBag.DrinkID = Drinkid;
                ViewBag.NowSort = sort;
            }
            return PartialView("_ChangeSortAny");
        }

        //GET: Manager/_ChangeSortAny
        [HttpPost]
        public ActionResult _ChangeSortAny(int DrinkID, int NowSort, int MenuID, string SortID, int? page)
        {
            //先判斷使用者不能輸入數字以外的數值或空值
            int i = 0;
            if (!(int.TryParse(SortID, out i)))
            {
                //輸入的值不合法則直接返回
                //回傳視圖
                return RedirectToAction("MyMenuDrinkDetail", "Manager", new { id = MenuID });
            }
            //輸入的是數字則進行排序
            //要判斷不得 <= 0,且輸入的最大值不可以超過最後一個
            int Sortid = Int32.Parse(SortID);
            if (Sortid <= 0)
            {
                //輸入的值不合法則直接返回
                //回傳視圖
                return RedirectToAction("MyMenuDrinkDetail", "Manager", new { id = MenuID });
            }
            var TotRcord = db.SizeTables.Where(m => m.MenuID == MenuID);
            var TotRcord2 = TotRcord.OrderByDescending(m => m.DrinkSort);
            int LastNum = 0;
            foreach (var item in TotRcord2)
            {
                LastNum = item.DrinkSort;
                break;
            }
            //輸入的最大值不可以超過最後一個
            if (Sortid > LastNum)
            {
                //輸入的值不合法則直接返回
                //回傳視圖
                return RedirectToAction("MyMenuDrinkDetail", "Manager", new { id = MenuID });
            }
            //先看是往前挪或往後挪或不動
            var myDrinkSort = db.SizeTables.Find(DrinkID);
            //不動
            if (Sortid == NowSort)
            {
                //不進行動作
            }
            //往後挪
            if (Sortid > NowSort)
            {
                //改變自己的編號並與將自己新的同編號及同編號以前的編號 -1 (自己舊編號以前的不動)
                //要先看指定編號有沒有值,如果沒有值就不用動了,只要改自己的就好
                //指定編號沒有值
                var Have = TotRcord.Where(m => m.DrinkSort == Sortid);
                if (Have.FirstOrDefault() == null)
                {
                    myDrinkSort.DrinkSort = Sortid;
                    db.SaveChanges();
                }
                //舊編號以後到指定編號更動
                if (Have.FirstOrDefault() != null)
                {
                    var ChangeThis = TotRcord.Where(m => m.DrinkSort > NowSort && m.DrinkSort <= Sortid);
                    ChangeThis = ChangeThis.OrderBy(m => m.DrinkSort);
                    foreach (var item in ChangeThis)
                    {
                        item.DrinkSort = item.DrinkSort - 1;
                    }
                    myDrinkSort.DrinkSort = Sortid;
                    db.SaveChanges();
                }
            }
            //往前挪
            if (Sortid < NowSort)
            {                
                //改變自己的編號並與將自己新的同編號及同編號以後的編號 +1 (自己舊編號以後的不動)                

                //要先看指定編號有沒有值,如果沒有值就不用動了,只要改自己的就好
                //指定編號沒有值
                var Have = TotRcord.Where(m => m.DrinkSort == Sortid);
                if (Have.FirstOrDefault() == null)
                {
                    myDrinkSort.DrinkSort = Sortid;
                    db.SaveChanges();
                }
                //指定編號到舊編號以前更動
                if (Have.FirstOrDefault() != null)
                {
                    var ChangeThis = TotRcord.Where(m => m.DrinkSort >= Sortid && m.DrinkSort < NowSort);
                    ChangeThis = ChangeThis.OrderBy(m => m.DrinkSort);
                    foreach (var item in ChangeThis)
                    {
                        item.DrinkSort = item.DrinkSort + 1;
                    }
                    myDrinkSort.DrinkSort = Sortid;
                    db.SaveChanges();
                }
            }

            //回傳視圖
            return RedirectToAction("MyMenuDrinkDetail", "Manager", new { id = MenuID });
        }

        //GET: Manager/MyDrinkEdit
        public ActionResult MyDrinkEdit(int SizePid)
        {
            SizeTable query = db.SizeTables.Find(SizePid);
            if (query == null)
            {
                return HttpNotFound();
            }

            var result = Models.ManagerModels.GetEditDrinkDetail_UseSizePID(SizePid);

            return View(result);
        }

        //POST: Manager/MyDrinkEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MyDrinkEdit(DrinkDetails EditD)
        {
            //甜度表不得為空
            var SweetCheck = Models.ManagerModels.CheckEditbox(EditD, "sweetCheck");

            //冰度表不得為空
            var IceCheck = Models.ManagerModels.CheckEditbox(EditD, "iceCheck");

            //判斷驗證及價錢
            if (!ModelState.IsValid || EditD.DrinkPrice < 0 || (SweetCheck == null || SweetCheck.Count == 0) || (IceCheck == null || IceCheck.Count == 0))
            {               
                if (EditD.DrinkPrice < 0)
                {
                    ViewBag.PriceEmpty = "項目的價格不得小於零.";
                }

                if (SweetCheck == null || SweetCheck.Count == 0)
                {
                    ViewBag.EmptyError = "甜度未正確填寫,請仔細填寫完畢再儲存.";
                }

                if (IceCheck == null || IceCheck.Count == 0)
                {
                    ViewBag.EmptyError = "冰度欄位尚未填寫,請填寫完畢再儲存.";                    
                }

                var query = Models.ManagerModels.GetEditDrinkDetail_UseSizePID(EditD.ID);

                EditD.SizeTypeM = EditD.SizeTypeM;
                EditD.ID = EditD.ID;
                EditD.SizeType = query.SizeType;
                EditD.Sweet = query.Sweet;
                EditD.IceHot = query.IceHot;
                EditD.AddItem = query.AddItem;

                return View(EditD);
            }

            //更新的異動項目
            //飲料種類.飲料名(DrinkType, DrinkName)
            //尺寸價表(Price, SizeID)
            //甜度表(SweetID 減少或新增)
            //冰度表(IceID 減少或新增)            
            //加料表(ItemPID 減少或新增)
            /////////////////////////////
            SizeTable sizeT = db.SizeTables.Find(EditD.ID);

            //飲料種類.飲料名更新
            var resultDrinkNT = db.Database.ExecuteSqlCommand(@"UPDATE menudrinks SET DrinkType = '" + EditD.DrinkType + "', DrinkName = '" + EditD.DrinkName + "' Where ID = '" + sizeT.DrinkID + "';");

            //尺寸價表更新
            int NewSize = Int32.Parse(EditD.SizeTypeM);
            var resultSizeT = db.Database.ExecuteSqlCommand(@"UPDATE sizetables SET Price = '" + EditD.DrinkPrice + "', SizeID = '" + NewSize + "' Where ID = '" + EditD.ID + "';");

            //甜度表更新/////////////////////////////////////////////////////////
            var SweetFind = db.SweetTables.Where(m => m.SizePID == EditD.ID);
            var SweetFalse = Models.ManagerModels.CheckEditbox(EditD, "sweetFalse");
            bool AreCheck = true;
            bool AreFalse = false;

            //檢查以前勾的有哪些
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
                    db.SweetTables.Add(new SweetTable()
                    {
                        SizePID = EditD.ID,
                        SweetID = newitem,                       
                    });
                }
            }
            //檢查不勾選的項目有哪些
            foreach (var itemsweetF in SweetFalse)
            {
                //檢查以前勾的有哪些
                foreach (var item in SweetFind)
                {
                    if (itemsweetF == item.SweetID)
                    {
                        //比對找到要刪除的項目
                        AreFalse = true;
                        break;
                    }
                }
                //刪除不勾選的項目
                if (AreFalse)
                {
                    var deleteS = db.SweetTables.Where(m => m.SizePID == EditD.ID && m.SweetID == itemsweetF);
                    db.SweetTables.RemoveRange(deleteS);
                }
            }

            //冰度表更新/////////////////////////////////////////////////////////////////
            var IceFind = db.IceTables.Where(m => m.SizePID == EditD.ID);
            var IceFalse = Models.ManagerModels.CheckEditbox(EditD, "iceFalse");
            //先將判斷變數歸零
            AreCheck = true;
            AreFalse = false;

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
                    db.IceTables.Add(new IceTable()
                    {
                        SizePID = EditD.ID,
                        IceID = newitem
                    });
                }
            }
            ////檢查不勾選的項目有哪些
            foreach (var itemiceF in IceFalse)
            {
                //檢查以前勾的有哪些
                foreach (var item in IceFind)
                {
                    if (itemiceF == item.IceID)
                    {
                        //比對找到要刪除的項目
                        AreFalse = true;
                        break;
                    }
                }
                //刪除不勾選的項目
                if (AreFalse)
                {
                    var deleteI = db.IceTables.Where(m => m.SizePID == EditD.ID && m.IceID == itemiceF);
                    db.IceTables.RemoveRange(deleteI);
                }
            }

            //加料表更新(寫入前要先判斷是否為空)///////////////////////////////////////////
            var AdditemFind = db.AddItemTables.Where(m => m.SizePID == EditD.ID);
            var AdditemCheck = Models.ManagerModels.CheckEditbox(EditD, "AdditemCheck");                       
            //如果都沒有勾的話就將加料表紀錄刪除(兩種情況)
            //本來就不可加料
            //從可加料變不可加料

            //先判斷是否本來就不可加料
            if (AdditemFind.FirstOrDefault() == null)
            {
                //本來就不可加料又無配料的情況
                if (AdditemCheck == null || AdditemCheck.Count == 0)
                {
                    //不更動
                }
                else
                {
                    //從無配料變有配料的情況(直接寫入)
                    var add = new List<AddItemTable>();
                    foreach (var item in AdditemCheck)
                    {
                        add.Add(new AddItemTable()
                        {
                            SizePID = EditD.ID,
                            ItemIDPriceID = item
                        });
                    }
                    db.AddItemTables.AddRange(add);
                }
            }
            else
            {
                //可加料有配料的情況
                //先將判斷變數歸零
                AreCheck = true;
                AreFalse = false;
                var AdditemFalse = Models.ManagerModels.CheckEditbox(EditD, "AdditemFalse");

                //檢查以前勾的有哪些
                foreach (var itemAdd in AdditemCheck)
                {
                    int newitem = 0;
                    //檢查新勾的有哪些
                    foreach (var item in AdditemFind)
                    {
                        if (itemAdd == item.ItemIDPriceID)
                        {
                            AreCheck = true;
                            break;
                        }
                        else
                        {
                            //比對找到新勾選的項目
                            AreCheck = false;
                            newitem = itemAdd;
                        }
                    }
                    //加入新勾選的項目
                    if (!AreCheck)
                    {
                        db.AddItemTables.Add(new AddItemTable()
                        {
                            SizePID = EditD.ID,
                            ItemIDPriceID = newitem
                        });
                    }
                }
                //檢查不勾選的項目有哪些
                foreach (var itemAddF in AdditemFalse)
                {
                    //檢查以前勾的有哪些
                    foreach (var item in AdditemFind)
                    {
                        if (itemAddF == item.ItemIDPriceID)
                        {
                            //比對找到要刪除的項目
                            AreFalse = true;
                            break;
                        }
                    }
                    //刪除不勾選的項目
                    if (AreFalse)
                    {
                        var deleteA = db.AddItemTables.Where(m => m.SizePID == EditD.ID && m.ItemIDPriceID == itemAddF);
                        db.AddItemTables.RemoveRange(deleteA);
                    }
                }
            }//End else.

            db.SaveChanges();

            //回到詳細介面
            return RedirectToAction("MyMenuDrinkDetail", "Manager", new { id = sizeT.MenuID });
        }

        //GET: Manager/MyDrinkDelete
        public ActionResult MyDrinkDelete(int SizePid)
        {
            //刪除飲料//////////
            //飲料甜度表刪除
            //飲料冰度表刪除
            //飲料加料價表刪除
            //飲料尺寸價表刪除

            SizeTable SizeT = db.SizeTables.Find(SizePid);

            //飲料甜度表
            var DeSweet = db.SweetTables.Where(m => m.SizePID == SizeT.ID);
            db.SweetTables.RemoveRange(DeSweet);

            //飲料冰度表
            var DeIce = db.IceTables.Where(m => m.SizePID == SizeT.ID);
            db.IceTables.RemoveRange(DeIce);

            //飲料加料價表
            var DeAdd = db.AddItemTables.Where(m => m.SizePID == SizeT.ID);
            db.AddItemTables.RemoveRange(DeAdd);

            //飲料尺寸價表
            db.SizeTables.Remove(SizeT);

            //刪除飲料細項訂單
            var deleteDetail = db.CreateBuyOrder_Details.Where(m => m.SizePID == SizePid);
            if (deleteDetail.FirstOrDefault() != null)
            {
                db.CreateBuyOrder_Details.RemoveRange(deleteDetail);
            }

            //刪除飲料表
            var MenuDrinkDe = db.MenuDrinks.Find(SizeT.DrinkID);
            if (MenuDrinkDe != null)
            {
                db.MenuDrinks.Remove(MenuDrinkDe);
            }

            db.SaveChanges();

            return RedirectToAction("MyMenuDrinkDetail", "Manager", new { id = SizeT.MenuID });
        }

        //GET: Manager/MyMenuDetail
        public ActionResult MyMenuDetail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Menu query = db.Menus.Find(id);
            if (query == null)
            {
                return HttpNotFound();
            }

            return View(query);
        }

        //GET: Manager/_ItemDetailPartial
        public ActionResult _ItemDetailPartial(int id)
        {
            //取出加料價表及配料名稱
            //使用AddItemCreate ViewModel
            var result = Models.ManagerModels.PickItemDetailPartial(id);
            return PartialView("_ItemDetailPartial", result);
        }

        //GET: Manager/_SweetDetailPartial
        public ActionResult _SweetDetailPartial(int id)
        {
            //取出甜度名稱
            var result = Models.ManagerModels.PickMenuSweet_UseMenuID(id);
            return PartialView("_SweetDetailPartial", result);
        }

        //GET: Manager/_IceDetailPartial
        public ActionResult _IceDetailPartial(int id)
        {
            //取出冰度名稱
            var result = Models.ManagerModels.PickMenuIce_UseMenuID(id);
            return PartialView("_IceDetailPartial", result);
        }

        //GET: Manager/_SizeDetailPartial
        public ActionResult _SizeDetailPartial(int id)
        {
            var result = Models.ManagerModels.PickMenuSize_UseMenuID(id);
            return PartialView("_SizeDetailPartial", result);
        }

        //GET: Manager/MyMenuNameEdit
        public ActionResult MyMenuNameEdit(int MenuID)
        {
            //以MenuID取出ViewModel
            var result = db.Menus.Find(MenuID);
            return View(result);
        }

        //POST: Manager/MyMenuNameEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MyMenuNameEdit(Menu EditM, HttpPostedFileBase file)
        {
            if (!ModelState.IsValid)
            {
                return View(EditM);
            }

            var result = db.Menus.Find(EditM.ID);
            DateTime time = DateTime.Now;

            //上傳圖片
            if (file != null)
            {
                if (file.ContentLength > 0 &&  file.ContentLength <= 1024 * 1024)
                {
                    //允許的圖片格式
                    var allowedExtensions = new[] { ".png", ".gif", ".jpg", ".jpeg" };
                    var extension = Path.GetExtension(file.FileName);
                    if (!allowedExtensions.Contains(extension))
                    {
                        //格式不正確
                        ViewBag.Message = "圖片格式錯誤.";
                        return View(EditM);
                    }

                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/MenuImage"), fileName);
                    file.SaveAs(path);
                    result.ImageName = fileName;
                }
                else
                {
                    //超出尺寸
                    ViewBag.Message = "圖片尺寸不能超過1024KB.";
                    return View(EditM);
                }
            }

            result.CreateTime = time;
            result.MenuPhone = EditM.MenuPhone;
            result.Addr = EditM.Addr;
            result.Open = EditM.Open;
            result.OrderConditions = EditM.OrderConditions;

            db.SaveChanges();
            return RedirectToAction("MyMenuDetail", "Manager", new { id = EditM.ID });
        }

        //GET: Manager/MyMenuAdditemEdit
        public ActionResult MyMenuAdditemEdit(int MenuID)
        {
            Menu query = db.Menus.Find(MenuID);
            if (query == null)
            {
                return HttpNotFound();
            }

            //以MenuID取出ViewModel
            var result = Models.ManagerModels.PickAddItemCreateEdit_UseMenuID(MenuID);
            return View(result);
        }

        //GET: Manager/_MyMenuAdditemNewEditEntry
        public ActionResult _MyMenuAdditemNewEditEntry(int id)
        {
            var modelView = new AddCreateEditOther()
            {
                LPrice = 0,
                MPrice = 0,
                SPrice = 0,
                deleteOrNot = false,
            };
            return PartialView("_MyMenuAdditemNewEditEntry", modelView);
        }

        //POST: Manager/MyMenuAdditemEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MyMenuAdditemEdit(AddItemCreateEdit Nitem)
        {
            //判斷價錢不得為負
            int Price = 0;
            int Price2 = 0;
            if (Nitem.AddCreateEditOther != null)
            {
                Price = Models.ManagerModels.MyMenuAdditemEdit_PriceNoSmall(Nitem.AddCreateEditOther);
            }
            if (Nitem.AddCreateNewOther != null)
            {
                Price2 = Models.ManagerModels.MyMenuAdditemEdit_PriceNoSmall(Nitem.AddCreateNewOther);
            }
            
            if (!ModelState.IsValid || Price < 0 || Price2 < 0)
            {
                if (Price < 0 || Price2 < 0)
                {
                    ViewBag.PriceSmall = "價錢不得為負數.";
                }

                return View(Nitem);
            }

            //同菜單不能建立重覆的種類價表
            if (Nitem.AddCreateNewOther != null)
            {
                //判斷是否有重複的種類
                foreach (var item in Nitem.AddCreateNewOther)
                {
                    var query = db.AddItemTypes.Where(m => m.ItemName == item.ItemName);
                    
                    if (query.FirstOrDefault() != null)
                    {
                        foreach (var queryItem in query)
                        {
                            var query2 = db.AddItemTypePrices.Where(m => m.MenuID == Nitem.MenuID && m.ItemID == queryItem.ID);
                            if (query2.FirstOrDefault() != null)
                            {
                                ViewBag.PriceSmall = "您建立了重覆的種類,請檢查後再做新增.";
                                return View(Nitem);
                            }
                        }                       
                    }
                }
            }

            if (Nitem.AddCreateEditOther != null)
            {
                //編輯以itemID更新舊資料
                //先判斷刪除     
                var AddItemDele = Models.ManagerModels.MyMenuAdditemEdit_DeleOrUpdate(Nitem.AddCreateEditOther, "DeleteItem");
                if (AddItemDele != null || AddItemDele.Count != 0)
                {
                    foreach (var item in AddItemDele)
                    {
                        //刪除種類表
                        var DeAddT = db.AddItemTypes.Find(item);
                        //刪除價錢表
                        var DeAddTP = db.AddItemTypePrices.Where(m => m.ItemID == item && m.MenuID == Nitem.MenuID);
                        foreach (var item2 in DeAddTP)
                        {
                            //刪除飲料加料價表
                            var DeAddTable = db.AddItemTables.Where(m => m.ItemIDPriceID == item2.ID);
                            db.AddItemTables.RemoveRange(DeAddTable);
                        }
                        db.AddItemTypePrices.RemoveRange(DeAddTP);
                        db.AddItemTypes.Remove(DeAddT);
                    }

                    db.SaveChanges();
                }

                //判斷更新
                var AddItemUpdate = Models.ManagerModels.MyMenuAdditemEdit_DeleOrUpdate(Nitem.AddCreateEditOther, "UpdateItem");
                if (AddItemUpdate != null || AddItemUpdate.Count != 0)
                {
                    foreach (var item in Nitem.AddCreateEditOther)
                    {
                        //更新配料名(種類表)
                        var UpAddT = db.AddItemTypes.Find(item.ItemID);
                        if (UpAddT != null)
                        {
                            UpAddT.ItemName = item.ItemName;
                        }

                        //更新價錢(價錢表)
                        var UpAddTP = db.AddItemTypePrices.Where(m => m.MenuID == Nitem.MenuID && m.ItemID == item.ItemID);
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
                    }//End Foreach.
                }//End If.
            }

            //新增(2種狀況)
            //已有的種類(判斷不得重複) 
            //新的種類:新增加料種類表,寫入新的配料價表
            if (Nitem.AddCreateNewOther != null)
            {
                foreach (var item in Nitem.AddCreateNewOther)
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
                    var AdditemPL = db.AddItemTypePrices.Add(new AddItemTypePrice() { ItemID = ItemID, SizeNumber = 0, ItemPrice = item.LPrice, MenuID = Nitem.MenuID });
                    var AdditemPM = db.AddItemTypePrices.Add(new AddItemTypePrice() { ItemID = ItemID, SizeNumber = 1, ItemPrice = item.MPrice, MenuID = Nitem.MenuID });
                    var AdditemPS = db.AddItemTypePrices.Add(new AddItemTypePrice() { ItemID = ItemID, SizeNumber = 2, ItemPrice = item.SPrice, MenuID = Nitem.MenuID });

                    db.SaveChanges();
                }
            }

            //返回檢視
            return RedirectToAction("MyMenuDetail", "Manager", new { id = Nitem.MenuID });
        }

        //GET: Manager/MyMenuSISEdit
        public ActionResult MyMenuSISEdit(int MenuID)
        {
            var result = Models.ManagerModels.GetFakeMenuSISEdit_View(MenuID);
            return View(result);
        }

        //GET: Manager/_MyMenuSisEditSweetmodel
        public ActionResult _MyMenuSisEditSweetmodel(string id, int MenuID)
        {
            string Sname = "";
            //先取出甜度名稱,讓使用者可編輯
            if (!String.IsNullOrWhiteSpace(id) && !String.IsNullOrEmpty(id))
            {
                //用MenuID.甜度ID取甜度名稱
                int ID = Int32.Parse(id);
                var Name = db.MenuSweets.Where(m => m.MenuID == MenuID && m.SweetID == ID);
                if (Name.FirstOrDefault() != null)
                {
                    foreach (var item in Name)
                    {
                        Sname = item.SweetName;
                    }
                }
                //如果MenuSweet找不到值(甜度未啟用)
                if (Name.FirstOrDefault() == null)
                {
                    var SweetType = db.SweetTypes.Find(ID);
                    Sname = SweetType.SweetName;
                }             
            }
            //返回彈窗
            ViewBag.Sname = Sname;
            ViewBag.SID = id;
            ViewBag.MenuID = MenuID;
            return PartialView();
        }

        //POST: Manager/_MyMenuSisEditSweetmodel
        [HttpPost]
        public ActionResult _MyMenuSisEditSweetmodel(string id, string sweetEdit, string MenuID)
        {
            //先判斷使用者輸入的值是否為空值,為空值則直接回傳視圖不寫入.更改
            if (String.IsNullOrEmpty(id) || String.IsNullOrEmpty(sweetEdit) || String.IsNullOrEmpty(MenuID))
            {
                int menuid2 = Int32.Parse(MenuID);
                return RedirectToAction("MyMenuSISEdit", new { MenuID = menuid2 });
            }
            //根據傳回來的甜度值及甜度id去店家甜度表改甜度
            //先看甜度是否啟用
            int ID = Int32.Parse(id);
            int menuid = Int32.Parse(MenuID);
            var Name = db.MenuSweets.Where(m => m.MenuID == menuid && m.SweetID == ID);
            //啟用.更改名字
            if (Name.FirstOrDefault() != null)
            {
                foreach (var item in Name)
                {
                    item.SweetName = sweetEdit;
                }
                db.SaveChanges();
            }
            //未啟用.寫入
            if (Name.FirstOrDefault() == null)
            {
                var SweetType = db.SweetTypes.Find(ID);
                db.MenuSweets.Add(new MenuSweet()
                {
                    MenuID = menuid,
                    SweetID = ID,
                    SweetName = sweetEdit
                });
                db.SaveChanges();
            }

            //並取出MenuID返回MyMenuSISEdit介面
            return RedirectToAction("MyMenuSISEdit", new { MenuID = menuid });
        }

        //GET: Manager/_MyMenuSisEditIcemodel
        public ActionResult _MyMenuSisEditIcemodel(string id, int MenuID)
        {
            string Iname = "";
            //先取出冰度名稱,讓使用者可編輯
            if (!String.IsNullOrWhiteSpace(id) && !String.IsNullOrEmpty(id))
            {
                //用MenuID.甜度ID取甜度名稱
                int ID = Int32.Parse(id);
                var Name = db.MenuIces.Where(m => m.MenuID == MenuID && m.IceID == ID);
                if (Name.FirstOrDefault() != null)
                {
                    foreach (var item in Name)
                    {
                        Iname = item.IceName;
                    }
                }
                //如果MenuSweet找不到值(甜度未啟用)
                if (Name.FirstOrDefault() == null)
                {
                    var IceType = db.IceTypes.Find(ID);
                    Iname = IceType.IceName;
                }
            }
            //返回彈窗
            ViewBag.Iname = Iname;
            ViewBag.IID = id;
            ViewBag.MenuID = MenuID;
            return PartialView();
        }

        //POST: Manager/_MyMenuSisEditIcemodel
        [HttpPost]
        public ActionResult _MyMenuSisEditIcemodel(string id, string iceEdit, string MenuID)
        {
            //先判斷使用者輸入的值是否為空值,為空值則直接回傳視圖不寫入.更改
            if (String.IsNullOrEmpty(id) || String.IsNullOrEmpty(iceEdit) || String.IsNullOrEmpty(MenuID))
            {
                int menuid2 = Int32.Parse(MenuID);
                return RedirectToAction("MyMenuSISEdit", new { MenuID = menuid2 });
            }
            //根據傳回來的甜度值及冰度id去店家冰度表改冰度
            //先看甜度是否啟用
            int ID = Int32.Parse(id);
            int menuid = Int32.Parse(MenuID);
            var Name = db.MenuIces.Where(m => m.MenuID == menuid && m.IceID == ID);
            //啟用.更改名字
            if (Name.FirstOrDefault() != null)
            {
                foreach (var item in Name)
                {
                    item.IceName = iceEdit;
                }
                db.SaveChanges();
            }
            //未啟用.寫入
            if (Name.FirstOrDefault() == null)
            {
                var IceType = db.IceTypes.Find(ID);
                db.MenuIces.Add(new MenuIce()
                {
                    MenuID = menuid,
                    IceID = ID,
                    IceName = iceEdit
                });
                db.SaveChanges();
            }
            //並取出MenuID返回MyMenuSISEdit介面
            return RedirectToAction("MyMenuSISEdit", new { MenuID = menuid });
        }

        //GET: Manager/_MyMenuSisEditSizemodel
        public ActionResult _MyMenuSisEditSizemodel(string id, int MenuID)
        {
            string Sname = "";
            //先取出甜度名稱,讓使用者可編輯
            if (!String.IsNullOrWhiteSpace(id) && !String.IsNullOrEmpty(id))
            {
                //用MenuID.甜度ID取甜度名稱
                int ID = Int32.Parse(id);
                var Name = db.MenuSizes.Where(m => m.MenuID == MenuID && m.SizeID == ID);
                if (Name.FirstOrDefault() != null)
                {
                    foreach (var item in Name)
                    {
                        Sname = item.SizeName;
                    }
                }
                //如果MenuSweet找不到值(甜度未啟用)
                if (Name.FirstOrDefault() == null)
                {
                    var SizeType = db.SizeTypes.Find(ID);
                    Sname = SizeType.SizeName;
                }
            }
            //返回彈窗
            ViewBag.Sname = Sname;
            ViewBag.SID = id;
            ViewBag.MenuID = MenuID;
            return PartialView();
        }

        //POST: Manager/_MyMenuSisEditSizemodel
        [HttpPost]
        public ActionResult _MyMenuSisEditSizemodel(string id, string sizeEdit, string MenuID)
        {
            //先判斷使用者輸入的值是否為空值,為空值則直接回傳視圖不寫入.更改
            if (String.IsNullOrEmpty(id) || String.IsNullOrEmpty(sizeEdit) || String.IsNullOrEmpty(MenuID))
            {
                int menuid2 = Int32.Parse(MenuID);
                return RedirectToAction("MyMenuSISEdit", new { MenuID = menuid2 });
            }
            //根據傳回來的甜度值及冰度id去店家冰度表改冰度
            //先看甜度是否啟用
            int ID = Int32.Parse(id);
            int menuid = Int32.Parse(MenuID);
            var Name = db.MenuSizes.Where(m => m.MenuID == menuid && m.SizeID == ID);
            //啟用.更改名字
            if (Name.FirstOrDefault() != null)
            {
                foreach (var item in Name)
                {
                    item.SizeName = sizeEdit;
                }
                db.SaveChanges();
            }
            //未啟用.寫入
            if (Name.FirstOrDefault() == null)
            {
                var SizeType = db.SizeTypes.Find(ID);
                db.MenuSizes.Add(new MenuSize()
                {
                    MenuID = menuid,
                    SizeID = ID,
                    SizeName = sizeEdit
                });
                db.SaveChanges();
            }
            //並取出MenuID返回MyMenuSISEdit介面
            return RedirectToAction("MyMenuSISEdit", new { MenuID = menuid });
        }

        //POST: Manager/MyMenuSISEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MyMenuSISEdit(MenuSISEdit Nsis)
        {
            if (!ModelState.IsValid)
            {
                var result = Models.ManagerModels.ErrorBackMenu_MenuSIS(Nsis);
                return View(result);
            }

            //分別判斷甜度,冰度,大小勾選及動態欄其一必填
            //先判斷是否有動態欄位,若無則檢查是否有勾選
            //判斷甜度.兩個都沒有選的情況
            var SweetCheck = Models.ManagerModels.CheckCheckboxNotEmpty_MenuSIS(Nsis, "sweet");
            if ((Nsis.SweetName == null || Nsis.SweetName.Count == 0) && (SweetCheck == null || SweetCheck.Count == 0))
            {
                ViewBag.EmptyError = "甜度欄位尚未填寫,請填寫完畢再儲存.";
                var result = Models.ManagerModels.ErrorBackMenu_MenuSIS(Nsis);
                return View(result);
            }
            //判斷冰度.兩個都沒有選的情況
            var IceCheck = Models.ManagerModels.CheckCheckboxNotEmpty_MenuSIS(Nsis, "ice");
            if ((Nsis.IceName == null || Nsis.IceName.Count == 0) && (IceCheck == null || IceCheck.Count == 0))
            {
                ViewBag.EmptyError = "冰度欄位尚未填寫,請填寫完畢再儲存.";
                var result = Models.ManagerModels.ErrorBackMenu_MenuSIS(Nsis);
                return View(result);
            }
            //判斷尺寸.兩個都沒有選的情況
            var SizeCheck = Models.ManagerModels.CheckCheckboxNotEmpty_MenuSIS(Nsis, "size");
            if ((Nsis.SizeName == null || Nsis.SizeName.Count == 0) && (SizeCheck == null || SizeCheck.Count == 0))
            {
                ViewBag.EmptyError = "尺寸欄位尚未填寫,請填寫完畢再儲存.";
                var result = Models.ManagerModels.ErrorBackMenu_MenuSIS(Nsis);
                return View(result);
            }

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
                        var NameS = db.SweetTypes.Find(newitem);
                        db.MenuSweets.Add(new MenuSweet()
                        {
                            MenuID = Nsis.MenuID,
                            SweetID = newitem,
                            SweetName = NameS.SweetName
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
                    if (NoRepeat.FirstOrDefault() == null)
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
                    int SweetID = Models.ManagerModels.SweetID(item.SweetName, "sweet");
                    //如果店家已有此甜度則不新增
                    var HaveThis = db.MenuSweets.Where(m => m.SweetID == SweetID);

                    if (HaveThis.FirstOrDefault() == null)
                    {
                        var NameS = db.SweetTypes.Find(SweetID);
                        NewMenuSweet.Add(new MenuSweet()
                        {
                            MenuID = Nsis.MenuID,
                            SweetID = SweetID,
                            SweetName = NameS.SweetName
                        });
                    }
                }
                if (NewMenuSweet != null && NewMenuSweet.Count() != 0)
                {
                    db.MenuSweets.AddRange(NewMenuSweet);
                }
            }
            //檢查未勾選的,從店家冰度表及飲料冰度表刪除////////////////////////////////////////
            var IceFind = db.MenuIces.Where(m => m.MenuID == Nsis.MenuID);
            var NoIce = Models.ManagerModels.CheckCheckboxNotEmpty_MenuSIS(Nsis, "iceNo");
            AreCheck = true;
            AreFalse = false;
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
                        var NameS = db.IceTypes.Find(newitem);
                        db.MenuIces.Add(new MenuIce()
                        {
                            MenuID = Nsis.MenuID,
                            IceID = newitem,
                            IceName = NameS.IceName
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
                    if (NoRepeat.FirstOrDefault() == null)
                    {
                        NewIceType.Add(new IceType()
                        {
                            IceName = item.IceName
                        });
                    }
                }
                if (NewIceType != null || NewIceType.Count != 0)
                {
                    db.IceTypes.AddRange(NewIceType);
                    db.SaveChanges();
                }
                //取出冰度ID,寫入店家冰度表
                var NewMenuIce = new List<MenuIce>();
                foreach (var item in Nsis.IceName)
                {
                    int IceID = Models.ManagerModels.SweetID(item.IceName, "ice");
                    //如果店家已有此甜度則不新增
                    var HaveThis = db.MenuIces.Where(m => m.IceID == IceID);
                    if (HaveThis.FirstOrDefault() == null)
                    {
                        var NameS = db.IceTypes.Find(IceID);
                        NewMenuIce.Add(new MenuIce()
                        {
                            MenuID = Nsis.MenuID,
                            IceID = IceID,
                            IceName = NameS.IceName
                        });
                    }                   
                }
                if (NewMenuIce != null && NewMenuIce.Count() != 0)
                {
                    db.MenuIces.AddRange(NewMenuIce);
                }
            }
            
            //檢查未勾選的,從店家尺寸表及飲料尺寸價表刪除////////////////////////////////////////
            var SizeFind = db.MenuSizes.Where(m => m.MenuID == Nsis.MenuID);
            var NoSize = Models.ManagerModels.CheckCheckboxNotEmpty_MenuSIS(Nsis, "sizeNo");
            AreCheck = true;
            AreFalse = false;
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
                        var NameS = db.SizeTypes.Find(newitem);
                        db.MenuSizes.Add(new MenuSize()
                        {
                            MenuID = Nsis.MenuID,
                            SizeID = newitem,
                            SizeName = NameS.SizeName
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
                    if (NoRepeat.FirstOrDefault() == null)
                    {
                        NewSizeType.Add(new SizeType()
                        {
                            SizeName = item.SizeName
                        });
                    }
                }
                if (NewSizeType != null || NewSizeType.Count != 0)
                {
                    db.SizeTypes.AddRange(NewSizeType);
                    db.SaveChanges();
                }
                //取出尺寸ID,寫入店家尺寸表
                var NewMenuSize = new List<MenuSize>();
                foreach (var item in Nsis.SizeName)
                {
                    int SizeID = Models.ManagerModels.SweetID(item.SizeName, "size");
                    //如果店家已有此尺寸則不新增
                    var HaveThis = db.MenuSizes.Where(m => m.SizeID == SizeID);
                    if (HaveThis.FirstOrDefault() == null)
                    {
                        var NameS = db.SizeTypes.Find(SizeID);
                        NewMenuSize.Add(new MenuSize()
                        {
                            MenuID = Nsis.MenuID,
                            SizeID = SizeID,
                            SizeName = NameS.SizeName
                        });
                    }                  
                }
                if (NewMenuSize != null && NewMenuSize.Count() != 0)
                {
                    db.MenuSizes.AddRange(NewMenuSize);
                }
            }

            db.SaveChanges();

            //返回檢視
            return RedirectToAction("MyMenuDetail", "Manager", new { id = Nsis.MenuID });
        }

        //GET: Manager/Add_NewMenu
        public ActionResult Add_NewMenu()
        {
            var result = Models.ManagerModels.GetFakeMenus();
            return View(result);
        }

        //POST: Manager/Add_NewMenu
        [HttpPost]
        public ActionResult Add_NewMenu(NewMenu newM, HttpPostedFileBase file)
        {
            if (!ModelState.IsValid)
            {                
                var Menu = Models.ManagerModels.ErrorBackMenu(newM);
                return View(Menu);
            }

            //先寫入主Menu,取ID(要判斷Menu名不重複)
            var query = db.Menus.Where(m => m.MenuName == newM.MenuName);
            if (query.FirstOrDefault() != null)
            {
                ViewBag.EmptyError = "您已有相同名稱的菜單,請勿重覆建立.";
                var Menu = Models.ManagerModels.ErrorBackMenu(newM);
                return View(Menu);
            }

            //上傳圖片
            string noImage = "19.jpg";
            if (file != null)
            {
                if (file.ContentLength > 0 && file.ContentLength <= 1024 * 1024)
                {
                    //允許的圖片格式
                    var allowedExtensions = new[] { ".png", ".gif", ".jpg", ".jpeg" };
                    var extension = Path.GetExtension(file.FileName);
                    if (!allowedExtensions.Contains(extension))
                    {
                        //格式不正確
                        ViewBag.Message = "圖片格式錯誤.";
                        var Menu = Models.ManagerModels.ErrorBackMenu(newM);
                        return View(Menu);
                    }

                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/MenuImage"), fileName);
                    file.SaveAs(path);
                    noImage = fileName;
                }
                else
                {
                    //超出尺寸
                    ViewBag.Message = "圖片尺寸不能超過1024KB.";
                    var Menu = Models.ManagerModels.ErrorBackMenu(newM);
                    return View(Menu);
                }
            }

            if (String.IsNullOrEmpty(newM.OrderConditions))
            {
                newM.OrderConditions = "無";
            }

            DateTime time = DateTime.Now;
            var MainMenu = new Menu()
            {
                CreateTime = time,
                MenuName = newM.MenuName,
                MenuPhone = newM.MenuPhone,
                Addr = newM.Addr,
                Open = false,
                ImageName = noImage,
                OrderConditions = newM.OrderConditions
            };
            db.Menus.Add(MainMenu);
            db.SaveChanges();
            int MenuID = Models.ManagerModels.MenuID(newM.MenuName);

            //甜度.冰度.尺寸給預設值//////////////////////////////////////////////////////////////////////////////////////////////
            //填入店家甜度表(預設: 1.2.3.4)
            var NewMenuSweet = new List<MenuSweet>();
            for (int i = 1; i <= 4; i++)
            {
                NewMenuSweet.Add(new MenuSweet()
                {
                    MenuID = MenuID,
                    SweetID = i
                });
            }
            db.MenuSweets.AddRange(NewMenuSweet);

            //填入店家冰度表(預設: 1.2.3.4.5)
            var NewMenuIce = new List<MenuIce>();
            for (int i = 1; i <= 5; i++)
            {
                NewMenuIce.Add(new MenuIce()
                {
                    MenuID = MenuID,
                    IceID = i
                });
            }
            db.MenuIces.AddRange(NewMenuIce);

            //填入店家尺寸表(預設: 1.2)
            var NewMenuSize = new List<MenuSize>();
            for (int i = 1; i <= 2; i++)
            {
                NewMenuSize.Add(new MenuSize()
                {
                    MenuID = MenuID,
                    SizeID = i
                });
            }
            db.MenuSizes.AddRange(NewMenuSize);

            //存入資料庫
            db.SaveChanges();

            //依照menuID進到新增飲料介面
            return RedirectToAction("Add_NewDrinks", "Manager", new { id = MenuID });
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
            Add2_DrinksView result = new Add2_DrinksView() {
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
        public ActionResult _Add2ButtonMenu(int MenuID)
        {
            //要先取出菜單的設定
            var result = Models.Add2_NewMenuAbout.Add2_MEnuSisEntryR(MenuID);
            return PartialView("_Add2ButtonMenu", result);
        }

        //POST: Manager/_Add2ButtonMenu
        [HttpPost]
        public ActionResult _Add2ButtonMenu2(Add2_MEnuSis Type)
        {
            var result = Models.Add2_NewMenuAbout.Add2_MEnuSisEntryR(22);
            return PartialView("_Add2ButtonMenu", result);
        }




        //GET: Manager/Add_NewDrinks
        public ActionResult Add_NewDrinks(int id)
        {
            Menu query = db.Menus.Find(id);
            if (query == null)
            {
                return HttpNotFound();
            }
            //用菜單ID取出菜單名及菜單ViewModel
            AddDrinkDetails result = Models.ManagerModels.CurrentAddDrinkDetails(id);

            return View(result);
        }

        //POST: Manager/AddDrinks
        [HttpPost]
        public ActionResult Add_NewDrinks(AddDrinkDetails addDks)
        {
            //判斷新增飲料欄位不得刪除全部,導致此欄位null
            if (addDks.DrinkDetails == null || addDks.DrinkDetails.Count == 0)
            {
                ViewBag.EmptyError = "您沒有新增任何飲品.";
                return View(addDks);
            }
            //飲品金額則以不為負數為基準
            int Price = Models.ManagerModels.DrinkPriceIsNotSmall(addDks);
            if (!ModelState.IsValid || Price < 0)
            {
                if (Price < 0)
                {
                    ViewBag.PriceSmall = "價錢不得為負值.";
                }
                var result = Models.ManagerModels.BackAddDrinkDetails(addDks);
                return View(addDks);
            }

            //判斷勾選甜度.冰度.欄位必須勾選一項(由於是多項,所以必須一樣樣判斷)
            //加料由於有可加與不可加所以就不判斷
            foreach (var item in addDks.DrinkDetails)
            {
                var CheckSweet = Models.ManagerModels.AddDrink_CheckboxNotEmpty(item, "sweet");
                var CheckIce = Models.ManagerModels.AddDrink_CheckboxNotEmpty(item, "ice");
                if ((CheckSweet == null || CheckSweet.Count == 0) || (CheckIce == null || CheckIce.Count == 0))
                {
                    ViewBag.EmptyError = "請將欄位填寫完全,甜度及冰度欄位請至少勾選一項.";
                    var result = Models.ManagerModels.BackAddDrinkDetails(addDks);
                    return View(addDks);
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
            foreach (var item in addDks.DrinkDetails)
            {
                var query = db.MenuDrinks.Where(m => m.MenuID == addDks.MenuID && m.DrinkType == item.DrinkType && m.DrinkName == item.DrinkName);
                if (query.FirstOrDefault() != null)
                {
                    continue;
                }
                else
                {
                    var NewDrinks = new MenuDrink()
                    {
                        MenuID = addDks.MenuID,
                        DrinkType = item.DrinkType,
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
            foreach (var item in addDks.DrinkDetails)
            {
                int DrinkID = Models.ManagerModels.PickDrinkID_UseModel(item, addDks.MenuID);
                //飲料尺寸價表
                int SizeID = Int32.Parse(item.SizeTypeM);
                NewSizeP.Add(new SizeTable()
                {
                    DrinkID = DrinkID,
                    SizeID = SizeID,
                    Price = item.DrinkPrice,
                    MenuID = addDks.MenuID
                });
            }
            db.SizeTables.AddRange(NewSizeP);
            db.SaveChanges();

            //依照DrinkID,再用DrinkID.SizeID.Price取SizePID//////////////////////////
            var NewSweetTable = new List<SweetTable>();
            var NewIceTable = new List<IceTable>();
            var NewAddItemTable = new List<AddItemTable>();
            foreach (var item in addDks.DrinkDetails)
            {
                int DrinkID = Models.ManagerModels.PickDrinkID_UseModel(item, addDks.MenuID);
                int SizePID = Models.ManagerModels.PickSizePID_UseDidSidPrice(item, DrinkID);
                var CheckSweet = Models.ManagerModels.AddDrink_CheckboxNotEmpty(item, "sweet");
                var CheckIce = Models.ManagerModels.AddDrink_CheckboxNotEmpty(item, "ice");
                var CheckAddItem = Models.ManagerModels.AddDrink_CheckboxNotEmpty(item, "additem");

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
            }
            db.SweetTables.AddRange(NewSweetTable);
            db.IceTables.AddRange(NewIceTable);
            if (NewAddItemTable != null || NewAddItemTable.Count != 0)
            {
                db.AddItemTables.AddRange(NewAddItemTable);
            }
            db.SaveChanges();

            //寫完依menuID回到飲料詳細介面
            return RedirectToAction("MyMenuDrinkDetail", "Manager", new { id = addDks.MenuID });
        }







        //GET: Manager/Add_NewAdditem
        public ActionResult Add_NewAdditem(int MenuID)
        {
            Menu query = db.Menus.Find(MenuID);
            if (query == null)
            {
                return HttpNotFound();
            }

            //以MenuID取出ViewModel
            var result = Models.ManagerModels.PickAddItemCreateEdit_UseMenuID(MenuID);
            return View(result);
        }

        //POST: Manager/MyMenuAdditemEdit
        [HttpPost]
        public ActionResult Add_NewAdditem(AddItemCreateEdit Nitem)
        {
            //判斷價錢不得為負
            int Price = 0;
            int Price2 = 0;
            if (Nitem.AddCreateEditOther != null)
            {
                Price = Models.ManagerModels.MyMenuAdditemEdit_PriceNoSmall(Nitem.AddCreateEditOther);
            }
            if (Nitem.AddCreateNewOther != null)
            {
                Price2 = Models.ManagerModels.MyMenuAdditemEdit_PriceNoSmall(Nitem.AddCreateNewOther);
            }

            if (!ModelState.IsValid || Price < 0 || Price2 < 0)
            {
                if (Price < 0 || Price2 < 0)
                {
                    ViewBag.PriceSmall = "價錢不得為負數.";
                }

                return View(Nitem);
            }

            //同菜單不能建立重覆的種類價表
            if (Nitem.AddCreateNewOther != null)
            {
                //判斷是否有重複的種類
                foreach (var item in Nitem.AddCreateNewOther)
                {
                    var query = db.AddItemTypes.Where(m => m.ItemName == item.ItemName);

                    if (query.FirstOrDefault() != null)
                    {
                        foreach (var queryItem in query)
                        {
                            var query2 = db.AddItemTypePrices.Where(m => m.MenuID == Nitem.MenuID && m.ItemID == queryItem.ID);
                            if (query2.FirstOrDefault() != null)
                            {
                                ViewBag.PriceSmall = "您建立了重覆的種類,請檢查後再做新增.";
                                return View(Nitem);
                            }
                        }
                    }
                }
            }

            if (Nitem.AddCreateEditOther != null)
            {
                //編輯以itemID更新舊資料
                //先判斷刪除     
                var AddItemDele = Models.ManagerModels.MyMenuAdditemEdit_DeleOrUpdate(Nitem.AddCreateEditOther, "DeleteItem");
                if (AddItemDele != null || AddItemDele.Count != 0)
                {
                    foreach (var item in AddItemDele)
                    {
                        //刪除種類表
                        var DeAddT = db.AddItemTypes.Find(item);
                        //刪除價錢表
                        var DeAddTP = db.AddItemTypePrices.Where(m => m.ItemID == item && m.MenuID == Nitem.MenuID);
                        foreach (var item2 in DeAddTP)
                        {
                            //刪除飲料加料價表
                            var DeAddTable = db.AddItemTables.Where(m => m.ItemIDPriceID == item2.ID);
                            db.AddItemTables.RemoveRange(DeAddTable);
                        }
                        db.AddItemTypePrices.RemoveRange(DeAddTP);
                        db.AddItemTypes.Remove(DeAddT);
                    }

                    db.SaveChanges();
                }

                //判斷更新
                var AddItemUpdate = Models.ManagerModels.MyMenuAdditemEdit_DeleOrUpdate(Nitem.AddCreateEditOther, "UpdateItem");
                if (AddItemUpdate != null || AddItemUpdate.Count != 0)
                {
                    foreach (var item in Nitem.AddCreateEditOther)
                    {
                        //更新配料名(種類表)
                        var UpAddT = db.AddItemTypes.Find(item.ItemID);
                        if (UpAddT != null)
                        {
                            UpAddT.ItemName = item.ItemName;
                        }

                        //更新價錢(價錢表)
                        var UpAddTP = db.AddItemTypePrices.Where(m => m.MenuID == Nitem.MenuID && m.ItemID == item.ItemID);
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
                    }//End Foreach.
                }//End If.
            }

            //新增(2種狀況)
            //已有的種類(判斷不得重複) 
            //新的種類:新增加料種類表,寫入新的配料價表
            if (Nitem.AddCreateNewOther != null)
            {
                foreach (var item in Nitem.AddCreateNewOther)
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
                    var AdditemPL = db.AddItemTypePrices.Add(new AddItemTypePrice() { ItemID = ItemID, SizeNumber = 0, ItemPrice = item.LPrice, MenuID = Nitem.MenuID });
                    var AdditemPM = db.AddItemTypePrices.Add(new AddItemTypePrice() { ItemID = ItemID, SizeNumber = 1, ItemPrice = item.MPrice, MenuID = Nitem.MenuID });
                    var AdditemPS = db.AddItemTypePrices.Add(new AddItemTypePrice() { ItemID = ItemID, SizeNumber = 2, ItemPrice = item.SPrice, MenuID = Nitem.MenuID });

                    db.SaveChanges();
                }
            }

            //返回檢視
            return RedirectToAction("Add_NewDrinks", "Manager", new { id = Nitem.MenuID });
        }

        //GET: Manager/Add_NewSweet
        public ActionResult Add_NewSweet(int MenuID)
        {
            //var result = Models.ManagerModels.GetFakeMenuSISEdit_View(MenuID);
            var sweet = Models.ManagerModels.MenuSIS_GetSweet(MenuID);
            var result = new MenuAdd_NewSweet()
            {
                MenuID = MenuID,
                Sweet = sweet
            };
            return View(result);
        }

        //POST: Manager/Add_NewSweet
        [HttpPost]
        public ActionResult Add_NewSweet(MenuAdd_NewSweet Nsis)
        {
            if (!ModelState.IsValid)
            {
                Nsis.Sweet = Models.ManagerModels.MenuSIS_GetSweet(Nsis.MenuID);
                return View(Nsis);
            }

            //先判斷是否有動態欄位,若無則檢查是否有勾選
            //判斷甜度.兩個都沒有選的情況
            var SweetCheck = Models.ManagerModels.CheckCheckboxNotEmpty_MenuSISsweet(Nsis, "sweet");
            if ((Nsis.SweetName == null || Nsis.SweetName.Count == 0) && (SweetCheck == null || SweetCheck.Count == 0))
            {
                ViewBag.EmptyError = "甜度欄位尚未填寫,請填寫完畢再儲存.";
                Nsis.Sweet = Models.ManagerModels.MenuSIS_GetSweet(Nsis.MenuID);
                return View(Nsis);
            }

            //判斷甜度.只填一個或都填的情況/////////////////////////////////////  
            //檢查未勾選的,從店家甜度表及飲料甜度表刪除
            var SweetFind = db.MenuSweets.Where(m => m.MenuID == Nsis.MenuID);
            var NoSweet = Models.ManagerModels.CheckCheckboxNotEmpty_MenuSISsweet(Nsis, "sweetNo");
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
                    if (NoRepeat.FirstOrDefault() == null)
                    {
                        NewSweetType.Add(new SweetType()
                        {
                            SweetName = item.SweetName
                        });
                    }
                }
                if (NewSweetType != null || NewSweetType.Count != 0)
                {
                    db.SweetTypes.AddRange(NewSweetType);
                    db.SaveChanges();
                }
                //取出甜度ID,寫入店家甜度表
                var NewMenuSweet = new List<MenuSweet>();
                foreach (var item in Nsis.SweetName)
                {
                    int SweetID = Models.ManagerModels.SweetID(item.SweetName, "sweet");
                    NewMenuSweet.Add(new MenuSweet()
                    {
                        MenuID = Nsis.MenuID,
                        SweetID = SweetID
                    });
                }
                db.MenuSweets.AddRange(NewMenuSweet);
            }

            //返回檢視
            db.SaveChanges();
            return RedirectToAction("Add_NewDrinks", "Manager", new { id = Nsis.MenuID });

        }

        //GET: Manager/Add_NewIce
        public ActionResult Add_NewIce(int MenuID)
        {           
            var Ice = Models.ManagerModels.MenuSIS_GetIce(MenuID);
            var result = new MenuAdd_NewIce()
            {
                MenuID = MenuID,
                IceHot = Ice
            };

            return View(result);
        }

        //POST: Manager/Add_NewIce
        [HttpPost]
        public ActionResult Add_NewIce(MenuAdd_NewIce Nsis)
        {
            if (!ModelState.IsValid)
            {
                Nsis.IceHot = Models.ManagerModels.MenuSIS_GetIce(Nsis.MenuID);
                return View(Nsis);
            }
            //判斷冰度.兩個都沒有選的情況
            var IceCheck = Models.ManagerModels.CheckCheckboxNotEmpty_MenuSIIce(Nsis, "ice");
            if ((Nsis.IceName == null || Nsis.IceName.Count == 0) && (IceCheck == null || IceCheck.Count == 0))
            {
                ViewBag.EmptyError = "冰度欄位尚未填寫,請填寫完畢再儲存.";
                Nsis.IceHot = Models.ManagerModels.MenuSIS_GetIce(Nsis.MenuID);
                return View(Nsis);
            }

            //檢查未勾選的,從店家冰度表及飲料冰度表刪除////////////////////////////////////////
            var IceFind = db.MenuIces.Where(m => m.MenuID == Nsis.MenuID);
            var NoIce = Models.ManagerModels.CheckCheckboxNotEmpty_MenuSIIce(Nsis, "iceNo");
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
                    if (NoRepeat.FirstOrDefault() == null)
                    {
                        NewIceType.Add(new IceType()
                        {
                            IceName = item.IceName
                        });
                    }
                }
                if (NewIceType != null || NewIceType.Count != 0)
                {
                    db.IceTypes.AddRange(NewIceType);
                    db.SaveChanges();
                }
                //取出冰度ID,寫入店家冰度表
                var NewMenuIce = new List<MenuIce>();
                foreach (var item in Nsis.IceName)
                {
                    int IceID = Models.ManagerModels.SweetID(item.IceName, "ice");
                    NewMenuIce.Add(new MenuIce()
                    {
                        MenuID = Nsis.MenuID,
                        IceID = IceID
                    });
                }
                db.MenuIces.AddRange(NewMenuIce);
            }

            //返回檢視
            db.SaveChanges();
            return RedirectToAction("Add_NewDrinks", "Manager", new { id = Nsis.MenuID });
        }

        //GET: Manager/Add_NewSize
        public ActionResult Add_NewSize(int MenuID)
        {
            var size = Models.ManagerModels.MenuSIS_GetSize(MenuID);
            var result = new MenuAdd_NewSize()
            {
                MenuID = MenuID,
                Size = size
            };

            return View(result);
        }

        //POST: Manager/Add_NewSize
        [HttpPost]
        public ActionResult Add_NewSize(MenuAdd_NewSize Nsis)
        {
            if (!ModelState.IsValid)
            {
                Nsis.Size = Models.ManagerModels.MenuSIS_GetSize(Nsis.MenuID);
                return View(Nsis);
            }

            //判斷尺寸.兩個都沒有選的情況
            var SizeCheck = Models.ManagerModels.CheckCheckboxNotEmpty_MenuSISize(Nsis, "size");
            if ((Nsis.SizeName == null || Nsis.SizeName.Count == 0) && (SizeCheck == null || SizeCheck.Count == 0))
            {
                ViewBag.EmptyError = "尺寸欄位尚未填寫,請填寫完畢再儲存.";
                Nsis.Size = Models.ManagerModels.MenuSIS_GetSize(Nsis.MenuID);
                return View(Nsis);
            }

            //檢查未勾選的,從店家尺寸表及飲料尺寸價表刪除////////////////////////////////////////
            var SizeFind = db.MenuSizes.Where(m => m.MenuID == Nsis.MenuID);
            var NoSize = Models.ManagerModels.CheckCheckboxNotEmpty_MenuSISize(Nsis, "sizeNo");
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
                    if (NoRepeat.FirstOrDefault() == null)
                    {
                        NewSizeType.Add(new SizeType()
                        {
                            SizeName = item.SizeName
                        });
                    }
                }
                if (NewSizeType != null || NewSizeType.Count != 0)
                {
                    db.SizeTypes.AddRange(NewSizeType);
                    db.SaveChanges();
                }
                //取出尺寸ID,寫入店家尺寸表
                var NewMenuSize = new List<MenuSize>();
                foreach (var item in Nsis.SizeName)
                {
                    int SizeID = Models.ManagerModels.SweetID(item.SizeName, "size");
                    NewMenuSize.Add(new MenuSize()
                    {
                        MenuID = Nsis.MenuID,
                        SizeID = SizeID
                    });
                }
                db.MenuSizes.AddRange(NewMenuSize);
            }

            db.SaveChanges();
            //返回檢視
            return RedirectToAction("Add_NewDrinks", "Manager", new { id = Nsis.MenuID });
        }

        //GET: Manager/Add_IndexImeage
        public ActionResult Add_IndexImeage()
        {
            var result = db.IndexImages.Find(1);
            return View(result);
        }

        //POST: Manager/Add_IndexImeage
        [HttpPost]
        public ActionResult Add_IndexImeage(HttpPostedFileBase file, HttpPostedFileBase file2, HttpPostedFileBase file3)
        {
            var result = db.IndexImages.Find(1);
            ViewBag.Message = "";

            if (file != null)
            {
                if (file.ContentLength > 0 && file.ContentLength <= 1024 * 1024)
                {
                    //允許的圖片格式
                    var allowedExtensions = new[] { ".png", ".gif", ".jpg", ".jpeg" };
                    var extension = Path.GetExtension(file.FileName);
                    if (!allowedExtensions.Contains(extension))
                    {
                        //格式不正確
                        ViewBag.Message = "圖片格式錯誤.";
                        return View(result);
                    }

                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/IndexImage"), fileName);
                    file.SaveAs(path);
                    result.Index01 = fileName;
                }
                else
                {
                    //超出尺寸
                    ViewBag.Message = "圖片1尺寸不能超過1024KB.";
                    return View(result);
                }
            }

            if (file2 != null)
            {
                if (file2.ContentLength > 0 && file2.ContentLength <= 1024 * 1024)
                {
                    //允許的圖片格式
                    var allowedExtensions = new[] { ".png", ".gif", ".jpg", ".jpeg" };
                    var extension = Path.GetExtension(file2.FileName);
                    if (!allowedExtensions.Contains(extension))
                    {
                        //格式不正確
                        ViewBag.Message = "圖片格式錯誤.";
                        return View(result);
                    }

                    var fileName = Path.GetFileName(file2.FileName);
                    var path = Path.Combine(Server.MapPath("~/IndexImage"), fileName);
                    file2.SaveAs(path);
                    result.Index02 = fileName;
                }
                else
                {
                    //超出尺寸
                    ViewBag.Message = "圖片2尺寸不能超過1024KB.";
                    return View(result);
                }
            }

            if (file3 != null)
            {
                if (file3.ContentLength > 0 && file3.ContentLength <= 1024 * 1024)
                {
                    //允許的圖片格式
                    var allowedExtensions = new[] { ".png", ".gif", ".jpg", ".jpeg" };
                    var extension = Path.GetExtension(file3.FileName);
                    if (!allowedExtensions.Contains(extension))
                    {
                        //格式不正確
                        ViewBag.Message = "圖片3格式錯誤.";
                        return View(result);
                    }

                    var fileName = Path.GetFileName(file3.FileName);
                    var path = Path.Combine(Server.MapPath("~/IndexImage"), fileName);
                    file3.SaveAs(path);
                    result.Index03 = fileName;
                }
                else
                {
                    //超出尺寸
                    ViewBag.Message = "圖片3尺寸不能超過1024KB.";
                    return View(result);
                }
            }

            db.SaveChanges();
            TempData["message2"] = "success";
            return RedirectToAction("ManagerHome", "Home");
        }





        //關閉連線
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}