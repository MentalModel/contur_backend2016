using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HanabiMM
{
    public class Reader
    {
        public IParser parser;
        public System.IO.TextReader reader;

        public Reader(IParser p, System.IO.TextReader r)
        {
            parser = p;
            reader = r;
        }

        public IEnumerable<DataInfo> read()
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                    yield return parser.parse(line);
            }
        }

        public IEnumerable<DataInfo> readFile()
        {
            var smth = System.IO.File.ReadAllLines(@"D:\projects\HanabiMM\input.txt");
            foreach (string line in smth)
            {
                yield return parser.parse(line);
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

    public class DataInfo
    {
        public int[] cardPositionsInHand;
        public ActionType action;
        public Hint hint;
        public string s;

        public List<string[]> playerCards { get; set; }
        public string[] deckCards { get; set; }

        public DataInfo()
        {
            playerCards = new List<string[]>();
            cardPositionsInHand = null;
            action = ActionType.NoAction;
            hint = null;
           
        }

        public DataInfo(int[] i, ActionType a, string s)
        {
            playerCards = new List<string[]>();
            cardPositionsInHand = i;
            action = a;
            hint = null;
            this.s = s;
        }

        public DataInfo(ActionType a, Hint h, string s)
        {
            playerCards = new List<string[]>();
            cardPositionsInHand = null;
            action = a;
            hint = h;
            this.s = s;
        }

        public DataInfo(List<string[]> playerCards, string[] deckCards, string s)
        {
            this.playerCards = playerCards;
            this.deckCards = deckCards;
            this.s = s;
        }
    }

    public interface IParser
    {
        DataInfo parse(string s);
    }

    public class Parser : IParser
    {
        public const int STARTS_AT = 5;
        public Dictionary<string, Func<string, DataInfo>> optionsInvoker;

        public Dictionary<string, Func<string, DataInfo>> createDictionaryOptions()
        {
            var dictionary = new Dictionary<string, Func<string, DataInfo>>
            {
                { "Start",      parseStartNewGame },
                { "Play",       parsePlay },
                { "Drop",       parseDrop },
                { "Tell color", parseColorHint },
                { "Tell rank",  parseRankHint }
            };
            return dictionary;
        }

        public DataInfo parse(string s)
        {
            foreach (var value in optionsInvoker)
                if (s.StartsWith(value.Key))
                    return optionsInvoker[value.Key].Invoke(s);
 
            return null;
        }

        public DataInfo parseStartNewGame(string s)
        {
            var tokens = s.Split(' ');
            int decksCardCount = tokens.Length - 5 * 3;

            string[] firstPlayerCards = new string[5];
            Array.Copy(tokens, 5, firstPlayerCards, 0, 5);

            string[] secondPlayerCards = new string[5];
            Array.Copy(tokens, 10, secondPlayerCards, 0, 5);

            string[] cards = new string[decksCardCount];
            Array.Copy(tokens, 15, cards, 0, decksCardCount);

            return new DataInfo(new List<string[]> { firstPlayerCards, secondPlayerCards },  cards, s );
        }

        public DataInfo parsePlay(string s)
        {
            try
            {
                var tokens = s.Split(' ');
                return new DataInfo(new[] { int.Parse(tokens[2]) }, ActionType.Play, s);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException();
            }
        }

        public DataInfo parseDrop(string s)
        {
            try
            {
                var tokens = s.Split(' ');
                return new DataInfo(new[] { int.Parse(tokens[2]) }, ActionType.Discard, s);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException();
            }
        }

        // Tell color Red for cards 0 1 2 3 4
        public DataInfo parseColorHint(string s)
        {
            try
            {
                var tokens = s.Split(' ');
                var color = (Suit)Enum.Parse(typeof(Suit), tokens[2]);
                return new DataInfo(ActionType.Clue, new Hint(color, getCardsPositionInHand(tokens).ToList()), s);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException();
            }
        }

        // Tell rank 1 for cards 2 4
        public DataInfo parseRankHint(string s)
        {
            try
            {
                var tokens = s.Split(' ');
                var color = (Rank)Enum.Parse(typeof(Rank), tokens[2]);
                return new DataInfo(ActionType.Clue, new Hint(color, getCardsPositionInHand(tokens).ToList()), s);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException();
            }
        }

        public Parser()
        {
            optionsInvoker  = createDictionaryOptions();
        }

        public IEnumerable<int> getCardsPositionInHand(string[] tokens)
        {
            var result = new List<int>();
            try
            {
                for (int i = STARTS_AT, countTokens = tokens.Length; i < countTokens; ++i)
                    result.Add(int.Parse(tokens[i]));
            }
            catch (ArgumentException)
            {
                throw new ArgumentException();
            }
            return result;
        }
    }
}
