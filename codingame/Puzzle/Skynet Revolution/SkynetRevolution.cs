using System;
using System.Collections.Generic;
using System.Linq;

namespace Codingame
{
    internal class SkynetRevolution
    {
        private static void Main(string[] args)
        {
            var inputs = Console.ReadLine().Split(' ');
            var N = int.Parse(inputs[0]); // the total number of nodes in the level, including the gateways
            var L = int.Parse(inputs[1]); // the number of links
            var E = int.Parse(inputs[2]); // the number of exit gateways

            for (var i = 0; i < L; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var N1 = int.Parse(inputs[0]); // N1 and N2 defines a link between these nodes
                var N2 = int.Parse(inputs[1]);

                var firstNode = Node.GetById(N1);
                var secondNode = Node.GetById(N2);

                Node.Link(firstNode, secondNode);
            }
            for (var i = 0; i < E; i++)
            {
                var EI = int.Parse(Console.ReadLine()); // the index of a gateway node
                Node.GetById(EI).IsGateWay = true;
            }

            // game loop
            while (true)
            {
                var SI = int.Parse(Console.ReadLine()); // The index of the node on which the Skynet agent is positioned this turn

                var link = FindBestLink(Node.GetById(SI));

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                // Example: 3 4 are the indices of the nodes you wish to sever the link between

                Console.WriteLine($"{link.nodeId1} {link.nodeId2}");
            }
        }

        private static (int nodeId1, int nodeId2) FindBestLink(Node startNode)
        {
            var nodes = new Queue<(Node Node, List<Node> Path)>();
            nodes.Enqueue((startNode, new List<Node>() { startNode }));
            var visited = new HashSet<int>();

            var pathsToGateways = new List<List<Node>>();

            while (nodes.Any())
            {
                var pair = nodes.Dequeue();
                var node = pair.Node;
                var path = pair.Path;

                if (node.IsGateWay)
                {
                    pathsToGateways.Add(path);
                    continue;
                }

                if (visited.Contains(node.Id))
                {
                    continue;
                }

                visited.Add(node.Id);

                foreach (var neighbour in node.Neighbours)
                {
                    if (node == startNode && neighbour.IsGateWay)
                    {
                        Node.Unlink(node, neighbour);

                        return (node.Id, neighbour.Id);
                    }

                    if (!visited.Contains(neighbour.Id))
                    {
                        nodes.Enqueue((neighbour, new List<Node>(path) { neighbour }));
                    }
                }
            }

            Console.Error.WriteLine($"Finished scanning.");

            var allTargets = new List<Node>();
            var distances = new Dictionary<Node, int>();

            Console.Error.WriteLine($"All paths: {pathsToGateways.Count}");
            var toIgnore = new List<Node>();

            foreach (var path in pathsToGateways)
            {

                Node beforeLast;
                if (path.Count > 1)
                {
                    beforeLast = path.Reverse<Node>().Skip(1).Take(1).FirstOrDefault();
                }
                else
                {
                    beforeLast = path.FirstOrDefault();
                }

                if (beforeLast.Id == 17)
                {
                    Console.Error.WriteLine(string.Join("->", path.Select(node => node.Id)));
                }

                foreach (var node in path.Skip(1).Take(path.Count - 2))
                {
                    if (!node.Neighbours.Any(neighbour => neighbour.IsGateWay))
                    {
                        Console.Error.WriteLine($"Ignoring {beforeLast.Id} since {node.Id} is not connected to any gateway.");
                        toIgnore.Add(beforeLast);
                    }
                }

                allTargets.Add(beforeLast);

                if (distances.ContainsKey(beforeLast))
                {
                    if (path.Count < distances[beforeLast])
                    {
                        distances[beforeLast] = path.Count;
                    }
                }
                else
                {
                    distances.Add(beforeLast, path.Count);
                }

            }

            Console.Error.WriteLine($"To ignore: {string.Join(", ", toIgnore.Select(node => node.Id))}");

            var targetsWithTargetNeighbours = allTargets.Where(target => target.Neighbours.Any(neighbour => allTargets.Contains(neighbour)));

            Console.Error.WriteLine($"targets with target neighbours: {targetsWithTargetNeighbours.Count()}");

            var multiGatewayTargets = allTargets.Where(target => target.Neighbours.Count(neighbour => neighbour.IsGateWay) > 1);

            Console.Error.WriteLine($"multiGatewayTargets: {multiGatewayTargets.Count()}");

            var multiGatewayOrdered = multiGatewayTargets.OrderBy(target => distances[target]);

            var firstMulti = multiGatewayOrdered.FirstOrDefault();
            if (firstMulti != null && distances[firstMulti] < 4)
            {
                var s = firstMulti;
                var g = s.Neighbours.FirstOrDefault(node => node.IsGateWay);

                Node.Unlink(s, g);

                return (s.Id, g.Id);
            }

            IEnumerable<Node> targets;
            if (targetsWithTargetNeighbours.Any())
            {
                targets = targetsWithTargetNeighbours;
            }
            else if (multiGatewayTargets.Any())
            {
                targets = multiGatewayTargets;
            }
            else
            {
                targets = allTargets;
            }

            var withoutIgnored = targets.Except(toIgnore);
            Console.Error.WriteLine($"Without ignored: {string.Join(", ", withoutIgnored.Select(node => node.Id))}");
            if (withoutIgnored.Any())
            {
                targets = withoutIgnored;
            }

            var selection = targets
                .OrderByDescending(n => n.Neighbours.Count(neighbour => neighbour.IsGateWay))
                .ThenBy(n => distances[n])
                .FirstOrDefault();

            Console.Error.WriteLine($"Picked node {selection.Id}");

            var gateway = selection.Neighbours.FirstOrDefault(node => node.IsGateWay);

            Node.Unlink(selection, gateway);

            return (selection.Id, gateway.Id);
        }

        private class Node
        {
            private static readonly Dictionary<int, Node> Lookup = new Dictionary<int, Node>();

            public static Node GetById(int id)
            {
                var exists = Lookup.TryGetValue(id, out var result);

                if (!exists)
                {
                    result = new Node(id);
                }

                return result;
            }

            public static void Link(Node node1, Node node2)
            {
                if (!node1.Neighbours.Contains(node2))
                {
                    node1.Neighbours.Add(node2);
                }

                if (!node2.Neighbours.Contains(node1))
                {
                    node2.Neighbours.Add(node1);
                }
            }

            public static void Unlink(Node node1, Node node2)
            {
                if (node1.Neighbours.Contains(node2))
                {
                    node1.Neighbours.Remove(node2);
                }

                if (node2.Neighbours.Contains(node1))
                {
                    node2.Neighbours.Remove(node1);
                }
            }

            public int Id { get; }
            public bool IsGateWay { get; set; }

            public List<Node> Neighbours { get; }

            public Node(int id)
            {
                Id = id;
                Neighbours = new List<Node>();

                Lookup.Add(id, this);
            }

            public bool Equals(Node node)
            {
                if (node is null)
                {
                    return false;
                }

                return Id == node.Id;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as Node);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }
    }
}
