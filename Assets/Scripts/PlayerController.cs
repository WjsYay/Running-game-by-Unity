using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public float forwardSpeed = 7.0f;
    private float initialForwardSpeed;
    private Animator playerAnim;
    private Vector3 startPosition;

    private static readonly int IsRunning = Animator.StringToHash("isRunning");
    private static readonly int JumpTrigger = Animator.StringToHash("jump");
    private static readonly int RollTrigger = Animator.StringToHash("roll");
    private static readonly int MoveLeftTrigger = Animator.StringToHash("moveLeft");
    private static readonly int MoveRightTrigger = Animator.StringToHash("moveRight");
    private static readonly int TurnLeftTrigger = Animator.StringToHash("turnLeft");

    public float jumpHeight = 2f;
    public float jumpDuration = 1f;
    private bool isJumping = false;
    private CapsuleCollider playerCollider;
    private float originalColliderHeight;
    private Vector3 originalColliderCenter;
    private bool isRolling = false; 

    public float trackSwitchDuration = 0.2f;
    private bool isSwitchingTrack = false;
    private const float LeftTrackZ = -3f;
    private const float MiddleTrackZ = 0f;
    private const float RightTrackZ = 3f;
    private const float TrackZThreshold = 0.1f;

    public AudioClip actionAudio;
    public float soundVolume = 5f;

    public float turnDuration = 0.5f;
    private bool isTurning = false;
    private Quaternion targetRotation;
    private Quaternion initialRotation;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        initialForwardSpeed = forwardSpeed;
        forwardSpeed = 0f;

        initialRotation = Quaternion.Euler(0, 0, 0);
        targetRotation = Quaternion.Euler(0, -90, 0);
        transform.rotation = initialRotation;

        playerAnim = GetComponent<Animator>();
        if (playerAnim != null)
        {
            playerAnim.SetBool(IsRunning, false);
            playerAnim.speed = 0f;
        }

        playerCollider = GetComponent<CapsuleCollider>();
        if (playerCollider != null)
        {
            originalColliderHeight = playerCollider.height;
            originalColliderCenter = playerCollider.center;
        }
        else
        {
            Debug.LogError("Player has no CapsuleCollider!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool gameIsReady = (GameManager.Instance != null && GameManager.Instance.isGameRunning);

        if (!gameIsReady)
        {
            forwardSpeed = 0f;
            if (playerAnim != null)
            {
                playerAnim.SetBool(IsRunning, false);
                playerAnim.speed = 0f;
            }
            return;
        }

        if (!isTurning && transform.rotation != targetRotation)
        {
            StartCoroutine(TurnToForwardCoroutine()); 
            return;
        }

        forwardSpeed = initialForwardSpeed;
        if (playerAnim != null)
        {
            playerAnim.SetBool(IsRunning, true);
            playerAnim.speed = 1f;
        }

        transform.position += Vector3.left * forwardSpeed * Time.deltaTime;

        CheckRollState();
        HandleAnimationInput();
        HandleTrackSwitch();
    }

    private IEnumerator TurnToForwardCoroutine()
    {
        isTurning = true;
        float elapsedTime = 0f;
        Quaternion startRotation = transform.rotation;

        if (playerAnim != null)
        {
            playerAnim.SetTrigger(TurnLeftTrigger);
        }

        while (elapsedTime < turnDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / turnDuration);
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            yield return null;
        }

        transform.rotation = targetRotation;
        isTurning = false; 

        if (playerAnim != null)
        {
            playerAnim.speed = 1f;
        }
    } 

    // 障碍碰撞检测
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            GameManager.Instance.TriggerGameOver(); 
        }
    }

    // 角色跳跃过程
    private IEnumerator JumpCoroutine()
    {
        isJumping = true;
        float startY = transform.position.y;
        float elapsedTime = 0f;

        while (elapsedTime < jumpDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (jumpDuration / 2);
            float yOffset = Mathf.Sin(t * Mathf.PI / 2) * jumpHeight;
            transform.position = new Vector3(
                transform.position.x,
                startY + yOffset, 
                transform.position.z
            );
            yield return null; 
        }

        elapsedTime = 0f;
        while (elapsedTime < jumpDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (jumpDuration / 2);
            float yOffset = Mathf.Cos(t * Mathf.PI / 2) * jumpHeight;
            transform.position = new Vector3(
                transform.position.x, 
                startY + yOffset, 
                transform.position.z
            );
            yield return null;
        }

        transform.position = new Vector3(transform.position.x, startY, transform.position.z);
        isJumping = false;
    }

    // 角色翻滚状态检测
    private void CheckRollState()
    {
        if (isRolling)
        {
            AnimatorStateInfo currentState = playerAnim.GetCurrentAnimatorStateInfo(0);
            if (!currentState.IsName("RollForward") && !playerAnim.IsInTransition(0))
            {
                playerCollider.height = originalColliderHeight;
                playerCollider.center = originalColliderCenter;
                isRolling = false;
            }
        }
    }

    // 键盘输入检测跳跃、翻滚
    private void HandleAnimationInput()
    {
        if (Input.GetKeyDown(KeyCode.W) && !isJumping && !isRolling)
        {
            playerAnim.SetTrigger(JumpTrigger);
            PlaySoundEffect(actionAudio);
            StartCoroutine(JumpCoroutine());
        }

        if (Input.GetKeyDown(KeyCode.S) && !isJumping && !isRolling)
        {
            playerAnim.SetTrigger(RollTrigger);
            PlaySoundEffect(actionAudio);
            isRolling = true;
            playerCollider.height = originalColliderHeight / 2;
            playerCollider.center = new Vector3(originalColliderCenter.x, originalColliderHeight / 4, originalColliderCenter.z);
        }
    }

    private void PlaySoundEffect(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, soundVolume);
        }
    }

    // 键盘输入检测左右移动
    private void HandleTrackSwitch()
    {
        if (isSwitchingTrack) return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            float targetZ = GetLeftTrackTargetZ();
            if (Mathf.Abs(targetZ - transform.position.z) > TrackZThreshold)
            {
                StartCoroutine(SwitchTrackCoroutine(targetZ, true));
            }
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            float targetZ = GetRightTrackTargetZ();
            if (Mathf.Abs(targetZ - transform.position.z) > TrackZThreshold)
            {
                StartCoroutine(SwitchTrackCoroutine(targetZ, false));
            }
        }
    }

    private float GetLeftTrackTargetZ()
    {
        float currentZ = transform.position.z;
        if (Mathf.Abs(currentZ - MiddleTrackZ) < TrackZThreshold)
        {
            return LeftTrackZ;
        }
        else if (Mathf.Abs(currentZ - RightTrackZ) < TrackZThreshold)
        {
            return MiddleTrackZ;
        }
        return currentZ;
    }

    private float GetRightTrackTargetZ()
    {
        float currentZ = transform.position.z;
        if (Mathf.Abs(currentZ - MiddleTrackZ) < TrackZThreshold)
        {
            return RightTrackZ;
        }
        else if (Mathf.Abs(currentZ - LeftTrackZ) < TrackZThreshold)
        {
            return MiddleTrackZ;
        }
        return currentZ;
    }

    // 实现左右变换道路
    private IEnumerator SwitchTrackCoroutine(float targetZ, bool isLeft)
    {
        isSwitchingTrack = true;
        float velocity = 0f;
        float currentZ = transform.position.z;

        if (!isJumping && !isRolling)
            if (playerAnim != null)
            {
                playerAnim.ResetTrigger(MoveLeftTrigger);
                playerAnim.ResetTrigger(MoveRightTrigger);
                
                if (isLeft)
                    playerAnim.SetTrigger(MoveLeftTrigger);
                else
                    playerAnim.SetTrigger(MoveRightTrigger);
            }

        while (Mathf.Abs(currentZ - targetZ) > 0.01f)
        {
            currentZ = Mathf.SmoothDamp(currentZ, targetZ, ref velocity, trackSwitchDuration);
            transform.position = new Vector3(transform.position.x, transform.position.y, currentZ);
            yield return null;
        }

        transform.position = new Vector3(transform.position.x, transform.position.y, targetZ);
        if (playerAnim != null)
        {
            playerAnim.SetBool(IsRunning, true);
            playerAnim.ResetTrigger(MoveLeftTrigger);
            playerAnim.ResetTrigger(MoveRightTrigger);
        }

        isSwitchingTrack = false;
    }
}
