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
    [SerializeField]
<<<<<<< Updated upstream
    public int blankMarkerIndex;//from 1 to 9. This index should be the index of background's marker.
=======
    public int blankMarkerIndex;//from 1 to 9
>>>>>>> Stashed changes

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

        GameLogic.Instance.Init(imageNameList, blankMarkerIndex);
    }
    private void Update()
    {
        GameLogic.Instance.Update();
    }
}