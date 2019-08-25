﻿using System;
using System.ComponentModel.DataAnnotations;
using Asm.BankPlus.Data.Entities;
using Asm.BankPlus.Models;

namespace Asm.BankPlus.Web.Models
{
    public class TransferModel
    {
        [Required]
        [Display(Name = "Transfer From")]
        public Guid SourceAccountId { get; set; }

        [Required]
        [Display(Name = "Transfer To")]
        public Guid DestinationAccountId { get; set;}

        [Range(0.01, Double.MaxValue)]
        [Required]
        public decimal Amount { get; set; }

        public string Description { get; set; }

        [Display(Name = "Set up a recurring transaction")]
        public bool RecurringTransaction { get; set; }

        [Display(Name = "Frequency")]
        public Schedule Schedule { get; set; }
    }
}