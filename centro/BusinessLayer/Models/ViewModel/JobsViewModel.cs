using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BusinessLayer.Models.DataModel;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Models.ViewModel
{
    public class JobsViewModel
    {
        public int JobID { get; set; }
        public int BuyerID { get; set; }
        public int CategoryID { get; set; }
        public string JobTitle { get; set; }
        public string JobDescription { get; set; }
        public string Requirements { get; set; }
        public string Speciality { get; set; }
        public decimal MinBudget { get; set; }
        public decimal MaxBudget { get; set; }
        public string Dimensions { get; set; }
        public string Material { get; set; }
        public bool IsDeleted { get; set; }
        public int? RequestStatus { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? AwardedTo { get; set; }
        public bool IsRead { get; set; }
        public bool IsActive { get; set; }
        public bool IsFavorite { get; set; }
        public string Specialties { get; set; }
        public bool IsPrivate { get; set; }
        public int? JobSentTo { get; set; }

        public int TotalApplicants { get; set; }

        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string UserName { get; set; }
        public int? ProfilePicId { get; set; }
        public int? CountryId { get; set; }
        public int? StateId { get; set; }
        public int? CityId { get; set; }
        public decimal? TotalEarning { get; set; }
        public DateTime LastLoginOn { get; set; }
        public string PostalCode { get; set; }
        public string StreetAddress1 { get; set; }
        public string StreetAddress2 { get; set; }
    }
}
