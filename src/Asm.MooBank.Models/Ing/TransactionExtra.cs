using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asm.MooBank.Models.Ing
{
    public class TransactionExtra
    {
        public Guid TransactionId { get; set; }

        public string Description { get; set; }

        public string PurchaseType { get; set; }

        public int ReceiptNumber { get; set; }

        public string Location { get; set; }

        public DateTime? PurchaseDate { get; set; }

        public string Reference { get; set; }
    }
}
