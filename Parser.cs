using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiMM
{
    public class Parser
    {
        public string[]     tokens;
        public Game         game;
        public const int    STARTS_AT = 5; 

        public Parser(Game newGame)
        {
            game = newGame;
        }

        public AbstractAction parseStartNewGame()
        {
            int decksCardCount = tokens.Length - 5 * 3;
            string[] firstPlayerCards   = new string[5];
            string[] secondPlayerCards  = new string[5];
            string[] decksCards         = new string[decksCardCount];

            Array.Copy(tokens, 5,   firstPlayerCards, 0, 5);
            Array.Copy(tokens, 10,  secondPlayerCards, 0, 5);
            Array.Copy(tokens, 15,  decksCards, 0, decksCardCount);

            return new StartNewGameAction(game, firstPlayerCards, secondPlayerCards, decksCards);
        }

        public AbstractAction parsePlay()
        {
            int cardIndexInHand = int.Parse(tokens[2]);
            return new PlayAction(game, cardIndexInHand);
        }

        public AbstractAction parseDrop()
        {
            int cardIndexInHand = int.Parse(tokens[2]);
            return new DropAction(game, cardIndexInHand);
        }

        public IEnumerable<int> getCardsPositionInHand()
        {
            List<int> result = new List<int>();
            for (int i = STARTS_AT, countTokens = tokens.Length; i < countTokens; ++i)
                result.Add(int.Parse(tokens[i]));
            return result;
        }

        // Tell color Red for cards 0 1 2 3 4
        public AbstractAction parseColorHint()
        {
            var color   = tokens[2];
            var newHint = new Hint((Suit)Enum.Parse(typeof(Suit), color), getCardsPositionInHand().ToList());
            return new HintColorAction(game, newHint);
        }

        // Tell rank 1 for cards 2 4
        public AbstractAction parseRankHint()
        {
            var rank    = tokens[2];
            var newHint = new Hint((Rank)Enum.Parse(typeof(Rank), rank), getCardsPositionInHand().ToList());
            return new HintRankAction(game, newHint);
        }

        public AbstractAction parseInput(string stringToParse)
        {
            tokens = stringToParse.Split(' ');
            switch (tokens[0])
            {
                case "Start":
                    return parseStartNewGame();

                case "Play":
                    return parsePlay();

                case "Drop":
                    return parseDrop();

                case "Tell":
                    if (tokens[1] == "color")
                        return parseColorHint();
                    return parseRankHint();
            }
            return null;
        }
    }
}
