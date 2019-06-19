using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkedCameraRig : NetworkBehaviour {

    // See ColorSync.cs for more info on SyncVars
    [SyncVar(hook = "OnUserNameChange")]
    public string UserName;
    // Scale of the camerarig. This is preferable to editing the CameraRig prefab.
    public float scaleFactor;
    //Prefab references
    public GameObject CameraRigPrefab;
    // The sphere prefab needs to be registered in the networkManager.
    public GameObject spherePrefab;
	public GameObject jengaPrefab;
    //Instance references.
    GameObject CameraRig;
    Transform NetHead;
    Transform TrackedHead;


    private LineRenderer laserLine;

	public int numOfFloors = 6;
    

    void Start () {

        laserLine = GetComponent<LineRenderer>();
        laserLine.enabled = false;

        NetHead = transform.Find("Head");
        
        // This line is essential:
        if (isLocalPlayer) {

            //From the login-screen.
            UserName = PlayerPrefs.GetString("PLAYER_USERNAME");
            CmdRegisterUserName(UserName);

            CameraRig = Instantiate(CameraRigPrefab);

            //Our starting position is dictated by the networkManager's StartPositions.
            //These can be placed in the scene (Empty objects) and given a NetworkStartPosition Component
            //They will be automatically registered.
            CameraRig.transform.position = transform.position;
            CameraRig.transform.rotation = transform.rotation;
            CameraRig.transform.localScale = Vector3.one * scaleFactor;
            
            // Add the NetworkInput component to the controller avatars. Remember that this only happens on a local player.
            var leftNetInput = transform.Find("LeftController").gameObject.AddComponent<NetworkInput>();
            var rightNetInput = transform.Find("RightController").gameObject.AddComponent<NetworkInput>();

            // Setting References.
            var controllerManager = CameraRig.GetComponent<SteamVR_ControllerManager>();
            leftNetInput.trackedObject = controllerManager.left.GetComponent<SteamVR_TrackedObject>();
            rightNetInput.trackedObject = controllerManager.right.GetComponent<SteamVR_TrackedObject>();
            leftNetInput.player = this;
            rightNetInput.player = this;
            var svrCamera = CameraRig.transform.GetComponentInChildren<SteamVR_Camera>();
            TrackedHead = svrCamera.transform;

            // Small hack to avoid seeing the username text (You could also put it above the player..)
            svrCamera.camera.nearClipPlane = 0.26f;
        }
        else {
            // When we start a client, It recieves the initial state of the syncvars,
            // but it doesn't call the hook methods. We need to do this ourselves.
            OnUserNameChange(UserName);
        }
    }
	/*
	[Command]
	internal void CmdCreateJenga(Vector3 pos){
		//Instantiate on the sever
        GameObject obj = Instantiate(jengaPrefab, pos, Quaternion.Euler(0.0f,0.0f,0.0f));
        //Instantiate on the client
        NetworkServer.Spawn(obj);
	}
    */
    [Command]
    internal void CmdCreateSphere(Vector3 pos, Quaternion rot)
    {
        //Instantiate on the sever
        GameObject obj = Instantiate(spherePrefab, pos, rot);
        //Instantiate on the client
        NetworkServer.Spawn(obj);
    }

	[Command]
	internal void CmdDestroyJenga()
	{
		GameObject[] jengaBlocks;
		jengaBlocks = GameObject.FindGameObjectsWithTag ("jenga");
		foreach (GameObject jenga in jengaBlocks) 
		{
			Destroy (jenga);
			NetworkServer.Destroy (jenga);
		}
	}

	[Command]
	internal void CmdResetJenga()
	{
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


    void Update () {
        // We do the head Avatar tracking here.
        if (isLocalPlayer) {
            NetHead.position = TrackedHead.position;
            NetHead.rotation = TrackedHead.rotation;
        }
    }

    //Syncvar Callback for user's name
    void OnUserNameChange(string newName)
    {
        var model = transform.Find("HeadModel");
        var tm = model.GetComponentInChildren<Text>();
        tm.text = newName;
        UserName = newName;
    }

    // Sends a message from the local player so that the server can then propagate to other clients.
    [Command]
    void CmdRegisterUserName(string username) {
        this.UserName = username;
    }
}
