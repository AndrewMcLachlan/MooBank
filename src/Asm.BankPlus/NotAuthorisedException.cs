using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using System.Text;

namespace Asm.BankPlus
{
    public class NotAuthorisedException : SecurityException
    {
        public NotAuthorisedException()
        {
        }

        public NotAuthorisedException(string message) : base(message)
        {
        }

        public NotAuthorisedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public NotAuthorisedException(string message, Type type) : base(message, type)
        {
        }

        public NotAuthorisedException(string message, Type type, string state) : base(message, type, state)
        {
        }

        protected NotAuthorisedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
