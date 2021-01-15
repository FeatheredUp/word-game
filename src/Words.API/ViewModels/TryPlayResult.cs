using System;
using System.Collections.Generic;
using System.Linq;
using Words.API.DataModels;

namespace Words.API.ViewModels
{
    public class TryPlayResult
    {
        public List<string> LastWords { get; }
        public int LastScore { get; set; }

        public ErrorResult ErrorResult { get; set; }

        public TryPlayResult(Turn turn)
        {
            LastWords = turn?.Words?.ToList();
            LastScore = turn.Score;
        }

        public TryPlayResult(Exception exception, ErrorType errorType)
        {
            ErrorResult = new ErrorResult(exception, errorType);
        }
    }
}
