using Assets.Resources.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ImgButtonScript : MonoBehaviour
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
        Debug.Log("********** IMAGE BUTTON PRESSED **********");
    }

    public void SetCurrentImage()
    {
        //change the texture of ImageFull (display full image)
        GameObject st = GameObject.Find("SolutionTarget");
        GameObject imageFull = (st.transform.Find("MountParent").gameObject).transform.GetChild(1).gameObject;
        Renderer rend = imageFull.GetComponent<Renderer>();
        var originalTexture = UnityEngine.Resources.Load(String.Format("Images/{0}", GameLogic.Instance.GetCurrentImageName())) as Texture2D;
        rend.material.mainTexture = originalTexture;
    }

    public void onButtonReleased(VirtualButtonBehaviour vb)
    {
        //change the texture for the cell
        //change the texture of ImageFull (display full image)
        GameLogic.Instance.OnNextGame();
    }
   

  
}
