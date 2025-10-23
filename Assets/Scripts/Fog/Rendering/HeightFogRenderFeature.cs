using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HeightFogRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Tooltip("Fog shader: Shaders/Fog/HeightFog.shader")]
        public Shader fogShader;

        [Tooltip("Place After Opaques / After Skybox (before post).")]
        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingSkybox;
    }

    class Pass : ScriptableRenderPass
    {
        static readonly string kTag = "HeightFogPass";
        readonly Material mat;
        RTHandle cameraColor;

        public Pass(Material m, RenderPassEvent evt)
        {
            mat = m;
            renderPassEvent = evt;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData data)
        {
            cameraColor = data.cameraData.renderer.cameraColorTargetHandle;
        }

        public override void Execute(ScriptableRenderContext ctx, ref RenderingData data)
        {
            if (mat == null) return;
            var cmd = CommandBufferPool.Get(kTag);

            // Full-screen draw using Blitter; reads _BlitTexture and writes back to cameraColor
            Blitter.BlitCameraTexture(cmd, cameraColor, cameraColor, mat, 0);

            ctx.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    public Settings settings = new Settings();
    Material _mat;
    Pass _pass;

    public override void Create()
    {
        if (settings.fogShader != null)
            _mat = CoreUtils.CreateEngineMaterial(settings.fogShader);
        _pass = new Pass(_mat, settings.passEvent);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
    {
        if (_mat == null) return;
        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(_mat);
    }
}