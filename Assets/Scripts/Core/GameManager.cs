using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Owns the win/lose rules: collect all treasure boxes AND eliminate every guard to win;
/// lose all lives to lose. Press R after the game ends to restart.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int totalTreasures = 3;
    public AudioClip winClip, loseClip;

    public int TreasuresCollected { get; private set; }
    public int Kills { get; private set; }
    public int GuardsAlive { get; private set; }
    public bool Ended { get; private set; }
    public bool Won { get; private set; }
    public bool AllTreasuresCollected => TreasuresCollected >= totalTreasures;

    public event Action StatsChanged;
    public event Action<bool> GameEnded;   // true = won

    PlayerHealth player;
    InputAction restartAction;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        restartAction = new InputAction("Restart", InputActionType.Button, "<Keyboard>/r");
    }

    void OnEnable() => restartAction.Enable();
    void OnDisable() => restartAction.Disable();
    void OnDestroy() { restartAction.Dispose(); if (Instance == this) Instance = null; }

    void Start()
    {
        player = FindFirstObjectByType<PlayerHealth>();
        if (player != null) player.Died += OnPlayerDied;
        StatsChanged?.Invoke();
    }

    void Update()
    {
        if (Ended && restartAction.WasPressedThisFrame())
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RegisterGuard()
    {
        GuardsAlive++;
        StatsChanged?.Invoke();
    }

    public void GuardKilled()
    {
        GuardsAlive--;
        Kills++;
        StatsChanged?.Invoke();
        CheckWin();
    }

    public void TreasureCollected()
    {
        TreasuresCollected++;
        StatsChanged?.Invoke();
        CheckWin();
    }

    void CheckWin()
    {
        if (!Ended && AllTreasuresCollected && GuardsAlive <= 0) EndGame(true);
    }

    void OnPlayerDied()
    {
        if (!Ended) EndGame(false);
    }

    void EndGame(bool won)
    {
        Ended = true;
        Won = won;

        if (player != null)
        {
            Sfx.Play(won ? winClip : loseClip, player.transform.position);
            var pc = player.GetComponent<PlayerController>();
            var pw = player.GetComponent<PlayerWeapons>();
            if (pc != null) pc.InputEnabled = false;
            if (pw != null) pw.InputEnabled = false;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameEnded?.Invoke(won);
    }
}
