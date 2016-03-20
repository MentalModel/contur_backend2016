
using System.Collections.Generic;

namespace HanabiMM
{
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
}
