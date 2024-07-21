using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroDisable : MonoBehaviour
{
    public GameObject gObject;
    public AudioSource aSource;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void cutIntroDisable()
	{
        gObject.SetActive(true);
        aSource.Play();
        print("DAD");
	}
}
