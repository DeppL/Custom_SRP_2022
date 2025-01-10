// ************************************
// *
// *文件名称：CameraRendererTextures.cs
// *描   述：
// *作   者：yuliang
// *创建日期：2025年1月10日11：9
// *修改日期：2025年1月10日11：9
// *
// ************************************

using UnityEngine.Experimental.Rendering.RenderGraphModule;

public readonly ref struct CameraRendererTextures
{
    public readonly TextureHandle
        colorAttachment,
        depthAttachment,
        colorCopy,
        depthCopy;

    public CameraRendererTextures(
        TextureHandle colorAttachment,
        TextureHandle depthAttachment,
        TextureHandle colorCopy,
        TextureHandle depthCopy
    )
    {
        this.colorAttachment = colorAttachment;
        this.depthAttachment = depthAttachment;
        this.colorCopy = colorCopy;
        this.depthCopy = depthCopy;
    }
}