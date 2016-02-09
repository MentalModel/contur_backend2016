using System.Collections.Generic;
using System.Linq;

namespace HanabiMM
{
    public class Pile
    {
        public List<Card> pile { get; set ; }

        public Pile()
        {
            pile = new List<Card>();
        }

        public override string ToString()
        {
            string result = "";
            foreach (Card card in pile)
                result += card + " ";
            return string.Format("{0}", result);
        }

        public void addCard(Card newCard)
        {
            pile.Add(newCard);
        }

        public void addCards(List<Card> newCards)
        {
            foreach(Card card in newCards)
                addCard(card);
        }

        public int getSize()
        {
            return pile.Count;
        }

        public Card getCardAtPosition(int position)
        {
            return pile[position];
        }

        public void deleteCardAtPosition(int position)
        {
            pile.RemoveAt(position);
        }

        public List<Card> getRankCard(Rank rank)
        {
            return pile.Select(x => x).Where(x => x.rank == rank).ToList();
        }

        public List<Card> getSuitCard(Suit suit)
        {
            return pile.Select(x => x).Where(x => x.suit == suit).ToList();
        }
    }
}
