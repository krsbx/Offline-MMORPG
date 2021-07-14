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

  bool isRunning {
    get { return movementController.isRunning; }
  }

  bool isGrounded {
    get {
      return movementController.isGrounded;
    }
  }
}
