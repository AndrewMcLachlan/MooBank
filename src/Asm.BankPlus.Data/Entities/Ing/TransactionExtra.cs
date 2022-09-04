using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.MooBank.Data.Entities.Ing
{
    public partial class TransactionExtra
    {
        public Guid TransactionId { get; set; }

        [StringLength(50)]
        public string Description { get; set; }

        [StringLength(20)]
        public string PurchaseType { get; set; }

        public int ReceiptNumber { get; set; }

        [StringLength(12)]
        public string Location { get; set; }

        public DateTime? PurchaseDate { get; set; }

        [StringLength(50)]
        public string Reference { get; set; }

        //public virtual Transaction Transaction { get; set; }
    }
}
