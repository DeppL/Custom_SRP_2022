using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

public class LightingPass
{
    private Lighting _lighting;
    private CullingResults _cullingResults;
    private ShadowSettings _shadowSettings;
    private bool _useLightsPerObject;
    private int _renderingLayerMask;

    void Render(RenderGraphContext context) => _lighting.Setup(
        context, _cullingResults, _shadowSettings,
        _useLightsPerObject, _renderingLayerMask);

    public static void Record(
        RenderGraph renderGraph, Lighting lighting,
        CullingResults cullingResults, ShadowSettings shadowSettings,
        bool useLightsPerObject, int renderingLayerMask)
    {
        using RenderGraphBuilder builder =
            renderGraph.AddRenderPass("Lighting", out LightingPass pass);
        pass._lighting = lighting;
        pass._cullingResults = cullingResults;
        pass._shadowSettings = shadowSettings;
        pass._useLightsPerObject = useLightsPerObject;
        pass._renderingLayerMask = renderingLayerMask;
        builder.SetRenderFunc<LightingPass>((pass, context) => pass.Render(context));
    }
}
