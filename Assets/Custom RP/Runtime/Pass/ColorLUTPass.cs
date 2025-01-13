// ************************************
// *
// *文件名称：ColorLUTPass.cs
// *描   述：
// *作   者：yuliang
// *创建日期：2025年1月13日11：20
// *修改日期：2025年1月13日11：20
// *
// ************************************

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;


using static PostFXSettings;
using static PostFXStack;

public class ColorLUTPass
{
    private static readonly ProfilingSampler sampler = new("Color LUT");

    private static readonly int colorGradingLUTId = Shader.PropertyToID("_ColorGradingLUT");
    private static readonly int colorGradingLUTParametersId = Shader.PropertyToID("_ColorGradingLUTParameters");
    private static readonly int colorGradingLUTInLogId = Shader.PropertyToID("_ColorGradingLUTInLogC");
    private static readonly int colorAdjustmentsId = Shader.PropertyToID("_ColorAdjustments");
    private static readonly int colorFilterId = Shader.PropertyToID("_ColorFilter");
    private static readonly int whiteBalanceId = Shader.PropertyToID("_WhiteBalance");
    private static readonly int splitToningShadowsId = Shader.PropertyToID("_SplitToningShadows");
    private static readonly int splitToningHighlightsIs = Shader.PropertyToID("_SplitToningHighlights");
    private static readonly int channelMixerRedId = Shader.PropertyToID("_ChannelMixerRed");
    private static readonly int channelMixerGreenId = Shader.PropertyToID("_ChannelMixerGreen");
    private static readonly int channelMixerBlueId = Shader.PropertyToID("_ChannelMixerBlue");
    private static readonly int smhShadowsId = Shader.PropertyToID("_SMHShadows");
    private static readonly int smhMidtonesId = Shader.PropertyToID("_SMHMidtones");
    private static readonly int smhHighlightsId = Shader.PropertyToID("_SMHHighlights");
    private static readonly int smhRangId = Shader.PropertyToID("_SMHRange");
    
   
    private static readonly GraphicsFormat colorFormat =
        SystemInfo.GetGraphicsFormat(DefaultFormat.HDR);

    private PostFXStack stack;
    private int colorLUTResolution;
    private TextureHandle colorLUT;
    
    void ConfigureColorAdjustments (CommandBuffer buffer, PostFXSettings settings)
    {
        ColorAdjustmentsSetting colorAdjustMents = settings.ColorAdjustments;
        buffer.SetGlobalVector(colorAdjustmentsId, new Vector4(
            Mathf.Pow(2f, colorAdjustMents.postExposure),
            colorAdjustMents.contrast * 0.01f + 1f,
            colorAdjustMents.hueShift * (1f / 360f),
            colorAdjustMents.saturation * 0.01f + 1f
        ));
        buffer.SetGlobalColor(colorFilterId, colorAdjustMents.colorFilter.linear);
    }
    void ConfigureWhiteBalance (CommandBuffer buffer, PostFXSettings settings)
    {
        WhiteBalanceSettings whiteBalance = settings.WhiteBalance;
        buffer.SetGlobalVector(whiteBalanceId, ColorUtils.ColorBalanceToLMSCoeffs(
            whiteBalance.temperature, whiteBalance.tint
        ));
    }
    void ConfigureSplitToning (CommandBuffer buffer, PostFXSettings settings)
    {
        SplitToningSettings splitToning = settings.SplitToning;
        Color splitColor = splitToning.shadowm;
        splitColor.a = splitToning.balance * 0.01f;
        buffer.SetGlobalColor(splitToningShadowsId, splitColor);
        buffer.SetGlobalColor(splitToningHighlightsIs, splitToning.highlights);
    }
    void ConfigureChannelMixer(CommandBuffer buffer, PostFXSettings settings)
    {
        ChannelMixerSettings channelMixer = settings.ChannelMixer;
        buffer.SetGlobalVector(channelMixerRedId, channelMixer.red);
        buffer.SetGlobalVector(channelMixerGreenId, channelMixer.green);
        buffer.SetGlobalVector(channelMixerBlueId, channelMixer.blue);
    }
    void ConfigureShadowsMidtonesHighlights (CommandBuffer buffer, PostFXSettings settings)
    {
        ShadowsMidtonesHighlightsSettings smh = settings.ShadowsMidtonesHighlights;
        buffer.SetGlobalColor(smhShadowsId, smh.shadows.linear);
        buffer.SetGlobalColor(smhMidtonesId, smh.midtones.linear);
        buffer.SetGlobalColor(smhHighlightsId, smh.highlights.linear);
        buffer.SetGlobalVector(smhRangId, new Vector4(
            smh.shadowStart, smh.shadowsEnd, smh.highlightsStart, smh.highlightsEnd
        ));
    }

    void Render(RenderGraphContext context)
    {
        PostFXSettings settings = stack.Settings;
        CommandBuffer buffer = context.cmd;
        
        ConfigureColorAdjustments(buffer, settings);
        ConfigureWhiteBalance(buffer, settings);
        ConfigureSplitToning(buffer, settings);
        ConfigureChannelMixer(buffer, settings);
        ConfigureShadowsMidtonesHighlights(buffer, settings);

        int lutHeight = colorLUTResolution;
        int lutWidth = lutHeight * lutHeight;
        
        buffer.SetGlobalVector(colorGradingLUTParametersId, new Vector4(
            lutHeight, 
            0.5f / lutWidth, 0.5f / lutHeight, 
            lutHeight / (lutHeight - 1f)
        ));
        ToneMappingSettings.Mode mode = settings.ToneMapping.mode;
        Pass pass = Pass.ColorGradingNone + (int)mode;
        buffer.SetGlobalFloat(
            colorGradingLUTInLogId, stack.BufferSettings.allowHDR && pass != Pass.ColorGradingNone ? 1f : 0f
        );
        stack.Draw(buffer, colorLUT, pass);
        buffer.SetGlobalVector(colorGradingLUTParametersId,
            new Vector4(1f / lutWidth, 1f / lutHeight, lutHeight - 1f)
        );
        buffer.SetGlobalTexture(colorGradingLUTId, colorLUT);
    }
    
    public static TextureHandle Record(
        RenderGraph renderGraph,
        PostFXStack stack,
        int colorLUTResolution)
    {
        using RenderGraphBuilder builder = renderGraph.AddRenderPass(
            sampler.name, out ColorLUTPass pass, sampler);
        pass.stack = stack;
        pass.colorLUTResolution = colorLUTResolution;

        int lutHeight = colorLUTResolution;
        int lutWidth = lutHeight * lutHeight;
        var desc = new TextureDesc(lutWidth, lutHeight)
        {
            colorFormat = colorFormat,
            name = "Color LUT"
        };
        pass.colorLUT = builder.WriteTexture(renderGraph.CreateTexture(desc));
        builder.SetRenderFunc<ColorLUTPass>(
            static (pass, context) => pass.Render(context));
        return pass.colorLUT;
    }
}