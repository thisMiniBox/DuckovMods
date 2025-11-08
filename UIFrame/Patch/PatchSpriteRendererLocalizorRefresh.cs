using HarmonyLib;
using SodaCraft.Localizations;
using System.Reflection; 
using UnityEngine;

namespace UIFrame.Patch
{
    [HarmonyPatch(typeof(SpriteRendererLocalizor), "Refresh")]
    public class PatchSpriteRendererLocalizorRefresh
    {
        private static FieldInfo spriteRendererField = AccessTools.Field(typeof(SpriteRendererLocalizor), "spriteRenderer");

        public static void Postfix(SpriteRendererLocalizor __instance)
        {
            if (GameOriginMainMenuUI.titleSprite != null && spriteRendererField != null)
            {
                SpriteRenderer targetSpriteRenderer = spriteRendererField.GetValue(__instance) as SpriteRenderer; 

                if (targetSpriteRenderer != null)
                {
                    targetSpriteRenderer.sprite = GameOriginMainMenuUI.titleSprite;
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"Harmony Patch: Failed to get SpriteRenderer from {__instance.gameObject.name} (SpriteRendererLocalizor component)'s spriteRenderer field. Value was null or not a SpriteRenderer.");
                }
            } else if (GameOriginMainMenuUI.titleSprite == null) {
                UnityEngine.Debug.LogWarning("Harmony Patch: GameOriginMainMenuUI.titleSprite is null, skipping sprite assignment.");
            } else if (spriteRendererField == null){
                UnityEngine.Debug.LogError("Harmony Patch: Failed to find 'spriteRenderer' field in SpriteRendererLocalizor. Is the field name correct or has it changed?");
            }
        }
    }
}