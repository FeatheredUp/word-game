using System;

namespace Words.API.ViewModels
{
    public class ErrorResult
    {
        public string ErrorMessage { get; set; }
        public ErrorType ErrorType { get; set; }

        public ErrorResult(Exception exception, ErrorType errorType)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            ErrorMessage = exception.Message;
            ErrorType = errorType;
        }
    }
}