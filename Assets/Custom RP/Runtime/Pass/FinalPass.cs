using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

public class FinalPass
{
    private static readonly ProfilingSampler sampler = new("Final");
    private CameraRendererCopier copier;
    private TextureHandle colorAttachMent;

    void Render(RenderGraphContext context)
    {
        CommandBuffer buffer = context.cmd;
        copier.CopyToCameraTarget(buffer, colorAttachMent);
        context.renderContext.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    public static void Record(
        RenderGraph renderGraph,
        CameraRendererCopier copier,
        in CameraRendererTextures textures)
    {
        using RenderGraphBuilder builder =
            renderGraph.AddRenderPass(sampler.name, out FinalPass pass, sampler);
        pass.copier = copier;
        pass.colorAttachMent = builder.ReadTexture(textures.colorAttachment);
        builder.SetRenderFunc<FinalPass>(
            static (pass, context) => pass.Render(context));
    }
}
