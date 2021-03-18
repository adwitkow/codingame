using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codingame
{
    class Game
    {
        static void Main(string[] args)
        {
            string magicPhrase = Console.ReadLine();

            var solver = new MagicSolver();
            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            var solution = solver.Solve(magicPhrase);

            Console.WriteLine(solution);
        }

        class MagicSolver
        {
            private readonly Stone[] Stones;
            private readonly Bilbo Bilbo;

            public MagicSolver()
            {
                Bilbo = new Bilbo();
                Stones = new Stone[Constants.StoneCount];
                for (int i = 0; i < Constants.StoneCount; i++)
                {
                    Stones[i] = new Stone(i);
                }
            }

            public string Solve(string phrase)
            {
                // TODO: strategy - figure out the absolutely most common letters
                // and pre-set them instead of setting letter by letter
                var builder = new StringBuilder();
                foreach (var ch in phrase)
                {
                    Console.Error.WriteLine($"Considering character '{ch}'");
                    var stone = FindBestStone(ch);

                    Console.Error.WriteLine($"Best stone: '{stone.Position}'");

                    builder.Append(Bilbo.GoToStone(stone));
                    builder.Append(stone.SelectLetter(ch));
                    builder.Append(Bilbo.TriggerStone());
                }
                return builder.ToString();
            }

            private Stone FindBestStone(char ch)
            {
                // TODO: This is not optimal at all
                return Stones
                    .OrderBy(s => s.DistanceTo(ch))
                    .ThenBy(s => Bilbo.DistanceTo(s))
                    .FirstOrDefault();
            }
        }

        class Bilbo
        {
            private int currentPosition;

            public string GoToStone(Stone stone)
            {
                var builder = new StringBuilder();

                var distance = stone.Position - currentPosition;

                var moveRight = distance > 0;
                if (Math.Abs(distance) > Constants.StoneCount / 2)
                {
                    moveRight = !moveRight;
                }

                Console.Error.WriteLine($"Distances from bilbo ({currentPosition}) to stone ({stone.Position}) - distance: {distance} (moveRight?: {moveRight})");
                while (currentPosition != stone.Position)
                {
                    if (moveRight)
                    {
                        builder.Append(MoveRight());
                    }
                    else
                    {
                        builder.Append(MoveLeft());
                    }
                }

                return builder.ToString();
            }

            public char TriggerStone()
            {
                return Constants.Trigger;
            }

            public int DistanceTo(Stone stone)
            {
                var baseDistance = Math.Abs(currentPosition - stone.Position);

                int distance;
                if (baseDistance > Constants.StoneCount / 2)
                {
                    distance = Constants.StoneCount - baseDistance;
                }
                else
                {
                    distance = baseDistance;
                }

                return distance;
            }

            private char MoveRight()
            {
                currentPosition = (currentPosition + 1) % Constants.StoneCount;
                return Constants.MoveRight;
            }

            private char MoveLeft()
            {
                if (currentPosition < 1)
                {
                    currentPosition = Constants.StoneCount;
                }
                currentPosition -= 1;
                return Constants.MoveLeft;
            }
        }

        class Stone
        {
            public readonly int Position;

            private int CurrentIndex { get; set; }
            private char CurrentLetter => Constants.Alphabet[CurrentIndex];

            public Stone(int position)
            {
                this.Position = position;
            }

            public int DistanceTo(char ch)
            {
                var index = Array.IndexOf(Constants.Alphabet, ch);
                var baseDistance = Math.Abs(CurrentIndex - index);

                int distance;
                if (baseDistance > Constants.Alphabet.Length / 2)
                {
                    distance = Constants.Alphabet.Length - baseDistance;
                }
                else
                {
                    distance = baseDistance;
                }

                return distance;
            }

            public string SelectLetter(char ch)
            {
                var builder = new StringBuilder();

                var index = Array.IndexOf(Constants.Alphabet, ch);
                var distance = index - CurrentIndex;
                var selectRight = distance > 0;

                if (Math.Abs(distance) > Constants.Alphabet.Length / 2)
                {
                    selectRight = !selectRight;
                }
                while (CurrentLetter != ch)
                {
                    if (selectRight)
                    {
                        builder.Append(NextLetter());
                    }
                    else
                    {
                        builder.Append(PreviousLetter());
                    }
                }

                return builder.ToString();
            }

            private char NextLetter()
            {
                if (this.CurrentIndex < Constants.Alphabet.Length - 1)
                {
                    this.CurrentIndex += 1;
                }
                else
                {
                    this.CurrentIndex = 0;
                }

                return Constants.NextLetter;
            }

            private char PreviousLetter()
            {
                if (this.CurrentIndex > 0)
                {
                    this.CurrentIndex -= 1;
                }
                else
                {
                    this.CurrentIndex = Constants.Alphabet.Length - 1;
                }

                return Constants.PreviousLetter;
            }
        }

        static class Constants
        {
            public static readonly char[] Alphabet = " ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            public static readonly int StoneCount = 30;
            public static readonly char MoveRight = '>';
            public static readonly char MoveLeft = '<';
            public static readonly char NextLetter = '+';
            public static readonly char PreviousLetter = '-';
            public static readonly char Trigger = '.';
            // public static readonly char StartLoop = '[';
            // public static readonly char EndLoop = ']';
        }
    }
}