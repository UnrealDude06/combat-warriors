using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class FighterController : MonoBehaviour
{
    [Header ("kills")]
    public int kills;
    public int killsNeeded;
    public float total_damage;
    public GameObject projectile;
    [Header("KnockBack")]
    public float knockBack_timer;
    public float knockBack_timer_default,knockBackSpeed;
    public bool knockBack;

    [Header("Health")]
    public float percent;
    [Header("Components")]
    public Rigidbody rb;
    public Animator anim;
    public float speed,default_speed,runSpeed,movementLerp,turnSpeed,gravity;

    public bool canControl,stopAccelerating;

    public float jumpSpeed,jumpShortSpeed;
    bool jump,jumpCancel;
    public bool isGrounded,dash;



    
    public enum comboStates {none,upperPunch1,upperPunch2,upperPunch3,air_punch,    upperKick1,upperKick2,upperKick3,     mid_air_kick, uppercut,kick_chamber,shoot_projectile  }

 

        [Header("Debug")]
    public float groundSlopeAngle;

    [Header ("Normal Attacks")]
    public comboStates current_combo ;
        
    bool activateTimerReset;
    public float default_cooldown,slideKick_cooldown;
    float cooldown;
    public float shooting_default,shootingTimer;


    public Transform nearEnemyCheck;
    public LayerMask enemyLayer;
    [SerializeField,Range(0,25f)]
    public float punchDamage,specialDamage,ariealDamage,upperCutDamage,airKickDamage;
    
    
    [Header("Particles")]
    public ParticleSystem attackParticle;










    
        ///hidden shiz
    Vector3 input, movement, localMove, groundSlopeDir;

    float horizontalAxis;
    float verticalAxis;
    float offset_distance;
    RaycastHit hit;
    Quaternion slope;
     public TMPro.TMP_Text percent_ui;
      public TMPro.TMP_Text pointCount;




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    { percent_ui.text = ( percent.ToString() + "%");
     pointCount.text = ( kills.ToString() );

        if(kills >= killsNeeded)
    {
        SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex + 1);
    }
        rb.AddForce(Vector3.down * -gravity * Time.deltaTime,ForceMode.Force);
        Movement();
        Jump();
        Animation();
     if(!knockBack)
     {
        PunchAttack();
     }
        knockBack_damage();
        TimerReset();
        ShootProjectile();
    }

     void Movement()
    {
                RaycastHit hit_ground;

        if (Physics.Raycast(transform.position, -transform.up, out hit_ground, 2f, LayerMask.GetMask("Default")))
        { //1 for the distance orignally
            if (hit_ground.collider != gameObject)
            {
                offset_distance = hit_ground.distance;
                Debug.DrawLine(transform.position, hit_ground.point, Color.cyan);
                if (offset_distance <= 5f)
                {
                    isGrounded = true;
                }

            }
        }
        else
        {
            isGrounded = false;
        }




        input = new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));
        input = Vector2.ClampMagnitude(input, 1);

        //camera forward and right vectors:
        var forward = Camera.main.transform.forward;
        var right = Camera.main.transform.right;

        //project forward and right vectors on the horizontal plane (y = 0)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();


        //this is the direction in the world space we want to move:
        movement = forward * input.y + right * input.x;



        
        if (input.magnitude > 0 )
        {
            Quaternion rot = Quaternion.LookRotation(movement);

            if(current_combo != comboStates.none || knockBack)
            return;
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, turnSpeed * Time.deltaTime);


        }


        
        if (canControl)
        {
           rb.velocity = Vector3.Lerp(rb.velocity ,new Vector3(movement.x * speed,rb.velocity.y,movement.z * speed),movementLerp * Time.deltaTime);
        }

       


        // Normal jump (full speed)
        if (jump && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpSpeed, rb.velocity.z);
            jump = false;
        }
        // Cancel the jump when the button is no longer pressed
        if (jumpCancel)
        {
            if (rb.velocity.y > jumpShortSpeed)
                rb.velocity = new Vector3(rb.velocity.x, jumpShortSpeed, rb.velocity.z);
            jumpCancel = false;
        }


     

    }
    
    void Jump()
    {
         if (Input.GetButtonDown("Jump") && isGrounded)   // Player starts pressing the button
        { jump = true; }
        if (Input.GetButtonUp("Jump") && !isGrounded)     // Player stops pressing the button
        { jumpCancel = true; }

        
    }


    void PunchAttack()
     {
       

        if(Input.GetButtonDown("Fire1")  && isGrounded)
        {

            if(current_combo == comboStates.upperPunch3||
            current_combo == comboStates.upperKick1||
            current_combo == comboStates.upperKick2
            )
            return;

            current_combo++;
            activateTimerReset = true;
            
            cooldown = default_cooldown;


           

        }



        if(Input.GetButtonDown("Fire2") && isGrounded )
        {

            if(current_combo == comboStates.upperKick2||
            current_combo == comboStates.upperPunch3
            )
            return;
        
           if(current_combo == comboStates.none||
            current_combo == comboStates.upperPunch1||
            current_combo == comboStates.upperPunch2)
            {
                current_combo = comboStates.upperKick1;
            }
            else if (current_combo == comboStates.upperKick1)
            {
                current_combo++;
            }

            activateTimerReset  =true;
            cooldown = default_cooldown;

        }

        if(Input.GetButtonDown("Jump") && current_combo == comboStates.upperPunch1)
        {
            current_combo  = comboStates.uppercut;
            activateTimerReset  =true;
            cooldown = default_cooldown;
        }


            if(Input.GetButtonDown("Fire1")  && !isGrounded)
        {
            current_combo  = comboStates.air_punch;
            activateTimerReset  =true;
            cooldown = default_cooldown;
        }
      if(Input.GetButtonDown("Fire2")  && !isGrounded)
        {
            current_combo  = comboStates.mid_air_kick;
            activateTimerReset  =true;
            cooldown = default_cooldown;
        }



         ////////////find enemy if plyaer attacks them and move player to enemy


          Collider[] colliderMove = Physics.OverlapSphere(nearEnemyCheck.transform.position, 2f,LayerMask.GetMask("Enemy"));
            if (colliderMove.Length != 0)
            {

                Debug.Log("Found something!");
                foreach(Collider nearEnemy in colliderMove)
                    {
                    if(Input.GetButtonDown("Fire1")  && cooldown <0 )
                        {
                            transform.LookAt(nearEnemy.gameObject.transform);
                            

                       // rb.MovePosition(Vector3.Lerp(transform.position,new Vector3(nearEnemy.gameObject.transform.position.x + Mathf.Abs(transform.forward.magnitude),nearEnemy.gameObject.transform.position.y,0),23f * Time.deltaTime));
                         transform.rotation = Quaternion.Euler(0,transform.eulerAngles.y,transform.eulerAngles.z);
                        }

                         if((current_combo == comboStates.upperPunch1||
                        current_combo == comboStates.upperPunch2||
                        current_combo == comboStates.upperPunch3 ))
                        {
                           transform.LookAt(nearEnemy.gameObject.transform);
                            

                        //rb.MovePosition(Vector3.Lerp(transform.position,new Vector3(nearEnemy.gameObject.transform.position.x+ Mathf.Abs(transform.forward.magnitude),nearEnemy.gameObject.transform.position.y,0),23f * Time.deltaTime));
                         transform.rotation = Quaternion.Euler(0,transform.eulerAngles.y,transform.eulerAngles.z);


                         if(current_combo == comboStates.air_punch||
                         current_combo == comboStates.mid_air_kick||
                         current_combo ==comboStates.uppercut)
                         {
                        transform.LookAt(nearEnemy.gameObject.transform);
                         transform.rotation = Quaternion.Euler(0,transform.eulerAngles.y,transform.eulerAngles.z);

                         }
                        }
                    }

            }
         DoDamage();
     }

    void Animation() 
        {
            anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.z));
            anim.SetBool("isGrounded", isGrounded);
            anim.SetFloat("Yvelocity", rb.velocity.y);
            anim.SetBool("canControl", canControl);
             anim.SetBool("knockback", knockBack);






        if(current_combo == comboStates.upperKick1)
        {
                    anim.SetLayerWeight(anim.GetLayerIndex("KickingLayer"),1f);
                anim.SetLayerWeight(anim.GetLayerIndex("PunchingLayer"),0f);

                    anim.SetBool("upperKick1", true);
            anim.SetBool("upperKick2", false);
            anim.SetBool("upperKick3", false);
        }

        if(current_combo == comboStates.upperKick2)
        {
            
            anim.SetBool("upperKick1", false);
            anim.SetBool("upperKick2", true);
            anim.SetBool("upperKick3", false);
        }

        if(current_combo == comboStates.upperKick3)
        {
        

                    anim.SetBool("upperKick1", false);
            anim.SetBool("upperKick2", false);
            anim.SetBool("upperKick3", true);
        }









        
        if(current_combo == comboStates.upperPunch1)
        {
            anim.SetLayerWeight(anim.GetLayerIndex("KickingLayer"),0f);
                anim.SetLayerWeight(anim.GetLayerIndex("PunchingLayer"),1f);
            
                    anim.SetBool("isAttacking", true);
            anim.SetBool("isAttacking2", false);
            anim.SetBool("isAttacking3", false);


        }

        if(current_combo == comboStates.upperPunch2)
        { anim.SetLayerWeight(anim.GetLayerIndex("KickingLayer"),0f);
                anim.SetLayerWeight(anim.GetLayerIndex("PunchingLayer"),1f);
        
            anim.SetBool("isAttacking", false);
            anim.SetBool("isAttacking2", true);
            anim.SetBool("isAttacking3", false);



        }

        if(current_combo == comboStates.upperPunch3)
        { anim.SetLayerWeight(anim.GetLayerIndex("KickingLayer"),0f);
                anim.SetLayerWeight(anim.GetLayerIndex("PunchingLayer"),1f);
                
                    anim.SetBool("isAttacking", false);
            anim.SetBool("isAttacking2", false);
            anim.SetBool("isAttacking3", true);

        }
        if(current_combo == comboStates.uppercut)
                {
                    anim.SetLayerWeight(anim.GetLayerIndex("PunchingLayer"),0f);
        anim.SetBool("uppercut", true);
                }
                else
                {
        anim.SetBool("uppercut", false);
                }


        if(current_combo == comboStates.air_punch)
                {
                    anim.SetLayerWeight(anim.GetLayerIndex("PunchingLayer"),0f);
        anim.SetBool("air_punch", true);
                }
                else
                {
        anim.SetBool("air_punch", false);
                }

                if(current_combo == comboStates.mid_air_kick)
                {
                    anim.SetLayerWeight(anim.GetLayerIndex("PunchingLayer"),0f);
        anim.SetBool("mid_air_kick", true);
                }
                else
                {
        anim.SetBool("mid_air_kick", false);
                }



        if(current_combo == comboStates.none)
        {
                        anim.SetLayerWeight(anim.GetLayerIndex("KickingLayer"),0f);
                anim.SetLayerWeight(anim.GetLayerIndex("PunchingLayer"),0f);

                    anim.SetBool("isAttacking", false);
            anim.SetBool("isAttacking2", false);
            anim.SetBool("isAttacking3", false);


            anim.SetBool("upperKick1", false);
            anim.SetBool("upperKick2", false);
            anim.SetBool("upperKick3", false);
             anim.SetBool("air_punch", false);  
        }
        }



     void TimerReset()
     {
        if(activateTimerReset)
        {
            cooldown -= Time.deltaTime;

            if(cooldown <= 0f)
            {
                current_combo  = comboStates.none;
                activateTimerReset = false;
                cooldown = default_cooldown;
                

            }
        }
     }

     void ShootProjectile()
     {
        
        if(Input.GetButton("Fire1") && shootingTimer >0)
        {
            shootingTimer -= Time.deltaTime;
            if(shootingTimer < 0)
            {
                shootingTimer = shooting_default;

                
                Instantiate(projectile,transform.position,transform.rotation);
               
            }
        }
        if(Input.GetButtonUp("Fire1") && shootingTimer < shooting_default)
        {
            shootingTimer = shooting_default;
            canControl = true;
        }
     }


     void DoDamage()
     {
         
        Collider[] hitColliders = Physics.OverlapSphere(nearEnemyCheck.transform.position, 1.2f,enemyLayer);
        if (hitColliders.Length != 0)
        {

        Debug.Log("Found something!");
        }

        if(hitColliders.Length !=0)
        {
            if(( Input.GetButtonDown("Fire1") ) )
            {
            foreach(Collider nearbyObject in hitColliders)
                {
                // Assuming that the enemy gameobject with the collider also holds the EnemyHealth script (!)
                
                if(current_combo == comboStates.upperPunch1 ||
                current_combo == comboStates.upperPunch2 ||
                current_combo == comboStates.upperPunch3 
                )
                {
                    
                  // Assuming that the enemy gameobject with the collider also holds the EnemyHealth script (!)
                EnemyController enemy = nearbyObject.GetComponent<EnemyController>();
                  ////use enemy rigidbody to yeet him forward, fast
               
               enemy.percent += punchDamage;
               enemy.small_knockBack = true;
                attackParticle.Play();

                    }

                }
            }
          if(( Input.GetButtonDown("Fire2") ) )
            {
            foreach(Collider nearbyObject in hitColliders)
                {
                // Assuming that the enemy gameobject with the collider also holds the EnemyHealth script (!)
               
                if(current_combo == comboStates.upperKick1 ||
                current_combo == comboStates.upperKick2 ||
                current_combo == comboStates.upperKick3 
                )
                    {
                       
                      // Assuming that the enemy gameobject with the collider also holds the EnemyHealth script (!)
                EnemyController enemy = nearbyObject.GetComponent<EnemyController>();
                  ////use enemy rigidbody to yeet him forward, fast
               
               enemy.percent += specialDamage;
               enemy.small_knockBack = true;
                    attackParticle.Play();
                    }

                }
            }
            



                foreach(Collider nearbyObject in hitColliders)
                    {
                    // Assuming that the enemy gameobject with the collider also holds the EnemyHealth script (!)
                    
                    if(current_combo == comboStates.uppercut)
                    {
                        
                    // Assuming that the enemy gameobject with the collider also holds the EnemyHealth script (!)
                    EnemyController enemy = nearbyObject.GetComponent<EnemyController>();
                    ////use enemy rigidbody to yeet him forward, fast
                
                enemy.percent += upperCutDamage;
                enemy.knockBack = true;
                    attackParticle.Play();
                    }

                     if(current_combo == comboStates.air_punch)
                    {
                        
                    // Assuming that the enemy gameobject with the collider also holds the EnemyHealth script (!)
                    EnemyController enemy = nearbyObject.GetComponent<EnemyController>();
                    ////use enemy rigidbody to yeet him forward, fast
                
                 enemy.percent += ariealDamage/2;
                 enemy.knockBack = true;
                    attackParticle.Play();
                    }
                      if(current_combo == comboStates.mid_air_kick)
                    {
                        
                    // Assuming that the enemy gameobject with the collider also holds the EnemyHealth script (!)
                    EnemyController enemy = nearbyObject.GetComponent<EnemyController>();
                    ////use enemy rigidbody to yeet him forward, fast
                
                 enemy.percent += airKickDamage/2;
                 enemy.knockBack = true;
                    attackParticle.Play();
                    }

                }
        }
          
    }

    void knockBack_damage()
    {
        if(knockBack )
        {
            canControl = false;

            movement = Vector3.zero;    
            
            total_damage= knockBackSpeed + percent;
                 
            rb.AddForce(-transform.forward* (total_damage)*Time.deltaTime,ForceMode.Impulse);
            knockBack_timer -=Time.deltaTime;
                    if(knockBack_timer < 0)
                    {
                        knockBack_timer = knockBack_timer_default;
                knockBack =false;
                  canControl = true;
                    }
        }
    



    }

      void OnCollisionEnter(Collision col) {
        if(col.gameObject.tag == "Dead")
        {
            kills--;
            ///change scene to game over
        
           percent = 0f;
                       transform.position = new Vector3 (10.91f,19,53f);


        }
 
    }


     

}
