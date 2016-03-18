using System.Collections.Generic;

namespace HanabiMM
{
    public class Player
    {
        private Pile playPile;
        private int  name;

        public Player(int name)
        {
            this.name   = name;
            playPile    = new Pile();
        }

        public List<Card> GetPile()
        {
            return playPile.GetCards();
        }

        public Card CardAtPosition(int position)
        {
            return playPile.GetCardAtPosition(position);
        }

        public Card PlayCard(int position)
        {
            var card  = playPile.GetCardAtPosition(position);
            playPile.RemoveCardAtPosition(position);
            return card;
        }

        public Card DropCard(int position)
        {
            return PlayCard(position);
        }

        public int GetName()
        {
            return name;
        }

        public void AddCard(Card card)
        {
            playPile.AddCard(card);
        }

        public void AddCards(List<Card> cards)
        {
            playPile.AddCards(cards);
        }

        public int CountCards()
        {
            return playPile.Count();
        }

        public void OpenNthRank(int index, Rank rank)
        {
            playPile.GetCardAtPosition(index).openRank(rank);
        }

        public void CloseNthRank(int index, Rank rank)
        {
            playPile.GetCardAtPosition(index).closeRank(rank);
        }

        public void OpenNthSuit(int index, Suit suitCard)
        {
            playPile.GetCardAtPosition(index).openSuit(suitCard);
        }

        public void CloseNthSuit(int index, Suit suitCard)
        {
            playPile.GetCardAtPosition(index).closeSuit(suitCard);
        }

        public override string ToString()
        {
            return string.Format("{0}", playPile);
        }
    }
}
