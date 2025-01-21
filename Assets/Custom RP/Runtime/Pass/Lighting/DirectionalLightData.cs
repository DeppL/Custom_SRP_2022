// ************************************
// *
// *文件名称：DirectionalLightData.cs
// *描   述：
// *作   者：yuliang
// *创建日期：2025年1月21日10：58
// *修改日期：2025年1月21日10：58
// *
// ************************************


using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

partial class LightingPass
{
    [StructLayout(LayoutKind.Sequential)]
    struct DirectionalLightData
    {
        public const int stride = 4 * 4 * 3;
        public Vector4 color, directionAndMask, shadowData;

        public DirectionalLightData(
            ref VisibleLight visibleLight, Light light, Vector4 shadowData)
        {
            color = visibleLight.finalColor;
            directionAndMask = -visibleLight.localToWorldMatrix.GetColumn(2);
            directionAndMask.w = light.renderingLayerMask.ReinterpretAsFloat();
            this.shadowData = shadowData;
        }
    }
}