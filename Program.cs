using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanabiMM
{

    /*
    public abstract class AbstractAction
    {
        public Game game;
        
        public AbstractAction(Game newGame)
        {
            game = newGame;
        }

        abstract public bool Execute();
    }

    public class StartNewGameAction : AbstractAction
    {
        public string[] firstPlayerCards {get; set; }
        public string[] secondPlayerCards { get; set; }
        public string[] deckCards { get; set; }

        public StartNewGameAction(Game game, string[] firstPlayerCards, 
            string[] secondPlayerCards, string[] deckCards) : base(game)
        {
            this.firstPlayerCards = firstPlayerCards;
            this.secondPlayerCards = secondPlayerCards;
            this.deckCards = deckCards;
        }

        public override bool Execute()
        {
            return game.startNewGame(this);
        }
    }

    public class PlayAction: AbstractAction
    {
        public int cardPositionInHand;

        public PlayAction(Game game, int cardIndex) : base(game)
        {
            cardPositionInHand = cardIndex;
        }

        public override bool Execute()
        {
            return game.processPlay(this);
        }
    }

    public class DropAction : AbstractAction
    {
        public int cardPositionInHand;

        public DropAction(Game game, int cardIndex) : base(game)
        {
            int cardPositionInHand = cardIndex;
        }

        public override bool Execute()
        {
            return game.processDrop(this);
        }
    }

    public class HintColorAction : AbstractAction
    {
        public Hint hint;

        public HintColorAction(Game game, Hint newHint) : base(game)
        {
            hint = newHint;
        }

        public override bool Execute()
        {
            return game.processColorHint(this); 
        }
    }

    public class HintRankAction : AbstractAction
    {
        public Hint hint;

        public HintRankAction(Game game, Hint newHint) : base(game)
        {
            hint = newHint;
        }

        public override bool Execute()
        {
            return game.processRankHint(this); // !!!
        }
    }

    */
    class Program
    {
        static void Main(string[] args)
        {
            Game newGame = new Game(2, Console.Out);
            newGame.Run();
        }
    }
}
