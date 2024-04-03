using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Text_Based_Game.Classes
{
    static class TextHelper
    {
        public static async Task PrintDeathAnimation(Color color)
        {
            var frame1 = await File.ReadAllLinesAsync(Path.Combine(Application.streamingAssetsPath, "Content/death1.txt"));
            var frame2 = await File.ReadAllLinesAsync(Path.Combine(Application.streamingAssetsPath,"Content/death2.txt"));
            var frame3 = await File.ReadAllLinesAsync(Path.Combine(Application.streamingAssetsPath,"Content/death3.txt"));
            var frame4 = await File.ReadAllLinesAsync(Path.Combine(Application.streamingAssetsPath,"Content/death4.txt"));
            var frame5 = await File.ReadAllLinesAsync(Path.Combine(Application.streamingAssetsPath,"Content/death5.txt"));
            var frame6 = await File.ReadAllLinesAsync(Path.Combine(Application.streamingAssetsPath,"Content/death6.txt"));
            var frame7 = await File.ReadAllLinesAsync(Path.Combine(Application.streamingAssetsPath,"Content/death7.txt"));

            Console.Clear();
            ChangeForegroundColor(color);
            foreach (var line in frame1)
            {
                Console.WriteLine(line);
            }
            await Console.Sleep(100);
            Console.Clear();
            foreach (var line in frame2)
            {
                Console.WriteLine(line);
            }
            await Console.Sleep(100);
            Console.Clear();
            foreach (var line in frame3)
            {
                Console.WriteLine(line);
            }
            await Console.Sleep(100);
            Console.Clear();
            foreach (var line in frame4)
            {
                Console.WriteLine(line);
            }
            await Console.Sleep(100);
            Console.Clear();
            foreach (var line in frame5)
            {
                Console.WriteLine(line);
            }
            await Console.Sleep(100);
            Console.Clear();
            foreach (var line in frame6)
            {
                Console.WriteLine(line);
            }
            await Console.Sleep(100);
            Console.Clear();
            foreach (var line in frame7)
            {
                Console.WriteLine(line);
            }

            ChangeForegroundColor(Color.gray);
        }
        /// <summary>
        /// Reads all lines in a file and either prints it line by line or letter by letter.
        /// </summary>
        public static async Task PrintTextFile(string path, bool letterByLetter)
        {
            string[] fileLines = await File.ReadAllLinesAsync(Path.Combine(Application.streamingAssetsPath, path));

            if (letterByLetter)
            {
#if !DEBUG_MODE
                foreach (string line in fileLines)
                {
                    await PrintStringCharByChar(line, Color.gray);
                    await Console.Sleep(500);
                    Console.WriteLine();
                }
#endif

#if DEBUG_MODE
                foreach (string line in fileLines)
                {
                    Console.WriteLine(line);
                }
#endif
            }
            else
            {
                foreach (string line in fileLines)
                {
                    Console.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Prints a string one character at a time. Can write text in color specified.
        /// </summary>
        public static async Task PrintStringCharByChar(string line, Color color)
        {
            ChangeForegroundColor(color);
            foreach (char c in line)
            {
                Console.Write(c);
#if DEBUG_MODE
                await Console.Sleep(5);
#endif

#if !DEBUG_MODE
                await Console.Sleep(25);
#endif
            }
            ChangeForegroundColor(Color.gray);
        }

        /// <summary>
        /// Writes text in color specified to the console. Defaults to write on a new line, can be set to write on the same line.
        /// </summary>
        public static void PrintTextInColor(string line, Color color, bool newLine = true)
        {
            ChangeForegroundColor(color);
            if (newLine)
            {
                Console.WriteLine(line);
            }
            else
            {
                Console.Write(line);
            }
            ChangeForegroundColor(Color.gray);
        }

        /// <summary>
        /// Changes the foreground (text) color of the console 
        /// </summary>
        public static void ChangeForegroundColor(Color color)
        {
            Console.SetForegroundColor(color);
        }

        /// <summary>
        /// Writes a blank line to the console
        /// </summary>
        public static void LineSpacing(int lines = 1)
        {
            if (lines < 1)
            {
                Console.WriteLine();
            }
            else
            {
                string newLines = "";
                for (int i = 0; i < lines; i++)
                {
                    newLines += "\n";
                }
                Console.WriteLine(newLines);
            }
        }
    }
}
