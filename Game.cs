using System;
using System.Collections.Generic;
using System.Linq;

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


    public static class CardExtension
    {
        public static bool isKnownCard(this Card card)
        {
            return (card.isKnownRank) && (card.isKnownSuit);
        }
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
        public int currentPlayer;
        public int risks;
        public int cards;
        public int score = 0;
        public int turn = -1;
        public bool finish = false;
        public bool notFinished = true;

        public  const int CountCardsOnHand = 5;
        private bool finished;

        public Player getCurrentPlayer()
        {
            return players[currentIndexOfPlayer];
        }

        public Player getNextPlayer()
        {
            return players[nextPlayer()];
        }

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
            currentIndexOfPlayer = 1;
            risks = 0;
        }

        public int nextPlayer()
        {
            return (currentIndexOfPlayer + 1) % players.Count;
        }

        public void checkRisks(Card card)
        {
            if (!card.isKnownCard())
                ++risks;
        }

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
            var player = getCurrentPlayer();
            var currentCard = player.playCard(action.cardPositionsInHand[0]);
            if (board.cardCanPlay(currentCard))
            {
                score++;
                cards++;
                if (deck.isEmpty())
                    return false;
                player.addCard(deck.Draw());
                board.addCard(currentCard);
                checkRisks(currentCard);
                if (deck.isEmpty())
                    return false;
                return true;
            }
            return false;  
        }

        public bool processDrop(DataInfo action)
        {
            var player = getCurrentPlayer();
            player.playCard(action.cardPositionsInHand[0]);
            if (deck.isEmpty())
                return false;
            player.addCard(deck.Draw());
            if (deck.isEmpty())
                return false;
            return true;
        }

        public bool processColorHint(DataInfo act)
        {
            var player = getNextPlayer();
            var pile  = player.getPile().ToList();
            var color = pile.Where(w => (w.suit == act.hint.suit)).Select(w => pile.IndexOf(w)).ToList();
            if (color.SequenceEqual(act.hint.pos))
            {
                // open card on user
                foreach (var index in act.hint.pos)
                    player.openNthSuit(index);
                return true;
            }
            return false;
        }

        public bool processRankHint(DataInfo act)
        {
            var player = getNextPlayer();
            var pile = player.getPile().ToList();
            var color = pile.Where(w => (w.rank == act.hint.rank)).Select(w => pile.IndexOf(w)).ToList();
            if (color.SequenceEqual(act.hint.pos))
            {
                // open card on user
                foreach (var index in act.hint.pos)
                    player.openNthRank(index);
                return true;
            }
            return false;
        }


        public void init()
        {
            turn = -1;
            finished = false;
            cards = 0;
            score = 0;
            risks = 0;
            players.Clear();
            players = Enumerable.Range(0, 2).Select(i => new Player(i)).ToList();
            deck = new Deck();
            board = new Board();
            currentIndexOfPlayer = 1;
        }

        public void Run()
        {
            

            string[] lines = System.IO.File.ReadAllLines(@"D:\projects\HanabiMM\input.txt");
            Parser parser = new Parser(); 

            //var reader = new Reader(parser, Console.In);
            //var parsedInfo = reader.readFile();

            //parsedInfo.ToArray();

            
            foreach (string line in lines)
            {
                //Console.WriteLine(line);
                turn++;

                //Console.WriteLine("Turn: " + turn + ", Score: " + score + ", Finished: " + finished);
                //Console.WriteLine("  Current player: " + players[currentIndexOfPlayer].ToString());
                // Console.WriteLine("     Next player: " + players[(currentIndexOfPlayer + 1) % 2].ToString());
                // Console.WriteLine("           Table: " + board);
                //Console.WriteLine("---------------------------------------------");
                if (finished)
                    continue;
                var parsedInfo = parser.parse(line);
                if (parsedInfo.action == ActionType.Play)
                {
                    notFinished = this.processPlay(parsedInfo);
                }
                else if (parsedInfo.action == ActionType.Discard)
                {
                    notFinished = this.processDrop(parsedInfo);
                }
                else if (parsedInfo.action == ActionType.Clue)
                {
                    if (parsedInfo.hint.rank != Rank.Zero)
                    {
                        notFinished = this.processRankHint(parsedInfo);
                    }
                    else
                        notFinished = this.processColorHint(parsedInfo);
                }
                else
                {
                    if (notFinished)
                    {
                       // Console.WriteLine("Turn: " + turn + ", cards: " + cards + ", with risk: " + risks);
                        init();
                        turn++;
                    }
                    notFinished = this.startNewGame(parsedInfo);
                }
                currentIndexOfPlayer = (currentIndexOfPlayer + 1) % 2;
                finished = !notFinished;
                if (finished)
                {
                    Console.WriteLine("Turn: " + turn + ", cards: " + cards + ", with risk: " + risks);
                    //Console.WriteLine("Turn: " + turn + ", Score: " + score + ", Finished: " + finished);
                   // Console.WriteLine("  Current player: " + players[currentIndexOfPlayer].ToString());
                   // Console.WriteLine("     Next player: " + players[(currentIndexOfPlayer + 1) % 2].ToString());
                   // Console.WriteLine("           Table: " + board);
                   // Console.WriteLine("---------------------------------------------");
                    init();
                }
                
            }
            finished = true;


        }

    }

};
