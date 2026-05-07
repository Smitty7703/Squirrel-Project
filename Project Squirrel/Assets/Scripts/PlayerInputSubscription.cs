using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSubscription : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    public bool InteractPressed { get; private set; } = false;
    public bool InteractReleased { get; private set; }

    InputSystem_Actions _Input = null;

    private void Awake()
    {
        _Input = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _Input.Player.Enable();

        _Input.Player.Interact.started += OnInteractStarted;
        _Input.Player.Interact.canceled += OnInteractCanceled;

        _Input.Player.Movement.started += OnMovementPerformed;
        _Input.Player.Movement.canceled += OnMovementCanceled;
    }

    private void OnDisable()
    {
        _Input.Player.Disable();

        _Input.Player.Interact.started -= OnInteractStarted;
        _Input.Player.Interact.canceled -= OnInteractCanceled;

        _Input.Player.Movement.started -= OnMovementPerformed;
        _Input.Player.Movement.canceled -= OnMovementCanceled;
    }

    private void Update()
    {
        //Reset frame-based events
        InteractPressed = false;
        InteractReleased = false;
    }

    void OnInteractStarted(InputAction.CallbackContext ctx)
    {
        player.PressInteract();
    }

    void OnInteractCanceled(InputAction.CallbackContext ctx)
    {
        player.ReleaseInteract();
    }

    void OnMovementPerformed(InputAction.CallbackContext ctx)
    {
        player.SetMovement(ctx.ReadValue<Vector2>());
    }

    void OnMovementCanceled(InputAction.CallbackContext ctx)
    {
        player.SetMovement(Vector2.zero);
    }
}
