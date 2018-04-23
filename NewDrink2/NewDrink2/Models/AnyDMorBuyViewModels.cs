using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NewDrink2.Models
{
    //選購菜單檢視View
    public class DMDetailsView
    {
        //用到三張表
        //Menus(菜單總表)
        //MenuDrink(飲料類名表)
        //SizeTable(飲料尺寸價表)
        public int MenuID { get; set; }

        public string MenuName { get; set; }

        //店家電話
        public string MenuPhon { get; set; }

        //店家外送條件
        public string OrderCondition { get; set; }

        public IList<DMDrinksDetailView> DMDrinksDetailView { get; set; }
    }

    //飲料尺寸價表檢視
    public class DMDrinksDetailView
    {
        public int DrinkID { get; set; }

        public string DrinkType { get; set; }

        public string DrinkName { get; set; }

        //備註
        public string Bathus { get; set; }

        public IList<DMDrinksPriceSize> DMDrinksPriceSize { get; set; }
    }

    //飲料尺寸價表檢視
    public class DMDrinksPriceSize
    {
        public string DrinkSize { get; set; }

        public int DrinkPrice { get; set; }

        //尺寸簡稱
        public string SimpleName { get; set; }

        //判斷冷熱
        public int HotOrIce { get; set; }
    }

    //飲料選購細項檢視
    public class BuyDMDrinkView
    {
        public int MenuID { get; set; }

        public int DrinkID { get; set; }

        public string DrinkName { get; set; }

        //備註
        public string Bathus { get; set; }

        //尺寸(chexkbox)
        [Required]
        [Display(Name = "大小")]
        public string SizeTypeM { get; set; }

        public IList<SelectListItem> SizeType { get; set; }

        //甜度(下拉選單)
        public IList<BuyDMDrinkSweet> SweetTypeM { get; set; }

        //冰度(下拉選單)
        public IList<BuyDMDrinkIce> IceTypeM { get; set; }

        //加料名 + 加料價(chexkbox)
        public IList<BuyDMDrinkAdditem> AddItemType { get; set; }

        //數量
        [DataType(DataType.PhoneNumber, ErrorMessage ="輸入的不是數字.")]
        [Display(Name = "數量")]
        [RegularExpression(@"^\d+$")]
        [Required]
        [Range(1, 99999, ErrorMessage ="請輸入1-99999之間的值")]
        public int Quantity { get; set; }
    }

    //Ajax甜度檢視
    public class BuyDMDrinkSweet
    {
        //甜度(下拉選單)
        [Required]
        [Display(Name = "甜度")]
        public string SweetTypeM { get; set; }

        public IList<SelectListItem> SweetType { get; set; }

    }

    //Ajax冰度檢視
    public class BuyDMDrinkIce
    {
        //冰度(下拉選單)
        [Required]
        [Display(Name = "冰度")]
        public string IceTypeM { get; set; }

        public IList<SelectListItem> IceType { get; set; }

    }


    ////////////////////////////////////////////////////////////////////////////////////////////////

    //Ajax尺寸檢視
    public class BuyDMDrinkSize_Box
    {
        //尺寸名 + 尺寸價(chexkbox)
        [Display(Name = "尺寸")]
        public IList<SelectListItem> Size { get; set; }
    }

    //Ajax冰度檢視
    public class BuyDMDrinkIce_Box
    {
        //冰度名 + 冰度價(chexkbox)
        [Display(Name = "冰度")]
        public IList<SelectListItem> IceTyp { get; set; }
    }

    //Ajax甜度檢視
    public class BuyDMDrinkSweet_Box
    {
        //甜度名 + 甜度價(chexkbox)
        [Display(Name = "冰度")]
        public IList<SelectListItem> SweetTyp { get; set; }
    }


    ////////////////////////////////////////////////////////////////////////////////////////////////


    //Ajax加料檢視
    public class BuyDMDrinkAdditem
    {
        //加料名 + 加料價(chexkbox)
        [Display(Name = "加料")]
        public IList<SelectListItem> AddItem { get; set; }
    }

    //////////// 飲料下單 /////////////////////////////////////////////////////////////////////////

    //下訂單用總表(團長與訂單)
    public class CreateBuyOrder_LeaderOrder
    {
        //訂單ID(流水號)
        [Key]
        public int OrderID { get; set; }

        //使用者(團長)ID
        public int UserID { get; set; }

        //菜單ID
        public int MenuID { get; set; }

        //訂單製造日期(核對訂單的有效期限)
        public DateTime CreateTime { get; set; }

        //訂單總價
        public int TotalCount { get; set; }

        //訂單結束訂購
        public int CanOrNotOrder { get; set; }

        //結帳,取消訂單時間
        public DateTime EndThisTime { get; set; }

        //定時功能
        public bool CheckEnd { get; set; }
    }

    //訂單用明細
    public class CreateBuyOrder_Detail
    {
        //明細流水號
        [Key]
        public int DetailID { get; set; }

        //訂單ID
        public int OrderID { get; set; }

        //菜單ID
        public int MenuID { get; set; }

        //使用者ID
        public int UserID { get; set; }

        //飲料ID
        public int DrinkID { get; set; }

        //尺寸價ID
        public int SizePID { get; set; }

        //甜度ID
        public int SweetID { get; set; }

        //冰度ID
        public int IceID { get; set; }

        //加料價01ID
        public int Additem01PID { get; set; }

        //加料價02ID
        public int Additem02PID { get; set; }

        //加料價03ID
        public int Additem03PID { get; set; }

        //飲料數量
        public int Quantity { get; set; }

        //飲料小計
        public int SCount { get; set; }
    }

    //訂單的團員(不包括團長),及訂單編號
    public class CreateBuyOrder_MemberOrder
    {
        //訂單團員流水號
        [Key]
        public int MemID { get; set; }

        //使用者(團員)ID
        public int UserID { get; set; }

        //訂單ID
        public int OrderID { get; set; }

        //使用者回覆狀態
        public bool Together { get; set; }

    }

    //訂單總覽用視圖
    public class AllOrder_View
    {
        //訂單ID
        public int OrderID { get; set; }

        //菜單名稱
        public string MenuName { get; set; }

        //團長姓名
        public string UserName { get; set; }

        //創建訂單的時間
        public DateTime CreateTime { get; set; }

        //菜單狀態
        public int CanOrder { get; set; }

        //使用者是否有下單
        public int HaveDrink { get; set; }
    }

    //訂單的品項清單
    public class OrderingDetailView
    {
        //訂單ID
        public int OrderID { get; set; }
      
        //飲料總計數量
        public int AllQuantity { get; set; }

        //飲料總計價錢
        public int AllCount { get; set; }

        //飲料詳細
        public IList<OrderingDetailView_Detail> OrderingDetailView_Detail { get; set; }
    }

    public class OrderingDetailView_Detail
    {
        //DetailID
        public int DetailID { get; set; }

        //訂購者ID
        public int UserID { get; set; }

        //訂購者名稱
        public string UserName { get; set; }

        //飲料名稱
        public string DrinkName { get; set; }

        //飲料大小
        public string DrinkSize { get; set; }

        //飲料甜度
        public string DrinkSweet { get; set; }

        //飲料冰度
        public string DrinkIce { get; set; }

        //飲料配料
        public string DrinkAdditem { get; set; }

        //飲料小計
        public int SCount { get; set; }

        //飲料數量
        public int SQuantity { get; set; }
    }

    //寄送Email視圖
    public class EmailModelView
    {
        public int OrderID { get; set; }

        //要寄給全部的人所以就不讓使用者寫
        //[Required]
        //[Display(Name = "收件人")]
        //public string To { get; set; }

        [Required]
        [Display(Name = "主旨")]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "內容")]
        public string Body { get; set; }

        [EmailAddress]
        [Display(Name = "公司Email")]
        public string Email { get; set; }

        [Display(Name = "Email密碼")]
        public string Password { get; set; }

        [Display(Name = "團員")]
        public IList<SelectListItem> Member { get; set; }

        //使用者可預設關團時間
        [Display(Name = "時間")]
        public string EndOverTime { get; set; }

        //確定自動結束
        public bool CheckEnd { get; set; }
    }

    public class ChangeMem
    {
        [Display(Name = "團員")]
        public IList<SelectListItem> Member { get; set; }
    }

    //寫入寄送郵件的訊息資料庫
    public class SendMessageView
    {
        //這裡的訊息以加密寫入

        //訊息流水號
        [Key]
        public string ID { get; set; }

        //收件人ID
        public int UserID { get; set; }

        //菜單ID
        public int OrderID { get; set; }

        //寄送時間(有效期限為2小時)
        public DateTime SendTime { get; set; }

        //寄送人的ID
        public int SentUser { get; set; }

        //寄信內容
        public string SentBody { get; set; }

        //連結網址
        public string SentAlink { get; set; }

        //寄信標題
        public string SentSubject { get; set; }

        //是否看過此訊息
        public bool ReadOrNot { get; set; }

        //使用者可預設關團時間
        public DateTime EndOverTime { get; set; }
    }

    //團長寄送的歷史
    public class LeaderSendMessage
    {
        //訊息流水號
        [Key]
        public string ID { get; set; }

        //寄送人的ID
        public int SentUser { get; set; }

        //菜單ID
        public int OrderID { get; set; }

        //寄信標題
        public string Subject { get; set; }

        //寄信內容
        public string Body { get; set; }

        //連結網址
        public string Alink { get; set; }

        //寄送時間(有效期限為2小時)
        public DateTime SendTime { get; set; }

        //使用者可預設關團時間
        public DateTime EndOverTime { get; set; }
    }

    //取消或結束訂單寄出的訊息紀錄
    public class OrderEndingSend
    {
        public int ID { get; set; }

        public int OrderID { get; set; }

        public int ToUser { get; set; }

        //0: 取消, 1: 結束
        public int EndOrCancel { get; set; }

        //是否已讀
        public bool ReadOrNot { get; set; }

        //結束時間
        public DateTime EndOverTime { get; set; }
    }

    //瀏覽每個團員的狀態
    public class MyMember_Response
    {
        //訂單團員流水號
        [Key]
        public int MemID { get; set; }

        //使用者(團員)ID
        public int UserID { get; set; }

        //使用者名稱
        public string UserName { get; set; }

        //訂單ID
        public int OrderID { get; set; }

        //使用者回覆狀態
        public bool Together { get; set; }
    }




}