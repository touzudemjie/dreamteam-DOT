using UnityEngine;

public class SetRenderTextureScript : MonoBehaviour
{
    [SerializeField] private Camera _cameraGreen;
    [SerializeField] private Material _cameraGreenMaterial;
    [SerializeField] private Camera _cameraRed;
    [SerializeField] private Material _cameraRedMaterial;
    void Start()
    {
        if(_cameraGreen.targetTexture != null)
        {
            _cameraGreen.targetTexture.Release();
            _cameraRed.targetTexture.Release();
        }
        _cameraGreen.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        _cameraGreenMaterial.mainTexture = _cameraGreen.targetTexture;
        _cameraRed.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        _cameraRedMaterial.mainTexture = _cameraRed.targetTexture;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
