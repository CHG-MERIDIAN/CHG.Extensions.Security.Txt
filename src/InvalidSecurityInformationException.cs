using System;
using System.Collections.Generic;
using System.Text;

namespace CHG.Extensions.Security.Txt
{
    [Serializable]
    public class InvalidSecurityInformationException : Exception
    {
        public InvalidSecurityInformationException() { }
        public InvalidSecurityInformationException(string message) : base(message) { }
        public InvalidSecurityInformationException(string message, Exception inner) : base(message, inner) { }
        protected InvalidSecurityInformationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }    
}
