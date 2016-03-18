using System;
using System.Linq;

namespace HanabiMM
{
    public class Board
    {
        private Card[]          boardCards;
        private const string    DELIMITER = " ";
        private const int       SUIT_COUNT = 5;

        public Board()
        {
            boardCards = new Card[SUIT_COUNT];
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
            boardCards[GetIndexFromSuit(card.suit)] = card;
        }

        public bool CardCanPlay(Card card)
        {
            var topRank = boardCards[GetIndexFromSuit(card.suit)].rank;
            return (topRank + 1 == card.rank);
        }

        public int GetScore()
        {
            return boardCards.Count(card => card.IsValidRank());
        }

        public int GetDepth()
        {
            return boardCards
                .Where(card => card.IsValidRank())
                .Select(card => card.rank)
                .Sum(c => (int)c);
        }

        public override string ToString()
        {
            string result = "";
            foreach (var card in boardCards)
                result += card + DELIMITER;

            return string.Format("{0}", result);
        }
    }
}
