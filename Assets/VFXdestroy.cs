using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXdestroy : MonoBehaviour
{
    private ParticleSystem VFX;

    private void Start()
    {
        VFX = GetComponentInChildren<ParticleSystem>();
    }

    private void Update()
    {
        if (VFX.isStopped)
        {
            Debug.Log("REEE");
            Destroy(gameObject);
        }
    }
}
