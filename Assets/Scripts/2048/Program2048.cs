using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;
using Console = UnityConsole.Console;


public class Program2048 : MonoBehaviour
{
    Color[] Colors =
    {
        new Color(0, 0, 0.545f),
        new Color(0.1961f, 0.3921f, 0),
        new Color(0, 0.545f, 0.545f),
        new Color(0.545f, 0, 0),
        new Color(0.545f, 0, 0.545f),
        new Color(0.502f, 0.502f, 0),
        Color.blue,
        Color.red,
        Color.magenta,
    };

    public enum Direction
    {
        Up = 1,
        Down = 2,
        Left = 3,
        Right = 4,
    }

    private Random random = new Random();


    private void Start()
    {
        UniTask.Create(() => Play(this.GetCancellationTokenOnDestroy()));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }
    }

    private async UniTask Play(CancellationToken cancellationToken = default)
    {
        try

        {
            Console.CursorVisible = false;
            while (true)
            {
                NewBoard:
                Console.Clear();
                int?[,] board = new int?[4, 4];
                int score = 0;
                while (true)
                {
                    // add a 2 or 4 randomly to the board
                    bool IsNull((int X, int Y) point) => board[point.X, point.Y] is null;
                    int nullCount = BoardValues(board).Count(IsNull);
                    if (nullCount is 0)
                    {
                        goto GameOver;
                    }

                    int index = random.Next(0, nullCount);
                    var (x, y) = BoardValues(board).Where(IsNull).ElementAt(index);
                    board[x, y] = random.Next(10) < 9 ? 2 : 4;
                    score += 2;

                    // make sure there are still valid moves left
                    if (!TryUpdate((int?[,])board.Clone(), ref score, Direction.Up) &&
                        !TryUpdate((int?[,])board.Clone(), ref score, Direction.Down) &&
                        !TryUpdate((int?[,])board.Clone(), ref score, Direction.Left) &&
                        !TryUpdate((int?[,])board.Clone(), ref score, Direction.Right))
                    {
                        goto GameOver;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    Render(board, score);
                    Direction direction;
                    GetDirection:
                    while (Console.GetKeyState(KeyCode.UpArrow) ||
                           Console.GetKeyState(KeyCode.DownArrow) ||
                           Console.GetKeyState(KeyCode.LeftArrow) ||
                           Console.GetKeyState(KeyCode.RightArrow) ||
                           Console.GetKeyState(KeyCode.W) ||
                           Console.GetKeyState(KeyCode.A) ||
                           Console.GetKeyState(KeyCode.S) ||
                           Console.GetKeyState(KeyCode.D))
                    {
                        
                        await Console.Sleep(16, cancellationToken);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }
                    }

                    KeyCode key = KeyCode.None;
                    while (key == KeyCode.None)
                    {
                        if (Console.GetKeyState(KeyCode.UpArrow) || Console.GetKeyState(KeyCode.W))
                        {
                            key = KeyCode.UpArrow;
                        }
                        if (Console.GetKeyState(KeyCode.DownArrow) || Console.GetKeyState(KeyCode.S))
                        {
                            key = KeyCode.DownArrow;
                        }
                        if (Console.GetKeyState(KeyCode.LeftArrow) || Console.GetKeyState(KeyCode.A))
                        {
                            key = KeyCode.LeftArrow;
                        }
                        if (Console.GetKeyState(KeyCode.RightArrow) || Console.GetKeyState(KeyCode.D))
                        {
                            key = KeyCode.RightArrow;
                        }

                        await Console.Sleep(16, cancellationToken);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }
                    }
                    switch (key)
                    {
                        case KeyCode.UpArrow:
                            direction = Direction.Up;
                            break;
                        case KeyCode.DownArrow:
                            direction = Direction.Down;
                            break;
                        case KeyCode.LeftArrow:
                            direction = Direction.Left;
                            break;
                        case KeyCode.RightArrow:
                            direction = Direction.Right;
                            break;
                        case KeyCode.End: goto NewBoard;
                        case KeyCode.Escape: goto Close;
                        default: goto GetDirection;
                    }

                    if (!TryUpdate(board, ref score, direction))
                    {
                        goto GetDirection;
                    }
                    await Console.Sleep(16, cancellationToken);
                }

                GameOver:
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                Render(board, score);
                Console.WriteLine("Game Over...");
                Console.WriteLine();
                Console.WriteLine("Play Again [enter], or quit [escape]?");
                GetInput:
                switch (await Console.ReadKey(true, cancellationToken))
                {
                    case KeyCode.Return: goto NewBoard;
                    case KeyCode.Escape: goto Close;
                    default: goto GetInput;
                }

            }

            Close:
            Console.Clear();
            Console.Write("2048 was closed.");
        }
        finally

        {
            Console.CursorVisible = true;
        }
    }


    bool TryUpdate(int?[,] board, ref int score, Direction direction)
    {
        (int X, int Y) Adjacent(int x, int y) =>
            direction switch
            {
                Direction.Up => (x + 1, y),
                Direction.Down => (x - 1, y),
                Direction.Left => (x, y - 1),
                Direction.Right => (x, y + 1),
                _ => throw new NotImplementedException(),
            };

        (int X, int Y) Map(int x, int y) =>
            direction switch
            {
                Direction.Up => (board.GetLength(0) - x - 1, y),
                Direction.Down => (x, y),
                Direction.Left => (x, y),
                Direction.Right => (x, board.GetLength(1) - y - 1),
                _ => throw new NotImplementedException(),
            };

        bool[,] locked = new bool[board.GetLength(0), board.GetLength(1)];

        bool update = false;

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                var (tempi, tempj) = Map(i, j);
                if (board[tempi, tempj] is null)
                {
                    continue;
                }

                KeepChecking:
                var (adji, adjj) = Adjacent(tempi, tempj);
                if (adji < 0 || adji >= board.GetLength(0) ||
                    adjj < 0 || adjj >= board.GetLength(1) ||
                    locked[adji, adjj])
                {
                    continue;
                }
                else if (board[adji, adjj] is null)
                {
                    board[adji, adjj] = board[tempi, tempj];
                    board[tempi, tempj] = null;
                    update = true;
                    tempi = adji;
                    tempj = adjj;
                    goto KeepChecking;
                }
                else if (board[adji, adjj] == board[tempi, tempj])
                {
                    board[adji, adjj] += board[tempi, tempj];
                    score += board[adji, adjj]!.Value;
                    board[tempi, tempj] = null;
                    update = true;
                    locked[adji, adjj] = true;
                }
            }
        }

        return update;
    }

    IEnumerable<(int, int)> BoardValues(int?[,] board)
    {
        for (int i = board.GetLength(0) - 1; i >= 0; i--)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                yield return (i, j);
            }
        }
    }

    Color GetColor(int? value) =>
        value is null
            ? new Color(169, 169, 169)
            : Colors[(value.Value / 2 - 1) % Colors.Length];

    void Render(int?[,] board, int score)
    {
        int horizontal = board.GetLength(0) * 8;
        string horizontalBar = new('═', horizontal);
        string horizontalSpace = new(' ', horizontal);

        Console.Clear();
        Console.WriteLine("2048");
        Console.WriteLine();
        Console.WriteLine($"╔{horizontalBar}╗");
        Console.WriteLine($"║{horizontalSpace}║");
        for (int i = board.GetLength(1) - 1; i >= 0; i--)
        {
            Console.Write("║");
            for (int j = 0; j < board.GetLength(0); j++)
            {
                Console.Write("  ");
                Color background = Console.BackgroundColor;
                Console.BackgroundColor = GetColor(board[i, j]);
                Console.Write($"{board[i, j],4}");
                Console.BackgroundColor = background;
                Console.Write("  ");
            }

            Console.WriteLine("║");
            Console.WriteLine($"║{horizontalSpace}║");
        }

        Console.WriteLine($"╚{horizontalBar}╝");
        Console.WriteLine($"Score: {score}");
    }
}