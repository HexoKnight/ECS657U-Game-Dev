using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : MonoBehaviour
{

    public float X_offset_speed = 0.2f;
    public float Y_offset_speed = 0.1f;

    private Vector2 offset = new Vector2(0f,0f);

    float scrollSpeed = 0.5f;
    Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        offset += new Vector2(X_offset_speed,Y_offset_speed) * Time.deltaTime;
        rend.material.mainTextureOffset = offset;

    }
}
