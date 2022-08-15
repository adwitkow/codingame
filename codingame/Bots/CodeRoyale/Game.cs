using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
public class Game
{
    private const int MapWidth = 1920;
    private const int MapHeight = 1000;
    private static Vector2 MapMiddle = new Vector2(MapWidth / 2, MapHeight / 2);

    static void Main(string[] args)
    {
        var parser = new InputParser();
        var gameState = parser.ParseInitialization();
        var decisionService = new DecisionService();
        // game loop
        while (true)
        {
            gameState = parser.ParseUpdate(gameState);

            var decision = decisionService.CreateDecision(gameState);
            Console.WriteLine(decision.MainDecision);
            Console.WriteLine(decision.TrainDecision);
        }
    }
}