using UnityEngine;

public class Player : MonoBehaviour
{
    public Camera playerCam;
    public Weapon rightWeapon;
    public Weapon leftWeapon;

    private void Update()
    {
        Debug.DrawRay(rightWeapon.transform.position, playerCam.transform.forward*100f, Color.cyan);
        Debug.DrawRay(leftWeapon.transform.position, playerCam.transform.forward*100f, Color.cyan);

        if (Input.GetMouseButtonDown(0))
        {
            rightWeapon.Charge();
        }
        if (Input.GetMouseButtonDown(1))
        {
            leftWeapon.Charge();
        }
        if (Input.GetMouseButtonUp(0))
        {
            rightWeapon.Shoot();
        }
        if (Input.GetMouseButtonUp(1))
        {
            leftWeapon.Shoot();
        }
    }

}
