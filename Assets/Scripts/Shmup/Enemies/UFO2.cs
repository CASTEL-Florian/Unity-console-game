using System;
using System.Linq;

namespace Shmup.Enemies
{
    internal class UFO2 : IEnemy
    {
        public static int scorePerKill = 80;
        public int Health = 50;
        public float X;
        public float Y;
        public int UpdatesSinceTeleport;
        public int TeleportFrequency = 360;

        private static readonly string[] Sprite =
        {
            @"     _!_     ",
            @"    /_O_\    ",
            @"-==<_‗_‗_>==-",
        };

        internal static int XMax = Sprite.Max(s => s.Length);
        internal static int YMax = Sprite.Length;

        public UFO2()
        {
            X = ShmupProgram.random.Next(ShmupProgram.gameWidth - XMax) + XMax / 2;
            Y = ShmupProgram.random.Next(ShmupProgram.gameHeight - YMax) + YMax / 2;
        }

        public void Render()
        {
            for (int y = 0; y < Sprite.Length; y++)
            {
                int yo = (int)Y + y;
                int yi = Sprite.Length - y - 1;
                if (yo >= 0 && yo < ShmupProgram.frameBuffer.GetLength(1))
                {
                    for (int x = 0; x < Sprite[y].Length; x++)
                    {
                        int xo = (int)X + x;
                        if (xo >= 0 && xo < ShmupProgram.frameBuffer.GetLength(0))
                        {
                            if (Sprite[yi][x] is not ' ')
                            {
                                ShmupProgram.frameBuffer[xo, yo] = Sprite[yi][x];
                            }
                        }
                    }
                }
            }
        }

        public void Update()
        {
            UpdatesSinceTeleport++;
            if (UpdatesSinceTeleport > TeleportFrequency)
            {
                X = ShmupProgram.random.Next(ShmupProgram.gameWidth - XMax) + XMax / 2;
                Y = ShmupProgram.random.Next(ShmupProgram.gameHeight - YMax) + YMax / 2;
                UpdatesSinceTeleport = 0;
            }
        }

        public bool CollidingWith(int x, int y)
        {
            int xo = x - (int)X;
            int yo = y - (int)Y;
            return
                yo >= 0 && yo < Sprite.Length &&
                xo >= 0 && xo < Sprite[yo].Length &&
                Sprite[yo][xo] is not ' ';
        }

        public bool IsOutOfBounds()
        {
            return !
                (X > 0 &&
                 X < ShmupProgram.gameWidth &&
                 Y > 0 &&
                 Y < ShmupProgram.gameHeight);
        }

        public void Shot()
        {
            Health--;
            if (Health <= 0)
            {
                ShmupProgram.enemies.Remove(this);
                ShmupProgram.score += scorePerKill;
            }
        }
    }
}