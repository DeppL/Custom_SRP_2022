// ************************************
// *
// *文件名称：ShadowTextures.cs
// *描   述：
// *作   者：yuliang
// *创建日期：2025年1月13日9：51
// *修改日期：2025年1月13日9：51
// *
// ************************************


using UnityEngine.Experimental.Rendering.RenderGraphModule;

public readonly ref struct ShadowTextures
{
    public readonly TextureHandle directinalAtlas, otherAtlas;

    public ShadowTextures(
        TextureHandle directinalAtlas,
        TextureHandle otherAtlas
    )
    {
        this.directinalAtlas = directinalAtlas;
        this.otherAtlas = otherAtlas;
    }
}
    