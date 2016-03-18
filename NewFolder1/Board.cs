using System;

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

        public int GetIndexFromSuit(Suit suit)
        {
            return ((int)suit - 1);
        }

        private void InitBoard()
        {
            for(var suit = Suit.Red; suit <= Suit.Yellow; ++suit)
                boardCards[GetIndexFromSuit(suit)] = new Card(suit, Rank.Zero, Holder.Board);
        }

        public void AddCard(Card card)
        {
            boardCards[GetIndexFromSuit(card.suit)] = card;
        }

        public bool IsFull()
        {
            foreach (var card in boardCards)
                if (card.rank < Rank.Five)
                    return false;
            return true;
        }

        public bool CardCanPlay(Card card)
        {
            var topRank = boardCards[GetIndexFromSuit(card.suit)].rank;
            return (topRank + 1 == card.rank);
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
