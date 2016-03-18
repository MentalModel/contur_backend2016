using System.Collections.Generic;
using System.Linq;

namespace HanabiMM
{
    public class Pile
    {
        private List<HeldCard> pile;

        public Pile()
        {
            pile = new List<HeldCard>();
        }

        public void AddCard(HeldCard newCard)
        {
            pile.Add(newCard);
        }

        public void AddCards(List<HeldCard> newCards)
        {
            foreach (HeldCard card in newCards)
                AddCard(card);
        }

        public int Count()
        {
            return pile.Count;
        }

        public HeldCard GetCardAtPosition(int position)
        {
            return pile[position];
        }

        public void RemoveCardAtPosition(int position)
        {
            pile.RemoveAt(position);
        }

        public List<HeldCard> GetCards()
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
