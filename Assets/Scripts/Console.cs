using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UnityConsole
{
    public static class Console
    {
        public static Color ForegroundColor
        {
            get => UnityConsole.Instance.ForegroundColor;
            set => UnityConsole.Instance.ForegroundColor = value;
        }
        
        public static Color BackgroundColor
        {
            get => UnityConsole.Instance.BackgroundColor;
            set => UnityConsole.Instance.BackgroundColor = value;
        }
        
        public static bool CursorVisible
        {
            get => UnityConsole.Instance.CursorVisible;
            set => UnityConsole.Instance.CursorVisible = value;
        }

        public static int WindowWidth
        {
            get => UnityConsole.Instance.WindowWidth;
            set => UnityConsole.Instance.WindowWidth = value;
        }

        public static int WindowHeight
        {
            get => UnityConsole.Instance.WindowHeight;
            set => UnityConsole.Instance.WindowHeight = value;
        }
        
        public static bool CenterContentX
        {
            get => UnityConsole.Instance.CenterContentX;
            set => UnityConsole.Instance.CenterContentX = value;
        }
        
        public static bool CenterContentY
        {
            get => UnityConsole.Instance.CenterContentY;
            set => UnityConsole.Instance.CenterContentY = value;
        }
        
        public static float BorderSize
        {
            get => UnityConsole.Instance.BorderSize;
            set => UnityConsole.Instance.BorderSize = value;
        }
        
        public static bool InputBufferActive
        {
            get => UnityConsole.Instance.InputBufferActive;
            set => UnityConsole.Instance.InputBufferActive = value;
        }
        
        public static float PixelsPerUnit
        {
            get => UnityConsole.Instance.PixelsPerUnit;
            set => UnityConsole.Instance.PixelsPerUnit = value;
        }
        
        public static void Write(string value)
        {
            UnityConsole.Instance.Write(value);
        }

        public static void Write(char value)
        {
            UnityConsole.Instance.Write(value);
        }

        public static void WriteLine(string value)
        {
            UnityConsole.Instance.WriteLine(value);
        }

        public static void WriteLine()
        {
            UnityConsole.Instance.WriteLine();
        }

        public static async UniTask<string> ReadLine()
        {
            return await UnityConsole.Instance.ReadLine();
        }
        
        public static async UniTask<KeyCode> ReadKey(bool intercept = false)
        {
            return await UnityConsole.Instance.ReadKey(intercept);
        }
        
        public static bool GetKeyState(KeyCode key)
        {
            return Input.GetKey(key);
        }
        
        public static bool KeyAvailable => UnityConsole.Instance.KeyAvailable;

        public static void Clear()
        {
            UnityConsole.Instance.Clear();
        }

        public static void ResetColor()
        {
            UnityConsole.Instance.ResetColor();
        }
        
        public static async UniTask Beep(int frequency = 800, int duration = 200)
        {
            await UnityConsole.Instance.Beep(frequency, duration);
        }
        
        public static async UniTask WaitUntil(Func<bool> condition, int frequency = 16)
        {
            await UniTask.Create(async () =>
            {
                while (!condition()) await UniTask.Delay(frequency);
            });
        }

        public static async UniTask Sleep(int milliseconds)
        {
            await UniTask.Delay(milliseconds);
        }
    }
}