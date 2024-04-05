using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityConsole;
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
            CurrentHp = (int)(CurrentHp * BossMultiplier);
            MaxHp = (int)(MaxHp * BossMultiplier);
            XpDropped *= BossMultiplier;
            MinDamage = (int)(MinDamage * BossMultiplier);
            MaxDamage = (int)(MaxDamage * BossMultiplier);
        }
        public Boss(PathDifficulty difficulty, string name) : this(difficulty) { Name = name; }

        // METHODS
        public async UniTask<string> RandomBossName()
        {
            Random random = new();
            string[] allNames = await FileLoader.ReadAllLinesAsync(Path.Combine(Application.streamingAssetsPath,Globals.BossNamePath));
            return allNames[random.Next(allNames.Length - 1)];
        }
    }
}