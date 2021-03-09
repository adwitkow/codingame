namespace Hackathon
{
    using System;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Numerics;
    using System.Collections;
    using System.Collections.Generic;

    /**
     * Auto-generated code below aims at helping you parse
     * the standard input according to the problem statement.
     **/
    class FallInLoveWithIA
    {
        public static readonly Vector2 MapBounds = new Vector2(17630, 9000);
        public static float DistanceToMiddleSquared = 0;
        public static int heroesPerPlayer = -1;

        // Ugh.
        public static Base PlayerBase;
        public static Base EnemyBase;

        static void Main(string[] args)
        {
            string[] inputs;

            PlayerBase = new Base(Console.ReadLine());
            EnemyBase = PlayerBase.OppositeBase();

            var middleVector = (EnemyBase.Position - PlayerBase.Position) / 2.2f;
            DistanceToMiddleSquared = Vector2.DistanceSquared(Vector2.Zero, middleVector);

            var controller = new UnitController();

            heroesPerPlayer = int.Parse(Console.ReadLine());

            // game loop
            while (true)
            {
                PlayerBase.UpdateHealthAndMana(Console.ReadLine());
                EnemyBase.UpdateHealthAndMana(Console.ReadLine());

                int visibleEntityCount = int.Parse(Console.ReadLine());

                var heroes = new List<Entity>();
                var opponents = new List<Entity>();
                var monsters = new List<Entity>();

                for (int i = 0; i < visibleEntityCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int id = int.Parse(inputs[0]);
                    int type = int.Parse(inputs[1]);
                    int x = int.Parse(inputs[2]);
                    int y = int.Parse(inputs[3]);
                    int shieldLife = int.Parse(inputs[4]);
                    int isControlled = int.Parse(inputs[5]);
                    int health = int.Parse(inputs[6]);
                    int vx = int.Parse(inputs[7]);
                    int vy = int.Parse(inputs[8]);
                    int nearBase = int.Parse(inputs[9]);
                    int threatFor = int.Parse(inputs[10]);

                    var entity = Entity.GetById(id);
                    if (entity == null)
                    {
                        entity = new Entity(id, type, x, y, shieldLife, isControlled, health, vx, vy, nearBase, threatFor);
                    }
                    else
                    {
                        entity.Update(x, y, shieldLife, isControlled, health, vx, vy, nearBase, threatFor);
                    }

                    switch (entity.Type)
                    {
                        case EntityType.Player:
                            heroes.Add(entity);
                            break;
                        case EntityType.Opponent:
                            opponents.Add(entity);
                            break;
                        case EntityType.Monster:
                            monsters.Add(entity);
                            break;
                        default:
                            throw new InvalidOperationException($"Invalid entity type for entity {id}");
                    }
                }

                var commands = controller.Update(heroes, opponents, monsters);
                foreach (var commandPair in commands.OrderBy(pair => pair.Key))
                {
                    Console.WriteLine(commandPair.Value);
                }
            }
        }

        public class UnitController
        {
            private IEnumerable<Vector2> AttackNodes = EnemyBase.GetAttackNodes();

            private List<BasePlayable> Playables = new List<BasePlayable>();
            private int stage = 0;
            private int iteration;

            public Dictionary<int, Command> Update(ICollection<Entity> heroes, ICollection<Entity> opponents, ICollection<Entity> monsters)
            {
                var commands = new Dictionary<int, Command>();

                if (monsters.Any())
                {
                    stage = Math.Max(stage, monsters.Max(m => m.Health));
                }

                if (!Playables.Any())
                {
                    Playables.Add(new Defender(heroes.First(), PlayerBase.GetPatrolNodes(7000, 8).Take(6), AttackNodes));
                    Playables.Add(new Midfielder(heroes.Skip(1).First(), 10000, PlayerBase.GetPatrolNodes(8000, 8).Take(7).Take(8), AttackNodes));
                    Playables.Add(new Midfielder(heroes.Skip(2).First(), 12200, PlayerBase.GetPatrolNodes(10000, 16).Take(8).Reverse(), AttackNodes));
                }

                if (stage > 18 && iteration < 1)
                {
                    Console.Error.WriteLine($"Recreating the playables... (stage: {stage}, mana: {PlayerBase.Mana}, iteration: {iteration})");

                    Playables.Clear();
                    Playables.Add(new Defender(heroes.First(), PlayerBase.GetPatrolNodes(7000, 8).Take(3), AttackNodes));
                    Playables.Add(new Midfielder(heroes.Skip(1).First(), 10000, PlayerBase.GetPatrolNodes(7000, 8).Skip(4).Take(3), AttackNodes));
                    Playables.Add(new Attacker(heroes.Skip(2).First(), EnemyBase.GetPatrolNodes(6200, 8).Take(6), AttackNodes));
                    iteration++;
                }

                if (stage > 20 && PlayerBase.Mana < 50 && iteration == 1)
                {
                    Console.Error.WriteLine($"Recreating the playables... (stage: {stage}, mana: {PlayerBase.Mana}, iteration: {iteration})");

                    Playables.Clear();
                    Playables.Add(new Attacker(heroes.Skip(2).First(), EnemyBase.GetPatrolNodes(7000, 8).Take(6), AttackNodes));
                    Playables.Add(new Defender(heroes.First(), PlayerBase.GetPatrolNodes(7000, 8).Take(3), AttackNodes));
                    Playables.Add(new Defender(heroes.Skip(1).First(), PlayerBase.GetPatrolNodes(7000, 8).Skip(4).Take(3), AttackNodes));

                    iteration++;
                }
                // TODO: Could be worth it to try and setup a few mana ranges with different strategies:
                // < 50 mana: defend and farm mana
                // 100+ mana: keep a single guy in defense and try to murder the opponent
                // 200+ mana: walk around the map and CONTROL every single living entity toward the enemy base

                // ignore monsters that are going towards the enemy
                // actually we could even WIND them to get there quicker if we have enough mana

                var mana = PlayerBase.Mana;
                foreach (var playable in Playables)
                {
                    var command = playable.Update(mana, monsters, opponents);
                    commands.Add(playable.Hero.Id, command);
                    if (command is SpellCommand)
                    {
                        mana -= 10;
                    }
                    else if (command is AttackCommand attack && !attack.Entity.IsShielded)
                    {
                        monsters.Remove(attack.Entity);
                    }
                }

                return commands;
            }
        }

        public class Base
        {
            public static readonly int RadiusSquared = 5000 * 5000;
            public static readonly int DangerRadiusSquared = 6500 * 6500;
            public static readonly int PatrolRadius = 7400; // 6000 of base view range + arbitrary number
            public static readonly int AttackRadius = 5000;

            public readonly Vector2 Position;
            public int Health;
            public int Mana;

            public Base(string initInput)
            {
                var inputs = initInput.Split(' ');
                int x = int.Parse(inputs[0]);
                int y = int.Parse(inputs[1]);

                this.Position = new Vector2(x, y);
            }

            public Base(Vector2 position)
            {
                this.Position = position;
            }

            public Base OppositeBase()
            {
                var oppositeVector = Vector2.Abs(this.Position - MapBounds);
                return new Base(oppositeVector);
            }

            public bool IsWithinRadius(Vector2 point)
            {
                return Vector2.DistanceSquared(Position, point) < RadiusSquared;
            }

            public void UpdateHealthAndMana(string input)
            {
                var segments = input.Split(' ');
                Health = int.Parse(segments[0]);
                Mana = int.Parse(segments[1]);
            }

            public IEnumerable<Vector2> GetPatrolNodes(int distance, int nodes)
            {
                return GetPointsInRadius(distance, nodes);
            }

            public IEnumerable<Vector2> GetAttackNodes()
            {
                var nodes = 16;
                return GetPointsInRadius(AttackRadius, nodes).Skip(1).Take(nodes - 2);
            }

            private IEnumerable<Vector2> GetPointsInRadius(int radius, int points)
            {
                var results = new List<Vector2>();
                var perimeter = Math.Round(radius * Math.PI * 0.5d); // 2PIr / 4 -> 0.5PIr
                Console.Error.WriteLine($"perimeter {perimeter}");
                // topleft base
                double baseDegree;
                if (this.Position == Vector2.Zero)
                {
                    baseDegree = 15;
                }
                else
                {
                    baseDegree = 195;
                }

                var step = 75d / points;
                for (int i = 0; i < points; i++)
                {
                    var degree = baseDegree + (step * i);
                    var degreeRadians = degree * (Math.PI / 180);
                    var x = (int)Math.Round(this.Position.X + radius * Math.Cos(degreeRadians));
                    var y = (int)Math.Round(this.Position.Y + radius * Math.Sin(degreeRadians));

                    var result = new Vector2(x, y);

                    results.Add(result);
                }

                return results;
            }
        }

        public class Entity
        {
            private static readonly Dictionary<int, Entity> Lookup = new Dictionary<int, Entity>();

            public static Entity GetById(int id)
            {
                Lookup.TryGetValue(id, out Entity result);
                return result;
            }

            public static IEnumerable<Entity> All()
            {
                return Lookup.Values;
            }

            public int Id { get; }
            public EntityType Type { get; }
            public Vector2 Position { get; private set; }
            public int ShieldLeft { get; private set; }
            public bool IsControlled { get; private set; }
            public int Health { get; private set; }
            public Vector2 Speed { get; private set; }
            public Base TargetedBase { get; private set; }
            public Base ThreatenedBase { get; private set; }
            public float DistanceToPlayerBaseSquared { get; private set; }
            public float DistanceToEnemyBaseSquared { get; private set; }
            public int ShieldDelay { get; private set; }

            public Vector2 NextPosition => Position + Speed;
            public bool IsShielded => ShieldLeft > 0 || ShieldDelay > 0;
            public bool IsThreateningEnemy => ThreatenedBase == EnemyBase;
            public bool IsOnPlayersHalfOfMap => DistanceToPlayerBaseSquared < DistanceToMiddleSquared;

            public Entity(int id, int type, int x, int y, int shieldLife, int isControlled, int health, int vX, int vY, int nearBase, int threatFor)
            {
                Lookup.Add(id, this);

                this.Id = id;
                this.Type = (EntityType)type;

                Update(x, y, shieldLife, isControlled, health, vX, vY, nearBase, threatFor);
            }

            public void Update(int x, int y, int shieldLife, int isControlled, int health, int vX, int vY, int nearBase, int threatFor)
            {
                this.Position = new Vector2(x, y);
                this.ShieldLeft = shieldLife;
                this.IsControlled = isControlled != 0;
                this.Health = health;
                this.Speed = new Vector2(vX, vY);

                if (ShieldDelay > 0)
                {
                    ShieldDelay--;
                }

                ComputeTargetedBase(nearBase, threatFor);

                DistanceToPlayerBaseSquared = Vector2.DistanceSquared(Position, PlayerBase.Position);
                DistanceToEnemyBaseSquared = Vector2.DistanceSquared(Position, EnemyBase.Position);
            }

            public float DistanceToEntitySquared(Entity entity)
            {
                return Vector2.DistanceSquared(Position, entity.Position);
            }

            public void CastedShield()
            {
                ShieldDelay = 2;
            }

            private void ComputeTargetedBase(int nearBase, int threatFor)
            {
                Base threatenedBase;
                switch (threatFor)
                {
                    case 1:
                        threatenedBase = PlayerBase;
                        break;
                    case 2:
                        threatenedBase = EnemyBase;
                        break;
                    default:
                        threatenedBase = null;
                        break;
                }

                this.ThreatenedBase = threatenedBase;

                if (nearBase == 1)
                {
                    this.TargetedBase = threatenedBase;
                }
                else
                {
                    this.TargetedBase = null;
                }
            }
        }

        public abstract class Command
        {
            public abstract string Keyword { get; }
            public string Message { get; set; }

            protected Command(string message)
            {
                this.Message = message;
            }

            public abstract string ConvertToString();

            public override string ToString()
            {
                return ConvertToString();
            }
        }

        public class MoveCommand : Command
        {
            public override string Keyword => "MOVE";

            public readonly Vector2 Destination;

            public MoveCommand(Vector2 destination)
                : base($"Moving to [{destination.X},{destination.Y}]")
            {
                this.Destination = destination;
            }

            public override string ConvertToString()
            {
                return $"{Keyword} {Destination.X} {Destination.Y} {Message}";
            }
        }

        public class AttackCommand : Command
        {
            public override string Keyword => "MOVE";

            public readonly Vector2 Destination;
            public readonly Entity Entity;

            public AttackCommand(Entity entity)
                : base($"Attackimg {entity.Type} {entity.Id}")
            {
                this.Entity = entity;

                if (entity.Type == EntityType.Monster)
                {
                    this.Destination = entity.NextPosition;
                }
                else
                {
                    this.Destination = entity.Position;
                }
            }

            public override string ConvertToString()
            {
                return $"{Keyword} {Destination.X} {Destination.Y} {Message}";
            }
        }

        public class WaitCommand : Command
        {
            public override string Keyword => "WAIT";

            public WaitCommand()
                : base("Nothing to do :)") { }

            public override string ConvertToString()
            {
                return $"{Keyword} {Message}";
            }
        }

        public abstract class SpellCommand : Command
        {
            public override string Keyword => "SPELL";

            public SpellType Type { get; }

            protected SpellCommand(SpellType type, string message)
                : base(message)
            {
                this.Type = type;
            }
        }

        public class WindSpell : SpellCommand
        {
            public Vector2 Destination { get; }

            public WindSpell(Vector2 destination)
                : base(SpellType.Wind, $"Casting WIND towards {destination}")
            {
                int x = (int)destination.X;
                int y = (int)destination.Y;

                if (x == 0)
                {
                    x = 5;
                }
                if (x == MapBounds.X)
                {
                    x = (int)MapBounds.X - 5;
                }

                if (y == 0)
                {
                    y = 5;
                }
                if (y == MapBounds.Y)
                {
                    y = (int)MapBounds.Y - 5;
                }

                this.Destination = new Vector2(x, y);
            }

            public override string ConvertToString()
            {
                return $"{Keyword} {this.Type.ToString().ToUpper()} {Destination.X} {Destination.Y} {Message}";
            }
        }

        public class ShieldSpell : SpellCommand
        {
            public int EntityId { get; }

            public ShieldSpell(int entityId)
                : base(SpellType.Shield, $"Casting SHIELD at entity {entityId}")
            {
                this.EntityId = entityId;
            }

            public override string ConvertToString()
            {
                return $"{Keyword} {this.Type.ToString().ToUpper()} {EntityId} {Message}";
            }
        }

        public class ControlSpell : SpellCommand
        {
            public int EntityId { get; }
            public Vector2 Destination { get; }

            public ControlSpell(int entityId, Vector2 destination)
                : base(SpellType.Control, $"Casting CONTROL at entity {entityId} towards {destination}")
            {
                this.EntityId = entityId;
                this.Destination = destination;
            }

            public override string ConvertToString()
            {
                return $"{Keyword} {this.Type.ToString().ToUpper()} {EntityId} {Destination.X} {Destination.Y} {Message}";
            }
        }

        public class Attacker : BasePlayable
        {
            private static readonly int AggressionRange = 7300 * 7300;
            private static readonly int EnemyWindRangeSquared = 7150 * 7150;
            private static readonly int EnemyShieldRangeSquared = 4600 * 4600;

            private Dictionary<int, int> BaseMonsterTicks = new Dictionary<int, int>(); // monster id, ticks

            private int stage = 0;

            public Attacker(Entity hero, IEnumerable<Vector2> patrolPoints, IEnumerable<Vector2> attackPoints)
                : base(hero, patrolPoints, attackPoints) { }

            public override Command Update(int mana, IEnumerable<Entity> monsters, IEnumerable<Entity> opponents)
            {
                if (monsters.Any())
                {
                    var max = monsters.Max(m => m.Health);
                    stage = Math.Max(stage, max);
                }

                for (int i = BaseMonsterTicks.Count - 1; i >= 0; i--)
                {
                    var tickPair = BaseMonsterTicks.ElementAt(i);

                    BaseMonsterTicks[tickPair.Key]--;
                    if (BaseMonsterTicks[tickPair.Key] < 1)
                    {
                        BaseMonsterTicks.Remove(tickPair.Key);
                    }
                }

                var visibleMonstersInBase = monsters.Where(m => m.DistanceToEnemyBaseSquared < Base.RadiusSquared && !m.IsShielded);
                foreach (var m in visibleMonstersInBase)
                {
                    ResetBaseTicks(m);
                }

                Console.Error.WriteLine("Monsters in enemy base: " + string.Join(", ", BaseMonsterTicks.Select(m => m.Key)));

                var closeMonster = monsters
                    .Where(m => m.DistanceToEntitySquared(Hero) < ViewRangeSquared && m.DistanceToEnemyBaseSquared < AggressionRange && !m.IsShielded)
                    .OrderBy(m => m.DistanceToEntitySquared(Hero) < ViewRangeSquared)
                    .FirstOrDefault();

                if (closeMonster != null)
                {
                    if (stage <= 20 && !closeMonster.IsThreateningEnemy)
                    {
                        Console.Error.WriteLine("1");
                        return new AttackCommand(closeMonster);
                    }
                    else
                    {
                        var lowHealth = closeMonster.Health < 5;
                        var enoughMana = mana > 10;
                        var isWithinShieldRadius = IsWithinShieldRadius(closeMonster);
                        var closeEnoughForShield = closeMonster.DistanceToEnemyBaseSquared < EnemyShieldRangeSquared;
                        var isWithinWindRadius = IsWithinWindRadius(closeMonster);
                        var isShielded = closeMonster.IsShielded;
                        var closeEnoughForWind = closeMonster.DistanceToEnemyBaseSquared < EnemyWindRangeSquared;
                        var closestOpponent = opponents.OrderBy(e => e.DistanceToEnemyBaseSquared).FirstOrDefault();

                        Console.Error.WriteLine("2");

                        if (enoughMana && !isShielded && isWithinShieldRadius && closeEnoughForShield && !lowHealth)
                        {
                            Console.Error.WriteLine("3");
                            return new ShieldSpell(closeMonster.Id);
                        }
                        else if (mana >= 20 && isWithinWindRadius && !isShielded && closeEnoughForWind && (!lowHealth || (closestOpponent != null && closeMonster.DistanceToEnemyBaseSquared < closestOpponent.DistanceToEnemyBaseSquared)))
                        {
                            Console.Error.WriteLine("4");
                            ResetBaseTicks(closeMonster);

                            return new WindSpell(EnemyBase.Position);
                        }
                        else if (enoughMana && BaseMonsterTicks.Any())
                        {
                            Console.Error.WriteLine("5");
                            Vector2 destination;
                            if (EnemyBase.Position == Vector2.Zero)
                            {
                                destination = new Vector2(2800, 2800);
                            }
                            else
                            {
                                destination = new Vector2(MapBounds.X - 2800, MapBounds.Y - 2800);
                            }

                            if (Vector2.DistanceSquared(Hero.Position, destination) < MoveDistanceSquared)
                            {
                                Console.Error.WriteLine($"{Hero.Position} is within 800 units to {destination}, clearing monsters list");
                                BaseMonsterTicks.Clear();
                            }

                            return new MoveCommand(destination);
                        }
                        else if (!closeEnoughForShield)
                        {
                            Console.Error.WriteLine("6");
                            return new AttackCommand(closeMonster);
                        }
                    }
                }

                // Patrol at the edge 6000 units from ENEMY base
                // and attack low enemies in range
                // WIND opponents away from their base
                // CONTROL monsters that are further than 7000 units and not threatening enemy base
                // WIND monsters that are within 6000 units
                // SHIELD monsters that are within 3000 units

                // if low on mana just farm monsters far from enemy base

                return new MoveCommand(SelectPatrolPoint());
            }

            private void ResetBaseTicks(Entity monster)
            {
                if (BaseMonsterTicks.ContainsKey(monster.Id))
                {
                    BaseMonsterTicks[monster.Id] = 3;
                }
                else
                {
                    BaseMonsterTicks.Add(monster.Id, 3);
                }
            }
        }

        public class Midfielder : BasePlayable
        {
            private static readonly int MonsterSpeedSquared = 400 * 400;
            private readonly int AggressionRangeSquared;

            private Vector2 LastAggressorPosition;
            private int AggressorDelay;

            private int stage = 0;

            public Midfielder(Entity hero, int aggressionRange, IEnumerable<Vector2> patrolPoints, IEnumerable<Vector2> attackPoints)
                : base(hero, patrolPoints, attackPoints)
            {
                this.AggressionRangeSquared = aggressionRange * aggressionRange;
            }

            public override Command Update(int mana, IEnumerable<Entity> monsters, IEnumerable<Entity> opponents)
            {
                if (monsters.Any())
                {
                    var max = monsters.Max(m => m.Health);
                    stage = Math.Max(stage, max);
                }

                var monstersInBase = monsters
                    .Where(m => PlayerBase.IsWithinRadius(m.Position) && m.TargetedBase == PlayerBase && m.IsShielded)
                    .OrderBy(m => m.DistanceToPlayerBaseSquared);

                var monsterInBase = monstersInBase.FirstOrDefault();
                if (monsterInBase != null && monsterInBase.Health / 2 > Math.Sqrt(monsterInBase.DistanceToPlayerBaseSquared / MonsterSpeedSquared))
                {
                    return new AttackCommand(monsterInBase);
                }

                // if there is an opponent near base, MURDER THEM.
                var aggressor = opponents.FirstOrDefault(e => (e.DistanceToPlayerBaseSquared < Hero.DistanceToPlayerBaseSquared || e.DistanceToPlayerBaseSquared < 7000 * 7000)
                    || (e.DistanceToEntitySquared(Hero) < ViewRangeSquared && stage > 20 && e.DistanceToEnemyBaseSquared > Base.RadiusSquared));
                if (aggressor != null && stage > 10)
                {
                    LastAggressorPosition = aggressor.Position;
                    AggressorDelay = 3;
                    //if (!Hero.IsShielded && stage > 15 && mana > 50)
                    //{
                    //    return new ShieldSpell(Hero.Id);
                    //}

                    var isTargetShielded = aggressor.IsShielded;

                    if (mana > 10 && !isTargetShielded)
                    {
                        if (IsHeroWithinWindRadius(aggressor))
                        {
                            return new WindSpell(EnemyBase.Position);
                        }
                        else if (IsHeroWithinControlRadius(aggressor))
                        {
                            return new ControlSpell(aggressor.Id, EnemyBase.Position);
                        }
                        else
                        {
                            return new AttackCommand(aggressor);
                        }
                    }
                }

                if (AggressorDelay > 0)
                {
                    AggressorDelay--;
                    return new MoveCommand(LastAggressorPosition);
                }

                // attack anything that gets close to 8200 units of base
                var targets = monsters
                    .Where(monster => monster.DistanceToPlayerBaseSquared < AggressionRangeSquared)
                    .OrderBy(monster => monster.DistanceToEntitySquared(Hero));

                var targetClosestToBase = targets.FirstOrDefault();

                if (targetClosestToBase != null)
                {
                    if (targetClosestToBase.ThreatenedBase == PlayerBase && mana > 30 && IsWithinControlRadius(targetClosestToBase) && targetClosestToBase.Health > 18)
                    {
                        return new ControlSpell(targetClosestToBase.Id, GetOptimalAttackPoint());
                    }
                    else
                    {
                        // and attack anything that gets close to 12200 units of base
                        return new AttackCommand(targetClosestToBase);
                    }
                }
                else
                {
                    // Patrol at the edge 10000 units from base
                    return new MoveCommand(SelectPatrolPoint());
                }


                // WIND opponents away along with the defender

                // if there is a lot of shielded hp in the base, help with the disposal
            }
        }

        public class Defender : BasePlayable
        {
            private static readonly int AggressionRange = 9200 * 9200;
            private static readonly int WindRadius = 4000 * 4000;

            public Defender(Entity hero, IEnumerable<Vector2> patrolPoints, IEnumerable<Vector2> attackPoints)
                : base(hero, patrolPoints, attackPoints) { }

            public override Command Update(int mana, IEnumerable<Entity> monsters, IEnumerable<Entity> opponents)
            {
                // if there is an opponent near base, MURDER THEM.
                var aggressor = opponents.FirstOrDefault(e => PlayerBase.IsWithinRadius(e.Position) || IsHeroWithinWindRadius(e));
                if (aggressor != null)
                {
                    var subtractedBase = (EnemyBase.Position - aggressor.Position) * 10;

                    if (mana > 10 && !aggressor.IsShielded && IsHeroWithinWindRadius(aggressor))
                    {
                        var rounded = new Vector2((int)Math.Round(subtractedBase.X), (int)Math.Round(subtractedBase.Y));

                        return new WindSpell(rounded);
                    }
                }

                // attack anything that gets close to 8200 units of base
                var targets = monsters
                    .Where(monster => monster.DistanceToPlayerBaseSquared < AggressionRange)
                    .OrderBy(monster => monster.DistanceToPlayerBaseSquared);

                var targetClosestToBase = targets.FirstOrDefault();

                if (targetClosestToBase == null)
                {
                    // Patrol at the edge 6000 units from base
                    return new MoveCommand(SelectPatrolPoint());
                }

                var isTargetShielded = targetClosestToBase.IsShielded;

                // rest of the defense logic stays the same i guess
                if (PlayerBase.IsWithinRadius(targetClosestToBase.Position))
                {
                    var subtractedBase = (targetClosestToBase.Position - PlayerBase.Position) * 10;
                    var distanceToBase = targetClosestToBase.DistanceToPlayerBaseSquared;

                    if (mana > 10 && !isTargetShielded && IsWithinWindRadius(targetClosestToBase) && !targetClosestToBase.IsControlled && distanceToBase < WindRadius)
                    {
                        var rounded = new Vector2((int)Math.Round(subtractedBase.X), (int)Math.Round(subtractedBase.Y));

                        return new WindSpell(rounded);
                    }
                    else
                    {
                        return new AttackCommand(targetClosestToBase);
                    }
                }
                else if (mana > 10
                    && IsWithinControlRadius(targetClosestToBase)
                    && targetClosestToBase.ThreatenedBase == PlayerBase
                    && !isTargetShielded
                    && targetClosestToBase.Health > 6
                    && (targetClosestToBase.DistanceToPlayerBaseSquared < Base.DangerRadiusSquared || targets.Count() > 6))
                {
                    return new ControlSpell(targetClosestToBase.Id, GetOptimalAttackPoint());
                }
                else
                {
                    return new AttackCommand(targetClosestToBase);
                }
            }
        }

        public abstract class BasePlayable
        {
            protected static readonly int ViewRangeSquared = 2200 * 2200;
            protected static readonly int MoveDistanceSquared = 800 * 800;

            private static readonly int WindRadiusSquared = 1280 * 1280;
            private static readonly int HeroWindRadiusSquared = 480 * 480;
            private static readonly int HeroControlRadiusSquared = 1600 * 1600;
            private static readonly int ControlRadiusSquared = 2200 * 2200;
            private static readonly int ShieldRadiusSquared = 2200 * 2200;

            private readonly Random random = new Random();

            private bool selectNext = false;

            public Entity Hero { get; set; }

            protected LinkedList<Vector2> PatrolPoints { get; }
            protected LinkedList<Vector2> AttackPoints { get; }

            public BasePlayable(Entity hero, IEnumerable<Vector2> patrolPoints, IEnumerable<Vector2> attackPoints)
            {
                this.Hero = hero;
                this.PatrolPoints = new LinkedList<Vector2>(patrolPoints);
                this.AttackPoints = new LinkedList<Vector2>(attackPoints);
            }

            public abstract Command Update(int mana, IEnumerable<Entity> monsters, IEnumerable<Entity> opponents);

            protected bool IsWithinControlRadius(Entity entity)
            {
                return Hero.DistanceToEntitySquared(entity) < ControlRadiusSquared;
            }

            protected bool IsWithinWindRadius(Entity entity)
            {
                return Hero.DistanceToEntitySquared(entity) < WindRadiusSquared;
            }

            protected bool IsHeroWithinWindRadius(Entity entity)
            {
                return Hero.DistanceToEntitySquared(entity) < HeroWindRadiusSquared;
            }

            protected bool IsHeroWithinControlRadius(Entity entity)
            {
                return Hero.DistanceToEntitySquared(entity) < HeroControlRadiusSquared;
            }

            protected bool IsWithinShieldRadius(Entity entity)
            {
                return Hero.DistanceToEntitySquared(entity) < ShieldRadiusSquared;
            }

            protected Vector2 GetOptimalAttackPoint()
            {
                return EnemyBase.Position;
            }

            protected Vector2 SelectPatrolPoint()
            {
                var closestPoint = PatrolPoints
                    .OrderBy(p => Vector2.DistanceSquared(Hero.Position, p))
                    .First();
                var distance = Vector2.DistanceSquared(Hero.Position, closestPoint);

                if (distance < MoveDistanceSquared)
                {
                    closestPoint = SelectNextNode(closestPoint);
                }

                return closestPoint;
            }

            private Vector2 SelectNextNode(Vector2 patrolPoint)
            {
                var node = PatrolPoints.Find(patrolPoint);

                LinkedListNode<Vector2> nextNode;
                if (selectNext)
                {
                    nextNode = node.Next;
                    if (nextNode == null)
                    {
                        selectNext = false;
                        nextNode = node.Previous;
                    }
                }
                else
                {
                    nextNode = node.Previous;
                    if (nextNode == null)
                    {
                        selectNext = true;
                        nextNode = node.Next;
                    }
                }

                return nextNode.Value;
            }
        }

        public enum SpellType
        {
            Shield,
            Wind,
            Control
        }

        public enum EntityType
        {
            Monster = 0,
            Player = 1,
            Opponent = 2
        }
    }
}