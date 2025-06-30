namespace QuanLy_PhongTro.Models.Momo
{
    public class MomoOptionModel
    {
        public string MomoApiUrl { get; set; }
        public string SecretKey { get; set; }
        public string AccessKey { get; set; }
        public string PartnerCode { get; set; }
        public string RequestType { get; set; }

        public MomoUrlConfig Booking { get; set; }
        public MomoUrlConfig Deposit { get; set; }
        public MomoUrlConfig PayBill { get; set; }
        public MomoUrlConfig PayContract { get; set; }

    }
    public class MomoUrlConfig
    {
        public string ReturnUrl { get; set; }
        public string NotifyUrl { get; set; }
    }

}
