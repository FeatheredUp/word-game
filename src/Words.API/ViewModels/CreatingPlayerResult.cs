using Words.API.DataModels;

namespace Words.API.ViewModels
{
    public class CreatingPlayerResult
    {
        public string PlayerName { get; set; }
        public bool IsCurrentPlayer { get; set; }

        public CreatingPlayerResult(Player player, PlayerId currentPlayerId)
        {
            PlayerName = player?.PlayerName?.Value;
            IsCurrentPlayer = player?.PlayerId?.Value == currentPlayerId?.Value;
        }
    }
}