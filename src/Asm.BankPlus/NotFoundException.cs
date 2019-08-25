using System;
using System.Collections.Generic;
using System.Text;

namespace Asm.BankPlus
{
    public sealed class NotFoundException : ApplicationException
    {
        public NotFoundException() : base()
        {
        }

        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
