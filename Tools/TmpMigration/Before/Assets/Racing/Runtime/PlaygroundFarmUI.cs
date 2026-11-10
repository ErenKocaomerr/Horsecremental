using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    [DefaultExecutionOrder(100)]
    public sealed class PlaygroundFarmUI : MonoBehaviour
    {
        [SerializeField] private GameplayWorld _world;
        [SerializeField] private Button _feed;
        [SerializeField] private Text _label;
        private PlayerProgression _player;
        private void Start()
        {
            _player = _world.Player;
            _player.Changed += Refresh;
            _world.Changed += Refresh;
            _feed.onClick.AddListener(Feed);
            Refresh();
        }
        private void Feed()
        {
            if (_world.IsManagement && !_world.ViewingRace) _player.TryFeed(_world.Settings);
        }
        private void Refresh()
        {
            _feed.interactable = _world.IsManagement && !_world.ViewingRace && _player.Carrots >= _world.Settings.FeedCost;
            _label.text = "Feed horse  " + _world.Settings.FeedCost + " Carrots\n+" + _world.Settings.FeedSpeed.ToString("0.##") + " Speed";
        }
        private void OnDestroy()
        {
            if (_player == null) return;
            _player.Changed -= Refresh;
            _world.Changed -= Refresh;
            _feed.onClick.RemoveListener(Feed);
        }
    }
}

