using System;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent, RequireComponent(typeof(Camera))]
public class CustomRenderPipelineCamera : MonoBehaviour
{
    [SerializeField]
    CameraSettings settings = default;

    private ProfilingSampler _sampler;
    public ProfilingSampler Sampler => _sampler ??= new (GetComponent<Camera>().name);
    public CameraSettings Settings => settings ?? new();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void OnEnable() => _sampler = null;
#endif
}
