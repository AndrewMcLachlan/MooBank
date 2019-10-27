using System;
using System.Collections.Generic;
using System.Text;

namespace Asm.BankPlus.Models
{
    public class AccountHolder
    {
        public Guid Id { get; set; }

        public string EmailAddress { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
