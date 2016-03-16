using System;
using System.Collections;
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
        public static bool isKnownRank(this Card card)
        {
            var numberOfPossibleRanks = 0;
            foreach (var v in card.rankBits)
                if ((bool)v)
                    numberOfPossibleRanks++;
            //var numberOfPossibleRanks = card.rankBits.ToString().Select(w => w).Count(w => w.Equals(true));
            return (numberOfPossibleRanks == 1);
        }

        public static bool isKnownColor(this Card card)
        {
            var numberOfPossibleSuits = 0;
            foreach (var v in card.suitBits)
                if ((bool)v)
                    numberOfPossibleSuits++;
            //var t = card.suitBits.ToString();
            //var numberOfPossibleSuits = card.suitBits.ToString().Select(w => w).Count(w => w.Equals(true));
            return (numberOfPossibleSuits == 1);
        }

        public static bool isKnownCard(this Card card)
        {
            return isKnownRank(card) && isKnownColor(card);
        }
    }

    public class Card
    {
        public  Suit    suit        { get; set; }
        public  Rank    rank        { get; set; }
        public  Holder  holder      { get; set; }
        public BitArray suitBits           { get; set; }
        public BitArray rankBits           { get; set; }

        public Card(Suit suit, Rank rank, Holder holder)
        {
            this.suit   = suit;
            this.rank   = rank;
            this.holder = holder;
            suitBits = new BitArray(5, true);
            rankBits = new BitArray(5, true);
        }

        public void openSuit(Suit suitCard)
        {
            if (this.isKnownColor())
                return;
            suitBits.SetAll(false);
            suitBits.Set((int)suitCard, true);
            //isKnownSuit = true;
        }

        public void closeSuit(Suit suitCard)
        {
            //suitBits.SetAll(false);
            suitBits.Set((int)suitCard, false);
            //isKnownSuit = true;
        }

        public void closeRank(Rank rankCard)
        {
            //suitBits.SetAll(false);
            rankBits.Set((int)rankCard - 1, false);
            //isKnownSuit = true;
        }

        public void openRank(Rank rankCard)
        {
            if (this.isKnownRank())
                return;
            rankBits.SetAll(false);
            rankBits.Set((int)rankCard - 1, true);
            //isKnownRank = true;
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
        public int level = 1;

        public  const int CountCardsOnHand = 5;
        private bool finished;
        private string pathToFile;
        private string pathOut;

        public Player getCurrentPlayer()
        {
            return players[currentIndexOfPlayer];
        }

        public int nextPlayer()
        {
            var value = (currentIndexOfPlayer + 1) % players.Count;
            return value;
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

        public Game(int countPlayers, System.IO.TextWriter logger, string path, string pathO, int level = 1)
        {
            log         = logger;
            deck        = new Deck();
            players     = Enumerable.Range(0, countPlayers).Select(i => new Player(i)).ToList();
            board = new Board();
            currentIndexOfPlayer = 1;
            risks = 0;
            pathToFile = path;
            pathOut = pathO;
            this.level = level;
        }


        public void checkRisks(Card card)
        {
            if (level == 1)
                return;

            var possibleColours = new List<int>();
            if (card.isKnownColor())
                possibleColours.Add((int)card.suit);
            else
            {
                for (int i = 0; i < 5; ++i)
                    if (card.suitBits[i])
                        possibleColours.Add(i);
            }
            var possibleRanks= new List<int>();
            if (card.isKnownRank())
                possibleRanks.Add((int)card.rank);
            else
            { 
                for (int i = 0; i < 5; ++i)
                if (card.rankBits[i])
                    possibleRanks.Add(i);
            }
            var query = possibleColours.SelectMany(x => possibleRanks, (x, y) => new { x, y});

            var flag = true;
            foreach (var t in query)
                if (!board.cardCanPlay(new Card((Suit)t.x, (Rank)(t.y), Holder.Player)))
                {
                    flag = false;
                    break;
                }
            if (!flag)
            {
                risks++;
                return;
            }
            return;
            //System.Console.Write(t + " ");
            //  var a = new string[] { "R", "G", "B", "Y", "W" };
            // var b = new string[] { "1", "2", "3", "4", "5" };


            // string[] query = a.SelectMany(x => b, (x, y) => (x + y).ToString()).ToArray();
            //foreach (var t in query)
            //   System.Console.Write(t + " ");

           // if (board.similarRanks() && board.boardCards[0].Peek().rank != Rank.Five)
            //    return;
            // if (card.rank == Rank.One && board.boardCards[(int)card.rank].Peek().rank == Rank.Zero)
           //   {
                   //risks++;
               //   return;
               //}
            if (!card.isKnownCard())
            {
                ++risks;
                return;
            }
   
            //if (!card.isKnownCard())
           //     ++risks;
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
            var player = getCurrentPlayer(); //getNextPlayer(); //
              
            var currentCard = player.playCard(action.cardPositionsInHand[0]);
            if (board.cardCanPlay(currentCard))
            {
                score++;
                cards++;
                if (deck.isEmpty())
                    return false;
                checkRisks(currentCard);
                player.addCard(deck.Draw());
                board.addCard(currentCard);
                
                if (deck.isEmpty())
                    return false;
                return true;
            }
            return false;  
        }

        public bool processDrop(DataInfo action)
        {
            var player = getCurrentPlayer();// getNextPlayer();//getCurrentPlayer();
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
            var player = getNextPlayer();//getCurrentPlayer();// getNextPlayer(); getCurrentPlayer();//
            var pile  = player.getPile().ToList();



            var color = pile.Where(w => (w.suit == act.hint.suit)).Select(w => pile.IndexOf(w)).ToList();
            if (color.SequenceEqual(act.hint.pos))
            {

                for(int i = 0; i < player.playPile.getSize(); ++i)
                    player.closeNthSuit(i, act.hint.suit);

                // open card on user
                foreach (var index in act.hint.pos)
                    player.openNthSuit(index, act.hint.suit);
                return true;
            }
            return false;
        }

        public bool processRankHint(DataInfo act)
        {
            var player = getNextPlayer();//getNextPlayer(); //getCurrentPlayer();//getNextPlayer();
            var pile = player.getPile().ToList();


            var color = pile.Where(w => (w.rank == act.hint.rank)).Select(w => pile.IndexOf(w)).ToList();
            if (color.SequenceEqual(act.hint.pos))
            {

                for (int i = 0; i < player.playPile.getSize(); ++i)
                    player.closeNthRank(i, act.hint.rank);

                // open card on user
                foreach (var index in act.hint.pos)
                    player.openNthRank(index, act.hint.rank);
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
            

            string[] lines = System.IO.File.ReadAllLines(pathToFile);
            Parser parser = new Parser();


            System.IO.StreamWriter file = new System.IO.StreamWriter(pathOut);
            

            

            //var reader = new Reader(parser, Console.In);
            //var parsedInfo = reader.readFile();

            //parsedInfo.ToArray();


            foreach (string line in lines)
            {
                //Console.WriteLine(line);
                turn++;
                //Console.WriteLine(currentIndexOfPlayer);
                var parsedInfo = parser.parse(line);
                //Console.WriteLine(parsedInfo.s);
              //  if (parsedInfo.s =="Tell color Blue for cards 2")
                {
                //    Console.WriteLine("");
                }
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
                
                finished = !notFinished;
                /*file.WriteLine(parsedInfo.s);
                file.WriteLine("Turn: " + turn + ", cards: " + cards + ", with risk: " + risks);

                file.WriteLine("Turn: " + turn + ", Score: " + score + ", Finished: " + finished);
                file.WriteLine("  Current player: " + players[(currentIndexOfPlayer + 1) % 2].ToString());
                file.WriteLine("     Next player: " + players[currentIndexOfPlayer].ToString());
                file.WriteLine("           Table: " + board);
                file.WriteLine("---------------------------------------------");
                */
                currentIndexOfPlayer = (currentIndexOfPlayer + 1) % 2;
                if (finished)
                {

                    file.WriteLine("Turn: " + turn + ", cards: " + cards + ", with risk: " + risks);
                    //Console.WriteLine("Turn: " + turn + ", cards: " + cards + ", with risk: " + risks);
                    
                    
                    //Console.WriteLine("Turn: " + turn + ", Score: " + score + ", Finished: " + finished);
                    //Console.WriteLine("  Current player: " + players[currentIndexOfPlayer].ToString());
                    //Console.WriteLine("     Next player: " + players[(currentIndexOfPlayer + 1) % 2].ToString());
                    //Console.WriteLine("           Table: " + board);
                    //Console.WriteLine("---------------------------------------------");
                    init();
                }
                
            }
            finished = true;

            file.Close();
        }

    }

};
