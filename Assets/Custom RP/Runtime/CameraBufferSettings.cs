using UnityEngine;
using System;

[Serializable]
public struct CameraBufferSettings
{
    public bool allowHDR;
    public bool copyColor, copyColorReflection, copyDepth, copyDepthReflections;

    [Range(CameraRenderer.renderScaleMin, CameraRenderer.renderScaleMax)]
    public float renderScale;
    public enum BicubicRescalingMode { Off, UpOnly, UpAndDown }
    public BicubicRescalingMode bicubicRescaling;
    
    [Serializable]
    public struct FXAA
    {
        public bool enabled;
        
        [Range(0.0312f, 0.0833f)]
        public float fixedThreshold;
        
        [Range(0.063f, 0.333f)]
        public float relativeThreshold;
        
        [Range(0f, 1f)]
        public float subpixelBlending;
        
        public Quality quality;
        public enum Quality { Low, Medium, High }
    }
    public FXAA fxaa;
    
}
