using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ColliderInputReciever : InputReciever
{
    private Vector3 clickPos;

    /// <summary>
    /// Called when the user clicks on the screen
    /// </summary>
    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                clickPos = hit.point;
                OnInputReceived();
            }
        }
    }

    /// <summary>
    /// Processes the input by passing the click position to all input handlers
    /// </summary>
    public override void OnInputReceived()
    {
        foreach (var handler in inputHandlers)
        {
            handler.ProcessInput(clickPos, null, null);
        }
    }
}
