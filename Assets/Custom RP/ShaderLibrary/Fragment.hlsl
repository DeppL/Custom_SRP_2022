﻿#ifndef FRAGMENT_INCLUDED
#define FRAGMENT_INCLUDED

TEXTURE2D(_CameraDepthTexture);
TEXTURE2D(_CameraColorTexture);

float4 _CameraBufferSize;

struct Fragment 
{
	float2 positionSS;
	float2 screenUV;
	float depth;
	float bufferDepth;
};

Fragment GetFragment (float4 positionSS)
{
	Fragment f;
	f.positionSS = positionSS.xy;
	f.screenUV = f.positionSS * _CameraBufferSize.xy;
	f.depth = IsOrthographicCamera() ?
		OrthographicDepthBufferToLinear(positionSS.z) : positionSS.w;
	f.bufferDepth = 
		LOAD_TEXTURE2D(_CameraDepthTexture, f.positionSS).r;
	f.bufferDepth = IsOrthographicCamera() ? 
		OrthographicDepthBufferToLinear(f.bufferDepth) :
		LinearEyeDepth(f.bufferDepth, _ZBufferParams);
	return f;
}

float4 GetBufferColor (Fragment fragment, float2 uvOffset = float2(0.0, 0.0))
{
	float2 uv = fragment.screenUV + uvOffset;
	return SAMPLE_TEXTURE2D_LOD(_CameraColorTexture, sampler_linear_clamp, uv, 0);
}

#endif