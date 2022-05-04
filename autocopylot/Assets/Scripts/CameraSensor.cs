using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using System;

public class CameraSensor : MonoBehaviour
{
    public RenderTexture renderTexture;
    Texture2D tex;
    int texwidth;
    int texheight;

    void Start()
    {
        texwidth = renderTexture.width;
        texheight = renderTexture.height;
        tex = new Texture2D(texwidth - 80, texheight - 80, TextureFormat.RGB24, false);
    }

    Texture2D GetImage()
    {
        var currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        tex.ReadPixels(new Rect(40, 40, texwidth - 40, texheight - 40), 0, 0);

        RenderTexture.active = currentRT;
        return tex;
    }


    public void SaveImage(string path)
    {
        Texture2D img = GetImage();
        byte[] bytes = img.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
    }
}
