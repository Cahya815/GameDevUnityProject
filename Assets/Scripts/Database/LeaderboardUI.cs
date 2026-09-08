using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject leaderboardPanel;
    public Transform leaderboardContent;
    public GameObject leaderboardEntryPrefab;
    public Button refreshButton;
    public Button closeButton;
    public Text statusText;

    [Header("Settings")]
    public int topPlayersCount = 10;
    public string loadingMessage = "Loading leaderboard...";
    public string errorMessage = "Failed to load leaderboard";

    private List<GameObject> spawnedEntries = new List<GameObject>();

    void Start()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(() => RefreshLeaderboard());

        if (closeButton != null)
            closeButton.onClick.AddListener(() => HideLeaderboard());

        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);
    }

    public void ShowLeaderboard()
    {
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(true);

        RefreshLeaderboard();
    }

    public void HideLeaderboard()
    {
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);
    }

    public async void RefreshLeaderboard()
    {
        await FetchAndDisplayLeaderboard();
    }

    private async Task FetchAndDisplayLeaderboard()
    {
        try
        {
            if (statusText != null)
            {
                statusText.text = loadingMessage;
                statusText.gameObject.SetActive(true);
            }

            ClearSpawnedEntries();

            var entries = await GameDataManager.instance.GetLeaderboard(topPlayersCount);

            if (entries == null || entries.Length == 0)
            {
                if (statusText != null)
                    statusText.text = "No leaderboard data available";
                return;
            }

            if (statusText != null)
                statusText.gameObject.SetActive(false);

            for (int i = 0; i < entries.Length; i++)
            {
                CreateLeaderboardEntry(i + 1, entries[i]);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Leaderboard error: {ex.Message}");
            if (statusText != null)
                statusText.text = errorMessage;
        }
    }

    private void CreateLeaderboardEntry(int rank, LeaderboardEntry entry)
    {
        if (leaderboardContent == null || leaderboardEntryPrefab == null)
            return;

        GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContent);
        spawnedEntries.Add(entryObj);

        Text[] texts = entryObj.GetComponentsInChildren<Text>();
        if (texts.Length >= 4)
        {
            texts[0].text = rank.ToString();
            texts[1].text = entry.player_name;
            texts[2].text = entry.fires_extinguished.ToString();
            texts[3].text = FormatDuration(entry.duration);
        }

        if (rank <= 3)
        {
            Image bg = entryObj.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = rank == 1 ? Color.yellow :
                           rank == 2 ? new Color(0.75f, 0.75f, 0.75f) :
                           new Color(0.8f, 0.5f, 0.2f);
            }
        }
    }

    private string FormatDuration(int seconds)
    {
        int mins = seconds / 60;
        int secs = seconds % 60;
        return $"{mins:00}:{secs:00}";
    }

    private void ClearSpawnedEntries()
    {
        foreach (var entry in spawnedEntries)
        {
            if (entry != null)
                Destroy(entry);
        }
        spawnedEntries.Clear();
    }

    void OnDestroy()
    {
        ClearSpawnedEntries();
    }
}
