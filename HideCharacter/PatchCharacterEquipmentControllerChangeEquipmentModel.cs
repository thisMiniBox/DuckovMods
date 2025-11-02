using HarmonyLib;

namespace HideCharacter
{
    [HarmonyPatch(typeof(CharacterEquipmentController), "ChangeEquipmentModel")]
    public class PatchCharacterEquipmentControllerChangeEquipmentModel
    {
        public static void Postfix(CharacterEquipmentController __instance)
        {
            var manage = ModBehaviour.hideHideCharacterManager;
            if (manage!=null)
            {
                if(manage.hide)
                    manage.SetCharacterHide(manage.hide);
            }
        }
    }
}