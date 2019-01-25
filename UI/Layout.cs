using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;


namespace StoryEngine.UI
{

    // A layout contains planes.
    // A plane can contain an interface.
    // An interface holds references to objects in the scene.

   public delegate void InitialiseLayout(Plane root); // called on init to create the layout
    public delegate void ResizeLayout(Plane root); // called when layout is resized

    public class Layout
    {

        // Class to manage a (binairy tree) layout of Planes. 

        InitialiseLayout initialiseLayout;
        ResizeLayout resizeLayout;

        Plane rootPlane;

        //public Layout(float _width, float _height, InitialiseLayout _initialiseLayout)
        //{

        //    initialiseLayout = _initialiseLayout;
        //    Initialise(_width, _height);

        //}

        public Layout(float _width, float _height,InitialiseLayout _initialiseLayout=null,ResizeLayout _resizeLayout=null)

        {


            initialiseLayout = _initialiseLayout;
            resizeLayout=_resizeLayout;

            Initialise(_width, _height);

        }

        void Initialise(float _width, float _height)
        {

            rootPlane = new Plane(0, _width, 0, _height);

            if (initialiseLayout != null)
                initialiseLayout(rootPlane);

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

        public void ResizePlaneRecursive(Plane _plane){

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




    //   public delegate void InitialisePlane (Plane _plane);
    //   public delegate void ResizePlane(Plane _plane);

    public class Plane
    {

        // Class to describe a single plane in a (binairy tree) layout.


        public float x0, y0, x1, y1;
        public float splitValue;
        public DIRECTION splitDirection;

        public Plane parent, root;
        public Plane[] children;
        public string address;

        //public Canvas canvas;
        public InterFace interFace;
        //public GameObject gameObject;
        //     public Vector2 screenOffset = Vector2.zero;


        public Plane()
        {
        }

        public Plane(Plane _parent)
        {
            parent = _parent;
            address = parent.address + "x";
            root = parent.root;
        }

        public Plane(float _x0, float _x1, float _y0, float _y1)
        {
            // Root only.
            x0 = _x0;
            x1 = _x1;
            y0 = _y0;
            y1 = _y1;
            address = "r";
            root = this;

        }

        public void Resize(){

            if (interFace!=null)
                interFace.Resize();

        }

        public Plane SearchAddress(string _address)
        {

            Plane returnPlane = null;

            if (children != null)
            {

                returnPlane = children[0].SearchAddress(_address);
                if (returnPlane != null)
                    return returnPlane;

                returnPlane = children[1].SearchAddress(_address);

                if (returnPlane != null)
                    return returnPlane;


            }
            else
            {

                if (address == _address)
                    returnPlane = this;

            }

            return returnPlane;


        }

        // Plane RecursiveSearch (Plane _plane, string _address){

        //    Plane 

        //}

        public void AddInterface(InterFace _interface)
        {

            interFace = _interface;
            interFace.plane = this;

        }

        void AddChildren()
        {

            if (children == null)
            {
                children = new Plane[2];
                children[0] = new Plane(this);
                children[0].address = children[0].address.Replace('x', '0');

                children[1] = new Plane(this);
                children[1].address = children[1].address.Replace('x', '1');

            }
        }

        public bool PointInPlane(Vector2 _point)
        {

            if (_point.x >= x0 && _point.x < x1 && _point.y >= y0 && _point.y < y1)
                return true;
            else
                return false;

        }

        public bool Split(float _value, DIRECTION _direction)
        {


            if (children != null && splitDirection != _direction)
            {

                // Already split, but in different direction. This shouldn't have been called.

                Debug.LogError("UI Plane already split but in different direction.");

                return false;
            }


            switch (_direction)
            {

                case DIRECTION.VERTICAL:

                    if (_value > x0 && _value < x1)
                    {

                        AddChildren();

                        splitValue = _value;
                        splitDirection = _direction;

                        children[0].x0 = x0;
                        children[0].x1 = _value;
                        children[0].y0 = y0;
                        children[0].y1 = y1;

                        children[1].x0 = _value;
                        children[1].x1 = x1;
                        children[1].y0 = y0;
                        children[1].y1 = y1;

                        return true;
                    }

                    break;

                case DIRECTION.HORIZONTAL:

                    AddChildren();

                    splitValue = _value;
                    splitDirection = _direction;

                    children[0].x0 = x0;
                    children[0].x1 = x1;
                    children[0].y0 = y0;
                    children[0].y1 = _value;

                    children[1].x0 = x0;
                    children[1].x1 = x1;
                    children[1].y0 = _value;
                    children[1].y1 = y1;

                    break;

                default:
                    break;

            }

            return false;

        }




    }
}