using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HanabiMM
{
    public static class HeldCardExtension
    {
        public static bool IsKnownRank(this HeldCard card)
        {
            return GetPossibleRanks(card).ToList().Count == 0;
        }

        public static bool IsKnownColor(this HeldCard card)
        {
            return GetPossibleSuits(card).ToList().Count == 0;
        }

        public static IEnumerable<Suit> GetPossibleSuits(this HeldCard card)
        {
            return GetPossibleCardCharacteristic<Suit>(card.suitBits);
        }

        public static IEnumerable<Rank> GetPossibleRanks(this HeldCard card)
        {
            return GetPossibleCardCharacteristic<Rank>(card.rankBits);
        }

        public static IEnumerable<T> GetPossibleCardCharacteristic<T>(BitArray bitArray)
        {
            var result = new List<T>();
            var enumValues = Enum.GetValues(typeof(T)).GetEnumerator();
            var i = 0;

            enumValues.MoveNext();          // miss first element (Zero or None)
            while (enumValues.MoveNext())
            {
                if (bitArray[i++])
                    result.Add((T)enumValues.Current);
            }
            return result;
        }

        public static bool isKnownCard(this HeldCard card)
        {
            return IsKnownRank(card) && IsKnownColor(card);
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
        public  BitArray     suitBits { get; private set; }
        public  BitArray     rankBits { get; private set; }
        private const int    SuitCount = 5;

        public HeldCard(Suit suit, Rank rank) : base(suit, rank)
        {
            suitBits = new BitArray(SuitCount, true);
            rankBits = new BitArray(SuitCount, true);
        }

        private int GetPositionFromRank(Rank rank)
        {
            return (int)rank - 1;
        }

        private int GetPositionFromSuit(Suit suit)
        {
            return (int)suit - 1;
        }

        public void OpenSuit(Suit suitCard)
        {
            Open(GetPositionFromSuit(suitCard), suitBits);
        }

        public void OpenRank(Rank rankCard)
        {
            Open(GetPositionFromRank(rankCard), rankBits);
        }

        private void Open(int position, BitArray bitArray)
        {
            if (this.IsKnownColor())
                return;
            bitArray.SetAll(false);
            bitArray.Set(position, true);
        }

        public void CloseSuit(Suit suitCard)
        {
            suitBits.Set(GetPositionFromSuit(suitCard), false);
        }

        public void CloseRank(Rank rankCard)
        {
            rankBits.Set(GetPositionFromRank(rankCard), false);
        }
    }

};

namespace HanabiMM
{
    public class Converter
    {
        private const string AllSuits = "NRGBWY";

        public IEnumerable<Card> GetCardsFromString(string [] cards)
        {
            var result = new List<Card>();
            foreach (var card in cards)
                result.Add(GetCardFromString(card));
            return result;
        }

        public Card GetCardFromString(string cardRepresent)
        {
            var suit = (Suit)Enum.Parse(typeof(Suit), AllSuits.IndexOf(cardRepresent[0]).ToString());
            var rank = (Rank)Enum.Parse(typeof(Rank), cardRepresent[1].ToString());
            return new Card(suit, rank);
        }
    }

    public class Game
    {
        private const int       CountCardsOnHand = 5;
        private IDeck           deck;
        private List<Player>    players;
        private IBoard          hanabiBoard;

        private int             currentIndexOfPlayer;
        private int             risks;
        private int             cards;
        private int             score;
        private int             turn;
        private bool            finished;
        private bool            missInput;
        private readonly int    countPlayers;

        private readonly Dictionary<ActionType, Func<CommandInfo, bool>> optionsInvoker;
        

        public Game(int countPlayers)
        {
            this.countPlayers = countPlayers;
            optionsInvoker = CreateDictionaryOptions();
            Init();
        }

        private Dictionary<ActionType, Func<CommandInfo, bool>> CreateDictionaryOptions()
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

        private void Init()
        {
            hanabiBoard     = new HanabiBoard();
            players         = Enumerable.Range(0, countPlayers).Select(i => new Player(i, hanabiBoard)).ToList();
            deck            = new Deck();
            finished        = false;
            missInput       = false;
            cards = score = risks = turn = 0;
            currentIndexOfPlayer = 1;
        }

        private Player GetCurrentPlayer()
        {
            return players[currentIndexOfPlayer];
        }

        private Player GetNextPlayer()
        {
            return players[NextPlayer()];
        }

        private void ChangeTurn()
        {
            currentIndexOfPlayer = (currentIndexOfPlayer + 1) % countPlayers;
        }

        private int NextPlayer()
        {
            var value = (currentIndexOfPlayer + 1) % countPlayers;
            return value;
        }

        private bool StartNewGame(CommandInfo commandInfo)
        {
            Init();
            var cardConverter = new Converter();
            deck.AddCards(cardConverter.GetCardsFromString(commandInfo.DeckCards).ToList());

            for (var i = 0; i < countPlayers; ++i)
                players[i].AddCards(cardConverter.GetCardsFromString(commandInfo.PlayerCards.ToArray()[i]).ToList());

            return false;
        }

        private void IncreaseGameParameters()
        {
            score = hanabiBoard.GetScore();
            cards = hanabiBoard.GetDepth();
        }

        private bool ProcessPlay(CommandInfo parsedCommandionType)
        {
            var player = GetCurrentPlayer();

            var currentCard = player.PlayCard(parsedCommandionType.CardPositionInHand);
            if (hanabiBoard.CardCanPlay(currentCard.Item1))
            {
                if (!currentCard.Item2)
                    risks++;
                hanabiBoard.AddCard(currentCard.Item1);
                IncreaseGameParameters();

                return !(CanGiveCardToPlayer(player) && !((HanabiBoard)hanabiBoard).BoardIsFull());
            }
            return true;
        }

        private bool ProcessDrop(CommandInfo parsedCommandionType)
        {
            var player = GetCurrentPlayer();
            player.DropCard(parsedCommandionType.CardPositionInHand);
            return !CanGiveCardToPlayer(player);
        }

        private bool CanGiveCardToPlayer(Player player)
        {
            var topDeckCard = deck.GetTop();

            if (topDeckCard != null)
                player.AddCard(topDeckCard);

            return !(deck.IsEmpty() || topDeckCard == null);
        }

        private bool ProcessSuitHint(CommandInfo parsedCommand)
        {
            var player = GetNextPlayer();
            var suitCardsPositions = player.GetAllPositionsOfSuit(parsedCommand.Hint.Suit).ToList();
            if (!suitCardsPositions.SequenceEqual(parsedCommand.Hint.CardHandPositions))
                return true;

            player.DeduceSuit(parsedCommand.Hint.Suit, parsedCommand.Hint.CardHandPositions);
            return false;
        }

        private bool ProcessRankHint(CommandInfo parsedCommand)
        {
            var player              = GetNextPlayer();
            var rankCardsPositions  = player.GetAllPositionsOfRank(parsedCommand.Hint.Rank).ToList();
            if (!rankCardsPositions.SequenceEqual(parsedCommand.Hint.CardHandPositions))
                return true;

            player.DeduceRank(parsedCommand.Hint.Rank, parsedCommand.Hint.CardHandPositions);
            return false;
        }

        private bool Execute(CommandInfo parsedInfo)
        {
            foreach (var value in optionsInvoker)
                if (parsedInfo.ActionType == value.Key)
                {
                    if (finished && value.Key != ActionType.StartGame)
                    {
                        missInput = true;
                        return true;
                    }
                    return optionsInvoker[value.Key].Invoke(parsedInfo);
                }
            return false;
        }

        public void Run()
        {
            var parser  = new Parser();
            string line = null;
            do
            {
                turn++;
                line = Console.ReadLine();
                if (line == null)
                    break;

                finished = Execute(parser.Parse(line));

                if (missInput)
                    continue;

                ChangeTurn();
                if (finished)
                    Console.WriteLine("Turn: " + turn + ", cards: " + cards + ", with risk: " + risks);
                } while (line != null);
        }
    }
};
