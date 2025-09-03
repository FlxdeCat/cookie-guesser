using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MainRoomSpawner : MonoBehaviour
{
    public static MainRoomSpawner Instance;

    [Header("Spawner Settings")]
    [SerializeField] private GameObject boxPrefab;
    [SerializeField] private Transform[] wallParents;
    [SerializeField] private int rows = 3;
    [SerializeField] private int cols = 18;
    [SerializeField] private Vector2 spacing = new Vector2(1.05f, 1.05f);
    [SerializeField] private Transform boxesParent;

    [Header("Offsets")]
    [SerializeField] private float verticalOffset = -1.25f;
    [SerializeField] private float inwardOffset = -0.2f;

    [Header("Shop Settings")]
    [SerializeField] private int baseUpgradeCost = 5;
    [SerializeField] private float costMultiplier = 1.15f;
    [SerializeField] private int skullsPerUpgrade = 2;
    [SerializeField] private int initialSkullCount = 10;
    [SerializeField] private TextMeshProUGUI upgradeDescription;
    [SerializeField] private TextMeshProUGUI upgradeCostText;

    [Header("Mark Settings")]
    [SerializeField] private int baseMarkCost = 5;
    [SerializeField] private int markCostIncrement = 5;
    [SerializeField] private int markCostIncrementInterval = 5;
    [SerializeField] private int initialMarkAccuracy = 100;
    [SerializeField] private int markAccuracyDecrement = 2;
    [SerializeField] private TextMeshProUGUI markDescription;
    [SerializeField] private TextMeshProUGUI markCostText;

    private bool debugMode = false;

    // tracks each box's current reward type
    private Dictionary<BoxController, BoxRewardType> boxTypes = new Dictionary<BoxController, BoxRewardType>();

    private int upgradeLevel = 0;
    private int markPurchaseCount = 0;
    private float currentMarkAccuracy = -1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SpawnBoxes();
        UpdateShopDescription();
        UpdateMarkUI();
    }

    private void SpawnBoxes()
    {
        int totalBoxes = wallParents.Length * rows * cols;
        int skullCount = Mathf.Clamp(initialSkullCount, 0, totalBoxes);

        // Create positions
        List<(int wall, int row, int col)> positions = new List<(int, int, int)>();
        for (int w = 0; w < wallParents.Length; w++)
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    positions.Add((w, r, c));

        // Shuffle
        for (int i = 0; i < positions.Count; i++)
        {
            int randomIndex = Random.Range(i, positions.Count);
            (positions[i], positions[randomIndex]) = (positions[randomIndex], positions[i]);
        }

        // pick skull positions
        HashSet<(int wall, int row, int col)> skullPositions = new HashSet<(int, int, int)>();
        for (int i = 0; i < skullCount && i < positions.Count; i++)
            skullPositions.Add(positions[i]);

        // spawn boxes
        foreach (var pos in positions)
        {
            Transform wall = wallParents[pos.wall];

            Bounds wallBounds = wall.GetComponent<MeshRenderer>().bounds;
            Vector3 wallCenter = wallBounds.center;

            float totalWidth = (cols - 1) * spacing.x;
            float totalHeight = (rows - 1) * spacing.y;

            Vector3 right = wall.right;
            Vector3 up = wall.up;
            Vector3 forward = wall.forward;

            Vector3 startPos = wallCenter - (right * totalWidth / 2f) + (up * totalHeight / 2f);
            Vector3 localOffset = (right * pos.col * spacing.x) - (up * pos.row * spacing.y);
            Vector3 inward = forward * inwardOffset;
            Vector3 downward = up * verticalOffset;
            Vector3 worldPos = startPos + localOffset + inward + downward;

            GameObject boxGO = Instantiate(boxPrefab, worldPos, wall.rotation, boxesParent);
            BoxController boxController = boxGO.GetComponentInChildren<BoxController>();

            BoxRewardType type = skullPositions.Contains(pos) ? BoxRewardType.Skull : BoxRewardType.Cookie;

            RegisterBox(boxController, type);

            if (debugMode)
                boxController.ToggleDebugCube(type);
        }
    }

    public void RegisterBox(BoxController box, BoxRewardType type)
    {
        if (box == null) return;
        if (!boxTypes.ContainsKey(box)) boxTypes.Add(box, type);
        else boxTypes[box] = type;
    }

    public void UpdateBoxType(BoxController box, BoxRewardType newType)
    {
        if (box == null) return;
        if (boxTypes.ContainsKey(box)) boxTypes[box] = newType;
        else boxTypes[box] = newType;
    }

    public BoxRewardType GetBoxType(BoxController box)
    {
        if (box == null) return BoxRewardType.Cookie;
        if (boxTypes.ContainsKey(box)) return boxTypes[box];
        return BoxRewardType.Cookie;
    }

    public int CurrentUpgradeCost => Mathf.CeilToInt(baseUpgradeCost * Mathf.Pow(costMultiplier, upgradeLevel));

    public void OnUpgradeClicked()
    {
        int costToPay = CurrentUpgradeCost;
        bool paid = GameManager.Instance.TrySpendCookies(costToPay);
        if (!paid)
        {
            GameManager.Instance.FlashCookieText(Color.red, 3, 0.12f, GameManager.Instance.failSfx);
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.successSfx != null)
            GameManager.Instance.PlaySfx(GameManager.Instance.successSfx);

        GameManager.Instance.IncreaseCookieReward();

        AddSkullsByCount(skullsPerUpgrade);

        upgradeLevel++;

        if (upgradeLevel % 3 == 0)
        {
            skullsPerUpgrade++;
        }

        UpdateShopDescription();
    }

    public void AddSkullsByCount(int count)
    {
        if (count <= 0) return;
        if (boxTypes == null || boxTypes.Count == 0) return;

        List<BoxController> cookieBoxes = boxTypes
            .Where(kv => kv.Value == BoxRewardType.Cookie)
            .Select(kv => kv.Key)
            .Where(b => b != null && !b.IsOpened && b.gameObject.activeInHierarchy)
            .ToList();

        int availableCookies = cookieBoxes.Count;
        if (availableCookies == 0) return;

        int toAdd = Mathf.Clamp(count, 0, availableCookies);
        if (toAdd <= 0) return;

        // Fisher–Yates-ish shuffle
        for (int i = 0; i < cookieBoxes.Count; i++)
        {
            int rand = Random.Range(i, cookieBoxes.Count);
            var tmp = cookieBoxes[i];
            cookieBoxes[i] = cookieBoxes[rand];
            cookieBoxes[rand] = tmp;
        }

        for (int i = 0; i < toAdd; i++)
        {
            BoxController b = cookieBoxes[i];
            if (b == null) continue;

            boxTypes[b] = BoxRewardType.Skull;
            UpdateBoxType(b, BoxRewardType.Skull);

            if(debugMode)
                b.ToggleDebugCube(BoxRewardType.Skull);
        }
    }

    public int CurrentMarkCost
    {
        get
        {
            int increments = markPurchaseCount / markCostIncrementInterval;
            return baseMarkCost + (increments * markCostIncrement);
        }
    }

    // Lazy init
    private void EnsureMarkInitialized()
    {
        if (currentMarkAccuracy < 0f)
        {
            currentMarkAccuracy = initialMarkAccuracy;
            markPurchaseCount = Mathf.Max(0, markPurchaseCount);
        }
    }

    private void UpdateMarkUI()
    {
        EnsureMarkInitialized();

        if (markDescription != null)
        {
            markDescription.text = $"Mark Accuracy: {currentMarkAccuracy:0.##}%";
        }

        if (markCostText != null)
        {
            markCostText.text = $"{CurrentMarkCost}";
        }
    }

    private BoxController PickRandomBoxOfType(BoxRewardType desiredType)
    {
        if (boxTypes == null || boxTypes.Count == 0) return null;

        var candidates = boxTypes
            .Where(kv => kv.Value == desiredType)
            .Select(kv => kv.Key)
            .Where(b => b != null && !b.IsOpened && b.gameObject.activeInHierarchy && !b.IsMarked)
            .ToList();

        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }

    public void OnMarkClicked()
    {
        int costToPay = CurrentMarkCost;
        bool paid = GameManager.Instance.TrySpendCookies(costToPay);
        if (!paid)
        {
            GameManager.Instance.FlashCookieText(Color.red, 3, 0.12f, GameManager.Instance.failSfx);
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.successSfx != null)
            GameManager.Instance.PlaySfx(GameManager.Instance.successSfx);

        float roll = Random.Range(0f, 100f);
        bool rollIndicatesCookie = roll < currentMarkAccuracy;

        BoxController target = PickRandomBoxOfType(rollIndicatesCookie ? BoxRewardType.Cookie : BoxRewardType.Skull);

        if (target == null)
        {
            target = PickRandomBoxOfType(rollIndicatesCookie ? BoxRewardType.Skull : BoxRewardType.Cookie);
        }

        if (target == null)
        {
            UpdateMarkUI();
            return;
        }

        target.ApplyMark(currentMarkAccuracy);

        markPurchaseCount++;
        currentMarkAccuracy = Mathf.Max(0f, currentMarkAccuracy - markAccuracyDecrement);

        UpdateMarkUI();
    }

    private void UpdateShopDescription()
    {
        if (upgradeDescription != null)
        {
            int minCookies = 0;
            int maxCookies = 0;
            if (GameManager.Instance != null)
            {
                minCookies = GameManager.Instance.MinCookies;
                maxCookies = GameManager.Instance.MaxCookies;
            }

            int skullCount = boxTypes.Values.Count(v => v == BoxRewardType.Skull);
            upgradeDescription.text = $"Current: {minCookies}-{maxCookies} Cookies with {skullCount} Skulls";
        }

        if (upgradeCostText != null)
        {
            upgradeCostText.text = $"{CurrentUpgradeCost}";
        }
    }

    public int CurrentSkullCount => boxTypes.Values.Count(v => v == BoxRewardType.Skull);
}
