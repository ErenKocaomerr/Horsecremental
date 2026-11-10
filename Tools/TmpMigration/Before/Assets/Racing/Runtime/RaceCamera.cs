using Unity.Cinemachine;
using UnityEngine;

namespace Horsecremental.Racing
{
    // Cinemachine owns follow movement and damping; this adapter only binds and resets it.
    public sealed class RaceCamera : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _virtualCamera;
        [SerializeField] private CinemachineFollow _follow;
        private Transform _target;

        public void Initialize(Transform target, RaceManager race)
        {
            _target = target;
            _virtualCamera.Follow = target;
            Snap();
        }

        private void Snap()
        {
            _virtualCamera.PreviousStateIsValid = false;
            _virtualCamera.ForceCameraPosition(_target.position + _follow.FollowOffset, Quaternion.identity);
        }
    }
}
