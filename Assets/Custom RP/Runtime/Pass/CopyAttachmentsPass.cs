using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

public class CopyAttachmentsPass
{
    private static readonly ProfilingSampler sampler = new("Copy Attachments");
    private CameraRenderer renderer;
    
    void Render(RenderGraphContext context)
    {
        renderer.CopyAttachments();
    }

    public static void Record(RenderGraph renderGraph, CameraRenderer renderer)
    {
        using RenderGraphBuilder builder = renderGraph.AddRenderPass(
            sampler.name, out CopyAttachmentsPass pass, sampler);
        pass.renderer = renderer;
        builder.SetRenderFunc<CopyAttachmentsPass>(
            (pass, context) => pass.Render(context));
    }
}
