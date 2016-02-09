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
            //return cards.FirstOrDefault(x => x.holder == Holder.Deck);
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

        /*
        public int? StuckAt(int suit)
        {
            for (int n = NextPlay(suit); n <= Card.NUMBERS; n++)
            {
                Card need = new Card { Suit = suit, Number = n };
                if (Down().CountSame(need) == need.Copies()) return n;
            }
            return null; // not stuck
        }
        public int NextPlay(int suit)
        {
            return all.Max(c => c.In == Card.Holder.BOARD && c.Suit == suit ? c.Number : 0) + 1;
        }
        public bool AllowPlay(Card n)
        {
            return NextPlay(n.Suit) == n.Number;
        }
        public IEnumerable<Card> Down()
        {
            return all.Where(c => c.In == Card.Holder.BOARD || c.In == Card.Holder.DISCARD);
        }
        public IEnumerable<Card> Board()
        {
            return all.Where(c => c.In == Card.Holder.BOARD);
        }
        */
    }





}
