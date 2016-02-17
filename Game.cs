using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiMM
{
    public enum ActionType
    {
        NoAction,
        Play,
        Discard,
        Clue
    }

    public enum ClueType
    {
        Rank,
        Suit
    }

    public enum Holder
    {
        Deck,
        Player,
        Board,
        Discard,
    }

    public enum Suit
    {
        Red,
        Green,
        Blue,
        White,
        Yellow
    }

    public enum Rank
    {
        Zero,
        One,
        Two,
        Three,
        Four,
        Five
    }

    public class Card
    {
        public  Suit    suit        { get; set; }
        public  Rank    rank        { get; set; }
        public  Holder  holder      { get; set; }
        public  bool    isKnownSuit { get; set; }
        public  bool    isKnownRank { get; set; }

        public Card(Suit suit, Rank rank, Holder holder)
        {
            this.suit   = suit;
            this.rank   = rank;
            this.holder = holder;
            isKnownSuit = isKnownRank = false;
        }

        public void openSuit()
        {
            isKnownSuit = true;
        }

        public void openRank()
        {
            isKnownRank = true;
        }

        public override string ToString()
        {
            return string.Format("{0}{1}", suit.ToString("G")[0], Convert.ToUInt16(rank));
        }
    }

    public class CardComparer : IEqualityComparer<Card>
    {
        public bool Equals(Card first, Card second)
        {
            return (first.suit == second.suit) && (first.rank == second.rank);
        }

        public int GetHashCode(Card card)
        {
            return card.GetHashCode();
        }
    }

    public class CardRankComparer : IEqualityComparer<Card>
    {
        public bool Equals(Card first, Card second)
        {
            return (first.rank == second.rank);
        }

        public int GetHashCode(Card card)
        {
            return card.GetHashCode();
        }
    }

    public class CardSuitComparer : IEqualityComparer<Card>
    {
        public bool Equals(Card first, Card second)
        {
            return (first.suit == second.suit);
        }

        public int GetHashCode(Card card)
        {
            return card.GetHashCode();
        }
    }
};

namespace HanabiMM
{
    public class Converter
    {
        public Converter()
        {
        }

        public Card getCardFromString(string cardRepresent)
        {
            var all = "RGBWY";
            var suit = (Suit)Enum.Parse(typeof(Suit), all.IndexOf(cardRepresent[0]).ToString());
            var rank = (Rank)Enum.Parse(typeof(Rank), cardRepresent[1].ToString());
            return new Card(suit, rank, Holder.Player);
        }
    }

    public class Game
    {
        private readonly    System.IO.TextWriter    log;
        public              Deck                    deck        { get; private set; }
        public              List<Player>            players     { get; private set; }
        public              int                     Fails       { get; private set; }
        public              int                     Hints       { get; private set; }
        public              int                     FinalTurns  { get; private set; }
        public Board board;
        public int currentIndexOfPlayer;
        public int risks;
        public int cards;
        public int score = 0;

        public  const int CountCardsOnHand = 5; 


        private bool isEndOfTheGame()
        {
            // to do
            if (deck.isEmpty())
                return true;
            if (board.boardIsFull())
                return true;

            return false;
        }

        public Game(int countPlayers, System.IO.TextWriter logger)
        {
            log         = logger;
            deck        = new Deck();
            players     = Enumerable.Range(0, countPlayers).Select(i => new Player(i)).ToList();
            board = new Board();
            currentIndexOfPlayer = 0;
            risks = 0;
        }

        private Card giveCardToPlayer()
        {
            Card draw = deck.Draw();
            if (draw == null)
            {
                log.WriteLine("Deck empty...");
                FinalTurns--;
                return null;
            }
            else
            {
                draw.holder = Holder.Player;
                return draw;
            }
        }

        public int nextPlayer()
        {
            return (currentIndexOfPlayer + 1) % players.Count;
        }

     //   public Card getCurrentPlayerNthCard(int index)
       // {

       // }

        public bool startNewGame(DataInfo action)
        {
            var startGameAction = action;
            var cardConverter = new Converter();
       
            foreach (string value in startGameAction.deckCards)
                deck.addCard(cardConverter.getCardFromString(value));

            foreach (string value in startGameAction.playerCards[0])
                players[0].addCard(cardConverter.getCardFromString(value));

            foreach (string value in startGameAction.playerCards[1])
                players[1].addCard(cardConverter.getCardFromString(value));

            return true;
        }

        public bool processPlay(DataInfo action)
        {
            Card c = players[currentIndexOfPlayer].lookAtCardAtPosition(action.cardPositionsInHand[0]);
            cards++;
            if (board.cardCanPlay(c))
            {
                if (!c.isKnownRank && !c.isKnownSuit)
                    risks++;
                Console.WriteLine("Yes !");
                players[currentIndexOfPlayer].playCard(action.cardPositionsInHand[0]);
                var card = deck.Draw();
                players[currentIndexOfPlayer].addCard(card);
                board.addCard(c);
                score++;
                return true;
            }
            Console.WriteLine("No ! Stop game !!!");
            return false;  
        }

        public bool processDrop(DataInfo abstractAction)
        {
            var action = abstractAction;
            Card c = players[currentIndexOfPlayer].lookAtCardAtPosition(action.cardPositionsInHand[0]);
            Console.WriteLine("Yes ! Drop it !");
            players[currentIndexOfPlayer].playCard(action.cardPositionsInHand[0]);
            var card = deck.Draw();
            players[currentIndexOfPlayer].addCard(card);
            return true;
        }

        public bool processColorHint(DataInfo abstractAction)
        {
            var act                 = abstractAction;
            currentIndexOfPlayer    = nextPlayer();
            var pile                = players[currentIndexOfPlayer].playPile.pile;
            var color = pile.Where(w => w.suit == act.hint.suit).Select(w => pile.IndexOf(w)).ToList();
            if (color.SequenceEqual(act.hint.pos))
            {
                Console.WriteLine("Yes ! Tell color !");
                return true;
            }
            return false;
        }

        public bool processRankHint(DataInfo abstractAction)
        {
            var act = abstractAction;
            currentIndexOfPlayer = nextPlayer();
            var pile = players[currentIndexOfPlayer].playPile.pile;
            var color = pile.Where(w => w.rank == act.hint.rank).Select(w => pile.IndexOf(w)).ToList();
            if (color.SequenceEqual(act.hint.pos))
            {
                Console.WriteLine("Yes ! Tell rank !");
                return true;
            }
            return false;
        }

        public void Run()
        {
            

            string[] lines = System.IO.File.ReadAllLines(@"D:\projects\HanabiMM\input.txt");
            Parser parser = new Parser();
            int turn = -1;
            bool finished = false;

            //var reader = new Reader(parser, Console.In);
            //var parsedInfo = reader.readFile();

            //parsedInfo.ToArray();

            
            foreach (string line in lines)
            {
                //Console.WriteLine(line);
                turn++;
                Console.WriteLine("Turn: " + turn + ", Score: " + score + ", Finished: " + finished);
                Console.WriteLine("  Current player: " + players[currentIndexOfPlayer].ToString());
                Console.WriteLine("     Next player: " + players[(currentIndexOfPlayer + 1) % 2].ToString());
                Console.WriteLine("           Table: " + board);
                Console.WriteLine("---------------------------------------------");
                var parsedInfo = parser.parse(line);
                if (parsedInfo.action == ActionType.Play)
                {
                    this.processPlay(parsedInfo);
                }
                else if (parsedInfo.action == ActionType.Discard)
                {
                    this.processDrop(parsedInfo);
                }
                else if (parsedInfo.action == ActionType.Clue)
                {
                    if (parsedInfo.hint.rank != Rank.Zero)
                    {
                        this.processRankHint(parsedInfo);
                    }
                    else
                        this.processColorHint(parsedInfo);
                }
                else
                    this.startNewGame(parsedInfo);
                    currentIndexOfPlayer = (currentIndexOfPlayer + 1) % 2;
                
            }
            finished = true;

            Console.WriteLine("Turn: " + turn + ", cards: " + cards + ", with risk: " + risks);
            Console.WriteLine("Turn: " + turn + ", Score: " + score + ", Finished: " + finished);
            Console.WriteLine("  Current player: " + players[currentIndexOfPlayer].ToString());
            Console.WriteLine("     Next player: " + players[(currentIndexOfPlayer + 1) % 2].ToString());
            Console.WriteLine("           Table: " + board);
            Console.WriteLine("---------------------------------------------");
        }
    }

};
