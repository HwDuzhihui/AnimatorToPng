using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AnimatorToPng : MonoBehaviour {

    public int FPS = 25;
    float m_DelayTime;
    float m_DelayTimeMax;
    List<Texture2D> m_TextureList = new List<Texture2D>();

    void Start () {
        m_DelayTimeMax = 1 / FPS;
        m_DelayTime = m_DelayTimeMax;

        Init();
	}
    void Update()
    {

        m_DelayTime -= Time.deltaTime;
        if(m_DelayTime < 0)
        {
            m_DelayTime = m_DelayTimeMax;
            StartCoroutine(PngStart());
        }
    }

    [ContextMenu("CameraInit")]
    void Init()
    {
        bool already = transform.GetComponentsInChildren<Camera>().Length > 0;
        if (already) return;

        GameObject o = new GameObject();
        o.name = "Camera";
        o.transform.SetParent(transform);
        o.transform.localScale = Vector3.one;
        o.transform.localPosition = Vector3.zero;
        Camera camera = o.AddComponent<Camera>();
        camera.orthographic = true;
        camera.nearClipPlane = -camera.farClipPlane / 2.0f;
        camera.clearFlags = CameraClearFlags.SolidColor;

    }

    IEnumerator PngStart()
    {

        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
        yield return new WaitForEndOfFrame();

        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenShot = GetCutTexture(screenShot);
        screenShot.Apply();

        m_TextureList.Add(screenShot);
    }
    void Save()
    {
        for(int i=0;i<m_TextureList.Count;i++)
        {
            string filename = Application.streamingAssetsPath + "/" + i + ".png";
            byte[] bytes = m_TextureList[i].EncodeToPNG();
            System.IO.File.WriteAllBytes(filename, bytes);
        }

        AssetDatabase.Refresh();
    }

    private void OnDestroy()
    {
        Save();
    }

    /// <summary>
    /// 裁剪边缘多余的图块
    /// </summary>
    /// <param name="tex"></param>
    /// <returns></returns>
    static Texture2D GetCutTexture(Texture2D tex)
    {
        int left = 0;
        int right = 0;
        int bottom = 0;
        int top = 0;

        bool complete = false;
        for (int i = 0; i < Screen.width; i++)
        {
            if (complete) break;
            for (int j = 0; j < Screen.height; j++)
            {
                Color c = tex.GetPixel(i, j);
                if (c.a != 0)
                {
                    left = i;
                    complete = true;
                    break;
                }
            }
        }
        complete = false;
        for (int i = Screen.width; i >= 0; i--)
        {
            if (complete) break;
            for (int j = 0; j < Screen.height; j++)
            {
                Color c = tex.GetPixel(i, j);
                if (c.a != 0)
                {
                    right = i;
                    complete = true;
                    break;
                }
            }
        }
        complete = false;
        for (int i = 0; i < Screen.height; i++)
        {
            if (complete) break;
            for (int j = 0; j < Screen.width; j++)
            {
                Color c = tex.GetPixel(j, i);
                if (c.a != 0)
                {
                    bottom = i;
                    complete = true;
                    break;
                }
            }
        }
        complete = false;
        for (int i = Screen.height; i >= 0; i--)
        {
            if (complete) break;
            for (int j = 0; j < Screen.width; j++)
            {
                Color c = tex.GetPixel(j, i);
                if (c.a != 0)
                {
                    top = i;
                    complete = true;
                    break;
                }
            }
        }

        int texWidth = right - left;
        int texHeight = top - bottom;

        Texture2D newTex = new Texture2D(texWidth,texHeight);

        for(int i = 0;i<tex.width;i++)
        {
            for(int j = 0;j< tex.height;j++)
            {
                bool x = i >= left && i <= right;
                bool y = j >= bottom && j <= top;

                if(x && y)
                {
                    int m = i - left;
                    int n = j - bottom;
                    newTex.SetPixel(m, n, tex.GetPixel(i, j));
                }

            }
        }

        return newTex;
    }
}
