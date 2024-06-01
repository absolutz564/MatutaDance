using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventController : MonoBehaviour
{
    // Start is called before the first frame update

    public void StartSucessAnim()
    {
        InteractionManager.instance.StartAnimEffects();
    }

    public void StartDance()
    {
        InteractionManager.instance.StartDance();
    }
}
