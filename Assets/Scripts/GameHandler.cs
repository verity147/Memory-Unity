using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GameHandler : MonoBehaviour
{
    private PostProcessLayer processLayer;

    private void Awake()
    {
        processLayer = Camera.main.GetComponent<PostProcessLayer>();
    }
}
