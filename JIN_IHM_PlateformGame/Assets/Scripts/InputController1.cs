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
    private bool jumped = false;
    private bool touchingFloor;              //Test si mur en dessous pour les collisions
    private bool canDoubleJump = false;
    private float timeJumping = 0;
    private float timeOnFloor = 0;

    public float movementSpeed = 20;        //Vitesse du joueur
    public float maxMovementSpeed = 10;     //Vitesse maximale du joueur
    public float inertiaFactor = 0.9f;     //Facteur inertielle [0,1]
    public float mass = 20;                 //Masse du joueur
    public float jumpForce = 10 ;           //Vitesse du saut
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

        acceleration = (inputs + new Vector2(0, customGravity + (jumping ? jumpForce : 0 ))) / mass;

        speed.x =  acceleration.x * Time.deltaTime * movementSpeed + inertiaFactor * speed.x; // simule une certaine inertie  
        speed.y = acceleration.y * Time.deltaTime; 

        speed.x = Mathf.Clamp(speed.x, -maxMovementSpeed, maxMovementSpeed);  // on limite la vitesse maximale du joueur

        if (sprint)
        {
            speed.x *= sprintVelocityModifier;  //TODO : fix
        }


        touchingFloor = (CheckCollisions(collider2d, new Vector2(0, speed.y).normalized, Mathf.Abs(speed.y) * Time.deltaTime)) ; //test si on touche le sol

        //Gestion du saut et de la chute//
        if (touchingFloor) //Incremente temps passé depuis l'appui de la touche de saut (0 si au sol)
        {
            timeJumping = 0;
            timeOnFloor += Time.deltaTime;
            canDoubleJump = true; //reset du double saut quand on touche le sol
        }
        else
        {
            timeJumping += Time.deltaTime;
            timeOnFloor = 0;
        }

        //if (jump && ( (touchingFloor && timeOnFloor > timeBeforeRejumpOnFloor) || (!touchingFloor && timeJumping < airTime)) )
        //{
        //    speed.y += jumpForce * (sprint ? sprintJumpModifier : 1);   //On saute plus haut si on sprint
        //}
        //else if (jump && (timeJumping > airTime + timeBeforeRejumpInAir) && canDoubleJump)  //TODO: condition à raffiner : "airTime + timeBeforeRejumpInAir" 
        //{
        //    timeJumping = 0;
        //    canDoubleJump = false;
        //}
        //else
        //    speed.y += customGravity * Time.deltaTime;    //Le cube tombe  

        if (jump && touchingFloor && !jumping)
        {
            StartCoroutine("JumpCoroutine");
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
        yield return new WaitForSeconds(airTime);
        jumped = true;
        jumping = false;
    }
}
