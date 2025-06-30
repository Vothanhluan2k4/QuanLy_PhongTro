using QuanLy_PhongTro.Models.Vnpay;

namespace QuanLy_PhongTro.Services.Vnpay
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context,string transactionType = "Booking");
        PaymentResponseModel PaymentExecute(IQueryCollection collections);

    }
}
