using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

public class VisibleGeometryPass
{
    private CameraRenderer _renderer;
    private bool useDynamicBatching, useGPUInstancing, useLightsPerObject;
    private int renderingLayerMask;

    void Render(RenderGraphContext context) => _renderer.DrawVisibleGeometry(
        useDynamicBatching, useGPUInstancing, useLightsPerObject, renderingLayerMask);

    public static void Record(
        RenderGraph renderGraph, CameraRenderer renderer,
        bool useDynamicBatching, bool useGPUInstancing, bool useLightsPerObject,
        int renderingLayerMask)
    {
        using RenderGraphBuilder builder =
            renderGraph.AddRenderPass("Visible Geometry", out VisibleGeometryPass pass);
        pass._renderer = renderer;
        pass.useDynamicBatching = useDynamicBatching;
        pass.useGPUInstancing = useGPUInstancing;
        pass.useLightsPerObject = useLightsPerObject;
        pass.renderingLayerMask = renderingLayerMask;
        builder.SetRenderFunc<VisibleGeometryPass>(
            (pass, context) => pass.Render(context));
    }
}
