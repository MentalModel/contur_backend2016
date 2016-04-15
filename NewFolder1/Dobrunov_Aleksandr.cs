using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;

namespace TestGameHanabi
{
    using Hanabi;
    using NUnit.Framework;

    [TestFixture]
    public class HanabiBoardTester
    {
        public IEnumerable<Card> GenerateAllCards()
        {
            for (Suit suit = Suit.Red; suit <= Suit.Yellow; ++suit)
                for (Rank rank = Rank.One; rank <= Rank.Five; ++rank)
                    yield return new Card(suit, rank);
        }

        public HanabiBoard GetFullBoard()
        {
            var board = new HanabiBoard();
            foreach (var card in GenerateAllCards())
                board.AddCard(card);
            return board;
        }

        public IEnumerable<Card> GetCardsWithSuitStartAtRank(Suit suit, Rank rank)
        {
            for (var rankOfCard = rank; rankOfCard <= Rank.Five; ++rankOfCard)
                yield return new Card(suit, rankOfCard);
        }

        public IEnumerable<Card> GetCardsWithSuitEndAtRank(Suit suit, Rank rank)
        {
            for (var rankOfCard = Rank.One; rankOfCard <= rank; ++rankOfCard)
                yield return new Card(suit, rankOfCard);
        }

        public HanabiBoard GetBoardWithSuitCardsUpToRank(Suit suit, Rank rank)
        {
            var board = new HanabiBoard();
            foreach (var card in GetCardsWithSuitEndAtRank(suit, rank))
                board.AddCard(card);
            return board;
        }

        [Test]
        public void TestFullBoard()
        {
            var board = GetFullBoard();
            Assert.AreEqual(board.BoardIsFull(), true);
        }

        [Test]
        public void TestCountCards()
        {
            var board = GetFullBoard();
            Assert.AreEqual(board.CountCards(), 25);
        }

        public void TestCardNextPlay(Suit suit, Rank rank)
        {
            var board = GetBoardWithSuitCardsUpToRank(suit, rank);
            Assert.AreEqual(board.CardCanPlay(new Card(suit, rank + 1)), true);

            for (var r = rank + 2; r <= Rank.Five; ++r)
                Assert.AreEqual(board.CardCanPlay(new Card(suit, r)), false);
        }

        [Test]
        public void TestRedCardNextPlay()
        {
            TestCardNextPlay(Suit.Red, Rank.One);
            TestCardNextPlay(Suit.Red, Rank.Two);
            TestCardNextPlay(Suit.Red, Rank.Three);
            TestCardNextPlay(Suit.Red, Rank.Four);
            TestCardNextPlay(Suit.Red, Rank.Five);
        }

        [Test]
        public void TestGreenCardNextPlay()
        {
            TestCardNextPlay(Suit.Green, Rank.One);
            TestCardNextPlay(Suit.Green, Rank.Two);
            TestCardNextPlay(Suit.Green, Rank.Three);
            TestCardNextPlay(Suit.Green, Rank.Four);
            TestCardNextPlay(Suit.Green, Rank.Five);
        }

        [Test]
        public void TestYellowCardNextPlay()
        {
            TestCardNextPlay(Suit.Yellow, Rank.One);
            TestCardNextPlay(Suit.Yellow, Rank.Two);
            TestCardNextPlay(Suit.Yellow, Rank.Three);
            TestCardNextPlay(Suit.Yellow, Rank.Four);
            TestCardNextPlay(Suit.Yellow, Rank.Five);
        }

        [Test]
        public void TestBlueCardNextPlay()
        {
            TestCardNextPlay(Suit.Blue, Rank.One);
            TestCardNextPlay(Suit.Blue, Rank.Two);
            TestCardNextPlay(Suit.Blue, Rank.Three);
            TestCardNextPlay(Suit.Blue, Rank.Four);
            TestCardNextPlay(Suit.Blue, Rank.Five);
        }

        [Test]
        public void TestWhiteCardNextPlay()
        {
            TestCardNextPlay(Suit.White, Rank.One);
            TestCardNextPlay(Suit.White, Rank.Two);
            TestCardNextPlay(Suit.White, Rank.Three);
            TestCardNextPlay(Suit.White, Rank.Four);
            TestCardNextPlay(Suit.White, Rank.Five);
        }

    }

    public class CardRankComparer : Comparer<Card>
    {
        public override int Compare(Card first, Card second)
        {
            return first.rank.CompareTo(second.rank);
        }
    }

    public class CardSuitComparer : Comparer<Card>
    {
        public override int Compare(Card first, Card second)
        {
            return first.suit.CompareTo(second.suit);
        }
    }

    public class CardComparer : Comparer<Card>
    {
        public override int Compare(Card first, Card second)
        {
            if (first.suit != second.suit)
                return -1;

            if (first.rank < second.rank)
                return -1;

            else if (first.rank > second.rank)
                return 1;

            return 0;
        }
    }

    [TestFixture]
    public class ParserTester
    {
        [Test]
        public void TestParseNewGame()
        {
            var command = new Parser().Parse("Start new game with deck R1 R2 R3 R4 R5 R1 R2 R3 R4 R5 R1 R2");

            var cards = new List<Card>();
            for (var i = 0; i < 2; ++i)
                for (var rank = Rank.One; rank <= Rank.Five; ++rank)
                    cards.Add(new Card(Suit.Red, rank));

            cards.Add(new Card(Suit.Red, Rank.One));
            cards.Add(new Card(Suit.Red, Rank.Two));

            CollectionAssert.AreEqual(cards, command.cards, new CardRankComparer());
            Assert.AreEqual(ActionType.StartGame, command.actionType);
        }

        public void TestParsePlayCardNo(int index)
        {
            var command = new Parser().Parse("Play card " + index.ToString());
            Assert.AreEqual(index, command.cardPosition);
            Assert.AreEqual(ActionType.Play, command.actionType);
        }

        public void TestParseDropCardNo(int index)
        {
            var command = new Parser().Parse("Drop card " + index.ToString());
            Assert.AreEqual(index, command.cardPosition);
            Assert.AreEqual(ActionType.Drop, command.actionType);
        }

        public void TestParseSuitHint(Suit suit, int[] position)
        {
            string[] colors = { "Red", "Green", "Blue", "White", "Yellow" };
            var command = new Parser().Parse("Tell color " + colors[(int)suit] + " for cards " + string.Join(" ", position));

            Assert.AreEqual(suit, command.hint.suit);
            Assert.AreEqual(position, command.hint.cardHandPositions);
            Assert.AreEqual(ActionType.Clue, command.actionType);
        }

        public void TestParseRankHint(Rank rank, int[] position)
        {
            var command = new Parser().Parse("Tell rank " + (int)rank + " for cards " + string.Join(" ", position));

            Assert.AreEqual(rank, command.hint.rank);
            Assert.AreEqual(position, command.hint.cardHandPositions);
            Assert.AreEqual(ActionType.Clue, command.actionType);
        }

        [Test]
        public void TestParseRankHintWithPosition()
        {
            TestParseRankHint(Rank.One, new int[] { 1 });
            TestParseRankHint(Rank.Two, new int[] { 2, 3, 4 });
            TestParseRankHint(Rank.Three, new int[] { 3, 4 });
            TestParseRankHint(Rank.Four, new int[] { 1, 2, 3, 4 });
            TestParseRankHint(Rank.One, new int[] { 2, 3 });
        }

        [Test]
        public void TestParseSuitHintWithPosition()
        {
            TestParseSuitHint(Suit.Red, new int[] { 1, 2, 3 });
            TestParseSuitHint(Suit.Green, new int[] { 2, 4 });
            TestParseSuitHint(Suit.Blue, new int[] { 3, 4 });
            TestParseSuitHint(Suit.White, new int[] { 4 });
            TestParseSuitHint(Suit.Yellow, new int[] { 2, 3, 4 });
        }

        [Test]
        public void TestParsePlayCard()
        {
            foreach (int index in Enumerable.Range(0, 4))
                TestParsePlayCardNo(index);
        }

        [Test]
        public void TestParseDropCard()
        {
            foreach (int index in Enumerable.Range(0, 4))
                TestParseDropCardNo(index);
        }

    }

    [TestFixture]
    public class GameTester
    {
        [Test]
        public void TestRightInitialization()
        {
            Card[] expectedOne = new[] {   new Card(Suit.Red, Rank.One),   new Card(Suit.Red, Rank.Two),
                                        new Card(Suit.Red, Rank.Three),
                                        new Card(Suit.Red, Rank.Four),  new Card(Suit.Red, Rank.Five)
        };

            Card[] expectedTwo = new[] {    new Card(Suit.Green, Rank.One),   new Card(Suit.Green, Rank.Two),
                                        new Card(Suit.Green, Rank.Three),
                                        new Card(Suit.Green, Rank.Four),  new Card(Suit.Green, Rank.Five)
        };

            Card[] expectedDeck = new[] {       new Card(Suit.Green, Rank.One),   new Card(Suit.Green, Rank.Two),
                                            new Card(Suit.Green, Rank.Three),
                                            new Card(Suit.Green, Rank.Four),  new Card(Suit.Green, Rank.Five)
        };

            var inputCards = expectedOne.Concat(expectedTwo).Concat(expectedDeck);
            var game = new Game(inputCards);
            CollectionAssert.AreEqual(expectedOne, ((HanabiPlayer)game.players[0]).playPile, new CardComparer());
            CollectionAssert.AreEqual(expectedTwo, ((HanabiPlayer)game.players[1]).playPile, new CardComparer());
            CollectionAssert.AreEqual(expectedDeck, game.deck, new CardComparer());
        }
    }
}

namespace Hanabi
{
    public enum ActionType  { StartGame, Play, Drop, Clue }
    public enum Suit        { Red, Green, Blue, White, Yellow }
    public enum Rank        { Zero, One, Two, Three, Four, Five }
    public enum GameStatus  { Continue, Finish }
    public enum HintType    { SuitHint, RankHint, CardHint };

    public interface IPlayer
    {
        Card PlayCard(int cardHandPosition);
        Card DropCard(int cardHandPosition);
        void AddCard(Card card);
        void AddCards(IEnumerable<Card> cards);
        void UseHint(Hint hint);
    }

    public class HanabiPlayer : IPlayer
    {
        public List<Card> playPile { get; private set; }


        public HanabiPlayer(IEnumerable<Card> cards)
        {
            playPile = new List<Card>();
            AddCards(cards);
        }

        public IEnumerable<Card> GetCards()
        {
            return playPile;
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

        public void UseHint(Hint hint)
        {
            if (hint.hintType == HintType.RankHint)
                UseRankHint(hint);
            else
                UseSuitHint(hint);
        }

        private void UseSuitHint(Hint hint)
        {
            foreach (var index in hint.cardHandPositions)
                ((HanabiCard)playPile[index]).OpenSuit(hint.suit);

            foreach (var index in Enumerable.Range(0, playPile.Count()).Except(hint.cardHandPositions))
                ((HanabiCard)playPile[index]).ExcludeSuit(hint.suit);
        }

        private void UseRankHint(Hint hint)
        {
            foreach (var index in hint.cardHandPositions)
                ((HanabiCard)playPile[index]).OpenRank(hint.rank);

            foreach (var index in Enumerable.Range(0, playPile.Count()).Except(hint.cardHandPositions))
                ((HanabiCard)playPile[index]).ExcludeRank(hint.rank);
        }
    }

    public class Hint
    {
        public HintType hintType;
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
            return new CommandInfo { actionType = ActionType.Clue,
                hint = new Hint { suit = suit, hintType = HintType.SuitHint, cardHandPositions = ParseCardPositions(tokens).ToArray() } };
        }

        public CommandInfo ParseRankHint(string[] tokens)
        {
            var rank = (Rank)Enum.Parse(typeof(Rank), tokens[2]);
            return new CommandInfo { actionType = ActionType.Clue,
                hint = new Hint { rank = rank, hintType = HintType.RankHint, cardHandPositions = ParseCardPositions(tokens).ToArray() } };
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
        int     CountCards();
        bool    BoardIsFull();
    }

    public class HanabiBoard : IBoard
    {
        private Dictionary<Suit, Stack<Card>> boardCards;
        private const int SuitCount     = 5;
        private const int MaxCardsCount = 25;

        public HanabiBoard()
        {
            boardCards = new Dictionary<Suit, Stack<Card>>();
            InitBoard();
        }

        private void InitBoard()
        {
            for (var suit = Suit.Red; suit <= Suit.Yellow; ++suit)
                boardCards[suit] = new Stack<Card>();
        }

        public void AddCard(Card card)
        {
            boardCards[card.suit].Push(card);
        }

        public bool CardCanPlay(Card card)
        {
            if (boardCards[card.suit].Count == 0)
                return (card.rank == Rank.One);

            var topRank = boardCards[card.suit].Peek().rank;
            return (topRank + 1) == card.rank;
        }

        public int CountCards()
        {
            return boardCards.Select(d => d.Value).Sum(stack => stack.Count);
        }

        public bool BoardIsFull()
        {
            return CountCards() == MaxCardsCount;
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
        private const int MinCountDeckCardsAfterDrop = 2;
        public Stack<Card> deck { get; private set; }
        IPlayer player;
        ActionType lastCommand;
        public  List<IPlayer> players { get; private set; }
        private IBoard hanabiBoard;
        private int currentIndexOfPlayer, risks, cards, score, turn;

        public Game(IEnumerable<Card> cards)
        {
            hanabiBoard = new HanabiBoard();
            players     = new List<IPlayer> {   new HanabiPlayer(cards.Take(CountCardsOnHand)),
                                                new HanabiPlayer(cards.Skip(CountCardsOnHand).Take(CountCardsOnHand)) };
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

        public void UpdateGameParameters()
        {
            turn++;
            if (lastCommand != ActionType.Clue)
                NextPlayer();
        }

        private void NextPlayer()
        {
            currentIndexOfPlayer = (currentIndexOfPlayer + 1) % 2;
            player = players[currentIndexOfPlayer];
        }

        private bool IsNotCorrectHint(Hint hint)
        {
            var cardsPositions = GetIndexesOfCardsWithProperty(hint);
            if (cardsPositions.SequenceEqual(hint.cardHandPositions))
                return false;
            return true;
        }

        private IEnumerable<int> GetIndexesOfCardsWithProperty(Hint hint)
        {
            var cards = ((HanabiPlayer)player).GetCards().ToList();
            if (hint.hintType == HintType.SuitHint)
                return cards.Where(w => (w.suit == hint.suit)).Select(w => cards.IndexOf(w));

           return cards.Where(w => (w.rank == hint.rank)).Select(w => cards.IndexOf(w)).ToList();
        }

        private GameStatus ProcessPlay(int cardPosition)
        {
            var currentCard = player.PlayCard(cardPosition);
            if (HasConflictsAfterPlay(currentCard))
                return GameStatus.Finish;

            CheckRisks(currentCard);   
            hanabiBoard.AddCard(currentCard);
            player.AddCard(deck.Pop());
            return (deck.Count == 0) ? GameStatus.Finish : GameStatus.Continue;
        }

        private bool HasConflictsAfterPlay(Card card)
        {
            return (deck.Count == 0) || !hanabiBoard.CardCanPlay(card);
        }

        private GameStatus ProcessDrop(int cardPosition)
        {
            var card = player.DropCard(cardPosition);
            if (HasConflictsAfterDrop(card))
                return GameStatus.Finish;

            player.AddCard(deck.Pop());
            return GameStatus.Continue;
        }

        private bool HasConflictsAfterDrop(Card card)
        {
            return deck.Count < MinCountDeckCardsAfterDrop;
        }

        private GameStatus ProcessHint(Hint hint)
        {
            if (IsNotCorrectHint(hint))
                return GameStatus.Finish;

            player.UseHint(hint);
            return GameStatus.Continue;
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

                case ActionType.Clue:
                    NextPlayer();               // because hint executes on the opponent
                    return ProcessHint(parsedInfo.hint);

                default:
                    return GameStatus.Finish;
            }
        }

        public void PrintStats()
        {
            Console.WriteLine("Turn: " + turn + ", cards: " + hanabiBoard.CountCards() + ", with risk: " + risks);
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