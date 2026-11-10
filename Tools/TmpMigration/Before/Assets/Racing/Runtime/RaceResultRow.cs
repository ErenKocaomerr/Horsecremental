using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    public sealed class RaceResultRow : MonoBehaviour
    {
        [SerializeField] private Text _place;
        [SerializeField] private Text _name;
        [SerializeField] private Text _time;
        [SerializeField] private Image _highlight;
        private RaceParticipant _participant;
        private bool _finished;

        public void Show(RaceParticipant participant)
        {
            if (_participant == participant && _finished == participant.HasFinished) return;
            _participant = participant;
            _finished = participant.HasFinished;
            _place.text = _finished ? participant.FinishPosition.ToString() : "—";
            _name.text = participant.DisplayName;
            _time.text = _finished ? participant.FinishTime.ToString("0.00") + " s" : "Racing";
            _highlight.color = participant.IsPlayer ? new Color(0.65f, 0.48f, 0.08f, 0.65f) : new Color(1f, 1f, 1f, 0.05f);
        }

        public void Clear()
        {
            _participant = null;
            _finished = false;
            _place.text = _name.text = _time.text = string.Empty;
            _highlight.color = new Color(1f, 1f, 1f, 0.05f);
        }
    }
}
