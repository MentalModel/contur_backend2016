using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hanabi
{
    public enum ActionType { NoAction, StartGame, Play, Drop, ClueRank, ClueSuit }

    public enum Suit { None, Red, Green, Blue, White, Yellow }

    public enum Rank { Zero, One, Two, Three, Four, Five }

    public enum GameStatus { Continue, Finish }

    public interface IPlayer
    {
        void AddCard(Card card);
        void AddCards(IEnumerable<Card> cards);
    }

    public class HanabiPlayer : IPlayer
    {
        private List<Card> playPile = new List<Card>();

        public IEnumerable<int> GetAllPositionsOfSuit(Suit suit)
        {
            return playPile.Where(w => (w.suit == suit)).Select(w => playPile.IndexOf(w));
        }

        public IEnumerable<int> GetAllPositionsOfRank(Rank rank)
        {
            return playPile.Where(w => (w.rank == rank)).Select(w => playPile.IndexOf(w));
        }

        public Card PlayCard(int cardHandPosition)
        {
            var card = playPile[cardHandPosition];
            playPile.RemoveAt(cardHandPosition);
            return card;
        }

        public Card DropCard(int cardHandPosition)
        {
            return PlayCard(cardHandPosition);
        }

        public IEnumerable<Card> GetPossibleCardsForTurn(Card playCard)
        {
            HeldCard card = playCard as HeldCard;
            return (card == null) ? null : card.possibleSuits.SelectMany(rank => card.possibleRanks, (rank, suit) => new Card(rank, suit));
        }

        public void AddCard(Card card)
        {
            playPile.Add(new HeldCard(card.suit, card.rank));
        }

        public void AddCards(IEnumerable<Card> cards)
        {
            foreach (var card in cards)
                AddCard(card);
        }

        public void UseSuitHint(Hint hint)
        {
            foreach (var index in hint.cardHandPositions)
                ((HeldCard)playPile[index]).OpenSuit(hint.suit);

            foreach (var index in Enumerable.Range(0, playPile.Count()).Except(hint.cardHandPositions))
                ((HeldCard)playPile[index]).ExcludeSuit(hint.suit);
        }

        public void UseRankHint(Hint hint)
        {
            foreach (var index in hint.cardHandPositions)
                ((HeldCard)playPile[index]).OpenRank(hint.rank);

            foreach (var index in Enumerable.Range(0, playPile.Count()).Except(hint.cardHandPositions))
                ((HeldCard)playPile[index]).ExcludeRank(hint.rank);
        }

    }

    public class Hint
    {
        public  int[]   cardHandPositions;
        public  Rank    rank;
        public  Suit    suit;
    }

    public class CommandInfo
    {
        public  int         cardPosition;
        public  ActionType  actionType;
        public  Hint        hint;
        public  List<Card>  cards;
    }

    public interface IParser
    {
        CommandInfo Parse(string s);
    }

    public class Parser : IParser
    {
        private const int   MissNonCards    = 5;
        private const char  Delimiter       = ' ';
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
            foreach (var value in optionsInvoker)
                if (inputString.StartsWith(value.Key))
                    return optionsInvoker[value.Key].Invoke(inputString.Split(Delimiter));
            return null;
        }

        public CommandInfo ParseStartNewGame(string[] tokens)
        {
            var cards = tokens.Skip(MissNonCards).Select(card => new CardValueParser().Parse(card));
            return new CommandInfo { cards = cards.ToList(), actionType = ActionType.StartGame };
        }

        public CommandInfo ParsePlay(string[] tokens)
        {
            return new CommandInfo { cardPosition = int.Parse(tokens[2]), actionType = ActionType.Play };
        }

        public CommandInfo ParseDrop(string[] tokens)
        {
            return new CommandInfo { cardPosition = int.Parse(tokens[2]), actionType = ActionType.Drop };
        }

        public CommandInfo ParseSuitHint(string[] tokens)
        {
            var suit = (Suit)Enum.Parse(typeof(Suit), tokens[2]);
            return new CommandInfo { actionType = ActionType.ClueSuit, hint = new Hint { suit = suit, cardHandPositions = ParseCardPositions(tokens).ToArray() } };
        }

        public CommandInfo ParseRankHint(string[] tokens)
        {
            var rank = (Rank)Enum.Parse(typeof(Rank), tokens[2]);
            return new CommandInfo { actionType = ActionType.ClueRank, hint = new Hint { rank = rank, cardHandPositions = ParseCardPositions(tokens).ToArray() } };
        }

        public IEnumerable<int> ParseCardPositions(string[] tokens)
        {
            return tokens.Skip(MissNonCards).Select(w => int.Parse(w));
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

        public void AddCard(Card card)
        {
            var position = GetIndexFromSuit(card.suit);
            boardCards[position] = card;
        }

        public bool CardCanPlay(Card card)
        {
            var position = GetIndexFromSuit(card.suit);

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
        public List<Suit> possibleSuits { get; private set; }
        public List<Rank> possibleRanks { get; private set; }

        public HeldCard(Suit suit, Rank rank) : base(suit, rank)
        {
            possibleSuits = Enum.GetValues(typeof(Suit)).OfType<Suit>().ToList();
            possibleRanks = Enum.GetValues(typeof(Rank)).OfType<Rank>().ToList();

            possibleSuits.Remove(Suit.None);
            possibleRanks.Remove(Rank.Zero);
        }

        public void OpenSuit(Suit suit)
        {
            possibleSuits.Clear();
            possibleSuits.Add(suit);
        }

        public void OpenRank(Rank rank)
        {
            possibleRanks.Clear();
            possibleRanks.Add(rank);
        }

        public void ExcludeSuit(Suit suit)
        {
            possibleSuits.Remove(suit);
        }

        public void ExcludeRank(Rank rank)
        {
            possibleRanks.Remove(rank);
        }

        public bool IsKnownRank()
        {
            return possibleRanks.Count == 0;
        }

        public bool IsKnownSuit()
        {
            return possibleSuits.Count == 0;
        }

        public bool IsKnownCard()
        {
            return IsKnownSuit() && IsKnownRank();
        }

    }

    public class CardValueParser
    {
        private const string AllSuits = "NRGBWY";

        public Card Parse(string cardRepresent)
        {
            var suit = (Suit)Enum.Parse(typeof(Suit), AllSuits.IndexOf(cardRepresent[0]).ToString());
            var rank = (Rank)Enum.Parse(typeof(Rank), cardRepresent[1].ToString());
            return new Card(suit, rank);
        }
    }

    public class Game
    {
        private const int CountCardsOnHand = 5;
        private List<Card> deck;
        IPlayer player;
        ActionType lastCommand;
        private List<IPlayer> players;
        private IBoard hanabiBoard;
        private int currentIndexOfPlayer, risks, cards, score, turn;

        public Game(IEnumerable<Card> cards)
        {
            Init();

            players[0].AddCards(cards.Take(5));
            players[1].AddCards(cards.Skip(5).Take(5));

            deck.AddRange(cards.Skip(10));
        }

        private void Init()
        {
            hanabiBoard = new HanabiBoard();
            players = new List<IPlayer>();
            AddPlayersToGame();
            deck = new List<Card>();
            cards = score = risks;
            turn = -1;
            currentIndexOfPlayer = 1;
        }

        private void AddPlayersToGame()
        {
            players = new List<IPlayer>();
            for (var i = 0; i < 2; ++i)
                players.Add(new HanabiPlayer());
        }

        private bool NotAllCardsInQueryCanPlay(IEnumerable<Card> query)
        {
            foreach (var card in query)
                if (!hanabiBoard.CardCanPlay(card))
                    return true;
            return false;
        }

        private bool IsRiskyTurn(Card card)
        {
            if (NotAllCardsInQueryCanPlay(((HanabiPlayer)player).GetPossibleCardsForTurn(card).ToList()))
                return true;
            return false;
        }

        public bool LastCommandWasAClue()
        {
            return (lastCommand == ActionType.ClueRank) || (lastCommand == ActionType.ClueSuit);
        }

        public void UpdateGameParameters()
        {
            turn++;
            if (!LastCommandWasAClue())
                NextPlayer();
        }

        private void NextPlayer()
        {
            currentIndexOfPlayer = (currentIndexOfPlayer + 1) % 2;
            player = players[currentIndexOfPlayer];
        }

        private bool IsCorrectHint(CommandInfo commandInfo)
        { 
            List<int> cardsPositions = null;
            if (commandInfo.actionType == ActionType.ClueSuit)
                cardsPositions = ((HanabiPlayer)player).GetAllPositionsOfSuit(commandInfo.hint.suit).ToList();
            else
                cardsPositions = ((HanabiPlayer)player).GetAllPositionsOfRank(commandInfo.hint.rank).ToList();

            if (cardsPositions.SequenceEqual(commandInfo.hint.cardHandPositions))
                return true;

            return false;
        }

        private GameStatus ProcessPlay(int cardPosition)
        {
            var currentCard = ((HanabiPlayer)player).PlayCard(cardPosition);

            if (NoConflictsAfterPlay(currentCard))
            {
                if (IsRiskyTurn(currentCard))
                    risks++;
                hanabiBoard.AddCard(currentCard);
                TakeTopDeckCard();

                if (deck.Count == 0)
                    return GameStatus.Finish;
                return GameStatus.Continue;
            }
            return GameStatus.Finish;
        }

        private bool NoConflictsAfterPlay(Card card)
        {
            return deck.Count > 0 && hanabiBoard.CardCanPlay(card);
        }

        private void TakeTopDeckCard()
        {
            player.AddCard(deck[0]);
            deck.RemoveAt(0);
        }

        private GameStatus ProcessDrop(int cardPosition)
        {
            var card = ((HanabiPlayer)player).DropCard(cardPosition);
            if (NoConflictsAfterDrop(card))
            {
                TakeTopDeckCard();
                return GameStatus.Continue;
            }
            return GameStatus.Finish;
        }

        private bool NoConflictsAfterDrop(Card card)
        {
            return deck.Count >= 2;
        }

        private GameStatus ProcessSuitHint(CommandInfo parsedCommand)
        {
            if (IsCorrectHint(parsedCommand))
            {
                ((HanabiPlayer)player).UseSuitHint(parsedCommand.hint);
                return GameStatus.Continue;
            }
            return GameStatus.Finish;
        }

        private GameStatus ProcessRankHint(CommandInfo parsedCommand)
        {
            if (IsCorrectHint(parsedCommand))
            {
                ((HanabiPlayer)player).UseRankHint(parsedCommand.hint);
                return GameStatus.Continue;
            }
            return GameStatus.Finish;
        }

        public GameStatus Execute(CommandInfo parsedInfo)
        {
            lastCommand = parsedInfo.actionType;
            switch (parsedInfo.actionType)
            {
                case ActionType.StartGame:
                    return GameStatus.Continue;

                case ActionType.Play:
                    return ProcessPlay(parsedInfo.cardPosition);

                case ActionType.Drop:
                    return ProcessDrop(parsedInfo.cardPosition);

                case ActionType.ClueRank:
                    NextPlayer();
                    return ProcessRankHint(parsedInfo);

                case ActionType.ClueSuit:
                    NextPlayer();
                    return ProcessSuitHint(parsedInfo);

                default:
                    return GameStatus.Finish;
            }
        }

        public void PrintStats()
        {
            Console.WriteLine("Turn: " + turn + ", cards: " + hanabiBoard.GetDepth() + ", with risk: " + risks);
        }
    }

    class Runner
    {
        public void Run()
        {
            var parser = new Parser();
            Game game = null;
            string line = null;
            bool finish = false;
            GameStatus gameStatus = GameStatus.Finish;
            do
            {
                line = Console.ReadLine();
                if (line == null)
                    break;
 
                var command = parser.Parse(line);

                if (finish && command.actionType != ActionType.StartGame)
                    continue;

                if (command.actionType == ActionType.StartGame)
                    game = new Game(command.cards);

                gameStatus = game.Execute(command);
                game.UpdateGameParameters();
                finish = gameStatus != GameStatus.Continue;

                if (gameStatus == GameStatus.Finish)
                {
                    game.PrintStats();
                    finish = true;
                }
            } while (line != null);

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            new Runner().Run();
        }
    }
}
