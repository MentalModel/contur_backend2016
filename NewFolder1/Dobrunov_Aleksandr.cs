using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hanabi
{
    public interface IPlayer
    {
        void AddCard(Card card);
        void AddCards(IEnumerable<Card> cards);
        int CountCards();
    }

    public class HanabiPlayer : IPlayer
    {
        private IPile playPile;
        private IBoard hanabiBoard;
        private readonly int name;

        public HanabiPlayer(int name, IBoard playBoard)
        {
            this.name = name;
            playPile = new Pile();
            hanabiBoard = (HanabiBoard)playBoard;
        }

        public Card CardAtPosition(int cardHandPosition)
        {
            return ((Pile)playPile)[cardHandPosition];
        }

        public IEnumerable<int> GetAllPositionsOfSuit(Suit suit)
        {
            var result = playPile.GetCards().ToList();
            return result.Where(w => (w.suit == suit)).Select(w => result.IndexOf(w));
        }

        public IEnumerable<int> GetAllPositionsOfRank(Rank rank)
        {
            var result = ((Pile)playPile).GetCards().ToList();
            return result.Where(w => (w.rank == rank)).Select(w => result.IndexOf(w));
        }

        public Tuple<Card, bool> PlayCard(int cardHandPosition)
        {
            var card = ((Pile)playPile)[cardHandPosition];
            ((Pile)playPile).RemoveCardAtPosition(cardHandPosition);

            return Tuple.Create(card, IsRiskyTurn(card));
        }

        private bool NotAllCardsInQueryCanPlay(IEnumerable<Card> query)
        {
            foreach (var card in query)
                if (!hanabiBoard.CardCanPlay(card))
                    return true;
            return false;
        }

        private bool IsRiskyTurn(Card moveCard)
        {
            HeldCard card = moveCard as HeldCard;
            if (card != null)
            {
                var query = GetPossibleCardsForTurn(card).ToList();
                if (NotAllCardsInQueryCanPlay(query))
                    return false;
                return true;
            }
            return true;
        }

        public Card DropCard(int cardHandPosition)
        {
            return PlayCard(cardHandPosition).Item1;
        }

        public int GetName()
        {
            return name;
        }

        public void AddCard(Card card)
        {
            playPile.AddCard(new HeldCard(card.suit, card.rank));
        }

        public void AddCards(IEnumerable<Card> cards)
        {
            foreach (var card in cards)
                AddCard(card);
        }

        public int CountCards()
        {
            return playPile.Count();
        }

        private void OpenNthRank(int index, Rank rank)
        {
            HeldCard card = ((Pile)playPile)[index] as HeldCard;
            if (card != null)
                card.OpenRank(rank);
        }

        private void CloseNthRank(int index, Rank rank)
        {
            HeldCard card = ((Pile)playPile)[index] as HeldCard;
            if (card != null)
                card.CloseRank(rank);
        }

        private void OpenNthSuit(int index, Suit suitCard)
        {
            HeldCard card = ((Pile)playPile)[index] as HeldCard;
            if (card != null)
                card.OpenSuit(suitCard);
        }

        private void CloseNthSuit(int index, Suit suitCard)
        {
            HeldCard card = ((Pile)playPile)[index] as HeldCard;
            if (card != null)
                card.CloseSuit(suitCard);
        }

        private IEnumerable<Card> GetDecartusMulti(Card turnCard)
        {
            HeldCard card = turnCard as HeldCard;
            if (card == null)
                return null;
            var cardHandPossibleSuits = card.GetPossibleSuits();
            var cardHandPossibleRanks = card.GetPossibleRanks();
            return cardHandPossibleSuits.SelectMany(x => cardHandPossibleRanks, (x, y) => new Card(x, y)).ToList();
        }

        private IEnumerable<Card> GetPossibleCardsForTurn(Card turnCard)
        {
            HeldCard card = turnCard as HeldCard;
            if (card == null)
                return null;

            var query = new List<Card>();
            if (card.IsKnownCard())
                query.Add(card);
            else
                query = GetDecartusMulti(card).ToList();

            return query;
        }

        private void DeduceSuitForMainCards(Suit suit, IEnumerable<int> cardHandPosition)
        {
            foreach (var index in cardHandPosition)
                OpenNthSuit(index, suit);
        }

        private void DeduceSuitForOtherCards(Suit suit, IEnumerable<int> cardHandPosition)
        {
            foreach (var index in cardHandPosition)
                CloseNthSuit(index, suit);
        }

        public void DeduceSuit(Suit suit, IEnumerable<int> cardHandPosition)
        {
            DeduceSuitForOtherCards(suit, Enumerable.Range(0, playPile.Count()).Except(cardHandPosition));
            DeduceSuitForMainCards(suit, cardHandPosition);
        }

        private void DeduceRankForMainCards(Rank rank, IEnumerable<int> cardHandPosition)
        {
            foreach (var index in cardHandPosition)
                OpenNthRank(index, rank);
        }

        private void DeduceRankForOtherCards(Rank rank, IEnumerable<int> cardHandPosition)
        {
            foreach (var index in cardHandPosition)
                CloseNthRank(index, rank);
        }

        public void DeduceRank(Rank rank, IEnumerable<int> cardHandPosition)
        {
            DeduceRankForOtherCards(rank, Enumerable.Range(0, playPile.Count()).Except(cardHandPosition));
            DeduceRankForMainCards(rank, cardHandPosition);
        }

        public override string ToString()
        {
            return string.Format("{0}", playPile);
        }
    }

    public interface IPile
    {
        void AddCard(Card newCard);
        void AddCards(IEnumerable<Card> newCards);
        int Count();
        IEnumerable<Card> GetCards();
    }

    public class Pile : IPile
    {
        private List<Card> pile;
        private const string Delimiter = " ";

        public Pile()
        {
            pile = new List<Card>();
        }

        public void AddCard(Card newCard)
        {
            pile.Add(newCard);
        }

        public void AddCards(IEnumerable<Card> newCards)
        {
            foreach (var card in newCards)
                AddCard(card);
        }

        public int Count()
        {
            return pile.Count;
        }

        private bool IsValidPosition(int position)
        {
            return (position >= 0) && (position < pile.Count);
        }

        public Card this[int cardHandPosition]
        {
            get
            {
                if (IsValidPosition(cardHandPosition))
                    return pile[cardHandPosition];
                return null;
            }
        }

        public void RemoveCardAtPosition(int cardHandPosition)
        {
            if (IsValidPosition(cardHandPosition))
                pile.RemoveAt(cardHandPosition);
        }

        public IEnumerable<Card> GetCards()
        {
            return pile;
        }

        public override string ToString()
        {
            string result = "";
            foreach (var card in pile)
                result += card + Delimiter;

            return string.Format("{0}", result);
        }
    }

    public class Hint
    {
        private readonly int[] cardHandPositions;
        private readonly Rank rank;
        private readonly Suit suit;

        public Rank Rank { get { return rank; } }
        public Suit Suit { get { return suit; } }

        public IEnumerable<int> CardHandPositions
        {
            get { return cardHandPositions; }
        }

        public Hint(Rank rank, IEnumerable<int> storedAtPositions)
        {
            this.rank = rank;
            suit = Suit.None;
            cardHandPositions = storedAtPositions.ToArray();
        }

        public Hint(Suit suit, IEnumerable<int> storedAtPositions)
        {
            rank = Rank.Zero;
            this.suit = suit;
            cardHandPositions = storedAtPositions.ToArray();
        }
    }

    public class CommandInfo
    {
        private readonly int cardPositionInHand;
        private readonly ActionType actionType;
        private readonly Hint hint;
        private readonly List<string[]> playerCards;
        private readonly string[] deckCards;

        public int CardPositionInHand { get { return cardPositionInHand; } }
        public ActionType ActionType { get { return actionType; } }
        public Hint Hint { get { return hint; } }
        public IEnumerable<string[]> PlayerCards { get { return playerCards; } }
        public string[] DeckCards { get { return deckCards; } }

        public CommandInfo(int cardPosition, ActionType action)
        {
            cardPositionInHand = cardPosition;
            actionType = action;
        }

        public CommandInfo(ActionType action, Hint hintToUser)
        {
            actionType = action;
            hint = hintToUser;
        }

        public CommandInfo(IEnumerable<string[]> playerCards, string[] deckCards)
        {
            this.playerCards = playerCards.ToList();
            this.deckCards = deckCards;
            actionType = ActionType.StartGame;
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
                { "Tell color", ParseSuitHint },
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

        public CommandInfo ParseStartNewGame(string[] tokens)
        {
            int decksCardCount = tokens.Length - 5 * 3;

            string[] firstPlayerCards = new string[5];
            Array.Copy(tokens, 5, firstPlayerCards, 0, 5);

            string[] secondPlayerCards = new string[5];
            Array.Copy(tokens, 10, secondPlayerCards, 0, 5);

            string[] cards = new string[decksCardCount];
            Array.Copy(tokens, 15, cards, 0, decksCardCount);

            return new CommandInfo(new List<string[]> { firstPlayerCards, secondPlayerCards }, cards);
        }

        public CommandInfo ParsePlay(string[] tokens)
        {
            return new CommandInfo(int.Parse(tokens[2]), ActionType.Play);
        }

        public CommandInfo ParseDrop(string[] tokens)
        {
            return new CommandInfo(int.Parse(tokens[2]), ActionType.Drop);
        }

        public CommandInfo ParseSuitHint(string[] tokens)
        {
            var suit = (Suit)Enum.Parse(typeof(Suit), tokens[2]);
            return new CommandInfo(ActionType.ClueSuit, new Hint(suit, GetCardsPositionInHand(tokens).ToList()));
        }

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

    public interface IDeck
    {
        void AddCard(Card card);
        void AddCards(IEnumerable<Card> cards);
        bool IsEmpty();
        Card GetTop();
    }

    public class Deck : IDeck
    {
        private List<Card> cards;

        public Deck()
        {
            cards = new List<Card>();
        }

        public Card GetTop()
        {
            if (cards.Count == 0)
                return null;

            var card = cards[0];
            cards.RemoveAt(0);
            return card;
        }

        public void AddCard(Card card)
        {
            cards.Add(card);
        }

        public void AddCards(IEnumerable<Card> cards)
        {
            foreach (var card in cards)
                AddCard(card);
        }

        public bool IsEmpty()
        {
            return (cards.Count == 0);
        }
    }

    public interface IBoard
    {
        void AddCard(Card card);
        bool CardCanPlay(Card card);
        int GetScore();
        int GetDepth();
    }

    public class HanabiBoard : IBoard
    {
        private Card[] boardCards;
        private const string Delimiter = " ";
        private const int SuitCount = 5;
        private const int MaxCardsCount = 25;

        public HanabiBoard()
        {
            boardCards = new Card[SuitCount];
            InitBoard();
        }

        private void InitBoard()
        {
            for (var suit = Suit.Red; suit <= Suit.Yellow; ++suit)
                boardCards[GetIndexFromSuit(suit)] = new Card(suit, Rank.Zero);
        }

        private int GetIndexFromSuit(Suit suit)
        {
            return ((int)suit - 1);
        }

        private bool IsValidBoardPosition(int position)
        {
            return (position >= 0) && (position < boardCards.Length);
        }

        public void AddCard(Card card)
        {
            var position = GetIndexFromSuit(card.suit);
            if (IsValidBoardPosition(position))
                boardCards[position] = card;
        }

        public bool CardCanPlay(Card card)
        {
            var position = GetIndexFromSuit(card.suit);
            if (!IsValidBoardPosition(position))
                return false;

            var topRank = boardCards[position].rank;
            return (topRank + 1 == card.rank);
        }

        public int GetScore()
        {
            return GetDepth();
        }

        public int GetDepth()
        {
            return boardCards
                .Select(card => card.rank)
                .Sum(c => (int)c);
        }

        public bool BoardIsFull()
        {
            return (GetDepth() == MaxCardsCount);
        }

        public override string ToString()
        {
            string result = "";
            foreach (var card in boardCards)
                result += card + Delimiter;

            return string.Format("{0}", result);
        }
    }

    public class Card
    {
        public readonly Suit suit;
        public readonly Rank rank;

        public Card(Suit suit, Rank rank)
        {
            this.suit = suit;
            this.rank = rank;
        }
    }

    public class HeldCard : Card
    {
        public BitArray suitBits { get; private set; }
        public BitArray rankBits { get; private set; }
        private const int SuitCount = 5;

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

        public static bool IsKnownCard(this HeldCard card)
        {
            return IsKnownRank(card) && IsKnownColor(card);
        }
    }

    public class CardConverter
    {
        private const string AllSuits = "NRGBWY";

        public IEnumerable<Card> GetCardsFromString(string[] cards)
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
        private const int CountCardsOnHand = 5;
        private IDeck deck;
        private List<IPlayer> players;
        private IBoard hanabiBoard;

        private int currentIndexOfPlayer;
        private int risks;
        private int cards;
        private int score;
        private int turn;
        private bool finished;
        private bool missInput;
        private readonly int countPlayers;

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
            hanabiBoard = new HanabiBoard();
            players = new List<IPlayer>();
            AddPlayersToGame();
            deck = new Deck();
            finished = false;
            missInput = false;
            cards = score = risks = turn = 0;
            currentIndexOfPlayer = 1;
        }

        private void AddPlayersToGame()
        {
            players = new List<IPlayer>();
            for (var i = 0; i < countPlayers; ++i)
                players.Add(new HanabiPlayer(i, hanabiBoard));
        }

        private IPlayer GetCurrentPlayer()
        {
            return (HanabiPlayer)players[currentIndexOfPlayer];
        }

        private IPlayer GetNextPlayer()
        {
            return (HanabiPlayer)players[NextPlayer()];
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
            var cardConverter = new CardConverter();
            deck.AddCards(cardConverter.GetCardsFromString(commandInfo.DeckCards).ToList());

            for (var i = 0; i < countPlayers; ++i)
                players[i].AddCards(cardConverter.GetCardsFromString(commandInfo.PlayerCards.ToArray()[i]).ToList());

            return false;
        }

        private void UpdateGameParameters()
        {
            score = hanabiBoard.GetScore();
            cards = hanabiBoard.GetDepth();
        }

        private bool ProcessPlay(CommandInfo parsedCommandionType)
        {
            var player = GetCurrentPlayer();

            var currentCard = ((HanabiPlayer)player).PlayCard(parsedCommandionType.CardPositionInHand);
            if (hanabiBoard.CardCanPlay(currentCard.Item1))
            {
                if (!currentCard.Item2)
                    risks++;
                hanabiBoard.AddCard(currentCard.Item1);
                UpdateGameParameters();

                return !(CanGiveCardToPlayer((HanabiPlayer)player) && !((HanabiBoard)hanabiBoard).BoardIsFull());
            }
            return true;
        }

        private bool ProcessDrop(CommandInfo parsedCommandionType)
        {
            var player = (HanabiPlayer)GetCurrentPlayer();
            player.DropCard(parsedCommandionType.CardPositionInHand);
            return !CanGiveCardToPlayer(player);
        }

        private bool CanGiveCardToPlayer(HanabiPlayer player)
        {
            var topDeckCard = deck.GetTop();

            if (topDeckCard != null)
                player.AddCard(topDeckCard);

            return !(deck.IsEmpty() || topDeckCard == null);
        }

        private bool ProcessSuitHint(CommandInfo parsedCommand)
        {
            var player = GetNextPlayer();
            var suitCardsPositions = ((HanabiPlayer)player).GetAllPositionsOfSuit(parsedCommand.Hint.Suit).ToList();
            if (!suitCardsPositions.SequenceEqual(parsedCommand.Hint.CardHandPositions))
                return true;

            ((HanabiPlayer)player).DeduceSuit(parsedCommand.Hint.Suit, parsedCommand.Hint.CardHandPositions);
            return false;
        }

        private bool ProcessRankHint(CommandInfo parsedCommand)
        {
            var player = GetNextPlayer();
            var rankCardsPositions = ((HanabiPlayer)player).GetAllPositionsOfRank(parsedCommand.Hint.Rank).ToList();
            if (!rankCardsPositions.SequenceEqual(parsedCommand.Hint.CardHandPositions))
                return true;

            ((HanabiPlayer)player).DeduceRank(parsedCommand.Hint.Rank, parsedCommand.Hint.CardHandPositions);
            return false;
        }

        private bool ShouldMissCommand(ActionType action)
        {
            if (finished && action != ActionType.StartGame)
            {
                missInput = true;
                return true;
            }
            return false;
        }

        private bool Execute(CommandInfo parsedInfo)
        {
            foreach (var value in optionsInvoker)
            {
                if (parsedInfo.ActionType == value.Key)
                {
                    if (ShouldMissCommand(value.Key))
                        return true;
                    return optionsInvoker[value.Key].Invoke(parsedInfo);
                }
            }
            return false;
        }

        public void Run()
        {
            var parser = new Parser();
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

    public enum ActionType { NoAction, StartGame, Play, Drop, ClueRank, ClueSuit }

    public enum Suit { None, Red, Green, Blue, White, Yellow }

    public enum Rank { Zero, One, Two, Three, Four, Five }

    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game(2);
            game.Run();
        }
    }
}
