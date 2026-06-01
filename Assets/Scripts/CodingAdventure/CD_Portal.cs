using UnityEngine;

public class CD_Portal : MonoBehaviour
{
    public CD_Portal _linkedPortal;
    public MeshRenderer screen;
    private Camera _playerCamera;
    private Camera _portalCamera;
    private RenderTexture _viewTexture;

    private void Awake()
    {
        _playerCamera = Camera.main;
       
    }
    void Start()
    {
        
    }
    private void CreateViewTexture()
    {
        if (_viewTexture == null || _viewTexture.width != Screen.width || _viewTexture.height != Screen.height)
        {
            _viewTexture.Release();
        }
        _viewTexture = new RenderTexture(Screen.width, Screen.height, 0);
        _portalCamera.targetTexture = _viewTexture;
        _linkedPortal.screen.material.SetTexture("_MainTex", _viewTexture);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
