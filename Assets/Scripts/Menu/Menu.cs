using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Console = UnityConsole.Console;

[Serializable]
public class MenuItem
{
    public string name;
    public int sceneIndex;
}
public class Menu : MonoBehaviour
{
    [SerializeField] private List<MenuItem> menuItems;
    private void Start()
    {
        UniTask.Create(Play);
    }

    private async UniTask Play()
    {
        Console.WriteLine(
            "This project demonstrates a the Unity Console package to port a .NET Console application to Unity.");
        Console.WriteLine();
        Console.WriteLine("The following scenes are available:");
        for (int i = 0; i < menuItems.Count; i++)
        {
            Console.WriteLine($"{i+1}: {menuItems[i].name}");
        }
        Console.WriteLine();
        Console.WriteLine("Press Escape at any time to return to this menu.");
        Console.Write("Choose a number to select a scene: ");

        string line = await Console.ReadLine();

        int result;
        while (!int.TryParse(line, out result) || result < 1 || result > menuItems.Count)
        {
            Console.WriteLine("Invalid input. Please enter a valid number.");
            line = await Console.ReadLine();
        }

        SceneManager.LoadScene(menuItems[result - 1].sceneIndex);
    }
}
