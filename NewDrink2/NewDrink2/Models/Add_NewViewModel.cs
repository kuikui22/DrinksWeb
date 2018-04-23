using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NewDrink2.Models
{
    //飲料新增表
    public class AddDrinkDetails2
    {
        //MenuID
        public int MenuID { get; set; }

        //MenuName
        public string MenuName { get; set; }

        //飲料細項(動態欄位)
        public IList<DrinkDetails> DrinkDetails { get; set; }
    }

    //加料表價格填寫(編輯)//////////////////////////////////////////////////////
    public class AddItemCreateEdit2
    {
        public int MenuID { get; set; }

        public string MenuName { get; set; }

        public IList<AddCreateEditOther> AddCreateEditOther { get; set; }

        public IList<AddCreateEditOther> AddCreateNewOther { get; set; }
    }

    //加料編輯動態表單使用
    public class AddCreateEditOther2
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

    //編輯店家甜度用
    public class MenuAdd_NewSweet
    {
        public int MenuID { get; set; }

        //CheckBox使用     
        [Display(Name = "甜度")]
        public IList<SelectListItem> Sweet { get; set; }

        //動態欄位新增
        [Display(Name = "甜度種類")]
        public IList<SweetType> SweetName { get; set; }

    }

    //編輯店家冰度用
    public class MenuAdd_NewIce
    {
        public int MenuID { get; set; }

        //CheckBox使用     
        [Display(Name = "冰度")]
        public IList<SelectListItem> IceHot { get; set; }

        //動態欄位新增
        [Display(Name = "冰度種類")]
        public IList<IceType> IceName { get; set; }

    }

    //編輯店家尺寸用
    public class MenuAdd_NewSize
    {
        public int MenuID { get; set; }

        //CheckBox使用     
        [Display(Name = "大小")]
        public IList<SelectListItem> Size { get; set; }

        //動態欄位新增
        [Display(Name = "大小種類")]
        public IList<SizeType> SizeName { get; set; }

    }

    //編輯店家甜度.冰度.尺寸用
    public class MenuSISEdit2
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

    public class AllTypeAndDrinkDetails
    {
        public IEnumerable<Add2_MEnuSis> OrderDetail { get; set; }
        public IEnumerable<Add2_DrinksView> BuyerDetail { get; set; }
    }


}