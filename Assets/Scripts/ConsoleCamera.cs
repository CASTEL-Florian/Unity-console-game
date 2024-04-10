using TMPro;
using UnityEngine;

namespace UnityConsole
{
    public class ConsoleCamera : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI consoleText;
        [SerializeField] private Transform textMask;

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
            textMask.localScale = new Vector3(2 * cam.aspect * size, size, 1f);
        }

        private void Update()
        {
            Vector2 renderedValues = consoleText.GetRenderedValues();
            Vector3 position = transform.position;
            if (renderedValues.y < ConsoleBottom)
            {
                transform.position = new Vector3(position.x, 0, position.z);
                textMask.gameObject.SetActive(false);
                return;
            }
            textMask.gameObject.SetActive(true);
            
            textMask.localPosition = new Vector3(0, ConsoleBottom / 2 + textMask.localScale.y / 2 , 10);
            transform.position = new Vector3(position.x, ConsoleBottom - renderedValues.y, position.z);
        }
    }
}