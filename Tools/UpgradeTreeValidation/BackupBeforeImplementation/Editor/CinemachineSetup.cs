using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Horsecremental.Racing.Editor
{
    internal static class CinemachineSetup
    {
        internal static CinemachineCamera Convert(Camera camera)
        {
            if (!camera.TryGetComponent(out CinemachineBrain brain)) brain = camera.gameObject.AddComponent<CinemachineBrain>();
            brain.UpdateMethod = CinemachineBrain.UpdateMethods.LateUpdate;
            brain.BlendUpdateMethod = CinemachineBrain.BrainUpdateMethods.LateUpdate;
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
            GameObject virtualObject = new GameObject(camera.name + " Cinemachine", typeof(CinemachineCamera));
            virtualObject.transform.SetParent(camera.transform.parent, false);
            virtualObject.transform.SetPositionAndRotation(camera.transform.position, camera.transform.rotation);
            CinemachineCamera virtualCamera = virtualObject.GetComponent<CinemachineCamera>();
            virtualCamera.Lens = LensSettings.FromCamera(camera);
            if (camera.TryGetComponent(out PixelPerfectCamera unused)) virtualObject.AddComponent<CinemachinePixelPerfect>();
            return virtualCamera;
        }

        internal static void ConfigureRace(Camera camera, RaceCamera adapter, Transform player)
        {
            CinemachineCamera virtualCamera = Convert(camera);
            virtualCamera.Follow = player;
            CinemachineFollow follow = virtualCamera.gameObject.AddComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(7f, 0.5f - RaceTrackBuilder.FIRST_LANE_Y, -10f);
            follow.TrackerSettings.BindingMode = BindingMode.WorldSpace;
            follow.TrackerSettings.PositionDamping = new Vector3(0.15f, 0f, 0f);
            RaceSceneBaker.SetReference(adapter, "_virtualCamera", virtualCamera);
            RaceSceneBaker.SetReference(adapter, "_follow", follow);
        }
    }
}
