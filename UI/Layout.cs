using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;


namespace StoryEngine.UI
{



    public delegate void InitialiseLayout(Plane root); // called on init to create the layout
    public delegate void ResizeLayout(Plane root); // called when layout is resized

    /*!
* \brief 
* Holds a (binairy tree) layout of Plane objects.
* 
* A layout can be recursively split into multiple Plane objects that can each hold a different Interface.
* If no size is defined it holds a single unlimited rootPlane which can hold a single Interface. 

*/

    public class Layout
    {

        InitialiseLayout initialiseLayout;
        ResizeLayout resizeLayout;

        Plane rootPlane;

        public Layout()
        {

            rootPlane = new Plane();

        }

        public Layout(float _width, float _height, InitialiseLayout _initialiseLayout = null, ResizeLayout _resizeLayout = null)

        {

            initialiseLayout = _initialiseLayout;
            resizeLayout = _resizeLayout;

            Initialise(_width, _height);

        }

        void Initialise(float _width, float _height)
        {

            rootPlane = new Plane(0, _width, 0, _height);

            if (initialiseLayout != null)
                initialiseLayout(rootPlane);

        }
        public Plane GetRootPlane()
        {
            return rootPlane;
        }

        public Plane GetPlaneByAddress(string _planeAddress)
        {
            return (rootPlane.SearchAddress(_planeAddress));
        }

        public Plane FindPlaneByPoint(Vector2 _point)
        {

            Plane plane = RecursivePointSearch(rootPlane, _point);


            return plane;
        }

        Plane RecursivePointSearch(Plane _plane, Vector2 _point)
        {

            Plane returnFromLevel = null;

            if (_plane.children != null)
            {
                Plane returnFromLevelDown;

                returnFromLevelDown = RecursivePointSearch(_plane.children[0], _point);

                if (returnFromLevelDown != null)
                {
                    returnFromLevel = returnFromLevelDown;
                    return returnFromLevel;
                }

                returnFromLevelDown = RecursivePointSearch(_plane.children[1], _point);

                if (returnFromLevelDown != null)
                {
                    returnFromLevel = returnFromLevelDown;
                    return returnFromLevel;
                }

            }

            else
            {

                if (_plane.PointInPlane(_point))
                {
                    returnFromLevel = _plane;
                    return returnFromLevel;
                }

            }

            return returnFromLevel;

        }

        public void Resize(float _width, float _height)
        {

            rootPlane.x1 = _width;
            rootPlane.y1 = _height;

            if (resizeLayout != null)
                resizeLayout(rootPlane);


            ResizePlaneRecursive(rootPlane);


        }

        public void ResizePlaneRecursive(Plane _plane)
        {

            if (_plane.children != null)
            {

                ResizePlaneRecursive(_plane.children[0]);
                ResizePlaneRecursive(_plane.children[1]);

            }
            else
            {
                // We call resize on the plane. Plane will call resize on interface if present.
                _plane.Resize();

            }

        }

        /*
        public void Resize(float _width, float _height)
        {

            rootPlane.x1 = _width;
            rootPlane.y1 = _height;

            UpdateLayout();

        }


        public void UpdateLayout()
        {

            if (rootPlane != null)
            {

                if (populatePlanes != null)
                    populatePlanes(rootPlane);

                DrawPlanes(rootPlane);

            }

        }

        void DrawPlanes(Plane plane)
        {

            if (plane.children != null)
            {

                DrawPlanes(plane.children[0]);
                DrawPlanes(plane.children[1]);

            }
            else
            {

                DrawPlane(plane);
            }

        }


        void DrawPlane(Plane _plane)
        {
                   

            // if there's a prefab, we use that as template for gameobject. 

            // 

            if (planePrefab != null)
            {


                GameObject gameObject=null;

                //if (!gameObjectMap.TryGetValue(_plane, out gameObject))

                if (_plane.gameObject == null)
                {
                    gameObject = GameObject.Instantiate(planePrefab);
                    gameObject.transform.SetParent(canvas.gameObject.transform, false);
                    //gameObjectMap.Add(_plane, gameObject);
                    _plane.gameObject = gameObject;

                }

                float m = margin;

                Vector2 anchorPosition = new Vector2(_plane.x0 + m, _plane.y0 + m);
                gameObject.GetComponent<RectTransform>().anchoredPosition = anchorPosition;
                Vector2 sizeDelta = new Vector2(_plane.x1 - _plane.x0 - 2 * m, _plane.y1 - _plane.y0 - 2 * m);
                gameObject.GetComponent<RectTransform>().sizeDelta = sizeDelta;


            }

            // Call external delegate to handle any specifics.

            if (_plane.gameObject!=null && onUpdatePlane != null)
                onUpdatePlane(_plane, _plane.gameObject);


        }
*/


    }




  


}