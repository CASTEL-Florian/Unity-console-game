using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Shmup.Enemies;
using UnityEngine.SceneManagement;
using Console = UnityConsole.Console;

namespace Shmup
{
    public class ShmupProgram : MonoBehaviour
    {
        internal static bool closeRequested;
        internal static Stopwatch stopwatch = new();
        internal static bool pauseUpdates;

        internal static int gameWidth = 80;
        internal static int gameHeight = 40;
        internal static int intendedMinConsoleWidth = gameWidth + 3;
        internal static int intendedMinConsoleHeight = gameHeight + 6;
        internal static char[,] frameBuffer = new char[gameWidth, gameHeight];
        internal static string topBorder = '□' + new string('-', gameWidth) + '□';
        internal static string bottomBorder = '□' + new string('-', gameWidth) + '□';

        internal static int consoleWidth;
        internal static int consoleHeight;
        internal static StringBuilder render = new(gameWidth * gameHeight);

        internal static long score;
        internal static int update;
        internal static bool isDead;

        internal static Player player = new()
        {
            X = gameWidth / 2,
            Y = gameHeight / 4,
        };

        internal static List<PlayerBullet> playerBullets = new();
        internal static List<PlayerBullet> explodingBullets = new();
        internal static List<IEnemy> enemies = new();
        internal static bool playing;
        internal static bool waitingForInput = true;
        internal static Random random = new();

        private void Start()
        {
            UniTask.Create(() => Play(this.GetCancellationTokenOnDestroy()));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene(0);
            }
        }


        internal async UniTask Play(CancellationToken cancellationToken = default)
        {
            consoleWidth = Console.WindowWidth;
            consoleHeight = Console.WindowHeight;
            if (consoleWidth < intendedMinConsoleWidth || consoleHeight < intendedMinConsoleHeight)
            {
                try
                {
                    Console.WindowWidth = intendedMinConsoleWidth;
                    Console.WindowHeight = intendedMinConsoleHeight;
                }
                catch
                {
                    // nothing
                }

                consoleWidth = Console.WindowWidth;
                consoleHeight = Console.WindowHeight;
            }

            Console.Clear();

            while (!closeRequested)
            {
                Initialize();
                while (!closeRequested && playing)
                {
                    await UpdateGame(cancellationToken);
                    if (closeRequested)
                    {
                        return;
                    }

                    if (cancellationToken.IsCancellationRequested) return;
                    Render();
                    await SleepAfterRender(cancellationToken);
                }
            }
        }

        internal static void Initialize()
        {
            score = 0;
            update = 0;
            isDead = false;
            player = new()
            {
                X = gameWidth / 2,
                Y = gameHeight / 4,
            };
            playerBullets = new();
            explodingBullets = new();
            enemies = new();
            playing = true;
            waitingForInput = true;
        }

        internal async UniTask UpdateGame(CancellationToken cancellationToken = default)
        {
            bool u = false;
            bool d = false;
            bool l = false;
            bool r = false;
            bool shoot = false;
            while (Console.KeyAvailable)
            {
                switch (await Console.ReadKey(true, cancellationToken))
                {
                    case KeyCode.Escape:
                        closeRequested = true;
                        return;
                    case KeyCode.Return:
                        playing = !isDead;
                        return;
                    case KeyCode.W or KeyCode.UpArrow:
                        u = true;
                        break;
                    case KeyCode.A or KeyCode.LeftArrow:
                        l = true;
                        break;
                    case KeyCode.S or KeyCode.DownArrow:
                        d = true;
                        break;
                    case KeyCode.D or KeyCode.RightArrow:
                        r = true;
                        break;
                    case KeyCode.Space:
                        shoot = true;
                        break;
                }
            }
            
            u = u || Console.GetKeyState(KeyCode.W);
            l = l || Console.GetKeyState(KeyCode.A);
            d = d || Console.GetKeyState(KeyCode.S);
            r = r || Console.GetKeyState(KeyCode.D);

            u = u || Console.GetKeyState(KeyCode.UpArrow);
            l = l || Console.GetKeyState(KeyCode.LeftArrow);
            d = d || Console.GetKeyState(KeyCode.DownArrow);
            r = r || Console.GetKeyState(KeyCode.RightArrow);

            shoot = shoot || Console.GetKeyState(KeyCode.Space);
            
            if (waitingForInput)
            {
                waitingForInput =  !(u || d || l || r || shoot);
            }

            if (pauseUpdates)
            {
                return;
            }

            if (isDead)
            {
                return;
            }

            if (waitingForInput)
            {
                return;
            }

            update++;

            if (update % 50 is 0)
            {
                SpawnARandomEnemy();
            }

            for (int i = 0; i < playerBullets.Count; i++)
            {
                playerBullets[i].Y++;
            }

            foreach (IEnemy enemy in enemies)
            {
                enemy.Update();
            }

            player.State = Player.States.None;
            if (l && !r)
            {
                player.X = Math.Max(0, player.X - 1);
                player.State |= Player.States.Left;
            }

            if (r && !l)
            {
                player.X = Math.Min(gameWidth - 1, player.X + 1);
                player.State |= Player.States.Right;
            }

            if (u && !d)
            {
                player.Y = Math.Min(gameHeight - 1, player.Y + 1);
                player.State |= Player.States.Up;
            }

            if (d && !u)
            {
                player.Y = Math.Max(0, player.Y - 1);
                player.State |= Player.States.Down;
            }

            if (shoot)
            {
                playerBullets.Add(new() { X = (int)player.X - 2, Y = (int)player.Y });
                playerBullets.Add(new() { X = (int)player.X + 2, Y = (int)player.Y });
            }

            explodingBullets.Clear();

            for (int i = 0; i < playerBullets.Count; i++)
            {
                PlayerBullet bullet = playerBullets[i];
                bool exploded = false;
                IEnemy[] enemiesClone = new IEnemy[enemies.Count];
                enemies.CopyTo(enemiesClone);
                for (int j = 0; j < enemiesClone.Length; j++)
                {
                    if (enemiesClone[j].CollidingWith(bullet.X, bullet.Y))
                    {
                        if (!exploded)
                        {
                            playerBullets.RemoveAt(i);
                            explodingBullets.Add(bullet);
                            i--;
                            exploded = true;
                        }

                        enemiesClone[j].Shot();
                    }
                }

                if (!exploded && (bullet.X < 0 || bullet.Y < 0 || bullet.X >= gameWidth || bullet.Y >= gameHeight))
                {
                    playerBullets.RemoveAt(i);
                    i--;
                }
            }

            foreach (IEnemy enemy in enemies)
            {
                if (enemy.CollidingWith((int)player.X, (int)player.Y))
                {
                    isDead = true;
                    return;
                }
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].IsOutOfBounds())
                {
                    enemies.RemoveAt(i);
                    i--;
                }
            }
        }

        internal static void SpawnARandomEnemy()
        {
            if (random.Next(2) is 0)
            {
                enemies.Add(new Tank()
                {
                    X = random.Next(gameWidth - 10) + 5,
                    Y = gameHeight + Tank.YMax,
                    YVelocity = -1f / 10f,
                });
            }
            else if (random.Next(2) is 0)
            {
                enemies.Add(new Helicopter()
                {
                    X = -Helicopter.XMax,
                    XVelocity = 1f / 3f,
                    Y = random.Next(gameHeight - 10) + 5,
                });
            }
            else if (random.Next(3) is 0 or 1)
            {
                enemies.Add(new UFO1()
                {
                    X = random.Next(gameWidth - 10) + 5,
                    Y = gameHeight + UFO1.YMax,
                });
            }
            else
            {
                enemies.Add(new UFO2());
            }
        }

        internal static void Render()
        {
            const int maxRetryCount = 10;
            int retry = 0;
            Retry:
            if (retry > maxRetryCount)
            {
                return;
            }

            if (consoleWidth != Console.WindowWidth || consoleHeight != Console.WindowHeight)
            {
                consoleWidth = Console.WindowWidth;
                consoleHeight = Console.WindowHeight;
                Console.Clear();
            }

            if (consoleWidth < intendedMinConsoleWidth || consoleHeight < intendedMinConsoleHeight)
            {
                Console.Clear();
                Console.Write(
                    $"Console too small at {consoleWidth}w x {consoleHeight}h. Please increase to at least {intendedMinConsoleWidth}w x {intendedMinConsoleHeight}h.");
                pauseUpdates = true;
                return;
            }

            pauseUpdates = false;
            ClearFrameBuffer();
            player.Render();
            foreach (IEnemy enemy in enemies)
            {
                enemy.Render();
            }

            foreach (PlayerBullet bullet in playerBullets)
            {
                if (bullet.X >= 0 && bullet.X < gameWidth && bullet.Y >= 0 && bullet.Y < gameHeight)
                {
                    frameBuffer[bullet.X, bullet.Y] = '^';
                }
            }

            foreach (PlayerBullet explode in explodingBullets)
            {
                if (explode.X >= 0 && explode.X < gameWidth && explode.Y >= 0 && explode.Y < gameHeight)
                {
                    frameBuffer[explode.X, explode.Y] = '#';
                }
            }

            render.Clear();
            render.AppendLine(topBorder);
            for (int y = gameHeight - 1; y >= 0; y--)
            {
                render.Append('|');
                for (int x = 0; x < gameWidth; x++)
                {
                    render.Append(frameBuffer[x, y]);
                }

                render.AppendLine("|");
            }

            render.AppendLine(bottomBorder);
            render.AppendLine($"Score: {score}                             ");
            if (waitingForInput)
            {
                render.AppendLine("Press [WASD] or [SPACEBAR] to start...  ");
            }

            if (isDead)
            {
                render.AppendLine("YOU DIED! Press [ENTER] to play again...");
            }
            else
            {
                render.AppendLine("                                        ");
            }

            try
            {
                Console.CursorVisible = false;
                Console.Clear();
                Console.Write(render.ToString());
            }
            catch
            {
                retry++;
                goto Retry;
            }
        }

        internal static void ClearFrameBuffer()
        {
            for (int x = 0; x < gameWidth; x++)
            {
                for (int y = 0; y < gameHeight; y++)
                {
                    frameBuffer[x, y] = ' ';
                }
            }
        }

        internal static async UniTask SleepAfterRender(CancellationToken cancellationToken = default)
        {
            float sleep = 1f / 120f - stopwatch.Elapsed.Seconds;
            if (sleep > 0)
            {
                await Console.Sleep((int)(sleep * 1000), cancellationToken);
            }

            stopwatch.Restart();
        }
    }
}