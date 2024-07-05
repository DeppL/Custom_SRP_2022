using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

public partial class CameraRenderer {

	public const float renderScaleMin = 0.1f, renderScaleMax = 2f;

	static ShaderTagId
		unlitShaderTagId = new ShaderTagId ("SRPDefaultUnlit"),
		litShaderTagId = new ShaderTagId ("CustomLit");

	private CommandBuffer buffer;

	ScriptableRenderContext context;

	public Camera camera;

	CullingResults cullingResults;

	Lighting lighting = new Lighting ();
	PostFXStack postFXStack = new PostFXStack();
	public static int 
		bufferSizeId = Shader.PropertyToID("_CameraBufferSize"),
		colorAttachmentId = Shader.PropertyToID("_CameraColorAttachment"),
		depthAttachmentId = Shader.PropertyToID("_CameraDepthAttachment"),
		colorTextureId = Shader.PropertyToID("_CameraColorTexture"),
		depthTextureId = Shader.PropertyToID("_CameraDepthTexture"),
		sourceTextureId = Shader.PropertyToID("_SourceTexture"),
		srcBlendId = Shader.PropertyToID("_CameraSrcBlend"),
		dstBlendId = Shader.PropertyToID("_CameraDstBlend");
	bool useHDR, useScaledRendering;
	static CameraSettings defaultCameraSettings = new CameraSettings();

	public bool useColorTexture, useDepthTexture, useIntermediateBuffer;
	Vector2Int bufferSize;

	Material material;
	Texture2D missingTexture;

	static bool copyTextureSupported = SystemInfo.copyTextureSupport > CopyTextureSupport.None;

	public CameraRenderer (Shader shader)
	{
		material = CoreUtils.CreateEngineMaterial(shader);
		missingTexture = new Texture2D(1, 1) {
			hideFlags = HideFlags.HideAndDontSave,
			name = "Missing"
		};
		missingTexture.SetPixel(0, 0, Color.white * 0.5f);
		missingTexture.Apply(true, true);
	}

	public void Dispose ()
	{
		CoreUtils.Destroy(material);
	}
	public void Render(
		RenderGraph renderGraph,
		ScriptableRenderContext context, Camera camera, CameraBufferSettings bufferSettings,
		bool useDynamicBatching, bool useGPUInstancing, bool useLightsPerObject,
		ShadowSettings shadowSettings, PostFXSettings postFXSettings,
		int colorLUTResolution
	) {
		this.context = context;
		this.camera = camera;
	
		ProfilingSampler cameraSampler;
		CameraSettings cameraSettings;
		if (camera.TryGetComponent(out CustomRenderPipelineCamera crpCamera))
		{
			cameraSampler = crpCamera.Sampler;
			cameraSettings = crpCamera.Settings;
		}
		else
		{
			cameraSampler = ProfilingSampler.Get(camera.cameraType);
			cameraSettings = defaultCameraSettings;
		}
		// useDepthTexture = true;
		if (camera.cameraType == CameraType.Reflection)
		{
			useColorTexture = bufferSettings.copyColorReflection;
			useDepthTexture = bufferSettings.copyDepthReflections;
		}
		else
		{
			useColorTexture = bufferSettings.copyColor && cameraSettings.copyColor;
			useDepthTexture = bufferSettings.copyDepth && cameraSettings.copyDepth;
		}

		if (cameraSettings.overridePostFX)
		{
			postFXSettings = cameraSettings.postFXSettings;
		}
		float renderScale = cameraSettings.GetRenderScale(bufferSettings.renderScale);
		useScaledRendering = renderScale < 0.99f || renderScale > 1.01f;
		PrepareForSceneWindow();
		if (!Cull(shadowSettings.maxDistance)) {
			return;
		}
		useHDR = bufferSettings.allowHDR && camera.allowHDR;
		if (useScaledRendering)
		{
			renderScale = Mathf.Clamp(renderScale, renderScaleMin, renderScaleMax);
			bufferSize.x = (int)(camera.pixelWidth * renderScale);
			bufferSize.y = (int)(camera.pixelHeight * renderScale);
		}
		else
		{
			bufferSize.x = camera.pixelWidth;
			bufferSize.y = camera.pixelHeight;
		}

		bufferSettings.fxaa.enabled &= cameraSettings.allowFXAA;
		postFXStack.Setup(
			camera, bufferSize, postFXSettings, cameraSettings.keepAlpha, useHDR, colorLUTResolution,
			cameraSettings.finalBlendMode, bufferSettings.bicubicRescaling,
			bufferSettings.fxaa);

		useIntermediateBuffer = useScaledRendering ||
		                        useColorTexture || useDepthTexture || postFXStack.IsActive;
		var renderGraphParameters = new RenderGraphParameters()
		{
			commandBuffer = CommandBufferPool.Get(),
			currentFrameIndex = Time.frameCount,
			executionName = cameraSampler.name,
			scriptableRenderContext = context
		};
		buffer = renderGraphParameters.commandBuffer;
		using (renderGraph.RecordAndExecute(renderGraphParameters))
		{
			using var _ = new RenderGraphProfilingScope(renderGraph, cameraSampler);
			LightingPass.Record(
				renderGraph, lighting,
				cullingResults, shadowSettings, useLightsPerObject,
				cameraSettings.maskLights ? cameraSettings.renderingLayerMask : -1);
			SetupPass.Record(renderGraph, this);
			VisibleGeometryPass.Record(
				renderGraph, this,
				useDynamicBatching, useGPUInstancing, useLightsPerObject,
				cameraSettings.renderingLayerMask
			);
			UnsupportedShadersPass.Record(renderGraph, this);
			if (postFXStack.IsActive)
			{
				PostFXPass.Record(renderGraph, postFXStack);
			}
			else if (useIntermediateBuffer)
			{
				FinalPass.Record(renderGraph, this, cameraSettings.finalBlendMode);
			}
			GizmosPass.Record(renderGraph, this);
		}
		Cleanup();
		Submit();
		CommandBufferPool.Release(renderGraphParameters.commandBuffer);
	}

	bool Cull (float maxShadowDistance) {
		if (camera.TryGetCullingParameters(out ScriptableCullingParameters p)) {
			p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
			cullingResults = context.Cull(ref p);
			return true;
		}
		return false;
	}

	 public void Setup () {
		context.SetupCameraProperties (camera);
		CameraClearFlags flags = camera.clearFlags;

		if (useIntermediateBuffer)
		{
			if (flags > CameraClearFlags.Color)
			{
				flags = CameraClearFlags.Color;
			}
			buffer.GetTemporaryRT(
				colorAttachmentId, bufferSize.x, bufferSize.y,
				32, FilterMode.Bilinear, 
				useHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default
			);
			buffer.GetTemporaryRT(
				depthAttachmentId, bufferSize.x, bufferSize.y,
				32, FilterMode.Point, RenderTextureFormat.Depth
			);
			buffer.SetRenderTarget(
				colorAttachmentId,
				RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
				depthAttachmentId,
				RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
			);
		}
		buffer.ClearRenderTarget (
			flags <= CameraClearFlags.Depth,
			flags <= CameraClearFlags.Color,
			flags == CameraClearFlags.Color ?
			camera.backgroundColor.linear : Color.clear
		);
		buffer.SetGlobalTexture(colorTextureId, missingTexture);
		buffer.SetGlobalTexture(depthTextureId, missingTexture);
		buffer.SetGlobalVector(bufferSizeId, new Vector4(
			1f / bufferSize.x, 1f / bufferSize.y, bufferSize.x, bufferSize.y));
		ExecuteBuffer ();
	}

	void Submit () {
		ExecuteBuffer();
		context.Submit();
	}

	public void ExecuteBuffer () {
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}

	public void DrawVisibleGeometry(
		bool useDynamicBatching, bool useGPUInstancing, bool useLightsPerObject,
		int renderingLayerMask
	) {
		ExecuteBuffer();
		PerObjectData lightsPerObjectFlags = useLightsPerObject ?
			PerObjectData.LightData | PerObjectData.LightIndices :
			PerObjectData.None;
		var sortingSettings = new SortingSettings(camera) {
			criteria = SortingCriteria.CommonOpaque
		};
		var drawingSettings = new DrawingSettings(
			unlitShaderTagId, sortingSettings
		) {
			enableDynamicBatching = useDynamicBatching,
			enableInstancing = useGPUInstancing,
			perObjectData = 
				PerObjectData.ReflectionProbes |
				PerObjectData.Lightmaps |
				PerObjectData.ShadowMask |
				PerObjectData.LightProbe |
				PerObjectData.LightProbeProxyVolume |
				PerObjectData.OcclusionProbe |
				PerObjectData.OcclusionProbeProxyVolume |
				lightsPerObjectFlags
		};
		drawingSettings.SetShaderPassName(1, litShaderTagId);

		var filteringSettings = new FilteringSettings (
			RenderQueueRange.opaque, renderingLayerMask: (uint)renderingLayerMask);

		context.DrawRenderers(
			cullingResults, ref drawingSettings, ref filteringSettings
		);

		context.DrawSkybox(camera);
		if (useColorTexture || useDepthTexture)
		{
			CopyAttachments();
		}

		sortingSettings.criteria = SortingCriteria.CommonTransparent;
		drawingSettings.sortingSettings = sortingSettings;
		filteringSettings.renderQueueRange = RenderQueueRange.transparent;

		context.DrawRenderers(
			cullingResults, ref drawingSettings, ref filteringSettings
		);
	}
	void Cleanup()
	{
		lighting.Cleanup();
		if (useIntermediateBuffer)
		{
			buffer.ReleaseTemporaryRT(colorAttachmentId);
			buffer.ReleaseTemporaryRT(depthAttachmentId);
			if (useColorTexture)
			{
				buffer.ReleaseTemporaryRT(colorTextureId);
			}
			if (useDepthTexture)
			{
				buffer.ReleaseTemporaryRT(depthTextureId);
			}
		}
	}

	void CopyAttachments ()
	{
		if (useColorTexture)
		{
			buffer.GetTemporaryRT(
				colorTextureId, bufferSize.x, bufferSize.y,
				0, FilterMode.Bilinear, 
				useHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default
			);
			if (copyTextureSupported)
			{
				buffer.CopyTexture(colorAttachmentId, colorTextureId);
			}
			else
			{
				Draw(colorAttachmentId, colorTextureId);
			}
		}
		if (useDepthTexture)
		{
			buffer.GetTemporaryRT(
				depthTextureId, bufferSize.x, bufferSize.y,
				32, FilterMode.Point, RenderTextureFormat.Depth
			);
			if (copyTextureSupported)
			{
				buffer.CopyTexture(depthAttachmentId, depthTextureId);
			}
			else
			{
				Draw(depthAttachmentId, depthTextureId, true);
			}
			if (!copyTextureSupported)
			{
				buffer.SetRenderTarget(
					colorAttachmentId,
					RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
					depthAttachmentId,
					RenderBufferLoadAction.Load, RenderBufferStoreAction.Store
				);
			}
			ExecuteBuffer();
		}
	}

	public void Draw(
		RenderTargetIdentifier from, RenderTargetIdentifier to, bool isDepth = false
	) 
	{
		buffer.SetGlobalTexture(sourceTextureId, from);
		buffer.SetRenderTarget(
			to, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
		);
		buffer.DrawProcedural(
			Matrix4x4.identity, material, isDepth ? 1 : 0, MeshTopology.Triangles, 3
		);
	}

	private static Rect fullViewRect = new Rect(0f, 0f, 1f, 1f);
    public void DrawFinal(CameraSettings.FinalBlendMode finalBlendMode)
    {
        buffer.SetGlobalFloat(srcBlendId, (float)finalBlendMode.source);
        buffer.SetGlobalFloat(dstBlendId, (float)finalBlendMode.destination);
        buffer.SetGlobalTexture(sourceTextureId, colorAttachmentId);
        buffer.SetRenderTarget(
            BuiltinRenderTextureType.CameraTarget,
            finalBlendMode.destination == BlendMode.Zero && camera.rect == fullViewRect ? 
                RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load,
            RenderBufferStoreAction.Store
        );
        buffer.SetViewport(camera.pixelRect);
        buffer.DrawProcedural(
            Matrix4x4.identity, material, 0,
            MeshTopology.Triangles, 3
        );
		buffer.SetGlobalFloat(srcBlendId, 1f);
		buffer.SetGlobalFloat(dstBlendId, 0f);
    }
}