using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D _rigidbody;

    [SerializeField] private float moveSpeed = 5f;

    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.down;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        _rigidbody.linearVelocity = moveInput * moveSpeed;

        if (moveInput.sqrMagnitude > 0.01f)     
        {
            lastMoveDirection = moveInput.normalized;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public Vector2 GetLastMoveDirection()
    {
        return lastMoveDirection;
    }
}
