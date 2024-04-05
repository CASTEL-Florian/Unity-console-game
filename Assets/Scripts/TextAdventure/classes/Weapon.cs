using System.IO;
using Cysharp.Threading.Tasks;
using UnityConsole;
using UnityEngine;
using Random = System.Random;

namespace Text_Based_Game.Classes
{
    enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    internal class Weapon
    {
        public string Name { get; set; }
        public Rarity Rarity { get; set; }
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
        public int MaxAttacksPerTurn { get; set; }
        public int MinAttacksPerTurn { get; set; }
        public int VitalityBonus { get; set; }
        public int StrengthBonus { get; set; }
        private Random Random { get; set; }

        // CONSTRUCTORS

        public Weapon(Rarity rarity)
        {
            Random = new();
            Rarity = rarity;
            MinAttacksPerTurn = GenerateMinAttacks();
            MaxAttacksPerTurn = GenerateMaxAttacks();
            VitalityBonus = GenerateStatBonus();
            StrengthBonus = GenerateStatBonus();
            MinDamage = GenerateMinDamage();
            MaxDamage = GenerateMaxDamage();
        }
        public Weapon(Rarity rarity, string name) : this(rarity) { Name = name; }

        /// <summary>
        /// Returns a random stat bonus based on rarity
        /// </summary>
        private int GenerateStatBonus()
        {
            switch (Rarity)
            {
                case Rarity.Common:
                    return 0;
                case Rarity.Uncommon:
                    if (Random.NextDouble() < 0.1f)
                    {
                        return Random.Next(3, 4 + 1);
                    }
                    else
                    {
                        return Random.Next(0, 2 + 1);
                    }
                case Rarity.Rare:
                    if (Random.NextDouble() < 0.1f)
                    {
                        return Random.Next(3, 6 + 1);
                    }
                    else
                    {
                        return Random.Next(1, 5 + 1);
                    }
                case Rarity.Epic:
                    if (Random.NextDouble() < 0.1f)
                    {
                        return Random.Next(5, 14 + 1);
                    }
                    else
                    {
                        return Random.Next(3, 12 + 1);
                    }
                case Rarity.Legendary:
                    if (Random.NextDouble() < 0.1f)
                    {
                        return 20;
                    }
                    else
                    {
                        return Random.Next(5, 15 + 1);
                    }
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Returns amount of max attacks, either same as min attacks or a low chance to get an extra attack
        /// </summary>
        private int GenerateMaxAttacks()
        {
            if (Random.NextDouble() < 0.2f)
            {
                return MinAttacksPerTurn + 1;
            }
            else
            {
                return MinAttacksPerTurn;
            }
        }
        /// <summary>
        /// Returns amount of minimum attacks based on rarity
        /// </summary>
        private int GenerateMinAttacks()
        {
            switch (Rarity)
            {
                case Rarity.Common:
                case Rarity.Uncommon:
                    if (Random.NextDouble() < 0.1f)
                    {
                        return Random.Next(1, 2 + 1);
                    }
                    else
                    {
                        return 1;
                    }
                case Rarity.Rare:
                    if (Random.NextDouble() < 0.1f)
                    {
                        return Random.Next(1, 3 + 1);
                    }
                    else
                    {
                        return Random.Next(1, 2 + 1);
                    }
                case Rarity.Epic:
                    if (Random.NextDouble() < 0.1f)
                    {
                        return Random.Next(2, 3 + 1);
                    }
                    else
                    {
                        return Random.Next(1, 3 + 1);
                    }
                case Rarity.Legendary:
                    if (Random.NextDouble() < 0.1f)
                    {
                        return 4;
                    }
                    else
                    {
                        return Random.Next(1, 3 + 1);
                    }
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Returns max damage based on rariy
        /// </summary>
        private int GenerateMaxDamage()
        {
            switch (Rarity)
            {
                case Rarity.Common:
                    if (Random.NextDouble() < 0.25f)
                    {
                        return 5;
                    }
                    else
                    {
                        return Random.Next(2, 4 + 1);
                    }
                case Rarity.Uncommon:
                    if (Random.NextDouble() < 0.25f)
                    {
                        return 10;
                    }
                    else
                    {
                        return Random.Next(4, 8 + 1);
                    }
                case Rarity.Rare:
                    if (Random.NextDouble() < 0.25f)
                    {
                        return 25;
                    }
                    else
                    {
                        return Random.Next(10, 17 + 1);
                    }
                case Rarity.Epic:
                    if (Random.NextDouble() < 0.25f)
                    {
                        return 50;
                    }
                    else
                    {
                        return Random.Next(20, 35 + 1);
                    }
                case Rarity.Legendary:
                    if (Random.NextDouble() < 0.25f)
                    {
                        return 75;
                    }
                    else
                    {
                        return Random.Next(40, 70 + 1);
                    }
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Returns minimum damage based on rarity
        /// </summary>
        private int GenerateMinDamage()
        {
            switch (Rarity)
            {
                case Rarity.Common:
                    if (Random.NextDouble() < 0.25f)
                    {
                        return 2;
                    }
                    else
                    {
                        return 1;
                    }
                case Rarity.Uncommon:
                    if (Random.NextDouble() < 0.25f)
                    {
                        return 4;
                    }
                    else
                    {
                        return Random.Next(1, 2 + 1);
                    }
                case Rarity.Rare:
                    if (Random.NextDouble() < 0.25f)
                    {
                        return 6;
                    }
                    else
                    {
                        return Random.Next(2, 4 + 1);
                    }
                case Rarity.Epic:
                    if (Random.NextDouble() < 0.25f)
                    {
                        return Random.Next(10, 15 + 1);

                    }
                    else
                    {
                        return Random.Next(4, 8 + 1);
                    }
                case Rarity.Legendary:
                    if (Random.NextDouble() < 0.25f)
                    {
                        return Random.Next(25, 30 + 1);
                    }
                    else
                    {
                        return Random.Next(15, 20 + 1);
                    }
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Reads all lines of a file containing weapon names and returns a random name
        /// </summary>
        public async UniTask<string> GenerateWeaponName()
        {
            string[] allNames;

            switch (Rarity)
            {
                case Rarity.Common:
                    allNames = await FileLoader.ReadAllLinesAsync
(Path.Combine(Application.streamingAssetsPath, Globals.CommonNamePath));
                    return allNames[Random.Next(allNames.Length)];

                case Rarity.Uncommon:
                    allNames = await FileLoader.ReadAllLinesAsync
(Path.Combine(Application.streamingAssetsPath,Globals.UncommonNamePath));
                    return allNames[Random.Next(allNames.Length)];

                case Rarity.Rare:
                    allNames = await FileLoader.ReadAllLinesAsync
(Path.Combine(Application.streamingAssetsPath,Globals.RareNamePath));
                    return allNames[Random.Next(allNames.Length)];

                case Rarity.Epic:
                    allNames = await FileLoader.ReadAllLinesAsync
(Path.Combine(Application.streamingAssetsPath,Globals.EpicNamePath));
                    return allNames[Random.Next(allNames.Length)];
                default:
                    return "Forlorn Baguette";
            }
        }
    }
}
