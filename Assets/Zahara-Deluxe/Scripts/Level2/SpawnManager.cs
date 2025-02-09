using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject ElephantPrefab;
    public GameObject PrincesaPrefab;

    public float spawnRangeX = 850;
    public float spawnPosY = 20;
    public float spawnPosY2 = -190;
    public float spawnPosY3 = -420;

    [SerializeField] private BushConfig bushConfig;
    private float[] rowWidths = new float[3];

    public int maxObjects = 15; 
    public float minDistance = 300f;
    public float objectLifetime = 10f;

    private float startDelay = 2;
    private float spawnInterval = 1f;
    private List<float> spawnHeights;
    private List<GameObject> spawnedObjects;

    private const int STARTING_SORT_ORDER = 1000;
    private const int SORT_ORDER_LAYER_DIFFERENCE = 100;
    public static SpawnManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        spawnHeights = new List<float> { spawnPosY, spawnPosY2, spawnPosY3 };
        spawnedObjects = new List<GameObject>();
        InitializeRowWidths();

        InvokeRepeating("SpawnObject", startDelay, spawnInterval);

        StartCoroutine(AdjustSpawnAndLifetime());
    }

    private void InitializeRowWidths()
    {
        if (bushConfig == null)
        {
            Debug.LogError("BushConfig no est� asignado en SpawnManager!");
            return;
        }

        float currentScale = bushConfig.baseScale;
        for (int i = 2; i >= 0; i--) 
        {
            float totalWidth = bushConfig.bushesPerRow * bushConfig.bushSpacing * currentScale;
            rowWidths[i] = totalWidth / 2;
            currentScale *= bushConfig.depthScaleFactor;
        }
    }

    void SpawnObject()
    {
        if (spawnedObjects.Count >= maxObjects)
        {
            spawnedObjects.RemoveAll(obj => obj == null);
            if (spawnedObjects.Count >= maxObjects) return; 
        }

        GameObject prefabToSpawn = ChoosePrefab();
        Vector3 spawnPosition = GetValidSpawnPosition();

        if (spawnPosition == Vector3.zero)
        {
            return;
        }

        Vector3 startPosition = spawnPosition - new Vector3(0, 30, 0);

        GameObject spawnedObject = Instantiate(prefabToSpawn, startPosition, Quaternion.identity);

        SpriteRenderer spriteRenderer = spawnedObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            int rowIndex;
            if (Mathf.Approximately(spawnPosition.y, spawnPosY3))
                rowIndex = 0;
            else if (Mathf.Approximately(spawnPosition.y, spawnPosY2))
                rowIndex = 1;
            else
                rowIndex = 2;

            spriteRenderer.sortingOrder = STARTING_SORT_ORDER - (rowIndex * SORT_ORDER_LAYER_DIFFERENCE);
        }

        spawnedObjects.Add(spawnedObject);
        StartCoroutine(MoveToUpright(spawnedObject.transform, startPosition, spawnPosition));

        StartCoroutine(DestroyAfterDelay(spawnedObject));
    }

    IEnumerator AdjustSpawnAndLifetime()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            if (spawnInterval > 0.5f)
            {
                spawnInterval = Mathf.Max(0.5f, spawnInterval - 0.05f);
                objectLifetime = spawnInterval;
            }
            else
            {
                Debug.Log("SpawnInterval y ObjectLifetime alcanzaron el límite mínimo.");
            }

            CancelInvoke("SpawnObject");
            InvokeRepeating("SpawnObject", spawnInterval, spawnInterval);

            Debug.Log($"SpawnInterval y ObjectLifetime sincronizados: {spawnInterval}");
        }
    }

    IEnumerator DestroyAfterDelay(GameObject obj)
    {
        yield return new WaitForSeconds(objectLifetime);

        if (obj != null)
        {
            spawnedObjects.Remove(obj);
            Destroy(obj);
        }
    }

    GameObject ChoosePrefab()
    {
        float chance = Random.value;
        return chance <= 0.7f ? ElephantPrefab : PrincesaPrefab;
    }

    Vector3 GetValidSpawnPosition()
    {
        int maxAttempts = 30; 

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 candidatePosition = GetRandomSpawnPosition();
            bool positionIsValid = true;

            foreach (GameObject obj in spawnedObjects)
            {
                if (obj != null && Vector3.Distance(obj.transform.position, candidatePosition) < minDistance)
                {
                    positionIsValid = false;
                    break;
                }
            }

            if (positionIsValid) return candidatePosition;
        }

        return Vector3.zero;
    }

    Vector3 GetRandomSpawnPosition()
    {
        int rowIndex = Random.Range(0, spawnHeights.Count);
        float rowSpawnRange = rowWidths[rowIndex];
        
        float randomX = Random.Range(-rowSpawnRange, rowSpawnRange);
        float randomY = spawnHeights[rowIndex];
        
        return new Vector3(randomX, randomY, 0);
    }

    IEnumerator MoveToUpright(Transform objTransform, Vector3 startPosition, Vector3 endPosition)
    {
        float duration = .5f;
        float elapsed = 0;

        if (objTransform == null) yield break;

        objTransform.position = startPosition;

        while (elapsed < duration)
        {
            if (objTransform == null) yield break;

            elapsed += Time.deltaTime;
            objTransform.position = Vector3.Lerp(startPosition, endPosition, elapsed / duration);

            yield return null;
        }

        if (objTransform != null)
        {
            objTransform.position = endPosition;
        }
    }

    public void StopSpawning()
    {
        CancelInvoke("SpawnObject");
    }
}
