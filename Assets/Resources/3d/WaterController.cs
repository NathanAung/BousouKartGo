using System.Collections;
using UnityEngine;

public class WaterController : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Texture[] baseMaps;   // 5 albedo textures
    [SerializeField] private Texture[] normalMaps; // 5 normal maps
    [SerializeField] private float switchInterval = 0.2f;

    private Material mat;

    private void Start()
    {
        if (targetRenderer == null || baseMaps.Length < 5 || normalMaps.Length < 5)
        {
            Debug.LogError("Assign renderer and 5 textures for both BaseMap and NormalMap!");
            return;
        }

        // make unique material instance so it only affects this object
        mat = new Material(targetRenderer.material);
        targetRenderer.material = mat;

        // enable URP normal mapping
        mat.EnableKeyword("_NORMALMAP");

        StartCoroutine(CycleTextures());
    }

    private IEnumerator CycleTextures()
    {
        int[] sequence = { 0, 1, 2, 3, 4, 3, 2, 1 }; // pattern
        int index = 0;

        while (true)
        {
            int texIndex = sequence[index];

            mat.SetTexture("_BaseMap", baseMaps[texIndex]);
            mat.SetTexture("_BumpMap", normalMaps[texIndex]);

            index = (index + 1) % sequence.Length;
            yield return new WaitForSeconds(switchInterval);
        }
    }
}