using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.Test
{
    public static class DirectionExtensions
    {
        public static bool CanMove(this Direction direction, int p)
        {
            switch (direction)
            {
                case Direction.LEFT: return (p % 3) != 0;
                case Direction.RIGHT: return (p % 3) != 2;
                case Direction.UP: return (p > 2);
                case Direction.DOWN: return (p < 6);
            }

            throw new ArgumentException();
        }

        public static int MoveFrom(this Direction direction, int p)
        {
            switch (direction)
            {
                case Direction.LEFT: return p - 1;
                case Direction.RIGHT: return p + 1;
                case Direction.UP: return p - 3;
                case Direction.DOWN: return p + 3;
            }

            throw new ArgumentException();
        }
    }

    public enum Direction
    {
        LEFT,
        RIGHT,
        UP,
        DOWN
    }

    public class EightPuzzleBoard
    {
        protected static Random rng = new Random();


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

        public int[] Board { get { return board; } }

        public EightPuzzleBoard Scramble()
        {
            return Scramble(10);
        }

        public EightPuzzleBoard Scramble(int n)
        {
            var newPuzzle = new EightPuzzleBoard(this);
            var direction = Enum.GetValues(typeof(Direction)).Cast<Direction>().ToList();

            for (int i = 0; i < n;)
            {
                int j = rng.Next(direction.Count);
                if (newPuzzle.CanMoveGap(direction[j]))
                {
                    newPuzzle.MoveGap(direction[j]);
                    i += 1;
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

        public int[] GetLocationOf(int val)
        {
            return ind2sub(GetPositionOf(val)); 
        }

        public EightPuzzleBoard MoveGap(Direction direction)
        {
            var pos1 = GapPosition;
            if (direction.CanMove(pos1))
            {
                var pos2 = direction.MoveFrom(pos1);
                Swap(pos1, pos2);
            }

            return this;
        }

        private void Swap(int pos1, int pos2)
        {
            var val = this[pos1];
            this[pos1] = this[pos2];
            this[pos2] = val;
        }

        public override bool Equals(object obj)
        {
            return (this == obj) || board.SequenceEqual(((EightPuzzleBoard)obj).board);
        }

        public override int GetHashCode()
        {
            return board.GetHashCode();
        }

        public bool CanMoveGap(Direction where)
        {
            return where.CanMove(GetPositionOf(0));
        }

        public override string ToString()
        {
            return
                board[0] + " " + board[1] + " " + board[2] + Environment.NewLine +
                board[3] + " " + board[4] + " " + board[5] + Environment.NewLine +
                board[6] + " " + board[7] + " " + board[8];
        }
    }
}
