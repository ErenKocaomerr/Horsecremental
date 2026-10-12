using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    public sealed class RaceUI : MonoBehaviour
    {
        [SerializeField] private Text _position;
        [SerializeField] private Text _boost;
        [SerializeField] private Text _countdown;
        [SerializeField] private Text _result;
        [SerializeField] private Text _feedback;
        [SerializeField] private GameObject _entryPanel;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private Image _boostFill;
        [SerializeField] private Button _start;
        [SerializeField] private Button _restart;
        [SerializeField] private Button _returnToStable;
        [SerializeField] private Text _reward;
        [SerializeField] private RaceResultRow[] _resultRows;
        private RaceManager _race;
        private float _feedbackRemaining;
        private string[] _positionLabels;
        private RaceParticipant[] _orderedResults;
        private bool _bound;
        private RaceRewards _rewards;
        private PlayerProgression _player;
        [SerializeField] private GameplayWorld _world;
        private float _displayedBoost;
        private float _boostPulse;
        private float _countdownPulse;

        public void Initialize(RaceManager race, RaceRewards rewards, PlayerProgression player)
        {
            Unbind();
            _race = race;
            _rewards = rewards;
            _player = player;
            _positionLabels = new string[race.ParticipantCount + 1];
            _orderedResults = new RaceParticipant[race.ParticipantCount];
            for (int i = 1; i < _positionLabels.Length; i++)
                _positionLabels[i] = "Place: " + i + " / " + race.ParticipantCount;
            Bind();
            RefreshState();
        }

        private void OnEnable() { Bind(); if (_race != null) RefreshState(); }
        private void OnDisable() => Unbind();

        private void Bind()
        {
            if (_race == null || _bound || !isActiveAndEnabled) return;
            _bound = true;
            _start.onClick.AddListener(StartMainRace);
            _restart.onClick.AddListener(StartMainRace);
            _returnToStable.onClick.AddListener(GameScenes.ReturnToStable);
            _rewards.Awarded += RefreshState;
            _race.Changed += RefreshState;
            _race.HudChanged += RefreshHud;
            _race.ParticipantFinished += OnParticipantFinished;
            _race.Player.Movement.BoostActivated += OnBoost;
            _race.Player.Movement.ObstacleHit += OnObstacleHit;
        }

        private void Unbind()
        {
            if (!_bound) return;
            _bound = false;
            _start.onClick.RemoveListener(StartMainRace);
            _restart.onClick.RemoveListener(StartMainRace);
            _returnToStable.onClick.RemoveListener(GameScenes.ReturnToStable);
            _rewards.Awarded -= RefreshState;
            _race.Changed -= RefreshState;
            _race.HudChanged -= RefreshHud;
            _race.ParticipantFinished -= OnParticipantFinished;
            _race.Player.Movement.BoostActivated -= OnBoost;
            _race.Player.Movement.ObstacleHit -= OnObstacleHit;
        }

        private void StartMainRace() { if (_world != null) _world.StartMainRace(); else _race.StartRace(); }
        private void OnBoost() { _boostPulse = 0.2f; ShowFeedback("BOOST!", new Color(1f, 0.85f, 0.2f)); }
        private void OnObstacleHit() => ShowFeedback("OBSTACLE HIT — SPEED LOST", new Color(1f, 0.45f, 0.3f));

        private void ShowFeedback(string message, Color color)
        {
            _feedback.text = message;
            _feedback.color = color;
            _feedbackRemaining = 1.2f;
        }

        private void OnParticipantFinished(RaceParticipant participant)
        {
            if (_race.Player.HasFinished) RefreshLeaderboard();
        }

        private void RefreshState()
        {
            _entryPanel.SetActive(_race.Phase == RacePhase.Ready && (_world == null || _world.IsManagement));
            bool showResult = _race.Player.HasFinished;
            _resultPanel.SetActive(showResult);
            _countdown.text = showResult ? string.Empty : _race.CountdownText;
            _countdownPulse = 0.3f;
            if (showResult) RefreshLeaderboard();
            if (_race.Phase == RacePhase.Countdown || showResult)
            {
                _feedbackRemaining = 0f;
                _feedback.text = string.Empty;
            }
            if (_race.Phase == RacePhase.Countdown)
            {
                for (int i = 0; i < _resultRows.Length; i++) _resultRows[i].Clear();
            }
            RefreshHud();
        }

        private void RefreshLeaderboard()
        {
            _result.text = _race.Player.FinishPosition == 1 ? "1ST PLACE!" : "PLACE " + _race.Player.FinishPosition + " — KEEP TRAINING!";
            _reward.text = "Place: " + _race.Player.FinishPosition + " / " + _race.ParticipantCount +
                "   |   Reward: +" + _rewards.EarnedCoins + " Coins\nTotal Coins: " + _player.Coins;
            for (int i = 0; i < _race.Participants.Count; i++)
            {
                RaceParticipant participant = _race.Participants[i];
                if (participant.HasFinished) _orderedResults[participant.FinishPosition - 1] = participant;
            }
            int pendingIndex = _race.FinishedCount;
            for (int i = 0; i < _race.Participants.Count; i++)
                if (!_race.Participants[i].HasFinished) _orderedResults[pendingIndex++] = _race.Participants[i];
            for (int i = 0; i < _orderedResults.Length; i++) _resultRows[i].Show(_orderedResults[i]);
        }

        private void RefreshHud()
        {
            HorseSimulation horse = _race.Player.Movement.Simulation;
            _position.text = _positionLabels[_race.GetPlayerPosition()];
            _boost.text = _race.Player.HasFinished ? "BOOST — RACE FINISHED" : "SPACE — RHYTHMIC BOOST TAPS";
        }

        private void Update()
        {
            if (_race == null) return;
            float dt = Time.unscaledDeltaTime;
            _displayedBoost = Mathf.Lerp(_displayedBoost, _race.Player.Movement.Simulation.BoostEnergy, 1f - Mathf.Exp(-16f * dt));
            _boostFill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 320f * _displayedBoost);
            _boostPulse = Mathf.Max(0f, _boostPulse - dt);
            float glow = _displayedBoost > 0.75f ? (1f + Mathf.Sin(Time.unscaledTime * 10f)) * 0.04f : 0f;
            _boostFill.rectTransform.localScale = new Vector3(1f, 1f + _boostPulse * 1.5f + glow, 1f);
            _boostFill.color = Color.Lerp(new Color(1f, 0.78f, 0.2f), new Color(1f, 0.35f, 0.12f), _displayedBoost);
            _countdownPulse = Mathf.Max(0f, _countdownPulse - dt);
            _countdown.transform.localScale = Vector3.one * (1f + _countdownPulse * 0.6f);
            if (_feedbackRemaining <= 0f) return;
            _feedbackRemaining -= dt;
            if (_feedbackRemaining <= 0f) _feedback.text = string.Empty;
        }
    }
}
