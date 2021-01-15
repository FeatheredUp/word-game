namespace Words.API.DataModels
{
    public class CreatedGame
    {
        public GameId GameId { get; }
        public Player Player { get; }

        public CreatedGame(PlayerName playerName)
        {
            GameId = new GameId();
            Player = new Player(playerName, true);
        }

        public override string ToString()
        {
            return $"{GameId.Value} - {Player}";
        }
    }
}