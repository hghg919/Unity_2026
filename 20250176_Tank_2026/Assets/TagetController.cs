using UnityEngine;
using Tanks.Complete; 

public class TagetController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ParticleSystem explosion;
    public AudioClip explosionSound;

    private bool isHit = false;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (isHit) return;
        if (!coll.collider.CompareTag("Shell")) return;
        
        isHit = true;

        ParticleSystem fx = Instantiate(explosion,transform.position,transform.rotation);
        fx.Play();
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }
        Destroy(gameObject);
        Destroy(fx.gameObject, 2f);
        
        //점수처리
        ShellExplosion shell = coll.collider.GetComponent<ShellExplosion>();
        int shooter = (shell != null) ? shell.playerNum : 0;
        HippoManager manager = Object.FindFirstObjectByType<HippoManager>();

        if (manager != null)
        {
            manager.OnHippoKilled(shooter);
        }



    }
}
