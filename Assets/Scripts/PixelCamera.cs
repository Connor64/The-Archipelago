using UnityEngine;

[ExecuteInEditMode]
public class PixelCamera : MonoBehaviour {
    public int virtualWidth = 720;
    private int virtualHeight;

    public Camera cam;

    protected void Start() {
    	cam = GetComponent<Camera>();
    	
        float ratio = ((float)cam.pixelHeight / (float)cam.pixelWidth);
        virtualHeight = Mathf.RoundToInt(virtualWidth * ratio);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        source.filterMode = FilterMode.Point;
        RenderTexture buffer = RenderTexture.GetTemporary(virtualWidth, virtualHeight, -1);
        buffer.filterMode = FilterMode.Point;
        Graphics.Blit(source, buffer);
        Graphics.Blit(buffer, destination);
        RenderTexture.ReleaseTemporary(buffer);
    }
}