using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Users;

public class HotkeysController : MonoBehaviour {
  //User New Input Systems
  public PlayerInput newInput;
  InputActionMap inputData;

  public Button[] Hotkeys;

  public GameObject prevSelected;

  private void Awake () {
    inputData = newInput.actions.FindActionMap("Players");
    inputData.Enable();

    prevSelected = Hotkeys[ 0 ].gameObject;

    SubscribeMethod();
  }

  void SubscribeMethod () {
    for (int i = 0; i < Hotkeys.Length; i++) {
      InputAction hotKeys = inputData.FindAction($"Hotkeys{i + 1}");

      SetButtonActions(ref hotKeys, i);
    }

    InputUser.onChange += SchemaHandler;
  }

  void SetButtonActions (ref InputAction hotKeys, int i) {
    Hotkeys[ i ].onClick.AddListener(() => {
      Debug.Log($"Hotkeys {i}");
    });
    hotKeys.performed += _ => {
      Hotkeys[ i ].onClick?.Invoke();
    };
  }

  void SchemaHandler (InputUser userInput, InputUserChange userInputChange, InputDevice deviceInput) {
    if (userInputChange == InputUserChange.ControlSchemeChanged) {
      switch (userInput.controlScheme.Value.name) {
        case "Xbox":
          EventSystem.current.SetSelectedGameObject(prevSelected);
          break;

        case "PC":
          prevSelected = EventSystem.current.currentSelectedGameObject;
          EventSystem.current.SetSelectedGameObject(null);
          break;
      }
    }
  }
}
