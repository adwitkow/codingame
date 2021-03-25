using System;
using System.Collections.Generic;
using System.Linq;

namespace Codingame
{
    internal class AneoSponsoredPuzzle
    {
        static void Main(string[] args)
        {
            var lights = new List<Light>();

            var maxSpeed = int.Parse(Console.ReadLine());
            int lightCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < lightCount; i++)
            {
                string[] inputs = Console.ReadLine().Split(' ');
                int distance = int.Parse(inputs[0]);
                int duration = int.Parse(inputs[1]);
                lights.Add(new Light(distance, duration));
                Console.Error.WriteLine($"{distance} {duration}");
            }

            var answer = Solve(maxSpeed, lights);

            // Write an answer using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Console.WriteLine(answer);
        }

        private static int Solve(int maxSpeed, IEnumerable<Light> lights)
        {
            var targetSpeed = 0;
            for (int speed = maxSpeed; speed > 0; speed--)
            {
                var speedInMetresPerSecond = (double)speed * 1000 / 60 / 60;
                var success = true;
                foreach (var light in lights)
                {
                    if (!light.IsGreen(speedInMetresPerSecond))
                    {
                        success = false;
                        break;
                    }

                    targetSpeed = speed;
                }

                if (success)
                {
                    return targetSpeed;
                }
            }

            return targetSpeed;
        }
            
        private struct Light
        {
            private static readonly double Tolerance = 0.0001;

            private int Distance { get; }
            private int Duration { get; }

            public Light(int distance, int duration)
            {
                this.Distance = distance;
                this.Duration = duration;
            }

            public bool IsGreen(double speed)
            {
                var secondsToTravel = Distance / speed;

                double toggles;
                if (secondsToTravel < Duration)
                {
                    toggles = 0;
                }
                else
                {
                    toggles = secondsToTravel / Duration;
                }
                
                var rounded = Math.Round(toggles);
                if (Math.Abs(toggles - rounded) < Tolerance)
                {
                    toggles = rounded;
                }

                return toggles % 2 < 1;
            }

            public override string ToString()
            {
                return $"Light (Distance: {Distance}, Duration: {Duration})";
            }
        }
    }
}
