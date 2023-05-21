using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePosition3D : MonoBehaviour
{
  [SerializeField] private Camera mainCamera;
  public Vector3 mouseWorldPosition;
  private PlayerInputSystem playerInput;

  // Start is called before the first frame update
  void Awake()
  {
    playerInput = new PlayerInputSystem();
    OnEnable();
  }

  void Update()
  {

    mouseWorldPosition = Vector3.zero;

    Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
    Ray ray = mainCamera.ScreenPointToRay(screenCenterPoint);

    if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f))
    {
      transform.position = raycastHit.point;
      mouseWorldPosition = raycastHit.point;
    }
  }

  private void OnEnable()
  {
    playerInput.Enable();
  }


  private void OnDisable()
  {
    playerInput.Disable();
  }
}
