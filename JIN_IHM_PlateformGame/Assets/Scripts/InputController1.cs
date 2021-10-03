using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController1 : MonoBehaviour
{
    [SerializeField] private LayerMask floorLayerMask;
    private BoxCollider2D boxCollider2d;
    private Collider2D collider2d;

    private Vector2 position;
    private Vector2 speed;
    private Vector2 acceleration;
    private Vector2 inputs;
    private Vector3 speedIncrement;

    private bool sprint = false;
    private bool jump = false;
    private bool canDoubleJump = false;
    private float timeSinceJump = 0;

    public float movementSpeed = 20;        //Vitesse du joueur
    public float maxMovementSpeed = 10;     //Vitesse maximale du joueur
    public float inertiaFactor = 0.9f;     //Facteur inertielle [0,1]
    public float mass = 20;                 //Masse du joueur
    public float jumpForce = 10 ;           //Vitesse du saut
    public float customGravity = -10;       //Force de gravite
    public float airTime = 0.2f;            //Temps en maximum dans les airs lors du saut
    public float sprintVelocityModifier = 1.2f;   //Multiplicateur de vitesse en sprintant
    public float sprintJumpModifier = 1.2f;       //Multiplicateur de force de saut en srprintant

    private bool touchingDown;              //Test si mur en dessous pour les collisions


    private void Awake()
    {
        position = transform.position;
        boxCollider2d = transform.GetComponent<BoxCollider2D>();
        collider2d = transform.GetComponent<Collider2D>();
        speed = new Vector2();
        speedIncrement = new Vector3();
    }

    private void Update()
    {
        getInput();

        acceleration = (inputs + new Vector2(0, customGravity)) / mass;
        speed.x =  acceleration.x * Time.deltaTime * movementSpeed + inertiaFactor * speed.x; // simule une certaine inertie  
        speed.y = acceleration.y * Time.deltaTime; 
        speed.x = Mathf.Clamp(speed.x, -maxMovementSpeed, maxMovementSpeed);  // on limite la vitesse maximale du joueur

        if (sprint)
        {
            speed.x *= sprintVelocityModifier;  //TODO : fix
        }

        touchingDown = (CheckCollisions(collider2d, new Vector2(0, speed.y).normalized, Mathf.Abs(speed.y) * Time.deltaTime)) ; //test si on touche le sol
        
        if (touchingDown) //Incremente temps passé depuis l'appui de la touche de saut (0 si au sol)
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
            speed.y += customGravity * Time.deltaTime;    //Le cube tombe  


        if (!CheckCollisions(collider2d, new Vector2(speed.x,0).normalized, Mathf.Abs(speed.x) * Time.deltaTime))  //test pour vérifier qu'on entre pas dans un mur
            position.x += speed.x * Time.deltaTime;  //horitontal movement
        if (!CheckCollisions(collider2d, new Vector2(0, speed.y).normalized, Mathf.Abs(speed.y) * Time.deltaTime)) //test pour vérifier qu'on entre pas dans le sol ou le plafond
            position.y += speed.y * Time.deltaTime;  //vertical movement
        speedIncrement = new Vector3(speed.x * Time.deltaTime, speed.y * Time.deltaTime, 0);  //sert à caster en avance pour les collisions
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

}
