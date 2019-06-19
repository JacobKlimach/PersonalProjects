using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class JengaSpawner : NetworkBehaviour {

	public GameObject jengaPrefab;
	public int numOfFloors = 6;

	public override void OnStartServer() {

		GameObject jenga1, jenga2, jenga3;
		Vector3 spawnPosition = new Vector3 (0.0f, 2.5f, 0.5f);
		Quaternion spawnRotation1 = Quaternion.Euler (0.0f, 0.0f, 0.0f);
		Quaternion spawnRotation2 = Quaternion.Euler (0.0f, 90.0f, 0.0f);
		float height = 2.2f;

		for (int i = 0; i < numOfFloors; i++) {
			Quaternion spawnRotation;
			Vector3 spawnPos1, spawnPos2, spawnPos3;
			if (i % 2 == 0) {
				spawnRotation = spawnRotation1;
				spawnPos1 = new Vector3 (-0.37f, height, 0.4f);
				spawnPos2 = new Vector3 (0.0f, height, 0.4f);
				spawnPos3 = new Vector3 (0.37f, height, 0.4f);
			} else {
				spawnRotation = spawnRotation2;
				spawnPos1 = new Vector3 (0.0f, height, 0.03f);
				spawnPos2 = new Vector3 (0.0f, height, 0.4f);
				spawnPos3 = new Vector3 (0.0f, height, 0.77f);
			}
			jenga1 = (GameObject)Instantiate (jengaPrefab, spawnPos1, spawnRotation);
			jenga2 = (GameObject)Instantiate (jengaPrefab, spawnPos2, spawnRotation);
			jenga3 = (GameObject)Instantiate (jengaPrefab, spawnPos3, spawnRotation);
			NetworkServer.Spawn (jenga1);
			NetworkServer.Spawn (jenga2);
			NetworkServer.Spawn (jenga3);
			height += 0.4f;
		}

	}

}
