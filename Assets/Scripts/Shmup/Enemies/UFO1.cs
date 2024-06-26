using System;
using System.Linq;

namespace Shmup.Enemies
{
    internal class UFO1 : IEnemy
    {
        public static int scorePerKill = 10;
        public int Health = 10;
        public float X;
        public float Y;
        public float XVelocity = 1f / 8f;
        public float YVelocity = 1f / 8f;

        private static readonly string[] Sprite =
        {
            @" _!_ ",
            @"(_o_)",
            @" ''' ",
        };

        internal static int XMax = Sprite.Max(s => s.Length);
        internal static int YMax = Sprite.Length;

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
            if (ShmupProgram.player.X < X)
            {
                X = Math.Max(ShmupProgram.player.X, X - XVelocity);
            }
            else
            {
                X = Math.Min(ShmupProgram.player.X, X + XVelocity);
            }

            if (ShmupProgram.player.Y < Y)
            {
                Y = Math.Max(ShmupProgram.player.Y, Y - YVelocity);
            }
            else
            {
                Y = Math.Min(ShmupProgram.player.Y, Y + YVelocity);
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
            return
                XVelocity <= 0 && X < -XMax ||
                YVelocity <= 0 && Y < -YMax ||
                XVelocity >= 0 && X > ShmupProgram.gameWidth + XMax ||
                YVelocity >= 0 && Y > ShmupProgram.gameHeight + YMax;
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