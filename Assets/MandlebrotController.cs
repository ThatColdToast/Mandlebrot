using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MandlebrotController : MonoBehaviour
{
    [Header("General")]
    public ComputeShader SimulationShader;
    public Vector2Int currentTextureSize;
    public Vector2 _JuliaOffset;

    // private Vector2 

    [Header("Render Settings")]
    // public Vector2 Scale;
    // public Vector2 Offset;
    public Vector2 _offset;
    public float _zoom;
    public bool _mandlebrot;
    
    private bool locked;

    private Camera _camera;
    private RenderTexture _Image;

    // public int _currentGeneration = 0;

    private Material _addMaterial;

    private void Awake()
    {
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;
        locked = false;
        _mandlebrot = true;

        _camera = GetComponent<Camera>();

        currentTextureSize.x = Screen.width;
        currentTextureSize.y = Screen.height;

        // Scale = new Vector2(1, 1);
        // Offset = new Vector2(0, 0);

        #if UNITY_EDITOR
            QualitySettings.vSyncCount = 0;  // VSync must be disabled
            Application.targetFrameRate = 200;
        #endif

        _zoom = 4;
        _offset = new Vector2(0f, 0f);
    }

    private void OnEnable()
    {
        SetUpScene();
    }

    public float movement = -0.01f;

    private void Update()
    {
        // Update Controls
        _zoom -= (Input.mouseScrollDelta.y * _zoom) * 0.05f;
        // _zoom = Mathf.Lerp(_zoom, _zoom - Input.mouseScrollDelta.y, .01f);

        if(Input.GetMouseButtonDown(0))
        {
            locked = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if(Input.GetMouseButton(1))
        {
            if(locked)
                _JuliaOffset = _offset;
            else
            {
                locked = true;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        if(Input.GetButtonDown("Switch Sets") && locked)
        {
            _mandlebrot = !_mandlebrot;
        }

        if(Input.GetButtonDown("Cancel"))
        {
            locked = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if(locked)
        {
            Vector2 tmp = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            tmp = tmp * _zoom * movement;
            _offset += tmp;
        }

        // if(Input.GetMouseButtonUp(1) && locked)
        // {
        //     locked = false;
        // }

        // Screen Size has been changed
        if (currentTextureSize.x != Screen.width || currentTextureSize.y != Screen.height)
        {
            currentTextureSize.x = Screen.width;
            currentTextureSize.y = Screen.height;
        }

        // Make sure we have a current render target
        InitRenderTexture();

        SetShaderParameters();

        // Debug.Log("Gen: " + _currentGeneration);

        // Set the target and dispatch the compute shader
        SimulationShader.SetTexture(0, "Image", _Image);
        // SimulationShader.SetTexture(0, "Result", _Result);
        // SimulationShader.SetTexture(0, "Kernel", _Kernel);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        SimulationShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        // Graphics.CopyTexture(_Result, _Start);
        // _currentGeneration++;
    }

    private void OnDisable()
    {
        // if (_sphereBuffer != null)
        //     _sphereBuffer.Release();
        
        // if (_triangleBuffer != null)
        //     _triangleBuffer.Release();
    }

    private void SetUpScene()
    {
        // Assign to compute buffer
        // _sphereBuffer = new ComputeBuffer(spheres.Count, sizeof(float) * 10); // 40);
        // _sphereBuffer.SetData(spheres);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Render(destination);
    }

    private void Render(RenderTexture destination)
    {
        InitRenderTexture();
        
        // Graphics.Blit(_Image, destination, Scale, Offset);
        Graphics.Blit(_Image, destination);
    }

    private void InitRenderTexture()
    {
        int textureScale = 1;
        if (_Image == null || _Image.width != Screen.width / textureScale || _Image.height != Screen.height / textureScale)
        // if (_Start == null || _Start.width != targetTextureSize.x || _Start.height != targetTextureSize.y)
        {
            // Release render texture if we already have one
            if (_Image != null)
                _Image.Release();



            _Image = new RenderTexture(Screen.width / textureScale, Screen.height / textureScale, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            // _Start = new RenderTexture(targetTextureSize.x, targetTextureSize.y, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _Image.enableRandomWrite = true;
            _Image.Create();
        }
    }

    private void SetShaderParameters()
    {
        // SimulationShader.SetInt("currentGeneration", _currentGeneration);
        SimulationShader.SetFloat("zoom", _zoom);
        SimulationShader.SetVector("offset", _offset);

        SimulationShader.SetVector("JuliaOffset", _JuliaOffset);
        SimulationShader.SetBool("mandlebrot", _mandlebrot);
        
    }
}
