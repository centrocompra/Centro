using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BusinessLayer.Models.DataModel;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Models.ViewModel
{
    public class DealViewModel
    {
        public int DealID { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(200, ErrorMessage = "Maximum 200 characters are allowed.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(200, ErrorMessage = "Maximum 200 characters are allowed.")]
        public string SubTitle { get; set; }

        public bool IsActive { get; set; }

        public IEnumerable<DealItemViewModel> DealItems { get; set; }
        public List<DealItemViewModel> DealItemsList { get; set; }
    }

    public class DealItemViewModel
    {
        public int? DealID { get; set; }
        public int DealItemID { get; set; }
        public int? CategoryID { get; set; }
        public string CategoryName { get; set; }
        [Required(ErrorMessage = "*Required.")]
        [StringLength(200, ErrorMessage = "Maximum 200 characters are allowed.")]
        public string Title { get; set; }

        public HttpPostedFileBase Picture { get; set; }
        public string PictureName { get; set; }

        public bool IsActive { get; set; }
    }
}
