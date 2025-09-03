using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AnimationManager : SingeltonPersistant<AnimationManager>
{
    internal void MoveTo(Transform transform, Vector3 targetPosition)
    {
        if (GameSettings.Instance.showAnimations)
            MoveWithAnimation(transform, targetPosition);
        else
            MoveInstant(transform, targetPosition);
    }

    private void MoveInstant(Transform transform, Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }

    private void MoveWithAnimation(Transform transform, Vector3 targetPosition)
    {
        throw new NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
