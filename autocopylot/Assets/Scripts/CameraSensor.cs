using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using System;

public class CameraSensor : MonoBehaviour
{
    public RenderTexture renderTexture;
    Texture2D texture;
    int textureWidth;
    int textureHeight;

    void Start()
    {
        textureWidth = renderTexture.width;
        textureHeight = renderTexture.height;
        texture = new Texture2D(textureWidth - 80, textureHeight - 80, TextureFormat.RGB24, false);
    }

    Texture2D GetImage()
    {
        var currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        texture.ReadPixels(new Rect(40, 40, textureWidth - 40, textureHeight - 40), 0, 0);

        RenderTexture.active = currentRT;
        return texture;
    }


    public void SaveImage(string path)
    {
        Texture2D img = GetImage();
        byte[] bytes = img.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
    }
}
