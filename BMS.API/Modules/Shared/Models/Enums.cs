namespace BMS.API.Modules.Shared.Models
{
    public enum Amenity
    {
        AC,
        NonAC,
        WiFi,
        WomenOnly,
        TwentyFourSeven,
        SilentZone
    }

    public enum DurationType
    {
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        HalfYearly,
        Yearly
    }

    public enum BookingStatus
    {
        Active,
        Expiring,
        Expired,
        Cancelled
    }

    public enum BookingSource
    {
        Online,
        Offline
    }

    public enum PaymentMethod
    {
        OnlinePrepay,
        PayAtLibrary
    }

    public enum PaymentStatus
    {
        Paid,
        Unpaid,
        Refunded
    }

    public enum PriceModifierType
    {
        Flat,
        Percent
    }

    public enum GenderRestriction
    {
        None,
        Male,
        Female
    }
}
