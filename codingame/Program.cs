using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;

namespace Codingame
{
    /**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
    class Player
    {
        static void Main(string[] args)
        {
            var random = new Random();

            int mapWidth = int.Parse(Console.ReadLine());
            int mapHeight = int.Parse(Console.ReadLine());

            int playerCount = int.Parse(Console.ReadLine());

            var map = new Map(mapWidth, mapHeight, playerCount - 1);

            // game loop
            while (true)
            {
                char up = char.Parse(Console.ReadLine());
                char right = char.Parse(Console.ReadLine());
                char down = char.Parse(Console.ReadLine());
                char left = char.Parse(Console.ReadLine());

                Console.Error.WriteLine("Updating enemies");
                for (int i = 0; i < playerCount - 1; i++)
                {
                    string[] inputs = Console.ReadLine().Split(' ');
                    int enemyX = int.Parse(inputs[0]);
                    int enemyY = int.Parse(inputs[1]);
                    var v = new Vector2(enemyX - 1, enemyY - 1);
                    map.UpdateEnemy(i, v);
                }

                var playerInputs = Console.ReadLine().Split(' ');
                var player = new Vector2(int.Parse(playerInputs[0]) - 1, int.Parse(playerInputs[1]) - 1);

                map.UpdatePlayer(player);
                map.UpdateSurroundings(player, up, right, down, left);

                Console.Error.WriteLine("Displaying map");
                Console.Error.WriteLine(map.DisplayMap(5));
                // check availability of the 

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");
                var directions = map.GetAvailableMoves();
                Console.Error.WriteLine($"Available directions: {string.Join(", ", directions)}");
                Console.WriteLine(directions.ElementAt(random.Next(0, directions.Count() - 1)).Command);

                // A = Prawo
                // B = Wait
                // C = Gora
                // D = Dol
                // E = Lewo
            }
        }

        public class Map
        {
            private static readonly string Enemy = "EE";
            private static readonly string Player = "()";
            private static readonly string Wall = "[]";
            private static readonly string Empty = "..";
            private static readonly string Unknown = "  ";

            private bool?[,] Nodes { get; }
            private readonly Vector2[] Enemies;
            private Vector2 PreviousPosition;
            private Vector2 PlayerPosition;

            public Map(int width, int height, int enemyCount)
            {
                Nodes = new bool?[width, height];
                Enemies = new Vector2[enemyCount];
                PlayerPosition = Vector2.Zero;
            }

            public void UpdatePlayer(Vector2 player)
            {
                Console.Error.WriteLine($"Updating player: {player}");
                this.PreviousPosition = PlayerPosition;
                this.PlayerPosition = player;

                SetNode((int)player.X, (int)player.Y, true);
            }

            public void UpdateSurroundings(Vector2 center, char up, char right, char down, char left)
            {
                var x = (int)center.X;
                var y = (int)center.Y;

                Console.Error.WriteLine($"Updating surroundings: {center}");

                SetNode(x, y + 1, IsTraversible(down));
                SetNode(x + 1, y, IsTraversible(right));
                SetNode(x, y - 1, IsTraversible(up));
                SetNode(x - 1, y, IsTraversible(left));
            }

            public void UpdateEnemy(int index, Vector2 position)
            {
                Console.Error.WriteLine($"Updating enemy {index}: {position}");
                Enemies[index] = position;

                SetNode((int)position.X, (int)position.Y, true);
            }

            public string DisplayMap(int radius)
            {
                int playerX = (int)PlayerPosition.X;
                int playerY = (int)PlayerPosition.Y;

                var results = new string[radius * 2 + 1, radius * 2 + 1];

                var startX = playerX - radius;
                var startY = playerY - radius;

                Console.Error.WriteLine("Converting the map");

                for (int x = startX; x < playerX + radius; x++)
                {
                    for (int y = startY; y < playerY + radius; y++)
                    {
                        var node = GetNode(x, y);
                        //var node = Nodes[x, y];
                        var v = new Vector2(x, y);
                        string symbol;
                        if (PlayerPosition == v)
                        {
                            symbol = Player;
                        }
                        else if (Enemies.Contains(v))
                        {
                            symbol = Enemy;
                        }
                        else
                        {
                            symbol = ConvertNodeToSymbol(node);
                        }
                        results[x - startX, y - startY] = symbol;
                    }
                }

                Console.Error.WriteLine("Building the string");

                var builder = new StringBuilder();
                for (int y = 0; y < results.GetLength(1); y++)
                {
                    for (int x = 0; x < results.GetLength(0); x++)
                    {
                        builder.Append(results[x % results.GetLength(0), y % results.GetLength(1)]);
                    }
                    builder.Append(Environment.NewLine);
                }

                return builder.ToString();
            }

            public IEnumerable<Direction> GetAvailableMoves()
            {
                var directions = new List<Direction>();
                var x = (int)PlayerPosition.X;
                var y = (int)PlayerPosition.Y;

                if (GetNode(x + 1, y).Value)
                {
                    directions.Add(Direction.Right);
                }

                if (GetNode(x, y + 1).Value)
                {
                    directions.Add(Direction.Down);
                }

                if (GetNode(x - 1, y).Value)
                {
                    directions.Add(Direction.Left);
                }

                if (GetNode(x, y - 1).Value)
                {
                    directions.Add(Direction.Up);
                }

                return directions;
            }

            private bool? GetNode(int x, int y)
            {
                return Nodes[x % Nodes.GetLength(0), y % Nodes.GetLength(1)];
            }

            private void SetNode(int x, int y, bool value)
            {
                Nodes[x % Nodes.GetLength(0), y % Nodes.GetLength(1)] = value;
            }

            private string ConvertNodeToSymbol(bool? node)
            {
                if (!node.HasValue)
                {
                    return Unknown;
                }
                else if (node.Value)
                {
                    return Empty;
                }
                else
                {
                    return Wall;
                }
            }

            private bool IsTraversible(char ch)
            {
                return ch != '#';
            }
        }

        public class Direction
        {
            public static readonly Direction Up = new Direction("Up", "C");
            public static readonly Direction Right = new Direction("Right", "A");
            public static readonly Direction Down = new Direction("Down", "D");
            public static readonly Direction Left = new Direction("Left", "E");
            public static readonly Direction None = new Direction("None", "B");

            public readonly string Command;
            public readonly string Name;

            private Direction(string name, string command)
            {
                this.Name = name;
                this.Command = command;
            }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}