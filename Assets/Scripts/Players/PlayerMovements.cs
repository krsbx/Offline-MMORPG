using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovements : MonoBehaviour {
  //Player Movement Events
  public delegate void TrackPlayerMovements ();
  public TrackPlayerMovements PlayerActions;

  //Main Component for Movements
  public CharacterController cc;
  Vector3 Velocity;

  //Movement Informations
  public float WalkSpeed = 4f;
  public float RunSpeed;
  public float RotationSmooth = 0.2f;
  public float Gravity = 3f;
  public float ConstantGravity = 2;
  public float jumpHeight = 5f;
  public float maxDistanceCheck = 0.1f;
  int DoubleJump = 0;

  [HideInInspector] public bool isRunning = false;
  [HideInInspector] public bool isMoving = false;

  float SmoothVelocity;

  //User New Input Systems
  public PlayerInput newInput;
  InputActionMap inputData;

  RaycastHit hitInfo;

  private void Awake () {
    cc = GetComponent<CharacterController>();

    newInput = GetComponent<PlayerInput>();

    inputData = newInput.actions.FindActionMap("Players");
    inputData.Enable();

    Cursor.lockState = CursorLockMode.Locked;

    SubscribeMethod();
  }

  private void LateUpdate () {
    PlayerActions?.Invoke();
  }

  void MovementController () {
    // Movement Axis
    //  Get All Movement Axis in Horizontal and Vertical
    //  Horizontal Default = A/D
    //  Vertical Default = W/S
    InputAction movementInput = inputData.FindAction("Movements");

    Vector2 movements = movementInput.ReadValue<Vector2>();

    float Horizontal = movements.x;
    float Vertical = movements.y;

    //Get Player Directions
    Vector3 directions = new Vector3(Horizontal, 0, Vertical).normalized;

    if (directions.magnitude >= 0.1f) {
      // Player Angle Directions
      float directionsAngle = Mathf.Atan2(directions.x, directions.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
      float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, directionsAngle, ref SmoothVelocity, RotationSmooth);

      //Set Player Directions
      transform.rotation = Quaternion.Euler(0f, angle, 0f);

      Vector3 moveDirections = Quaternion.Euler(0f, directionsAngle, 0f) * Vector3.forward;

      isMoving = true;

      //Move Player
      switch (newInput.currentControlScheme) {
        case "Xbox":
          isRunning = movements.magnitude > 0.5;

          cc.Move(isRunning ? moveDirections * RunSpeed * Time.deltaTime : moveDirections * WalkSpeed * Time.deltaTime);
          break;
        case "PC":
          cc.Move(isRunning ? moveDirections * RunSpeed * Time.deltaTime : moveDirections * WalkSpeed * Time.deltaTime);
          break;
      }

    } else {
      isRunning = false;
      isMoving = false;
    }

    if (isGrounded && Velocity.y < 0) {
      Velocity.y = ConstantGravity;
      DoubleJump = 0;
    } else {
      Velocity.y += Gravity * Time.deltaTime;
    }

    cc.Move(Velocity * Time.deltaTime);
  }

  public bool isGrounded {
    get { return GroundChecker(out hitInfo); }
  }

  bool GroundChecker (out RaycastHit hit) {
    // Physics.Raycast(transform.position + Vector3.down * ( cc.height / 2 - cc.center.y ), Vector3.down, out hit, 0.1f, -1, QueryTriggerInteraction.Ignore);

    // Physics.SphereCast(transform.position + Vector3.down * ( cc.height / 2 - cc.center.y ), cc.radius / 2, Vector3.down, out hit, maxDistanceCheck, -1, QueryTriggerInteraction.Ignore);
    return Physics.SphereCast(transform.position, cc.radius, Vector3.down, out hit, ( cc.height / 2 - cc.center.y ) + maxDistanceCheck, -1, QueryTriggerInteraction.Ignore);
  }

  private void OnDrawGizmos () {
    Gizmos.DrawWireSphere(transform.position + Vector3.down * ( cc.height / 2 - cc.center.y ), cc.radius);
  }

  void Jump () {
    Velocity.y = Mathf.Sqrt(jumpHeight * ConstantGravity * Gravity);
    DoubleJump++;
  }

  void SubscribeMethod () {
    //Subscribet PlayerActions
    PlayerActions += MovementController;

    //Jump Actions
    InputAction jumpButton = inputData.FindAction("Jump");

    jumpButton.performed += _ => {
      if (isGrounded || DoubleJump < 2) {
        Jump();
      }
    };

    //Cursor/Interract Actions
    InputAction escButton = inputData.FindAction("Escape");
    escButton.performed += _ => {
      if (newInput.currentControlScheme == "PC") {
        if (Cursor.lockState == CursorLockMode.Locked) {
          Cursor.lockState = CursorLockMode.None;
        } else {
          Cursor.lockState = CursorLockMode.Locked;
        }
      }
    };

    //Run Actions
    InputAction runButton = inputData.FindAction("Run");

    runButton.performed += _ => {
      isRunning = true;
    };

    runButton.canceled += _ => {
      isRunning = false;
    };

    newInput.onControlsChanged += (Action) => {
      Cursor.lockState = CursorLockMode.Locked;
    };
  }
}
