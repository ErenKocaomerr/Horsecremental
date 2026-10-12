using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    public sealed class BlacksmithUI : MonoBehaviour
    {
        [SerializeField] private GameplayWorld _world;
        [SerializeField] private ProgressionSettings _settings;
        [SerializeField] private Text[] _cards;
        [SerializeField] private Button[] _crafts;
        [SerializeField] private Text _status;
        [SerializeField] private Text _feedback;
        [SerializeField] private Image _fill;
        private PlayerProgression _player;
        private readonly Text[] _craftLabels = new Text[3];
        private void Awake()
        {
            _player = ProgressionSession.GetOrCreate(_settings);
            for (int i = 0; i < 3; i++) _craftLabels[i] = _crafts[i].GetComponentInChildren<Text>();
        }
        private void OnEnable()
        {
            _crafts[0].onClick.AddListener(Shoes); _crafts[1].onClick.AddListener(Saddle); _crafts[2].onClick.AddListener(Bridle);
            _player.Changed += Refresh; _player.CraftCompleted += Complete;
            InvokeRepeating(nameof(RefreshTimer), 0f, 0.1f); Refresh();
        }
        private void OnDisable()
        {
            _crafts[0].onClick.RemoveListener(Shoes); _crafts[1].onClick.RemoveListener(Saddle); _crafts[2].onClick.RemoveListener(Bridle);
            _player.Changed -= Refresh; _player.CraftCompleted -= Complete; CancelInvoke();
        }
        private void Shoes() => Craft(EquipmentType.Horseshoes);
        private void Saddle() => Craft(EquipmentType.Saddle);
        private void Bridle() => Craft(EquipmentType.Bridle);
        private void Craft(EquipmentType type)
        {
            if (!_world.IsManagement || _world.Area != ManagementArea.Blacksmith) return;
            if (_player.IsCrafting) { _feedback.text = "BLACKSMITH BUSY — " + _player.ActiveCraft.Name; return; }
            if (!_player.HasNextTier(type)) return;
            EquipmentTier next = _player.NextEquipment(type);
            if (!_player.TryStartCraft(type))
            {
                _feedback.text = "Missing: " + Mathf.Max(0, next.Coins - _player.Coins) + " Coins / " +
                    Mathf.Max(0, _player.CraftMetalCost(type) - _player.Metal) + " Metal";
                _feedback.transform.localScale = Vector3.one * 1.08f;
                return;
            }
            _feedback.text = "Crafting " + next.Name;
        }
        private void Refresh()
        {
            for (int i = 0; i < 3; i++)
            {
                EquipmentType type = (EquipmentType)i;
                bool nextAvailable = _player.HasNextTier(type);
                _cards[i].text = type + "\nTier " + _player.EquipmentLevel(type);
                _crafts[i].interactable = nextAvailable;
                _craftLabels[i].text = _player.IsCrafting ? "BUSY" : nextAvailable ? "CRAFT" : "MAX";
            }
            RefreshTimer();
        }
        private void Complete(EquipmentType type) => _feedback.text = _player.Equipment(type).Name + " equipped!";
        private void RefreshTimer() => _status.text = _player.IsCrafting ? _player.ActiveCraft.Name + " • " +
            _player.CraftRemaining.ToString("0.0") + "s" : "FORGE READY";
        private void Update()
        {
            _fill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 780f * _player.CraftProgress);
            _feedback.transform.localScale = Vector3.Lerp(_feedback.transform.localScale, Vector3.one, 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime));
        }
    }
}
