using TMPro;
using UnityEngine;

namespace UnityConsole
{
    public class ConsoleCamera : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI consoleText;
        [SerializeField] private Camera cam;        
        public float AspectRatio
        {
            get => cam.aspect;
            set => cam.aspect = value;
        }

        public float ConsoleBottom { get; set; }
        
        public float Size => cam.orthographicSize;
        
        public void SetCameraSize(float size)
        {
            cam.orthographicSize = size;
        }

        private void Update()
        {
            Vector2 renderedValues = consoleText.GetRenderedValues();
            Vector3 position = transform.position;
            if (renderedValues.y < ConsoleBottom)
            {
                transform.position = new Vector3(position.x, 0, position.z);
                return;
            }
            transform.position = new Vector3(position.x, ConsoleBottom - renderedValues.y, position.z);
        }
    }
}