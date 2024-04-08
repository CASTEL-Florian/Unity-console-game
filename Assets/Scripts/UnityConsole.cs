using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;

namespace UnityConsole
{
    public class UnityConsole : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI consoleBackground;
        [SerializeField] private TextMeshProUGUI consoleText;
        [SerializeField] private Color defaultForegroundColor;
        [SerializeField] private int defaultWindowWidth = 93;
        [SerializeField] private int defaultWindowHeight = 27;
        [SerializeField] private float borderSize = 0.5f;
        [SerializeField] private ConsoleCamera consoleCamera;
        [SerializeField] private bool centerContentX = false;
        [SerializeField] private bool centerContentY = false;
        
        [SerializeField] private RectTransform consoleRectTransform;
        
        private string backgroundText;
        private string bodyText;
        private string inputText;

        private Color currentForegroundColor;
        private Color currentBackgroundColor;

        private bool isGettingKey;
        private bool isGettingLine;
        private KeyCode keyCode;

        private bool needsUpdate;

        private string cursor = "|";
        private readonly string visibleCursor = "|";
        private readonly string invisibleCursor = "<alpha=#00>|";
        bool isBlinkCursorActive;
        private Coroutine blinkCursorCoroutine;
        
        private const float CharacterSpacing = 0.2f;
        private float characterHeight;

        public int WindowWidth
        {
            get => Mathf.RoundToInt(consoleRectTransform.sizeDelta.x / CharacterSpacing);
            set
            {
                if (WindowWidth == value)
                {
                    return;
                }
                consoleRectTransform.sizeDelta =
                    new Vector2(value * CharacterSpacing, consoleRectTransform.sizeDelta.y);
                UpdateCamera();
            }
        }

        public int WindowHeight
        {
            get => Mathf.RoundToInt(consoleCamera.ConsoleBottom / characterHeight);
            set
            {
                if (WindowHeight == value)
                {
                    return;
                }
                consoleCamera.ConsoleBottom = value * characterHeight;
                UpdateCamera();
            }
        }


        public Color ForegroundColor
        {
            get => currentForegroundColor;
            set
            {
                currentForegroundColor = value;
                bodyText += $"<color=#{ColorUtility.ToHtmlStringRGBA(currentForegroundColor)}>";
            }
        }
        
        public Color BackgroundColor
        {
            get => currentBackgroundColor;
            set
            {
                currentBackgroundColor = value;
                backgroundText += $"<mark=#{ColorUtility.ToHtmlStringRGBA(currentBackgroundColor)}>";
                backgroundText += $"<color=#{ColorUtility.ToHtmlStringRGBA(currentBackgroundColor)}>";
            }
        }
        
        public bool CenterContentX
        {
            get => centerContentX;
            set
            {
                centerContentX = value;
                UpdateCamera();
            }
        }
        
        public bool CenterContentY
        {
            get => centerContentY;
            set
            {
                centerContentY = value;
                UpdateCamera();
            }
        }
        
        public float BorderSize
        {
            get => borderSize;
            set
            {
                borderSize = value;
                UpdateCamera();
            }
        }

        private bool cursorVisible = true;
        public bool CursorVisible
        {
            get => cursorVisible;
            set
            {
                cursorVisible = value;
                cursor = cursorVisible ? visibleCursor : invisibleCursor;
            }
        }


        public static UnityConsole Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                
                // Initialize in Awake to ensure that the console is ready to use in Start
                consoleText.text = "A";
                consoleText.ForceMeshUpdate();
                characterHeight = consoleText.GetRenderedValues().y;
                consoleText.text = "";
                bodyText = $"<mspace={CharacterSpacing}>";
                backgroundText = $"<mspace={CharacterSpacing}>";
                BackgroundColor = Color.clear;
                ForegroundColor = defaultForegroundColor;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            WindowWidth = defaultWindowWidth;
            WindowHeight = defaultWindowHeight;
            
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
            string text = value.Replace("\n", "<br>").Replace("\r", "");
            bodyText += text;

            
            
            char[] chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] != ' ' && chars[i] != '\n')
                {
                    chars[i] = '_';
                }
            }
            backgroundText += new string(chars);;
            needsUpdate = true;
        }

        public void Write(char value)
        {
            if (value == '\r')
            {
                return;
            } 
            if (value == '\n')
            {
                bodyText += "<br>";
                backgroundText += "<br>";
            }
            else
            {
                bodyText += value;
                if (value == ' ')
                {
                    backgroundText += " ";
                }
                else
                {
                    backgroundText += "_";
                }
            }
            needsUpdate = true;
        }

        public void WriteLine(string value)
        {
            Write(value + "\n");
        }

        public void WriteLine()
        {
            Write('\n');
        }

        public async UniTask<string> ReadLine()
        {
            isGettingLine = true;
            isBlinkCursorActive = true;
            await Console.WaitUntil(() => !isGettingLine);
            isBlinkCursorActive = false;
            if (cursorVisible)
            {
                cursor = visibleCursor;
            }
            string result = inputText;
            inputText = "";
            return result;
        }

        public async UniTask<KeyCode> ReadKey()
        {
            isGettingKey = true;
            isBlinkCursorActive = true;
            await Console.WaitUntil(() => !isGettingKey);
            isBlinkCursorActive = false;

            if ((int)KeyCode.A <= (int)keyCode && (int)keyCode <= (int)KeyCode.Z)
            {
                Write(keyCode.ToString());
            }

            if (cursorVisible)
            {
                cursor = visibleCursor;
            }
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
            ForegroundColor = defaultForegroundColor;
            BackgroundColor = Color.clear;
        }
        

        private IEnumerator BlinkCursor()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.5f);
                if (!cursorVisible)
                {
                    continue;
                }
                if (!isBlinkCursorActive)
                {
                    cursor = visibleCursor;
                    needsUpdate = true;
                    continue;
                }

                cursor = cursor == invisibleCursor ? visibleCursor : invisibleCursor;
                needsUpdate = true;
            }
        }

        private void UpdateConsoleText()
        {
            consoleText.text = bodyText + inputText + cursor;
            consoleBackground.text = backgroundText;
        }

        private void UpdateCamera()
        {
            float cameraSize;
            if (WindowHeight * characterHeight * consoleCamera.AspectRatio > WindowWidth * CharacterSpacing + 2 * borderSize) // Height is the limiting factor
            {
                cameraSize = WindowHeight * characterHeight / 2;
            }
            else // Width is the limiting factor
            {
                cameraSize = (WindowWidth * CharacterSpacing + 2 * borderSize) / consoleCamera.AspectRatio / 2;
            }
            consoleCamera.SetCameraSize(cameraSize);

            if (centerContentX)
            {
                consoleRectTransform.position = new Vector3(0, 0, 0);
            }
            else
            {
                consoleRectTransform.position =
                    new Vector3(
                        borderSize - consoleCamera.AspectRatio * cameraSize + consoleRectTransform.sizeDelta.x / 2,
                        0, 0);
            }
            
            if (centerContentY)
            {
                consoleRectTransform.position = new Vector3(consoleRectTransform.position.x, WindowHeight * characterHeight / 2, 0);
            }
            else
            {
                consoleRectTransform.position =
                    new Vector3(
                        consoleRectTransform.position.x,
                        cameraSize, 0);
            }
        }
    }
}