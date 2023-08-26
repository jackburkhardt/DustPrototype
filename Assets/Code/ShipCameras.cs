using Cinemachine;
using System;
using UnityEngine;

namespace Code
{

    public class ShipCameras : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Camera _nadirCam;
        [SerializeField] private Camera _chaseCam;
        private ShipControls _shipControls;
        
        [SerializeField] float MAX_CHASE_DISTANCE = 20f;
        [SerializeField] float MIN_CHASE_DISTANCE = 5f;

        private void Awake()
        {
            _shipControls = GetComponent<ShipControls>();
        }

        private void LateUpdate()
        {
            if (Input.GetKey(KeyCode.C))
            {
                animator?.SetBool("Nadir", true);
                // _nadirCam.enabled = !_nadirCam.enabled;
                // _chaseCam.enabled = !_chaseCam.enabled;
            }

            else animator?.SetBool("Nadir", false);

            // chase after the ship, varying camera distance depending on speed but keeping upright and above the ship
            if (_chaseCam != null && _chaseCam.enabled)
            {
                Vector3 forwardVec = transform.right;
                
                float speed = Mathf.Abs(_shipControls.velocity);
                Vector3 newPos = transform.position - forwardVec * 15f + Vector3.up * 5f;
                float bias = speed / Mathf.Abs(_shipControls.maxVelocity);
                // adjust camera distance based on bias
                newPos -= forwardVec * Mathf.Lerp(MIN_CHASE_DISTANCE, MAX_CHASE_DISTANCE, bias);
                _chaseCam.transform.position = Vector3.Lerp(_chaseCam.transform.position, newPos, bias);
                _chaseCam.transform.LookAt(transform.position + forwardVec * 30f);
            }
            
        }
    }
}