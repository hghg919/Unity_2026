using UnityEngine;

public class HippoManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject hippoPrefab;
    public Camera mainCamera;

    private GameObject currentHippo;

    void Start()
    {
        SpawnHippo();
    }

    public void SpawnHippo()
    {
        if (currentHippo != null)
        {
            Destroy(currentHippo);
        }

        float spawnHeight = 0;
        float distance = mainCamera.transform.position.y - spawnHeight;

        float viewX = Random.Range(0.15f, 0.85f);
        float viewY = Random.Range(0.15f, 0.85f);

        Vector3 spawnPos = mainCamera.ViewportToWorldPoint(new Vector3(viewX, viewY, distance));
        spawnPos.y = spawnHeight;
        
        currentHippo = Instantiate(hippoPrefab, spawnPos, Quaternion.identity);
    }

    public void OnHippoKilled(int platerNum)
    {
        ScoreManager.instance.AddScore(platerNum);
        SpawnHippo();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
