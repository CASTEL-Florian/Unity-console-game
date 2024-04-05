using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = System.Random;
using Console = UnityConsole.Console;

namespace Text_Based_Game.Classes
{
    enum PathDifficulty
    {
        Easy,
        Medium,
        Hard,
        Final
    }

    enum PathStepType
    {
        Walking,
        PlayerTalk,
        BossFight,
        MobFight
    }

    internal class GamePath
    {
        public GameManager GameManagerRef { get; private set; }
        public string PathStartMessage { get; set; }
        public string PathCompletionMessage { get; set; }
        public PathDifficulty Difficulty { get; set; }
        public int PathLength { get; set; }
        public List<PathStepType> PathSteps { get; set; }
        public bool IsCompleted { get; set; }
        public Player PlayerRef { get; set; }
        public float XpOnCompletion { get; set; }
        public float XpFromMobsOnPath { get; set; }

        private readonly Color darkRed = new Color(0.772f, 0.059f, 0.122f);
        
        private readonly Color darkGray = new Color(0.663f, 0.663f, 0.663f);

        // CONSTRUCTORS
        public GamePath(string pathStartMessage, string pathCompletionMessage, int pathLength, Player player, PathDifficulty difficulty, GameManager gameManager)
        {
            GameManagerRef = gameManager;
            PathStartMessage = pathStartMessage;
            PathCompletionMessage = pathCompletionMessage;

            Difficulty = difficulty;
            PathLength = 5;
            //PathLength = pathLength;
            PathSteps = MakePath(Difficulty);
            IsCompleted = false;
            PlayerRef = player;
            switch (Difficulty)
            {
                case PathDifficulty.Easy:
                    XpOnCompletion = 100f;
                    break;
                case PathDifficulty.Medium:
                    XpOnCompletion = 200f;
                    break;
                case PathDifficulty.Hard:
                    XpOnCompletion = 300f;
                    break;
                case PathDifficulty.Final:
                    XpOnCompletion = 1000f;
                    break;
            }
            XpFromMobsOnPath = 0;
        }

        // METHODS

        private List<PathStepType> MakePath(PathDifficulty difficulty)
        {
            List<PathStepType> steps = new();

            switch (difficulty)
            {
                case PathDifficulty.Easy:
                    for (int i = 0; i <= PathLength / 2; i++)
                    {
                        steps.Add(PathStepType.MobFight);
                    }
                    for (int i = 0; i < PathLength / 2; i++)
                    {
                        steps.Add(PathStepType.Walking);
                    }
                    for (int i = 0; i < PathLength / 2; i++)
                    {
                        steps.Add(PathStepType.PlayerTalk);
                    }
                    steps.Add(PathStepType.BossFight);
                    break;
                case PathDifficulty.Medium:
                    for (int i = 0; i <= PathLength / 2; i++)
                    {
                        steps.Add(PathStepType.MobFight);
                    }
                    for (int i = 0; i < PathLength / 2; i++)
                    {
                        steps.Add(PathStepType.Walking);
                    }
                    for (int i = 0; i < PathLength / 2; i++)
                    {
                        steps.Add(PathStepType.PlayerTalk);
                    }
                    steps.Add(PathStepType.BossFight);
                    break;
                case PathDifficulty.Hard:
                    for (int i = 0; i <= PathLength / 2; i++)
                    {
                        steps.Add(PathStepType.MobFight);
                    }
                    for (int i = 0; i < PathLength / 2; i++)
                    {
                        steps.Add(PathStepType.Walking);
                    }
                    for (int i = 0; i < PathLength / 2; i++)
                    {
                        steps.Add(PathStepType.PlayerTalk);
                    }
                    steps.Add(PathStepType.BossFight);
                    break;
                case PathDifficulty.Final:
                    for (int i = 0; i <= PathLength / 2; i++)
                    {
                        steps.Add(PathStepType.BossFight);
                    }
                    for (int i = 0; i < PathLength / 2; i++)
                    {
                        steps.Add(PathStepType.Walking);
                    }
                    for (int i = 0; i < PathLength / 2; i++)
                    {
                        steps.Add(PathStepType.PlayerTalk);
                    }
                    steps.Add(PathStepType.BossFight);
                    break;
            }

            return ShufflePath(steps);
        }


        /// <summary>
        /// 
        /// </summary>
        public async UniTask TraversePath()
        {
            PlayerRef.SetCurrentHpToMax();
            PlayerRef.CurrentLocation = Location.Path;
            await TextHelper.PrintStringCharByChar(PathStartMessage, Color.white);
            TextHelper.LineSpacing(0);
            Random random = new();
            await Console.Sleep(random.Next(500, 1500));
            for (int i = 0; i < PathSteps.Count; i++)
            {
                switch (PathSteps[i])
                {
                    case PathStepType.Walking:
                        TextHelper.LineSpacing(0);
                        TextHelper.PrintTextInColor("*walking*", darkGray);
                        break;
                    case PathStepType.PlayerTalk:
                        TextHelper.LineSpacing(0);
                        await PlayerRef.SpeakAboutEnvironment();
                        break;
                    case PathStepType.MobFight:
                        Enemy currentEnemy = new(Difficulty);
                        await GameManagerRef.SimulateRegularCombat(currentEnemy);
                        await ShowOptionsAfterInteractiveEvent();
                        break;
                    case PathStepType.BossFight:
                        Boss currentBoss = new(Difficulty);
                        currentBoss.Name = await currentBoss.RandomBossName();
                        await GameManagerRef.SimulateBossCombat(currentBoss);
                        if (i != PathSteps.Count - 1)
                        {
                            await ShowOptionsAfterInteractiveEvent();
                        }
                        break;
                }
                await Console.Sleep(random.Next(500, 1500));
            }
            await PathCompleted();
        }

        /// <summary>
        /// Shuffles everything but the last element
        /// </summary>
        static List<PathStepType> ShufflePath(List<PathStepType> list)
        {
            Random random = new();
            List<PathStepType> itemsCopy = new(list);

            int copyCount = itemsCopy.Count - 1;

            for (int i = copyCount - 1; i > 0; i--)
            {
                int j = random.Next(0, copyCount--);
                (itemsCopy[j], itemsCopy[i]) = (itemsCopy[i], itemsCopy[j]);
            }

            return itemsCopy;
        }

        /// <summary>
        /// 
        /// </summary>
        public async UniTask ShowOptionsAfterInteractiveEvent()
        {
            //GameManager.HandleInputBuffering();
            Console.Write("Do you want to go back to (t)own, change your (e)quipment or (c)ontinue your adventure?: ");
            KeyCode key = await Console.ReadKey();
            bool validInput = key == KeyCode.T || key == KeyCode.C || key == KeyCode.E;
            while (!validInput)
            {
                Console.Write("\nNo choice was made, please try again: ");
                key = await Console.ReadKey();
                if (key == KeyCode.T || key == KeyCode.C || key == KeyCode.E) validInput = true;
            }
            TextHelper.LineSpacing(0);
            if (key == KeyCode.T)
            {
                await TeleportToTown();
            }
            else if (key == KeyCode.E)
            {
                await PlayerRef.ChangeEquipmentScreen();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private async UniTask PathCompleted()
        {
            IsCompleted = true;
            float totalXpGained = XpOnCompletion + XpFromMobsOnPath;
            TextHelper.PrintTextInColor($"{PathCompletionMessage}, ", Color.white, false);
            TextHelper.PrintTextInColor($"{totalXpGained} XP gained.", Color.blue, false);
            TextHelper.LineSpacing(0);
            PlayerRef.IncreaseXP(totalXpGained);
            TextHelper.LineSpacing(0);
            if (Difficulty == PathDifficulty.Final)
            {
                Globals.EndTime = DateTime.Now;
                TextHelper.LineSpacing(0);
                await TextHelper.PrintTextFile(Globals.OutroPath, true);
                TextHelper.LineSpacing();
                await TextHelper.PrintTextFile(Globals.CreditsPath, true);
                TextHelper.LineSpacing();
                Console.WriteLine("This run took you: " + Globals.EndTime.Subtract(Globals.StartTime));
                Console.Write($"\nDo you want to (s)tart Journey {Globals.NewGameModifier + 1} or (q)uit : ");
                KeyCode key = await Console.ReadKey();
                bool validInput = key == KeyCode.S || key == KeyCode.Q;
                while (!validInput)
                {
                    Console.Write("\nNo choice was made, please try again: ");
                    key = await Console.ReadKey();
                    if (key == KeyCode.S || key == KeyCode.P) validInput = true;
                }

                if (key == KeyCode.S)
                {
                    await GameManagerRef.StartNewJourney();
                    return;
                }

                Environment.Exit(0);
            }
            await TeleportToTown();
        }

        /// <summary>
        /// 
        /// </summary>
        public async UniTask TeleportToTown(string enemyName = "")
        {
            PlayerRef.Respawns = 3;
            if (!PlayerRef.IsDead)
            {

                Console.Write("Teleporting back to town... ");
                if (!IsCompleted)
                {
                    TextHelper.PrintTextInColor($"{XpFromMobsOnPath} XP gained.\n\n", Color.blue, false);
                    PlayerRef.IncreaseXP(XpFromMobsOnPath);
                }
                else
                {
                    Console.WriteLine("\n");
                }
            }
            else
            {
                await TextHelper.PrintDeathAnimation(Color.red);
                await Console.Sleep(2000);
                TextHelper.LineSpacing(0);
                Console.WriteLine($"You've died to {enemyName}, teleporting back to town...");
                float xpLost = XpFromMobsOnPath * 0.5f;
                PlayerRef.IncreaseXP(XpFromMobsOnPath - xpLost);
                TextHelper.PrintTextInColor($"You've lost {xpLost} XP in the temporal twist...\n", darkRed);
                await Console.Sleep(2000);
                Console.Clear();
                await TextHelper.PrintTextFile(Globals.TitlePath, false);
                Console.WriteLine();
            }
            await GameManagerRef.ShowTownOptions();
            Console.WriteLine();
        }
    }
}
