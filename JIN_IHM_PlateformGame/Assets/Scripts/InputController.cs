using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum playerState
{
    Grounded,
    Flying
};

public class InputController : MonoBehaviour
{
    private Vector2 position;
    private Vector2 speed;
    private Vector2 speedInput;

    private bool jump = false;
    private float airTime = 10;

    public float movementSpeed = 20;
    public float inertia = 0.95f;
    public float jumpForce = 10 ;
    public float customGravity = -10;

    playerState state = playerState.Flying;

    private void Awake()
    {
        position = transform.position;

        this.gameObject.SetActive(false);
        
    }

    private void Update()
    {
        getInput();

        speed = speedInput * Time.deltaTime * movementSpeed + inertia * speed; ;  //simule l'accélération et une certaine inertie     

        airTime += Time.deltaTime;
        if (jump && (state == playerState.Grounded  || airTime < 0.3) )
        {
            speed.y = jumpForce * Time.deltaTime;
        }
        else
        {
            speed.y = customGravity * Time.deltaTime;
        }

        if (state == playerState.Grounded && speed.y < 0)  //anti clipping dans le sol
            speed.y = 0;

        position.x += speed.x * Time.deltaTime;  //horitontal movement
        position.y += speed.y * Time.deltaTime;  //vertical movement
        transform.position = position;  //update position

        Debug.Log(state);
    }

    private void getInput()
    {
        speedInput = new Vector2(Input.GetAxis("Horizontal"), 0f);
        jump = Input.GetButton("Jump");
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            state = playerState.Grounded;
            airTime = 0;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            state = playerState.Flying;
    }

}
