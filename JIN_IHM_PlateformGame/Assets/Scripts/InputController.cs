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
    private Vector3 speedIncrement;

    private bool sprint = false;
    private bool jump = false;
    private bool canDoubleJump = false;
    private float timeSinceJump = 0;

    public float movementSpeed = 20;        //Vitesse du joueur
    public float inertia = 0.5f;            //Inertie du joueur
    public float jumpForce = 10 ;           //Vitesse du saut
    public float customGravity = -10;       //Force de gravite
    public float airTime = 0.2f;            //Temps en maximum dans les airs lors du saut
    public float sprintVelocityModifier = 1.2f;   //Multiplicateur de vitesse en sprintant
    public float sprintJumpModifier = 1.2f;       //Multiplicateur de force de saut en srprintant

    private bool touchingRight;             //Test si mur à droite pour les collisions
    private bool touchingLeft;              //Test si mur à gauche pour les collisions
    private bool touchingTop;               //Test si mur au dessus pour les collisions
    private bool touchingDown;              //Test si mur en dessous pour les collisions

    private float extraRaycastCheck = 0.1f;


    private void Awake()
    {
        position = transform.position;
        boxCollider2d = transform.GetComponent<BoxCollider2D>();
        speed = new Vector2();
        speedIncrement = new Vector3();
    }

    private void Update()
    {
        getInput();


        speed =  speedInput * movementSpeed; // + inertia * Time.deltaTime * new Vector2(speed.x,0) ;   //simule une certaine inertie     //TODO : fix inertia : idea detlatime

        if (sprint)
        {
            speed *= sprintVelocityModifier;  //
        }

        if (IsGrounded()) //Incremente temps passé depuis l'appui de la touche de saut (0 si au sol)
        {
            timeSinceJump = 0;
            canDoubleJump = true; //resest du double saut quand on touche le sol
        }
        else
            timeSinceJump += Time.deltaTime;


        if (jump && (touchingDown || timeSinceJump < airTime))
        {
            speed.y = jumpForce * (sprint ? sprintJumpModifier : 1);   //On saute plus haut si on sprint
        }
        else if (jump && (timeSinceJump > airTime) && canDoubleJump)
        {
            timeSinceJump = 0;
            canDoubleJump = false;
        }
        else
            speed.y += customGravity * Time.deltaTime;    //Le cube tombe  //TODO : à fix

        IsGrounded();
        if ((touchingDown && speed.y < 0) && !(jump))  //collision en bas
        {
            speed.y = 0;
        }

        IsTouchingWall();
        if (touchingLeft && speed.x < 0 || touchingRight && speed.x > 0)  //collision à gauche
        {
            speed.x = 0;
        }

        IsTouchingCeiling();
        if (touchingTop && speed.y > 0) //collision en haut
        {
            speed.y = 0;
        }        


        position.x += speed.x * Time.deltaTime;  //horitontal movement
        position.y += speed.y * Time.deltaTime;  //vertical movement
        speedIncrement = new Vector3(speed.x * Time.deltaTime, speed.y * Time.deltaTime, 0);  //sert à caster en avance pour les collisions
        transform.position = position;  //update position
    }

    private void getInput()
    {
        speedInput = new Vector2(Input.GetAxis("Horizontal"), 0f);
        jump = Input.GetButton("Jump");
        sprint = Input.GetButton("Sprint");
    }

    private bool IsGrounded()   //Utilisation du raycast pour detecter les collisions au sol
    {
        RaycastHit2D raycastHitDown = Physics2D.Raycast(boxCollider2d.bounds.min - new Vector3(0, extraRaycastCheck, 0) + speedIncrement, Vector2.right, boxCollider2d.bounds.size.x, floorLayerMask);

        Color rayColor; //Debut debuging
        if (raycastHitDown.collider != null)
        {
            rayColor = Color.green;
        }
        else
        {
            rayColor = Color.red;
        }
        Debug.DrawRay(boxCollider2d.bounds.min - new Vector3(0, extraRaycastCheck, 0), Vector2.right * boxCollider2d.bounds.size.x, rayColor);
        Debug.Log(raycastHitDown.collider); //Fin debuging

        touchingDown = raycastHitDown.collider != null ;

        return touchingDown;
    }

    private bool IsTouchingWall()   //Utilisation du raycast pour detecter les collisions au sol
    {
        RaycastHit2D raycastHitLeft = Physics2D.Raycast(boxCollider2d.bounds.min - new Vector3(extraRaycastCheck, 0, 0) + speedIncrement, Vector2.up, boxCollider2d.bounds.size.y, floorLayerMask);
        RaycastHit2D raycastHitRight = Physics2D.Raycast(boxCollider2d.bounds.max + new Vector3(extraRaycastCheck, 0, 0) + speedIncrement, Vector2.down, boxCollider2d.bounds.size.y, floorLayerMask);

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

        Debug.DrawRay(boxCollider2d.bounds.min - new Vector3(extraRaycastCheck, 0, 0), Vector2.up * (boxCollider2d.bounds.size.y), rayColorLeft);
        Debug.DrawRay(boxCollider2d.bounds.max + new Vector3(extraRaycastCheck, 0, 0), Vector2.down * (boxCollider2d.bounds.size.y), rayColorRight);

        Debug.Log(raycastHitLeft.collider);
        Debug.Log(raycastHitRight.collider);//Fin debuging

        touchingLeft = raycastHitLeft.collider != null;
        touchingRight = raycastHitRight.collider != null;

        return touchingLeft || touchingRight;
    }

    private bool IsTouchingCeiling()   //Utilisation du raycast pour detecter les collisions au sol
    {
        RaycastHit2D raycastHitTop = Physics2D.Raycast(boxCollider2d.bounds.max + new Vector3(0, extraRaycastCheck, 0) + speedIncrement, Vector2.left, boxCollider2d.bounds.size.x, floorLayerMask);

        Color rayColor; //Debut debuging
        if (raycastHitTop.collider != null)
        {
            rayColor = Color.green;
        }
        else
        {
            rayColor = Color.red;
        }
        Debug.DrawRay(boxCollider2d.bounds.max + new Vector3(0, extraRaycastCheck, 0), Vector2.left * boxCollider2d.bounds.size.x, rayColor);
        Debug.Log(raycastHitTop.collider); //Fin debuging

        touchingTop = raycastHitTop.collider != null;
        return touchingTop;
    }

}
