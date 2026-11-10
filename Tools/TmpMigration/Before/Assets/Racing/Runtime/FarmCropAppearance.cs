using UnityEngine;

namespace Horsecremental.Racing
{
    // Crop art follows the model's events; interaction and growth remain in FarmWorldView.
    public sealed class FarmCropAppearance : MonoBehaviour
    {
        [SerializeField] private GameplayWorld _world;
        [SerializeField] private SpriteRenderer[] _plots;
        [SerializeField] private Sprite[] _stages;
        private PlayerProgression _player;

        private void OnEnable()
        {
            _world.Changed += BindPlayer;
            BindPlayer();
        }

        private void Start() => BindPlayer();

        private void BindPlayer()
        {
            if (_world.Player == null || _player == _world.Player) return;
            if (_player != null) _player.Crops.StageChanged -= RefreshPlot;
            _player = _world.Player;
            _player.Crops.StageChanged += RefreshPlot;
            for (int i = 0; i < _plots.Length; i++) RefreshPlot(i);
        }

        private void RefreshPlot(int index)
        {
            int stage = (int)_player.Crops.Stage(index);
            _plots[index].sprite = _stages[Mathf.Clamp(stage, 0, _stages.Length - 1)];
        }

        private void OnDisable()
        {
            _world.Changed -= BindPlayer;
            if (_player != null) _player.Crops.StageChanged -= RefreshPlot;
            _player = null;
        }
    }
}
