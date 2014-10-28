using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Centro.Models
{
    public class Captcha
    {
        [Display(Name = "Type the Code", Order = 20)]
        [Remote("ValidateCaptcha", "Captcha", "",ErrorMessage="Invalid Code!")]
        [Required(ErrorMessage = "*Required")]
        public virtual string CaptchaValue { get; set; }
        public Captcha()
        {

        }
    }

    public class InvisibleCaptcha
    {
        [Display(Name = "InvisibleCaptcha", Order = 20)]
        [Remote("ValidateInvisibleCaptcha", "Captcha", "", ErrorMessage = "Invalid Code!")]
        public virtual string InvisibleCaptchaValue { get; set; }
        public InvisibleCaptcha()
        {

        }
    }
}