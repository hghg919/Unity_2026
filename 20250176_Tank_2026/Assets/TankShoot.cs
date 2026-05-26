using Tanks.Complete;
using UnityEngine;

public class TankShoot : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Rigidbody prefabShell;
    public Transform fireTransform;
    public float launchForce = 20f;

    public int playerNum = 1;
    string fireName;

    void Start()
    {
        fireName = "Fire" + playerNum;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown(fireName))
        {
             Rigidbody shell = Instantiate(prefabShell, fireTransform.position, fireTransform.rotation);
            shell.linearVelocity = fireTransform.forward * launchForce;

            ShellExplosion shellScript = shell.GetComponent<ShellExplosion>();

            if(shellScript != null )
            {
                shellScript.playerNum = playerNum;
            }
        }
    }
}
