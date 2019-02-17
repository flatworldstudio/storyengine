using UnityEngine;
using System.Collections.Generic;

namespace StoryEngine.UI
{

    /*!
* \brief 
* Holds a collection of Plane objects.
* 
* Adds a root Plane by default that covers the whole screen. Planes are layered in the order they are added.
* So the last Plane added will be on top.
*/

    public class Layout
    {

        Plane rootPlane;
        List<Plane> AllPlanes;

        /*! \brief Basic constructor creates root Plane and a list of planes.  */

        public Layout()
        {

            rootPlane = new Plane();
            AllPlanes = new List<Plane>
            {
                rootPlane
            };

        }

        /*! \brief Add an interface to the root plane. This is usually for single plane layouts. */       
        public void AddInterface (InterFace _interface)
        {
            rootPlane.AddInterface(_interface);
        }

        public void AddPlane(Plane _plane)
        {
            AllPlanes.Add(_plane);

        }
        public Plane GetRootPlane()
        {
           
            return rootPlane;
        }

        public Plane FindPlaneByPoint(Vector2 _point)
        {

            Plane plane = null;

                int p = AllPlanes.Count - 1;
           
                while (plane == null)
                {
                    Plane check = AllPlanes[p];
                    if (check.WorldCoordinateInPlane(_point))
                    {
                    plane = check;
                    }
                    p--;
                }

            return plane;
        }

    }

}