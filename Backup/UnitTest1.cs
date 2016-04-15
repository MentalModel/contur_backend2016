using System;
using NUnit.Framework;
using Hanabi;

namespace UnitTestProject1
{
    [TestFixture]
    public class HanabiBoardTester
    {
        [Test]
        public void TestMethod1()
        {
            var board = new HanabiBoard();
            for (Suit suit = Suit.Red; suit < Suit.Yellow; ++suit)
                for (Rank rank = Rank.One; rank <= Rank.Five; ++rank)
                    board.AddCard(new Card(suit, rank));


            Assert.AreEqual(board.BoardIsFull(), true);
        }
    }
}
