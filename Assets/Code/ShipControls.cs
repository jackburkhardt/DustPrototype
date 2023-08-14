using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipControls : MonoBehaviour
{
    public float velocity = 0f;
    public float maxVelocity = 35f;
    public float accelerationRate = 0.3f;
    public float decelerationRate = 0.18f;
    [SerializeField] private float _inputDamper = 0.3f;
    
    private Rigidbody _rb;
    private Transform _transform;
    int accelerationCounter = 0;

    // Start is called before the first frame update
    void Awake()
    {
        this._rb = GetComponent<Rigidbody>();
        this._transform = GetComponent<Transform>();
        DebugDisplay.AddVariable("Velocity", velocity);
    }

    // Update is called once per frame
    void Update()
    {

        // add force in the forward direction
        _rb.AddForce(_transform.right * velocity);




        // for A and D keys, rotate around the Y axis for turning effect64
        //_transform.Rotate(0.0f, 0.0f, Input.GetAxis("Horizontal") * _inputDamper);

        // rotate based on pitch, yaw, roll axis (numpad)
        _transform.Rotate(Input.GetAxis("FlightRoll") * _inputDamper, Input.GetAxis("FlightPitch") * _inputDamper, Input.GetAxis("Horizontal") * _inputDamper);
        
        // clamp speed
        _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, maxVelocity);


    }

    private void FixedUpdate()
    {
        float inputAxis = Input.GetAxis("Vertical");
        // update the acceleration counter if abssolute value of axis > 0.5
        if (Mathf.Abs(inputAxis) > 0.5f)
        {
            accelerationCounter++;
        }
        else if (Mathf.Abs(inputAxis) <= 0.1f) // account for slightly off axis controllers
        {
            accelerationCounter = -1;
        }
        
        float tempVelocity = velocity;
        // increase velocity every 5 frames, decrease every other frame
        if (accelerationCounter % 5 == 0)
        {
            tempVelocity += inputAxis > 0 ? accelerationRate : -accelerationRate;
            
        } else if (accelerationCounter == -1 && Mathf.Abs(tempVelocity) - decelerationRate >= 0)
        {
            // decrease velocity if not accelerating
            tempVelocity += tempVelocity > 0 ? -decelerationRate : decelerationRate;
        }

        // clamp velocity
        velocity = Mathf.Clamp(tempVelocity, -maxVelocity, maxVelocity);
    }
}
