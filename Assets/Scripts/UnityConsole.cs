using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class UnityConsole : MonoBehaviour
{
    public TextMeshProUGUI consoleText;
    public string bodyText;
    public string inputText;
    [SerializeField] private Color defaultForegroundColor;
    private Color defaultBackgroundColor;

    private Color currentForegroundColor;
    
    private bool isGettingKey;
    private bool isGettingLine;
    private KeyCode keyCode;

    private bool needsUpdate;
    
    private string cursor = "|";
    private string visibleCursor = "|";
    private string invisibleCursor = "<alpha=#00>|";
    bool blinkCursor = false;
    
    private Coroutine blinkCursorCoroutine;
    
    public static UnityConsole Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            bodyText = "<mspace=0.2>";
            SetForegroundColor(defaultForegroundColor);
            defaultBackgroundColor = Color.clear;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(BlinkCursor());
    }

    private void Update()
    {
        if (needsUpdate)
        {
            UpdateConsoleText();
            needsUpdate = false;
        }
        if (!isGettingKey && !isGettingLine)
        {
            return;
        }
        if (isGettingKey && Input.anyKeyDown)
        {
            isGettingKey = false;
            keyCode = Input.inputString.Length >= 1 ? (KeyCode)Input.inputString[0] : KeyCode.None;
            return;
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ProcessCommand(inputText);
        }
        else if (Input.anyKeyDown)
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        foreach (char c in Input.inputString)
        {
            if (c == '\b')
            {
                if (inputText.Length > 0)
                {
                    inputText = inputText.Substring(0, inputText.Length - 1);
                }
            }
            else if ((c == '\n') || (c == '\r'))
            {
                ProcessCommand(inputText);
            }
            else
            {
                inputText += c;
            }

            needsUpdate = true;
        }
    }

    private void ProcessCommand(string command)
    {
        WriteLine(command);
        isGettingLine = false;
        
    }

    public void Write(string value)
    {
        bodyText += value;
        needsUpdate = true;
    }
    
    public void Write(char value)
    {

        bodyText += value;
        needsUpdate = true;
    }

    public void WriteLine(string value)
    {
        bodyText += value + "\n";
        needsUpdate = true;
    }
    
    public void WriteLine()
    {
        bodyText += "\n";
        needsUpdate = true;
    }
    
    public async Task<string> ReadLine()
    {
        isGettingLine = true;
        blinkCursor = true;
        await Console.WaitUntil(() => !isGettingLine);
        blinkCursor = false;
        cursor = visibleCursor;
        string result = inputText;
        inputText = "";
        return result;
    }

    public async Task<KeyCode> ReadKey()
    {
        isGettingKey = true;
        blinkCursor = true;
        await Console.WaitUntil(() => !isGettingKey);
        blinkCursor = false;
        
        if ((int)KeyCode.A <= (int)keyCode && (int)keyCode <= (int)KeyCode.Z)
        {
            Write(keyCode.ToString());
        }
        cursor = visibleCursor;
        return keyCode;

    }

    public void Clear()
    {
        bodyText = "<mspace=0.2><color=#" + ColorUtility.ToHtmlStringRGB(currentForegroundColor) + ">";
        inputText = "";
        needsUpdate = true;
    }

    public void ResetColor()
    {
        currentForegroundColor = defaultForegroundColor;
    }
    

    public void SetForegroundColor(Color color)
    {
        currentForegroundColor = color;
        bodyText += $"<color=#{ColorUtility.ToHtmlStringRGB(currentForegroundColor)}>";
    }

    public void SetTitle(string title)
    {
        gameObject.name = title;
    }

    private IEnumerator BlinkCursor()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if (!blinkCursor)
            {
                cursor = visibleCursor;
                continue;
            }
            cursor = cursor == invisibleCursor ? visibleCursor : invisibleCursor;
            needsUpdate = true;
        }
    }
    
    private void UpdateConsoleText()
    {
        consoleText.text = bodyText + inputText + cursor;
    }
}