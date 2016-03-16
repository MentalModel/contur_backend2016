using System.Collections.Generic;

namespace HanabiMM
{
    public class Player
    {
        public Pile playPile;
        public int  name;

        public Player(int name)
        {
            this.name   = name;
            playPile    = new Pile();
        }

        public List<Card> getPile()
        {
            return playPile.pile;
        }

        public Card lookAtCardAtPosition(int position)
        {
            return playPile.getCardAtPosition(position);
        }

        public Card playCard(int position)
        {
            var card  = playPile.getCardAtPosition(position);
            playPile.deleteCardAtPosition(position);
            return card;
        }

        public int getName()
        {
            return name;
        }

        public void addCard(Card c)
        {
            playPile.addCard(c);
        }

        public void addCards(List<Card> p)
        {
            playPile.addCards(p);
        }

        public int numCards()
        {
            return playPile.getSize();
        }

        public void openNthRank(int index, Rank rank)
        {
            playPile.pile[index].openRank(rank);
        }

        public void closeNthRank(int index, Rank rank)
        {
            playPile.pile[index].closeRank(rank);
        }

        public void openNthSuit(int index, Suit suitCard)
        {
            playPile.pile[index].openSuit(suitCard);
        }

        public void closeNthSuit(int index, Suit suitCard)
        {
            playPile.pile[index].closeSuit(suitCard);
        }

        public override string ToString()
        {
            return string.Format("{0}", playPile);
        }
    }
}
