using System;
using System.Collections.Generic;
using System.Text;

namespace Asm.BankPlus
{
    public sealed class ExistsException : ApplicationException
    {
        public ExistsException() : base()
        {
        }

        public ExistsException(string message) : base(message)
        {
        }

        public ExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
