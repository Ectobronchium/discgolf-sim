using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThrowUI : MonoBehaviour
{
    public ThrowManager manager;
    public OrbitCamera  orbitCamera;  // drag Main Camera here

    [Header("Sliders")]
    public Slider sliderSpeed;
    public Slider sliderPitch;
    public Slider sliderRoll;
    public Slider sliderNose;
    public Slider sliderWindX;
    public Slider sliderWindY;

    [Header("Labels")]
    public TMP_Text labelSpeed;
    public TMP_Text labelPitch;
    public TMP_Text labelRoll;
    public TMP_Text labelNose;
    public TMP_Text labelWindX;
    public TMP_Text labelWindY;

    [Header("Buttons and dropdown")]
    public TMP_Dropdown discDropdown;
    public Button       throwButton;
    public Button       resetButton;
    public Button       clearButton;
    public Button       cameraToggleButton;
    public TMP_Text     cameraButtonLabel;

    void Start()
    {
        sliderSpeed.minValue = 10f; sliderSpeed.maxValue = 35f;
        sliderPitch.minValue = -10f; sliderPitch.maxValue = 45f;
        sliderRoll.minValue  = -45f; sliderRoll.maxValue = 45f;
        sliderNose.minValue  = -5f; sliderNose.maxValue = 5f;
        sliderWindX.minValue = -10f; sliderWindX.maxValue = 10f;
        sliderWindY.minValue = -10f; sliderWindY.maxValue = 10f;

        SyncSlidersToManager();

        sliderSpeed.onValueChanged.AddListener(v => { manager.SetSpeed(v); UpdateLabels(); });
        sliderPitch.onValueChanged.AddListener(v => { manager.SetPitch(v); UpdateLabels(); });
        sliderRoll.onValueChanged.AddListener(v => { manager.SetRoll(v); UpdateLabels(); });
        sliderNose.onValueChanged.AddListener(v => { manager.SetNose(v); UpdateLabels(); });
        sliderWindX.onValueChanged.AddListener(v => { manager.SetWindX(v); UpdateLabels(); });
        sliderWindY.onValueChanged.AddListener(v => { manager.SetWindY(v); UpdateLabels(); });

        discDropdown.ClearOptions();
        discDropdown.AddOptions(
            new System.Collections.Generic.List<string>(manager.availableDiscs));
        discDropdown.value = manager.discIndex;
        discDropdown.onValueChanged.AddListener(manager.SetDisc);

        throwButton.onClick.AddListener(manager.Throw);

        resetButton.onClick.AddListener(() =>
        {
            manager.ResetParameters();
            SyncSlidersToManager();
            UpdateLabels();
        });

        clearButton.onClick.AddListener(manager.ClearAllThrows);

        cameraToggleButton.onClick.AddListener(() =>
        {
            orbitCamera.ToggleMode();
            UpdateCameraLabel();
        });

        UpdateLabels();
        UpdateCameraLabel();
    }

    void SyncSlidersToManager()
    {
        sliderSpeed.SetValueWithoutNotify(manager.speed);
        sliderPitch.SetValueWithoutNotify(manager.pitchDeg);
        sliderRoll.SetValueWithoutNotify(manager.rollDeg);
        sliderNose.SetValueWithoutNotify(manager.noseDeg);
        sliderWindX.SetValueWithoutNotify(manager.wind.x);
        sliderWindY.SetValueWithoutNotify(manager.wind.y);
    }

    const string ValueColour = "#09165b"; //random dark font colour more or less.

    void UpdateLabels()
    {
        labelSpeed.text =
            $"Speed: <color={ValueColour}>{manager.speed:F1} m/s</color>";

        labelPitch.text =
            $"Pitch: <color={ValueColour}>{manager.pitchDeg:F1}°</color>";

        labelRoll.text =
            $"Roll: <color={ValueColour}>{manager.rollDeg:F1}°</color>";

        labelNose.text =
            $"Nose: <color={ValueColour}>{manager.noseDeg:F1}°</color>";

        float windX = manager.wind.x;
        float windY = manager.wind.y;

        // X is downrange.
        // The disc travels in +X.
        string windXLabel;
        string windXArrow;

        if (windX < 0f)
        {
            windXLabel = "Headwind";
            windXArrow = "↓";
        }
        else if (windX > 0f)
        {
            windXLabel = "Tailwind";
            windXArrow = "↑";
        }
        else
        {
            windXLabel = "Head/Tail Wind";
            windXArrow = "•";
        }

        // Y is sideways.
        // Sim +Y maps to Unity -X, which is visually left.
        string windYArrow;

        if (windY > 0f)
            windYArrow = "←";
        else if (windY < 0f)
            windYArrow = "→";
        else
            windYArrow = "•";

        labelWindX.text =
            $"{windXLabel}: " +
            $"<color={ValueColour}>{Mathf.Abs(windX):0.0} m/s {windXArrow}</color>";

        labelWindY.text =
            $"Crosswind: " +
            $"<color={ValueColour}>{Mathf.Abs(windY):0.0} m/s {windYArrow}</color>";
    }

    void UpdateCameraLabel()
    {
        if (cameraButtonLabel == null) return;
        cameraButtonLabel.text = orbitCamera.ModeName();
    }
}