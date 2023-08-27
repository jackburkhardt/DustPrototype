using Cinemachine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

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

    [SerializeField] private float _thrustForce = 10f;
    [SerializeField] private float _torqueDamper = 0.1f;
    private bool _shiftHeld;
    private bool _ctrlHeld;
    private Vector2 _moveInput;


    [System.Serializable]
    public class NozzleInfo
    {
        public Vector3 direction; // Direction in which this nozzle pushes the shuttle
        public Transform nozzleTransform;
        public VisualEffect nozzleEffect;
        public NozzleMovement MovementType;
    }

    public enum NozzleMovement
    {
        Strafe,
        Torque
    }


    public List<NozzleInfo> nozzles;


    // Start is called before the first frame update
    void Awake()
    {
        this._rb = GetComponent<Rigidbody>();
        this._transform = GetComponent<Transform>();
        DebugDisplay.AddVariable("Velocity", velocity);

        //turn off all VFX
        foreach (var nozzle in nozzles)
        {
            nozzle.nozzleEffect.SetFloat("emissionRate", 0);
        }


    }

    // Update is called once per frame
    void Update()
    {
        // rotate based on pitch, yaw, roll axis (numpad)
        //_rb.AddTorque(-Input.GetAxis("FlightPitch") * _inputDamper, Input.GetAxis("Horizontal") * _inputDamper, Input.GetAxis("FlightRoll") * _inputDamper);
        // add force in the forward direction


       





        // for A and D keys, rotate around the Y axis for turning effect
        //_transform.Rotate(0.0f, 0.0f, Input.GetAxis("Horizontal") * _inputDamper);



        // clamp speed
        _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, maxVelocity);
    }

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
        Debug.Log(_moveInput);
    }

    public void OnModifierA(InputValue value)
    {
        if (value.Get<float>() < 0.5f) { _shiftHeld = false; return; }


        { StopAllCoroutines(); StartCoroutine(OnStrafeStart()); }

        IEnumerator OnStrafeStart()
        {
            foreach (var nozzle in nozzles)
            {
                if (nozzle.MovementType == NozzleMovement.Strafe) nozzle.nozzleEffect.SetFloat("emissionRate", 16);

            };

            yield return new WaitForSeconds(0.2f);

            foreach(var nozzle in nozzles)
            {
                if (nozzle.MovementType == NozzleMovement.Strafe) nozzle.nozzleEffect.SetFloat("emissionRate", 0);
            };

            _shiftHeld = true;
        }
    }

    public void OnModifierB(InputValue value)
    {
        if (value.Get<float>() < 0.5f) { _ctrlHeld = false; return; }

        { StopAllCoroutines(); StartCoroutine(OnTorqueStart()); }

        IEnumerator OnTorqueStart()
        {
            foreach (var nozzle in nozzles)
            {
                if (nozzle.MovementType == NozzleMovement.Torque) nozzle.nozzleEffect.SetFloat("emissionRate", 16);

            };

            yield return new WaitForSeconds(0.2f);

            foreach (var nozzle in nozzles)
            {
                if (nozzle.MovementType == NozzleMovement.Torque) nozzle.nozzleEffect.SetFloat("emissionRate", 0);
            };

            _ctrlHeld = true;
        }

    }

    private void DefaultControl()
    {
        Vector3 desiredMovementDirection = new Vector3(0, -_moveInput.x, _moveInput.y).normalized;

        foreach (var nozzle in nozzles)
        {

            float alignmentToDesiredDirection = Vector3.Dot(nozzle.direction.normalized, -desiredMovementDirection);

            if (alignmentToDesiredDirection > 0.5f) // This nozzle should be active
            {
                nozzle.nozzleEffect.SetFloat("emissionRate", 16);
                _rb.AddRelativeForce(Vector3.Scale(nozzle.direction.normalized, Vector3.forward) * _thrustForce * -alignmentToDesiredDirection);
                _rb.AddTorque(Vector3.Scale(Vector3.up, nozzle.direction.normalized) * _thrustForce * _torqueDamper * alignmentToDesiredDirection);

            }
            else
            {
                nozzle.nozzleEffect.SetFloat("emissionRate", 0);
            }
        }

    }


    private void HandleStrafing()
    {
        Vector3 desiredMovementDirection = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;

        foreach (var nozzle in nozzles)
        {
            if (nozzle.MovementType != NozzleMovement.Strafe) continue;

            float alignmentToDesiredDirection = Vector3.Dot(nozzle.direction.normalized, -desiredMovementDirection);

            if (alignmentToDesiredDirection > 0.5f) // This nozzle should be active
            {
                nozzle.nozzleEffect.SetFloat("emissionRate", 16);
                _rb.AddRelativeForce(nozzle.direction.normalized * _thrustForce * -alignmentToDesiredDirection);
               
            }
            else
            {
                nozzle.nozzleEffect.SetFloat("emissionRate", 0);
            }
        }
    }


    private void HandleTorque()
    {
        Vector3 desiredMovementDirection = new Vector3(_moveInput.y, 0, -_moveInput.x).normalized;

        foreach (var nozzle in nozzles)
        {

            if (nozzle.MovementType != NozzleMovement.Torque) continue;

            float alignmentToDesiredDirection = Vector3.Dot(Vector3.Scale(new Vector3(1,0,1), nozzle.direction.normalized), desiredMovementDirection.normalized);

            if (alignmentToDesiredDirection > 0.5f) // This nozzle should be active
            {
                nozzle.nozzleEffect.SetFloat("emissionRate", 16);
                _rb.AddTorque(Vector3.Scale(desiredMovementDirection.Abs(), nozzle.direction.normalized) * _thrustForce * _torqueDamper * alignmentToDesiredDirection);
            }
            else
            {
                nozzle.nozzleEffect.SetFloat("emissionRate", 0);
            }
        }
    }



    private void FixedUpdate()
    {

        if (_shiftHeld)
        {
            HandleStrafing();
        }
        else if (_ctrlHeld)
        {
            HandleTorque();
        }

        else DefaultControl();

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
