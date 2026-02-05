using UnityEngine;

public class AutoJuice : MonoBehaviour
{
    private UIJuice juice;

    private void Awake() => juice = GetComponent<UIJuice>();

    private void OnEnable()
    {
        // Start with the Entrance "Pop", then chain into the Loop
        juice.PlayEntrance();

        // Wait for entrance to finish, then pulse
        Invoke("StartLoop", juice.duration);
    }

    private void StartLoop() => juice.PlayPulseLoop();
}