using HarmonyLib;
using UnityEngine;

namespace SceneView
{
    // [HarmonyPatch(typeof(GameObject), "Internal_CreateGameObject")]
    // public class PatchGameObjectStart
    // {
    //     static void Postfix(GameObject __instance)
    //     {
    //         Debug.Log($"{__instance.name}初始化了");
    //     }
    // }
}