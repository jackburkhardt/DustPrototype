using System;
using UnityEngine;

namespace Code
{
    public class SSBMovement : MonoBehaviour
    {
        [SerializeField] public bool doRotation;
        [SerializeField] public bool doMovement;
        
        [SerializeField] private Vector3 _rotationAxis;
        [SerializeField] private float _rotationSpeedX;
        [SerializeField] private float _rotationSpeedY;
        [SerializeField] private float _rotationSpeedZ;
        
        [SerializeField] private Transform _movementAnchor1;
        [SerializeField] private Transform _movementAnchor2;
        [SerializeField] private float _movementSpeed = 0.3f;
        private  float _moveProgress = 0.0f;


        private void FixedUpdate()
        {
            if (doRotation)
            {
                transform.Rotate(_rotationAxis.x * _rotationSpeedX, _rotationAxis.y * _rotationSpeedY, _rotationAxis.z * _rotationSpeedZ);
            }
            
            if (doMovement)
            {
                _moveProgress += _movementSpeed * Time.deltaTime;
                transform.position = Vector3.Lerp(_movementAnchor1.position, _movementAnchor2.position, _moveProgress % 1.0f);
            }
        }
    }
}