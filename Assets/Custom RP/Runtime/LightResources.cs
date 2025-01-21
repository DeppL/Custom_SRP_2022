// ************************************
// *
// *文件名称：LightResources.cs
// *描   述：
// *作   者：yuliang
// *创建日期：2025年1月21日14：6
// *修改日期：2025年1月21日14：6
// *
// ************************************

using UnityEngine.Experimental.Rendering.RenderGraphModule;

public readonly ref struct LightResources
{
    public readonly ComputeBufferHandle
        directionalLightDataBuffer, otherLightDataBuffer;

    public readonly ShadowResources shadowResources;

    public LightResources(
        ComputeBufferHandle directionalLightDataBuffer,
        ComputeBufferHandle otherLightDataBuffer,
        ShadowResources shadowResources)
    {
        this.directionalLightDataBuffer = directionalLightDataBuffer;
        this.otherLightDataBuffer = otherLightDataBuffer;
        this.shadowResources = shadowResources;
    }
}