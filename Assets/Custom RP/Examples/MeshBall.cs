using UnityEngine;
using UnityEngine.Rendering;
public class MeshBall : MonoBehaviour {

	const int BALL_COUNT_MAX = 1023;
	static int
		baseColorId = Shader.PropertyToID("_BaseColor"),
		metallicId = Shader.PropertyToID("_Metallic"),
		smoothnessId = Shader.PropertyToID("_Smoothness");

	[SerializeField]
	Mesh mesh = default;

	[SerializeField]
	Material material = default;

	[SerializeField] 
	LightProbeProxyVolume lightProbeVolume = null;
	
	Matrix4x4[] matrices = new Matrix4x4[BALL_COUNT_MAX];
	Vector4[] baseColors = new Vector4[BALL_COUNT_MAX];
	float[]
		metallic = new float[BALL_COUNT_MAX],
		smoothness = new float[BALL_COUNT_MAX];

	MaterialPropertyBlock block;

	void Awake () {
		for (int i = 0; i < matrices.Length; i++) {
			matrices[i] = Matrix4x4.TRS(
				Random.insideUnitSphere * 10f,
				Quaternion.Euler(
					Random.value * 360f, Random.value * 360f, Random.value * 360f
				),
				Vector3.one * Random.Range(0.5f, 1.5f)
			);
			baseColors[i] =
				new Vector4(
					Random.value, Random.value, Random.value,
					Random.Range(0.5f, 1f)
				);
			metallic[i] = Random.value < 0.25f ? 1f : 0f;
			smoothness[i] = Random.Range(0.05f, 0.95f);
		}
	}

	void Update () {
		if (block == null) {
			block = new MaterialPropertyBlock();
			block.SetVectorArray(baseColorId, baseColors);
			block.SetFloatArray(metallicId, metallic);
			block.SetFloatArray(smoothnessId, smoothness);

			if (!lightProbeVolume)
			{
				var postions = new Vector3[BALL_COUNT_MAX];
				for (int i = 0; i < matrices.Length; i++)
				{
					postions[i] = matrices[i].GetColumn(3);
				}
				var lightProbes = new SphericalHarmonicsL2[BALL_COUNT_MAX];
				var occlusionProbes = new Vector4[BALL_COUNT_MAX];
				LightProbes.CalculateInterpolatedLightAndOcclusionProbes(postions, lightProbes, occlusionProbes);
				block.CopySHCoefficientArraysFrom(lightProbes);
				block.CopyProbeOcclusionArrayFrom(occlusionProbes);
			}
		}
		Graphics.DrawMeshInstanced(
			mesh, 0, material, matrices, BALL_COUNT_MAX, block,
			ShadowCastingMode.On, true, 0, null, 
			lightProbeVolume ? LightProbeUsage.UseProxyVolume : LightProbeUsage.CustomProvided,
			lightProbeVolume
		);
	}
}