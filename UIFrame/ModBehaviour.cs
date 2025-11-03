using UnityEngine;

namespace UIFrame
{
    public class ModBehaviour:Duckov.Modding.ModBehaviour
    {
        protected override void OnAfterSetup()
        {
            Debug.Log("OnAfterSetup");
        }

        protected override void OnBeforeDeactivate()
        {
            Debug.Log("OnBeforeDeactivate");
        }
    }
}