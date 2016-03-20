using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HanabiMM
{
    public class Player
    {
        private IPile           playPile;
        private IBoard          hanabiBoard;
        private readonly int    name;

        public Player(int name, IBoard playBoard)
        {
            this.name   = name;
            playPile    = new Pile();
            hanabiBoard = (HanabiBoard)playBoard;
        }

        public Card CardAtPosition(int cardHandPosition)
        {
            return ((Pile)playPile)[cardHandPosition];
        }

        public IEnumerable<int> GetAllPositionsOfSuit(Suit suit)
        {
            var result = playPile.GetCards().ToList();
            return result.Where(w => (w.suit == suit)).Select(w => result.IndexOf(w));
        }

        public IEnumerable<int> GetAllPositionsOfRank(Rank rank)
        {
            var result = ((Pile)playPile).GetCards().ToList();
            return result.Where(w => (w.rank == rank)).Select(w => result.IndexOf(w));
        }

        public Tuple<Card, bool> PlayCard(int cardHandPosition)
        {
            var card  = ((Pile)playPile)[cardHandPosition];
            ((Pile)playPile).RemoveCardAtPosition(cardHandPosition);

            return Tuple.Create(card, IsRiskyTurn(card));
        }

        private bool NotAllCardsInQueryCanPlay(IEnumerable<Card> query)
        {
            foreach (var card in query)
                if (!hanabiBoard.CardCanPlay(card))
                    return true;
            return false;
        }

        private bool IsRiskyTurn(Card moveCard)
        {
            HeldCard card = moveCard as HeldCard;
            if (card != null)
            {
                var query = GetPossibleCardsForTurn(card).ToList();
                if (NotAllCardsInQueryCanPlay(query))
                    return false;
                return true;
            }
            return true;
        }

        public Card DropCard(int cardHandPosition)
        {
            return PlayCard(cardHandPosition).Item1;
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

        private void OpenNthRank(int index, Rank rank)
        {
            HeldCard card = ((Pile)playPile)[index] as HeldCard;
            if (card != null)
                card.OpenRank(rank);
        }

        private void CloseNthRank(int index, Rank rank)
        {
            HeldCard card = ((Pile)playPile)[index] as HeldCard;
            if (card != null)
                card.CloseRank(rank);
        }

        private void OpenNthSuit(int index, Suit suitCard)
        {
            HeldCard card = ((Pile)playPile)[index] as HeldCard;
            if (card != null)
                card.OpenSuit(suitCard);
        }

        private void CloseNthSuit(int index, Suit suitCard)
        {
            HeldCard card = ((Pile)playPile)[index] as HeldCard;
            if (card != null)
                card.CloseSuit(suitCard);
        }

        private IEnumerable<Card> GetDecartusMulti(Card turnCard)
        {
            HeldCard card = turnCard as HeldCard;
            if (card == null)
                return null;
            var cardHandPossibleSuits =  card.GetPossibleSuits();
            var cardHandPossibleRanks =  card.GetPossibleRanks();
            return cardHandPossibleSuits.SelectMany(x => cardHandPossibleRanks, (x, y) => new Card(x, y)).ToList();
        }

        private IEnumerable<Card> GetPossibleCardsForTurn(Card turnCard)
        {
            HeldCard card = turnCard as HeldCard;
            if (card == null)
                return null;

            var query = new List<Card>();
            if (card.isKnownCard())
                query.Add(card);
            else
                query = GetDecartusMulti(card).ToList();

            return query;
        }

        private void DeduceSuitForMainCards(Suit suit, IEnumerable<int> cardHandPosition)
        {
            foreach (var index in cardHandPosition)
                OpenNthSuit(index, suit);
        }

        private void DeduceSuitForOtherCards(Suit suit, IEnumerable<int> cardHandPosition)
        {
            foreach(var index in cardHandPosition)
                CloseNthSuit(index, suit);
        }

        public void DeduceSuit(Suit suit, IEnumerable<int> cardHandPosition)
        {
            DeduceSuitForOtherCards(suit, Enumerable.Range(0, playPile.Count()).Except(cardHandPosition));
            DeduceSuitForMainCards(suit, cardHandPosition);
        }

        private void DeduceRankForMainCards(Rank rank, IEnumerable<int> cardHandPosition)
        {
            foreach (var index in cardHandPosition)
                OpenNthRank(index, rank);
        }

        private void DeduceRankForOtherCards(Rank rank, IEnumerable<int> cardHandPosition)
        {
            foreach (var index in cardHandPosition)
                CloseNthRank(index, rank);
        }

        public void DeduceRank(Rank rank, IEnumerable<int> cardHandPosition)
        {
            DeduceRankForOtherCards(rank, Enumerable.Range(0, playPile.Count()).Except(cardHandPosition));
            DeduceRankForMainCards(rank, cardHandPosition);
        }

        public override string ToString()
        {
            return string.Format("{0}", playPile);
        }
    }
}
