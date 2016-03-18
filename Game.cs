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
        None = 0,
        Red,
        Green,
        Blue,
        White,
        Yellow
    }

    public enum Rank
    {
        Zero = 0,
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
            return CountTruePositions(card.rankBits);
        }

        public static bool isKnownColor(this Card card)
        {
            return CountTruePositions(card.suitBits);
        }

        public static bool CountTruePositions(BitArray bitArray)
        {
            var countElementsEqualTrue = 0;
            foreach (bool bit in bitArray)
                if (bit)
                    countElementsEqualTrue++;
            return (countElementsEqualTrue == 1);
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
            suitBits.Set((int)suitCard - 1, true);
        }

        public void closeSuit(Suit suitCard)
        {
            suitBits.Set((int)suitCard - 1, false);
        }

        public void closeRank(Rank rankCard)
        {
            rankBits.Set((int)rankCard - 1, false);
        }

        public void openRank(Rank rankCard)
        {
            if (this.isKnownRank())
                return;
            rankBits.SetAll(false);
            rankBits.Set((int)rankCard - 1, true);
        }

        public override string ToString()
        {
            return string.Format("{0}{1}", suit.ToString("G")[0], Convert.ToUInt16(rank));
        }
    }

};

namespace HanabiMM
{
    public class Converter
    {
        private string ALL = "NRGBWY";

        public Converter()
        {
        }

        public IEnumerable<Card> GetCardsFromString(string [] cards)
        {
            var result = new List<Card>();
            foreach (var card in cards)
                result.Add(GetCardFromString(card));
            return result;
        }

        public Card GetCardFromString(string cardRepresent)
        {
            var suit = (Suit)Enum.Parse(typeof(Suit), ALL.IndexOf(cardRepresent[0]).ToString());
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
            if (deck.IsEmpty())
                return true;
            if (board.IsFull())
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

        public IEnumerable<T> GeneratePossibilitiesForTurn<T>(Card card, BitArray bits)
        {
            var result      = new List<T>();
            var enumValues  = Enum.GetValues(typeof(T)).GetEnumerator();
            var i = 0;

            enumValues.MoveNext();          // miss first element (Zero or None)
            while (enumValues.MoveNext())
            {
                if (bits[i++])
                    result.Add((T)enumValues.Current);
            }
            return result;
        }

        public IEnumerable<Card> GetDecartusMulti(Card card)
        {
            var possibleSuits = GeneratePossibilitiesForTurn<Suit>(card, card.suitBits);
            var possibleRanks = GeneratePossibilitiesForTurn<Rank>(card, card.rankBits);
            return possibleSuits.SelectMany(x => possibleRanks, (x, y) => new Card(x, y, Holder.Board)).ToList();
        }

        public IEnumerable<Card> GetPossibleCardsForTurn(Card card)
        {
            var query = new List<Card>();
            if (card.isKnownCard())
                query.Add(card);
            else
                query = GetDecartusMulti(card).ToList();

            return query;
        }

        public bool NotAllCardsInQueryCanPlay(List<Card> query)
        {
            foreach (var card in query)
                if (!board.CardCanPlay(card))
                    return true;
            return false;
        }

        public void CheckRisksForCard(Card card)
        {
            if (level == 1) return;

            var query = GetPossibleCardsForTurn(card).ToList();
            if (NotAllCardsInQueryCanPlay(query))
                risks++;
        }

        public bool startNewGame(CommandInfo commandInfo)
        {
            var cardConverter = new Converter();

            deck.AddCards(cardConverter.GetCardsFromString(commandInfo.deckCards).ToList());

            for (var i = 0; i < 2; ++i)
                players[i].AddCards(cardConverter.GetCardsFromString(commandInfo.playerCards[i]).ToList());

            return true;
        }

        public bool processPlay(CommandInfo action)
        {
            var player = getCurrentPlayer();
              
            var currentCard = player.PlayCard(action.cardPositionsInHand[0]);
            if (board.CardCanPlay(currentCard))
            {
                score++;
                cards++;
                if (deck.IsEmpty())
                    return false;
                CheckRisksForCard(currentCard);
                player.AddCard(deck.Draw());
                board.AddCard(currentCard);
                
                if (deck.IsEmpty())
                    return false;
                return true;
            }
            return false;  
        }

        public bool processDrop(CommandInfo action)
        {
            var player = getCurrentPlayer();
            player.DropCard(action.cardPositionsInHand[0]);

            if (deck.IsEmpty())
                return false;

            player.AddCard(deck.Draw());

            if (deck.IsEmpty())
                return false;

            return true;
        }

        public bool processColorHint(CommandInfo act)
        {
            var player = getNextPlayer();//getCurrentPlayer();// getNextPlayer(); getCurrentPlayer();//
            var pile  = player.GetPile().ToList();

            var color = pile.Where(w => (w.suit == act.hint.suit)).Select(w => pile.IndexOf(w)).ToList();
            if (color.SequenceEqual(act.hint.pos))
            {

                for(int i = 0; i < player.CountCards(); ++i)
                    player.CloseNthSuit(i, act.hint.suit);

                // open card on user
                foreach (var index in act.hint.pos)
                    player.OpenNthSuit(index, act.hint.suit);
                return true;
            }
            return false;
        }

        public bool processRankHint(CommandInfo act)
        {
            var player = getNextPlayer();//getNextPlayer(); //getCurrentPlayer();//getNextPlayer();
            var pile = player.GetPile().ToList();


            var color = pile.Where(w => (w.rank == act.hint.rank)).Select(w => pile.IndexOf(w)).ToList();
            if (color.SequenceEqual(act.hint.pos))
            {

                for (int i = 0; i < player.CountCards(); ++i)
                    player.CloseNthRank(i, act.hint.rank);

                // open card on user
                foreach (var index in act.hint.pos)
                    player.OpenNthRank(index, act.hint.rank);
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
            Parser Parser = new Parser();


            System.IO.StreamWriter file = new System.IO.StreamWriter(pathOut);

            //var reader = new Reader(Parser, Console.In);
            //var ParsedInfo = reader.readFile();

            //ParsedInfo.ToArray();


            foreach (string line in lines)
            {
                turn++;
                var ParsedInfo = Parser.Parse(line);

                if (ParsedInfo.action == ActionType.Play)
                {
                    notFinished = this.processPlay(ParsedInfo);
                }
                else if (ParsedInfo.action == ActionType.Discard)
                {
                    notFinished = this.processDrop(ParsedInfo);
                }
                else if (ParsedInfo.action == ActionType.Clue)
                {
                    if (ParsedInfo.hint.rank != Rank.Zero)
                    {
                        notFinished = this.processRankHint(ParsedInfo);
                    }
                    else
                        notFinished = this.processColorHint(ParsedInfo);
                }
                else
                {
                    if (notFinished)
                    {
                        init();
                        turn++;
                    }
                    notFinished = this.startNewGame(ParsedInfo);
                }
                finished = !notFinished;
                currentIndexOfPlayer = (currentIndexOfPlayer + 1) % 2;
                if (finished)
                {
                    file.WriteLine("Turn: " + turn + ", cards: " + cards + ", with risk: " + risks);
                    init();
                }
                
            }
            finished = true;

            file.Close();
        }

    }

};
