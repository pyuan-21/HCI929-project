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

    public void InitImage()
    {
        setImage();
    }

    private void setImage()
    {
        GameObject st = GameObject.Find("SolutionTarget");
        GameObject imageFull = (st.transform.Find("MountParent").gameObject).transform.GetChild(1).gameObject;
        Renderer rend = imageFull.GetComponent<Renderer>();
        var originalTexture = UnityEngine.Resources.Load(String.Format("Images/{0}", GameLogic.Instance.GetCurrentImageName())) as Texture2D;
        rend.material.mainTexture = originalTexture;
    }

    public void onButtonReleased(VirtualButtonBehaviour vb)
    {
        //GameObject st = GameObject.Find("SolutionTarget");
        //GameObject congrats = (st.transform.Find("MountParent").gameObject).transform.GetChild(0).gameObject;
        //congrats.SetActive(false);

        GameObject vicUI = GameObject.Find("VictoryUI");
        GameObject image = vicUI.transform.GetChild(2).gameObject; //Image2
        image.SetActive(false);
               
        //get the next image from ImageNameList
        //change the texture for the cell
        GameLogic.Instance.OnChangeImage();

        //change the texture of ImageFull (display full image)
        setImage();
    }
   

  
}
