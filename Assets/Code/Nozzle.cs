using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.VFX;

public class Nozzle : MonoBehaviour
{
    [SerializeField] VisualEffect _vfx;
    bool nozzleIsActive = false;

   public enum NozzleType
    {
        Forward,
        Backward,
        Left,
        Right,
        LeftTurn,
        RightTurn
    }

    public List<NozzleType> nozzleTypes;

    private void Start()
    {
        if (_vfx == null) _vfx = GetComponent<VisualEffect>();
    }

    //play vfx if input.getaxis("vertical") is more than 0.5 and nozzle type is forward

    // Start is called before the first frame update

    string[] axes = {"Vertical", "Horizontal"};
    List<string> axesList = new List<string>();

    private void Update()
    {
        for (int i = 0; i < axes.Length; i++)
        {
            if (Mathf.Abs(Input.GetAxis(axes[i])) > 0.5f)
            {
              
            }
        }

       



    }

    public void ActivateNozzleOnInputAxisMatch(string inputAxis)
    {
        if (Mathf.Abs(Input.GetAxis(inputAxis)) < 0.5f) return;

        Nozzle.NozzleType? nozzleType = null;



        switch (inputAxis)
        {
            case "Vertical":
                nozzleType = (Input.GetAxis(inputAxis) > 0) ? Nozzle.NozzleType.Forward : Nozzle.NozzleType.Backward;
                break;
            case "Horizontal":
                nozzleType = (Input.GetAxis(inputAxis) > 0) ? Nozzle.NozzleType.Right : Nozzle.NozzleType.Left;
                break;
        }
        
    }

    
}
