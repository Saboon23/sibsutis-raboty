using UnityEngine;

public class Target : MonoBehaviour
{
    public GameObject explosion;
    public int health = 1;

    float lifeTime = 6f;
    float size = 1f;
    float bornTime;
    bool dead;

    public void Setup(float newSize, float newLife)
    {
        size = newSize;
        lifeTime = newLife;
        transform.localScale = Vector3.one * newSize;
    }

    void Start()
    {
        bornTime = Time.time;
    }

    void Update()
    {
        if (!dead && Time.time - bornTime > lifeTime) Destroy(gameObject);
    }

    public void Hit()
    {
        if (dead) return;
        health--;
        if (health > 0) return;

        dead = true;
        float left = Mathf.Max(0f, lifeTime - (Time.time - bornTime));
        int points = Mathf.Max(1, Mathf.RoundToInt((2f - size) * 10f + left * 4f));
        if (ScoreManager.instance != null) ScoreManager.instance.Add(points);

        if (explosion != null)
        {
            GameObject fx = Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }
        Destroy(gameObject);
    }
}
