using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class SizeButtonScript : MonoBehaviour
{
    public VirtualButtonBehaviour Vb;

    // Start is called before the first frame update
    void Start()
    {
        Vb.RegisterOnButtonPressed(onButtonPressed);
        Vb.RegisterOnButtonReleased(onButtonReleased);

    }

    public void onButtonPressed(VirtualButtonBehaviour vb)
    {
        Debug.Log("******** SIZE BUTTON PRESSED *******");
    }

    public void onButtonReleased(VirtualButtonBehaviour vb)
    {
        //Debug.Log("btn unprSSSSSSSSSSSSSSSSSSSSessed");
    }

}
