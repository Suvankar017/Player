using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HipTracker : MonoBehaviour
{
    public Transform hip;

    private void Update()
    {
        Vector3 worldPos = hip.position;
        transform.position = worldPos;
    }
}
