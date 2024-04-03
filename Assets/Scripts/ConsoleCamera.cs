using TMPro;
using UnityEngine;

public class ConsoleCamera : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI consoleText;
    [SerializeField] private float consoleBottom = 7;

    private void Update()
    {
        Vector2 renderedValues = consoleText.GetRenderedValues();
        if (renderedValues.y < consoleBottom)
        {
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            return;
        }
        
        transform.position = new Vector3(transform.position.x, consoleBottom - renderedValues.y, transform.position.z);
    }
}
