using System;
using System.Collections.Generic;
using System.Linq;

namespace Codingame
{
    internal class ShadowsOfTheKnight
    {
        static void Main(string[] args)
        {
            string[] inputs;
            inputs = Console.ReadLine().Split(' ');
            int W = int.Parse(inputs[0]); // width of the building.
            int H = int.Parse(inputs[1]); // height of the building.

            var map = new bool?[W, H];
            Console.Error.WriteLine($"Width: {W}, Height: {H}");

            int N = int.Parse(Console.ReadLine()); // maximum number of turns before game over.
            inputs = Console.ReadLine().Split(' ');
            int currentX = int.Parse(inputs[0]);
            int currentY = int.Parse(inputs[1]);

            // game loop
            while (true)
            {
                string bombDir = Console.ReadLine(); // the direction of the bombs from batman's current location (U, UR, R, DR, D, DL, L or UL)

                switch (bombDir)
                {
                    case "U":
                        map = MarkKnownPoints(map, (x, y) => x == currentX && y < currentY);
                        break;
                    case "UR":
                        map = MarkKnownPoints(map, (x, y) => x > currentX && y < currentY);
                        break;
                    case "R":
                        map = MarkKnownPoints(map, (x, y) => x > currentX && y == currentY);
                        break;
                    case "DR":
                        map = MarkKnownPoints(map, (x, y) => x > currentX && y > currentY);
                        break;
                    case "D":
                        map = MarkKnownPoints(map, (x, y) => x == currentX && y > currentY);
                        break;
                    case "DL":
                        map = MarkKnownPoints(map, (x, y) => x < currentX && y > currentY);
                        break;
                    case "L":
                        map = MarkKnownPoints(map, (x, y) => x < currentX && y == currentY);
                        break;
                    case "UL":
                        map = MarkKnownPoints(map, (x, y) => x < currentX && y < currentY);
                        break;
                    default:
                        throw new InvalidOperationException("AAAAAaaaaaaaaaa");
                }

                DisplayMap(map);

                var result = SelectMiddleKnownPoint(map);

                currentX = result.X;
                currentY = result.Y;
                    
                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");


                // the location of the next window Batman should jump to.
                Console.WriteLine($"{currentX} {currentY}");
            }
        }

        private static bool?[,] MarkKnownPoints(bool?[,] map, Func<int, int, bool> belongsToTargets)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    if (!map[x,y].HasValue || map[x,y].Value)
                    {
                        map[x, y] = belongsToTargets(x, y);
                    }
                }
            }

            return map;
        }

        private static (int X, int Y) SelectMiddleKnownPoint(bool?[,] map)
        {
            var sequenceStartX = -1;
            var sequenceEndX = -1;

            var sequenceStartY = -1;
            var sequenceEndY = -1;

            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    if (map[x, y].HasValue && map[x,y].Value)
                    {
                        if (sequenceStartX == -1)
                        {
                            sequenceStartX = x;
                        }
                        
                        if (sequenceEndX <= x)
                        {
                            sequenceEndX = x;
                        }

                        if (sequenceStartY == -1)
                        {
                            sequenceStartY = y;
                        }
                        
                        if (sequenceEndY <= y)
                        {
                            sequenceEndY = y;
                        }
                    }
                }
            }

            Console.Error.WriteLine($"sequenceStartX: {sequenceStartX}; sequenceEndX: {sequenceEndX}");
            Console.Error.WriteLine($"sequenceStartY: {sequenceStartY}; sequenceEndY: {sequenceEndY}");

            var centerX = sequenceStartX + ((sequenceEndX - sequenceStartX) / 2);
            var centerY = sequenceStartY + ((sequenceEndY - sequenceStartY) / 2);

            Console.Error.WriteLine($"centerX: {centerX}; centerY: {centerY}");

            return (centerX, centerY);
        }

        private static void DisplayMap(bool?[,] map)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                var line = "";
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    if (!map[x, y].HasValue)
                    {
                        line += "?";
                    }
                    else if (map[x,y].Value)
                    {
                        line += "X";
                    }
                    else
                    {
                        line += ".";
                    }
                }
                Console.Error.WriteLine(line);
            }
        }
    }
}
