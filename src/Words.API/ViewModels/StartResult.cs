using System;
namespace Words.API.ViewModels
{
    public class StartResult
    {
        public ErrorResult ErrorResult { get; set; }

        public StartResult(Exception exception, ErrorType errorType)
        {
            ErrorResult = new ErrorResult(exception, errorType);
        }

        public StartResult()
        {
        }
    }
}
