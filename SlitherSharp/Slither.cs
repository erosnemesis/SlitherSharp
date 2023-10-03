using System.Runtime.CompilerServices;
using SlitherSharp.Models;
using static SlitherSharp.Constants;

namespace SlitherSharp;

internal class Slither
{
    private Direction _currentDirection = Direction.Right;

    private int _snakeSpeed = 200;
    private LinkedList<Snake> _snake;

    private int _applesCollected = 0;

    private Apple _applePosition = new Apple();

    private Random _random = new Random(DateTime.Now.Millisecond);

    public Slither()
    {
        _snake = new LinkedList<Snake>();
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
        await SetSnakePosition(_snake.Last);
    }

    private async Task SetSnakePosition(LinkedListNode<Snake> snakeNode)
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
            await SetSnakePosition(snakeNode.Previous); // recurse through the linked list.
            return;
        }

        SetCurrentDirection(snakeNode);

        
    }

    private void SetCurrentDirection(LinkedListNode<Snake> snakeNode)
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
        if (_applePosition.X == _snake.First.Value.X && _applePosition.Y == _snake.First.Value.Y)
        {
            _applesCollected += 1;

            _snake.AddFirst(new Snake { X = _applePosition.X, Y = _applePosition.Y });

            FindValidApplePosition();

            SetAndWrite(_applePosition.X, _applePosition.Y, APPLE);
        }

        await Task.CompletedTask;
    }

    private bool AppleSnakeCollision()
    {
        return _snake.Any(snake => _applePosition.X == snake.X && _applePosition.Y == snake.Y);
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
            Console.SetWindowSize(ConsoleWidth, ConsoleHeight);
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

        SetAndWrite(_applePosition.X, _applePosition.Y, APPLE);

        SetAndWrite(_snake.First.Value.X, _snake.First.Value.Y, SNAKE);

        Console.CursorVisible = false;
    }

    private void FindValidApplePosition()
    {
        do
        {
            _applePosition.X = _random.Next(1, GameWidth - 1);
            _applePosition.Y = _random.Next(1, GameHeight - 1);
        } while (AppleSnakeCollision());
    }
}