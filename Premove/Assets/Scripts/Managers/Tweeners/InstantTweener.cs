using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantTweener : MonoBehaviour, IObjectTweener
{
    public bool MoveTo(Transform transform, Vector3 targetPosition)
    {
        transform.position = targetPosition;

        return false;
    }
}
