using System;
using System.Collections.Generic;
using Asm.MooBank.Domain.Entities;
using Asm.MooBank.Models;

namespace Asm.MooBank.Web.Models
{
    public class HistoryModel
    {
        public string Account { get; set; }
        public int Page { get; set; }

        public int TotalRecords { get; set; }

        public int TotalPages
        {
            get
            {
                return (int)Math.Ceiling((double)TotalRecords / (double)PageSize);
            }
        }

        public int PageSize { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public IEnumerable<HistoryTransaction> Transactions { get; set; }

        public HistoryModel()
        {
            Transactions = new List<HistoryTransaction>();
        }
    }

    public class HistoryTransaction
    {
        public DateTime Date { get; set; }

        public string Account { get; set; }

        public decimal Amount { get; set; }

        public TransactionType TransactionType { get; set; }

        public string Description { get; set; }
    }
}