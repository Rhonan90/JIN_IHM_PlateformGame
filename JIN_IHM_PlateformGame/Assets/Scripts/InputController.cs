using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] private LayerMask floorLayerMask;
    private BoxCollider2D boxCollider2d;

    private Vector2 position;
    private Vector2 speed;
    private Vector2 speedInput;

    private bool jump = false;
    private float timeSinceJump = 0;

    public float movementSpeed = 20;        //Vitesse du joueur
    public float inertia = 0.5f;            //Inertie du joueur
    public float jumpForce = 10 ;           //Vitesse du saut
    public float customGravity = -10;       //Force de gravite
    public float airTime = 0.2f;            //Temps en maximum dans les airs lors du saut

    private void Awake()
    {
        position = transform.position;
        boxCollider2d = transform.GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        getInput();

        speed = speedInput * movementSpeed + inertia * speed;  //simule l'accélération et une certaine inertie     

        if (IsGrounded()) //Incremente temps passé depuis l'appui de la touche de saut (0 si au sol)
            timeSinceJump = 0;
        else
            timeSinceJump += Time.deltaTime;
        
        if (jump && (IsGrounded()  || timeSinceJump < airTime) ) //Gestion vitesse verticale
            speed.y = jumpForce;
        else
            speed.y = customGravity;

        if (IsGrounded() && speed.y < 0)  //anti clipping dans le sol
            speed.y = 0;

        if (IsTouchingWall())  //anti clipping dans le sol
            speed.x = 0;

        position.x += speed.x * Time.deltaTime;  //horitontal movement
        position.y += speed.y * Time.deltaTime;  //vertical movement
        transform.position = position;  //update position
    }

    private void getInput()
    {
        speedInput = new Vector2(Input.GetAxis("Horizontal"), 0f);
        jump = Input.GetButton("Jump");
    }

    private bool IsGrounded()   //Utilisation du raycast pour detecter les collisions au sol
    {
        RaycastHit2D raycastHit = Physics2D.Raycast(boxCollider2d.bounds.center, Vector2.down, boxCollider2d.bounds.extents.y, floorLayerMask);

        Color rayColor; //Debut debuging
        if (raycastHit.collider != null)
        {
            rayColor = Color.green;
        }
        else
        {
            rayColor = Color.red;
        }
        Debug.DrawRay(boxCollider2d.bounds.center, Vector2.down * (boxCollider2d.bounds.extents.y), rayColor);
        Debug.Log(raycastHit.collider); //Fin debuging

        return raycastHit.collider != null;
    }

    private bool IsTouchingWall()   //Utilisation du raycast pour detecter les collisions au sol
    {
        float extraHeightText = .1f;
        RaycastHit2D raycastHitLeft = Physics2D.Raycast(boxCollider2d.bounds.center, Vector2.left, boxCollider2d.bounds.extents.x + extraHeightText, floorLayerMask);
        RaycastHit2D raycastHitRight = Physics2D.Raycast(boxCollider2d.bounds.center, Vector2.right, boxCollider2d.bounds.extents.x + extraHeightText, floorLayerMask);

        Color rayColorLeft; //Debut debuging
        Color rayColorRight;

        if (raycastHitLeft.collider != null)
            rayColorLeft = Color.green;
        else
            rayColorLeft = Color.red;

        if (raycastHitRight.collider != null)
            rayColorRight = Color.green;
        else
            rayColorRight = Color.red;

        Debug.DrawRay(boxCollider2d.bounds.center, Vector2.left * (boxCollider2d.bounds.extents.x + extraHeightText), rayColorLeft);
        Debug.DrawRay(boxCollider2d.bounds.center, Vector2.right * (boxCollider2d.bounds.extents.x + extraHeightText), rayColorRight);

        Debug.Log(raycastHitLeft.collider);
        Debug.Log(raycastHitRight.collider);//Fin debuging

        return raycastHitLeft.collider != null && raycastHitRight.collider != null;
    }
}
