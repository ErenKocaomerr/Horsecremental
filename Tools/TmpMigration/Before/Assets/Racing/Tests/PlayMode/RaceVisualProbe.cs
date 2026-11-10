using UnityEngine;

namespace Horsecremental.Racing.Tests
{
    [DefaultExecutionOrder(300)]
    public sealed class RaceVisualProbe : MonoBehaviour
    {
        private HorseView _view;
        private HorseSimulation _horse;
        private Transform _sprite;
        private float _previousX;
        private bool _hasRenderedSample;
        public int Samples { get; private set; }
        public int BackwardsSteps { get; private set; }
        public float MaximumError { get; private set; }

        public void Initialize(HorseView view, HorseSimulation horse)
        {
            _view = view;
            _horse = horse;
            _sprite = view.RenderRoot.Find("Horse Sprite");
            _previousX = view.RenderRoot.position.x;
        }

        private void LateUpdate()
        {
            if (_view == null) return;
            float fraction = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            float expected = Mathf.Lerp(_horse.PreviousDistance, _horse.Distance, fraction);
            float actual = _view.RenderRoot.position.x;
            MaximumError = Mathf.Max(MaximumError, Mathf.Abs(actual - expected));
            float expectedHeight = Mathf.Lerp(_horse.PreviousHeight, _horse.Height, fraction);
            MaximumError = Mathf.Max(MaximumError, Mathf.Abs(_sprite.localPosition.y - expectedHeight));
            // Compare consecutive rendered frames, not an inherited transform before its first LateUpdate.
            if (_hasRenderedSample && actual < _previousX - 0.0001f) BackwardsSteps++;
            _hasRenderedSample = true;
            _previousX = actual;
            Samples++;
        }
    }
}
