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
    [SerializeField] private float _torqueDamper = 0.5f;
    [SerializeField] private float _maxThrustDamper = 0.5f;
    private bool _shiftHeld;
    private bool _ctrlHeld;
    private float _verticalInput;
    private Vector2 _moveInput;
    [SerializeField] private Animator animator;


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

        _rb.centerOfMass = Vector3.zero;

        //turn off all VFX
        foreach (var nozzle in nozzles)
        {
            nozzle.nozzleEffect.SetFloat("emissionRate", 0);
        }


    }

    bool nadirCam = false;

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.C))
        {
            animator?.SetBool("Nadir", nadirCam = !nadirCam);
        }

        // clamp speed
        _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, maxVelocity);
    }

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    public void OnVertical(InputValue value)
    {
        _verticalInput = value.Get<float>();
        Debug.Log(_verticalInput);
    }

    public void OnModifierA(InputValue value)
    {
        if (value.Get<float>() < 0.5f) { _shiftHeld = false; animator?.SetBool("Strafe", false); return; }

        //lil nozzle flash effect
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
            animator?.SetBool("Strafe", true);
        }
    }

    public void OnModifierB(InputValue value)
    {
        if (value.Get<float>() < 0.5f) { _ctrlHeld = false; animator?.SetBool("Torque", false); return; }

        { StopAllCoroutines(); StartCoroutine(OnTorqueStart()); }

        //lil nozzle flash effect
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
            animator?.SetBool("Torque", true);
        }

    }

    private void DefaultControl()
    {
        Vector3 desiredMovementDirection = new Vector3(0, _verticalInput, _moveInput.y).normalized;
        Vector3 desiredTorqueDirection = new Vector3(0, -_moveInput.x, 0).normalized;

        foreach (var nozzle in nozzles)
        {
            //checks if nozzle is aligned with desired movement direction. values closer to 1 are more aligned
            float alignmentToDesiredMovement = Vector3.Dot(nozzle.direction.normalized, -desiredMovementDirection);
            float alignmentToDesiredTorque = Vector3.Dot(nozzle.direction.normalized, -desiredTorqueDirection);

            if (alignmentToDesiredMovement < 0.5f && alignmentToDesiredTorque < 0.5f) { nozzle.nozzleEffect.SetFloat("emissionRate", 0); continue; }

            nozzle.nozzleEffect.SetFloat("emissionRate", 16);

            if (alignmentToDesiredMovement > 0.5f) _rb.AddRelativeForce(Vector3.Scale(nozzle.direction.normalized, new Vector3(0, 1, 1)) * _thrustForce * -alignmentToDesiredMovement * _maxThrustDamper);

            if (alignmentToDesiredTorque > 0.5f) _rb.AddRelativeTorque(Vector3.Scale(Vector3.up, nozzle.direction.normalized) * _thrustForce * _torqueDamper * alignmentToDesiredTorque);
        }
    }

    private void HandleStrafing()
    {
        Vector3 desiredMovementDirection = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;

        foreach (var nozzle in nozzles)
        {
            
            if (nozzle.MovementType != NozzleMovement.Strafe) continue;

            //checks if nozzle is aligned with desired movement direction. values closer to 1 are more aligned

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

            //checks if nozzle is aligned with desired movement direction. values closer to 1 are more aligned

            float alignmentToDesiredDirection = Vector3.Dot(Vector3.Scale(new Vector3(1,0,1), nozzle.direction.normalized), desiredMovementDirection.normalized);

            if (alignmentToDesiredDirection > 0.5f) // This nozzle should be active
            {
                nozzle.nozzleEffect.SetFloat("emissionRate", 16);
                _rb.AddRelativeTorque(nozzle.direction.normalized * _thrustForce * _torqueDamper * alignmentToDesiredDirection);
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
