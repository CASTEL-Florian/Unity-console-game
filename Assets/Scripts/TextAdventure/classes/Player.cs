using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityConsole;
using UnityEngine;
using Random = System.Random;
using Console = UnityConsole.Console;

namespace Text_Based_Game.Classes
{
    enum Location
    {
        Town,
        Path
    }

    enum Stat
    {
        Vitality,
        Strength
    }

    internal class Player
    {
        private const int StartingVitality = 5;
        private const int StartingStrength = 5;
        public const string Name = "Alaric";
        private GameManager GameManagerRef { get; set; }
        private string[] EnvironmentObservations;
        public Weapon EquippedWeapon = new(Rarity.Common, "Broken Sword Hilt");
        public List<Weapon> WeaponInventory = new List<Weapon>();
        public int CurrentLevel = 1;
        public float XpToLevelUp = 200;
        public float CurrentXP = 0;
        public int TotalSkillPointsGained = 0;
        public int AvailableSkillpoints = 0;
        public int SkillPointsPerLevel = 1;
        public int Vitality { get; set; }
        public int VitalitySkillPoints { get; set; }
        public int Strength { get; set; }
        public int StrengthSkillPoints { get; set; }
        public float MaxHp { get; set; }
        public float CurrentHp { get; set; }
        public Location CurrentLocation { get; set; }
        public int Respawns { get; set; }
        public bool IsDead { get; set; }
        private Random Random { get; set; }

        // CONSTRUCTORS
        public Player(GameManager gameManagerRef)
        {
            Random = new();
            Vitality = StartingVitality;
            Strength = StartingStrength;
            MaxHp = CalculateMaxHP();
            SetCurrentHpToMax();
            Respawns = 3;
            IsDead = false;
            CurrentLocation = Location.Town;
            GameManagerRef = gameManagerRef;
        }

        // METHODS

        /// <summary>
        /// Adds a weapon to player inventory
        /// </summary>
        public void PickUpWeapon(Weapon loot)
        {
            WeaponInventory.Add(loot);
        }
        
        public async UniTask LoadEnvironmentObservations()
        {
            EnvironmentObservations = await FileLoader.ReadAllLinesAsync(Path.Combine(Application.streamingAssetsPath, Globals.EnvironmentObservationsPath));
        }

        /// <summary>
        /// Returns amount of attacks (array[0]) and total damage of all attacks (array[1])
        /// </summary>
        public int[] CalculateAttack()
        {

            int numberOfAttacks = Random.Next(EquippedWeapon.MinAttacksPerTurn, EquippedWeapon.MaxAttacksPerTurn + 1);

            int totalDamage = 0;
            for (int i = 0; i < numberOfAttacks; i++)
            {
                totalDamage += Random.Next(EquippedWeapon.MinDamage + Strength, EquippedWeapon.MaxDamage + Strength + 1);
            }


            return new int[] {numberOfAttacks, totalDamage};
        }

        /// <summary>
        /// 
        /// </summary>
        public async UniTask ChangeEquipmentScreen()
        {
            if (WeaponInventory.Count == 0)
            {
                TextHelper.LineSpacing(0);
                Console.WriteLine("No weapons in bag");
                if (CurrentLocation != Location.Town)
                {
                    await GameManagerRef.CurrentPath.ShowOptionsAfterInteractiveEvent();
                }
                else
                {
                    await ShowStats();
                }
                return;
            }
            TextHelper.LineSpacing(0);
            Console.WriteLine("Weapons:");
            Console.WriteLine($"Currently Equipped: {EquippedWeapon.Name} ({EquippedWeapon.Rarity})");
            Console.WriteLine($"    Damage: {EquippedWeapon.MinDamage} - {EquippedWeapon.MaxDamage}");
            if (EquippedWeapon.MinAttacksPerTurn == EquippedWeapon.MaxAttacksPerTurn)
            {
                Console.WriteLine($"    Attacks per turn: {EquippedWeapon.MinAttacksPerTurn}");
            }
            else
            {
                Console.WriteLine($"    Attacks per turn: {EquippedWeapon.MinAttacksPerTurn} - {EquippedWeapon.MaxAttacksPerTurn}");
            }
            Console.WriteLine($"    Vitality Boost: {EquippedWeapon.VitalityBonus}");
            Console.WriteLine($"    Strength Boost: {EquippedWeapon.StrengthBonus}");
            for (var i = 0; i < WeaponInventory.Count; i++)
            {
                Weapon temp = WeaponInventory[i];
                Console.WriteLine($"{i + 1}. {temp.Name} ({temp.Rarity})");
                Console.WriteLine($"    Damage: {temp.MinDamage} - {temp.MaxDamage}");
                if (temp.MinAttacksPerTurn == temp.MaxAttacksPerTurn)
                {
                    Console.WriteLine($"    Attacks per turn: {temp.MinAttacksPerTurn}");
                }
                else
                {
                    Console.WriteLine($"    Attacks per turn: {temp.MinAttacksPerTurn} - {temp.MaxAttacksPerTurn}");
                }
                Console.WriteLine($"    Vitality Boost: {temp.VitalityBonus}");
                Console.WriteLine($"    Strength Boost: {temp.StrengthBonus}");
            }
            TextHelper.LineSpacing(0);
            Console.Write("Equip weapon by pressing the corresponding number or (r)eturn to previous selection (Please press enter as well): ");

            bool validInput = false;
            do
            {
                string input = await Console.ReadLine();
                _ = int.TryParse(input, out int valueAsInt);
                if (input.ToLower() == "r" || valueAsInt <= WeaponInventory.Count && valueAsInt != 0)
                {
                    validInput = true;
                    if (input == "r")
                    {
                        if (CurrentLocation != Location.Town)
                        {
                            await GameManagerRef.CurrentPath.ShowOptionsAfterInteractiveEvent();
                        }
                        else
                        {
                            await GameManagerRef.ShowTownOptions();
                        }
                        return;
                    }
                    else
                    {
                        EquipWeapon(WeaponInventory[valueAsInt - 1]);
                        if (CurrentLocation != Location.Town)
                        {
                            await GameManagerRef.CurrentPath.ShowOptionsAfterInteractiveEvent();
                        }
                        else
                        {
                            await GameManagerRef.ShowTownOptions();
                        }
                    }
                }
                else
                {
                    Console.Write("No choice was made, please try again: ");
                }
            } while (!validInput);
        }

        /// <summary>
        /// Equips a weapon from inventory and unequips currently equipped weapon
        /// </summary>
        public void EquipWeapon(Weapon weapon)
        {
            DecreaseStat(Stat.Vitality, EquippedWeapon.VitalityBonus);
            DecreaseStat(Stat.Strength, EquippedWeapon.StrengthBonus);
            WeaponInventory.Add(EquippedWeapon);
            EquippedWeapon = weapon;
            WeaponInventory.Remove(EquippedWeapon);
            IncreaseStat(Stat.Vitality, weapon.VitalityBonus);
            IncreaseStat(Stat.Strength, weapon.StrengthBonus);
        }

        /// <summary>
        /// Sets current hp to max
        /// </summary>
        public void SetCurrentHpToMax()
        {
            CurrentHp = MaxHp;
        }

        /// <summary>
        /// Returns mx hp after calculation
        /// </summary>
        private int CalculateMaxHP()
        {
            return 20 * Vitality;
        }

        /// <summary>
        /// 
        /// </summary>
        public async UniTask ShowStats()
        {
            Console.WriteLine("\nYour Stats: ");
            Console.WriteLine($"Level: {CurrentLevel}");
            Console.WriteLine($"Current XP: {(int)CurrentXP}");
            Console.WriteLine($"XP needed for next level: {(int)XpToLevelUp}");
            Console.WriteLine($"{AvailableSkillpoints} stat points available");
            Console.WriteLine($"Max HP: {MaxHp}");
            Console.WriteLine($"Vitality: {Vitality}");
            Console.WriteLine($"Strength: {Strength}");
            TextHelper.LineSpacing(0);
            Console.WriteLine($"Equipped Weapon: {EquippedWeapon.Name} ({EquippedWeapon.Rarity})");
            Console.WriteLine($"    Damage: {EquippedWeapon.MinDamage} - {EquippedWeapon.MaxDamage}");
            if (EquippedWeapon.MinAttacksPerTurn == EquippedWeapon.MaxAttacksPerTurn)
            {
                Console.WriteLine($"    Attacks per turn: {EquippedWeapon.MinAttacksPerTurn}");
            }
            else
            {
                Console.WriteLine($"    Attacks per turn: {EquippedWeapon.MinAttacksPerTurn} - {EquippedWeapon.MaxAttacksPerTurn}");
            }
            Console.WriteLine($"    Vitality Boost: {EquippedWeapon.VitalityBonus}");
            Console.WriteLine($"    Strength Boost: {EquippedWeapon.StrengthBonus}");
            if (AvailableSkillpoints > 0)
            {
                Console.Write("\nWould you like to increase (v)itality or (s)trength, (r)espec or (c)ontinue to your adventure?: ");
                KeyCode key = await Console.ReadKey();
                bool validInput = false;
                if (key == KeyCode.V || key == KeyCode.S || key == KeyCode.R || key == KeyCode.C) validInput = true;
                while (!validInput)
                {
                    Console.Write("\nNo choice was made, please try again: ");
                    key = await Console.ReadKey();
                    if (key == KeyCode.V || key == KeyCode.S || key == KeyCode.R || key == KeyCode.C) validInput = true;
                }
                Console.WriteLine("\n");
                switch (key)
                {
                    case KeyCode.C:
                        await GameManagerRef.ShowTownOptions();
                        break;
                    case KeyCode.V:
                    case KeyCode.S:
                        IncreaseStatWithSkillpoints(key);
                        await ShowStats();
                        break;
                    case KeyCode.R:
                        ResetSkillPoints();
                        await ShowStats();
                        break;
                }
            }
            else
            {
                Console.Write("\nWould you like to (r)espec, change your (e)quipment or (c)ontinue to your adventure?: ");
                KeyCode key = await Console.ReadKey();
                bool validInput = false;
                if (key == KeyCode.R || key == KeyCode.E || key == KeyCode.C) validInput = true;
                while (!validInput)
                {
                    Console.Write("\nNo choice was made, please try again: ");
                    key = await Console.ReadKey();
                    if (key == KeyCode.R || key == KeyCode.E || key == KeyCode.C) validInput = true;
                }
                //Console.WriteLine("\n");
                switch (key)
                {
                    case KeyCode.C:
                        await GameManagerRef.ShowTownOptions();
                        break;
                    case KeyCode.R:
                        ResetSkillPoints();
                        await ShowStats();
                        break;
                    case KeyCode.E:
                        await ChangeEquipmentScreen();
                        break;
                }
            }
        }

        /// <summary>
        /// Increase a stat by 1 and decrease available skillpoints by 1
        /// </summary>
        public void IncreaseStatWithSkillpoints(KeyCode key)
        {
            if (AvailableSkillpoints <= 0) return;

            switch (key)
            {
                case KeyCode.S:
                    Strength++;
                    StrengthSkillPoints++;
                    break;
                case KeyCode.V:
                    Vitality++;
                    VitalitySkillPoints++;
                    MaxHp = CalculateMaxHP();
                    CurrentHp = MaxHp;
                    break;
            }

            AvailableSkillpoints--;
        }

        /// <summary>
        /// Increase a stat by a specified amount [DOES NOT DECREASE AVAILABLE SKILLPOINTS]
        /// </summary>
        public void IncreaseStat(Stat stat, int amount)
        {
            switch (stat)
            {
                case Stat.Vitality:
                    Vitality += amount;
                    MaxHp = CalculateMaxHP();
                    break;
                case Stat.Strength:
                    Strength += amount;
                    break;
            }
        }

        /// <summary>
        /// Decrease a stat by a specified amount
        /// </summary>
        public void DecreaseStat(Stat stat, int amount)
        {
            switch (stat)
            {
                case Stat.Vitality:
                    Vitality -= amount;
                    MaxHp = CalculateMaxHP();
                    if (CurrentHp > MaxHp)
                    {
                        SetCurrentHpToMax();
                    }
                    break;
                case Stat.Strength:
                    Strength -= amount;
                    break;
            }
        }
        /// <summary>
        /// Increases players current xp and checks if player can level up
        /// </summary>
        public void IncreaseXP(float xpGained)
        {
            CurrentXP += xpGained;
            if (CurrentXP >= XpToLevelUp)
            {
                LevelUp();
            }
        }

        /// <summary>
        /// Decreases players current xp to a maximum of 0
        /// </summary>
        public void DecreaseXP(float xpLost)
        {
            CurrentXP -= xpLost;
            if (CurrentXP < 0)
            {
                CurrentXP = 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetSkillPoints()
        {
            Vitality -= VitalitySkillPoints;
            Strength -= StrengthSkillPoints;

            MaxHp = CalculateMaxHP();
            SetCurrentHpToMax();

            AvailableSkillpoints += VitalitySkillPoints + StrengthSkillPoints;

            VitalitySkillPoints = 0;
            StrengthSkillPoints = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public void LevelUp()
        {
            CurrentLevel++;

            // TESTING
            if (CurrentLevel == 5)
            {
                GameManagerRef.CanTakeFinalPath = true;
            }
            /////////////////////

            CurrentXP -= XpToLevelUp;
            XpToLevelUp *= 1.11f;
            AvailableSkillpoints += SkillPointsPerLevel;
            TotalSkillPointsGained += SkillPointsPerLevel;
            Console.Write("Congratulations, ");
            TextHelper.PrintTextInColor($"you've reached lvl {CurrentLevel}", Color.blue, false);
            Console.Write($"! {SkillPointsPerLevel} new skill {(SkillPointsPerLevel == 1 ? "point" : "points")} available.");
            Console.WriteLine("\n");
            if (CurrentXP >= XpToLevelUp)
            {
                LevelUp();
            }
        }
        /// <summary>
        /// Decreases current hp by specified amount and checks if current hp is under/equals to 0 and marks player as dead if it is
        /// </summary>
        public void TakeDamage(int damageTaken)
        {
            CurrentHp -= damageTaken;
            if (CurrentHp <= 0)
            {
                CurrentHp = 0;
                IsDead = true;
            }
        }

        /// <summary>
        /// Increases players current hp by amount specified and limits current hp to max hp
        /// </summary>
        public void Heal(int amountHealed)
        {
            // if player is already at max hp
            if (CurrentHp == MaxHp)
            {
                return;
            }
            // heal player for amountHealed
            CurrentHp += amountHealed;
            // if hp goes over maxHp
            if (CurrentHp >= MaxHp)
            {
                // set current hp to max hp
                CurrentHp = MaxHp;
            }
        }

        /// <summary>
        /// Gets a random sentence from an array and writes it to the console
        /// </summary>
        public async UniTask SpeakAboutEnvironment()
        {
            string randomSentence = EnvironmentObservations[Random.Next(EnvironmentObservations.Length)];
            await TextHelper.PrintStringCharByChar(randomSentence, Color.white);
            TextHelper.LineSpacing(0);
        }
    }
}
