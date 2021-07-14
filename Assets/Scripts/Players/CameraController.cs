using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {
  public Transform mainCamera;

  public CinemachineTargetGroup targetGroup;
  public CinemachineInputProvider cmInputProvider;
  public CinemachineFreeLook freeLook;

  public float switchDelay = 0.5f;
  public float currentDelay = 0f;

  public CharacterController cc;


  //User New Input Systems
  PlayerInput newInput;
  InputActionMap inputData;

  PlayerMovements movementController;

  public InputActionReference Aiming;

  private void Awake () {
    mainCamera = Camera.main.transform;

    newInput = GetComponent<PlayerInput>();

    movementController = GetComponent<PlayerMovements>();

    movementController.PlayerActions += SwitchTarget;
    movementController.PlayerActions += AimingHandler;

    inputData = newInput.actions.FindActionMap("Players");
    inputData.Enable();

    SubscribeMethod();
  }

  private void LateUpdate () {
    if (currentDelay < switchDelay && LockedOn) {
      currentDelay += Time.deltaTime;
    }
  }

  void SearchTarget () {
    RaycastHit[] allHits = Physics.BoxCastAll(transform.position + Vector3.up * cc.center.y / 2 + transform.forward * 4f, new Vector3(5, 3, 8), mainCamera.forward, Quaternion.identity, 1, -1, QueryTriggerInteraction.Ignore);

    Transform target = null;
    float closestTarget = Mathf.Infinity;

    foreach (RaycastHit hit in allHits) {
      if (hit.collider.CompareTag("Enemy")) {
        if (LockedTo == hit.transform) continue;

        Vector3 targetDirections = hit.transform.position - transform.position;

        float targetDistances = targetDirections.sqrMagnitude;

        if (targetDistances < closestTarget) {
          closestTarget = targetDistances;
          target = hit.transform;
        }
      }
    }

    if (target) {
      LockedTo = target;
    }
  }

  void SubscribeMethod () {
    //Lock On Actions
    InputAction lockButton = inputData.FindAction("LockOn");

    lockButton.performed += _ => {
      if (!LockedOn) {
        SearchTarget();
        freeLook.m_RecenterToTargetHeading.m_enabled = true;
      } else {
        LockedTo = null;
        freeLook.m_RecenterToTargetHeading.m_enabled = false;
      }
    };
  }

  void SwitchTarget () {
    if (currentDelay >= switchDelay) {
      if (LockedOn) {
        //Switch Targets
        Vector2 switchLockOn = Vector2.zero;

        InputAction aiming = inputData.FindAction("Aims");
        switch (newInput.currentControlScheme) {
          case "Xbox":
            switchLockOn = aiming.ReadValue<Vector2>();
            break;
          case "PC":
            switchLockOn = InvertVector2(aiming.ReadValue<Vector2>());
            break;
        }

        if (switchLockOn.x <= -0.1f || switchLockOn.x >= 0.1f) {
          SearchTarget();
          currentDelay = 0f;
        }
      }
    }
  }

  void AimingHandler () {
    if (LockedOn || Cursor.lockState != CursorLockMode.Locked) {
      cmInputProvider.XYAxis = null;
    } else if (Cursor.lockState == CursorLockMode.Locked) {
      cmInputProvider.XYAxis = Aiming;
    }
  }

  Vector2 InvertVector2 (Vector2 vals) {
    return new Vector2(vals.y, vals.x);
  }

  private void OnDrawGizmos () {
    Gizmos.DrawWireCube(transform.position + Vector3.up * cc.center.y / 2 + transform.forward * 4f, new Vector3(5, 3, 8));
  }

  bool LockedOn {
    get {
      return targetGroup.m_Targets[ 1 ].target != null;
    }
  }

  Transform LockedTo {
    get {
      return targetGroup.m_Targets[ 1 ].target;
    }
    set {
      targetGroup.m_Targets[ 1 ].target = value;
    }
  }
}
