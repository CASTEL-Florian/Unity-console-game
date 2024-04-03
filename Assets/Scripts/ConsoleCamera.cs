using TMPro;
using UnityEngine;

namespace UnityConsole
{
    public class ConsoleCamera : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI consoleText;
        [SerializeField] private float consoleBottom = 7;

        private void Update()
        {
            Vector2 renderedValues = consoleText.GetRenderedValues();
            Vector3 position = transform.position;
            if (renderedValues.y < consoleBottom)
            {
                transform.position = new Vector3(position.x, 0, position.z);
                return;
            }

            transform.position = new Vector3(position.x, consoleBottom - renderedValues.y, position.z);
        }
    }
}