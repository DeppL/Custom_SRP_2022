using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

public class UnsupportedShadersPass
{
#if UNITY_EDITOR
    private CameraRenderer _renderer;

    void Render(RenderGraphContext context) => _renderer.DrawUnsupportedShaders();
#endif

    [Conditional("UNITY_EDITOR")]
    public static void Record(RenderGraph renderGraph, CameraRenderer renderer)
    {
#if UNITY_EDITOR
        using RenderGraphBuilder builder = renderGraph.AddRenderPass(
            "Unsupported Shaders", out UnsupportedShadersPass pass);
        pass._renderer = renderer;
        builder.SetRenderFunc<UnsupportedShadersPass>((pass, context) => pass.Render(context));
#endif
    }
}
