using System;
using System.Collections;
using System.Text;
using System.Threading;
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
        [SerializeField] private Vector2Int defaultWindowSize = new Vector2Int(960, 540);
        [SerializeField] bool fixedAspectRatio = true;
        [SerializeField] bool isInputBufferActive = false;

        [SerializeField] private BeepPlayer beepPlayer;
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
        private const float CharacterHeight = 0.36f;

        private float pixelsPerUnit;

        private readonly Queue inputBuffer = new();

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
                    new Vector2((value + 0.3f) * CharacterSpacing, consoleRectTransform.sizeDelta.y);
                UpdateCamera();
            }
        }

        public int WindowHeight
        {
            get => Mathf.RoundToInt(consoleCamera.ConsoleBottom / CharacterHeight);
            set
            {
                if (WindowHeight == value)
                {
                    return;
                }

                consoleCamera.ConsoleBottom = value * CharacterHeight;
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
                backgroundText += $"<color=#{ColorUtility.ToHtmlStringRGBA(currentBackgroundColor)}>";
                backgroundText += $"<mark=#{ColorUtility.ToHtmlStringRGBA(currentBackgroundColor)}>";
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
                if (cursorVisible)
                {
                    cursor = visibleCursor;
                }
                else
                {
                    cursor = "";
                }
            }
        }

        public bool KeyAvailable =>
            inputBuffer.Count > 0 && isInputBufferActive || Input.anyKeyDown && !isInputBufferActive;

        public bool InputBufferActive
        {
            get => isInputBufferActive;
            set
            {
                isInputBufferActive = value;
                inputBuffer.Clear();
            }
        }

        public float PixelsPerUnit
        {
            get => pixelsPerUnit;
            set
            {
                if (pixelsPerUnit < 1)
                {
                    pixelsPerUnit = 1;
                }

                pixelsPerUnit = value;
                UpdateCamera();
            }
        }


        public static UnityConsole Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                // Initialize in Awake to ensure that the console is ready to use in Start

                bodyText = FormattableString.Invariant($"<mspace={CharacterSpacing}>");
                bodyText += FormattableString.Invariant($"<line-height={CharacterHeight}>");
                backgroundText = FormattableString.Invariant($"<mspace={CharacterSpacing}>");
                backgroundText += FormattableString.Invariant($"<line-height={CharacterHeight}>");

                BackgroundColor = Color.clear;
                ForegroundColor = defaultForegroundColor;

                pixelsPerUnit = defaultWindowSize.y / (2 * consoleCamera.Size);

                if (!fixedAspectRatio && Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    Screen.SetResolution(defaultWindowSize.x, defaultWindowSize.y, FullScreenMode.Windowed);
                    consoleCamera.AspectRatio = (float)defaultWindowSize.x / defaultWindowSize.y;
                }

                WindowWidth = defaultWindowWidth;
                WindowHeight = defaultWindowHeight;
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

            if (isInputBufferActive && Input.anyKeyDown)
            {
                if (Input.inputString.Length > 0)
                {
                    inputBuffer.Enqueue(Input.inputString);
                }
            }

            if (!isGettingKey && !isGettingLine)
            {
                return;
            }


            if (isGettingKey)
            {
                if (inputBuffer.Count > 0)
                {
                    keyCode = (KeyCode)((string)inputBuffer.Dequeue())[0];
                    isGettingKey = false;
                    return;
                }

                if (!Input.anyKeyDown) return;
                keyCode = Input.inputString.Length >= 1 ? (KeyCode)Input.inputString[0] : KeyCode.None;
                if (keyCode != KeyCode.None)
                {
                    isGettingKey = false;
                }
            }
            else
            {
                HandleInput();
            }
        }

        private void HandleInput()
        {
            string input;
            if (inputBuffer.Count > 0)
            {
                input = (string)inputBuffer.Dequeue();
            }
            else
            {
                input = Input.inputString;
            }

            foreach (char c in input)
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
            string text = value.Replace("\r", "");
            bodyText += text.Replace("\n", "<br>");


            char[] chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if ((chars[i] != ' ') && chars[i] != '\n')
                {
                    chars[i] = '_';
                }
            }

            backgroundText += new string(chars);
            needsUpdate = true;
        }

        public void Write(char value)
        {
            Write(value.ToString());
        }

        public void WriteLine(string value)
        {
            Write(value + "\n");
        }

        public void WriteLine()
        {
            Write('\n');
        }

        public async UniTask<string> ReadLine(CancellationToken cancellationToken = default)
        {
            isGettingLine = true;
            isBlinkCursorActive = true;
            await Console.WaitUntil(() => !isGettingLine, cancellationToken: cancellationToken);
            isBlinkCursorActive = false;
            if (cursorVisible)
            {
                cursor = visibleCursor;
            }

            string result = inputText;
            inputText = "";
            return result;
        }

        public async UniTask<KeyCode> ReadKey(bool intercept = false, CancellationToken cancellationToken = default)
        {
            isGettingKey = true;
            isBlinkCursorActive = true;
            await Console.WaitUntil(() => !isGettingKey, cancellationToken: cancellationToken);
            isBlinkCursorActive = false;

            if ((int)KeyCode.A <= (int)keyCode && (int)keyCode <= (int)KeyCode.Z) // If the key is a letter.
            {
                if (!intercept)
                {
                    Write(keyCode.ToString());
                }
            }

            if (cursorVisible)
            {
                cursor = visibleCursor;
            }

            return keyCode;
        }

        public void Clear()
        {
            bodyText = FormattableString.Invariant(
                $"<mspace={CharacterSpacing}><color=#{ColorUtility.ToHtmlStringRGBA(currentForegroundColor)}>");
            backgroundText = FormattableString.Invariant(
                $"<mspace={CharacterSpacing}><color=#{ColorUtility.ToHtmlStringRGBA(currentBackgroundColor)}>");
            backgroundText +=
                FormattableString.Invariant($"<mark=#{ColorUtility.ToHtmlStringRGBA(currentBackgroundColor)}>");
            bodyText += FormattableString.Invariant($"<line-height={CharacterHeight}>");
            backgroundText += FormattableString.Invariant($"<line-height={CharacterHeight}>");
            inputText = "";
            needsUpdate = true;
        }

        public void ResetColor()
        {
            ForegroundColor = defaultForegroundColor;
            BackgroundColor = Color.clear;
        }

        public async UniTask Beep(int frequency = 800, int duration = 200,
            CancellationToken cancellationToken = default)
        {
            await beepPlayer.Beep(frequency, duration, cancellationToken);
        }


        private IEnumerator BlinkCursor()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.5f);
                if (!cursorVisible)
                {
                    cursor = "";
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
            StringBuilder sb = new StringBuilder();
            sb.Append(bodyText);
            sb.Append(inputText);
            sb.Append(cursor);
            consoleText.text = sb.ToString();
            consoleBackground.text = backgroundText;
        }

        private void UpdateCamera()
        {
            if (WindowHeight < 1 || WindowWidth < 1)
            {
                return;
            }

            if (!fixedAspectRatio && Application.platform != RuntimePlatform.WebGLPlayer)
            {
                Screen.SetResolution((int)(((WindowWidth + 0.3f) * CharacterSpacing + 2 * borderSize) * pixelsPerUnit),
                    (int)(WindowHeight * CharacterHeight * pixelsPerUnit), FullScreenMode.Windowed);
                consoleCamera.AspectRatio =
                    (WindowWidth * CharacterSpacing + 2 * borderSize) / (WindowHeight * CharacterHeight);
            }

            float cameraSize;
            if (WindowHeight * CharacterHeight * consoleCamera.AspectRatio >
                (WindowWidth + 0.3f) * CharacterSpacing + 2 * borderSize) // Height is the limiting factor
            {
                cameraSize = WindowHeight * CharacterHeight / 2;
            }
            else // Width is the limiting factor
            {
                cameraSize = ((WindowWidth + 0.3f) * CharacterSpacing + 2 * borderSize) / consoleCamera.AspectRatio / 2;
            }

            consoleCamera.SetCameraSize(cameraSize);


            consoleRectTransform.position =
                new Vector3(
                    borderSize - consoleCamera.AspectRatio * cameraSize + consoleRectTransform.sizeDelta.x / 2,
                    0, 0);


            consoleRectTransform.position =
                new Vector3(
                    consoleRectTransform.position.x,
                    cameraSize, 0);
        }
    }
}