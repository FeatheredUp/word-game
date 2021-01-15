
namespace Words.API.DataModels
{
    public class TilePlacement
    {
        public string Letter { get; }
        public int Row { get; }
        public int Column { get; }

        public TilePlacement(string letter, int row, int column)
        {
            Letter = letter;
            Row = row;
            Column = column;
        }

        public override string ToString()
        {
            return $"'{Letter}' at ({Row}, {Column})";
        }
    }
}
