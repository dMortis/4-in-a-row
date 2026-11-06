using System;

// Pure board rules, shared by the player, computer and validation checks.
public static class ConnectFourRules
{
    public const int Columns = 7;
    public const int Rows = 6;
    private static readonly int[] MoveOrder = { 3, 2, 4, 1, 5, 0, 6 };

    public static int AvailableRow(int[,] board, int column)
    {
        if (column < 0 || column >= Columns) return -1;
        for (int row = 0; row < Rows; row++)
            if (board[column, row] == 0) return row;
        return -1;
    }

    public static bool IsFull(int[,] board)
    {
        for (int column = 0; column < Columns; column++)
            if (AvailableRow(board, column) >= 0) return false;
        return true;
    }

    public static int Winner(int[,] board)
    {
        int[,] directions = { { 1, 0 }, { 0, 1 }, { 1, 1 }, { 1, -1 } };
        for (int x = 0; x < Columns; x++)
        for (int y = 0; y < Rows; y++)
        {
            int color = board[x, y];
            if (color == 0) continue;
            for (int d = 0; d < 4; d++)
            {
                int dx = directions[d, 0], dy = directions[d, 1];
                int endX = x + 3 * dx, endY = y + 3 * dy;
                if (endX < 0 || endX >= Columns || endY < 0 || endY >= Rows) continue;
                if (board[x + dx, y + dy] == color && board[x + 2 * dx, y + 2 * dy] == color && board[endX, endY] == color)
                    return color;
            }
        }
        return 0;
    }

    public static int ChooseComputerMove(int[,] source, int depth = 5)
    {
        int[,] board = (int[,])source.Clone();
        if (Winner(board) != 0 || IsFull(board)) return -1;
        int bestMove = -1, bestScore = int.MinValue;
        foreach (int column in MoveOrder)
        {
            int row = AvailableRow(board, column);
            if (row < 0) continue;
            board[column, row] = 2;
            int score = Search(board, Math.Max(0, depth - 1), false, -1000000, 1000000);
            board[column, row] = 0;
            if (score > bestScore) { bestScore = score; bestMove = column; }
        }
        return bestMove;
    }

    private static int Search(int[,] board, int depth, bool computer, int alpha, int beta)
    {
        int winner = Winner(board);
        if (winner == 2) return 100000 + depth;
        if (winner == 1) return -100000 - depth;
        if (IsFull(board)) return 0;
        if (depth == 0) return Evaluate(board);
        int best = computer ? -1000000 : 1000000;
        foreach (int column in MoveOrder)
        {
            int row = AvailableRow(board, column);
            if (row < 0) continue;
            board[column, row] = computer ? 2 : 1;
            int score = Search(board, depth - 1, !computer, alpha, beta);
            board[column, row] = 0;
            if (computer) { best = Math.Max(best, score); alpha = Math.Max(alpha, best); }
            else { best = Math.Min(best, score); beta = Math.Min(beta, best); }
            if (alpha >= beta) break;
        }
        return best;
    }

    private static int Evaluate(int[,] board)
    {
        int score = 0;
        for (int row = 0; row < Rows; row++)
            score += board[3, row] == 2 ? 6 : board[3, row] == 1 ? -6 : 0;
        int[,] directions = { { 1, 0 }, { 0, 1 }, { 1, 1 }, { 1, -1 } };
        for (int x = 0; x < Columns; x++)
        for (int y = 0; y < Rows; y++)
        for (int d = 0; d < 4; d++)
        {
            int dx = directions[d, 0], dy = directions[d, 1];
            int endX = x + 3 * dx, endY = y + 3 * dy;
            if (endX < 0 || endX >= Columns || endY < 0 || endY >= Rows) continue;
            int red = 0, yellow = 0;
            for (int i = 0; i < 4; i++)
            {
                int cell = board[x + i * dx, y + i * dy];
                if (cell == 1) red++;
                if (cell == 2) yellow++;
            }
            if (red == 0) score += yellow == 3 ? 80 : yellow == 2 ? 12 : yellow;
            if (yellow == 0) score -= red == 3 ? 100 : red == 2 ? 14 : red;
        }
        return score;
    }
}
