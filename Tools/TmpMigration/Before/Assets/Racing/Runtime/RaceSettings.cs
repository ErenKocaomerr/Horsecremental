using UnityEngine;

namespace Horsecremental.Racing
{
    [CreateAssetMenu(menuName = "Horsecremental/Race Settings")]
    public sealed class RaceSettings : ScriptableObject
    {
        [SerializeField, Min(100f)] private float _trackLength = 420f;
        [SerializeField] private HorseStats _playerStats;
        [SerializeField] private HorseStats[] _opponentStats;
        [SerializeField] private Texture2D[] _horseSheets;
        [SerializeField] private float[] _obstacleDistances = { 65f, 145f, 230f, 315f, 385f };
        [SerializeField, Min(0.1f)] private float _obstacleWidth = 0.65f;
        [SerializeField, Min(0.1f)] private float _obstacleHeight = 0.65f;
        [SerializeField, Range(0.15f, 0.4f)] private float _aiJumpLeadSeconds = 0.28f;

        public float TrackLength => _trackLength;
        public HorseStats PlayerStats => _playerStats;
        public HorseStats[] OpponentStats => _opponentStats;
        public Texture2D[] HorseSheets => _horseSheets;
        public float AiJumpLeadSeconds => _aiJumpLeadSeconds;

        public RaceObstacle[] CreateObstacles()
        {
            RaceObstacle[] obstacles = new RaceObstacle[_obstacleDistances.Length];
            for (int i = 0; i < obstacles.Length; i++)
                obstacles[i] = new RaceObstacle(_obstacleDistances[i], _obstacleWidth, _obstacleHeight);
            return obstacles;
        }

        private void OnValidate()
        {
            if (_obstacleDistances == null) return;
            System.Array.Sort(_obstacleDistances);
            for (int i = 0; i < _obstacleDistances.Length; i++)
                _obstacleDistances[i] = Mathf.Clamp(_obstacleDistances[i], 10f, _trackLength - 10f);
        }
    }
}
