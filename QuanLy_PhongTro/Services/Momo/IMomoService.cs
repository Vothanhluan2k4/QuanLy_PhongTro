using QuanLy_PhongTro.Models.Momo;
using QuanLy_PhongTro.Models.Order;

namespace QuanLy_PhongTro.Services.Momo
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfoModel model, string transactionType = "Booking");
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}