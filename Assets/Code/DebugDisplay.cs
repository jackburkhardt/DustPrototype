using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    public class DebugDisplay : MonoBehaviour
    {
        private static Dictionary<string, object> _trackedVariables = new Dictionary<string, object>();

        public static void AddVariable(string name, object variable)
        {
            _trackedVariables.Add(name, variable);
        }

        private void OnGUI()
        {
            foreach (var variable in _trackedVariables)
            {
                GUILayout.Label($"{variable.Key}: {variable.Value}");
            }
        }
    }