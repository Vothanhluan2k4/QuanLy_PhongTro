namespace QuanLy_PhongTro.Models.Momo
{
    public class MomoExecuteResponseModel
    {
       
        public string PartnerCode { get; set; }
        public string OrderId { get; set; }
        public string RequestId { get; set; }
        public string Amount { get; set; }
        public string OrderInfo { get; set; }
        public string OrderType { get; set; }
        public string TransId { get; set; }
        public int ErrorCode { get; set; }
        public string Message { get; set; }
        public string PayType { get; set; }
        public DateTime ResponseTime { get; set; } 
        public string ExtraData { get; set; }
        public string Signature { get; set; }

        public bool Success => ErrorCode == 0;

    }
}
