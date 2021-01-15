using System;
using System.Runtime.Serialization;

namespace Words.API.Exceptions
{
    [Serializable]
    public class EmptyRackException : ValidationException
    {
        public EmptyRackException(string letter) : base($"The rack is empty, so cannot remove letter '{letter}'.") { }

        protected EmptyRackException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public EmptyRackException() : base("The rack is empty.") { }

        public EmptyRackException(string message, Exception innerException) : base(message, innerException) { }
    }
}
