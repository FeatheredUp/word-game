using System;
using System.Collections.Generic;
using System.Linq;
using Words.API.DataModels;

namespace Words.API.ViewModels
{
    public class CreatingResult
    {
        public IEnumerable<CreatingPlayerResult> Players { get; set; }
        public bool CanStart { get; set; }
        public bool HasStarted { get; set; }
        public ErrorResult ErrorResult { get; set; }

        public CreatingResult() { }

        public CreatingResult(IEnumerable<Player> players, PlayerId currentPlayerId, bool hasStarted)
        {
            if (players == null) throw new ArgumentNullException(nameof(players));
            if (currentPlayerId == null) throw new ArgumentNullException(nameof(currentPlayerId));

            Players = players.Select(p => new CreatingPlayerResult(p, currentPlayerId)); 

            bool isCreator = IsCurrentPlayerCreator(players, currentPlayerId);
            bool enoughPlayers = players.ToList().Count > 1;

            CanStart = !hasStarted && isCreator && enoughPlayers;

            HasStarted = hasStarted;
        }

        public CreatingResult(Exception exception, ErrorType errorType)
        {
            ErrorResult = new ErrorResult(exception, errorType);
        }

        private static bool IsCurrentPlayerCreator(IEnumerable<Player> players, PlayerId playerId)
        {
            return players.FirstOrDefault(p => p.PlayerId.Value == playerId.Value && p.IsCreator) != null;
        }
    }
}
