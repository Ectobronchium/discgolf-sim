using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class DiscThrow : MonoBehaviour
{
    public DiscFlightModel model;
    public Color colour;
    public System.Func<Vector3> GetWind;

    Transform _discVisual;
    LineRenderer _line;
    TrailRenderer _trail;
    List<Vector3> _points = new List<Vector3>();

    Vector3 _position;
    Vector3 _velocity;
    Vector3 _attitude;
    bool _inFlight;
    const float MaxPathSegmentLength = 0.05f;
    Vector3 _lastPathPosition;
    bool _hasLastPathPosition;

    public bool InFlight => _inFlight;

    public void Initialize(Vector3 position, Vector3 velocity, Vector3 attitude)
    {
        _position = position;
        _velocity = velocity;
        _attitude = attitude;
        _inFlight = true;

        _line = GetComponent<LineRenderer>();
        SetupVisual();
        transform.position = SimToUnity(_position);
    }

    void SetupVisual()
    {

        // Disc (will be a 3D model if I have the time)
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        // Needs to be a bit bigger than IRL to feel right visually
        float d = model.aero.Diameter * 2.5f;
        go.transform.localScale = new Vector3(d, 0.015f, d);

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", colour);
        // Emission to see the disc
        mat.SetColor("_EmissionColor", colour * 1.5f);
        mat.EnableKeyword("_EMISSION");
        go.GetComponent<Renderer>().material = mat;
        Destroy(go.GetComponent<Collider>());
        _discVisual = go.transform;

        // The actual flight path. Still a bit choppy
        _line.useWorldSpace = true;

        var lineMaterial = new Material(
            Shader.Find("Universal Render Pipeline/Unlit")
        );

        lineMaterial.SetColor("_BaseColor", colour);

        _line.material = lineMaterial;

        _line.startWidth = 0.15f;
        _line.endWidth = 0.15f;

        _line.numCornerVertices = 4;
        _line.numCapVertices = 4;

        _line.positionCount = 0;

        _line.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;

        _line.receiveShadows = false;
        _line.alignment = LineAlignment.View;

        var grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(colour, 0f),
                new GradientColorKey(colour, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.6f, 0f),
                new GradientAlphaKey(0.6f, 1f)
            }
        );
        _line.colorGradient = grad;

        // Speed trail
        var trailGo = new GameObject("Trail");
        trailGo.transform.SetParent(transform);
        trailGo.transform.localPosition = Vector3.zero;

        _trail = trailGo.AddComponent<TrailRenderer>();
        _trail.material = new Material(Shader.Find("Sprites/Default"));
        _trail.time = 0.2f;
        _trail.minVertexDistance = 0.02f;
        _trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _trail.receiveShadows = false;

        var trailWidth = new AnimationCurve();
        trailWidth.AddKey(0f, 0.4f);   // disc width
        trailWidth.AddKey(1f, 0f);
        _trail.widthCurve = trailWidth;

        var trailGrad = new Gradient();
        trailGrad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(colour, 0.3f),
                new GradientColorKey(colour, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0.5f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        _trail.colorGradient = trailGrad;
    }

    void FixedUpdate()
    {
        if (!_inFlight || model == null)
            return;

        if (_position.z <= 0f)
        {
            _inFlight = false;

            if (_trail != null)
                _trail.emitting = false;

            Debug.Log(
                $"Landed at x={_position.x:F1}m  y={_position.y:F1}m"
            );

            return;
        }

        Vector3 wind = GetWind != null
            ? GetWind()
            : Vector3.zero;

        float dt = Time.fixedDeltaTime;

        model.Advance(
            _position,
            _velocity,
            _attitude,
            wind,
            out Vector3 vel,
            out Vector3 acc,
            out Vector3 angvel
        );

        _position += vel * dt;
        _velocity += acc * dt;
        _attitude += angvel * dt;

        Vector3 unityPosition = SimToUnity(_position);

        transform.position = unityPosition;
        transform.rotation = SimToUnityRotation(_attitude);

        AddPathPoints(unityPosition);
    }

    Vector3 SimToUnity(Vector3 p) => new Vector3(-p.y, p.z, p.x);

    Quaternion SimToUnityRotation(Vector3 attitude)
    {
        return Quaternion.Euler(
            -attitude.y * Mathf.Rad2Deg,
            -attitude.z * Mathf.Rad2Deg,
             attitude.x * Mathf.Rad2Deg
        );
    }
    void AddPathPoints(Vector3 newPosition)
    {
        if (!_hasLastPathPosition)
        {
            _points.Add(newPosition);

            _lastPathPosition = newPosition;
            _hasLastPathPosition = true;

            _line.positionCount = _points.Count;
            _line.SetPosition(_points.Count - 1, newPosition);

            return;
        }

        float distance = Vector3.Distance(_lastPathPosition, newPosition);

        int segments = Mathf.Max(
            1,
            Mathf.CeilToInt(distance / MaxPathSegmentLength)
        );

        int startIndex = _points.Count;

        _line.positionCount += segments;

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;

            Vector3 point = Vector3.Lerp(
                _lastPathPosition,
                newPosition,
                t
            );

            _points.Add(point);
            _line.SetPosition(startIndex + i - 1, point);
        }

        _lastPathPosition = newPosition;
    }
}