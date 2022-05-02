using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class CameraSensor : MonoBehaviour
{
    public RenderTexture renderTexture;

    Texture2D tex;


    Texture2D GetImage()
    {
        var currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        int texwidth = renderTexture.width;
        int texheight = renderTexture.height;

        tex = new Texture2D(texwidth, texheight, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, texwidth, texheight), 0, 0);

        RenderTexture.active = currentRT;
        return tex;
    }


    public void SaveImage(string path)
    {
        Texture2D img = GetImage();
        byte[] bytes = img.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
    }

    public void Update()
    {		
        SaveImage("../data/test.png");
    }
}
