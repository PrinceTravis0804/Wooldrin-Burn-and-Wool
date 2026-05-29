using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGameHUDManager : MonoBehaviour
{
    public static InGameHUDManager Instance { get; private set; }

    [Header("Dynamic Heart Display (Top-Left)")]
    [Tooltip("The main UI Image component that shows Wooldrin's liquid health heart.")]
    public Image singleHeartImage;
    [Tooltip("4 sliced heart sprites representing health levels in order: [0] Empty, [1] 1/3, [2] 2/3, [3] Full.")]
    public Sprite[] heartSprites;

    [Header("Single-Icon Wool Indicator (Bottom-Right)")]
    [Tooltip("The single UI Image component on your pedestal shelf representing the Wool Ability.")]
    public Image singleWoolIndicator;
    [Tooltip("4 sliced circular wool sprites in order: [0] Full, [1] 2/3, [2] 1/3, [3] Empty ring.")]
    public Sprite[] woolChargeSprites;

    [Header("Single-Icon Fire Indicator (Bottom-Right)")]
    [Tooltip("The single UI Image component on your pedestal shelf representing the Fire Spirit.")]
    public Image singleFireIndicator;
    [Tooltip("4 sliced circular fire sprites in order: [0] Full, [1] 2/3, [2] 1/3, [3] Empty ring.")]
    public Sprite[] fireChargeSprites;

    [Header("Dynamic Custom Color Settings")]
    [Tooltip("Color tint of the skill icon when it is fully charged and ready to cast.")]
    public Color readyColor = Color.white;
    [Tooltip("Color tint of the skill icon when it is locked out or recharging (Slowly regenerating).")]
    public Color cooldownColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    [Header("Menus & Panels")]
    [Tooltip("The parent GameObject containing your Pause Menu canvas panel.")]
    public GameObject pauseMenuPanel;
    [Tooltip("The parent GameObject containing your Settings/Controls submenu panel.")]
    public GameObject settingsMenuPanel;

    private WooldrinHealth playerHealth;
    private PlayerController playerController;
    private FireSpiritController fireSpirit;
    private bool isPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        FindPlayerReferences();

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsMenuPanel != null) settingsMenuPanel.SetActive(false);
    }

    private void Update()
    {
        if (playerHealth == null || playerController == null)
        {
            FindPlayerReferences();
        }

        // Toggle pause menu
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive) return;
            TogglePause();
        }

        UpdateHUDVisuals();
    }

    private void FindPlayerReferences()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerHealth = playerObj.GetComponent<WooldrinHealth>();
            playerController = playerObj.GetComponent<PlayerController>();
        }

        if (fireSpirit == null)
        {
            fireSpirit = FindObjectOfType<FireSpiritController>();
        }
    }

    private void UpdateHUDVisuals()
    {
        // 1. Dynamic Heart State Update
        if (playerHealth != null && singleHeartImage != null && heartSprites != null && heartSprites.Length >= 4)
        {
            int healthLevel = Mathf.Clamp(playerHealth.woolLayers, 0, 3);
            singleHeartImage.sprite = heartSprites[healthLevel];
        }

        // If player reference is lost, freeze interface progress
        if (playerController == null) return;

        // 2. Animate and Darken Single Wool Indicator
        if (singleWoolIndicator != null && woolChargeSprites != null && woolChargeSprites.Length >= 4)
        {
            float totalWoolLevel = GetChargeProgressRatio(
                playerController.CurrentWoolCharges,
                playerController.MaxWoolCharges,
                playerController.WoolRechargeTimer,
                playerController.WoolRechargeDuration
            );

            // Set the circular liquid fill sprite frame
            singleWoolIndicator.sprite = GetSpriteFromRatio(totalWoolLevel, woolChargeSprites);

            // Darken the icon ONLY when completely locked out (all charges spent)
            if (playerController.IsWoolLockedOut)
            {
                singleWoolIndicator.color = cooldownColor;
            }
            else
            {
                singleWoolIndicator.color = readyColor;
            }
        }

        // 3. Animate and Darken Single Fire Indicator
        if (singleFireIndicator != null && fireChargeSprites != null && fireChargeSprites.Length >= 4)
        {
            float totalFireLevel = GetChargeProgressRatio(
                playerController.CurrentFireCharges,
                playerController.MaxFireCharges,
                playerController.FireRechargeTimer,
                playerController.FireRechargeDuration
            );

            // Set the circular liquid fill sprite frame
            singleFireIndicator.sprite = GetSpriteFromRatio(totalFireLevel, fireChargeSprites);

            // Darken the icon ONLY when completely locked out (all charges spent)
            if (playerController.IsFireLockedOut)
            {
                singleFireIndicator.color = cooldownColor;
            }
            else
            {
                singleFireIndicator.color = readyColor;
            }
        }
    }

    /// <summary>
    /// Combines current whole charges and fractional recharge progress into a unified percentage (0.0 to 1.0)
    /// </summary>
    private float GetChargeProgressRatio(int current, int max, float timer, float duration)
    {
        if (max <= 0) return 0f;

        float progressFraction = 0f;
        if (current < max && duration > 0f)
        {
            progressFraction = timer / duration;
        }

        float totalValue = current + progressFraction;
        return Mathf.Clamp01(totalValue / max);
    }

    /// <summary>
    /// Maps a 0.0-1.0 progress ratio directly to your 4 sliced circular sprite frames:
    /// Ratio >= 0.85        -> Sprite [0] (Full Liquid)
    /// Ratio 0.50 to 0.85   -> Sprite [1] (2/3 Liquid)
    /// Ratio 0.15 to 0.50   -> Sprite [2] (1/3 Liquid)
    /// Ratio < 0.15         -> Sprite [3] (Empty Outline)
    /// </summary>
    private Sprite GetSpriteFromRatio(float ratio, Sprite[] sprites)
    {
        if (ratio >= 0.85f) return sprites[0]; // Full
        if (ratio >= 0.50f) return sprites[1]; // 2/3 Full
        if (ratio >= 0.15f) return sprites[2]; // 1/3 Full
        return sprites[3];                     // Empty
    }

    // --- PAUSE MENU SYSTEM ---
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
            Cursor.visible = true;
        }
        else
        {
            ResumeGame();
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsMenuPanel != null) settingsMenuPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        if (settingsMenuPanel != null) settingsMenuPanel.SetActive(true);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }

    public void CloseSettings()
    {
        if (settingsMenuPanel != null) settingsMenuPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }

    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartLevel();
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToMainMenu();
        }
    }
}