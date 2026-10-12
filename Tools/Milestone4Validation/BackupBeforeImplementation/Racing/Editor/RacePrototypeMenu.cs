using UnityEditor;
using UnityEditor.SceneManagement;

namespace Horsecremental.Racing.Editor
{
    public static class RacePrototypeMenu
    {
        public const string SCENE_PATH = "Assets/Racing/Scenes/Milestone1Race.unity";

        [MenuItem("Horsecremental/Open Milestone 1 Race")]
        public static void OpenRace()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(SCENE_PATH);
        }
    }
}
