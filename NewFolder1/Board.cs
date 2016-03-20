using System.Linq;

namespace HanabiMM
{
    public interface IBoard
    {
        void    AddCard(Card card);
        bool    CardCanPlay(Card card);
        int     GetScore();
        int     GetDepth();
    }

    public class HanabiBoard : IBoard
    {
        private Card[]          boardCards;
        private const string    Delimiter = " ";
        private const int       SuitCount = 5;
        private const int       MaxCardsCount = 25;

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
                .Where(card => card.IsValidRank())
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
}
