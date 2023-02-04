using FishNet.Object;
using System.Collections;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Stats")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    private float currentSpeed;

    [Space, SerializeField] private float jumpForce;

    [Space, SerializeField] private float slidingSpeed;
    [SerializeField]private float slidingTime;

    [Header("Groud check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float radius;
    [SerializeField] private LayerMask groudLayer;

    [Header("Components")]
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private CapsuleCollider capsuleCollider;

    private Vector3 move;

    private bool isRun = false;
    private bool isSliding = false;

    private float moveVertical;
    private float moveHorizontal;

    private const string inputY = "Vertical";
    private const string inputX = "Horizontal";

    private void Start()
    {
        currentSpeed = walkSpeed;
    }

    private void Update()
    {
        if (!IsOwner) { return; }

        Inputs();
        Jump();
        Run();
        Slide();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) { return; }

        Move();
    }

    private void Inputs()
    {
        moveVertical = Input.GetAxis(inputY);
        moveHorizontal = Input.GetAxis(inputX);
    }

    private void Move()
    {
        if (isSliding) { return; }

        move = transform.forward * moveVertical + transform.right * moveHorizontal;

        _rigidbody.MovePosition(transform.position + move * currentSpeed * Time.deltaTime);
    }

    private void Jump()
    {
        if ((Input.GetKeyDown(KeyCode.Space)) && (IsGrounded()))
        {
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Force);
        }
    }

    private void Run()
    {
        if ((Input.GetKey(KeyCode.LeftShift)) && (!isSliding))
        {
            weaponController.IsCanShoot = false;
            weaponController.CurrentWeapon.IsAiming = false;

            weaponController.CurrentWeapon.RunAnim(true);

            currentSpeed = runSpeed;

            isRun = true;
        }
        else
        {
            weaponController.IsCanShoot = true;

            weaponController.CurrentWeapon.RunAnim(false);

            currentSpeed = walkSpeed;

            isRun = false;
        }
    }

    private void Slide()
    {
        if ((Input.GetKey(KeyCode.C)) && (isRun) && (!isSliding))
        {
            StartCoroutine(StartSliding());
        }
    }

    private IEnumerator StartSliding()
    {
        isSliding = true;

        weaponController.CurrentWeapon.SlidingAnim(true);
        capsuleCollider.height = 1;

        float m_currentTime = 0;
        Vector3 m_direction = move;

        while (slidingTime > m_currentTime)
        {
            m_currentTime += Time.deltaTime;

            _rigidbody.MovePosition(transform.position + m_direction * slidingSpeed * Time.deltaTime);

            yield return new WaitForFixedUpdate();
        }

        isSliding = false;

        capsuleCollider.height = 2;
        weaponController.CurrentWeapon.SlidingAnim(false);
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, radius, groudLayer);
    }
}
