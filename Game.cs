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
        public BitArray suitBits           { get; set; }
        public BitArray rankBits           { get; set; }

        public Card(Suit suit, Rank rank)
        {
            this.suit   = suit;
            this.rank   = rank;
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

        public bool IsValidSuit()
        {
            return (suit != Suit.None);
        }

        public bool IsValidRank()
        {
            return (rank != Rank.Zero);
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

    public class HeldCard : Card
    {
        public HeldCard(Suit suit, Rank rank)
            :base(suit, rank)
        {

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
            return new Card(suit, rank);
        }
    }

    public class Game
    {
        private const int CountCardsOnHand = 5;

        private System.IO.TextWriter log;
        private Deck deck;
        private List<Player> players;
        private Board board;

        private int currentIndexOfPlayer;
        private int risks;
        private int cards;
        private int score;
        private int turn;
        private bool notFinished;
        private int level;


        private bool finished;
        private string pathToFile;
        private string pathOut;

        public void Init()
        {
            players = Enumerable.Range(0, 2).Select(i => new Player(i)).ToList();

            deck = new Deck();
            board = new Board();

            turn = -1;
            finished = notFinished = false;
            cards = 0;
            score = 0;
            risks = 0;
            currentIndexOfPlayer = 1;
        }

        public Player GetCurrentPlayer()
        {
            return players[currentIndexOfPlayer];
        }

        public Player GetNextPlayer()
        {
            return players[NextPlayer()];
        }

        public int NextPlayer()
        {
            var value = (currentIndexOfPlayer + 1) % players.Count;
            return value;
        }

        public Game(int countPlayers, System.IO.TextWriter logger, string path, string pathO, int level = 1)
        {
            log = logger;
            pathToFile = path;
            pathOut = pathO;
            this.level = level;
            Init();
        }

        public IEnumerable<T> GeneratePossibilitiesForTurn<T>(Card card, BitArray bits)
        {
            var result = new List<T>();
            var enumValues = Enum.GetValues(typeof(T)).GetEnumerator();
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
            return possibleSuits.SelectMany(x => possibleRanks, (x, y) => new Card(x, y)).ToList();
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

        public bool StartNewGame(CommandInfo commandInfo)
        {
            var cardConverter = new Converter();

            deck.AddCards(cardConverter.GetCardsFromString(commandInfo.deckCards).ToList());

            for (var i = 0; i < 2; ++i)
                players[i].AddCards(cardConverter.GetCardsFromString(commandInfo.playerCards[i]).ToList());

            return true;
        }

        public void IncreaseGameParameters()
        {
            score = board.GetScore();
            cards = board.GetDepth();
        }

        public bool ProcessPlay(CommandInfo action)
        {
            var player = GetCurrentPlayer();

            var currentCard = player.PlayCard(action.cardPositionsInHand[0]);
            if (board.CardCanPlay(currentCard))
            {
                CheckRisksForCard(currentCard);
                board.AddCard(currentCard);
                IncreaseGameParameters();

                return CanGiveCardToPlayer(player);
            }
            return false;
        }

        public bool ProcessDrop(CommandInfo action)
        {
            var player = GetCurrentPlayer();
            player.DropCard(action.cardPositionsInHand[0]);
            return CanGiveCardToPlayer(player);
        }

        public bool CanGiveCardToPlayer(Player player)
        {
            var topDeckCard = deck.Draw();

            if (topDeckCard != null)
                player.AddCard(topDeckCard);

            return !(deck.IsEmpty() || topDeckCard == null);
        }

        public void DeduceSuitForMainCards(Player player, CommandInfo act)
        {
            foreach (var index in act.hint.pos)
                player.OpenNthSuit(index, act.hint.suit);
        }

        public void DeduceSuitForOtherCards(Player player, Suit suit)
        {
            for (int i = 0; i < player.CountCards(); ++i)
                player.CloseNthSuit(i, suit);
        }

        public void DeduceSuit(Player player, CommandInfo act)
        {
            DeduceSuitForOtherCards(player, act.hint.suit);
            DeduceSuitForMainCards(player, act);
        }

        public void DeduceRankForMainCards(Player player, CommandInfo act)
        {
            foreach (var index in act.hint.pos)
                player.OpenNthRank(index, act.hint.rank);
        }

        public void DeduceRankForOtherCards(Player player, Rank rank)
        {
            for (int i = 0; i < player.CountCards(); ++i)
                player.CloseNthRank(i, rank);
        }

        public void DeduceRank(Player player, CommandInfo act)
        {
            DeduceRankForOtherCards(player, act.hint.rank);
            DeduceRankForMainCards(player, act);
        }

        public bool ProcessColorHint(CommandInfo act)
        {
            var player = GetNextPlayer();
            var suitCardsPositions = player.GetAllPositionsOfSuitCards(act.hint.suit).ToList();
            if (!suitCardsPositions.SequenceEqual(act.hint.pos))
                return false;

            DeduceSuit(player, act);
            return true;
        }

        public bool ProcessRankHint(CommandInfo act)
        {
            var player = GetNextPlayer();
            var rankCardsPositions = player.GetAllPositionsOfRankCards(act.hint.rank).ToList();
            if (!rankCardsPositions.SequenceEqual(act.hint.pos))
                return false;

            DeduceRank(player, act);
            return true;
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
                    notFinished = this.ProcessPlay(ParsedInfo);
                }
                else if (ParsedInfo.action == ActionType.Discard)
                {
                    notFinished = this.ProcessDrop(ParsedInfo);
                }
                else if (ParsedInfo.action == ActionType.Clue)
                {
                    if (ParsedInfo.hint.rank != Rank.Zero)
                    {
                        notFinished = this.ProcessRankHint(ParsedInfo);
                    }
                    else
                        notFinished = this.ProcessColorHint(ParsedInfo);
                }
                else
                {
                    if (notFinished)
                    {
                        Init();
                        turn++;
                    }
                    notFinished = this.StartNewGame(ParsedInfo);
                }
                finished = !notFinished;
                currentIndexOfPlayer = (currentIndexOfPlayer + 1) % 2;
                if (finished)
                {
                    file.WriteLine("Turn: " + turn + ", cards: " + cards + ", with risk: " + risks);
                    Init();
                }

            }
            finished = true;
            file.Close();
        }
    }
};
