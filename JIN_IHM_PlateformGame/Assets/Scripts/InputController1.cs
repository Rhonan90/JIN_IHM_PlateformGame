using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController1 : MonoBehaviour
{
    private Collider2D collider2d;

    private Vector2 position;
    private Vector2 speed;
    private Vector2 acceleration;
    private Vector2 inputs;

    private bool sprint = false;
    private bool jump = false;
    private bool jumping = false;
    private bool dash = false;
    private bool dashing = false;
    private bool canDash = false;
    private bool touchingFloor;              //Test si mur en dessous pour les collisions
    private bool canDoubleJump = false;
    private float timeOnFloor = 0;
    private float timeAfterFirstJump = -1000;

    public float movementAcceleration = 20;        //Vitesse du joueur
    public float maxMovementSpeed = 10;     //Vitesse maximale du joueur
    public float inertiaFactor = 0.9f;     //Facteur inertielle [0,1]
    public float mass = 20;                 //Masse du joueur
    public float jumpForce = 10 ;           //Force du saut
    public float dashForce = 10;            //Force/vitesse du dash
    public float dashDuration = 0.2f;            //Durée du dash
    public float customGravity = -10;       //Force de gravite
    public float airTime = 0.2f;            //Temps en maximum de saut (nuancier)
    public float timeBeforeRejumpInAir = 0.2f;         //Temps avant de pouvoir resauter en l'air
    public float timeBeforeRejumpOnFloor = 0.2f;       //Temps avant de pouvoir resauter au sol                                                   
    public float sprintVelocityModifier = 1.2f;   //Multiplicateur de vitesse en sprintant
    public float sprintJumpModifier = 1.2f;       //Multiplicateur de force de saut en srprintant


    private void Awake()
    {
        position = transform.position;
        collider2d = transform.GetComponent<Collider2D>();
        speed = new Vector2();
    }

    private void Update()
    {
        getInput();

        acceleration = (inputs + new Vector2((dashing ? dashForce : 0), (!dashing ?  (jumping ? jumpForce : customGravity) : 0) )) / mass;

        speed.x =  acceleration.x * Time.deltaTime * movementAcceleration + inertiaFactor * speed.x; // simule une certaine inertie  
        speed.y = acceleration.y * Time.deltaTime; 

        speed.x = Mathf.Clamp(speed.x, -maxMovementSpeed, maxMovementSpeed);  // on limite la vitesse maximale du joueur

        if (sprint && !sprint)  //désactivé pour l'instant
        {
            speed.x *= sprintVelocityModifier;  //TODO : fix
        }

        touchingFloor = (CheckCollisions(collider2d, new Vector2(0, speed.y).normalized, Mathf.Abs(speed.y) * Time.deltaTime)) ; //test si on touche le sol

        //Gestion du saut et de la chute//
        if (touchingFloor) //Incremente temps passé depuis l'appui de la touche de saut (0 si au sol)
        {   
            timeOnFloor += Time.deltaTime;
            canDash = true;
        }
        else
        {   
            timeOnFloor = 0;
            timeAfterFirstJump += Time.deltaTime;
        }

        if (jump && touchingFloor && !jumping && timeOnFloor > timeBeforeRejumpOnFloor)
        {
            StartCoroutine("JumpCoroutine");
        }
        else if (jump && !touchingFloor && !jumping && canDoubleJump && timeAfterFirstJump > timeBeforeRejumpInAir)
        {
            StartCoroutine("DoubleJumpCoroutine");
        }

        if(dash && !dashing && canDash && !touchingFloor) //Le joueur peut dash dans les airs une fois, le saut permet de dash à nouveau
        {
            StartCoroutine("DashCoroutine");
        }

        //Gestion des collisions et du déplacement//
        if (!CheckCollisions(collider2d, new Vector2(speed.x, 0).normalized, Mathf.Abs(speed.x) * Time.deltaTime))  //test pour vérifier qu'on entre pas dans un mur    /!\ TODO : Mathf.Abs(speed.x) * Time.deltaTime) à affiner
            position.x += speed.x * Time.deltaTime;  //horitontal movement
        if (!CheckCollisions(collider2d, new Vector2(0, speed.y).normalized, Mathf.Abs(speed.y) * Time.deltaTime)) //test pour vérifier qu'on entre pas dans le sol ou le plafond    /!\ TODO : Mathf.Abs(speed.y) * Time.deltaTime) à affiner
            position.y += speed.y * Time.deltaTime;  //vertical movement
        transform.position = position;  //update position
    }

    private void getInput()
    {
        inputs = new Vector2(Input.GetAxis("Horizontal"), 0f);
        jump = Input.GetButton("Jump");
        sprint = Input.GetButton("Sprint");
        dash = Input.GetButton("Dash");
    }

    private bool CheckCollisions(Collider2D moveCollider, Vector2 direction, float distance)
    {
        if (moveCollider!=null)
        {
            RaycastHit2D[] hits = new RaycastHit2D[10];
            ContactFilter2D filter = new ContactFilter2D() { };

            int numHits = moveCollider.Cast(direction, filter, hits, distance);

            for (int i = 0; i < numHits; i++)
            {
                if (!hits[i].collider.isTrigger)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private IEnumerator JumpCoroutine()
    {
        jumping = true;
        canDash = true;
        yield return new WaitForSeconds(airTime);
        jumping = false;
        canDoubleJump = true;
        timeAfterFirstJump = 0;
        
    }

    private IEnumerator DoubleJumpCoroutine()
    {
        jumping = true;
        canDash = true;
        canDoubleJump = false;
        yield return new WaitForSeconds(airTime);
        jumping = false;
    }

    private IEnumerator DashCoroutine()     
    {
        float dashDirection = Mathf.Sign(speed.x);
        dashForce = dashDirection * Mathf.Abs(dashForce);
        dashing = true;
        canDash = false;
        yield return new WaitForSeconds(dashDuration);
        dashing = false;
    }
}
