using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace StoryEngine
{
    public class ScrollLogs : MonoBehaviour
    {

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetPosition(float value)
        {
            transform.localPosition = new Vector3(-0.5f*Screen.width,(-0.5f+ value * 4)* Screen.height, 0);

        }
    }
}