using UnityEngine;

public class TargetZone : MonoBehaviour
{
    public GameObject targetPrefab;
    public int count = 4;
    public Vector2 sizeRange = new Vector2(0.5f, 1.6f);
    public Vector2 lifeRange = new Vector2(3f, 9f);
    public Vector3 area = new Vector3(12f, 3.5f, 12f);
    public float cooldown = 6f;

    float nextSpawn;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || Time.time < nextSpawn) return;
        nextSpawn = Time.time + cooldown;
        for (int i = 0; i < count; i++) Spawn();
    }

    void Spawn()
    {
        if (targetPrefab == null) return;
        Vector3 pos = transform.position + new Vector3(
            Random.Range(-area.x, area.x) * 0.5f,
            Random.Range(1f, area.y),
            Random.Range(-area.z, area.z) * 0.5f);

        GameObject obj = Instantiate(targetPrefab, pos, Random.rotation);
        Target target = obj.GetComponent<Target>();
        if (target != null)
            target.Setup(Random.Range(sizeRange.x, sizeRange.y),
                         Random.Range(lifeRange.x, lifeRange.y));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.25f);
        Gizmos.DrawCube(transform.position + Vector3.up * area.y * 0.5f, area);
    }
}
