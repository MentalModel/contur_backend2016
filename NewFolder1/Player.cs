using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HanabiMM
{
    public class Player
    {
        private Pile    playPile;
        private Board   board;
        private int     name;

        public Player(int name, Board playBoard)
        {
            this.name   = name;
            playPile    = new Pile();
            board       = playBoard;
        }

        public Card CardAtPosition(int position)
        {
            return playPile.GetCardAtPosition(position);
        }

        public IEnumerable<int> GetAllPositionsOfSuitCards(Suit suit)
        {
            var result = playPile.GetCards();
            return result.Where(w => (w.suit == suit)).Select(w => result.IndexOf(w));
        }

        public IEnumerable<int> GetAllPositionsOfRankCards(Rank rank)
        {
            var result = playPile.GetCards();
            return result.Where(w => (w.rank == rank)).Select(w => result.IndexOf(w));
        }

        public Tuple<Card, bool> PlayCard(int position)
        {
            var card  = playPile.GetCardAtPosition(position);
            playPile.RemoveCardAtPosition(position);

            return Tuple.Create((Card)card, IsRiskyTurn(card));
        }

        public bool NotAllCardsInQueryCanPlay(IEnumerable<HeldCard> query)
        {
            foreach (var card in query)
                if (!board.CardCanPlay(card))
                    return true;
            return false;
        }

        public bool IsRiskyTurn(HeldCard card)
        {
            var query = GetPossibleCardsForTurn(card).ToList();
            if (NotAllCardsInQueryCanPlay(query))
                return false;
            return true;
        }

        public Card DropCard(int position)
        {
            return PlayCard(position).Item1;
        }

        public int GetName()
        {
            return name;
        }

        public void AddCard(Card card)
        {
            playPile.AddCard(new HeldCard(card.suit, card.rank));
        }

        public void AddCards(IEnumerable<Card> cards)
        {
            foreach (var card in cards)
                AddCard(card);
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

        public IEnumerable<T> GeneratePossibilitiesForTurn<T>(HeldCard card, BitArray bits)
        {
            var result = new List<T>();
            var enumValues = Enum.GetValues(typeof(T)).GetEnumerator();
            var i = 0;

            enumValues.MoveNext();          // miss first element (Zero or None)
            while (enumValues.MoveNext())
            {
                if (bits[i++])
                    result.Add((T)enumValues.Current);
            }
            return result;
        }

        public IEnumerable<HeldCard> GetDecartusMulti(HeldCard card)
        {
            var possibleSuits = GeneratePossibilitiesForTurn<Suit>(card, card.suitBits);
            var possibleRanks = GeneratePossibilitiesForTurn<Rank>(card, card.rankBits);
            return possibleSuits.SelectMany(x => possibleRanks, (x, y) => new HeldCard(x, y)).ToList();
        }

        public IEnumerable<HeldCard> GetPossibleCardsForTurn(HeldCard card)
        {
            var query = new List<HeldCard>();
            if (card.isKnownCard())
                query.Add(card);
            else
                query = GetDecartusMulti(card).ToList();

            return query;
        }

        public void DeduceSuitForMainCards(Suit suit, IEnumerable<int> positions)
        {
            foreach (var index in positions)
                OpenNthSuit(index, suit);
        }

        public void DeduceSuitForOtherCards(Suit suit, IEnumerable<int> positions)
        {
            foreach(var index in positions)
                CloseNthSuit(index, suit);
        }

        public void DeduceSuit(Suit suit, IEnumerable<int> positions)
        {
            DeduceSuitForOtherCards(suit, Enumerable.Range(0, playPile.Count()).Except(positions));
            DeduceSuitForMainCards(suit, positions);
        }

        public void DeduceRankForMainCards(Rank rank, IEnumerable<int> positions)
        {
            foreach (var index in positions)
                OpenNthRank(index, rank);
        }

        public void DeduceRankForOtherCards(Rank rank, IEnumerable<int> positions)
        {
            foreach (var index in positions)
                CloseNthRank(index, rank);
        }

        public void DeduceRank(Rank rank, List<int> positions)
        {
            DeduceRankForOtherCards(rank, Enumerable.Range(0, playPile.Count()).Except(positions));
            DeduceRankForMainCards(rank, positions);
        }


        public override string ToString()
        {
            return string.Format("{0}", playPile);
        }
    }
}
