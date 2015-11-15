using UnityEngine;
using System.Collections.Generic;
using Leap;

public class Player : MonoBehaviour
{
    readonly float MAX_VERTICAL_ANGLE = 85f; // max angle when rotating around the X axis

    Starling starlingComponent;

    Controller controller;

	// Use this for initialization
	void Start()
	{
        starlingComponent = GetComponent<Starling>();
        controller = null;//new Controller();
	}
	
	// Update is called once per frame
	void Update()
	{	
        if( controller != null && controller.IsConnected )
        {
            var frame = controller.Frame();
            var hand = frame.Hands[0];
            
            float pitch = hand.Direction.Pitch;
            float yaw = hand.Direction.Yaw;
            float roll = hand.PalmNormal.Roll;
            
            var handDirection = hand.Direction;

            if( !hand.IsValid )
                transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);

            // rotation around Y axis
            transform.RotateAround(transform.position, Vector3.up, hand.PalmPosition.x * 1f * Time.deltaTime);

            // rotation around X axis
            transform.eulerAngles = new Vector3(hand.PalmNormal.z * 90f, transform.eulerAngles.y, transform.eulerAngles.z);

            // keep angle within range
            if( Util.NormalizeAngle(transform.eulerAngles.x) < -MAX_VERTICAL_ANGLE )
                transform.eulerAngles = new Vector3(-MAX_VERTICAL_ANGLE, transform.eulerAngles.y, transform.eulerAngles.z);
            if( Util.NormalizeAngle(transform.eulerAngles.x) > MAX_VERTICAL_ANGLE )
                transform.eulerAngles = new Vector3(MAX_VERTICAL_ANGLE, transform.eulerAngles.y, transform.eulerAngles.z);

        }
        else
        {
            if(Input.GetKey(KeyCode.A))
                transform.RotateAround(transform.position, Vector3.up, -90f * Time.deltaTime);
            else if(Input.GetKey(KeyCode.D))
                transform.RotateAround(transform.position, Vector3.up, 90f * Time.deltaTime);

            if( Input.GetKey(KeyCode.W) )
            {
                if(Util.NormalizeAngle(transform.eulerAngles.x) > -MAX_VERTICAL_ANGLE)
                    transform.Rotate(new Vector3(-180f * Time.deltaTime, 0f, 0f));

                if(Util.NormalizeAngle(transform.eulerAngles.x) < -MAX_VERTICAL_ANGLE)
                    transform.eulerAngles = new Vector3(-MAX_VERTICAL_ANGLE, transform.eulerAngles.y, transform.eulerAngles.z);
            }
            else if(Input.GetKey(KeyCode.S))
            {
                if(Util.NormalizeAngle(transform.eulerAngles.x) < MAX_VERTICAL_ANGLE)
                    transform.Rotate(new Vector3(180f * Time.deltaTime, 0f, 0f));

                if(Util.NormalizeAngle(transform.eulerAngles.x) > MAX_VERTICAL_ANGLE)
                    transform.eulerAngles = new Vector3(MAX_VERTICAL_ANGLE, transform.eulerAngles.y, transform.eulerAngles.z);
            }
        }
	}

	void FixedUpdate()
	{
        GetComponent<Rigidbody>().velocity = transform.forward * starlingComponent.maxSpeed;
	}

    void OnCollisionEnter(Collision other)
    {
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }
    
    void OnCollisionStay(Collision other)
    {
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }
    
    void OnCollisionExit(Collision other)
    {
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }
}
