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
    class CodinGameSponsoredContest
    {
        static void Main(string[] args)
        {
            int mapHeight = int.Parse(Console.ReadLine());
            int mapWidth = int.Parse(Console.ReadLine());

            int playerCount = int.Parse(Console.ReadLine());

            var map = new Map(mapWidth, mapHeight, playerCount - 1);

            // game loop
            while (true)
            {
                char up = char.Parse(Console.ReadLine());
                char right = char.Parse(Console.ReadLine());
                char down = char.Parse(Console.ReadLine());
                char left = char.Parse(Console.ReadLine());

                map.DisplayMap(10);

                for (int i = 0; i < playerCount - 1; i++)
                {
                    string[] inputs = Console.ReadLine().Split(' ');
                    int enemyX = int.Parse(inputs[0]);
                    int enemyY = int.Parse(inputs[1]);
                    var v = new Vector2(enemyX, enemyY);
                    map.UpdateEnemy(i, v);
                }

                var playerInputs = Console.ReadLine().Split(' ');
                var player = new Vector2(int.Parse(playerInputs[0]), int.Parse(playerInputs[1]));

                map.UpdatePlayer(player);
                map.UpdateSurroundings(player, up, right, down, left);

                var directions = map.GetAvailableMoves();
                Console.Error.WriteLine($"Available directions: {string.Join(", ", directions)}");

                var nextMove = map.GetNextMove();
                Console.Error.WriteLine($"Next move: {nextMove.Name}");

                Console.WriteLine(nextMove.Command);
            }
        }

        public class Map
        {
            private static readonly Random random = new Random();

            private static readonly string Enemy = "EE";
            private static readonly string Player = "()";
            private static readonly string Wall = "[]";
            private static readonly string Empty = "..";
            private static readonly string Unknown = "  ";

            private bool?[,] Nodes { get; }
            private readonly Vector2[] Enemies;
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
                this.PlayerPosition = player;

                SetNodeValue((int)player.X, (int)player.Y, true);
            }

            public void UpdateSurroundings(Vector2 center, char up, char right, char down, char left)
            {
                var x = (int)center.X;
                var y = (int)center.Y;

                SetNodeValue(x, y + 1, IsTraversible(down));
                SetNodeValue(x + 1, y, IsTraversible(right));
                SetNodeValue(x, y - 1, IsTraversible(up));
                SetNodeValue(x - 1, y, IsTraversible(left));
            }

            public void UpdateEnemy(int index, Vector2 position)
            {
                Enemies[index] = position;

                SetNodeValue((int)position.X, (int)position.Y, true);
            }

            public void DisplayMap(int radius)
            {
                // This needs to be void since codinGame culls longer console messages

                int playerX = (int)PlayerPosition.X;
                int playerY = (int)PlayerPosition.Y;

                var results = new string[radius * 2 + 1, radius * 2 + 1];

                var startX = playerX - radius;
                var startY = playerY - radius;

                Console.Error.WriteLine($"Converting the map (startX: {startX}, startY: {startY})");

                for (int x = startX; x < playerX + radius; x++)
                {
                    for (int y = startY; y < playerY + radius; y++)
                    {
                        var node = GetNodeValue(x, y);
                        var v = GetNode(x, y);
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

                for (int y = 0; y < results.GetLength(1); y++)
                {
                    var builder = new StringBuilder();
                    for (int x = 0; x < results.GetLength(0); x++)
                    {
                        var node = GetNode(x, y);
                        builder.Append(results[(int)node.X, (int)node.Y]);
                    }
                    Console.Error.WriteLine(builder.ToString());
                }
            }

            public IEnumerable<Direction> GetAvailableMoves()
            {
                var directions = new List<Direction>();
                var x = (int)PlayerPosition.X;
                var y = (int)PlayerPosition.Y;

                if (GetNodeValue(x + 1, y).Value)
                {
                    directions.Add(Direction.Right);
                }

                if (GetNodeValue(x, y + 1).Value)
                {
                    directions.Add(Direction.Down);
                }

                if (GetNodeValue(x - 1, y).Value)
                {
                    directions.Add(Direction.Left);
                }

                if (GetNodeValue(x, y - 1).Value)
                {
                    directions.Add(Direction.Up);
                }

                return directions;
            }

            public Direction GetNextMove()
            {
                var undiscoveredPoints = new List<Vector2>();

                for (int x = 0; x < Nodes.GetLength(0); x++)
                {
                    for (int y = 0; y < Nodes.GetLength(1); y++)
                    {
                        if (!GetNodeValue(x, y).HasValue && HasEmptyNeighbours(x, y))
                        {
                            undiscoveredPoints.Add(new Vector2(x, y));
                        }
                    }
                }

                var ordered = undiscoveredPoints
                    .OrderBy(p => Vector2.DistanceSquared(p, PlayerPosition));

                var pointsQueue = new Queue<Vector2>(ordered);

                var tries = 0;
                Direction direction = null;
                while (direction == null && pointsQueue.Any())
                {
                    Vector2 closest = pointsQueue.Dequeue();
                    direction = BFS(PlayerPosition, closest);

                    if (tries > 10)
                    {
                        return GetPanicMove();
                    }

                    tries++;
                }

                if (direction == null)
                {
                    return GetPanicMove();
                }

                return direction;
            }

            public Direction GetPanicMove()
            {
                var x = (int)PlayerPosition.X;
                var y = (int)PlayerPosition.Y;

                // TODO: CLEAN THIS UP LMAOOOO
                var roll = random.Next(0, 4);
                switch(roll)
                {
                    case 0:
                        if (IsEmpty(x + 1, y) && IsSafe(x + 1, y))
                        {
                            return Direction.Right;
                        }

                        if (IsEmpty(x - 1, y) && IsSafe(x - 1, y))
                        {
                            return Direction.Left;
                        }

                        if (IsEmpty(x, y + 1) && IsSafe(x, y + 1))
                        {
                            return Direction.Down;
                        }

                        if (IsEmpty(x, y - 1) && IsSafe(x, y - 1))
                        {
                            return Direction.Up;
                        }
                        break;
                    case 1:

                        if (IsEmpty(x - 1, y) && IsSafe(x - 1, y))
                        {
                            return Direction.Left;
                        }

                        if (IsEmpty(x, y + 1) && IsSafe(x, y + 1))
                        {
                            return Direction.Down;
                        }

                        if (IsEmpty(x, y - 1) && IsSafe(x, y - 1))
                        {
                            return Direction.Up;
                        }

                        if (IsEmpty(x + 1, y) && IsSafe(x + 1, y))
                        {
                            return Direction.Right;
                        }
                        break;
                    case 2:
                        if (IsEmpty(x, y + 1) && IsSafe(x, y + 1))
                        {
                            return Direction.Down;
                        }

                        if (IsEmpty(x, y - 1) && IsSafe(x, y - 1))
                        {
                            return Direction.Up;
                        }

                        if (IsEmpty(x + 1, y) && IsSafe(x + 1, y))
                        {
                            return Direction.Right;
                        }

                        if (IsEmpty(x - 1, y) && IsSafe(x - 1, y))
                        {
                            return Direction.Left;
                        }
                        break;
                    case 3:
                        if (IsEmpty(x, y - 1) && IsSafe(x, y - 1))
                        {
                            return Direction.Up;
                        }

                        if (IsEmpty(x + 1, y) && IsSafe(x + 1, y))
                        {
                            return Direction.Right;
                        }

                        if (IsEmpty(x - 1, y) && IsSafe(x - 1, y))
                        {
                            return Direction.Left;
                        }

                        if (IsEmpty(x, y + 1) && IsSafe(x, y + 1))
                        {
                            return Direction.Down;
                        }
                        break;
                    default:
                        return Direction.None;
                }

                return Direction.None;
            }

            private Direction BFS(Vector2 start, Vector2 goal)
            {
                // TODO: Move the pathfinding logic to a separate class
                var queue = new Queue<Pathable>();
                var visited = new HashSet<Vector2>();

                Console.Error.WriteLine($"Finding path to {goal}");

                queue.Enqueue(new Pathable(start));

                while (queue.Any())
                {
                    var current = queue.Dequeue();

                    if (visited.Contains(current.Point))
                    {
                        continue;
                    }

                    visited.Add(current.Point);

                    if (current.Point == goal)
                    {
                        Console.Error.WriteLine($"Path found: {string.Join(", ", current.Directions.Select(d => d.Name))}");
                        return current.Directions.Dequeue();
                    }

                    var x = (int)current.Point.X;
                    var y = (int)current.Point.Y;

                    if (IsEmpty(x + 1, y) && IsSafe(x + 1, y))
                    {
                        var point = GetNode(x + 1, y);
                        queue.Enqueue(new Pathable(point, current, Direction.Right));
                    }

                    if (IsEmpty(x - 1, y) && IsSafe(x - 1, y))
                    {
                        var point = GetNode(x - 1, y);
                        queue.Enqueue(new Pathable(point, current, Direction.Left));
                    }

                    if (IsEmpty(x, y + 1) && IsSafe(x, y + 1))
                    {
                        var point = GetNode(x, y + 1);
                        queue.Enqueue(new Pathable(point, current, Direction.Down));
                    }

                    if (IsEmpty(x, y - 1) && IsSafe(x, y - 1))
                    {
                        var point = GetNode(x, y - 1);
                        queue.Enqueue(new Pathable(point, current, Direction.Up));
                    }
                }

                return null;
            }

            private bool HasEmptyNeighbours(int x, int y)
            {
                return IsEmpty(x + 1, y)
                    || IsEmpty(x - 1, y)
                    || IsEmpty(x, y + 1)
                    || IsEmpty(x, y - 1);
            }

            private bool IsSafe(int x, int y)
            {
                var vector = new Vector2(x, y);
                foreach (var enemy in Enemies)
                {
                    if (enemy == vector || Vector2.DistanceSquared(enemy, vector) < 2)
                    {
                        return false;
                    }
                }

                return true;
            }

            private bool IsEmpty(int x, int y)
            {
                var node = GetNodeValue(x, y);
                return !node.HasValue || node.Value;
            }

            private bool? GetNodeValue(int x, int y)
            {
                var node = GetNode(x, y);
                return Nodes[(int)node.X, (int)node.Y];
            }

            private Vector2 GetNode(int x, int y)
            {
                if (x < 1)
                {
                    x = Nodes.GetLength(0) + x;
                }

                if (y < 1)
                {
                    y = Nodes.GetLength(1) + y;
                }

                return new Vector2(x % Nodes.GetLength(0), y % Nodes.GetLength(1));
            }

            private void SetNodeValue(int x, int y, bool value)
            {
                var node = GetNode(x, y);
                Nodes[(int)node.X, (int)node.Y] = value;
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

        public class Pathable
        {
            public Vector2 Point { get; }
            public Queue<Direction> Directions { get; }

            public Pathable(Vector2 point)
            {
                this.Point = point;
                this.Directions = new Queue<Direction>();
            }

            public Pathable(Vector2 point, Pathable parent, Direction direction)
            {
                this.Point = point;
                this.Directions = new Queue<Direction>(parent.Directions);

                Directions.Enqueue(direction);
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