using System;
using Words.API.DataModels;

namespace Words.API.ViewModels
{
    public class JoinResult
    {
        public string PlayerId { get; set; }
        public ErrorResult ErrorResult { get; set; }

        public JoinResult()
        {
        }

        public JoinResult(PlayerId playerId)
        {
            if (playerId == null) throw new ArgumentNullException(nameof(playerId));
            PlayerId = playerId.Value;
        }

        public JoinResult(Exception exception, ErrorType errorType)
        {
            ErrorResult = new ErrorResult(exception, errorType);
        }
    }
}
