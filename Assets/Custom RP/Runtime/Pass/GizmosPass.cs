using System.Diagnostics;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;


public class GizmosPass
{
#if UNITY_EDITOR

    private static readonly ProfilingSampler sampler = new("Gizmos");
    private bool requiresDepthCopy;
    private CameraRendererCopier copier;
    private TextureHandle depthAttachment;
    void Render(RenderGraphContext context)
    {
        CommandBuffer buffer = context.cmd;
        ScriptableRenderContext renderContext = context.renderContext;
        if (requiresDepthCopy)
        {
            copier.CopyByDrawing(
                buffer, depthAttachment, BuiltinRenderTextureType.CameraTarget, true);
            renderContext.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }
        renderContext.DrawGizmos(copier.Camera, GizmoSubset.PreImageEffects);
        renderContext.DrawGizmos(copier.Camera, GizmoSubset.PostImageEffects);
    }
#endif

    [Conditional("UNITY_EDITOR")]
    public static void Record(
        RenderGraph renderGraph,
        bool useIntermediateBuffer,
        CameraRendererCopier copier,
        in CameraRendererTextures textures)
    {
#if UNITY_EDITOR
        if (Handles.ShouldRenderGizmos())
        {
            using RenderGraphBuilder builder = 
                renderGraph.AddRenderPass(sampler.name, out GizmosPass pass, sampler);
            pass.requiresDepthCopy = useIntermediateBuffer;
            pass.copier = copier;
            if (useIntermediateBuffer)
            {
                pass.depthAttachment = builder.ReadTexture(textures.depthAttachment);
            }
            builder.SetRenderFunc<GizmosPass>(
                static (pass, context) => pass.Render(context));
        }
#endif
    }
}
