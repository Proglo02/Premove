using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSetter : MonoBehaviour
{
    private MeshRenderer _meshRenderer;

    private MeshRenderer meshRenderer
    {
        get
        {
            if (_meshRenderer == null)
            {
                _meshRenderer = GetComponentInChildren<MeshRenderer>();

                if (_meshRenderer == null)
                    throw new System.Exception("Missing piece model");
            }
            return _meshRenderer;
        }
    }

    /// <summary>
    /// Set the material of the mesh renderer
    /// </summary>
    public void SetMaterial(Material material)
    {
        meshRenderer.material = material;
    }
}
