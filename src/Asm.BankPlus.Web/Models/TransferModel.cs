﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
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