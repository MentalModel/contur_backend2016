using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public override string ToString()
        {
            return string.Format("{0}", playPile);
        }
    }
}
