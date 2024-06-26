using System.Threading;
using Cysharp.Threading.Tasks;
using Text_Based_Game.Classes;
using UnityEngine;
using UnityEngine.SceneManagement;
using Console = UnityConsole.Console;

namespace Text_Based_Game
{
    public class TextAdventureProgram : MonoBehaviour
    {
        private bool quitApplication;
        private void Start()
        {
            UniTask.Create(Play);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene(0);
            }
            if (quitApplication)
            {
                Application.Quit();
            }
        }
        
        private async UniTask Play()
        {
            // print the title
            await TextHelper.PrintTextFile(Globals.TitlePath, false, this.GetCancellationTokenOnDestroy());
            // spacing
            TextHelper.LineSpacing(0);
            // wait 2.5 seconds
            //await Console.Sleep(2500);
            // print the intro lore
            await TextHelper.PrintTextFile(Globals.IntroPath, true, this.GetCancellationTokenOnDestroy());
            // spacing
            TextHelper.LineSpacing(0);
            // ask the player if they want to start the game
            Console.Write("Are you ready to start your adventure? (Y)es or any other key to quit: ");
            // get input
            
            KeyCode key = await Console.ReadKey(cancellationToken:this.GetCancellationTokenOnDestroy());
            // if input does not equal 'Y'/'y', quit game
            if (key != KeyCode.Y)
            {
                if (Application.isEditor)
                {
                    await TextHelper.PrintStringCharByChar("\nYou can't quit in the Unity Editor!", Color.gray);
                }
                else if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    await TextHelper.PrintStringCharByChar("\nIf you want to quit in the WebGL version, just close the tab!", Color.gray);
                }
                else
                {
                    quitApplication = true;
                }
            }

            
            // spacing
            TextHelper.LineSpacing();
            // initialize GameManager
            GameManager gameManager = new();
            await gameManager.LoadFiles(this.GetCancellationTokenOnDestroy());
            // start game
            await gameManager.StartGame(this.GetCancellationTokenOnDestroy());
        }
    }
}