namespace FoodOrderingSystem.Models
{
    public class PaymentRequestModel
    {
        public long SyncId { get; set; }
        public string SubCompany { get; set; }
        public string AssetName { get; set; }
        public int AssetPrice { get; set; }
        public string ClientIp { get; set; }
        public string CountryCode { get; set; }
        public string CurrencyCode { get; set; }
        public string LanguageCode { get; set; }
        public string NotifyPage { get; set; }
        public string RedirectPage { get; set; }
        public string ErrorPage { get; set; }
        public string PaymentType { get; set; }
    }
}
