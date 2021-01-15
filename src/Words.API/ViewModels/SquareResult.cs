namespace Words.API.ViewModels
{
    public class SquareResult
    {
        public string Letter { get; set; }
        public int Height { get; set; }
        public SquareTypeResult SquareType {get; set;}

        public SquareResult(string letter, int height, SquareTypeResult squareType)
        {
            Letter = letter;
            Height = height;
            SquareType = squareType;
        }
    }
}