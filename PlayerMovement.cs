using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] public Collider2D capsuleCollider;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject player;
    [SerializeField] private Transform leftGroundCheck, rightGroundCheck;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private GameObject enemyBiker;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource footStepAudioSource;
    [SerializeField] private AudioClip jumpSound, dashSound, reverseGravitySound, footStepStoneSound, landSound;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private TrailRenderer trailRenderer;



    private float horizontalValue;
    private bool isFacingRight = true;
    private float rayDistanceGround = 0.05f;
    private bool playerJumping = false;
    public bool reverseGravityActivated = false;
    public bool canReverseGravity;
    private float gravityAmount = 3f;
    public bool canMove;
    private float maxFallSpeed = -15f;

    private float dashForce = 25f;
    private int dashCounter = 0;
    private float dashTime = 0.2f;

    void Start()
    {
        rb.gravityScale = gravityAmount;
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level 1"))
        {
            canReverseGravity = false;
        }
    }

    void Update()
    {
        if (player.transform.localScale.x > 0)
        {
            isFacingRight = true;
        }
        else if (player.transform.localScale.x < 0)
        {
            isFacingRight = false;
        }

        horizontalValue = Input.GetAxis("Horizontal");

        if (horizontalValue > 0 && !isFacingRight && !reverseGravityActivated && !playerCombat.isDead && canMove)
        {
            flipPlayer();
        }
        if (horizontalValue < 0 && isFacingRight && !reverseGravityActivated && !playerCombat.isDead && canMove)
        {
            flipPlayer();
        }
        if (-horizontalValue > 0 && !isFacingRight && reverseGravityActivated && !playerCombat.isDead && canMove)
        {
            flipPlayer();
        }
        if (-horizontalValue < 0 && isFacingRight && reverseGravityActivated && !playerCombat.isDead && canMove)
        {
            flipPlayer();
        }

        float absoluteVelocityX = Mathf.Abs(rb.velocity.x);

        if (absoluteVelocityX > 0.4f && absoluteVelocityX < 6f)
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isRunning", false);
        }
        else if (absoluteVelocityX >= 6f)
        {
            animator.SetBool("isRunning", true);
            animator.SetBool("isWalking", false);
        }
        else
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
        }
        checkIfGrounded();
        if (checkIfGrounded() && Input.GetKeyDown(KeyCode.Space) && !playerCombat.isDead && canMove)
        {
            jump();
        }
        if (checkIfGrounded() && Input.GetKeyDown(KeyCode.W) && canReverseGravity && !playerCombat.isDead && canMove)
        {
            reverseGravity();
        }
        if (canMove && !checkIfGrounded() && dashCounter < 1 && Input.GetKeyDown(KeyCode.S) && horizontalValue != 0)
        {
            dash();
        }

        if (animator.GetBool("isJumping"))
        {
            if (checkIfGrounded())
            {
                animator.SetTrigger("land");
                animator.SetBool("isJumping", false);
                playerJumping = false;
            }
        }
        controllFallSpeed();
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.LeftShift) && !playerCombat.isDead && canMove)
        {
            rb.velocity = new Vector2(horizontalValue * runSpeed * Time.deltaTime, rb.velocity.y);
        }
        else if (!playerCombat.isDead && canMove)
        {
            rb.velocity = new Vector2(horizontalValue * walkSpeed * Time.deltaTime, rb.velocity.y);
        }
        if (playerJumping && !checkIfGrounded())
        {
            animator.SetBool("isJumping", true);
        }
    }
    private void flipPlayer()
    {
        isFacingRight = !isFacingRight;
        Vector3 playerScale = player.transform.localScale;
        player.transform.localScale = new Vector3(playerScale.x * -1, playerScale.y, playerScale.z);
    }
    private void dash()
    {
        dashCounter++;
        canMove = false;
        rb.constraints |= RigidbodyConstraints2D.FreezePositionY;
        Vector2 dashDirection = new Vector2(horizontalValue, 0).normalized;
        rb.velocity = dashDirection * dashForce;
        trailRenderer.emitting = true;
        audioSource.PlayOneShot(dashSound, 0.2f);
        animator.SetTrigger("spin");
        Invoke("enableCharacterMove", dashTime);
    }
    private void enableCharacterMove()
    {
        trailRenderer.emitting = false;
        canMove = true;
        rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
    }

    private bool checkIfGrounded()
    {
        RaycastHit2D leftHit = Physics2D.Raycast(leftGroundCheck.position, Vector2.down, rayDistanceGround, whatIsGround);
        RaycastHit2D rightHit = Physics2D.Raycast(rightGroundCheck.position, Vector2.down, rayDistanceGround, whatIsGround);

        Debug.DrawRay(leftGroundCheck.position, Vector2.down * rayDistanceGround, Color.green, 25f);
        if (leftHit.collider != null && leftHit.collider.CompareTag("isGround") || rightHit.collider != null && rightHit.collider.CompareTag("isGround"))
        {
            dashCounter = 0;
            return true;
        }
        else
        {
            
            return false;
        }
    }
    
   
    private void jump()
    {
        audioSource.PlayOneShot(jumpSound, 0.6f);
        if (reverseGravityActivated)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(new Vector2(0, -jumpForce));
        }
        else
        {
            rb.AddForce(new Vector2(0, jumpForce));
        }
        playerJumping = true;
    }
    public void reverseGravity()
    {
        audioSource.PlayOneShot(reverseGravitySound, 0.2f);
        if (!reverseGravityActivated)
        {
            reverseGravityActivated = true;
            rb.gravityScale = -gravityAmount;
        }
        else if (reverseGravityActivated)
        {
            reverseGravityActivated = false;
            rb.gravityScale = gravityAmount;
        }
        animator.SetTrigger("spin");
        playerJumping = true;

        flipPlayer();
        if (player.transform.rotation.z == 0)
        {
            player.transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else
        {
            player.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
    public void reverseGravityNoAnimation()
    {
        if (!reverseGravityActivated)
        {
            reverseGravityActivated = true;
            rb.gravityScale = -gravityAmount;
        }
        else if (reverseGravityActivated)
        {
            reverseGravityActivated = false;
            rb.gravityScale = gravityAmount;
        }
        playerJumping = true;

        flipPlayer();
        if (player.transform.rotation.z == 0)
        {
            player.transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else
        {
            player.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }


    public void playStepSound()
    {
        if(checkIfGrounded())
        footStepAudioSource.PlayOneShot(footStepStoneSound, 0.04f);
    }
    public void playLandSound()
    {
        footStepAudioSource.PlayOneShot(landSound, 0.15f);
    }
    public void controllFallSpeed()
    {
        float currentFallSpeed = rb.velocity.y;

        float clampedFallSpeed = Mathf.Clamp(currentFallSpeed, maxFallSpeed, 50f);
        if(rb.velocity.y < clampedFallSpeed)
        {
        rb.velocity = new Vector2(rb.velocity.x, clampedFallSpeed);
        }
    }
}
  

