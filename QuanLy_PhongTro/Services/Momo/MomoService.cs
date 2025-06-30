using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Crmf;
using QuanLy_PhongTro.Models.Momo;
using QuanLy_PhongTro.Models.Order;
using RestSharp;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace QuanLy_PhongTro.Services.Momo
{
    public class MomoService : IMomoService
    {
        private readonly IOptions<MomoOptionModel> _options;
        public MomoService(IOptions<MomoOptionModel> options)
        {
            _options = options;
        }

        public async Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfoModel model, string transactionType = "Booking")
        {
            model.OrderId = DateTime.UtcNow.Ticks.ToString();

            var urlConfig = transactionType switch
            {
                "Deposit" => _options.Value.Deposit,
                "PayBill" => _options.Value.PayBill,
                "PayContract" => _options.Value.PayContract,
                _ => _options.Value.Booking
            };

            var returnUrl = urlConfig?.ReturnUrl ?? throw new ArgumentException("ReturnUrl is not configured.");
            var notifyUrl = urlConfig?.NotifyUrl ?? throw new ArgumentException("NotifyUrl is not configured.");

            model.OrderInfo = "Khách hàng: " + model.FullName + ". Nội dung: " + model.OrderInfo;
            var rawData =
                $"partnerCode={_options.Value.PartnerCode}" +
                $"&accessKey={_options.Value.AccessKey}" +
                $"&requestId={model.OrderId}" +
                $"&amount={model.Amount}" +
                $"&orderId={model.OrderId}" +
                $"&orderInfo={model.OrderInfo}" +
                $"&returnUrl={returnUrl}" +
                $"&notifyUrl={notifyUrl}" +
                $"&extraData=";

            var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

            var client = new RestClient(_options.Value.MomoApiUrl);
            var request = new RestRequest() { Method = Method.Post };
            request.AddHeader("Content-Type", "application/json; charset=UTF-8");

            // Create an object representing the request data
            var requestData = new
            {
                accessKey = _options.Value.AccessKey,
                partnerCode = _options.Value.PartnerCode,
                requestType = _options.Value.RequestType,
                notifyUrl = notifyUrl,
                returnUrl = returnUrl,
                orderId = model.OrderId,
                amount = model.Amount.ToString(),
                orderInfo = model.OrderInfo,
                requestId = model.OrderId,
                extraData = "",
                signature = signature
            };

            request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), ParameterType.RequestBody);

            var response = await client.ExecuteAsync(request);
            var momoResponse = JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);
            return momoResponse;
        }

        public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
            {
            var response = new MomoExecuteResponseModel
            {
                PartnerCode = GetQueryValue(collection, "partnerCode"),
                OrderId = GetQueryValue(collection, "orderId"),
                RequestId = GetQueryValue(collection, "requestId"),
                Amount = GetQueryValue(collection, "amount"),
                OrderInfo = GetQueryValue(collection, "orderInfo"),
                OrderType = GetQueryValue(collection, "orderType"),
                TransId = GetQueryValue(collection, "transId"),
                ErrorCode = int.TryParse(GetQueryValue(collection, "errorCode"), out int errorCode) ? errorCode : -1,
                Message = GetQueryValue(collection, "message"),
                PayType = GetQueryValue(collection, "payType"),
                ResponseTime = DateTime.TryParse(GetQueryValue(collection, "responseTime"), out DateTime responseTime) ? responseTime : DateTime.MinValue,
                ExtraData = GetQueryValue(collection, "extraData"),
                Signature = GetQueryValue(collection, "signature")
            };

            Console.WriteLine($"MoMo Callback Response: {JsonConvert.SerializeObject(response)}");

            return response;
        }

        private string GetQueryValue(IQueryCollection collection, string key)
        {
            return collection.FirstOrDefault(s => s.Key == key).Value.ToString() ?? string.Empty;
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            byte[] hashBytes;

            using (var hmac = new HMACSHA256(keyBytes))
            {
                hashBytes = hmac.ComputeHash(messageBytes);
            }

            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return hashString;
        }
    }
}