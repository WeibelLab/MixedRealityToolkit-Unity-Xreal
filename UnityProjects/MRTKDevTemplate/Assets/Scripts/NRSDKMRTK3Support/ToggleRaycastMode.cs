using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class ToggleRaycastMode : MonoBehaviour
{
    // Start is called before the first frame update
    
    public void ToggleMode()
    {
        NRInput.RaycastMode = NRInput.RaycastMode == RaycastModeEnum.Gaze ? RaycastModeEnum.Laser : RaycastModeEnum.Gaze;
    }
}
