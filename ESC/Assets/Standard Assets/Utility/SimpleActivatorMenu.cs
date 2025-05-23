using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityStandardAssets.Utility
{
    public class SimpleActivatorMenu : MonoBehaviour
    {
        // A simple menu which toggles active objects and displays name in UI Text
        public Text camSwitchButton;
        public GameObject[] objects;

        private int m_CurrentActiveObject;

        private void OnEnable()
        {
            m_CurrentActiveObject = 0;
            camSwitchButton.text = objects[m_CurrentActiveObject].name;
        }

        public void NextCamera()
        {
            int nextactiveobject = m_CurrentActiveObject + 1 >= objects.Length ? 0 : m_CurrentActiveObject + 1;

            for (int i = 0; i < objects.Length; i++)
            {
                objects[i].SetActive(i == nextactiveobject);
            }

            m_CurrentActiveObject = nextactiveobject;
            camSwitchButton.text = objects[m_CurrentActiveObject].name;
        }
    }
}
