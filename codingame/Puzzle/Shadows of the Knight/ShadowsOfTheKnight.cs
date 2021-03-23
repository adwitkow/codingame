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

            Console.Error.WriteLine($"Width: {W}, Height: {H}");

            int N = int.Parse(Console.ReadLine()); // maximum number of turns before game over.
            inputs = Console.ReadLine().Split(' ');
            int currentX = int.Parse(inputs[0]);
            int currentY = int.Parse(inputs[1]);

            var boundsX1 = 0;
            var boundsX2 = W - 1;
            var boundsY1 = 0;
            var boundsY2 = H - 1;

            // game loop
            while (true)
            {
                string bombDir = Console.ReadLine(); // the direction of the bombs from batman's current location (U, UR, R, DR, D, DL, L or UL)

                switch (bombDir)
                {
                    case "U":
                        boundsX1 = currentX;
                        boundsX2 = currentX;
                        boundsY2 = currentY - 1;
                        break;
                    case "UR":
                        boundsX1 = currentX + 1;
                        boundsY2 = currentY - 1;
                        break;
                    case "R":
                        boundsX1 = currentX + 1;
                        boundsY1 = currentY;
                        boundsY2 = currentY;
                        break;
                    case "DR":
                        boundsX1 = currentX + 1;
                        boundsY1 = currentY + 1;
                        break;
                    case "D":
                        boundsX1 = currentX;
                        boundsX2 = currentX;
                        boundsY1 = currentY + 1;
                        break;
                    case "DL":
                        boundsX2 = currentX - 1;
                        boundsY1 = currentY + 1;
                        break;
                    case "L":
                        boundsX2 = currentX - 1;
                        boundsY1 = currentY;
                        boundsY2 = currentY;
                        break;
                    case "UL":
                        boundsX2 = currentX - 1;
                        boundsY2 = currentY - 1;
                        break;
                    default:
                        throw new InvalidOperationException("AAAAAaaaaaaaaaa");
                }

                var centerX = boundsX1 + ((boundsX2 - boundsX1) / 2);
                var centerY = boundsY1 + ((boundsY2 - boundsY1) / 2);

                currentX = centerX;
                currentY = centerY;
                    
                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");


                // the location of the next window Batman should jump to.
                Console.WriteLine($"{currentX} {currentY}");
            }
        }
    }
}
