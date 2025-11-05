

using UnityEngine;

namespace UIFrame.Patch
{
    [HarmonyLib.HarmonyPatch(typeof(SceneLoader), "LoadMainMenu")]
    public class PatchSceneLoaderLoadMainMenu
    {
        public static void Postfix()
        {
            Debug.Log("LoadMainMenu called");
        }
    }
}