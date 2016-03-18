using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HanabiMM
{
    public class Reader
    {
        public IParser Parser;
        public System.IO.TextReader reader;

        public Reader(IParser p, System.IO.TextReader r)
        {
            Parser = p;
            reader = r;
        }

        public IEnumerable<CommandInfo> read()
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                    yield return Parser.Parse(line);
            }
        }

        public IEnumerable<CommandInfo> readFile()
        {
            var smth = System.IO.File.ReadAllLines(@"D:\projects\HanabiMM\input.txt");
            foreach (string line in smth)
            {
                yield return Parser.Parse(line);
            }
        }
    }

    public class Hint
    {
        public List<int> pos;
        public Rank rank;
        public Suit suit;

        public Hint(Rank rank, List<int> storedAtPositions)
        {
            this.rank = rank;
            pos = storedAtPositions;
        }

        public Hint(Suit suit, List<int> storedAtPositions)
        {
            this.suit = suit;
            pos = storedAtPositions;
        }

        public Hint()
        {
            rank = Rank.Zero;
            suit = Suit.Red;
            pos = new List<int>();
        }
    }

    public class CommandInfo
    {
        public int[] cardPositionsInHand;
        public ActionType action;
        public Hint hint;

        public List<string[]> playerCards { get; set; }
        public string[] deckCards { get; set; }

        public CommandInfo()
        {
            playerCards = new List<string[]>();
            cardPositionsInHand = null;
            action = ActionType.StartGame;
            hint = null;
           
        }

        public CommandInfo(int[] i, ActionType a)
        {
            playerCards = new List<string[]>();
            cardPositionsInHand = i;
            action = a;
            hint = null;
        }

        public CommandInfo(ActionType a, Hint h)
        {
            playerCards = new List<string[]>();
            cardPositionsInHand = null;
            action = a;
            hint = h;
        }

        public CommandInfo(List<string[]> playerCards, string[] deckCards)
        {
            this.playerCards = playerCards;
            this.deckCards = deckCards;
            action = ActionType.StartGame;
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
                { "Tell color", ParseColorHint },
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
            return new CommandInfo(new[] { int.Parse(tokens[2]) }, ActionType.Play);
        }

        public CommandInfo ParseDrop(string[] tokens)
        {
            return new CommandInfo(new[] { int.Parse(tokens[2]) }, ActionType.Drop);
        }

        // Tell color Red for cards 0 1 2 3 4
        public CommandInfo ParseColorHint(string[] tokens)
        {
            var color = (Suit)Enum.Parse(typeof(Suit), tokens[2]);
            return new CommandInfo(ActionType.ClueSuit, new Hint(color, GetCardsPositionInHand(tokens).ToList()));
        }

        // Tell rank 1 for cards 2 4
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
