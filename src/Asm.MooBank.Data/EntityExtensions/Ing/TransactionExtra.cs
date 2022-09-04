using System;
using System.ComponentModel.DataAnnotations;

namespace Asm.MooBank.Data.Entities.Ing
{
    public partial class TransactionExtra
    {
        public static explicit operator Models.Ing.TransactionExtra(TransactionExtra entity)
        {
            return new Models.Ing.TransactionExtra
            {
                Description = entity.Description,
                Location = entity.Location,
                PurchaseDate = entity.PurchaseDate,
                PurchaseType = entity.PurchaseType,
                ReceiptNumber = entity.ReceiptNumber,
                Reference = entity.Reference,
                TransactionId = entity.TransactionId,
            };
        }

        public static explicit operator TransactionExtra(Models.Ing.TransactionExtra entity)
        {
            return new TransactionExtra
            {
                Description = entity.Description,
                Location = entity.Location,
                PurchaseDate = entity.PurchaseDate,
                PurchaseType = entity.PurchaseType,
                ReceiptNumber = entity.ReceiptNumber,
                Reference = entity.Reference,
                TransactionId = entity.TransactionId,
            };
        }
    }
}
