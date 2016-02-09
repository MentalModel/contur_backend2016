using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiMM
{
    public class Board
    {
        public Stack<Card>[]    boardCards;
        public const string     DELIMITER = " ";

        public Board()
        {
            boardCards = new Stack<Card>[5];
            int i = 0;
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                boardCards[i] = new Stack<Card>();
                boardCards[i++].Push(new Card(suit, Rank.Zero, Holder.Board));
            }
        }

        public override string ToString()
        {
            string result = "";
            foreach (Stack<Card> stack in boardCards)
                result += stack.Peek() + DELIMITER;
  
            return string.Format("{0}", result);
        }

        public void addCard(Card card)
        {
            boardCards[Convert.ToUInt16(card.suit)].Push(card);
        }

        public bool boardIsFull()
        {
            foreach (Stack<Card> stack in boardCards)
                if (stack.Count < 5)
                    return false;
            return true;
        }

        public bool cardCanPlay(Card card)
        {
            int  suit    = Convert.ToUInt16(card.suit);
            Rank topRank = boardCards[suit].Peek().rank;

            if (topRank + 1 == card.rank)
                return true;

            return false;
        }
    }
}
