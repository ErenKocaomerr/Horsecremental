using System;
using UnityEngine;

namespace Horsecremental.Racing
{
    // Scene-independent session, reset on Play even with domain reload disabled. No save data yet.
    public static class ProgressionSession
    {
        private static ProgressionSettings _settings;
        public static PlayerProgression Player { get; private set; }

        public static PlayerProgression GetOrCreate(ProgressionSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (Player != null && _settings != settings)
                throw new InvalidOperationException("Stable and Race must share one ProgressionSettings asset.");
            if (Player == null)
            {
                _settings = settings;
                Player = settings.CreatePlayer();
            }
            return Player;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            Player = null;
            _settings = null;
        }
    }
}
