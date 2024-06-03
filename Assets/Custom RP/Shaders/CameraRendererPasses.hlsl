#ifndef CUSTOM_CAMERA_RENDERER_PASSES_INCLUDED
#define CUSTOM_CAMERA_RENDERER_PASSES_INCLUDED

TEXTURE2D(_SourceTexture);

struct Varyings 
{
	float4 positionCS : SV_POSITION;
	float2 fxUV : VAR_FX_UV;
};


Varyings DefaultPassVertex (uint vertexID : SV_VERTEXID)
{
	Varyings output;
	output.positionCS = float4(
		vertexID <= 1 ? -1.0 : 3.0,
		vertexID == 1 ? 3.0 : -1.0,
		0.0, 1.0
	);
	output.fxUV = float2(
		vertexID <= 1 ? 0.0 : 2.0,
		vertexID == 1 ? 2.0 : 0.0
	);
	if (_ProjectionParams.x < 0.0)
	{
		output.fxUV.y = 1.0 - output.fxUV.y;
	}
	return output;
}

float4 CopyPassFragment(Varyings input) : SV_TARGET
{
	return SAMPLE_TEXTURE2D_LOD(_SourceTexture, sampler_linear_clamp, input.fxUV, 0);
}

float CopyDepthPassFragment(Varyings input) : SV_DEPTH
{
	return SAMPLE_DEPTH_TEXTURE_LOD(_SourceTexture, sampler_point_clamp, input.fxUV, 0);
}

#endif
