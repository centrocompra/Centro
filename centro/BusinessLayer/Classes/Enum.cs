using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BusinessLayer.Classes
{
    public enum UserRole
    {
        Guest = 1,
        Buyer = 2,
        Seller = 3,
        Administrator = 4
    }

    public enum Gender
    {
        Male = 1,
        Female = 2,
        Other = 3
    }
    public enum TagType
    {
        UserTag = 1,
        ProductTag = 2,
        ProductMaterial = 3
    }
    public enum AuthenticationFrom
    {
        Website = 1,
        Facebook = 2,
        Twitter = 3
    }

    public enum ActionStatus
    {
        Successfull = 1,
        Error = 2,
        LoggedOut = 3,
        Unauthorized = 4
    }

    public enum SortMode
    {
        Asc = 1,
        Desc = 2,
        None = 3
    }

    public enum SelectedSellerTab
    {
        ShopDetails = 1,
        ListItems = 2,
        GetPaid = 3,
        PreviewShop = 4
    }

    public enum YesNoOption
    {
        No = 0,
        Yes = 1
    }

    public enum ShopSignupStepCompleted
    {
        [DescriptionAttribute("Shop Details")]
        ShopDetails = 1,
        [DescriptionAttribute("Get Paid")]
        GetPaid = 2,
        [DescriptionAttribute("List Items")]
        ListItems = 3,
        [DescriptionAttribute("Preview Shop")]
        PreviewShop = 4,
    }

    public enum ProductCondition
    {
        New = 1,
        Used = 2
    }

    public enum Manufacturer
    {
        [DescriptionAttribute("I did")]
        Idid = 1,
        [DescriptionAttribute("Another company or person")]
        AnotherCompanyOrPerson = 2
    }


    public enum ActionOnUser
    {
        Activate = 101,
        DeActivate = 102,
        Delete = 103
    }
    public enum ActionOnCategory
    {
        Publish = 111,
        UnPublish = 112,
        Delete = 113
    }
    public enum SelectedProductListingTab
    {
        Active = 1,
        Inactive = 2,
        SoldOut = 3
    }
    public enum ActionOnProduct
    {
        Activate = 101,
        DeActivate = 102,
        Delete = 103,
        Featured = 104
    }

    public enum OrderStatus
    {
        Pending = 1,
        Completed = 2,
        Canceled = 3
    }

    public enum ShippingStatus
    {
        NotShipped = 1,
        Shipped = 2,
        Received = 3
    }

    public enum PaymentStatus
    {
        Approved = 1,
        Declined = 2,
        Error = 3,
        HeldForReview = 4
    }

    public enum ServiceType
    {
        ProfessionalServices = 1,
        PrinterDimensionsServices = 2
    }
    public enum Services
    {
        //[DescriptionAttribute("3D Modeling")]
        //Modelling3D = 1,
        //[DescriptionAttribute("3D Scanning")]
        //Scanning3D = 2,
        //[DescriptionAttribute("Beta Testing")]
        //BetaTesting = 3,
        [DescriptionAttribute("Business Services / Consulting")]
        BusinessServices = 4,
        //[DescriptionAttribute("Commercial Grade Prototype Fabrication")]
        //CommercialGradePrototypeFabrication = 5,
        //[DescriptionAttribute("Graphic Design")]
        //GraphicDesign = 6,
        [DescriptionAttribute("Marketing / Writing")]
        MarketingWriting = 7,
        //[DescriptionAttribute("Personal Prototype Fabrication")]
        //PersonalPrototypeFabrication = 8,
        //[DescriptionAttribute("Sketching / Design")]
        //SketchingDesign = 9,
        [DescriptionAttribute("Tutoring")]
        Tutoring = 10
    }

    public enum Speciality
    {
        [DescriptionAttribute("Product Design Coding")]
        ProductDesignCoding = 1,
        [DescriptionAttribute("Product Printing")]
        ProductPrinting = 2,
        [DescriptionAttribute("Materials for Sale")]
        MaterialsforSale = 3,
        [DescriptionAttribute("Machine Inventions")]
        MachineInventions = 4,
        [DescriptionAttribute("Downloadable Products")]
        DownloadableProducts = 5,
        [DescriptionAttribute("Physical Products")]
        PhysicalProducts = 6
    }

    public enum MessagePlaceHolder
    {
        Inbox = 1,
        Sent = 2,
        Draft = 3,
        Archive = 4,
        Trash = 5,
        Spam = 6,
        Reply = 7
    }
    public enum CustomRequestStatus
    {
        [DescriptionAttribute("Draft")]
        Draft = 1,
        [DescriptionAttribute("Submitted")]
        Submitted = 2,
        [DescriptionAttribute("Accepted")]
        Accepted = 3,
        [DescriptionAttribute("Declined")]
        Declined = 4,
        [DescriptionAttribute("On Hold")]
        OnHold = 5,
        [DescriptionAttribute("Completed")]
        Completed = 6,
        [DescriptionAttribute("Waiting to start")]
        WaitingToStart = 7,
        [DescriptionAttribute("Working")]
        Working = 8
    }

    public enum InvoiceStatus
    {
        [DescriptionAttribute("Pending")]
        Pending = 1,
        [DescriptionAttribute("Payment Escrowed")]
        EscrowPayment = 2,
        [DescriptionAttribute("Payment Released")]
        ReleasePayment = 3,
        [DescriptionAttribute("Completed")]
        Completed = 4
    }

    public enum HubTemplate
    {
        Template1 = 1,
        Template2 = 2
    }

    public enum HubStatus
    {
        Active = 1,
        InActive = 2
    }

    public enum FeedBackType
    {
        [DescriptionAttribute("Product")]
        Product = 1,
        [DescriptionAttribute("Shop")]
        Shop = 2
    }

    public enum ActivityType
    {
        Hub = 1,
        HubComment = 2,
        Contest = 3,
        ContestWinner = 4,
        ContestComment = 5,
        Product = 6
    }

    public enum FollowType
    {
        Hub = 1,
        Contest = 2,
        Product = 3,
        Alerts = 4,
        Jobs = 5
    }

    public enum SendDownloadVia
    {
        CloudFileTransfer = 1,
        OtherContactSellerFirstForMoreInfo = 2
    }

    public enum JobShowIn
    {
        InterviewInvited = 1,
        JobsApplied = 2
    }

    public enum ContractNextStep
    {
        [DescriptionAttribute("Contractor to Create Invoice")]
        CreateInvoice = 1,
        [DescriptionAttribute("Buyer Accept Invoice & Pays")]
        ReleaseEscrow = 2,
        [DescriptionAttribute("Contractor Works on Project Segment")]
        StartWork = 3,
        [DescriptionAttribute("Buyer Receives Work")]
        ReleasePayment = 4
        //,
        //[DescriptionAttribute("Create a new invoice")]
        //CreateNewInvoice = 5
    }
}
