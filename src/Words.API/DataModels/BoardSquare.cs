namespace Words.API.DataModels
{
    public class BoardSquare
    {
        public string Letter { get; private set; }
        public int Height { get; private set; }
        public SquareType SquareType { get; private set; }

        public BoardSquare(string letter, int height, SquareType squareType)
        {
            Letter = letter;
            Height = height;
            SquareType = squareType;
        }

        public override string ToString()
        {
            if (Height == 0) return "BLANK";
            if (SquareType == SquareType.Unplayable) return "[ ]";
            var startingIndicator = SquareType == SquareType.Starting ? " *" : "";

            return $"{Letter} ({Height}{startingIndicator})";
        }

        public void AddTile(string letter)
        {
            Height += 1;
            Letter = letter;
        }

        public void SetTile(string letter, short height, SquareType squareType)
        {
            Letter = letter;
            Height = height;
            SquareType = squareType;
        }
    }
}