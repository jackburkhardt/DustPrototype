using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

public class Sattelite : MonoBehaviour
{
    [SerializeField] VisualEffect vfx;
    [SerializeField] Transform rocket;
    [SerializeField] Rigidbody _rigidbody;
    [SerializeField] Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        vfx.SetFloat("emissionRate", 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            vfx.SetFloat("emissionRate", 16);
            
        }

        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            animator?.SetBool("Shoot", false);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            animator?.SetBool("Shoot", true);
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            Time.timeScale = 0.1f;
        }

        if (Input.GetKeyUp(KeyCode.V))
        {
            Time.timeScale = 1f;
        }   
  

        if (Input.GetKeyUp(KeyCode.P))
        {
            vfx.SetFloat("emissionRate", 0);
        }   
    }

    private void FixedUpdate()
    {

        if (Input.GetKey(KeyCode.P))
        {
            if (_rigidbody.angularVelocity.x > 0) _rigidbody.AddRelativeTorque(-0.2f, 0, 0);
               // _rigidbody.angularVelocity.Set(_rigidbody.angularVelocity.x - 0.1f * Time.deltaTime, _rigidbody.angularVelocity.y, _rigidbody.angularVelocity.z); 
        }

        
    }
}
