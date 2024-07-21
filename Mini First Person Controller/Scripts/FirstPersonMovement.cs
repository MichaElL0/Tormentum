using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed;
	float defaultSpeed;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9;
    public KeyCode runningKey = KeyCode.LeftShift;
	public float stamina;
	private float startStamina;
	private bool isMoving;

	private float _horizontal;

    [Header("Health")]
    public int health = 100;

	public CinemachineVirtualCamera vCamera;
	public CinemachineRecomposer recomposer;
	public NoiseSettings myNoiseProfile;
	public NoiseSettings myDeafultNoiseProfile;

	Rigidbody rigidbody;
	private Vector3 movement;

	public AudioLowPassFilter audioFilterUnder15;
	public AudioReverbZone reverbZoneUnder15;
	

	/// <summary> Functions to override movement speed. Will use the last added override. </summary>
	public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

	void Awake()
    {
        // Get the rigidbody on this.
        rigidbody = GetComponent<Rigidbody>();
		
    }

	private void Start()
	{
		defaultSpeed = speed;
		DeafultValue();
		startStamina = stamina;
	}

	void FixedUpdate()
    {
		if(Input.GetKeyDown(KeyCode.G))
		{
			print("GGGGGGG");
			vCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
		}

        // Update IsRunning from input.
        IsRunning = canRun && Input.GetKey(runningKey);

        // Get targetMovingSpeed.
        float targetMovingSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
        {
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
        }

        // Get targetVelocity from input.
        Vector2 targetVelocity =new Vector2( Input.GetAxis("Horizontal") * targetMovingSpeed, Input.GetAxis("Vertical") * targetMovingSpeed);

        // Apply movement.
        rigidbody.velocity = transform.rotation * new Vector3(targetVelocity.x, rigidbody.velocity.y, targetVelocity.y);


		float horizontal = Input.GetAxis("Horizontal");
		_horizontal = horizontal;
		float vertical = Input.GetAxis("Vertical");
		movement = new Vector3(horizontal, 0.0f, vertical);

		isMoving = (movement.magnitude != 0) ? true : false;

		

	}

	private void Update()
	{

		//Leaning
		if (_horizontal != 0)
		{
			if (_horizontal >= 0.1f)
			{
				//print("Right");
				recomposer.m_Dutch = math.lerp(recomposer.m_Dutch, -2f, 0.1f * _horizontal);
			}
			else if (_horizontal <= -0.1f)
			{
				//print("Right");
				recomposer.m_Dutch = math.lerp(recomposer.m_Dutch, 2f, 0.1f * -_horizontal);
			}
		}
		else if (_horizontal == 0)
		{
			float _velocity = 0.0f;
			recomposer.m_Dutch = Mathf.SmoothDamp(recomposer.m_Dutch, 0f, ref _velocity, 0.025f);

			if(Mathf.Abs(recomposer.m_Dutch) < 0.01f)
			{
				recomposer.m_Dutch = 0f;
			}
		}

		

		//Stamina
		if (stamina == 0)
		{
			canRun = false;
		}

		if(Input.GetKey(KeyCode.LeftShift) && isMoving == true)
		{
			if(stamina <= 0)
			{
				stamina = 0;
				canRun = false;
			}
			else
			{
				stamina -= Time.deltaTime * 1.35f;
			}
		}
		else 
		{
			if(stamina >= startStamina)
			{
				stamina = startStamina;
			}
			else
			{
				stamina += Time.deltaTime * 0.75f;
				if(stamina >= startStamina / 2)
				{
					canRun = true;
				}
			}
		}


		//Effects on different Health
        if(health <= 15)
        {
            Under15HP();
        }
        else if (health <= 50)
        {
            Under50HP();
        }
		else
		{
			DeafultValue();
		}

        if(Input.GetKeyDown(KeyCode.E)) 
        {
            health -= 30;
        }
	}

    void Under15HP()
    {
		print("Under 15");
		UnityEngine.Rendering.VolumeProfile profile = GameObject.Find("Global Volume").GetComponent<UnityEngine.Rendering.Volume>().profile;

		stamina = 0f;
		speed = 4f;

		audioFilterUnder15.enabled = true;
		reverbZoneUnder15.enabled = true;

		UnityEngine.Rendering.Universal.ChromaticAberration myChromaticAberration;
        profile.TryGet(out myChromaticAberration);
		myChromaticAberration.intensity.Override(1);

        UnityEngine.Rendering.Universal.FilmGrain myGrain;
		profile.TryGet(out myGrain);
        myGrain.intensity.Override(1);
		myGrain.type.Override(FilmGrainLookup.Large02);

		UnityEngine.Rendering.Universal.ColorCurves curves;
		profile.TryGet(out curves);
	    curves.active = true;

		vCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 2.5f;
		vCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_NoiseProfile = myNoiseProfile;
		
	}

	void Under50HP()
	{
		print("Under 50");
		UnityEngine.Rendering.VolumeProfile profile = GameObject.Find("Global Volume").GetComponent<UnityEngine.Rendering.Volume>().profile;

		speed = 5f;

		audioFilterUnder15.enabled = false;
		reverbZoneUnder15.enabled = false;

		UnityEngine.Rendering.Universal.ChromaticAberration myChromaticAberration;
		profile.TryGet(out myChromaticAberration);
		myChromaticAberration.intensity.Override(0.6f);
		

		UnityEngine.Rendering.Universal.FilmGrain myGrain;
		profile.TryGet(out myGrain);
		

		myGrain.intensity.Override(1f);
		myGrain.type.Override(FilmGrainLookup.Medium1);

		UnityEngine.Rendering.Universal.ColorCurves curves;
		profile.TryGet(out curves);
		curves.active = false;

		vCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 2f;
		vCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_NoiseProfile = myNoiseProfile;
	}


	void DeafultValue()
	{
		//print("Default");
		speed = defaultSpeed;

		audioFilterUnder15.enabled = false;
		reverbZoneUnder15.enabled = false;

		UnityEngine.Rendering.VolumeProfile profile = GameObject.Find("Global Volume").GetComponent<UnityEngine.Rendering.Volume>().profile;

		UnityEngine.Rendering.Universal.ChromaticAberration myChromaticAberration;
		profile.TryGet(out myChromaticAberration);
		myChromaticAberration.intensity.Override(0.35f);

		UnityEngine.Rendering.Universal.FilmGrain myGrain;
		profile.TryGet(out myGrain);
		myGrain.intensity.Override(0.8f);
		myGrain.type.Override(FilmGrainLookup.Thin2);

		UnityEngine.Rendering.Universal.ColorCurves curves;
		profile.TryGet(out curves);
		curves.active = false;

		vCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 1f;
		vCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 1f;
		vCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_NoiseProfile = myDeafultNoiseProfile;
	}

}