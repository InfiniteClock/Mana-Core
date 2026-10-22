using System.Collections.Generic;
using UnityEngine;

public class Reticle : MonoBehaviour
{
    public float zRotation;

    [SerializeField]
    [Tooltip("References to the individual pieces of the reticle")]
    public List<RectTransform> reticlePieces;
    [SerializeField]
    [Tooltip("The angle that the reticle should move towards during bloom. !! List indexes MUST match reticlePieces list !!")]
    public List<float> reticleDirections;

    // The default positions of the individual reticle pieces
    [HideInInspector]
    public List<Vector3> reticlePositions;

    private void Awake()
    {
        reticlePositions = new List<Vector3>();
        for (int i = 0; i < reticlePieces.Count; i++)
        {
            reticlePositions.Add(reticlePieces[i].transform.localPosition);
        }
    }

    public void SetZRotation(float newRot)
    {
        zRotation = newRot;

        transform.rotation = Quaternion.Euler(0f, 0f, newRot);
    }
}
