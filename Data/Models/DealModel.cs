using System;

namespace A2ParserTestTask.Data.Models
{
    public class DealModel
    {
        public int Id { get; set; }

        public string Number { get; set; }
       
        public string Date { get; set; }

        public double VolumeBuyer { get; set; }

        public double VolumeSeller { get; set; }

        public int BuyerId { get; set; }

        public ContractorModel Buyer { get; set; }

        public int SellerId { get; set; }

        public ContractorModel Seller { get; set; }
    }
}
