using UnityEngine;
using System;

namespace StoryEngine.UI
{
    /*!
* \brief
* Holds ui info to pass onto handlers.
* 
*/
    public class UIArgs : EventArgs
    {

        public Event uiEvent;
        public Vector3 delta;

        public UIArgs() : base() // extend the constructor 
        {

        }

    }
}