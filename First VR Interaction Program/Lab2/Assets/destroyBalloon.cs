using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyBalloon : MonoBehaviour {

	// Use this for initialization
	void Start () {  
	}
	
	// Update is called once per frame
	void Update () {

        //If the balloon reaches a height of 20
        if (gameObject.transform.position.y > 15)
        {
            Destroy(gameObject);
        }

    }
}
