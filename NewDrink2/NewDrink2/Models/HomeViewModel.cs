using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NewDrink2.Models
{
    public class HaveMessageAndEndMessage
    {
        public IEnumerable<SendMessageView> SendMessageView_s { get;set;}

        public List<SendMessageView> EndMsg { get; set; }
    }

    public class Add2_DrinksView
    {
        //菜單ID
        public int MenuID{ get; set; }

        //菜單名
        public string MenuName { get; set; }

        //飲料種類
        [Required]
        [Display(Name = "飲料種類")]
        public string DrinkType { get; set; }

        //飲料細項(動態欄位)
        public IList<Add2_Drinks_detail> Add2_Drinks_details { get; set; }

    }

    public class Add2_Drinks_detail
    {
        //飲料名稱
        [Required]
        [Display(Name = "飲料名稱")]
        public string DrinkName { get; set; }

        //尺寸(下拉選單)
        [Required]
        [Display(Name = "大小")]
        public string SizeTypeM { get; set; }

        public IEnumerable<SelectListItem> SizeType { get; set; }

        //飲料尺寸 + 價錢
        public IList<Add2_Drinks_Size> Add2_Drinks_Sizes { get; set; }

        //價錢
        [Required]
        [Display(Name = "單價")]
        public int DrinkPrice { get; set; }

        //甜度(checkbox)
        [Display(Name = "甜度")]
        public IList<SelectListItem> Sweet { get; set; }

        //冰度(chexkbox)
        [Display(Name = "冰度")]
        public IList<SelectListItem> IceHot { get; set; }

        //加料名 + 加料價(chexkbox)
        [Display(Name = "加料")]
        public IList<SelectListItem> AddItem { get; set; }

        //備註
        [Display(Name = "備註")]
        public string Bathus { get; set; }
    }

    public class Add2_Drinks_Size
    {
        public int SizeID { get; set; }

        public string SizeName { get; set; }

        [Required]
        [Display(Name = "價錢")]
        public int Price { get; set; }
    }

    public class Add2_MEnuSis
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

        public IList<AddItemCreate> AddItemCreate { get; set; }

    }


}