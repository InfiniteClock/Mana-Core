using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Camera playerCam;
    public Weapon rightWeapon;
    public Weapon leftWeapon;

    private InputAction rightFireInput;
    private InputAction leftFireInput;
    private void OnEnable()
    {
        rightFireInput = InputSystem.actions.FindAction("Right Fire");
        leftFireInput = InputSystem.actions.FindAction("Left Fire");

        // Interaction = Press Only, Press Point = 1
        rightFireInput.performed += RightCharge;
        rightFireInput.canceled += RightFire;
        leftFireInput.performed += LeftCharge;
        leftFireInput.canceled += LeftFire;
    }
    private void OnDisable()
    {
        rightFireInput.performed -= RightCharge;
        rightFireInput.canceled -= RightFire;
        leftFireInput.performed -= LeftCharge;
        leftFireInput.canceled -= LeftFire;
    }
    private void Update()
    {
        Debug.DrawRay(rightWeapon.transform.position, playerCam.transform.forward*100f, Color.cyan);
        Debug.DrawRay(leftWeapon.transform.position, playerCam.transform.forward*100f, Color.cyan);
    }
    private void RightCharge(InputAction.CallbackContext context) => rightWeapon.Charge(); //rightWeapon.Charge();
    private void LeftCharge(InputAction.CallbackContext context) => leftWeapon.Charge(); //leftWeapon.Charge();
    private void RightFire(InputAction.CallbackContext context) => rightWeapon.Shoot(); //rightWeapon.Shoot();
    private void LeftFire(InputAction.CallbackContext context) => leftWeapon.Shoot(); //leftWeapon.Shoot();

}
