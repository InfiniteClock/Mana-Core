using UnityEngine;


[DisallowMultipleComponent]
public class WeaponShaderController : MonoBehaviour
{
    [Range(0f, 1f)] public float visibility = 1f;
    [SerializeField] private Renderer[] objectRenderers;
    [SerializeField] private float visibleAmount = 0f;
    [SerializeField] private float hiddenAmount = 1.02f;

    private static readonly int MaterializationId = Shader.PropertyToID("_MaterializationAmount");
    private MaterialPropertyBlock propBlock;
    private void OnEnable()
    {
        if (objectRenderers == null || objectRenderers.Length == 0)
        {
            objectRenderers = GetComponentsInChildren<Renderer>(true);
        }
        ApplyMaterialization();
    }

    private void LateUpdate()
    {
        ApplyMaterialization();
    }

    private void ApplyMaterialization()
    {
        if (objectRenderers == null) return;
        if (propBlock == null) propBlock = new MaterialPropertyBlock();
        float amount = Mathf.Lerp(hiddenAmount, visibleAmount, Mathf.Clamp01(visibility));

        foreach (Renderer target in objectRenderers)
        {
            if (target == null) continue;
            Material[] materials = target.sharedMaterials;
            for (int slot = 0; slot < materials.Length; slot++)
            {
                Material material = materials[slot];
                if (material == null || !material.HasProperty(MaterializationId)) continue;
                target.GetPropertyBlock(propBlock, slot);
                propBlock.SetFloat(MaterializationId, amount);
                target.SetPropertyBlock(propBlock, slot);
            }
        }
    }

    private void OnDisable()
    {
        if (objectRenderers == null) return;
        if (propBlock == null) propBlock = new MaterialPropertyBlock();
        foreach (Renderer target in objectRenderers)
        {
            if (target == null) continue;
            Material[] materials = target.sharedMaterials;
            for (int slot = 0; slot < materials.Length; slot++)
            {
                Material material = materials[slot];
                if (material == null || !material.HasProperty(MaterializationId)) continue;
                target.GetPropertyBlock(propBlock, slot);
                propBlock.SetFloat(MaterializationId, material.GetFloat(MaterializationId));
                target.SetPropertyBlock(propBlock, slot);
            }
        }
    }
}

