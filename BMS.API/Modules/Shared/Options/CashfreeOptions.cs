namespace BMS.API.Modules.Shared.Options
{
    /// <summary>
    /// Bound from the "Cashfree" configuration section (see appsettings.Development.json).
    /// AppId/SecretKey are sandbox test credentials for now - swap to live credentials (and a
    /// live BaseUrl) via appsettings.Production.json / environment variables when going live.
    /// </summary>
    public class CashfreeOptions
    {
        public string AppId { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://sandbox.cashfree.com/pg";
        public string ApiVersion { get; set; } = "2023-08-01";
    }
}
