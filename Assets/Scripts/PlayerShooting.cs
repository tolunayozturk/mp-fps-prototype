﻿using UnityEngine;
using UnityEngine.Networking;

public class PlayerShooting : NetworkBehaviour 
{
	[SerializeField] float shotCooldown = .3f;
	[SerializeField] Transform firePosition;
	[SerializeField] ShotEffectsManager shotEffects;

	[SyncVar (hook = "OnScoreChanged")] int score;

	float ellapsedTime;
	bool canShoot;

	void Start()
	{
		if (isLocalPlayer)
			canShoot = true;
	}

	[ServerCallback]
	void OnEnable()
	{
		score = 0;
	}

	void Update()
	{
		if (!canShoot)
			return;

		ellapsedTime += Time.deltaTime;

		if (Input.GetButtonDown ("Fire1") && ellapsedTime > shotCooldown) 
		{
			ellapsedTime = 0f;
			CmdFireShot (firePosition.position, firePosition.forward);
		}

	}

	[Command]
	void CmdFireShot(Vector3 origin, Vector3 direction)
	{
		RaycastHit hit;

		Ray ray = new Ray (origin, direction);
		Debug.DrawRay (ray.origin, ray.direction * 3f, Color.red, 1f);

		bool result = Physics.Raycast (ray, out hit, 50f);

		if (result) 
		{
			PlayerHealth enemy = hit.transform.GetComponent<PlayerHealth> ();

			if (enemy != null) 
			{
				bool wasKillShot = enemy.TakeDamage ();
				if (wasKillShot) {
					score++;
				}
			}
		}

		RpcProcessShotEffects (result, hit.point);
	}

	[ClientRpc]
	void RpcProcessShotEffects(bool playImpact, Vector3 point)
	{
		shotEffects.Initialize ();
		shotEffects.PlayShotEffects ();

		if (playImpact) {
			shotEffects.PlayImpactEffect (point);
			shotEffects.DestroyBulletImpact ();
		}
	}

	void OnScoreChanged(int value)
	{
		score = value;
		if (isLocalPlayer) {
			PlayerCanvas.canvas.SetKills (value);
		}
	}
}