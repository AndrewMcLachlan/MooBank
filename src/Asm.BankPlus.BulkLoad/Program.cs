using System;
using Asm.BankPlus.Data;
using Microsoft.EntityFrameworkCore;

namespace Asm.BankPlus.BulkLoad
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BankPlusContext>();
            //optionsBuilder.UseSqlServer("");

            using (BankPlusContext db = new BankPlusContext(optionsBuilder.Options))
            {

            }
        }
    }
}
