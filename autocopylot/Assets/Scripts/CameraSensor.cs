using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CameraSensor : MonoBehaviour {
    public RenderTexture renderTexture;

    Texture2D texture;
    int textureWidth;
    int textureHeight;

    void Start() {
        textureWidth = renderTexture.width;
        textureHeight = renderTexture.height;
        texture = new Texture2D(textureWidth - 80, textureHeight - 80, TextureFormat.RGB24, false);
    }

    private void FixedUpdate() {
        UpdateUiTexture();
    }

    void UpdateUiTexture() {
        RawImage rawImg = GameObject.Find("RawImage").GetComponent<RawImage>();
        
        Texture2D camera_texture = GetCameraView();
        Texture2D ui_texture = new Texture2D(camera_texture.width, camera_texture.height, TextureFormat.RGB24, false);

        ui_texture.SetPixels(camera_texture.GetPixels());
        ui_texture.Apply();

        rawImg.texture = ui_texture;
    }

    Texture2D GetCameraView() {
        var currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        texture.ReadPixels(new Rect(40, 40, textureWidth - 40, textureHeight - 40), 0, 0);
        RenderTexture.active = currentRT;

        return texture;
    }


    public void SaveImage(string path) {
        Texture2D img = GetCameraView();
        byte[] bytes = img.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
    }
}
