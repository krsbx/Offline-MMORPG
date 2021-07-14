using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimations : MonoBehaviour {
  public Animator playerAnimator;
  public PlayerMovements movementController;

  //User New Input Systems
  public PlayerInput newInput;
  InputActionMap inputData;

  //Attack Counter
  int attackCounter = 1;
  float attackReseter = 0f;
  float attackResetIn = 1f;

  private void Awake () {
    newInput = GetComponent<PlayerInput>();

    movementController = GetComponent<PlayerMovements>();

    inputData = newInput.actions.FindActionMap("Players");
    inputData.Enable();

    SubscribeMethod();
  }

  private void LateUpdate () {
    playerAnimator.SetBool("Walk", isMoving);
    playerAnimator.SetBool("Run", Runnable);

    if (attackCounter > 1 && attackReseter < attackResetIn) {
      attackReseter += Time.deltaTime;
    } else {
      attackCounter = 1;
      attackReseter = 0f;
    }

    AnimatorStateInfo animatorStateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);

    bool isDodging = animatorStateInfo.IsName("Dodge");

    if (!isDodging) {
      cc.center = new Vector3(0f, 0.5f, 0f);
      cc.height = 3f;
    }
  }

  void SubscribeMethod () {
    InputAction attackButton = inputData.FindAction("Attacks");
    attackButton.performed += _ => {
      if (attackCounter <= 3) {
        playerAnimator.SetTrigger($"Sword{attackCounter}");
        attackCounter++;
      }
    };

    InputAction dodgeButton = inputData.FindAction("Dodge");

    dodgeButton.started += _ => {
      if (Runnable && isGrounded) {
        playerAnimator.Play("Dodge", 0);

        cc.center = new Vector3(0f, 0f, 0f);
        cc.height = 1f;
      }
    };
  }

  bool isMoving {
    get { return movementController.isMoving; }
  }

  bool Runnable {
    get {
      return isMoving && isRunning;
    }
  }

  CharacterController cc {
    get { return movementController.cc; }
  }

  bool isRunning {
    get { return movementController.isRunning; }
  }

  bool isGrounded {
    get {
      return movementController.isGrounded;
    }
  }
}
