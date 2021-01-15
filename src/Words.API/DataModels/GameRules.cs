using System;
using System.Collections.Generic;
using System.Linq;

namespace Words.API.DataModels
{
    public class GameRules
    {
        public const int MaxRows = 10;
        public const int MaxColumns = 10;

        private static readonly IReadOnlyDictionary<string, Ruleset> _Mapping = new Dictionary<string, Ruleset>
        {
            { "Mini Upwords", Ruleset.MiniUpwords },
            { "Small Upwords", Ruleset.SmallUpwords },
            { "Standard Upwords", Ruleset.StandardUpwords }
        };

        public static IEnumerable<string> Rulesets => _Mapping.Keys;

        public Ruleset RuleSet { get; }
        public List<string> Letters { get; }
        public SquareTypes SquareTypes { get; }
        public int RackSize { get; }
        public int MaxPlayers { get; }
        public int StackHeight { get; }
        public int DebitPerRemainingTile { get; }

        public override string ToString()
        {
            return RuleSet.ToString();
        }
        public GameRules(Ruleset ruleSet) : this(ruleSet, null) { }

        public GameRules(string name) : this(null, name) { }

        private GameRules(Ruleset? ruleSet, string name)
        {
            if (ruleSet == null)
            {
                ruleSet = GetRulesetFromName(name);
            }

            if (ruleSet == null)
            {
                throw new ArgumentNullException(nameof(ruleSet));
            }

            RuleSet = ruleSet.Value;

            switch (ruleSet)
            {
            case Ruleset.SmallUpwords:
                RackSize = 7;
                MaxPlayers = 4;
                StackHeight = 5;
                DebitPerRemainingTile = 5;
                Letters = "FJKQVWXZBBCCGGHHRRYYDDDLLLMMMNNNPPPSSSUUUIIIIOOOOTTTTAAAAAEEEEEE".Select(c => c == 'Q' ? "Qu" : c + "").ToList();
                SquareTypes = new SquareTypes()
                    .WithRow(1, SquareType.Unplayable)
                    .WithRow(10, SquareType.Unplayable)
                    .WithColumn(1, SquareType.Unplayable)
                    .WithColumn(10, SquareType.Unplayable)
                    .WithSquare(5, 5, SquareType.Starting)
                    .WithSquare(5, 6, SquareType.Starting)
                    .WithSquare(6, 5, SquareType.Starting)
                    .WithSquare(6, 6, SquareType.Starting);
                break;

            case Ruleset.StandardUpwords:
                RackSize = 7;
                MaxPlayers = 4;
                StackHeight = 5;
                DebitPerRemainingTile = 5;
                Letters = "JQVXZKKWWYYBBBFFFGGGHHHPPPCCCCDDDDDLLLLLMMMMMNNNNNRRRRRTTTTTUUUUUSSSSSSAAAAAAAIIIIIIIOOOOOOOEEEEEEEE".Select(c => c == 'Q' ? "Qu" : c + "").ToList();
                SquareTypes = new SquareTypes()
                    .WithSquare(5, 5, SquareType.Starting)
                    .WithSquare(5, 6, SquareType.Starting)
                    .WithSquare(6, 5, SquareType.Starting)
                    .WithSquare(6, 6, SquareType.Starting);
                break;

            case Ruleset.MiniUpwords:
                RackSize = 5;
                MaxPlayers = 4;
                StackHeight = 3;
                DebitPerRemainingTile = 3;
                Letters = "AACDEEFIILLMNOOPRSSTUWY".Select(c => c + "").ToList();
                SquareTypes = GetMinWordBoardLayout();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(ruleSet), $"'{ruleSet}' is not a supported game set.");
            }
        }

        public static Ruleset GetRulesetOrDefault(string name)
        {
            if (_Mapping.TryGetValue(name, out var set)) return set;

            return Ruleset.MiniUpwords;
        }

        private static SquareTypes GetMinWordBoardLayout()
        {
            var result =  new SquareTypes()
                .WithRow(1, SquareType.Unplayable)
                .WithRow(10, SquareType.Unplayable)
                .WithColumn(1, SquareType.Unplayable)
                .WithColumn(10, SquareType.Unplayable)

                .WithSquare(5, 5, SquareType.Starting)
                .WithSquare(5, 6, SquareType.Starting)
                .WithSquare(6, 5, SquareType.Starting)
                .WithSquare(6, 6, SquareType.Starting)
                .WithRowEndsUnplayable(2, SquareType.Unplayable, 4)
                .WithRowEndsUnplayable(3, SquareType.Unplayable, 3)
                .WithRowEndsUnplayable(4, SquareType.Unplayable, 2)
                .WithRowEndsUnplayable(7, SquareType.Unplayable, 2)
                .WithRowEndsUnplayable(8, SquareType.Unplayable, 3)
                .WithRowEndsUnplayable(9, SquareType.Unplayable, 4);

            return result;
        }

        private static Ruleset? GetRulesetFromName(string name)
        {
            foreach (var set in Enum.GetValues(typeof(Ruleset)).Cast<Ruleset>())
            {
                if (set.ToString() == name)
                {
                    return set;
                }
            }

            return null;
        }
    }
}
