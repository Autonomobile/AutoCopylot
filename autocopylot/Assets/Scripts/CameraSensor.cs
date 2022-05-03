using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using System;

public class CameraSensor : MonoBehaviour
{
    static string saveFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/collect/";
    static System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

    public bool doSave = true;
    public RenderTexture renderTexture;
    Texture2D tex;


    Texture2D GetImage()
    {
        var currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        int texwidth = renderTexture.width;
        int texheight = renderTexture.height;

        tex = new Texture2D(texwidth - 80, texheight - 80, TextureFormat.RGB24, false);
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

    public void Update()
    {
        if (doSave)
            SaveImage(saveFolder + GetCurrentTime() + ".png");
    }

    public static string GetCurrentTime()
    {
        return (System.DateTime.UtcNow - epochStart).TotalSeconds.ToString().Replace(",", ".");
    }
}
