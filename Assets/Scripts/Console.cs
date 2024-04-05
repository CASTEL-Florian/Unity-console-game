using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UnityConsole
{
    public static class Console
    {
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

        public static async UniTask<KeyCode> ReadKey()
        {
            return await UnityConsole.Instance.ReadKey();
        }

        public static void Clear()
        {
            UnityConsole.Instance.Clear();
        }

        public static void ResetColor()
        {
            UnityConsole.Instance.ResetColor();
        }

        public static void SetForegroundColor(Color color)
        {
            UnityConsole.Instance.SetForegroundColor(color);
        }

        public static async UniTask WaitUntil(Func<bool> condition, int frequency = 16)
        {
            await Task.Run(async () =>
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