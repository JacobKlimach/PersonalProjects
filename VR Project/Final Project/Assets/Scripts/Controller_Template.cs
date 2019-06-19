using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller_Template : MonoBehaviour 
{

    //Variable declarations
    private Vector3 difference;
    public Transform cameraRig;
    public Transform headCamera;
    public Transform controllerEnd;

    public Color highlightColor;
    Material originalMaterial, tempMaterial;
    Renderer rend = null;

    private LineRenderer laserLine;
    private GameObject collidingObject;
    private GameObject objectInHand;

    public Rigidbody attachPoint;
    SteamVR_TrackedObject trackedObj;
    FixedJoint joint;

    private bool haptic = true;
    private bool highlight = false;

    private Vector2 touchpad;

    public GameObject boolText;
    public GameObject highlightText;
    public GameObject objectTag;

    private SteamVR_Controller.Device Controller 
    {
       get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    
    // Use this for initialization

    void Awake() 
        {
            trackedObj = GetComponent<SteamVR_TrackedObject>();
        }

    void Start() 
        {
            laserLine = GetComponent<LineRenderer>();
			highlightColor = Color.white;


    }

    
    void FixedUpdate() 
    {
        var device = SteamVR_Controller.Input((int)trackedObj.index);


        //If we are colliding with an interactable object
        if (collidingObject && (collidingObject.tag == "examinable" || collidingObject.tag == "multi")) 
        {
            //Conditionals determining which axis to rotate based on where we are pressing on the touchpad
            if (Controller.GetTouch(SteamVR_Controller.ButtonMask.Touchpad) && touchpad.x > 0.5) {
                collidingObject.transform.Rotate(0, 1, 0);
            }
            if (Controller.GetTouch(SteamVR_Controller.ButtonMask.Touchpad) && touchpad.x < -0.5) {
                collidingObject.transform.Rotate(0, -1, 0);
            }
            if (Controller.GetTouch(SteamVR_Controller.ButtonMask.Touchpad) && touchpad.y > 0.5) {
                collidingObject.transform.Rotate(0, 0, 1);
            }
            if (Controller.GetTouch(SteamVR_Controller.ButtonMask.Touchpad) && touchpad.y < -0.5) {
                collidingObject.transform.Rotate(0, 0, -1);
            }
        }

        //If the joint is null and we are pressing down the controller trigger
        if (joint == null && device.GetTouch(SteamVR_Controller.ButtonMask.Trigger)) 
        {
            //If we are colliding with an object and it is interactable
            if (collidingObject && (collidingObject.tag == "interactable" || collidingObject.tag == "multi")) 
            {
                //Attach the object to the controller joint
                joint = collidingObject.AddComponent<FixedJoint>();
                joint.connectedBody = attachPoint;

            }
        }
        
        //If the joint is not null and we are releasing the trigger
        else if (joint != null && device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger)) 
        {
            //Destroy the joint attaching the object to the controller
            var go = joint.gameObject;
            var rigidbody = go.GetComponent<Rigidbody>();
            Object.DestroyImmediate(joint);
            joint = null;

            var origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;

            //Set the released object's velocity and angular velocity to the controller's velocity and angular velocity (throw the object)
            if (origin != null) 
            {
                rigidbody.velocity = origin.TransformVector(device.velocity);
                rigidbody.angularVelocity = origin.TransformVector(device.angularVelocity);
            }
            else 
            {
                rigidbody.velocity = device.velocity;
                rigidbody.angularVelocity = device.angularVelocity;
            }

            rigidbody.maxAngularVelocity = rigidbody.angularVelocity.magnitude;
        }
    }

    // Update is called once per frame
    void Update() 
    {

        Renderer currRend;
        RaycastHit hit;
        touchpad = Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);

        boolText.GetComponent<TextMesh>().text = "Haptic Feedback: " + haptic.ToString();
        highlightText.GetComponent<TextMesh>().text = "Interactables Highlight: " + highlight.ToString();

        if (collidingObject)
            objectTag.GetComponent<TextMesh>().text = "Current Object Tag: " + collidingObject.tag;
        else
            objectTag.GetComponent<TextMesh>().text = "No Object Selected";

        //If the touchpad button is pressed, and we are touching within the center region of the pad (creating a deadzone to separate teleportation from option toggling)
        if ( Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad) && (touchpad.y < 0.5) && (touchpad.y > -0.5) && (touchpad.x < 0.5) && (touchpad.x > -0.5)) 
            {
            //Render the laser line from the controller to 10 units forward from the controller
            laserLine.SetPosition(0, controllerEnd.position);
            laserLine.enabled = true;
            laserLine.SetPosition(1, controllerEnd.forward * 10);

            //If the laser line is intersecting with something
            if (Physics.Raycast(controllerEnd.position, controllerEnd.forward, out hit)) 
            {

                //Grab the renderer of the game object we are colliding over
                currRend = hit.collider.gameObject.GetComponent<Renderer>();

                //Render the line from the controller to the hit location
                laserLine.SetPosition(1, hit.point);


                
            //If we are raycasting over an interactable object, and we have object highlighting on
            if ( (hit.collider.gameObject.tag == "interactable" || collidingObject.tag == "multi") && highlight) 
            {

                    //If our current renderer is null (No object intersected)
                    if (currRend == rend)
                        //Do nothing
                        return;

                    //If we are intersecting with an object
                    if (currRend && currRend != rend) 
                    {
                        if (rend) 
                        {
                            //Grab it's original material to store below
                            rend.sharedMaterial = originalMaterial;
                        }

                    }

                    //If we have an intersected object
                    if (currRend)
                        //Grab it's renderer
                        rend = currRend;
                    //Otherwise, if we aren't, do nothing
                    else
                        return;

                    //Store the original object material
                    originalMaterial = rend.sharedMaterial;

                    //Highlight the interactable object by changing it's material to yellow
                    tempMaterial = new Material(originalMaterial);
                    rend.material = tempMaterial;
                    rend.material.color = highlightColor;

                }
                //Otherwise, if we are not intersecting on a raycast anymore
                else {
                    //Stop highlighting the object and set the material back to the original, and unreference it's renderer
                    if (rend) {
                        rend.sharedMaterial = originalMaterial;
                        rend = null;
                    }
                }
            }
        }

        //If the touch pad was released
		if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad)&& (touchpad.y < 0.5) && (touchpad.y > -0.5) && (touchpad.x < 0.5) && (touchpad.x > -0.5)) 
        {
            //If we hit something with our raycast
            if (Physics.Raycast(controllerEnd.position, controllerEnd.forward, out hit)) 
            {
                //If the line is intersecting with the floor
                if (hit.collider.gameObject.tag == "floor") 
                {
                    //Take the rig center minus the head camera center
                    difference = cameraRig.transform.position - headCamera.transform.position;
                    //set the y component to zero (we don't want to move the player vertically, only horizontally)
                    difference.y = 0;
                    //Move the camera rig to the hit point, while compensating for cameraRig vs playerHead offset
                    cameraRig.transform.position = hit.point + difference;
                }
               
            }
            //Stop rendering the laser
            laserLine.enabled = false;
        }

        
        
        //If the grip is pressed
        if(Controller.GetPress(SteamVR_Controller.ButtonMask.Grip)) 
        {
            //If we press the touchpad in the top half
            if(Controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad) && touchpad.y > 0.5) 
            {
                //Toggle haptic feedback on or off
                if (haptic) {
                    haptic = !haptic;
                }
                else if (!haptic) {
                    haptic = !haptic;
                }
            }
            //If we press the touchpad in the bottom half
            if(Controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad) && touchpad.y < -0.5) 
            {
                //Toggle raycast highlighting on interactable objects
                if (highlight) 
                {
                    highlight = !highlight;
                }
                else if (!highlight) 
                {
                    highlight = !highlight;
                }
            }
            //If we press the touchpad in the right half, toggle "examinable" tag
            if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad) && touchpad.x > 0.5) 
            {
                //If we are colliding with an object
                if (collidingObject) 
                {
                    //Check all possible tag cases and change tag accoridngly
                    if (collidingObject.tag == "interactable") 
                    {
                        collidingObject.tag = "multi";
						collidingObject.GetComponent<Rigidbody>().isKinematic = true;
                    }
                    else if (collidingObject.tag == "multi") {
                        collidingObject.tag = "interactable";
						collidingObject.GetComponent<Rigidbody>().isKinematic = false;
                    }
                    else if (collidingObject.tag == "examinable") {
                        collidingObject.tag = "Untagged";
						collidingObject.GetComponent<Rigidbody>().isKinematic = false;
                    }
                    else 
                    {
                        collidingObject.tag = "examinable";
						collidingObject.GetComponent<Rigidbody>().isKinematic = true;
                    }
                        
                }
                
            }
            //If we press the touchpad in the left half, toggle "interactable" tag
            if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad) && touchpad.x < -0.5) {
                //If we are colliding with an object
                if (collidingObject) 
                {
                    //Check all possible tag cases and change tag accoridngly
                    if (collidingObject.tag == "interactable") 
                    {
                        collidingObject.tag = "Untagged";
                    }
                    else if (collidingObject.tag == "multi") 
                    {
                        collidingObject.tag = "examinable";
                    }
                    else if (collidingObject.tag == "examinable") 
                    {
                        collidingObject.tag = "multi";
                    }
                    else 
                    {
                        collidingObject.tag = "interactable";
                    }
                }

            }
        }
        
    }
    
    //Methods to handle collisions
    public void OnTriggerEnter(Collider other) 
    {
        if (!other.GetComponent<Rigidbody>()) 
        {
            return;
        }
        collidingObject = other.gameObject;

        //If the colliding object is interactable, and haptic feedback is enabled, trigger a haptic pulse to let the user know it is an object of interest
        if( (collidingObject.tag == "interactable" || collidingObject.tag == "multi") && haptic)
        Controller.TriggerHapticPulse(2000);
    }

    public void OnTriggerExit(Collider obj) 
    {
        //If the colliding object was interactable, and haptic feedback is enabled, trigger a haptic pulse to let the user know they are leaving a collision zone

        if ( collidingObject && (collidingObject.tag == "interactable" || collidingObject.tag == "multi")&& haptic) {
            Controller.TriggerHapticPulse(2000);
            collidingObject = null;

        }
        
    }   
}