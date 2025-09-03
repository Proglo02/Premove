using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjectTweener
{
    /// <summary>
    /// Moves the given transform to the target position
    /// </summary>
    bool MoveTo(Transform transform, Vector3 targetPosition);
}
