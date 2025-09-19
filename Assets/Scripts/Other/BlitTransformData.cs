using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

[ExecuteInEditMode]
public class BlitTransformData : MonoBehaviour
{
    public GameObject _gameObject;
    private Transform _transform;
    public Material _material;
    private static readonly int forwardID = Shader.PropertyToID("_Forward");
    private static readonly int leftID = Shader.PropertyToID("_Left");
    private static readonly int upID = Shader.PropertyToID("_Up");
    void Start()
    {
        _transform = _gameObject.transform;
    }
    
    void Update()
    {
        if (_material == null)
            return;
        _material.SetVector(forwardID, _transform.forward);
        //Debug.Log(_transform.forward);
        _material.SetVector(leftID, _transform.right);
        _material.SetVector(upID, _transform.up);
        //Debug.Log(_transform.up);
    }
}
