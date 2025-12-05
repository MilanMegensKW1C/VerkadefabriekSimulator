using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField]
    Transform character;
    public float sensitivity = 2;
    public float smoothing = 1.5f;

    // interne state
    Vector2 velocity;
    Vector2 frameVelocity;

    void Reset()
    {
        var pm = GetComponentInParent<FirstPersonMovement>();
        if (pm != null) character = pm.transform;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        // initialiseer velocity vanaf huidige rotatie (voorkomt snap naar default)
        if (character != null)
        {
            float yaw = character.eulerAngles.y;
            float pitch = transform.localEulerAngles.x;
            // local pitch can be 360-> convert to -range
            if (pitch > 180) pitch -= 360f;
            velocity = new Vector2(yaw, -pitch);
        }
    }

    void Update()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1f / smoothing);
        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -90, 90);

        transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
        if (character != null)
            character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
    }

    // -------------------------
    // Public helper: forceer rotatie en sync internal velocity
    // worldEuler in degrees (x = pitch, y = yaw)
    // -------------------------
    public void ApplyRotationEuler(Vector3 worldEuler)
    {
        // normaliseer pitch in -180..180
        float pitch = worldEuler.x;
        if (pitch > 180) pitch -= 360f;

        if (character != null)
        {
            // zet character yaw
            character.rotation = Quaternion.Euler(0f, worldEuler.y, 0f);

            // zet camera pitch (local)
            transform.localRotation = Quaternion.Euler(-pitch, 0f, 0f);

            // sync internal velocity (yaw, -pitch)
            velocity = new Vector2(worldEuler.y, pitch * -1f);
            velocity.y = Mathf.Clamp(velocity.y, -90, 90);
        }
        else
        {
            transform.rotation = Quaternion.Euler(worldEuler);
            velocity = new Vector2(worldEuler.y, pitch * -1f);
            velocity.y = Mathf.Clamp(velocity.y, -90, 90);
        }
    }
}
