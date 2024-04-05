using System;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityConsole;
using UnityEngine;
using Random = System.Random;
using Console = UnityConsole.Console;

namespace Text_Based_Game.Classes
{
    internal class GameManager
    {
        // PROPERTIES
        private Player Player { get; set; }
        public GamePath CurrentPath { get; set; }

        // VARIABLES
        public bool CanTakeFinalPath = false;
        private readonly int MaxPathLengthEasy = 15;
        private readonly int MinPathLengthEasy = 10;
        private readonly int MaxPathLengthMedium = 20;
        private readonly int MinPathLengthMedium = 10;
        private readonly int MaxPathLengthHard = 25;
        private readonly int MinPathLengthHard = 15;
        private readonly int MaxPathLengthFinal = 30;
        private readonly int MinPathLengthFinal = 20;
        private string[] ReturnToTownMessages;
        private string[] PathStartMessages;
        private string[] PathCompletionMessages;
        
        private readonly Color darkRed = new Color(0.772f, 0.059f, 0.122f);
        private readonly Color darkYellow = new Color(0.933f, 0.667f, 0);
        public Random Random { get; set; }

        // CONSTRUCTORS
        public GameManager()
        {
            Random = new();
            Player = new(this);
        }

        // METHODS

        /// <summary>
        /// Start the first path
        /// </summary>
        public async UniTask StartGame()
        {
            //Console.WriteLine("TESTING NewGameModifier: " + Globals.NewGameModifier);
            Globals.StartTime = DateTime.Now;
            CurrentPath = GeneratePath(PathDifficulty.Easy);
            await CurrentPath.TraversePath();
        }

        public async UniTask LoadFiles()
        {
            ReturnToTownMessages = await FileLoader.ReadAllLinesAsync
                (Path.Combine(Application.streamingAssetsPath, Globals.ReturnToTownMessagesPath));
            PathStartMessages = await FileLoader.ReadAllLinesAsync
                (Path.Combine(Application.streamingAssetsPath, Globals.PathStartMessagesPath));
            PathCompletionMessages = await FileLoader.ReadAllLinesAsync
                (Path.Combine(Application.streamingAssetsPath, Globals.PathCompletionMessagesPath));
            await Player.LoadEnvironmentObservations();
        }

        /// <summary>
        /// 
        /// </summary>
        public async UniTask StartNewJourney()
        {
            Globals.NewGameModifier++;
            Player.WeaponInventory.Clear();
            Console.Clear();
            await TextHelper.PrintTextFile(Globals.TitlePath, false);
            TextHelper.LineSpacing();
            Player.CurrentLocation = Location.Town;
            CurrentPath = GeneratePath(PathDifficulty.Easy);
            Player.SetCurrentHpToMax();
            await StartGame();
        }

        /// <summary>
        /// 
        /// </summary>
        public async UniTask SimulateRegularCombat(Enemy enemy)
        {
            TextHelper.PrintTextInColor($"\nYou've encountered {enemy.Name}, {enemy.CurrentHp} HP", Color.yellow);
            await Console.Sleep(500);
            do
            {
                if (Random.NextDouble() < enemy.DodgeChance)
                {
                    TextHelper.PrintTextInColor($"The {enemy.Name} gracefully evades your attack", darkYellow);
                }
                else
                {
                    int[] playerAttack = Player.CalculateAttack();
                    enemy.TakeDamage(playerAttack[1]);
                    if (playerAttack[0] == 0)
                    {
                        TextHelper.PrintTextInColor($"You attack {enemy.Name} but trip and whiff entirely", Color.yellow);
                    }
                    else if (playerAttack[0] == 1)
                    {
                        TextHelper.PrintTextInColor($"You attack {enemy.Name} and do {playerAttack[1]} dmg, {enemy.CurrentHp}/{enemy.MaxHp} HP", Color.yellow);
                    }
                    else
                    {
                        TextHelper.PrintTextInColor($"You attack {enemy.Name} {playerAttack[0]} times for a total of {playerAttack[1]} dmg, {enemy.CurrentHp}/{enemy.MaxHp} HP", Color.yellow);
                    }
                }
                await Console.Sleep(500);

                if (enemy.CurrentHp > 0)
                {
                    int enemyDamage = enemy.CalculateAttack();
                    Player.TakeDamage(enemyDamage);
                    TextHelper.PrintTextInColor($"{enemy.Name} attacks, you take {enemyDamage} dmg, {Player.CurrentHp}/{Player.MaxHp} HP", darkYellow);
                }
                await Console.Sleep(500);

            } while (enemy.CurrentHp > 0 && Player.CurrentHp > 0);

            if (enemy.CurrentHp <= 0)
            {
                CurrentPath.XpFromMobsOnPath += enemy.XpDropped;
                TextHelper.PrintTextInColor($"The {enemy.Name} collapses, ", Color.yellow, false);
                if (enemy.WeaponToDrop != null)
                {
                    TextHelper.PrintTextInColor($"you've gained {enemy.XpDropped} XP", Color.blue, false);
                    TextHelper.PrintTextInColor($" and {enemy.WeaponToDrop.Name} ({enemy.WeaponToDrop.Rarity})!\n\n", Color.blue, false);
                    Player.PickUpWeapon(enemy.WeaponToDrop);
                }
                else
                {
                    TextHelper.PrintTextInColor($"you've gained {enemy.XpDropped} XP!\n\n", Color.blue, false);
                }
            }
            else
            {
                Player.IsDead = true;
                await CurrentPath.TeleportToTown(enemy.Name);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public async UniTask SimulateBossCombat(Boss boss)
        {
            TextHelper.PrintTextInColor($"\nYou've encountered {boss.Name}, {boss.CurrentHp} HP", Color.red);
            await Console.Sleep(500);
            do
            {
                if (Random.NextDouble() < boss.DodgeChance)
                {
                    TextHelper.PrintTextInColor($"{boss.Name} gracefully evades your attack", darkRed);
                }
                else
                {
                    int[] playerAttack = Player.CalculateAttack();
                    boss.TakeDamage(playerAttack[1]);
                    if (playerAttack[0] == 0)
                    {
                        TextHelper.PrintTextInColor($"You attack {boss.Name} but trip and whiff entirely", Color.red);
                    }
                    else if (playerAttack[0] == 1)
                    {
                        TextHelper.PrintTextInColor($"You attack {boss.Name} and do {playerAttack[1]} dmg, {boss.CurrentHp}/{boss.MaxHp} HP", Color.red);
                    }
                    else
                    {
                        TextHelper.PrintTextInColor($"You attack {boss.Name} {playerAttack[0]} times for a total of {playerAttack[1]} dmg, {boss.CurrentHp}/{boss.MaxHp} HP", Color.red);
                    }
                }
                await Console.Sleep(500);

                if (boss.CurrentHp > 0)
                {
                    int enemyDamage = boss.CalculateAttack();
                    Player.TakeDamage(enemyDamage);
                    TextHelper.PrintTextInColor($"{boss.Name} attacks, you take {enemyDamage} dmg, {Player.CurrentHp}/{Player.MaxHp} HP", darkRed);
                }
                await Console.Sleep(500);

            } while (boss.CurrentHp > 0 && Player.CurrentHp > 0);

            if (boss.CurrentHp <= 0)
            {
                CurrentPath.XpFromMobsOnPath += boss.XpDropped;
                TextHelper.PrintTextInColor($"{boss.Name} collapses, ", Color.red, false);
                if (boss.WeaponToDrop != null)
                {
                    TextHelper.PrintTextInColor($"you've gained {boss.XpDropped} XP", Color.blue, false);
                    TextHelper.PrintTextInColor($" and {boss.WeaponToDrop.Name} ({boss.WeaponToDrop.Rarity})!\n", Color.blue, false);
                    Player.PickUpWeapon(boss.WeaponToDrop);
                }
                else
                {
                    TextHelper.PrintTextInColor($"you've gained {boss.XpDropped} XP!\n", Color.blue, false);
                }
            }
            else
            {
                if (Player.Respawns > 0)
                {
                    Console.Write($"Would you like to (r)espawn ({Player.Respawns} / 3) or (t)eleport to town?: ");
                    KeyCode key = await Console.ReadKey();
                    bool isValidInput = false;
                    if (key == KeyCode.R || key == KeyCode.T) isValidInput = true;
                    while (!isValidInput)
                    {
                        Console.Write("\ntry again: ");
                        key = await Console.ReadKey();
                        if (key == KeyCode.R || key == KeyCode.T) isValidInput = true;
                    }

                    if (key == KeyCode.R)
                    {
                        Player.Respawns--;
                        Player.Heal((int)Player.MaxHp / 2);
                        Player.IsDead = false;
                        await SimulateBossCombat(boss);
                    }
                    else
                    {
                        TextHelper.LineSpacing(0);
                        Player.IsDead = true;
                        await CurrentPath.TeleportToTown(boss.Name);
                    }
                }
                else
                {
                    Player.IsDead = true;
                    await CurrentPath.TeleportToTown(boss.Name);
                }
            }
            TextHelper.ChangeForegroundColor(Color.gray);
        }
        

        public async UniTask ChoosePath()
        {
            if (!CanTakeFinalPath)
            {
                Console.Write("\nDo you want to venture down the (e)asy, (m)edium or (h)ard path?: ");
                KeyCode key = await Console.ReadKey();
                bool validInput = false;
                if (key == KeyCode.E || key == KeyCode.M || key == KeyCode.H) validInput = true;
                while (!validInput)
                {
                    Console.Write("\nNo choice was made, please try again: ");
                    key = await Console.ReadKey();
                    if (key == KeyCode.E || key == KeyCode.M || key == KeyCode.H) validInput = true;
                }

                switch (key)
                {
                    case KeyCode.E:
                        CurrentPath = GeneratePath(PathDifficulty.Easy);
                        break;
                    case KeyCode.M:
                        CurrentPath = GeneratePath(PathDifficulty.Medium);
                        break;
                    case KeyCode.H:
                        CurrentPath = GeneratePath(PathDifficulty.Hard);
                        break;
                }
            }
            else
            {
                Console.Write("\nDo you want to venture down the (e)asy, (m)edium, (h)ard or (f)inal path?: ");
                KeyCode key = await Console.ReadKey();
                bool validInput = false;
                if (key == KeyCode.E || key == KeyCode.M || key == KeyCode.H || key == KeyCode.F) validInput = true;
                while (!validInput)
                {
                    Console.Write("\nNo choice was made, please try again: ");
                    key = await Console.ReadKey();
                    if (key == KeyCode.E || key == KeyCode.M || key == KeyCode.H || key == KeyCode.F) validInput = true;
                }

                switch (key)
                {
                    case KeyCode.E:
                        CurrentPath = GeneratePath(PathDifficulty.Easy);
                        break;
                    case KeyCode.M:
                        CurrentPath = GeneratePath(PathDifficulty.Medium);
                        break;
                    case KeyCode.H:
                        CurrentPath = GeneratePath(PathDifficulty.Hard);
                        break;
                    case KeyCode.F:
                        CurrentPath = GeneratePath(PathDifficulty.Final);
                        break;
                }
            }
            Console.WriteLine("\n");
            await CurrentPath.TraversePath();
        }

        /// <summary>
        /// 
        /// </summary>
        public async UniTask ShowTownOptions()
        {
            Player.IsDead = false;
            if (Player.CurrentLocation != Location.Town)
            {
                await TextHelper.PrintStringCharByChar(ReturnToTownMessages[Random.Next(ReturnToTownMessages.Length)], Color.white);
                Player.CurrentLocation = Location.Town;
                Player.SetCurrentHpToMax();
            }

            // HandleInputBuffering();

            Console.Write("\nDo you want to start another (p)ath or see your (s)tats?: ");
            KeyCode key = await Console.ReadKey();
            bool validInput = false;
            if (key == KeyCode.S || key == KeyCode.P) validInput = true;
            while (!validInput)
            {
                Console.Write("\nNo choice was made, please try again: ");
                key = await Console.ReadKey();
                if (key == KeyCode.S || key == KeyCode.P) validInput = true;
            }

            if (key == KeyCode.S)
            {
                await Player.ShowStats();
            }
            else if (key == KeyCode.P)
            {
                await ChoosePath();
            }
        }

        /// <summary>
        /// Returns a GamePath based of the difficulty
        /// </summary>
        public GamePath GeneratePath(PathDifficulty chosenDifficulty)
        {
            string randomPathStartMessage = PathStartMessages[Random.Next(PathStartMessages.Length)];
            string randomPathCompletionMessage = PathCompletionMessages[Random.Next(PathCompletionMessages.Length)];

            int minLength = MinPathLengthEasy;
            int maxLength = MaxPathLengthEasy;
            switch (chosenDifficulty)
            {
                case PathDifficulty.Medium:
                    minLength = MinPathLengthMedium;
                    maxLength = MaxPathLengthMedium;
                    break;
                case PathDifficulty.Hard:
                    minLength = MinPathLengthHard;
                    maxLength = MaxPathLengthHard;
                    break;
                case PathDifficulty.Final:
                    minLength = MinPathLengthFinal;
                    maxLength = MaxPathLengthFinal;
                    break;
            }
            int randomPathLength = Random.Next(minLength, maxLength + 1);

            GamePath newPath = new(
                randomPathStartMessage,
                randomPathCompletionMessage,
                randomPathLength,
                Player,
                chosenDifficulty,
                this
                );

            return newPath;
        }
    }
}
