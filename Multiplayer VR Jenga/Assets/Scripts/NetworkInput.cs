using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

//This object is added to the controller network avatars when the program is running as a client.
[RequireComponent(typeof(Rigidbody), typeof(FixedJoint))]
public class NetworkInput : NetworkBehaviour {
    
    public SteamVR_TrackedObject trackedObject;
    public NetworkedCameraRig player;
    GameObject model;

	private Rigidbody finRB;
	private Quaternion startRotation;
	private float percent;
    
	private GameObject collidingObject;
	private GameObject objectInHand;

    private void Start()
    {
        // Consider why our models are neither meshRenderers on the network avatars, nor child objects of them.
        // How come we have to use joints? 

        model = transform.parent.Find(this.name + "Model").gameObject;


		startRotation = trackedObject.transform.rotation;

		percent = 0.1f;
    }

	public void OnTriggerEnter(Collider other)
	{
		if (!other.GetComponent<Rigidbody> ()) 
		{
			return;
		}
		collidingObject = other.gameObject;
	}

	public void OnTriggerExit(Collider obj)
	{
		collidingObject = null;
	}

    void Update () 
	{
        if (trackedObject)
        {
            // disable hands if not tracked.
            model.SetActive(trackedObject.isValid);
            if (trackedObject.isValid) 
			{
                // have the net avatars track the steamvr tracked-objects
                transform.position = trackedObject.transform.position;
                transform.rotation = trackedObject.transform.rotation;

                // Send input events to the NetworkedCameraRig instance. (This is our local player)
                var input = SteamVR_Controller.Input((int)trackedObject.index);

                if (input.GetPressDown(Valve.VR.EVRButtonId.k_EButton_ApplicationMenu))
                {
                    // Why does this method have to live in the NetworkedCameraRig class?
					if (player.isServer) 
					{
						player.CmdDestroyJenga ();
						player.CmdResetJenga ();
					}
                }


					if (input.GetPressDown (Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)) {
					
						if (collidingObject) {
							GrabObject ();
						}
					}

					if (input.GetPressUp (Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)) {
						if (objectInHand) {
							ReleaseObject ();
						}
					}
				
            } 
        }
    }

	private void GrabObject()
	{
		objectInHand = collidingObject;
		objectInHand.transform.SetParent (this.transform);
		objectInHand.GetComponent<Rigidbody> ().isKinematic = true;
	}

	private void ReleaseObject()
	{
		objectInHand.GetComponent<Rigidbody> ().isKinematic = false;
		objectInHand.transform.SetParent (null);
	}
}
