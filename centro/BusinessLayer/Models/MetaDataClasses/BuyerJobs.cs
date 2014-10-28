using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Classes;
using BusinessLayer.Models.ViewModel;

namespace BusinessLayer.Models.DataModel
{
    public class Jobs
    {
        public GroupByDate Date { get; set; }
        public JobsViewModel BuyerJobView { get; set; }
    }

    public class DateWiseJobs
    {
        public DateTime CreatedOn { get; set; }
        public List<Jobs> BuyerJobs { get; set; }
    }

    public class JobsFilter
    {
        public string SearchType { get; set; }
        public string Keyword { get; set; }
        public decimal MinBudget { get; set; }
        public decimal MaxBudget { get; set; }
        public int LoggedInUserID { get; set; }
        public bool IsAwarded { get; set; }
    }

    [MetadataType(typeof(BuyerJobMeta))]
    public partial class BuyerJob
    {
        public virtual string Seller { get; set; }
    }

    public class BuyerJobMeta
    {
        [Required(ErrorMessage = "*Required.")]
        public virtual int CategoryId { get; set; }

        [Required(ErrorMessage = "*Required.")]
        public virtual string Seller { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 100 characters are allowed.")]
        [RegularExpression("^([A-Za-z0-9 ]*)$", ErrorMessage = "Only letters and numbers are allowed.")]
        public virtual string JobTitle { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(2000, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 2000 characters are allowed.")]
        public virtual string JobDescription { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(2000, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 2000 characters are allowed.")]
        public virtual string Requirements { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Range(10, Double.MaxValue, ErrorMessage = "Minimum Budget is 10$.")]
        public virtual decimal MinBudget { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Range(0.1, Double.MaxValue, ErrorMessage = "Invalid Budget.")]
        [NumericGreaterThan("MinBudget", AllowEquality = true, ErrorMessage = "Max Budget must be greater than or equal to min budget.")]
        public virtual decimal MaxBudget { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 25 characters are allowed.")]
        public virtual string Dimensions { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 25 characters are allowed.")]
        public virtual string Material { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 500 characters are allowed.")]
        public virtual string Specialties { get; set; }

        [Required(ErrorMessage = "*Required.")]
        public virtual DateTime IdealStartDate { get; set; }
    }
}