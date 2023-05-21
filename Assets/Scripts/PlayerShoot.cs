using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;
using UnityEngine.VFX;

public class PlayerShoot : MonoBehaviour
{
  public bool isAim = false;
  public GameObject aimCamera;
  public GameObject mainCamera;
  public GameObject aimReticle;
  public Rigidbody bullet;
  public GameObject bulletSpawn;

  public MousePosition3D mousePosition3D;
  public VisualEffect visualEffect;


  public float bulletForce = 20f;
  public float coolDown = 0.8f;

  private PlayerInputSystem playerInput;
  private Animator animator;
  private RigBuilder rigBuilder;
  private bool isShooting = false;

  // Start is called before the first frame update
  void Awake()
  {
    playerInput = new PlayerInputSystem();
    animator = GetComponent<Animator>();
    rigBuilder = GetComponent<RigBuilder>();
    OnEnable();

    playerInput.Player.Aim.performed += Aim;
    playerInput.Player.Shoot.performed += Shoot;
  }

  private void FixedUpdate()
  {
    isAim = playerInput.Player.Aim.ReadValue<float>() > 0;

    if (!isAim)
    {
      animator.SetFloat("isAim", 0);
      rigBuilder.enabled = false;


      if (!mainCamera.activeInHierarchy)
      {
        mainCamera.SetActive(true);
        aimCamera.SetActive(false);
        aimReticle.SetActive(false);

      }
    }
  }

  private void Aim(InputAction.CallbackContext context)
  {
    if (context.performed && !isAim)
    {
      isAim = true;
      animator.SetFloat("isAim", 1);
      rigBuilder.enabled = true;

      if (!aimCamera.activeInHierarchy)
      {
        aimCamera.SetActive(true);
        aimReticle.SetActive(true);
        mainCamera.SetActive(false);
      }
    }
  }

  private void Shoot(InputAction.CallbackContext context)
  {
    if (context.performed && isAim && !isShooting)
    {
      animator.SetTrigger("Shoot");
      visualEffect.Play();
      Transform spawnPosition = bulletSpawn.transform;
      Vector3 aimDir = (mousePosition3D.mouseWorldPosition - spawnPosition.position).normalized;

      Rigidbody spawnedBullet = Instantiate(bullet, spawnPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
      spawnedBullet.AddForce(spawnPosition.forward * bulletForce, ForceMode.Impulse);
      isShooting = true;

      Invoke("ResetCoolDown", coolDown);
    }
  }

  private void ResetCoolDown()
  {
    isShooting = false;
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
