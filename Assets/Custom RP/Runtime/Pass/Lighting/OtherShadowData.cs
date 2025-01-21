// ************************************
// *
// *文件名称：OtherShadowData.cs
// *描   述：
// *作   者：yuliang
// *创建日期：2025年1月21日11：24
// *修改日期：2025年1月21日11：24
// *
// ************************************


using UnityEngine;

partial class Shadows
{
    struct OtherShadowData
    {
        public const int stride = 4 * 4 + 4 * 16;

        public Vector4 tileData;
        
        public Matrix4x4 shadowMatrix;

        public OtherShadowData(
            Vector2 offset,
            float scale,
            float bias,
            float border,
            Matrix4x4 matrix)
        {
            tileData.x = offset.x * scale + border;
            tileData.y = offset.y * scale + border;
            tileData.z = scale - border - border;
            tileData.w = bias;
            shadowMatrix = matrix;
        }
    }
}