using System;
using HarmonyLib;
using Object = UnityEngine.Object;

namespace SceneView
{
    [HarmonyPatch(typeof(Object), nameof(Object.Destroy), new Type[] { typeof(Object)})]
    public class PatchObjectDestroy
    {
        private static void Postfix(Object __instance)
        {
            
        }
    }
}