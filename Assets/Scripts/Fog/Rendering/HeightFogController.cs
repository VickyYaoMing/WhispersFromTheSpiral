using UnityEngine;

public class HeightFogController : MonoBehaviour
{
    [Header("Color & Density of fog")]
    public Color fogColor = new Color(0.72f, 0.78f, 0.85f, 1);
    [Range(0, 0.2f)] public float density = 0.025f;
    public float fogHeight = 2.5f;
    [Range(0, 2f)] public float heightFalloff = 0.6f;

    [Header("Noise")]
    //Values will probably need some adjusting to fit the game better
    [Range(0, 1)] public float noiseAmp = 0.25f;
    [Range(0.005f, 0.1f)] public float noiseScale = 0.035f;
    public Vector2 noiseScroll = new Vector2(0.005f, 0.0f);
    [Range(0, 1)] public float ditherStrength = 0.08f;
    public Texture2D blueNoiseTex; // Even pixels my g.

    [Header("Near Veil")]
    public float nearVeilStart = 0.35f;
    public float nearVeilEnd = 1.2f;

    [Header("Sanity/Stress")]
    [Range(0, 1)] public float sanityBoost = 0f;

    public void OnEnable()
    {
        Shader.SetGlobalTexture("_blueNoiseTex", blueNoiseTex);
    }
    void OnDisable()
    {
        Shader.SetGlobalColor("_FogColor", fogColor);
        Shader.SetGlobalFloat("_Density", density);
        Shader.SetGlobalFloat("_Height", fogHeight);
        Shader.SetGlobalFloat("_HeightFalloff", heightFalloff);
        Shader.SetGlobalFloat("_NoiseAmp", noiseAmp);
        Shader.SetGlobalFloat("_NoiseScale", noiseScale);
        Shader.SetGlobalVector("_NoiseScroll", noiseScroll);
        Shader.SetGlobalFloat("_NearVeilStart", nearVeilStart);
        Shader.SetGlobalFloat("_NearVeilEnd", nearVeilEnd);
        Shader.SetGlobalFloat("_SanityBoost", sanityBoost);
        Shader.SetGlobalFloat("_DitherStrength", ditherStrength);
    }
}
