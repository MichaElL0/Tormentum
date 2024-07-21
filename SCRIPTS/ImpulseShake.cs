using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ImpulseShake : MonoBehaviour
{
    static CinemachineImpulseSource impulseSource;

    // Start is called before the first frame update
    void Start()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

	public static void Shake(float velocity)
    {
        impulseSource.GenerateImpulse(velocity);
    }
}
