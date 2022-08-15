using System.Numerics;

public class DecisionService
{
    public Decision CreateDecision(GameState gameState)
    {
        var friendlyQueen = gameState.FriendlyQueen;
        var enemyQueen = gameState.EnemyQueen;

        var buildableSitesByDistance = gameState.AllSites.Values.OrderByDistance(friendlyQueen)
            .Where(site => !(site.Type == StructureType.Tower && site.Owner == Owner.Enemy)
                && IsNotWithinEnemyTowersRange(site, gameState.EnemySites));

        Console.Error.WriteLine("Included towers: " + buildableSitesByDistance.OfType<Tower>().Count());

        Console.Error.WriteLine($"Closest site: {buildableSitesByDistance.FirstOrDefault().Id}");

        // Write an action using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");

        // First line: A valid queen action
        // Second line: A set of training instructions

        var allFriendlyBarracks = gameState.AllSites.Values.OfType<Barracks>()
            .Where(b => b.Owner == Owner.Friendly);
        var nonFriendlyBuildableSites = buildableSitesByDistance.Where(site => site.Owner != Owner.Friendly
            || (site is Mine mine && mine.IncomeRate < mine.MaxMineSize && mine.Gold > 0)
            || (site is Tower tower));

        var closestBuildableSite = nonFriendlyBuildableSites.FirstOrDefault();
        while (closestBuildableSite != null && closestBuildableSite is Tower tower && tower.AttackRadius > 500)
        {
            nonFriendlyBuildableSites = nonFriendlyBuildableSites.Skip(1);
            closestBuildableSite = nonFriendlyBuildableSites.FirstOrDefault();
        }

        var buildingToBuild = ChooseBuildingTarget(closestBuildableSite, allFriendlyBarracks);

        var mainDecision = CreateMainDecision(gameState, friendlyQueen, closestBuildableSite, buildingToBuild);
        var trainDecision = CreateTrainDecision(gameState, allFriendlyBarracks, enemyQueen);

        return new Decision(mainDecision, trainDecision);
    }

    private bool IsNotWithinEnemyTowersRange(Site site, IEnumerable<Site> enemySites)
    {
        var enemyTowers = enemySites.OfType<Tower>();
        foreach (var tower in enemyTowers)
        {
            if (Vector2.Distance(tower.Position, site.Position) < tower.AttackRadius)
            {
                return false;
            }
        }

        return true;
    }

    private static string ChooseBuildingTarget(Site? closestBuildableSite, IEnumerable<Barracks> allFriendlyBarracks)
    {
        var shouldBuildKnightBarracks = !allFriendlyBarracks.Any(b => b.CreepType == CreepType.Knight);
        var shouldBuildArcherBarracks = !allFriendlyBarracks.Any(b => b.CreepType == CreepType.Archer);
        var shouldBuildGiantBarracks = !allFriendlyBarracks.Any(b => b.CreepType == CreepType.Giant);

        string buildingToBuild;
        if (shouldBuildKnightBarracks)
        {
            buildingToBuild = "BARRACKS-KNIGHT";
        }
        else if (closestBuildableSite != null && closestBuildableSite.Gold > 0)
        {
            buildingToBuild = "MINE";
        }
        else if (shouldBuildGiantBarracks)
        {
            buildingToBuild = "BARRACKS-GIANT";
        }
        else if (shouldBuildArcherBarracks)
        {
            buildingToBuild = "BARRACKS-ARCHER";
        }
        else
        {
            buildingToBuild = "TOWER";
        }

        return buildingToBuild;
    }

    private static string CreateMainDecision(GameState gameState, Unit friendlyQueen, Site? closestBuildableSite, string buildingToBuild)
    {
        string mainDecision;
        if (closestBuildableSite is null)
        {
            // move to the closest tower
            mainDecision = $"MOVE 0 0";
        }
        else if (gameState.EnemySites.OfType<Barracks>().Any(b => b.TrainingCooldown > 0)
            || gameState.EnemyUnits.Knights.Any())
        {
            mainDecision = $"BUILD {closestBuildableSite.Id} TOWER";
        }
        else
        {
            Console.Error.WriteLine($"Building mine on site {closestBuildableSite.Id} (max level: {closestBuildableSite.MaxMineSize})");
            mainDecision = $"BUILD {closestBuildableSite.Id} {buildingToBuild}";
        }

        return mainDecision;
    }

    private static string CreateTrainDecision(GameState gameState, IEnumerable<Barracks> allFriendlyBarracks, Unit enemyQueen)
    {
        var trainableSites = allFriendlyBarracks
            .Where(b => b.TrainingCooldown == 0);
        var orderedTrainableSites = trainableSites.OrderByDistance(enemyQueen);

        var archerBarracks = orderedTrainableSites.Where(site => site.CreepType == CreepType.Archer);
        var knightBarracks = orderedTrainableSites.Where(site => site.CreepType == CreepType.Knight);
        var giantBarracks = orderedTrainableSites.Where(site => site.CreepType == CreepType.Giant);

        var potentialDecisions = new Queue<(int SiteId, int Cost)>();
        if (gameState.EnemyUnits.Giants.Any() && archerBarracks.Any())
        {
            potentialDecisions.Enqueue((archerBarracks.First().Id, Costs.Archer));
        }

        if (gameState.EnemySites.OfType<Tower>().Count() > 3 && giantBarracks.Any())
        {
            potentialDecisions.Enqueue((giantBarracks.First().Id, Costs.Giant));
        }

        if (knightBarracks.Any())
        {
            potentialDecisions.Enqueue((knightBarracks.First().Id, Costs.Knight));
        }

        var resultGold = gameState.Gold;
        var barrackIdsToTrain = new List<int>();
        while (potentialDecisions.TryDequeue(out var potentialDecision))
        {
            if (resultGold > potentialDecision.Cost)
            {
                barrackIdsToTrain.Add(potentialDecision.SiteId);
                resultGold -= potentialDecision.Cost;
            }
        }

        string result;
        if (barrackIdsToTrain.Any())
        {
            result = $"TRAIN {string.Join(" ", barrackIdsToTrain)}";
        }
        else
        {
            result = "TRAIN";
        }
        return result;
    }
}