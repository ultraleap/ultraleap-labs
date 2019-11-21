using UnityEngine;

/// <summary>
/// This script will obtain a number of variables from each texture that gets passed to the "Texture Objects" array in
/// the HapticRenderer class. Each texture must contain this script in order for the haptic rendering process to work 
/// appropriately. Attributes can be updated via the "Update Attributes button from within the Unity Editor.
/// </summary>

public class TextureAttributes : MonoBehaviour
{

    [SerializeField] [ReadOnly] [Tooltip("The image's height map being used for haptic texture rendering.")] private Texture2D _heightMap;
    public Texture2D HeightMap { get { return _heightMap; } set { _heightMap = value; } }

    [SerializeField] [ReadOnly] [Tooltip("The value set for how smooth the texture should be.")] private float _hapticSmoothness;
    public float Smoothness { get { return _hapticSmoothness; } set { _hapticSmoothness = value; } }

    [SerializeField] [ReadOnly] [Tooltip("The minimum intensity setting for haptic bumpiness. Match value to Max Intensity for flat texture.")] private float _minIntensity;
    public float MinIntensity { get { return _minIntensity; } set { _minIntensity = value; } }

    [SerializeField] [ReadOnly] [Tooltip("The maximum intensity setting for haptic bumpiness.")] private float _maxIntensity;
    public float MaxIntensity { get { return _maxIntensity; } set { _maxIntensity = value; } }

    [SerializeField] [ReadOnly] [Tooltip("The size of the texture height map.")] private Vector3 _heightMapSize;
    public Vector3 HeightMapSize { get { return _heightMapSize; } set { _heightMapSize = value; } }

    [SerializeField] [ReadOnly] [Tooltip("The position of the texture height map.")] private Vector3 _heightMapPos;
    public Vector3 HeightMapPos { get { return _heightMapPos; } set { _heightMapPos = value; } }

    [SerializeField] [ReadOnly] [Tooltip("The minimum bound position of the texture height map.")] private Vector3 _minBounds;
    public Vector3 MinBounds { get { return _minBounds; } set { _minBounds = value; } }

    [SerializeField] [ReadOnly] [Tooltip("The maximum bound position of the texture height map.")] private Vector3 _maxBounds;
    public Vector3 MaxBounds { get { return _maxBounds; } set { _maxBounds = value; } }

    private void OnEnable()
    {
        UpdateTexture();
    }

    private void UpdateTexture()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        HeightMap = meshRenderer.sharedMaterial.GetTexture("_DispTex") as Texture2D;
        Smoothness = meshRenderer.sharedMaterial.GetFloat("_HapticSmoothness");
        MinIntensity = meshRenderer.sharedMaterial.GetFloat("_HapticIntensityMinimum");
        MaxIntensity = meshRenderer.sharedMaterial.GetFloat("_HapticIntensityMaximum");
        HeightMapSize = meshRenderer.bounds.size;
        HeightMapPos = meshRenderer.transform.position;
        MinBounds = GetMinBounds(HeightMapSize, HeightMapPos);
        MaxBounds = GetMaxBounds(HeightMapSize, HeightMapPos);
    }

    private Vector3 GetMinBounds(Vector3 _heightMapSize, Vector3 _heightMapPos)
    {
        return new Vector3(_heightMapPos.x - _heightMapSize.x / 2, 0, _heightMapPos.z - _heightMapSize.z / 2);
    }

    private Vector3 GetMaxBounds(Vector3 _heightMapSize, Vector3 _heightMapPos)
    {
        return new Vector3(_heightMapPos.x + _heightMapSize.x / 2, 0, _heightMapPos.z + _heightMapSize.z / 2);
    }

#if UNITY_EDITOR

    public void ForceUpdateTexture()
    {
        UpdateTexture();
    }

#endif
}
