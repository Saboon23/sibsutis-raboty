using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Camera cam;
    public Transform shellPoint;
    public GameObject shellPrefab;
    public GameObject hitEffect;
    public ParticleSystem muzzleFlash;
    public float range = 120f;
    public float fireRate = 0.18f;

    float nextShot;

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextShot)
        {
            nextShot = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        if (muzzleFlash != null) muzzleFlash.Play();
        EjectShell();

        RaycastHit hit;
        if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range)) return;

        if (hitEffect != null)
        {
            GameObject fx = Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(fx, 2f);
        }

        Target target = hit.transform.GetComponent<Target>();
        if (target == null) target = hit.transform.GetComponentInParent<Target>();
        if (target != null) target.Hit();
    }

    void EjectShell()
    {
        if (shellPrefab == null || shellPoint == null) return;
        GameObject shell = Instantiate(shellPrefab, shellPoint.position, shellPoint.rotation);
        Rigidbody rb = shell.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(shellPoint.right * Random.Range(1.4f, 2.6f) +
                        shellPoint.up * Random.Range(0.8f, 1.8f), ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 1.8f, ForceMode.Impulse);
        }
        Destroy(shell, 4f);
    }
}
