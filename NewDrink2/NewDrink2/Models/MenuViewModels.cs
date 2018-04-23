using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NewDrink2.Models
{
    public class Menu
    {
        public int ID { get; set; }

        [Display(Name = "建立時間")]
        public DateTime CreateTime { get; set; }

        [Required]
        [Display(Name = "菜單名稱")]
        public string MenuName { get; set; }

        [Display(Name = "開放")]
        public bool Open { get; set; }

        [Required]
        [Display(Name = "店家電話")]
        public string MenuPhone { get; set; }

        [Required]
        [Display(Name = "店家地址")]
        public string Addr { get; set; }

        public string ImageName { get; set; }

        [Display(Name = "訂購條件")]
        public string OrderConditions { get; set; }
    }


    //甜度
    public class SweetType
    {
        public int ID { get; set; }

        [Required]
        [Display(Name = "甜度種類")]
        public string SweetName { get; set; }
    }

    //冰度
    public class IceType
    {
        public int ID { get; set; }

        [Required]
        [Display(Name = "冰度種類")]
        public string IceName { get; set; }
    }

    //加料
    public class AddItemType
    {
        public int ID { get; set; }

        [Required]
        [Display(Name = "加料種類")]
        public string ItemName { get; set; }
    }

    //尺寸
    public class SizeType
    {
        public int ID { get; set; }

        [Required]
        [Display(Name = "大小種類")]
        public string SizeName { get; set; }

        public string SimpleName { get; set; }
    }

    //加料價
    public class AddItemTypePrice
    {
        public int ID { get; set; }

        public int ItemID { get; set; }

        //0大杯,1中杯,2小杯,3例外
        public int SizeNumber { get; set; }

        public int ItemPrice { get; set; }

        public int MenuID { get; set; }
    }   

    //加料表價格填寫
    public class AddItemCreate
    {
        public int ID { get; set; }

        [Required]
        [Display(Name = "加料種類")]
        public string ItemName { get; set; }

        [Required]
        [Display(Name = "大杯價")]
        public int LPrice { get; set; }

        [Required]
        [Display(Name = "中杯價")]
        public int MPrice { get; set; }

        [Required]
        [Display(Name = "小杯價")]
        public int SPrice { get; set; }
    }

    //新增菜單
    public class NewMenu
    {
        //使用表: Menu,SweetType,IceType,AddItemType,SizeType,AddItemTypePrice
        //Menu
        public int ID { get; set; }

        [Required]
        [Display(Name = "菜單名稱")]
        public string MenuName { get; set; }

        [Display(Name = "開放")]
        public bool Open { get; set; }

        [Required]
        [Display(Name = "店家電話")]
        [StringLength(15, ErrorMessage = "{0}的長度至少必須為{2}個字元。", MinimumLength = 8)]
        public string MenuPhone { get; set; }

        [Required]
        [Display(Name = "店家地址")]
        [StringLength(256, ErrorMessage = "{0}的長度至少必須為{2}個字元。", MinimumLength = 8)]
        public string Addr { get; set; }

        //圖片
        public string ImageName { get; set; }

        [Display(Name = "訂購條件")]
        public string OrderConditions { get; set; }

        //CheckBox使用     
        [Display(Name = "甜度")]
        public IList<SelectListItem> Sweet { get; set; }

        [Display(Name = "冰度")]
        public IList<SelectListItem> IceHot { get; set; }

        [Display(Name = "大小")]
        public IList<SelectListItem> Size { get; set; }

        //動態欄位新增
        [Display(Name = "加料種類")]
        public IList<AddItemCreate> AddItemCreate { get; set; }

        [Display(Name = "甜度種類")]
        public IList<SweetType> SweetName { get; set; }

        [Display(Name = "冰度種類")]
        public IList<IceType> IceName { get; set; }

        [Display(Name = "大小種類")]
        public IList<SizeType> SizeName { get; set; }

    }

    //店家甜度表
    public class MenuSweet
    {
        public int ID { get; set; }

        public int MenuID { get; set; }

        public int SweetID { get; set; }

        public string SweetName { get; set; }
    }

    //店家冰度表
    public class MenuIce
    {
        public int ID { get; set; }

        public int MenuID { get; set; }

        public int IceID { get; set; }

        public string IceName { get; set; }
    }

    //店家尺寸表
    public class MenuSize
    {
        public int ID { get; set; }

        public int MenuID { get; set; }

        public int SizeID { get; set; }

        public string SizeName { get; set; }
    }

    //店家加料表
    public class MenuAddItem
    {
        public int ID { get; set; }

        public int MenuID { get; set; }

        public int ItemIDPriceID { get; set; }
    }

    //////////////////飲料細項用表//////////////////////////////////    

    //飲料細項
    public class DrinkDetails
    {
        public int ID { get; set; }

        //飲料種類
        [Required]
        [Display(Name = "飲料種類")]
        public string DrinkType { get; set; }

        //飲料名
        [Required]
        [Display(Name = "飲料名稱")]
        public string DrinkName { get; set; }

        //尺寸(下拉選單)
        [Required]
        [Display(Name = "大小")]
        public string SizeTypeM { get; set; }

        public IEnumerable<SelectListItem> SizeType { get; set; }

        //甜度(checkbox)
        [Display(Name = "甜度")]
        public IList<SelectListItem> Sweet { get; set; }

        //冰度(chexkbox)
        [Display(Name = "冰度")]
        public IList<SelectListItem> IceHot { get; set; }

        //加料名 + 加料價(chexkbox)
        [Display(Name = "加料")]
        public IList<SelectListItem> AddItem { get; set; }

        //價錢
        [Required]
        [Display(Name = "單價")]
        public int DrinkPrice { get; set; }
    }

    //飲料新增表
    public class AddDrinkDetails
    {
        //MenuID
        public int MenuID { get; set; }

        //MenuName
        public string MenuName { get; set; }

        //飲料細項(動態欄位)
        public IList<DrinkDetails> DrinkDetails { get; set; }
    }

    //飲料名及種類表
    public class MenuDrink
    {
        public int ID { get; set; }

        public int MenuID { get; set; }

        public string DrinkType { get; set; }

        public string DrinkName { get; set; }

        //備註
        public string Bathus { get; set; }
    }

    //飲料甜度表
    public class SweetTable
    {
        public int ID { get; set; }

        public int SizePID { get; set; }

        public int SweetID { get; set; }
    }

    //飲料冰度表
    public class IceTable
    {
        public int ID { get; set; }

        public int SizePID { get; set; }

        public int IceID { get; set; }
    }

    //飲料尺寸價表
    public class SizeTable
    {
        public int ID { get; set; }

        public int DrinkID { get; set; }

        public int SizeID { get; set; }

        public int Price { get; set; }

        public int MenuID { get; set; }

        public int DrinkSort { get; set; }
    }

    //飲料加料表
    public class AddItemTable
    {
        public int ID { get; set; }

        public int SizePID { get; set; }

        public int ItemIDPriceID { get; set; }
    }

    ///////飲料檢視用表//////////////////////////////////
    //飲料檢視總表
    public class MenuDrinkNameType
    {
        public int MenuID { get; set; }

        //飲料名稱.種類
        public int DrinkID { get; set; }

        public string DrinkType { get; set; }

        public string DrinkName { get; set; }

        //飲料大小.單價
        public int SizePID { get; set; }

        public string SizeName { get; set; }

        public int DrinkPrice { get; set; }

        //飲料甜度總數
        public int CanUseSweet { get; set; }

        //飲料冰度總數
        public int CanUseIce { get; set; }

        //飲料加料總數
        public int CanUseAddItem { get; set; }

        //排序
        public int DrinkSort { get; set; }
    }

    //加料表價格填寫(編輯)//////////////////////////////////////////////////////
    public class AddItemCreateEdit
    {
        public int MenuID { get; set; }

        public string MenuName { get; set; }

        public IList<AddCreateEditOther> AddCreateEditOther { get; set; }

        public IList<AddCreateEditOther> AddCreateNewOther { get; set; }
    }

    //加料編輯動態表單使用
    public class AddCreateEditOther
    {
        public int ItemID { get; set; }

        [Required]
        [Display(Name = "加料種類")]
        public string ItemName { get; set; }

        [Required]
        [Display(Name = "大杯價")]
        public int LPrice { get; set; }

        [Required]
        [Display(Name = "中杯價")]
        public int MPrice { get; set; }

        [Required]
        [Display(Name = "小杯價")]
        public int SPrice { get; set; }

        public bool deleteOrNot { get; set; }
    }

    //編輯店家甜度.冰度.尺寸用
    public class MenuSISEdit
    {
        public int MenuID { get; set; }

        //CheckBox使用     
        [Display(Name = "甜度")]
        public IList<SelectListItem> Sweet { get; set; }

        [Display(Name = "冰度")]
        public IList<SelectListItem> IceHot { get; set; }

        [Display(Name = "大小")]
        public IList<SelectListItem> Size { get; set; }

        //動態欄位新增
        [Display(Name = "甜度種類")]
        public IList<SweetType> SweetName { get; set; }

        [Display(Name = "冰度種類")]
        public IList<IceType> IceName { get; set; }

        [Display(Name = "大小種類")]
        public IList<SizeType> SizeName { get; set; }
    }


}