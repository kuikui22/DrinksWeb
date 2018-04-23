using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NewDrink2.Models
{
    public class User
    {
        public int ID { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "電子郵件")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "姓名")]
        [StringLength(20, ErrorMessage = "{0}的長度至少必須為{2}個字元。", MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "{0} 的長度至少必須為 {2} 個字元。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "確認密碼")]
        [Compare("Password", ErrorMessage = "密碼和確認密碼不相符。")]
        public string ConfirmPsd { get; set; }

        [Display(Name = "身份")]
        public int Identity { get; set; }

        //寄信狀態
        public int SendOrNot { get; set; }

        //寄信時間
        public DateTime SendTime { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "電子郵件")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; }
    }

    public class Identity
    {
        public int ID { get; set; }

        public int Identity1 { get; set; }

        public int Identity2 { get; set; }
    }

    public class UserCanDo
    {
        public int ID { get; set; }

        public bool BuyDrink { get; set; }

        public bool OrderSet { get; set; }

        public bool Message { get; set; }

        public bool Callnotice { get; set; }

        public bool ChangePsd { get; set; }

        public bool MyUserSet { get; set; }

        public bool MyMenuSet { get; set; }
    }

    public class IndexImage
    {
        public int ID { get; set; }

        [Required]
        [Display(Name = "圖片1")]
        public string Index01 { get; set; }

        [Required]
        [Display(Name = "圖片2")]
        public string Index02 { get; set; }

        [Required]
        [Display(Name = "圖片3")]
        public string Index03 { get; set; }
    }

}