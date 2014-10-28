using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer.Classes;
using BusinessLayer.Models.DataModel;

namespace BusinessLayer.Models.ViewModel
{
    public class ActionOutputBase
    {
        public ActionStatus Status { get; set; }
        public String Message { get; set; }
        public List<String> Results { get; set; }
    }

    public class ActionOutput<T> : ActionOutputBase
    {
        public T Object { get; set; }
        public List<T> List { get; set; }
    }

    public class ActionOutput : ActionOutputBase
    {
        public int ID { get; set; }
    }

    public class PagingResult<T>
    {
        public List<T> List { get; set; }
        public int TotalCount { get; set; }
        public ActionStatus Status { get; set; }
        public String Message { get; set; }
    }

    public class UserDetails
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public String Email { get; set; }
        public Int32 Id { get; set; }
        public String Username { get; set; }
        public UserRole UserRole { get; set; }
        public Gender Gender { get; set; }
        public Shop ShopDetails { get; set; }
        public List<ShopPicture> ShopPictures { get; set; }
        public String ProfilePicture { get; set; }
        public String UserLocation { get; set; }
    }

    public class UserProductPicture
    {
        public Int32 PictureId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string SavedName { get; set; }
        public string Thumbnail { get; set; }
        public string MimeType { get; set; }
        public int SizeInBytes { get; set; }
        public int SizeInKB { get; set; }
        public int SizeInMB { get; set; }
        public string Counter { get; set; }
    }

    public class ProductFileViewModel
    {
        public Int32 ProductFileId { get; set; }
        public string DisplayName { get; set; }
        public string SavedName { get; set; }        
        public string MimeType { get; set; }
        public int SizeInBytes { get; set; }
        public decimal SizeInKB { get; set; }
        public decimal SizeInMB { get; set; }
    }

    public class ProductFiles
    {
        public List<ProductFileViewModel> ProductTempFiles { get; set; }
    }

    public class UserProductTempPictures
    {
        public List<UserProductPicture> UserTempPictures { get; set; }
    }

    public class FileAttachmentViewModel
    {
        public Int32 AttachmentID { get; set; }
        public string DisplayName { get; set; }
        public string SavedName { get; set; }
        public string MimeType { get; set; }
        public int SizeInBytes { get; set; }
        public decimal SizeInKB { get; set; }
        public decimal SizeInMB { get; set; }
    }
    public class RequestAttachmentsTempFile
    {
        public List<FileAttachmentViewModel> RequestAttachments { get; set; }
    }
    public class SiteUserDetails
    {
        public UserDetails LoggedInUser { get; set; }
        public UserDetails CurrentUser { get; set; }
        public Boolean IsAuthenticated { get; set; }
        public String LoginText { get; set; }
        public string SiteURL { get; set; }
    }

    public static class SiteURL
    {
        public static string URL { get; set; }
    }

    public class SelectedTabs
    {
        public SelectedSellerTab SellerMainTab { get; set; }
        //public SelectedInnerMemberTab InnerUserTab { get; set; }
        //public SelectedAdminMainTab AdminMainTab { get; set; }
        //public SelectedAdminInnerTab AdminInnerTab { get; set; }
        //public SelectedAdminInnerSubTab AdminInnerSubTab { get; set; }
    }
    public class UserAction
    {
        public ActionOnUser ActionID { get; set; }
        public List<Int32> UserID { get; set; }
    }
    public class CategoryAction
    {
        public ActionOnCategory ActionID { get; set; }
        public List<Int32> CategoryID { get; set; }
    }
    public class ProductAction 
    {
        public ActionOnProduct ActionID { get; set; }
        public Int32 ShopId { get; set; }
        public List<Int32> ProductID { get; set; }
    }
    public class ShopServices
    {
        public Services ServiceId { get; set; }
        public String ServiceName { get; set; }
        public Boolean IsChecked { get; set; }
    }
    public class ShopSpeciality
    {
        public Speciality SpecialityId { get; set; }
        public String SpecialityName { get; set; }
        public Boolean IsChecked { get; set; }
    }

    public class UserRegistrationThreadObject
    {
        public User User { get; set; }
        public string Path { get; set; }
        public string SiteURL { get; set; }
        public string encodedURL { get; set; }
    }

    public class PasswordResetThreadObject
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public string SiteURL { get; set; }
        public string Path { get; set; }
    }
}
