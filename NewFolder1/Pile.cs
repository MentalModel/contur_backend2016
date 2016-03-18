using System.Collections.Generic;
using System.Linq;

namespace HanabiMM
{
    public class Pile
    {
        private List<Card> pile;

        public Pile()
        {
            pile = new List<Card>();
        }

        public void AddCard(Card newCard)
        {
            pile.Add(newCard);
        }

        public void AddCards(List<Card> newCards)
        {
            foreach (Card card in newCards)
                AddCard(card);
        }

        public int Count()
        {
            return pile.Count;
        }

        public Card GetCardAtPosition(int position)
        {
            return pile[position];
        }

        public void RemoveCardAtPosition(int position)
        {
            pile.RemoveAt(position);
        }

        public List<Card> GetCards()
        {
            return pile;
        }

        public override string ToString()
        {
            string result = "";
            foreach (var card in pile)
                result += card + " ";

            return string.Format("{0}", result);
        }
    }
}
