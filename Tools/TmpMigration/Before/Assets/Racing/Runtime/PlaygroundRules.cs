using System;

namespace Horsecremental.Racing
{
    public interface IPlaygroundSettings
    {
        int BaseHorseCount { get; }
        int MaxHorseCount { get; }
        int PlotCount { get; }
        float InitialCropSpread { get; }
        float MidThreshold { get; }
        float LargeThreshold { get; }
        float ClickGrowth { get; }
        float ClickCooldown { get; }
        int BaseYield { get; }
        int MaxPickups { get; }
        float DropSeconds { get; }
        float CollectionSeconds { get; }
        float ScatterRadius { get; }
        float HoverRadius { get; }
        float RaceBlend { get; }
    }

    public enum CropStage { Sprout, Mid, Large, Ready }
    public enum PickupResource { Carrots }

    // Normalized phases are also the positions used by the physical training view.
    public sealed class TrainingProduction
    {
        private readonly double[] _phases;
        public int Count { get; private set; }
        public event Action<int> LapCompleted;
        public TrainingProduction(int initialCount, int maximum, double initialPhase = 0d)
        {
            if (maximum < 1 || maximum > 6 || initialCount < 1 || initialCount > maximum)
                throw new ArgumentOutOfRangeException(nameof(initialCount));
            _phases = new double[maximum];
            Count = initialCount;
            for (int i = 0; i < Count; i++) _phases[i] = (initialPhase + (double)i / Count) % 1d;
        }
        public double Phase(int horse) => _phases[horse];
        public bool AddHorse()
        {
            if (Count == _phases.Length) return false;
            double largestGap = -1d;
            double start = 0d;
            for (int i = 0; i < Count; i++)
            {
                double nearest = 1d;
                for (int j = 0; j < Count; j++)
                {
                    if (i == j) continue;
                    double gap = (_phases[j] - _phases[i] + 1d) % 1d;
                    if (gap < nearest) nearest = gap;
                }
                if (nearest > largestGap) { largestGap = nearest; start = _phases[i]; }
            }
            _phases[Count++] = (start + largestGap * 0.5d) % 1d;
            return true;
        }
        public void Advance(float seconds, float lapSeconds)
        {
            if (!ValidDelta(seconds) || !ValidDelta(lapSeconds)) return;
            double step = seconds / (double)lapSeconds;
            for (int i = 0; i < Count; i++)
            {
                _phases[i] += step;
                while (_phases[i] >= 1d)
                {
                    _phases[i] -= 1d;
                    LapCompleted?.Invoke(i);
                }
            }
        }
        internal static bool ValidDelta(float value) => value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
    }

    public sealed class CropGrowth
    {
        private readonly double[] _progress;
        private readonly IPlaygroundSettings _settings;
        public int Count => _progress.Length;
        public event Action<int> StageChanged;
        public CropGrowth(IPlaygroundSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _progress = new double[settings.PlotCount];
            for (int i = 0; i < Count; i++)
                _progress[i] = i * settings.InitialCropSpread / Math.Max(1, Count - 1);
        }
        public double Progress(int plot) => _progress[plot];
        public CropStage Stage(int plot) => _progress[plot] >= 1d ? CropStage.Ready :
            _progress[plot] >= _settings.LargeThreshold ? CropStage.Large :
            _progress[plot] >= _settings.MidThreshold ? CropStage.Mid : CropStage.Sprout;
        public void Advance(float seconds, float duration)
        {
            if (!ValidDelta(seconds) || !ValidDelta(duration)) return;
            for (int i = 0; i < Count; i++) Grow(i, seconds / (double)duration);
        }
        public void Grow(int plot, double amount)
        {
            if (amount <= 0d || double.IsNaN(amount) || double.IsInfinity(amount)) return;
            CropStage before = Stage(plot);
            _progress[plot] = Math.Min(1d, _progress[plot] + amount);
            if (Stage(plot) != before) StageChanged?.Invoke(plot);
        }
        public bool Harvest(int plot)
        {
            if (Stage(plot) != CropStage.Ready) return false;
            _progress[plot] = 0d;
            StageChanged?.Invoke(plot);
            return true;
        }
        private static bool ValidDelta(float value) => value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
    }
}

