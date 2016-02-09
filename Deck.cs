using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiMM
{
    public class Deck
    {
        private List<Card> cards;

        public Deck()
        {
            cards = new List<Card>();
        }

        //public int Score() { return all.Count(c => c.In == Card.Holder.BOARD); }
        //public int Depth() { return all.Count(c => c.In == Card.Holder.DECK); }

        public Card Draw()
        {
            var card = cards[0];
            cards.RemoveAt(0);
            return card;
        }

        public void addCard(Card card)
        {
            cards.Add(card);
        }

        public void addCards(List<Card> cards)
        {
            foreach (Card card in cards)
                addCard(card);
        }

        public bool isEmpty()
        {
            return cards.Count <= 1;
        }

        public void reverse()
        {
            cards.Reverse();
        }
    }
}
