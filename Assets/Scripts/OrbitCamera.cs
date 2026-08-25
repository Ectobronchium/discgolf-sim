using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitCamera : MonoBehaviour
{
    public enum Mode { POV, Orbit, TopDown }
    public Mode mode = Mode.POV;

    [Header("Orbit")]
    public Vector3 orbitTarget = new Vector3(0f, 15f, 46f);
    public float distance = 60f;
    public float sensitivity = 0.3f;
    public float scrollSpeed = 0.1f;

    [Header("Top Down")]
    public float topDownHeight = 60f;
    public float topDownZ = 41f;

    [Header("Follow")]
    public Vector3 followOffset = new Vector3(0f, 2f, -8f);
    public float   followSmooth = 5f;

    Vector3    _povPosition;
    Quaternion _povRotation;

    float _yaw;
    float _pitch;

    void Start()
    {
        // Save the Inspector-set position as the POV home
        _povPosition = transform.position;
        _povRotation = transform.rotation;

        // Init orbit angles from current position so no jump on first drag
        Vector3 dir = transform.position - orbitTarget;
        distance = dir.magnitude;
        _yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        _pitch = Mathf.Asin(dir.y / Mathf.Max(distance, 0.01f)) * Mathf.Rad2Deg;
    }

    public void ToggleMode()
    {
        // Cycle POV → Orbit → TopDown → POV
        mode = mode switch
        {
            Mode.POV => Mode.Orbit,
            Mode.Orbit => Mode.TopDown,
            Mode.TopDown => Mode.POV,
            _            => Mode.POV
        };
    }

    public string ModeName() => mode switch
    {
        Mode.POV => "Camera: POV",
        Mode.Orbit => "Camera: Orbit",
        Mode.TopDown => "Camera: Top",
        _            => "Camera"
    };

    void Update()
    {
        switch (mode)
        {
            case Mode.POV: UpdatePOV(); break;
            case Mode.Orbit: UpdateOrbit(); break;
            case Mode.TopDown: UpdateTopDown(); break;
        }
    }

    void UpdatePOV()
    {
        transform.position = _povPosition;
        transform.rotation = _povRotation;
    }

    void UpdateOrbit()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            _yaw += Mouse.current.delta.x.ReadValue() * sensitivity;
            _pitch -= Mouse.current.delta.y.ReadValue() * sensitivity;
            _pitch = Mathf.Clamp(_pitch, -5f, 85f);
        }

        float scroll = Mouse.current.scroll.y.ReadValue();
        if (Mathf.Abs(scroll) > 0.01f)
            distance = Mathf.Clamp(distance - scroll * scrollSpeed * distance, 5f, 300f);

        Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
        transform.position = orbitTarget + rot * new Vector3(0f, 0f, -distance);
        transform.LookAt(orbitTarget);
    }

    void UpdateTopDown()
    {
        Transform latest = GetLatestDisc();
        float scroll = Mouse.current.scroll.y.ReadValue();
        if (Mathf.Abs(scroll) > 0.01f)
            topDownHeight = Mathf.Clamp(topDownHeight - scroll * 2f, 10f, 200f);

        if (latest == null) return;

        // Smoothly follow disc's X and Z, stay at fixed height
        Vector3 target = new Vector3(latest.position.x, topDownHeight, latest.position.z);
        transform.position = Vector3.Lerp(transform.position, target, followSmooth * Time.deltaTime);
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    Transform GetLatestDisc()
    {
        var throws = FindObjectsByType<DiscThrow>(FindObjectsSortMode.None);
        if (throws.Length == 0) return null;
        Transform latest = null;
        int highest = -1;
        foreach (var t in throws)
        {
            string[] parts = t.gameObject.name.Split('_');
            if (parts.Length > 1 && int.TryParse(parts[1], out int idx) && idx > highest)
            {
                highest = idx;
                latest = t.transform;
            }
        }
        return latest;
    }
}