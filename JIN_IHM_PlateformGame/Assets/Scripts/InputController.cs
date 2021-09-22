using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private Vector2 speed;
    private Vector2 position;

    public Vector2 maxSpeed;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 speedInput = new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));

        speed = maxSpeed * speedInput;  // ~ (1 - a) * speed + a * speedInput pour simuler un peu d'inertie

        position.x += speed.x * Time.deltaTime;
        position.y = transform.position.y;
        transform.position = position;

    }
}
