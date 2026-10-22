using UnityEngine;

public class ManaCorePickup : MonoBehaviour
{
    [Header("References")]
    public WeaponType element;
    public GameObject weaponPrefab;
    public float spinSpeed;
    public float gravity;
    public float objectHeight;
    public string spawnInSFX;
    public LayerMask whatIsGround;

    private bool isGrounded;
    private RaycastHit groundHit;
    private Rigidbody rb;
    private void Start()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(spawnInSFX);

        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        // Checks if ground is beneath mana core, and applies gravity if it is not
        Vector3 posOffset = transform.position;
        posOffset.y -= objectHeight * 0.5f;
        isGrounded = Physics.Raycast(posOffset, Vector3.down, out groundHit, 0.1f, whatIsGround);
        Debug.DrawRay(posOffset, Vector3.down * 0.1f, Color.blue);

        if (!isGrounded)
            rb.AddForce(Vector3.up * gravity, ForceMode.Force);

        // Gives the core a lil spin
        rb.angularVelocity = new Vector3(0f, spinSpeed, 0f);
    }

    public void PickThisUp()
    {
        Destroy(gameObject);
    }
}
