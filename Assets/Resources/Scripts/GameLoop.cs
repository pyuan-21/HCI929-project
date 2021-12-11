using Assets.Resources.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
/// <summary>
/// inheriting from DefaultInitializationErrorHandler to replace it
/// </summary>
public class GameLoop : MonoBehaviour
{
    [SerializeField]
    public List<String> imageNameList;

    void Start()
    {
        VuforiaApplication.Instance.OnVuforiaInitialized += OnVuforiaInitialized;
    }
    void OnDestroy()
    {
        VuforiaApplication.Instance.OnVuforiaInitialized -= OnVuforiaInitialized;
    }

    void OnVuforiaInitialized(VuforiaInitError vuforiaInitError)
    {
        if (vuforiaInitError != VuforiaInitError.NONE)
        {
            return;
        }
        Debug.Log("GameLoop.OnVuforiaInitialized");

        GameLogic.Instance.Init(imageNameList);
    }
    private void Update()
    {
        GameLogic.Instance.Update();
    }
}