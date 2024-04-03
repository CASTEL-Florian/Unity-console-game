using System.IO;
using UnityEngine;
using Random = System.Random;

namespace Text_Based_Game.Classes
{
    internal class Boss : Enemy
    {
        private readonly float BossMultiplier = 1.5f;

        // CONSTRUCTORS
        public Boss(PathDifficulty difficulty) : base(difficulty)
        {
            Name = RandomBossName();
            CurrentHp = (int)(CurrentHp * BossMultiplier);
            XpDropped *= BossMultiplier;
            MinDamage = (int)(MinDamage * BossMultiplier);
            MaxDamage = (int)(MaxDamage * BossMultiplier);
        }
        public Boss(PathDifficulty difficulty, string name) : this(difficulty) { Name = name; }

        // METHODS
        private string RandomBossName()
        {
            Random random = new();
            string[] allNames = File.ReadAllLines(Path.Combine(Application.streamingAssetsPath,Globals.BossNamePath));
            return allNames[random.Next(allNames.Length - 1)];
        }
    }
}