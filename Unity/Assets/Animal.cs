using UnityEngine;

public class Animal
{
    public string name;
    public string sound;

    public void PlaySound()
    {
        Debug.Log(name + " : " + sound);
    }
}
