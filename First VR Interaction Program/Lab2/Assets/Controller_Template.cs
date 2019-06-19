using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller_Template : MonoBehaviour {

    public Rigidbody balloonPrefab;
    public Rigidbody balloonInstance;
    public Transform controllerEnd;
    private float balloonTime = 0;
    private SteamVR_TrackedObject trackedObj;
    private LineRenderer laserLine;
 
    private SteamVR_Controller.Device Controller {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }
    
    // Use this for initialization

    void Awake() {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void Start() {
        laserLine = GetComponent<LineRenderer>();


        foreach (string i in UnityEngine.Input.GetJoystickNames()) {
            print(i);

        }
        
    }


    // Update is called once per frame
    void Update () {

        RaycastHit hit;

        // If left Grip Button is pressed, Spawn ballon
        if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip)) {
            Debug.Log(gameObject.name + " Grip Press");
    
        }
        if (Controller.GetPress(SteamVR_Controller.ButtonMask.Grip)) {
            Debug.Log(gameObject.name + " Grip Press");
    
        }
        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Grip)) {
            Debug.Log(gameObject.name + " Grip Release");

        }




        if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad)) 
        {
            Debug.Log(gameObject.name + "Pad Press");

            balloonInstance = Instantiate(balloonPrefab, controllerEnd.position, controllerEnd.rotation);

            balloonTime = 0;

            balloonInstance.isKinematic = true;
            balloonInstance.useGravity = false;
            
        }

		if (Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad)) 
		{

            balloonTime += Time.deltaTime;

            if (balloonTime<2)
            {
				//Snap the balloon's position to the controller position
				balloonInstance.transform.position = controllerEnd.position;
				balloonInstance.transform.rotation = controllerEnd.rotation;

				//If the current scale is less than the desired maximum scale (by default set to 0.1). It is sufficient to check any of the 3 scale parameters as we are scaling all 3 equally.
				if (balloonInstance.transform.localScale.x < 1)
                {
					//Bind the local scale to be the value of the temp scale variable
					balloonInstance.transform.localScale *= 1.025f;
				}
                //If the balloon has reached the desired scale size, let it go
                else
                {
                    balloonInstance.isKinematic = false;
                    balloonInstance.useGravity = true;

                    
                }

            }
			//Once it has been 2 seconds, let the balloon go
			else
            {
				balloonInstance.isKinematic = false;
				balloonInstance.useGravity = true;

				//No longer bind the balloon to the controller
				transform.parent = null;
			}

            

        }

        //If the button was released before 2 seconds is up, perform the same (detach and allow the balloon to float)
        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            balloonInstance.isKinematic = false;
            balloonInstance.useGravity = true;

            //No longer bind the balloon to the controller
            transform.parent = null;
        }


            if (Controller.GetHairTriggerDown())
        {
            
        }
        if (Controller.GetHairTrigger())
        {

            laserLine.SetPosition(0, controllerEnd.position);

            Debug.Log(gameObject.name + "Press Hold");
            laserLine.enabled = true;
            laserLine.SetPosition(1, controllerEnd.forward*10);



            if (Physics.Raycast(controllerEnd.position, controllerEnd.forward, out hit))
            {
                laserLine.SetPosition(1, hit.point);
                Debug.Log("hit");

                if (hit.collider.gameObject.tag == "balloon")
                {
                    Destroy(hit.collider.gameObject);
                }
                
            }
           
        }
        if (Controller.GetHairTriggerUp()) {
            Debug.Log(gameObject.name + " Trigger Release");
            laserLine.enabled = false;
        }

        


    }
}
