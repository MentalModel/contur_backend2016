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

        public Card Draw()
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

        public void AddCards(List<Card> cards)
        {
            foreach (Card card in cards)
                AddCard(card);
        }

        public bool IsEmpty()
        {
            return (cards.Count == 0);
        }

        public void Reverse()
        {
            cards.Reverse();
        }

        public List<Card> getCards()
        {
            return cards;
        }
    }
}
