using System;
using Unity.VisualScripting;
using UnityEngine;

public class ShadowSwordController : MonoBehaviour
{
    [SerializeField] private Material shaderMat;
    [SerializeField] private float materializationSpeed;
    private float scrollValue = -1f;

    void Start()
    {
        shaderMat = GetComponent<MeshRenderer>().material; 
    }

    void Update()
    {

        if (scrollValue < 0)
        {
            scrollValue += materializationSpeed;
            shaderMat.SetFloat("_Slash_Scroll", scrollValue);
        } 

    }
}
