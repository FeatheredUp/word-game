using System;
using System.Runtime.Serialization;

namespace Words.API.Exceptions
{
    [Serializable]
    public class LetterNotOnRackException : ValidationException
    {
        public LetterNotOnRackException(string rack, string letter) : base($"The letter '{letter}' is not on the rack '{rack}'.") { }

        protected LetterNotOnRackException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public LetterNotOnRackException() : base("That letters is not on the rack.") { }

        public LetterNotOnRackException(string message, Exception innerException) : base(message, innerException) { }

        public LetterNotOnRackException(string message) : base(message) { }
    }
}
