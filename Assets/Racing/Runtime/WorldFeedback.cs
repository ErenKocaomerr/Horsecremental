using UnityEngine;

namespace Horsecremental.Racing
{
    public sealed class WorldFeedback : MonoBehaviour
    {
        [SerializeField] private TextMesh[] _labels;
        private float[] _remaining;
        private Vector3[] _origins;
        private int _next;
        private void Awake()
        {
            _remaining = new float[_labels.Length];
            _origins = new Vector3[_labels.Length];
            for (int i = 0; i < _labels.Length; i++) _labels[i].gameObject.SetActive(false);
        }
        public void Show(Vector3 position, string message, Color color)
        {
            int index = _next++ % _labels.Length;
            _origins[index] = position;
            _remaining[index] = 1.2f;
            TextMesh text = _labels[index];
            text.text = message;
            text.color = color;
            text.transform.position = position;
            text.gameObject.SetActive(true);
        }
        private void Update()
        {
            for (int i = 0; i < _labels.Length; i++)
            {
                if (_remaining[i] <= 0f) continue;
                _remaining[i] = Mathf.Max(0f, _remaining[i] - Time.deltaTime);
                if (_remaining[i] == 0f) { _labels[i].gameObject.SetActive(false); continue; }
                _labels[i].transform.position = _origins[i] + Vector3.up * (1.2f - _remaining[i]) * 0.7f;
                Color color = _labels[i].color;
                color.a = Mathf.Clamp01(_remaining[i] * 2f);
                _labels[i].color = color;
            }
        }
    }
}

