using System;
using System.Collections.Generic;
using UnityEngine;

namespace Horsecremental.Racing
{
    public enum RacePhase { Ready, Countdown, Racing, Finished }

    public sealed class RaceManager : MonoBehaviour
    {
        private RaceParticipant[] _participants;
        private RaceParticipant[] _finishCandidates;
        private float[] _crossingTimes;
        private float _trackLength;
        private float _countdownRemaining;
        private float _goRemaining;
        private float _hudRemaining;
        private int _countdownNumber;
        private int _finishedCount;

        public RacePhase Phase { get; private set; }
        public RaceParticipant Player { get; private set; }
        public float ElapsedTime { get; private set; }
        public int ParticipantCount => _participants.Length;
        public int FinishedCount => _finishedCount;
        public IReadOnlyList<RaceParticipant> Participants { get; private set; }
        public string CountdownText { get; private set; } = string.Empty;
        public event Action Changed;
        public event Action HudChanged;
        public event Action<RaceParticipant> ParticipantFinished;
        public event Action RaceStarted;

        public void Initialize(RaceParticipant[] participants, float trackLength)
        {
            _participants = participants ?? throw new ArgumentNullException(nameof(participants));
            Participants = Array.AsReadOnly(participants);
            _finishCandidates = new RaceParticipant[participants.Length];
            _crossingTimes = new float[participants.Length];
            _trackLength = trackLength;
            for (int i = 0; i < participants.Length; i++)
                if (participants[i].IsPlayer) Player = participants[i];
            if (Player == null) throw new ArgumentException("Race requires a player.", nameof(participants));
            Phase = RacePhase.Ready;
        }

        public void StartRace()
        {
            if (_participants == null || Phase == RacePhase.Countdown ||
                (Phase == RacePhase.Racing && !Player.HasFinished)) return;
            for (int i = 0; i < _participants.Length; i++) _participants[i].ResetParticipant();
            ElapsedTime = 0f;
            _finishedCount = 0;
            _countdownRemaining = 3f;
            _countdownNumber = 3;
            _goRemaining = _hudRemaining = 0f;
            CountdownText = "3";
            Phase = RacePhase.Countdown;
            RaceStarted?.Invoke();
            Changed?.Invoke();
            HudChanged?.Invoke();
        }

        public void PrepareRace()
        {
            for (int i = 0; i < _participants.Length; i++) _participants[i].ResetParticipant();
            Phase = RacePhase.Ready;
            _finishedCount = 0;
            ElapsedTime = 0f;
            CountdownText = string.Empty;
            Changed?.Invoke();
        }

        public int GetPlayerPosition()
        {
            if (Player.HasFinished) return Player.FinishPosition;
            int position = 1;
            for (int i = 0; i < _participants.Length; i++)
            {
                RaceParticipant opponent = _participants[i];
                if (opponent != Player && (opponent.HasFinished ||
                    opponent.Movement.Simulation.Distance > Player.Movement.Simulation.Distance)) position++;
            }
            return position;
        }

        private void FixedUpdate()
        {
            if (_participants == null) return;
            float deltaTime = Time.fixedDeltaTime;
            if (Phase == RacePhase.Countdown)
            {
                _countdownRemaining -= deltaTime;
                int number = Mathf.CeilToInt(_countdownRemaining);
                if (number <= 0)
                {
                    Phase = RacePhase.Racing;
                    CountdownText = "GO";
                    _goRemaining = 0.75f;
                    for (int i = 0; i < _participants.Length; i++) _participants[i].Movement.Simulation.Start();
                    Changed?.Invoke();
                }
                else if (number != _countdownNumber)
                {
                    _countdownNumber = number;
                    CountdownText = number == 2 ? "2" : "1";
                    Changed?.Invoke();
                }
                return;
            }

            if (Phase != RacePhase.Racing) return;
            int candidateCount = 0;
            for (int i = 0; i < _participants.Length; i++)
            {
                RaceParticipant participant = _participants[i];
                if (participant.HasFinished) continue;
                if (participant.AI != null) participant.AI.Think();
                participant.Movement.Step(deltaTime);
                HorseSimulation horse = participant.Movement.Simulation;
                if (horse.Distance < _trackLength) continue;
                float fraction = Mathf.Clamp01((_trackLength - horse.PreviousDistance) /
                    Mathf.Max(0.0001f, horse.Distance - horse.PreviousDistance));
                float crossingTime = ElapsedTime + fraction * deltaTime;
                int insertion = candidateCount;
                // Resolve same-tick finishes by crossing time, not participant iteration order.
                while (insertion > 0 && _crossingTimes[insertion - 1] > crossingTime)
                {
                    _finishCandidates[insertion] = _finishCandidates[insertion - 1];
                    _crossingTimes[insertion] = _crossingTimes[insertion - 1];
                    insertion--;
                }
                _finishCandidates[insertion] = participant;
                _crossingTimes[insertion] = crossingTime;
                candidateCount++;
            }

            ElapsedTime += deltaTime;
            for (int i = 0; i < candidateCount; i++)
            {
                _finishedCount++;
                _finishCandidates[i].Finish(_finishedCount, _crossingTimes[i], _trackLength);
                ParticipantFinished?.Invoke(_finishCandidates[i]);
                if (_finishCandidates[i].IsPlayer) Changed?.Invoke();
            }

            if (_finishedCount == _participants.Length)
            {
                Phase = RacePhase.Finished;
                Changed?.Invoke();
            }
            if (_goRemaining > 0f)
            {
                _goRemaining -= deltaTime;
                if (_goRemaining <= 0f)
                {
                    CountdownText = string.Empty;
                    Changed?.Invoke();
                }
            }
            _hudRemaining -= deltaTime;
            if (_hudRemaining <= 0f)
            {
                _hudRemaining = 0.1f;
                HudChanged?.Invoke();
            }
        }
    }
}
