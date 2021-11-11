using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
public class WebRequestDownloader : MonoBehaviour
{
    public static IEnumerator DownloadImage(string url, Action<Sprite> callback)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);

        DownloadHandler handle = www.downloadHandler;

        Debug.Log(url);
        //Send Request and wait
        yield return www.SendWebRequest();

        if (www.isHttpError || www.isNetworkError)
        {
            UnityEngine.Debug.Log("Error while Receiving: " + www.error);
        }
        else
        {
            UnityEngine.Debug.Log("Success");

            //Load Image
            Texture2D texture2d = new Texture2D(8, 8);
            Sprite sprite = null;
            if (texture2d.LoadImage(handle.data))
            {
                sprite = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), Vector2.zero);
            }
            if (sprite != null)
            {
                callback(sprite);
            }
        }
    }
}
