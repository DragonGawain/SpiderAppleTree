using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    static Inputs inputs;

    static Player player;

    private void Awake()
    {
        inputs = new Inputs();
        inputs.Player.Enable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public static void EnableMovementInputs() => inputs.Player.Enable();

    public static void DisableMovementInputs() => inputs.Player.Disable();

    public static void UpdatePlayerRef(Player player) => PlayerManager.player = player;
}
