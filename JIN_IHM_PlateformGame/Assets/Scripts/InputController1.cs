using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController1 : MonoBehaviour
{
    private Collider2D collider2d;
    public ParticleSystem jumpEffect;
    public ParticleSystem rightWallFrictionEffect;
    public ParticleSystem leftWallFrictionEffect;

    private Vector2 position;
    private Vector2 speed;
    private Vector2 acceleration;
    private Vector2 inputs;

    public int target = 150;

    private bool sprint = false;
    private bool jump = false;
    private bool jumping = false;
    private bool wallJumping = false;
    private float wallJumpDirection;
    private bool dash = false;
    private bool dashing = false;
    private bool canDash = false;
    private bool touchingFloor;              //Test si mur en dessous pour les collisions
    private bool touchingWall;
    private bool canDoubleJump = false;
    private bool wallJumpedFromRight = false;
    private bool wallJumpedFromLeft = false;
    private float timeOnFloor = 0;
    private float timeAfterFirstJump = -1000;

    public float movementAcceleration = 20;        //Vitesse du joueur
    public float maxMovementSpeed = 10;     //Vitesse maximale du joueur
    public float inertiaFactor = 0.9f;     //Facteur inertielle [0,1]
    public float mass = 20;                 //Masse du joueur
    public float jumpForce = 10 ;           //Force du saut
    public float wallJumpForce = 10;           //Force du saut au mur
    public float wallJumpDuration = 0.2f;     //Dur�e du saut au mur
    public float dashForce = 10;            //Force/vitesse du dash
    public float dashDuration = 0.2f;            //Dur�e du dash
    public float customGravity = -10;       //Force de gravite
    public float wallResistance = 1.4f;
    public float airTime = 0.2f;            //Temps en maximum de saut (nuancier)
    public float airSpeedMultiplier = 0.5f;  //Multiplicateur de vitesse du joueur lorsqu'il est en l'air
    public float timeBeforeRejumpInAir = 0f;         //Temps avant de pouvoir resauter en l'air
    public float timeBeforeRejumpOnFloor = 0f;       //Temps avant de pouvoir resauter au sol                                                   
    public float sprintVelocityModifier = 1.2f;   //Multiplicateur de vitesse en sprintant
    public float sprintJumpModifier = 1.2f;       //Multiplicateur de force de saut en srprintant


    private void Awake()
    {
        position = transform.position;
        collider2d = transform.GetComponent<Collider2D>();
        speed = new Vector2();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = target;
    }

    private void Update()
    {
        if (Application.targetFrameRate != target)
            Application.targetFrameRate = target;

        getInput();

        acceleration = (inputs * (sprint ? sprintVelocityModifier : 1) + new Vector2((dashing ? dashForce : (wallJumping ? wallJumpDirection * wallJumpForce /2 : 0)), (dashing ?  0 : (jumping ? jumpForce : (wallJumping ? wallJumpForce : customGravity) )))) ;

        speed.y = acceleration.y * Time.deltaTime;

        touchingFloor = (CheckCollisions(collider2d, new Vector2(0, speed.y).normalized, Mathf.Abs(speed.y) * Time.deltaTime)); //test si on touche le sol
        touchingWall = (CheckCollisions(collider2d, new Vector2(speed.x, 0).normalized, Mathf.Abs(speed.x) * Time.deltaTime)); //test si on touche le sol

        speed.x =  acceleration.x * Time.deltaTime * movementAcceleration * (!touchingFloor ? airSpeedMultiplier : 1)  + inertiaFactor * speed.x; // simule une certaine inertie  

        speed.x = Mathf.Clamp(speed.x, -maxMovementSpeed, maxMovementSpeed);  // on limite la vitesse maximale du joueur
       
        if (touchingFloor) 
            speed.y = 0;
        if (touchingWall && Mathf.Sign(speed.y) == -1)
        {
            speed.y /= wallResistance;
            if (Mathf.Sign(speed.x)==-1)
            {
                leftWallFrictionEffect.gameObject.SetActive(true);
            }
            else if (Mathf.Sign(speed.x) == 1 )
                rightWallFrictionEffect.gameObject.SetActive(true);      
        }
        else
        {
            rightWallFrictionEffect.gameObject.SetActive(false);
            leftWallFrictionEffect.gameObject.SetActive(false);
        }

        //Gestion du saut et de la chute//
        if (touchingFloor) //Incremente temps pass� depuis l'appui de la touche de saut (0 si au sol)
        {   
            timeOnFloor += Time.deltaTime;
            canDash = true;
            wallJumpedFromLeft = false;
            wallJumpedFromRight = false; 
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
        else if (jump && touchingWall && !touchingFloor && ((Mathf.Sign(speed.x) == -1 && !wallJumpedFromLeft) || (Mathf.Sign(speed.x) == 1 && !wallJumpedFromRight))) // ptetre � rajouter direction == vers le mur
        {
            IEnumerator WallJump = WallJumpCoroutine(Mathf.Sign(speed.x));
            StartCoroutine(WallJump);
        }
        else if (jump && !touchingFloor && !jumping && canDoubleJump && timeAfterFirstJump > timeBeforeRejumpInAir) 
        {
            StartCoroutine("DoubleJumpCoroutine");
        }

        if(dash && !dashing && canDash && !touchingFloor) //Le joueur peut dash dans les airs une fois, le saut permet de dash � nouveau
        {
            StartCoroutine("DashCoroutine");
        }



        //Gestion des collisions et du d�placement//
        if (!touchingWall && !CheckCollisions(collider2d, new Vector2(speed.x, speed.y).normalized, Mathf.Sqrt(speed.x*speed.x+speed.y*speed.y) * Time.deltaTime) ) //test pour v�rifier qu'on entre pas dans un mur en diagonale
            position.x += speed.x * Time.deltaTime;  //horitontal movement
        if (!touchingFloor) //test pour v�rifier qu'on entre pas dans le sol ou le plafond    /!\ TODO : Mathf.Abs(speed.y) * Time.deltaTime) � affiner
            position.y += speed.y * Time.deltaTime;  //vertical movement


        transform.position = position;  //update position
    }

    private void getInput()
    {
        inputs = new Vector2(Input.GetAxis("Horizontal"), 0f);
        jump = Input.GetButtonDown("Jump");
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
        jumpEffect.Play();
        yield return new WaitForSeconds(airTime);
        jumping = false;
        canDoubleJump = true;
        timeAfterFirstJump = 0;
        
    }

    private IEnumerator DoubleJumpCoroutine()
    {
        jumping = true;
        canDoubleJump = false;
        jumpEffect.Play();
        yield return new WaitForSeconds(airTime);
        canDash = true;
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

    private IEnumerator WallJumpCoroutine(float wallDirection)  //wallDirection:1 == le mur de droite , -1 == le mur de gauche
    {
        wallJumpDirection = -wallDirection;
        wallJumping = true;
        if (wallDirection == -1) { wallJumpedFromLeft = true; wallJumpedFromRight = false; }
        else { wallJumpedFromRight = true; wallJumpedFromLeft = false; }
        yield return new WaitForSeconds(wallJumpDuration);
        wallJumping = false;
    }
}
