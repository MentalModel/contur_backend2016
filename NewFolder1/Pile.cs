using System.Collections.Generic;
using System.Linq;

namespace HanabiMM
{
    public interface IPile
    {
        void                AddCard(Card newCard);
        void                AddCards(IEnumerable<Card> newCards);
        int                 Count();
        IEnumerable<Card>   GetCards();
    }

    public class Pile : IPile
    {
        private List<Card> pile;
        private const string Delimiter = " ";

        public Pile()
        {
            pile = new List<Card>();
        }

        public void AddCard(Card newCard)
        {
            pile.Add(newCard);
        }

        public void AddCards(IEnumerable<Card> newCards)
        {
            foreach (var card in newCards)
                AddCard(card);
        }

        public int Count()
        {
            return pile.Count;
        }

        private bool IsValidPosition(int position)
        {
            return (position >= 0) && (position < pile.Count);
        }

        public Card this[int cardHandPosition]
        {
            get
            {
                if (IsValidPosition(cardHandPosition))
                    return pile[cardHandPosition];
                return null;
            }
        }

        public void RemoveCardAtPosition(int cardHandPosition)
        {
            if (IsValidPosition(cardHandPosition))
                pile.RemoveAt(cardHandPosition);
        }

        public IEnumerable<Card> GetCards()
        {
            return pile;
        }

        public override string ToString()
        {
            string result = "";
            foreach (var card in pile)
                result += card + Delimiter;

            return string.Format("{0}", result);
        }
    }
}
