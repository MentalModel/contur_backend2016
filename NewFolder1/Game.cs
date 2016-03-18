using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HanabiMM
{
    public enum ActionType
    {
        NoAction,
        StartGame,
        Play,
        Drop,
        ClueRank,
        ClueSuit
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
        public static bool isKnownRank(this HeldCard card)
        {
            return CountTruePositions(card.rankBits);
        }

        public static bool isKnownColor(this HeldCard card)
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

        public static bool isKnownCard(this HeldCard card)
        {
            return isKnownRank(card) && isKnownColor(card);
        }
    }

    public class Card
    {
        public  Suit    suit        { get; set; }
        public  Rank    rank        { get; set; }

        public Card(Suit suit, Rank rank)
        {
            this.suit   = suit;
            this.rank   = rank;
        }

        public bool IsValidSuit()
        {
            return (suit != Suit.None);
        }

        public bool IsValidRank()
        {
            return (rank != Rank.Zero);
        }

        public override string ToString()
        {
            return string.Format("{0}{1}", suit.ToString("G")[0], Convert.ToUInt16(rank));
        }
    }

    public class HeldCard : Card
    {
        public BitArray suitBits { get; set; }
        public BitArray rankBits { get; set; }

        public HeldCard(Suit suit, Rank rank)
            :base(suit, rank)
        {
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
        private readonly Dictionary<ActionType, Func<CommandInfo, bool>> optionsInvoker;


        private bool finished;
        private string pathToFile;
        private string pathOut;


        public Dictionary<ActionType, Func<CommandInfo, bool>> CreateDictionaryOptions()
        {
            var dictionary = new Dictionary<ActionType, Func<CommandInfo, bool>>
            {
                { ActionType.StartGame, StartNewGame },
                { ActionType.Play,      ProcessPlay },
                { ActionType.ClueRank,  ProcessRankHint },
                { ActionType.ClueSuit,  ProcessSuitHint },
                { ActionType.Drop,      ProcessDrop }
            };
            return dictionary;
        }

        public void Init()
        {
            board = new Board();
            players = Enumerable.Range(0, 2).Select(i => new Player(i, board)).ToList();
            deck = new Deck();
            turn = 0;
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

        public Game(int countPlayers, System.IO.TextWriter logger, string path, string pathO)
        {
            optionsInvoker = CreateDictionaryOptions();
            log = logger;
            pathToFile = path;
            pathOut = pathO;
            Init();
        }

        public bool StartNewGame(CommandInfo commandInfo)
        {
            Init();
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
            if (board.CardCanPlay(currentCard.Item1))
            {
                if (!currentCard.Item2)
                    risks++;
                board.AddCard(currentCard.Item1);
                IncreaseGameParameters();

                return CanGiveCardToPlayer(player) && !board.BoardIsFull();
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
            var topDeckCard = deck.GetTop();

            if (topDeckCard != null)
                player.AddCard(topDeckCard);

            return !(deck.IsEmpty() || topDeckCard == null);
        }

        public bool ProcessSuitHint(CommandInfo act)
        {
            var player = GetNextPlayer();
            var suitCardsPositions = player.GetAllPositionsOfSuitCards(act.hint.suit).ToList();
            if (!suitCardsPositions.SequenceEqual(act.hint.pos))
                return false;

            player.DeduceSuit(act.hint.suit, act.hint.pos);
            return true;
        }

        public bool ProcessRankHint(CommandInfo act)
        {
            var player = GetNextPlayer();
            var rankCardsPositions = player.GetAllPositionsOfRankCards(act.hint.rank).ToList();
            if (!rankCardsPositions.SequenceEqual(act.hint.pos))
                return false;

            player.DeduceRank(act.hint.rank, act.hint.pos);
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

                foreach (var value in optionsInvoker)
                    if (ParsedInfo.action == value.Key)
                        notFinished = optionsInvoker[value.Key].Invoke(ParsedInfo);

                currentIndexOfPlayer = (currentIndexOfPlayer + 1) % 2;
                if (!notFinished)
                    file.WriteLine("Turn: " + turn + ", cards: " + cards + ", with risk: " + risks);
            }
            finished = true;
            file.Close();
        }
    }
};
