using SlitherSharp.Models;
using static SlitherSharp.Constants;

namespace SlitherSharp;

internal class Slither
{
    private Direction _currentDirection = Direction.Right;

    private const int SpeedDelta = 9;
    private int _snakeSpeed = 400;
    private int _applesCollected = 0;

    private readonly Apple _apple = new();
    private readonly LinkedList<Snake> _snake = new();

    private readonly Random _random = new(DateTime.Now.Millisecond);

    public Slither()
    {
        _snake.AddFirst(new Snake{X = GameWidth / 2, Y = GameHeight / 2});
    }

    public async Task StartGame()
    {
        InitializeScreen();

        _ = Task.Run(ReadKey);
        for (;;)
        {
            await Task.Delay(_snakeSpeed);

            await MoveSnake();

            await CheckAndMoveApple();

            Console.CursorVisible = false;

            await CheckAndResetWindowSize();
        }
    }

    private async Task MoveSnake()
    {
        if (_snake.Last is null)
            throw new ArgumentNullException("The snake initialized without a starting value.");

        await SetSnakePosition(_snake.Last);
    }

    private Task SetSnakePosition(LinkedListNode<Snake> snakeNode)
    {
        while (true)
        {
            if (snakeNode.Next == null)
            {
                SetAndWrite(snakeNode.Value.X, snakeNode.Value.Y, BLANK);
            }


            if (snakeNode.Previous != null)
            {
                snakeNode.Value.X = snakeNode.Previous.Value.X;
                snakeNode.Value.Y = snakeNode.Previous.Value.Y;
                SetAndWrite(snakeNode.Previous.Value.X, snakeNode.Previous.Value.Y, SNAKE);

                snakeNode = snakeNode.Previous;
                continue;
            }

            ProcessSnakeDirection(snakeNode);

            break;
        }

        return Task.CompletedTask;
    }

    private void ProcessSnakeDirection(LinkedListNode<Snake> snakeNode)
    {
        switch (_currentDirection)
        {
            case Direction.Left:
            {
                snakeNode.Value.X -= 1;
                if (snakeNode.Value.X < 1)
                    snakeNode.Value.X = GameWidth - 1;
                SetAndWrite(snakeNode.Value.X, snakeNode.Value.Y, SNAKE);
                break;
            }
            case Direction.Right:
            {
                snakeNode.Value.X += 1;
                if (snakeNode.Value.X > GameWidth - 1)
                    snakeNode.Value.X = 1;
                SetAndWrite(snakeNode.Value.X, snakeNode.Value.Y, SNAKE);
                break;
            }
            case Direction.Up:
            {
                snakeNode.Value.Y -= 1;
                if (snakeNode.Value.Y < 1)
                    snakeNode.Value.Y = GameHeight - 1;
                SetAndWrite(snakeNode.Value.X, snakeNode.Value.Y, SNAKE);
                break;
            }
            case Direction.Down:
            {
                snakeNode.Value.Y += 1;
                if (snakeNode.Value.Y > GameHeight - 1)
                    snakeNode.Value.Y = 1;
                SetAndWrite(snakeNode.Value.X, snakeNode.Value.Y, SNAKE);
                break;
            }
        }
    }

    private async Task CheckAndMoveApple()
    {
        if (_apple.X == _snake.First?.Value.X && _apple.Y == _snake.First.Value.Y)
        {
            _applesCollected += 1;

            _snake.AddFirst(new Snake { X = _apple.X, Y = _apple.Y });

            FindValidApplePosition();

            SetAndWrite(_apple.X, _apple.Y, APPLE);

            IncreaseSpeed(SpeedDelta);
        }

        await Task.CompletedTask;
    }

    

    private void SetAndWrite(int left, int top, char value)
    {
        Console.SetCursorPosition(left, top);
        Console.Write(value);
    }

    private async Task CheckAndResetWindowSize()
    {
        if (Console.WindowHeight != ConsoleHeight || Console.WindowWidth != ConsoleWidth)
        {
            if (OperatingSystem.IsWindows())
                Console.SetWindowSize(ConsoleWidth, ConsoleHeight);
            else
                throw new NotSupportedException($"The operating system you are running this on is not supported.");
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    private void ReadKey()
    {
        var key = new ConsoleKeyInfo();

        while (!Console.KeyAvailable && key.Key != ConsoleKey.Escape)
        {
            key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    if (_currentDirection == Direction.Down)
                        break;
                    _currentDirection = Direction.Up;
                    break;
                case ConsoleKey.DownArrow:
                    if (_currentDirection == Direction.Up)
                        break;
                    _currentDirection = Direction.Down;
                    break;

                case ConsoleKey.RightArrow:
                    if (_currentDirection == Direction.Left)
                        break;
                    _currentDirection = Direction.Right;
                    break;

                case ConsoleKey.LeftArrow:
                    if(_currentDirection == Direction.Right)
                        break;
                    _currentDirection = Direction.Left;
                    break;

                case ConsoleKey.Escape:
                    break;
            }
        }
    }

    private async void InitializeScreen()
    {
        await CheckAndResetWindowSize();
        Console.Title = "Slither Sharp";

        // Create border
        for (var i = 0; i <= GameWidth; i++)
        {
            for (var j = 0; j <= GameHeight; j++)
            {
                if (i == 0 || (i > 0 && (j == 0 || j == GameHeight)) || i == GameWidth)
                {
                    SetAndWrite(i, j, BORDER);
                }
            }
        }

        FindValidApplePosition();

        SetAndWrite(_apple.X, _apple.Y, APPLE);

        SetAndWrite(_snake.First.Value.X, _snake.First.Value.Y, SNAKE);

        Console.CursorVisible = false;
    }

    private void FindValidApplePosition()
    {
        do
        {
            _apple.X = _random.Next(1, GameWidth - 1);
            _apple.Y = _random.Next(1, GameHeight - 1);
        } while (AppleSnakeCollision());
    }

    private bool AppleSnakeCollision()
    {
        return _snake.Any(snake => _apple.X == snake.X && _apple.Y == snake.Y);
    }

    private void IncreaseSpeed(int x)
    {
        _snakeSpeed = (int)(_snakeSpeed * Math.Log(x, 10)) + 0;
    }
}