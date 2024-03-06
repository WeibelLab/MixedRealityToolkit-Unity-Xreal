using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleHandImage : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
     GameObject HandImage; 

    // Update is called once per frame
    public void CheckAndToggleHandImage()
    {
        

        if (HandImage != null)
        {
            HandImage.SetActive(!HandImage.activeSelf);
        }
    }
}
