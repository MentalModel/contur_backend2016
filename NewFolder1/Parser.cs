using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HanabiMM
{
    public class Hint
    {
        private readonly int[]  cardHandPositions;
        private readonly Rank   rank;
        private readonly Suit   suit;

        public Rank Rank { get { return rank; } }
        public Suit Suit { get { return suit; } }

        public IEnumerable<int> CardHandPositions
        {
            get { return cardHandPositions; }
        }

        public Hint(Rank rank, IEnumerable<int> storedAtPositions)
        {
            this.rank           = rank;
            suit                = Suit.None;
            cardHandPositions   = storedAtPositions.ToArray();
        }

        public Hint(Suit suit, IEnumerable<int> storedAtPositions)
        {
            rank                = Rank.Zero;
            this.suit           = suit;
            cardHandPositions   = storedAtPositions.ToArray();
        }
    }

    public class CommandInfo
    {
        private readonly int            cardPositionInHand;
        private readonly ActionType     actionType;
        private readonly Hint           hint;
        private readonly List<string[]> playerCards;
        private readonly string[]       deckCards;

        public int                      CardPositionInHand  { get { return cardPositionInHand; } }
        public ActionType               ActionType          { get { return actionType; } }
        public Hint                     Hint                { get { return hint; } }
        public IEnumerable<string[]>    PlayerCards         { get { return playerCards; } }
        public string[]                 DeckCards           { get { return deckCards; } }

        public CommandInfo(int cardPosition, ActionType action)
        {
            cardPositionInHand  = cardPosition;
            actionType          = action;
        }

        public CommandInfo(ActionType action, Hint hintToUser)
        {
            actionType  = action;
            hint        = hintToUser;
        }

        public CommandInfo(IEnumerable<string[]> playerCards, string[] deckCards)
        {
            this.playerCards    = playerCards.ToList();
            this.deckCards      = deckCards;
            actionType          = ActionType.StartGame;
        }
    }

    public interface IParser
    {
        CommandInfo Parse(string s);
    }

    public class Parser : IParser
    {
        private const int STARTS_AT = 5;
        private readonly Dictionary<string, Func<string[], CommandInfo>> optionsInvoker;

        public Parser()
        {
            optionsInvoker = CreateDictionaryOptions();
        }

        public Dictionary<string, Func<string[], CommandInfo>> CreateDictionaryOptions()
        {
            var dictionary = new Dictionary<string, Func<string[], CommandInfo>>
            {
                { "Start",      ParseStartNewGame },
                { "Play",       ParsePlay },
                { "Drop",       ParseDrop },
                { "Tell color", ParseSuitHint },
                { "Tell rank",  ParseRankHint }
            };
            return dictionary;
        }

        public CommandInfo Parse(string inputString)
        {
            if (inputString == null)
                return null;

            foreach (var value in optionsInvoker)
                if (inputString.StartsWith(value.Key))
                {
                    var tokens = inputString.Split(' ');
                    return optionsInvoker[value.Key].Invoke(tokens);
                }
            return null;
        }

        public CommandInfo ParseStartNewGame(string [] tokens)
        {
            int decksCardCount = tokens.Length - 5 * 3;

            string[] firstPlayerCards = new string[5];
            Array.Copy(tokens, 5, firstPlayerCards, 0, 5);

            string[] secondPlayerCards = new string[5];
            Array.Copy(tokens, 10, secondPlayerCards, 0, 5);

            string[] cards = new string[decksCardCount];
            Array.Copy(tokens, 15, cards, 0, decksCardCount);

            return new CommandInfo(new List<string[]> { firstPlayerCards, secondPlayerCards },  cards);
        }

        public CommandInfo ParsePlay(string[] tokens)
        {
            return new CommandInfo(int.Parse(tokens[2]), ActionType.Play);
        }

        public CommandInfo ParseDrop(string[] tokens)
        {
            return new CommandInfo(int.Parse(tokens[2]), ActionType.Drop);
        }

        public CommandInfo ParseSuitHint(string[] tokens)
        {
            var suit = (Suit)Enum.Parse(typeof(Suit), tokens[2]);
            return new CommandInfo(ActionType.ClueSuit, new Hint(suit, GetCardsPositionInHand(tokens).ToList()));
        }

        public CommandInfo ParseRankHint(string[] tokens)
        {
            var color = (Rank)Enum.Parse(typeof(Rank), tokens[2]);
            return new CommandInfo(ActionType.ClueRank, new Hint(color, GetCardsPositionInHand(tokens).ToList()));
        }

        public IEnumerable<int> GetCardsPositionInHand(string[] tokens)
        {
            var result = new List<int>();
            for (int i = STARTS_AT, countTokens = tokens.Length; i < countTokens; ++i)
                result.Add(int.Parse(tokens[i]));
            return result;
        }
    }
}
