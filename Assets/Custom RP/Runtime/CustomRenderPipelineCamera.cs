using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent, RequireComponent(typeof(Camera))]
public class CustomRenderPipelineCamera : MonoBehaviour
{
    [SerializeField]
    CameraSettings settings = default;

    private ProfilingSampler sampler;
    public ProfilingSampler Sampler => sampler ??= new (GetComponent<Camera>().name);
    public CameraSettings Settings => settings ?? new();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void OnEnable() => sampler = null;
#endif
}
