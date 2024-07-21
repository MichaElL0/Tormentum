using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

public class Shooting : MonoBehaviour
{
	[Header("Gun stats")]
    public int minDamage = 19;
    public int maxDamage = 25;
    public int bulletsPerShot = 6;
    public int range = 75;
    public int maxMagSize = 2;
    public int bulletsIn;
    public int bulletsInTotal = 12;
	public float maxEffectiveDistance = 15f;
    public float reloadTime, spread;
	private float timeBtwShoots;
	public float startTimeBtwShoots;
	float timeHolding = 0f;
	public float recoilAmmount = 0.1f;

	public bool shooting, reloading, canShoot, noAmmo;

	private Coroutine reloadCoroutine;

    public Camera fpsCam;
    public Transform shootPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;

	[Header("Sounds")]
	public AudioSource gunSource;
	//public AudioSource stoneImpactSFX;

	[Header("Audio Clips impact")]
	public AudioClip stoneSFX;


	public List<GameObject> bulletHoles;
	GameObject bulletHoleDecal;

	[Header("Particles")]
	public GameObject impactParticleStone;
	public GameObject impactParticleSmoke;
	public GameObject shotParticleSmoke;
	public GameObject muzzleFlash;


	// Start is called before the first frame update
	void Start()
    {
        bulletsIn = maxMagSize;
		canShoot = true;
    }

    // Update is called once per frame
    void Update()
    {

		noAmmo = bulletsIn > 0 ? noAmmo = false : noAmmo = true;

		if (bulletsIn <= 0)
		{
			bulletsIn = 0;
		}

		if (canShoot == true)
        {
			if (timeBtwShoots <= 0)
			{
				if (Input.GetKeyDown(KeyCode.Mouse0) && shooting == false && noAmmo == false && !reloading)
				{
					timeBtwShoots = startTimeBtwShoots;
					Shoot();
				}
			}
			else
			{
				timeBtwShoots -= Time.deltaTime;
			}
		}


		if (Input.GetKey(KeyCode.R) && reloading == false && shooting == false && bulletsIn != maxMagSize && bulletsInTotal != 0)
		{
			print("Reloading started...");
			reloadCoroutine = StartCoroutine(ReloadCoroutine());
			reloading = true;

		}
		else if(!Input.GetKey(KeyCode.R) && reloading)
		{
			StopCoroutine(reloadCoroutine);
			reloading = false;
			timeHolding = 0f; 
			print("Reloading cancaled.");
		}
	}

    void Shoot()
    {
		if(!gunSource.isPlaying)
		{
			gunSource.Play();
		}
			
		gunSource.Play();

		Instantiate(shotParticleSmoke, shootPoint.position, Quaternion.identity);
		Instantiate(muzzleFlash, shootPoint.position, Quaternion.identity);

		
		ImpulseShake.Shake(0.5f); 

        shooting = true;
        bulletsIn--;

		for (int i = 0; i < bulletsPerShot; i++)
		{
			//Spread
			float x = Random.Range(-spread, spread);
			float y = Random.Range(-spread, spread);

			Vector3 direction = fpsCam.transform.forward + new Vector3(x, y, 0);

			if (Physics.Raycast(fpsCam.transform.position, direction, out rayHit, range, whatIsEnemy))
			{
				Debug.DrawRay(fpsCam.transform.position, direction * 30f, Color.green, 15);

				var whatIsHit = rayHit.transform.gameObject.tag;

				print(whatIsHit);
				//Hitting enemy
				if (rayHit.collider.CompareTag("Enemy"))
				{
					float distance = Mathf.Clamp(rayHit.distance, 1, maxEffectiveDistance);
					rayHit.collider.GetComponent<EnemyScript>().TakeDamage(Convert.ToInt32(Math.Round(Random.Range(minDamage, maxDamage + 1) * (distance / Math.Round(rayHit.distance, 1)), 1)));

				}
				else
				{
					switch(whatIsHit)
					{
						case "stone":
							print("Stone surface is hit");
							bulletHoleDecal = bulletHoles[0];
							Instantiate(impactParticleStone, rayHit.point, Quaternion.LookRotation(rayHit.normal));
							AudioSource.PlayClipAtPoint(stoneSFX, rayHit.point);
							break;
						case "wood":
							//VFX
							//bullet hole
							//particle hit
							break;
						default:
							print("Default");
							Instantiate(impactParticleStone, rayHit.point, Quaternion.LookRotation(rayHit.normal));
							//VFX
							break;
					}

					Instantiate(impactParticleSmoke, rayHit.point, Quaternion.LookRotation(rayHit.normal));
					GameObject bulletDecal = Instantiate(bulletHoleDecal, rayHit.point, Quaternion.identity, rayHit.transform);
					bulletDecal.transform.rotation = Quaternion.LookRotation(rayHit.normal);
				}
			}
		}
		shooting = false;
	}

    void Reload()
    {
		if(bulletsIn == 1 && bulletsInTotal > 0)
		{
			bulletsIn++;
			bulletsInTotal--;
		}
		else if(bulletsIn == 0 && bulletsInTotal > 0)
		{
			bulletsIn += 2;
			bulletsInTotal -= 2;
		}
		else if(bulletsIn < 2 && bulletsInTotal == 1)
		{
			bulletsIn++;
			bulletsInTotal--;
		}
		print("Reloaded");
		reloading = false;
	}

	IEnumerator ReloadCoroutine()
	{
		while (true)
		{
			timeHolding += Time.deltaTime;

			if (timeHolding >= reloadTime)
			{
				Reload();
				timeHolding = 0f;
				reloading = false;
				yield break;
			}

			yield return null;
		}
	}

	void CameraRecoil(float valueX)
	{
		fpsCam.transform.rotation = Quaternion.Euler(100, 0, 0);
	}
}
