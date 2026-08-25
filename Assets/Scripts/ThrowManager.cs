using UnityEngine;
using System.Collections.Generic;

public class ThrowManager : MonoBehaviour
{
    [Header("Disc")]
    public string[] availableDiscs;
    public int discIndex = 0;
    public float mass = 0.175f;

    [Header("Throw parameters")]
    public float speed = 24.2f;
    public float omega = 116.8f;
    public float pitchDeg = 15.5f;
    public float rollDeg = 14.7f;
    public float noseDeg = 0f;
    public float yawDeg = 0f;
    public float releaseHeight = 1.3f;

    [Header("Wind (sim space: x=downrange, y=sideways, z=up)")]
    public Vector3 wind = Vector3.zero;

    [Header("Settings")]
    public int maxThrows = 5;

    // Colour palette of 5 colours that cycle
    static readonly Color[] Palette =
    {
        new Color(0.2f, 0.8f, 1.0f),  // sægrænn
        new Color(1.0f, 0.5f, 0.1f),  // appelsínugulur
        new Color(0.4f, 1.0f, 0.4f),  // grænn
        new Color(1.0f, 0.3f, 0.5f),  // bleikur
        new Color(0.9f, 0.9f, 0.2f),  // gulur
    };

    readonly List<DiscThrow> _throws = new List<DiscThrow>();
    int _throwCount = 0;

    void Update()
    {
        // Space bar to throw disc, should implement some info in the app
        if (Input.GetKeyDown(KeyCode.Space))
            Throw();
    }

    [Header("Prefab")]
    public GameObject discPrefab;

    public void Throw()
    {
        if (_throws.Count >= maxThrows)
        {
            Destroy(_throws[0].gameObject);
            _throws.RemoveAt(0);
        }

        Color colour = Palette[_throwCount % Palette.Length];
        _throwCount++;

        var aero = new DiscAerodynamics(availableDiscs[discIndex], mass);
        var model = new DiscFlightModel(aero, omega);

        GameObject go;
        if (discPrefab != null)
            go = Instantiate(discPrefab);
        else
        {
            go = new GameObject($"Throw_{_throwCount}");
            go.AddComponent<LineRenderer>();
        }
        go.name = $"Throw_{_throwCount}";

        var disc = go.GetComponent<DiscThrow>() ?? go.AddComponent<DiscThrow>();
        disc.model = model;
        disc.colour = colour;
        disc.GetWind = () => wind;

        // Initialize
        float pitch = pitchDeg * Mathf.Deg2Rad;
        float yaw = yawDeg * Mathf.Deg2Rad;

        Vector3 position = new Vector3(0f, 0f, releaseHeight);
        Vector3 velocity = new Vector3(
            speed * Mathf.Cos(pitch) * Mathf.Cos(yaw),
            speed * Mathf.Cos(pitch) * Mathf.Sin(-yaw),
            speed * Mathf.Sin(pitch)
        );
        Vector3 attitude = new Vector3(rollDeg * Mathf.Deg2Rad, noseDeg * Mathf.Deg2Rad, 0f);
        attitude += Transforms.T_12(attitude).Transpose().MultiplyVector(new Vector3(0f, pitch, 0f));
        disc.Initialize(position, velocity, attitude);
        _throws.Add(disc);
    }     
    public void ClearAllThrows()
    {
        foreach (var t in _throws)
            if (t != null) Destroy(t.gameObject);
        _throws.Clear();
    }

    public void ResetParameters()
    {
        discIndex = 0;
        speed = 24.2f;
        pitchDeg = 15.5f;
        rollDeg = 14.7f;
        noseDeg = 0f;
        yawDeg = 0f;
        wind = Vector3.zero;
    }      


    public void SetDisc(int index) => discIndex = index;
    public void SetSpeed(float v) => speed = v;
    public void SetPitch(float v) => pitchDeg = v;
    public void SetRoll(float v) => rollDeg = v;
    public void SetNose(float v) => noseDeg = v;
    public void SetWindX(float v) => wind.x = v;
    public void SetWindY(float v) => wind.y = v;
}