using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class InputControllerAnimated : MonoBehaviour
{

    [Header("External references (do not modify)")]
    public ParticleSystem jumpEffect;
    public ParticleSystem doubleJumpEffect;
    public ParticleSystem leftWallJumpEffect;
    public ParticleSystem rightWallJumpEffect;
    public ParticleSystem rightWallFrictionEffect;
    public ParticleSystem leftWallFrictionEffect;
    public TrailRenderer leftDashTrail;
    public TrailRenderer rightDashTrail;
    public Animator playerAnimator;
    private Collider2D collider2d;

    private Vector2 position;
    private Vector2 speed;
    private Vector2 acceleration;
    private Vector2 inputs;

    [Space(10)]
    [Header("FPS target")]
    [Range(60, 200)]
    [Tooltip("150 is good, indeed fps affects gameSpeed so try not to change it too much (below 120)")]
    public int target = 150;

    private AudioSource[] SFX;
    private bool respawn = false;
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
    private float minSpeedThreshold = 0.001f;  //Vitesse minimale
    private float timeDashing;

    [Space(10)]
    [Header("Player caracteristics")]
    [Range(50, 100)]
    public float movementAcceleration = 75;        //Vitesse du joueur
    [Range(10, 30)]
    public float maxMovementSpeed = 20;     //Vitesse maximale du joueur
    [Range(0.9f, 1)]
    public float inertiaFactor = 0.96f;     //Facteur inertielle [0,1]

    [Space(10)]
    [Header("World caracteristics")]
    [Range(-50, -300)]
    public float customGravity = -150;       //Force de gravite
    [Range(1, 2)]
    public float wallResistance = 1.5f;

    [Space(10)]
    [Header("Special Actions Values")]
    [Range(50, 150)]
    public float jumpForce = 100 ;           //Force du saut
    [Range(0f, 0.4f)]
    public float airTime = 0.2f;            //Temps en maximum de saut (nuancier)
    [Range(0.5f, 2)]
    public float doubleJumpMultiplier = 1;
    [Range(60, 200)]
    public float wallJumpForce = 120;           //Force du saut au mur
    [Range(0f, 0.5f)]
    public float wallJumpDuration = 0.2f;     //Dur�e du saut au mur
    [Range(100, 300)]
    public float dashForce = 200;            //Force/vitesse du dash
    [Range(0, 0.4f)]
    public float dashDuration = 0.2f;            //Dur�e du dash
    [Range(0, 1.5f)]
    public float airSpeedMultiplier = 0.7f;  //Multiplicateur de vitesse du joueur lorsqu'il est en l'air
    [Range(0, 0.5f)]
    public float timeBeforeRejumpInAir = 0f;         //Temps avant de pouvoir resauter en l'air
    [Range(0, 0.5f)]
    public float timeBeforeRejumpOnFloor = 0f;       //Temps avant de pouvoir resauter au sol
    [Range(0.5f, 2f)]
    public float sprintVelocityModifier = 1.5f;   //Multiplicateur de vitesse en sprintant
    [Range(0.5f, 1.5f)]
    public float sprintJumpModifier = 1.2f;       //Multiplicateur de force de saut en srprintant

    //Game Management
    private bool feedbacks=true;
    private bool ControlsActivated;
    private bool JumpActivated = true;
    private bool DoubleJumpActivated = false;
    private bool WallJumpActivated = false;
    private bool DashActivated = false;
    private Transform respawnPoint;

    private GameManager gameManager;
    private bool gamePaused = false;
    private bool pause = false;

    private void Awake()
    {
        position = transform.position;
        collider2d = transform.GetComponent<Collider2D>();
        speed = new Vector2();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = target;
        respawnPoint = GameObject.Find("RespawnPoint").transform;
        SFX = GameObject.Find("SFX").GetComponents<AudioSource>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        int currentLevel = gameManager.getLevelId();

        if (currentLevel >1)
        {
            WallJumpActivated = true;
            if (currentLevel > 2)
            {
                DoubleJumpActivated = true;
                if (currentLevel > 3)
                {
                    DashActivated = true;
                }
            }
        }
        ControlsActivated = true;
        feedbacks = gameManager.getFeedbacks();
        playerAnimator.enabled = feedbacks;

    }

    private void Update()
    {
        if (Application.targetFrameRate != target)
            Application.targetFrameRate = target;

        getInput();

        if (pause)
            gamePause();

        if (respawn)
        {
            speed = Vector2.zero;
            position = respawnPoint.position;
        }

        acceleration = (inputs * (sprint ? sprintVelocityModifier : 1) + new Vector2((dashing ? dashForce : (wallJumping ? wallJumpDirection * wallJumpForce /2 : 0)), jumping ? jumpForce : (wallJumping ? wallJumpForce : customGravity) )) ;

        speed.y += acceleration.y * Time.deltaTime;
        if (dashing)
            speed.y = 0;

        touchingFloor = (CheckCollisions(collider2d, new Vector2(0, speed.y).normalized, Mathf.Abs(speed.y) * Time.deltaTime)); //test si on touche le sol
        touchingWall = (CheckCollisions(collider2d, new Vector2(speed.x, 0).normalized, Mathf.Abs(speed.x) * Time.deltaTime)); //test si on touche le sol

        speed.x =  acceleration.x * Time.deltaTime * movementAcceleration * (!touchingFloor ? airSpeedMultiplier : 1) + inertiaFactor * speed.x; // simule une certaine inertie  + inertiaFactor * speed.x

        if (Mathf.Abs(speed.x) < minSpeedThreshold)
            speed.x = 0;

        speed.x = Mathf.Clamp(speed.x, -maxMovementSpeed, maxMovementSpeed);  // on limite la vitesse maximale du joueur

        if (touchingFloor)
        {
            speed.y = 0;
            playerAnimator.SetBool("OnFloor", true);
            canDoubleJump = true;
        }
        else
            playerAnimator.SetBool("OnFloor", false);

        if (touchingWall && Mathf.Sign(speed.y) == -1)
        {
            speed.y /= wallResistance;
            playerAnimator.SetTrigger("Sliding");
            if (Mathf.Sign(speed.x) == -1)
            {
                if(feedbacks)
                    leftWallFrictionEffect.gameObject.SetActive(true);
                if (!SFX[3].isPlaying)
                {
                    if (feedbacks)
                        SFX[3].Play();
                }
            }
            else if (Mathf.Sign(speed.x) == 1)
            {
                if (feedbacks)
                    rightWallFrictionEffect.gameObject.SetActive(true);
                if (!SFX[2].isPlaying)
                {
                    if (feedbacks)
                        SFX[2].Play();
                }
            }
        }
        else
        {
            SFX[2].Stop();
            SFX[3].Stop();
            playerAnimator.SetTrigger("NotSliding");
            rightWallFrictionEffect.gameObject.SetActive(false);
            leftWallFrictionEffect.gameObject.SetActive(false);
        }

        if (dashing)
            timeDashing += Time.deltaTime;

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

        if (jump && touchingFloor && !jumping && JumpActivated && timeOnFloor > timeBeforeRejumpOnFloor)
        {
            StartCoroutine("JumpCoroutine");
        }
        else if (jump && touchingWall && !touchingFloor && WallJumpActivated && ((Mathf.Sign(speed.x) == -1 && !wallJumpedFromLeft) || (Mathf.Sign(speed.x) == 1 && !wallJumpedFromRight))) // ptetre � rajouter direction == vers le mur
        {
            IEnumerator WallJump = WallJumpCoroutine(Mathf.Sign(speed.x));
            StartCoroutine(WallJump);
        }
        else if (jump && !touchingFloor && canDoubleJump && DoubleJumpActivated && timeAfterFirstJump > timeBeforeRejumpInAir) 
        {
            StartCoroutine("DoubleJumpCoroutine");
        }

        if(dash && !dashing && canDash && DashActivated && !touchingFloor) //Le joueur peut dash dans les airs une fois, le saut permet de dash � nouveau
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
        if (ControlsActivated)
        {
            inputs = new Vector2(Input.GetAxis("Horizontal"), 0f);
            jump = Input.GetButtonDown("Jump");
            sprint = Input.GetButton("Sprint");
            dash = Input.GetButton("Dash");
            respawn = Input.GetButtonDown("Respawn");
        }
        pause = Input.GetButtonDown("Pause");
    }

    private bool CheckCollisions(Collider2D moveCollider, Vector2 direction, float distance)
    {
        bool colliding = false;
        if (moveCollider!=null)
        {
            RaycastHit2D[] hits = new RaycastHit2D[10];
            ContactFilter2D filter = new ContactFilter2D() { };

            int numHits = moveCollider.Cast(direction, filter, hits, distance);

            for (int i = 0; i < numHits; i++)
            {
                if (hits[i].transform.gameObject.CompareTag("Lava"))
                {
                    position = respawnPoint.position;  //on respawn quand onn tombe dans la lave
                }
                if (hits[i].transform.gameObject.CompareTag("Victory"))
                {
                    LevelFinished();
                }
                if (!hits[i].collider.isTrigger)
                {
                    if ( direction != Vector2.down && hits[i].transform.gameObject.CompareTag("UpGround"))  //On passe � travers les sols gris sauf en descendant
                    {
                        //return false;
                    }
                    else
                        colliding = true;
                    if (hits[i].transform.gameObject.CompareTag("Moving") && direction == Vector2.down)  //On bouge avec les plateformes mobiles
                    {
                        PlateFormMove plateform = hits[i].transform.gameObject.GetComponent<PlateFormMove>();
                        position += new Vector2(plateform.getPlateformSpeed(), 0) * Time.deltaTime;
                    }
                    
                }
            }
            if (colliding)
                return true;
        }
        return false;
    }

    private IEnumerator JumpCoroutine()
    {
        if (feedbacks)
            SFX[0].Play();
        jumping = true;
        canDash = true;
        if (feedbacks)
            jumpEffect.Play();
        playerAnimator.SetTrigger("Jumping");
        yield return new WaitForSeconds(airTime);
        jumping = false;
        timeAfterFirstJump = 0;
        
    }

    private IEnumerator DoubleJumpCoroutine()
    {
        if (feedbacks)
            SFX[1].Play();
        jumping = true;
        canDoubleJump = false;
        speed.y = 0;
        if (feedbacks)
            doubleJumpEffect.Play();
        playerAnimator.SetTrigger("DoubleJumping");
        yield return new WaitForSeconds(airTime * doubleJumpMultiplier);
        canDash = true;
        jumping = false;
    }

    private IEnumerator DashCoroutine()     
    {
        float dashDirection = Mathf.Sign(speed.x);
        TrailRenderer trail = (dashDirection == 1 ? leftDashTrail : rightDashTrail);
        dashForce = dashDirection * Mathf.Abs(dashForce);
        dashing = true;
        canDash = false;
        timeDashing = 0;
        if (feedbacks)
            trail.emitting = true;
        if (feedbacks)
            playerAnimator.SetTrigger("Dashing");
        while (timeDashing < dashDuration)
        {
            trail.time = 1;
            yield return null;
        }
        dashing = false;
        trail.emitting = false;
        trail.time = 0;
    }

    private IEnumerator WallJumpCoroutine(float wallDirection)  //wallDirection:1 == le mur de droite , -1 == le mur de gauche
    {
        wallJumpDirection = -wallDirection;
        wallJumping = true;
        if (wallDirection == -1) { 
            wallJumpedFromLeft = true; 
            wallJumpedFromRight = false;
            if (feedbacks) 
                leftWallJumpEffect.Play();
            if (feedbacks) 
                SFX[5].Play(); }
        else 
        { 
            wallJumpedFromRight = true; 
            wallJumpedFromLeft = false;
            if (feedbacks) 
                rightWallJumpEffect.Play();
            if (feedbacks) 
                SFX[4].Play(); }
        yield return new WaitForSeconds(wallJumpDuration);
        wallJumping = false;
    }

    private void LevelFinished()
    {
        if (!gamePaused)
        {
            gamePaused = true;
            gameManager.EndLevelMenu();
            ControlsActivated = false;
        }
    }

    private void gamePause()
    {
        gamePaused = !gamePaused;
        if (gamePaused)
        {
            gameManager.GamePausedMenu();
            ControlsActivated = false;
        }
        else
        {
            gameManager.GameUnPaused();
            ControlsActivated = true;
        }
    }
}
