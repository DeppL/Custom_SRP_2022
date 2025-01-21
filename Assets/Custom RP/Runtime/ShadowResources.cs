// ************************************
// *
// *文件名称：ShadowTextures.cs
// *描   述：
// *作   者：yuliang
// *创建日期：2025年1月13日9：51
// *修改日期：2025年1月13日9：51
// *
// ************************************


using Unity.Collections;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

public readonly ref struct ShadowResources
{
    public readonly TextureHandle directinalAtlas, otherAtlas;

    public readonly ComputeBufferHandle
        directionalShadowCascadesBuffer,
        directionalShadowMatricesBuffer,
        otherShadowDataBuffer;

    public ShadowResources(
        TextureHandle directinalAtlas,
        TextureHandle otherAtlas,
        ComputeBufferHandle directionalShadowCascadesBuffer,
        ComputeBufferHandle directionalShadowMatricesBuffer,
        ComputeBufferHandle otherShadowDataBuffer
    )
    {
        this.directinalAtlas = directinalAtlas;
        this.otherAtlas = otherAtlas;
        this.directionalShadowCascadesBuffer = directionalShadowCascadesBuffer;
        this.directionalShadowMatricesBuffer = directionalShadowMatricesBuffer;
        this.otherShadowDataBuffer = otherShadowDataBuffer;
    }
}
    