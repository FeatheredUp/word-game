using System.Collections.Generic;

namespace Words.API.ViewModels
{
    public class PlayInput
    {
        public string GameId { get; set; }
        public string PlayerId { get; set; }
        public List<TilePlacementInput> TilePlacements { get; set; }
    }
}
