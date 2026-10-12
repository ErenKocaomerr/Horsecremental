using Unity.Cinemachine;
using UnityEngine;

namespace Horsecremental.Racing
{
    public sealed class WorldCameraNavigation : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _stable;
        [SerializeField] private CinemachineCamera _entry;
        [SerializeField] private CinemachineCamera _race;
        [SerializeField] private CinemachineCamera _farm;
        [SerializeField] private CinemachineCamera _blacksmith;
        [SerializeField] private CinemachineBrain _brain;
        private HorseMovement _horse;
        private bool _racing;
        private float _lensSize;
        private float _kick;
        public void Initialize(HorseMovement horse, float blend)
        {
            _horse = horse;
            _brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, blend);
            _lensSize = _race.Lens.OrthographicSize;
            horse.BoostActivated += OnBoost;
            horse.ObstacleHit += OnHit;
            ShowStable();
        }
        public void ShowStable() => Select(_stable, false);
        public void ShowEntry() => Select(_entry, false);
        public void ShowFarm() => Select(_farm, false);
        public void ShowBlacksmith() => Select(_blacksmith, false);
        public void ShowRace() { _race.PreviousStateIsValid = false; Select(_race, true); }
        private void Select(CinemachineCamera camera, bool racing)
        {
            _racing = racing;
            _stable.Priority.Value = camera == _stable ? 20 : 0;
            _entry.Priority.Value = camera == _entry ? 20 : 0;
            _race.Priority.Value = camera == _race ? 20 : 0;
            if (_farm != null) _farm.Priority.Value = camera == _farm ? 20 : 0;
            if (_blacksmith != null) _blacksmith.Priority.Value = camera == _blacksmith ? 20 : 0;
        }
        private void OnBoost() => _kick = 0.12f;
        private void OnHit() => _kick = -0.15f;
        private void LateUpdate()
        {
            if (_horse == null || !_racing) return;
            _kick = Mathf.MoveTowards(_kick, 0f, Time.deltaTime);
            LensSettings lens = _race.Lens;
            float speedFraction = Mathf.Clamp01(_horse.Simulation.Speed / 24f);
            lens.OrthographicSize = Mathf.Lerp(lens.OrthographicSize, _lensSize + speedFraction * 0.35f + _kick,
                1f - Mathf.Exp(-5f * Time.deltaTime));
            _race.Lens = lens;
        }
        private void OnDestroy()
        {
            if (_horse == null) return;
            _horse.BoostActivated -= OnBoost;
            _horse.ObstacleHit -= OnHit;
        }
    }
}
