using UnityEngine;

namespace Horsecremental.Racing
{
    public sealed class FarmWorldView : MonoBehaviour
    {
        [SerializeField] private Transform[] _plants;
        [SerializeField] private SpriteRenderer[] _halos;
        [SerializeField] private WorldFeedback _feedback;
        [SerializeField] private WorldPickupManager _pickups;
        [SerializeField] private StableAudioHooks _audio;
        private PlayerProgression _player;
        private float[] _punch;
        private float[] _cooldowns;
        private IPlaygroundSettings _settings;
        public int PlotCount => _plants.Length;
        public Vector3 PlotPosition(int index) => _plants[index].parent.position;
        public void Initialize(PlayerProgression player, IPlaygroundSettings settings)
        {
            _player = player;
            _settings = settings;
            _punch = new float[_plants.Length];
            _cooldowns = new float[_plants.Length];
            player.Crops.StageChanged += Stage;
            player.CropHarvested += Harvest;
            int oldStored = player.TakeLegacyStoredCarrots();
            if (oldStored > 0) _pickups.Spawn(PlotPosition(0), oldStored, PickupResource.Carrots);
        }
        public bool Click(int plot)
        {
            if (plot < 0 || plot >= _plants.Length || _cooldowns[plot] > 0f) return false;
            _cooldowns[plot] = _settings.ClickCooldown;
            if (_player.HarvestCrop(plot)) return true;
            if (!_player.GrowCrop(plot)) return false;
            _punch[plot] = 1f;
            _audio.Play(StableSound.CropClick);
            _feedback.Show(PlotPosition(plot) + Vector3.up * 0.7f, "+Growth", new Color(0.7f, 1f, 0.55f));
            return true;
        }
        private void Stage(int plot)
        {
            _punch[plot] = 1f;
            _audio.Play(StableSound.GrowthStage);
        }
        private void Harvest(int plot, int yield)
        {
            _pickups.Spawn(PlotPosition(plot) + Vector3.up * 0.4f, yield, PickupResource.Carrots);
            _audio.Play(StableSound.Harvest);
        }
        public int HitPlot(Vector2 point)
        {
            for (int i = 0; i < _plants.Length; i++)
            {
                Vector3 position = PlotPosition(i);
                if (Mathf.Abs(point.x - position.x) < 0.7f && Mathf.Abs(point.y - position.y - 0.2f) < 0.8f) return i;
            }
            return -1;
        }
        public void Highlight(int plot)
        {
            for (int i = 0; i < _halos.Length; i++)
                _halos[i].color = i == plot ? new Color(1f, 0.94f, 0.45f, 0.6f) :
                    _player.Crops.Stage(i) == CropStage.Ready ? new Color(0.7f, 1f, 0.4f, 0.4f) : new Color(0.2f, 0.3f, 0.1f, 0.15f);
        }
        private void Update()
        {
            if (_player == null) return;
            for (int i = 0; i < _plants.Length; i++)
            {
                _cooldowns[i] = Mathf.Max(0f, _cooldowns[i] - Time.deltaTime);
                _punch[i] = Mathf.Max(0f, _punch[i] - Time.deltaTime * 4f);
                CropStage stage = _player.Crops.Stage(i);
                float scale = stage == CropStage.Sprout ? 0.32f : stage == CropStage.Mid ? 0.58f : stage == CropStage.Large ? 0.85f : 1.08f;
                float bounce = stage == CropStage.Ready ? Mathf.Sin(Time.time * 3f + i) * 0.05f : 0f;
                float punch = Mathf.Sin(_punch[i] * Mathf.PI) * 0.16f;
                _plants[i].localScale = new Vector3(scale * (1f + punch), scale * (1f - punch * 0.5f), 1f);
                _plants[i].localPosition = Vector3.up * (0.1f + bounce);
            }
        }
        private void OnDestroy()
        {
            if (_player == null) return;
            _player.Crops.StageChanged -= Stage;
            _player.CropHarvested -= Harvest;
        }
    }
}

