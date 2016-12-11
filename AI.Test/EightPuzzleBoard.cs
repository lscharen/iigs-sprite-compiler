using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.Test
{
    public class EightPuzzleBoard
    {
        protected static Random rng = new Random();

        public enum Direction
        {
            LEFT,
            RIGHT,
            UP,
            DOWN
        }

        private int[] board;

        public EightPuzzleBoard()
            : this(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 })
        {
        }

        public EightPuzzleBoard(int[] aBoard)
        {
            this.board = aBoard;
        }

        public EightPuzzleBoard(EightPuzzleBoard aBoard)
        {
            this.board = (int[])aBoard.board.Clone();
        }

        public EightPuzzleBoard Scramble()
        {
            return Scramble(10);
        }

        public EightPuzzleBoard Scramble(int n)
        {
            var newPuzzle = new EightPuzzleBoard(this);
            var direction = Enum.GetValues(typeof(Direction)).Cast<Direction>().ToList();

            for (int i = 0; i < n; i++)
            {
                int j = rng.Next(direction.Count);
                if (newPuzzle.CanMoveGap(direction[j]))
                {
                    newPuzzle.MoveGap(direction[j]);
                }
            }

            return newPuzzle;
        }

        private int[] ind2sub(int x)
        {
            if (x < 0 || x > 8)
            {
                return null;
            }

            return new int[] { x / 3, x % 3 };
        }

        protected int sub2ind(int x, int y)
        {
            return x * 3 + y;
        }

        public int this[int key]
        {
            get
            {
                return board[key];
            }

            set
            {
                board[key] = value;
            }
        }

        private int GapPosition { get { return GetPositionOf(0); } }

        public int CountMismatches(EightPuzzleBoard aBoard)
        {
            int count = 0;
            for (int i = 0; i < 9; i++)
            {
                if (board[i] != aBoard[i] && board[i] != 0)
                {
                    count++;
                }
            }
            return count;
        }

        private int GetPositionOf(int val)
        {
            int retVal = -1;
            for (int i = 0; i < 9; i++)
            {
                if (board[i] == val)
                {
                    retVal = i;
                }
            }
            return retVal;
        }

        public int[] Board { get { return board; } }


    }
}
