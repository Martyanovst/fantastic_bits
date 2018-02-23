using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net.Configuration;
using System.Runtime.CompilerServices;

/**
 * Grab Snaffles and try to throw them through the opponent's goal!
 * Move towards a Snaffle and use your team id to determine where you need to throw it.
 **/

class Player
{
    private static Dictionary<int, Vector> snaffles;

    private static Dictionary<int, Wizard> wizards;


    static Dictionary<int, Vector> enemies = new Dictionary<int, Vector>();
    private static int myTeamId;

    static void readData(string[] inputs)
    {
        inputs = Console.ReadLine().Split(' ');
        var myScore = int.Parse(inputs[0]);
        var myMagic = int.Parse(inputs[1]);
        inputs = Console.ReadLine().Split(' ');
        var opponentScore = int.Parse(inputs[0]);
        var opponentMagic = int.Parse(inputs[1]);
        var entities = int.Parse(Console.ReadLine());
        snaffles = new Dictionary<int, Vector>();
        for (var i = 0; i < entities; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            var entityId = int.Parse(inputs[0]);
            var entityType = inputs[1]; // "WIZARD", "OPPONENT_WIZARD" or "SNAFFLE" (or "BLUDGER" after first league)
            var x = int.Parse(inputs[2]); // position
            var y = int.Parse(inputs[3]); // position
            var vx = int.Parse(inputs[4]); // velocity
            var vy = int.Parse(inputs[5]); // velocity
            var state = int.Parse(inputs[6]);
            switch (entityType)
            {
                case "SNAFFLE":
                    if (!enemies.ContainsValue(new Vector(x, y))) snaffles[entityId] = new Vector(x, y);
                    break;
                case "WIZARD":
                    wizards[entityId].location = new Vector(x, y);
                    wizards[entityId].state = state;
                    break;
                case "OPPONENT_WIZARD":
                    enemies[entityId] = new Vector(x, y);
                    break;
            }
        }
    }

    static void Main(string[] args)
    {
        string[] inputs = new string[7];
        myTeamId = int.Parse(Console.ReadLine());
        Wizard.finish = myTeamId == 1 ? new Vector(0, 3750) : new Vector(15500, 3750);
        Wizard.start = myTeamId == 1 ? new Vector(15500, 3750) : new Vector(0, 3750);
        var first = myTeamId == 1 ? 2 : 0;
        var second = myTeamId == 1 ? 3 : 1;
        wizards = new Dictionary<int, Wizard>
        {
            [first] = new Wizard(Wizard.start, 2),
            [second] = new Wizard(Wizard.start, 3),
        };

        while (true)
        {
            readData(inputs);
            foreach (var wiz in wizards.Keys)
                wizards[wiz].Move(snaffles);
        }
    }

    public class Vector
    {
        public Vector(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X;
        public double Y;
        public double Length => Math.Sqrt(X * X + Y * Y);
        public double Angle => Math.Atan2(Y, X);
        public static Vector Zero = new Vector(0, 0);

        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.X - b.X, a.Y - b.Y);
        }
    }

    public class Wizard
    {
        public enum Action
        {
            Search,
            Attack
        }

        public Vector location;
        public int id;
        public Action action = Action.Search;
        public Vector target;
        public int target_id;
        public static Vector finish;
        public static Vector start;
        public int[] targets = new int[4];
        public int state = 0;

        public Wizard(Vector loc, int id)
        {
            this.id = id;
            location = loc;
        }

        public void Move(Dictionary<int, Vector> snaffles)
        {
            try
            {
                switch (action)
                {
                    case Action.Search:
                        target_id = snaffles.Keys.OrderBy(x => (snaffles[x] - location).Length).First(x => !targets.Contains(x));
                        target = snaffles[target_id];

                        targets[id] = target_id;
                        action = Action.Attack;
                        Console.WriteLine($"MOVE {target.X} {target.Y} 150");
                        return;
                    case Action.Attack:
                        target = snaffles[target_id];
                        if (state == 1)
                        {
                            Console.WriteLine($"THROW {finish.X} {finish.Y} 499");
                            action = Action.Search;
                        }
                        else Console.WriteLine($"MOVE {target.X} {target.Y} 150");
                        return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"MOVE {start.X} {start.Y} 150");
                action = Action.Search;
            }
        }
    }
}