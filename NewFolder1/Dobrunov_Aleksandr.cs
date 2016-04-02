using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;

namespace Hanabi
{
    public enum ActionType  { StartGame, Play, Drop, ClueRank, ClueSuit }
    public enum Suit        { Red, Green, Blue, White, Yellow }
    public enum Rank        { Zero, One, Two, Three, Four, Five }
    public enum GameStatus  { Continue, Finish }

    public interface IPlayer
    {
        Card PlayCard(int cardHandPosition);
        Card DropCard(int cardHandPosition);
        void AddCard(Card card);
        void AddCards(IEnumerable<Card> cards);
    }

    public class HanabiPlayer : IPlayer
    {
        private List<Card> playPile;

        public HanabiPlayer(IEnumerable<Card> cards)
        {
            playPile = new List<Card>();
            AddCards(cards);
        }

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
            HanabiCard card = playCard as HanabiCard;
            return (card == null) ? null : card.possibleSuits.SelectMany(rank => card.possibleRanks, (rank, suit) => new Card(rank, suit));
        }

        public void AddCard(Card card)
        {
            playPile.Add(new HanabiCard(card.suit, card.rank));
        }

        public void AddCards(IEnumerable<Card> cards)
        {
            foreach (var card in cards)
                AddCard(card);
        }

        public void UseSuitHint(Hint hint)
        {
            foreach (var index in hint.cardHandPositions)
                ((HanabiCard)playPile[index]).OpenSuit(hint.suit);

            foreach (var index in Enumerable.Range(0, playPile.Count()).Except(hint.cardHandPositions))
                ((HanabiCard)playPile[index]).ExcludeSuit(hint.suit);
        }

        public void UseRankHint(Hint hint)
        {
            foreach (var index in hint.cardHandPositions)
                ((HanabiCard)playPile[index]).OpenRank(hint.rank);

            foreach (var index in Enumerable.Range(0, playPile.Count()).Except(hint.cardHandPositions))
                ((HanabiCard)playPile[index]).ExcludeRank(hint.rank);
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
        CommandInfo Parse(string line);
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
            if (inputString == null)
                return null;
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
        void    AddCard(Card card);
        bool    CardCanPlay(Card card);
        int     GetScore();
        int     GetDepth();
    }

    public class HanabiBoard : IBoard
    {
        private Card[] boardCards;
        private const int SuitCount     = 5;
        private const int MaxCardsCount = 25;

        public HanabiBoard()
        {
            boardCards = new Card[SuitCount];
            InitBoard();
        }

        private void InitBoard()
        {
            for (var suit = Suit.Red; suit <= Suit.Yellow; ++suit)
                boardCards[(int)suit] = new Card(suit, Rank.Zero);
        }

        public void AddCard(Card card)
        {
            boardCards[(int)card.suit] = card;
        }

        public bool CardCanPlay(Card card)
        {
            var topRank = boardCards[(int)card.suit].rank;
            return (topRank + 1) == card.rank;
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
            return GetDepth() == MaxCardsCount;
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

    public class HanabiCard : Card
    {
        public List<Suit> possibleSuits { get; private set; }
        public List<Rank> possibleRanks { get; private set; }

        public HanabiCard(Suit suit, Rank rank) : base(suit, rank)
        {
            possibleSuits = Enum.GetValues(typeof(Suit)).OfType<Suit>().ToList();
            possibleRanks = Enum.GetValues(typeof(Rank)).OfType<Rank>().ToList();
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

    }

    public class CardValueParser
    {
        private const string AllSuits = "RGBWY";

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
        private Stack<Card> deck;
        IPlayer player;
        ActionType lastCommand;
        private List<IPlayer> players;
        private IBoard hanabiBoard;
        private int currentIndexOfPlayer, risks, cards, score, turn;

        public Game(IEnumerable<Card> cards)
        {
            hanabiBoard = new HanabiBoard();
            players     = new List<IPlayer> { new HanabiPlayer(cards.Take(CountCardsOnHand)), new HanabiPlayer(cards.Skip(CountCardsOnHand).Take(CountCardsOnHand)) };
            deck        = new Stack<Card>(cards.Skip(CountCardsOnHand * 2).Reverse());
            this.cards = score = risks;
            turn = -1;
            currentIndexOfPlayer = 0;
            lastCommand = ActionType.StartGame;
        }

        private bool NotAllCardsInQueryCanPlay(IEnumerable<Card> query)
        {
            foreach (var card in query)
                if (!hanabiBoard.CardCanPlay(card))
                    return true;
            return false;
        }

        private void CheckRisks(Card card)
        {
            if (NotAllCardsInQueryCanPlay(((HanabiPlayer)player).GetPossibleCardsForTurn(card).ToList()))
                risks++;
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
            var currentCard = player.PlayCard(cardPosition);
            if (NoConflictsAfterPlay(currentCard))
            {
                CheckRisks(currentCard);   
                hanabiBoard.AddCard(currentCard);
                player.AddCard(deck.Pop());
                return (deck.Count == 0) ? GameStatus.Finish : GameStatus.Continue;
            }
            return GameStatus.Finish;
        }

        private bool NoConflictsAfterPlay(Card card)
        {
            return (deck.Count > 0) && hanabiBoard.CardCanPlay(card);
        }

        private GameStatus ProcessDrop(int cardPosition)
        {
            var card = player.DropCard(cardPosition);
            if (NoConflictsAfterDrop(card))
            {
                player.AddCard(deck.Pop());
                return GameStatus.Continue;
            }
            return GameStatus.Finish;
        }

        private bool NoConflictsAfterDrop(Card card)
        {
            return deck.Count >= 2;
        }

        public delegate void CardHintDelegate(Hint hint);

        public void UseRankHintDelegate(Hint hint)
        {
            ((HanabiPlayer)player).UseRankHint(hint);
        }

        public void UseSuitHintDelegate(Hint hint)
        {
            ((HanabiPlayer)player).UseSuitHint(hint);
        }

        private GameStatus ProcessHint(CommandInfo parsedCommand, CardHintDelegate Task)
        {
            NextPlayer();
            if (IsCorrectHint(parsedCommand))
            {
                Task(parsedCommand.hint);
                return GameStatus.Continue;
            }
            return GameStatus.Finish;
        }

        public GameStatus Execute(CommandInfo parsedInfo)
        {
            UpdateGameParameters();
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
                    return ProcessHint(parsedInfo, UseRankHintDelegate);

                case ActionType.ClueSuit:
                    return ProcessHint(parsedInfo, UseSuitHintDelegate);

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
        private bool finish             = false;
        private Game game               = null;
        private GameStatus gameStatus   = GameStatus.Finish;

        public bool ShouldIgnoreCommand(CommandInfo command)
        {
            return finish && (command.actionType != ActionType.StartGame);
        }

        public bool ShouldStartNewGame(CommandInfo command)
        {
            return command.actionType == ActionType.StartGame;
        }

        public void ProcessFinishGame()
        {
            finish = gameStatus != GameStatus.Continue;
            if (gameStatus == GameStatus.Finish)
            { 
                game.PrintStats();
                finish = true;
            }
        }

        public void Run()
        {
            while (true)
            {
                var command = new Parser().Parse(Console.ReadLine());
                if (command == null)
                    break;

                if (ShouldIgnoreCommand(command))
                    continue;

                if (ShouldStartNewGame(command))
                    game = new Game(command.cards);

                gameStatus = game.Execute(command); 
                ProcessFinishGame();
            }
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