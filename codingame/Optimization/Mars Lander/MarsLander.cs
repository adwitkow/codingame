using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Codingame
{
    internal class MarsLander
    {
        public static readonly double Gravity = 3.711d;
        public static Vector2 LandingPoint1 = Vector2.Zero;
        public static Vector2 LandingPoint2 = Vector2.Zero;
        public static ICollection<Vector2> LandPoints;

        private static void Main(string[] args)
        {
            var lander = new Lander();
            LandPoints = new List<Vector2>();

            string[] inputs;
            var N = int.Parse(Console.ReadLine()); // the number of points used to draw the surface of Mars.
            for (var i = 0; i < N; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var landX = int.Parse(inputs[0]); // X coordinate of a surface point. (0 to 6999)
                var landY = int.Parse(inputs[1]); // Y coordinate of a surface point. By linking all the points together in a sequential fashion, you form the surface of Mars.

                var currentPoint = new Vector2(landX, landY);

                if (LandPoints.Any())
                {
                    var lastPoint = LandPoints.Last();
                    if (lastPoint.Y == currentPoint.Y)
                    {
                        LandingPoint1 = lastPoint;
                        LandingPoint2 = currentPoint;
                    }
                }

                LandPoints.Add(currentPoint);
            }

            // game loop
            while (true)
            {
                inputs = Console.ReadLine().Split(' ');
                var X = int.Parse(inputs[0]);
                var Y = int.Parse(inputs[1]);
                var HS = int.Parse(inputs[2]); // the horizontal speed (in m/s), can be negative.
                var VS = int.Parse(inputs[3]); // the vertical speed (in m/s), can be negative.

                lander.SetPosition(X, Y);
                lander.SetSpeed(HS, VS);
                lander.Fuel = int.Parse(inputs[4]);     // the quantity of remaining fuel in liters.
                lander.Rotation = int.Parse(inputs[5]); // the rotation angle in degrees (-90 to 90).
                lander.Power = int.Parse(inputs[6]);    // the thrust power (0 to 4).

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                // R P. R is the desired rotation angle. P is the desired thrust power.
                if (!lander.IsAbovePlatform())
                {
                    var command = lander.GoTowardsPlatform();
                    Console.WriteLine(command);
                }
                else
                {
                    if (lander.IsStable())
                    {
                        Console.WriteLine("0 0");
                    }
                    else
                    {
                        var command = lander.Stabilize();
                        Console.WriteLine(command);
                    }
                }
            }
        }

        private class Lander
        {
            private static readonly Vector2 MaxSpeed = new Vector2(20, 40);

            public Vector2 Position { get; private set; }
            public Vector2 Speed { get; private set; }
            public Vector2 Momentum { get; private set; }
            public int Fuel { get; set; }
            public int Rotation { get; set; }
            public int Power { get; set; }

            public void SetPosition(int x, int y)
            {
                Position = new Vector2(x, y);
            }

            public void SetSpeed(int hSpeed, int vSpeed)
            {
                var oldSpeed = Speed;
                Speed = new Vector2(hSpeed, vSpeed);
                Momentum = oldSpeed - Speed;
            }

            public string Stabilize()
            {
                var angle = 0;
                var power = 4;

                if (Speed.X > 50)
                {
                    angle = 90;
                }
                else if (Speed.X > 35)
                {
                    angle = 35;
                }
                else if (Speed.X > 5)
                {
                    angle = 22;
                }

                if (Speed.X < -50)
                {
                    angle = -90;
                }
                else if (Speed.X < -35)
                {
                    angle = -35;
                }
                else if (Speed.X < -5)
                {
                    angle = -22;
                }

                if (Position.Y - LandingPoint1.Y < 20)
                {
                    angle = 0;
                }

                var hSpeedAbs = Math.Abs(Speed.X); // TODO

                return $"{angle} {power}";
            }

            public bool IsStable()
            {
                if (Momentum.Y > 0)
                {
                    return false;
                }

                return Rotation == 0 && Math.Abs(Speed.X) < MaxSpeed.X && Math.Abs(Speed.Y) < MaxSpeed.Y;
            }

            public bool IsAbovePlatform()
            {
                return Position.X > LandingPoint1.X && Position.X < LandingPoint2.X;
            }

            public string GoTowardsPlatform()
            {
                var power = 4;
                var angle = 0;

                if (Speed.X > 50)
                {
                    angle = 35;
                }

                if (Speed.X < -50)
                {
                    angle = -35;
                }

                if (Position.X > LandingPoint2.X && Speed.X > -40)
                {
                    angle = 35;
                }

                if (Position.X < LandingPoint1.X && Speed.X < 40)
                {
                    angle = -35;
                }

                if ((Position.Y > 2900 && Speed.Y > 0) || (Math.Abs(Speed.X) > 25 && Math.Abs(Speed.Y) < 3))
                {
                    power = 0;
                }

                return $"{angle} {power}";
            }
        }
    }
}
