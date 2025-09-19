using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayPosition : MonoBehaviour
{
    public Material grass;
    private static readonly int Position = Shader.PropertyToID("_PlayPosition");

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        grass.SetVector(Position, transform.position);
    }
}
