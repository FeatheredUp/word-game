using System;
using System.Runtime.Serialization;

namespace Words.API.Exceptions
{
    [Serializable]
    public class FullRackException : ValidationException
    {
        public FullRackException(string rack, string letter) : base($"The rack is already full.  It contains '{rack}' and so cannot add letter '{letter}'.") { }

        protected FullRackException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public FullRackException() : base("The rack is already full.") { }

        public FullRackException(string message, Exception innerException) : base(message, innerException) { }

        public FullRackException(string message) : base(message) { }
    }
}
