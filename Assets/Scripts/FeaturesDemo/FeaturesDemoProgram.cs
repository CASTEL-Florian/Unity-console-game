using System.Threading;
using Console = UnityConsole.Console;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FeaturesDemoProgram : MonoBehaviour
{
    private static readonly string[] Sgdc =
    {
        @" #######                                                 ",
        @" #       ######   ##   ##### #    # #####  ######  ####  ",
        @" #       #       #  #    #   #    # #    # #      #      ",
        @" #####   #####  #    #   #   #    # #    # #####   ####  ",
        @" #       #      ######   #   #    # #####  #           # ",
        @" #       #      #    #   #   #    # #   #  #      #    # ",
        @" #       ###### #    #   #    ####  #    # ######  ####  ",
    };

    [SerializeField] private float scrollSpeed = 10f;

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
        if (!Input.GetKey(KeyCode.LeftControl))
        {
            return;
        }

        float scroll = Input.mouseScrollDelta.y;
        Console.PixelsPerUnit += scroll * scrollSpeed;
    }

    private async UniTask Play(CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < Sgdc.Length; i++)
        {
            Console.WriteLine(Sgdc[i]);
        }
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("Porting Dotnet Console app to Unity");
        Console.WriteLine("Press any key to continue, press ESC to go back to the main menu");
        Console.WriteLine("Use CTRL + Mouse Scroll to zoom in/out (only in windowed mode)");
        
        Console.WriteLine();
        await Console.ReadKey(true, cancellationToken);
        Console.WriteLine("Implementation of most of the Console class methods:");
        Console.WriteLine();
        Console.WriteLine("Console.Write(string value);");
        Console.WriteLine("Console.Write(char value);");
        Console.WriteLine("Console.WriteLine(string value);");
        Console.WriteLine("Console.WriteLine();");
        Console.WriteLine("Console.ReadLine();");
        Console.WriteLine("Console.ReadKey(bool intercept);");
        Console.WriteLine("Console.Clear();");
        Console.WriteLine("Console.Beep(int frequency, int duration);");
        Console.WriteLine("Console.ResetColor();");
        Console.WriteLine("Console.ForegroundColor {get; set; }");
        Console.WriteLine("Console.BackgroundColor {get; set; }");
        Console.WriteLine("Console.KeyAvailable {get; }");
        Console.WriteLine("Console.CursorVisible {get; set; }");
        Console.WriteLine("Console.WindowWidth {get; set; }");
        Console.WriteLine("Console.WindowHeight {get; set; }");

        await Console.ReadKey(true, cancellationToken);
        Console.ForegroundColor = Color.green;
        Console.WriteLine("Change the color!");
        Console.ForegroundColor = Color.red;
        Console.WriteLine("Change the color!");
        Console.ForegroundColor = Color.yellow;
        Console.WriteLine("Change the color!");
        Console.ForegroundColor = Color.blue;
        Console.WriteLine("Change the color!");
        await Console.ReadKey(true, cancellationToken);
        Console.ForegroundColor = Color.black;
        Console.BackgroundColor = Color.green;
        Console.WriteLine("Change the color!");
        Console.BackgroundColor = Color.red;
        Console.WriteLine("Change the color!");
        Console.BackgroundColor = Color.yellow;
        Console.WriteLine("Change the color!");
        Console.BackgroundColor = Color.blue;
        Console.WriteLine("Change the color!");
        Console.ResetColor();
        
        await Console.ReadKey(true, cancellationToken);
        Console.WriteLine();
        Console.WriteLine("Play music! (Not supported in WebGL)");
        await Console.ReadKey(true, cancellationToken);
        await PlayMusic(cancellationToken);
        Console.WriteLine("Resize the console window!");
        await Console.ReadKey(true, cancellationToken);
        Console.WriteLine();
        Console.Clear();
        Console.WriteLine("Resize the console window!");
        Console.WriteLine();
        string loremIpsum = Resources.Load<TextAsset>("LoremIpsum").text;
        Console.WriteLine(loremIpsum);
        Console.CursorVisible = false;
        for (int i = 0; i < 10; i++)
        {
            Console.WindowWidth += 2;
            await Console.Sleep(100, cancellationToken);
        }

        for (int i = 0; i < 10; i++)
        {
            Console.WindowHeight += 1;
            await Console.Sleep(100, cancellationToken);
        }

        for (int i = 0; i < 10; i++)
        {
            Console.WindowWidth -= 2;
            await Console.Sleep(100, cancellationToken);
        }

        for (int i = 0; i < 10; i++)
        {
            Console.WindowHeight -= 1;
            await Console.Sleep(100, cancellationToken);
        }
        
        await Console.ReadKey(true, cancellationToken);
        Console.Clear();
        Console.WriteLine("Other useful methods:\n");
        await Console.ReadKey(true, cancellationToken);
        Console.WriteLine("Console.Sleep(int milliseconds);");
        Console.WriteLine("Console.GetKeyState(KeyCode key);");
        Console.WriteLine("Console.PixelsPerUnit {get; set; }");
        Console.WriteLine("Console.InputBufferActive {get; set; }");
        Console.WriteLine("Console.BorderSize {get; set; }");


        await Console.ReadKey(true, cancellationToken);
        Console.Clear();
        Console.WindowWidth += 11;
        Console.Write("\n\n");
        Console.WriteLine("Dotnet Console app                                       Unity Script");
        await Console.ReadKey(true, cancellationToken);
        Console.Write("\n\n\n");
        Console.Write("internal class Program                                 |  ");
        Console.Write("internal class Program");
        Console.ForegroundColor = Color.green;
        Console.WriteLine(" : MonoBehaviour");
        Console.ResetColor();

        Console.Write("{                                                      |  ");
        Console.WriteLine("{");

        Console.ForegroundColor = Color.red;
        Console.Write("    static void Main(string[] args)");
        Console.ResetColor();
        Console.Write("                    |  ");
        Console.ForegroundColor = Color.green;
        Console.WriteLine("    private void Start()");
        Console.ResetColor();


        Console.Write("    {                                                  |  ");
        Console.WriteLine("    {");
        Console.Write("        StartGame();                                   |  ");
        Console.ForegroundColor = Color.green;
        Console.Write("        UniTask.Create(");
        Console.ResetColor();
        Console.WriteLine("StartGame);");
        Console.Write("    }                                                  |  ");
        Console.WriteLine("    }");
        Console.WriteLine("                                                       |  ");

        Console.Write("    private ");
        Console.ForegroundColor = Color.red;
        Console.Write("void");
        Console.ResetColor();
        Console.Write(" StartGame()                           |  ");
        Console.Write("    private ");
        Console.ForegroundColor = Color.green;
        Console.Write("async UniTask ");
        Console.ResetColor();
        Console.WriteLine("StartGame()");

        Console.Write("    {                                                  |  ");
        Console.WriteLine("    {");

        Console.Write("        Console.ForegroundColor = ");
        Console.ForegroundColor = Color.red;
        Console.Write("ConsoleColor.Green");
        Console.ResetColor();
        Console.Write(";  |  ");
        Console.Write("        Console.ForegroundColor = ");
        Console.ForegroundColor = Color.green;
        Console.Write("Color.green");
        Console.ResetColor();
        Console.WriteLine(";");

        Console.Write("        Console.WriteLine(\"Hello\");                    |  ");
        Console.WriteLine("        Console.WriteLine(\"Hello\");");

        Console.ForegroundColor = Color.red;
        Console.Write("        ConsoleKeyInfo");
        Console.ResetColor();
        Console.Write(" key = Console.ReadKey();        |  ");
        Console.ForegroundColor = Color.green;
        Console.Write("        KeyCode");
        Console.ResetColor();
        Console.Write(" key = ");
        Console.ForegroundColor = Color.green;
        Console.Write("await");
        Console.ResetColor();
        Console.WriteLine(" Console.ReadKey();");

        Console.ForegroundColor = Color.red;
        Console.Write("        Thread");
        Console.ResetColor();
        Console.Write(".Sleep(1000);                            |  ");
        Console.ForegroundColor = Color.green;
        Console.Write("        await Console");
        Console.ResetColor();
        Console.WriteLine(".Sleep(1000);");


        Console.Write("    }                                                  |  ");
        Console.WriteLine("    }");
        Console.Write("}                                                      |  ");
        Console.WriteLine("}");
        Console.CursorVisible = false;
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("Press ESC to go back to the main menu");
    }


    // Original code for Console.Beep demonstration from https://learn.microsoft.com/en-us/dotnet/api/system.console.beep?view=net-8.0#system-console-beep(system-int32-system-int32)
    public async UniTask PlayMusic(CancellationToken cancellationToken = default)
    {
        Note[] notes =
        {
            new Note(Tone.A, Duration.QUARTER),
            new Note(Tone.Asharp, Duration.QUARTER),
            new Note(Tone.B, Duration.QUARTER),
            new Note(Tone.C, Duration.QUARTER),
            new Note(Tone.Csharp, Duration.QUARTER),
            new Note(Tone.D, Duration.QUARTER),
            new Note(Tone.Dsharp, Duration.QUARTER),
            new Note(Tone.E, Duration.QUARTER),
            new Note(Tone.F, Duration.QUARTER),
            new Note(Tone.Fsharp, Duration.QUARTER),
            new Note(Tone.G, Duration.QUARTER),
            new Note(Tone.Gsharp, Duration.QUARTER),
        };

// Play the song
        await PlayNotes(notes, cancellationToken);
    }

// Play the notes in a song.
    private async UniTask PlayNotes(Note[] tune, CancellationToken cancellationToken = default)
    {
        foreach (Note n in tune)
        {
            if (n.NoteTone == Tone.REST)
                await Console.Sleep((int)n.NoteDuration, cancellationToken);
            else
                await Console.Beep((int)n.NoteTone, (int)n.NoteDuration, cancellationToken);
        }
    }

// Define the frequencies of notes in an octave, as well as
// silence (rest).
    protected enum Tone
    {
        REST = 0,
        GbelowC = 196,
        A = 220,
        Asharp = 233,
        B = 247,
        C = 262,
        Csharp = 277,
        D = 294,
        Dsharp = 311,
        E = 330,
        F = 349,
        Fsharp = 370,
        G = 392,
        Gsharp = 415
    }

// Define the duration of a note in units of milliseconds.
    protected enum Duration
    {
        WHOLE = 800,
        HALF = WHOLE / 2,
        QUARTER = HALF / 2,
        EIGHTH = QUARTER / 2,
        SIXTEENTH = EIGHTH / 2,
    }

// Define a note as a frequency (tone) and the amount of
// time (duration) the note plays.
    protected struct Note
    {
        Tone toneVal;
        Duration durVal;

// Define a constructor to create a specific note.
        public Note(Tone frequency, Duration time)
        {
            toneVal = frequency;
            durVal = time;
        }

// Define properties to return the note's tone and duration.
        public Tone NoteTone
        {
            get { return toneVal; }
        }

        public Duration NoteDuration
        {
            get { return durVal; }
        }
    }
}