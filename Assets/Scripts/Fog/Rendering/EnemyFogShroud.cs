using UnityEngine;

public class EnemyFogShroud : MonoBehaviour
{
    public float radius = 3.5f;
    [Range(-1, 1)] public float densityDelta = 0.2f; // + higher, -thinner
    static readonly int _SDFCenter = Shader.PropertyToID("_SDFCenter");
    static readonly int _SDFRadius = Shader.PropertyToID("_SDFRadius");
    static readonly int _SDFDelta = Shader.PropertyToID("_SDFDelta");

    void LateUpdate()
    {
        Shader.SetGlobalVector(_SDFCenter, transform.position);
        Shader.SetGlobalFloat(_SDFRadius, radius);
        Shader.SetGlobalFloat(_SDFDelta, densityDelta);
    }
}
