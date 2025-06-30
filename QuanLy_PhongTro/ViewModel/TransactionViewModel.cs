using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.ViewModel
{
    public class TransactionViewModel
    {

        public string TransactionId { get; set; }

        public string OrderDescription { get; set; }
        
        public string PaymentMethod { get; set; }
        public double Amount { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
