using System.Collections;
using UnityEngine;
using Vuforia;

namespace Assets.Resources.Scripts
{
    //just use Status filter: Tracked, refer:https://library.vuforia.com/articles/Solution/tracking-state.html
    public class ObserverEventHandler : DefaultObserverEventHandler
    {
        protected override void OnTrackingFound()
        {
            base.OnTrackingFound();
            GameLogic.Instance.AddImageTarget(gameObject);
        }

        protected override void OnTrackingLost()
        {
            base.OnTrackingLost();
            GameLogic.Instance.RemoveImageTarget(gameObject);
        }
    }
}