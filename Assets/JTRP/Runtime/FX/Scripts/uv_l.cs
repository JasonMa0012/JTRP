using UnityEngine;
using System.Collections;

public class uv_l : MonoBehaviour 
{
    public float scrollSpeed;
    //public Texture uvtexture;//放2d圖
    void Update()
    {
        float offset = Time.time * scrollSpeed;
        GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(-offset, 0f));
    }
}


