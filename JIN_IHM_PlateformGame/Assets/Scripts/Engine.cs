using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    public Vector2 speed;
    public Vector2 position;

    private Vector2 lastSpeedVector;

    void Start()
    {
        position.x = transform.position.x;
    }

    void Update()
    {
        position.x += speed.x * Time.deltaTime;
        position.y = transform.position.y;
        transform.position = position;
        lastSpeedVector = speed;
    }
}
