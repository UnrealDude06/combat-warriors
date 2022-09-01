using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float percent;
    public GameObject player;
    public float rotateSpeed;
    public float detectRadius,closePlayerRadius;
    public float speed,maxSpeed,cooldown,default_cooldown,attackDamage;
    Collider[] detectPlayer;
   public  bool move,attack,knockBack;
    public Rigidbody rb;
    public Animator anim;
   public  float attackingCool,default_attackingCool,knockBackSpeed,knockBack_timer,knockBack_timer_default;

   public bool grabbed,small_knockBack;


       public float jumpSpeed,jumpShortSpeed;
    bool jump,jumpCancel;
    public bool isGrounded;
   
    public TMPro.TMP_Text percent_ui;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cooldown = default_cooldown;
        attackingCool = default_attackingCool;
        knockBack_timer = knockBack_timer_default;
       
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("isAttacking",attack);
        anim.SetFloat("Speed",Mathf.Abs(rb.velocity.magnitude));
        anim.SetBool("isGrounded",true);
        anim.SetBool("ouch",small_knockBack);
         anim.SetBool("grabbed",grabbed);


 percent_ui.text = ( percent.ToString() + "%");
  


          detectPlayer = Physics.OverlapSphere(transform.position, closePlayerRadius,LayerMask.GetMask("Player"));
        if (detectPlayer.Length != 0)
        {

        Debug.Log("Found player and hes tooooo close !");


         foreach(Collider nearbyPlayer in detectPlayer)
                {
                    attackingCool -=Time.deltaTime;
                    if(attackingCool < 0 && !knockBack)
                    {
                        attackingCool = default_attackingCool;
                 attack= true;
                 player.GetComponent<FighterController>().percent += attackDamage;

                    }

                }
        }

  Collider[] findPlayerFar = Physics.OverlapSphere(transform.position, detectRadius,LayerMask.GetMask("Player"));
        if (findPlayerFar.Length != 0)
        {

        Debug.Log("Found player!");


         foreach(Collider nearbyPlayer in findPlayerFar)
                {
                    move= true;


                }
        }



        if(move && !small_knockBack)
        {
           FollowTargetWithRotation(player.transform,02f,speed);
            
        }

        if(attack && cooldown >0)
        {
            rb.velocity = Vector3.zero;
            cooldown -= Time.deltaTime;

            
            player.GetComponent<FighterController>().knockBack = true;





            if(cooldown<0)
            {
                cooldown = default_cooldown;
                attack = false;
                player.GetComponent<FighterController>().percent += attackDamage;
            }
        }
        if(knockBack)
        {
            move =false;
            attack =false;
            rb.AddForce(-transform.forward* (knockBackSpeed + percent)*Time.deltaTime,ForceMode.Impulse);
            knockBack_timer -=Time.deltaTime;
                    if(knockBack_timer < 0)
                    {
                        knockBack_timer = knockBack_timer_default;
                knockBack =false;
                attackingCool = default_attackingCool;
                    }
        }
        if(small_knockBack)
        {
             move =false;
            attack =false;
            rb.AddForce(-transform.forward* (knockBackSpeed )*Time.deltaTime,ForceMode.Impulse);
            knockBack_timer -=Time.deltaTime;
                    if(knockBack_timer < 0)
                    {
                        knockBack_timer = knockBack_timer_default;
                small_knockBack =false;
                attackingCool = default_attackingCool;
                    }
        }

        if(grabbed)
        {
            move=false;
            attack = false;
            rb.velocity = Vector3.zero;
        }

        if(player.GetComponent<FighterController>().knockBack == true )
        {
            attack  = false;
        }



        //////jump
        if(player.gameObject.GetComponent<FighterController>().isGrounded !=true && player.gameObject.GetComponent<FighterController>().rb.velocity.y < 0) 
            {
                RaycastHit checkplayerPos ;
                Physics.Raycast(transform.position,transform.up,out checkplayerPos,5f,LayerMask.GetMask("Player"));
                if(checkplayerPos.collider)
                {
                    jump = true;

                }
            }
        

        














        if (rb.velocity.magnitude > maxSpeed)
        {
            Vector3.ClampMagnitude(rb.velocity, maxSpeed);
        }





float offset_distance;
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


     

        // Normal jump (full speed)
        if (jump && isGrounded && !small_knockBack)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpSpeed, rb.velocity.z);
            jump = false;
        }
        // Cancel the jump when the button is no longer pressed
        if (jumpCancel && !small_knockBack)
        {
            if (rb.velocity.y > jumpShortSpeed && !small_knockBack)
                rb.velocity = new Vector3(rb.velocity.x, jumpShortSpeed, rb.velocity.z);
            jumpCancel = false;
        }

    }


    void FollowTargetWithRotation(Transform target, float distanceToStop, float speed)
     {
         if(Vector3.Distance(transform.position, target.position) > distanceToStop)
         {
             transform.LookAt(target);
             Vector3 folowVector = transform.forward * speed;
                folowVector.y =  rb.velocity.y;

             //   rb.MovePosition( Vector3.MoveTowards(transform.position,target.position,speed * Time.deltaTime));
                 transform.rotation = Quaternion.Euler(0,transform.eulerAngles.y,transform.eulerAngles.z);
             //rb.AddRelativeForce(folowVector * Time.deltaTime, ForceMode.VelocityChange);
                rb.velocity=  folowVector;
         }
         


     }


    
         


            private void OnDrawGizmos()
    {
          Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, closePlayerRadius);
        Gizmos.color = Color.green;
       Gizmos.DrawWireSphere(transform.position,detectRadius);
    }
     void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "Deadzone")
        {
             
        }
    }


      void OnCollisionEnter(Collision col) {
        if(col.gameObject.tag == "Dead")
        {
            player.gameObject.GetComponent<FighterController>().kills++;
            ///change scene to game over
        
           percent = 0f;
            transform.position = new Vector3 (10.91f,19,53f);

        }
        if(col.gameObject.tag == "projectile")
        {
            Destroy(col.gameObject);
            percent++;
            knockBack = true;
        }
    }
  

}
