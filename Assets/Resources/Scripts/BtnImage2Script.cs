using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class BtnImage2Script : MonoBehaviour
{
    public VirtualButtonBehaviour Vb;
    public GameObject solutionImg;
    public Texture texture;
    Renderer m_Renderer;
    // Start is called before the first frame update
    void Start()
    {
        Vb.RegisterOnButtonPressed(onButtonPressed);
        Vb.RegisterOnButtonReleased(onButtonReleased);
        m_Renderer = GetComponent<Renderer>();

    }

    public void onButtonPressed(VirtualButtonBehaviour vb)
    {
        solutionImg = GameObject.Find("SolutionTarget/MountParent/ImageFull");
        //texture = Resources.Load("Images/Puzzle/ew.jpg") as texture;
        
    }

    public void onButtonReleased(VirtualButtonBehaviour vb)
    {
        //solutionImg.renderer.material.mainTexture = texture;
    }
}
