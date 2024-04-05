# C# Console Game Porting to Unity
![Capture d'écran 2024-04-03 213709](https://github.com/CASTEL-Florian/Unity-console-game/assets/106156391/0fd2c67a-92d8-4db6-972f-1c71ed94ed11)
*The same game running in the console (on the right) and in Unity (on the left).*

<br><br>

This project shows an example porting a C# console game into a Unity project.
The current implementation of the console supports the following methods:
```C#
Console.Write(string value);
Console.Write(char value);
Console.WriteLine(string value);
Console.WriteLine();
Console.ReadLine();
Console.ReadKey();
Console.Clear();
Console.ResetColor();
Console.SetForegroundColor(Color color);
```
Console.BackgroundColor is not supported.

The project contains a porting of the text adventure game **Alaric's Adventure: the Beginning**.

Original game by Mekkmann : https://mekkmann.itch.io/alarics-adventure-text-based-game

<br><br>

# How to port your console game to Unity
The main steps to consider when converting a console game to a Unity game are the following:
- Replace `Thread.Sleep(time)` with `Console.Sleep(time)`
<br>
- Replace the read functions with async functions and use KeyCode instead of ConsoleKeyInfo:
`ConsoleKeyInfo key = Console.ReadKey();` with `KeyCode key = await Console.ReadKey();`
`string input = Console.ReadLine();` with `string input = await Console.ReadLine();`
<br>
- Turn every function that call a read function on the Console into an async function.

**In the following code, I use UniTask instead of Task for the WebGL support. See the WebGL support section for more information**
  
For example:

```C#
private void F1(){
  F2();
}

private void F2(){
  string input = Console.ReadLine();
}
```
Replace the code above with:
```C#
private async UniTask F1(){
  await F2();
}

private async UniTask F2(){
  string input = await Console.ReadLine();
}
```

<br>

- Replace the entry point of the game with a Monobehaviour:

```C#
internal class Program
    {
        static void Main(string[] args)
        {
            GameManager gameManager = new();
            gameManager.StartGame();
        }
    }
```
Replace the code above with:

```C#
public class Program : MonoBehaviour
{
    private void Start()
    {
        UniTask.Run(Play).ContinueWith((t) =>
        {
            if (t.IsFaulted) Debug.LogError(t.Exception);
        });
    }

    private async Task Play()
    {
        GameManager gameManager = new();
        await gameManager.StartGame();
    }
}
```
<br>

- Replace the colors in the game with Unity colors.
<br>

# WebGL support
The carriage return character produces weird results in WebGL TextMeshPro. The character is therefore automatically removed from texts before printing in the console.

The framework support WebGL using [UniTask](https://github.com/Cysharp/UniTask) instead of Task for async method.

A FileLoader class provides the following methods to load from a file. These methods are also compatible with non-WebGL builds.
```C#
public static async UniTask<string[]> ReadAllLinesAsync(string path);
public static async UniTask<string> ReadAllTextAsync(string path);
public static async UniTask<byte[]> ReadAllBytesAsync(string path);
```

These methods need to be executed in asynchronous code. For example, you may nee to move the calls async methods out of the costructors:
```C#
private void CreateEnemy(string namePath){
    Enemy enemy = new Enemy(namePath);
}
public Enemy()
{
    Name = await FileLoader.ReadAllLinesAsync(string path); // Compilation error
}
```
Replace the code above with
```C#
private async UniTask CreateEnemy(string namePath){
    Enemy enemy = new Enemy(namePath);
    await enemy.LoadNameFromFile(namePath)
}
public Enemy()
{
}

// In the Enemy class
public async UniTask LoadNameFromFile(path){
  Name = await FileLoader.ReadAllLinesAsync(string path);
}
```

- Put the resources of the project in a folder such as Resources or StreamingAssets and change the paths in the code accordingly.
