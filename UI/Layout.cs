using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;


namespace StoryEngine.UI
{

    public delegate void PopulatePlanes(Plane root);
    public delegate void OnUpdatePlane(Plane branch, GameObject gameObject);

    public class Layout
    {

        // Class to hold, update, visualise a (binairy tree) layout of planes. 

        GameObject planePrefab;

        public Canvas canvas;
        int margin;
        public PopulatePlanes populatePlanes;
        public OnUpdatePlane onUpdatePlane;

        Plane uiRoot;

        //Dictionary<Plane, GameObject> gameObjectMap;
        //Dictionary<string, Interface> interfaceMap;
     //   Dictionary<string, Plane> planeAddresses;

        public Layout(float _width, float _height, Canvas _canvas, GameObject _planePrefab, int _margin)
        {
            margin = _margin;
            planePrefab = _planePrefab;
            canvas = _canvas;

            if (_canvas == null || _planePrefab == null)
            {
                Log.Warning("Canvas or plane reference is null, cannot make layout.");
                return;
            }

            List<Transform> tempList = canvas.gameObject.transform.Cast<Transform>().ToList();

            foreach (Transform child in tempList)
            {
                GameObject.Destroy(child.gameObject);
            }

            //gameObjectMap = new Dictionary<Plane, GameObject>();
            //interfaceMap = new Dictionary<string, Interface>();
        //    planeAddresses= new  Dictionary<string, Plane>();


            uiRoot = new Plane(0, _width, 0, _height);

         //   planeAddresses.Add(uiRoot.address,uiRoot);

        }

        public void AddInterfaceByAddress(Interface _interface, string _planeAddress)
        {
            Plane plane;

            plane= uiRoot.SearchAddress(_planeAddress);

         //   planeAddresses.TryGetValue(_planeAddress,out plane);

            if (plane!=null)
                plane.interFace=_interface;

            //if (interfaceMap.ContainsKey(_planeAddress))
            //    interfaceMap.Remove(_planeAddress);

            //interfaceMap.Add(_planeAddress, _interface);

        }

        public Interface GetInterfaceByAddress(string _planeAddress)
        {

           // Interface returnInterface;

            Plane plane;
            plane= uiRoot.SearchAddress(_planeAddress);
            //planeAddresses.TryGetValue(_planeAddress,out plane);

            if (plane!=null)
                return plane.interFace;
            

            //interfaceMap.TryGetValue(_planeAddress, out returnInterface);

            return null;

        }

        //public GameObject GetGameObjectByPlane(Plane _plane)
        //{

        //    GameObject gameObject = null;
        //    gameObjectMap.TryGetValue(_plane, out gameObject);
        //    return gameObject;

        //}

       

        public Plane FindPlaneByPoint(Vector2 _point)
        {

            Plane plane = RecursivePointSearch(uiRoot, _point);


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


        public void UpdateLayout(float _width, float _height)
        {

            uiRoot.x1 = _width;
            uiRoot.y1 = _height;

            UpdateLayout();

        }


        public void UpdateLayout()
        {

            if (uiRoot != null)
            {

                if (populatePlanes != null)
                    populatePlanes(uiRoot);

                DrawPlanes(uiRoot);

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


            GameObject gameObject=null;

            //if (!gameObjectMap.TryGetValue(_plane, out gameObject))
            if (_plane.gameObject==null)
            {
                gameObject = GameObject.Instantiate(planePrefab);
                gameObject.transform.SetParent(canvas.gameObject.transform, false);
                //gameObjectMap.Add(_plane, gameObject);
                _plane.gameObject=gameObject;

            }

            float m = margin;

            Vector2 anchorPosition = new Vector2(_plane.x0 + m, _plane.y0 + m);
            gameObject.GetComponent<RectTransform>().anchoredPosition = anchorPosition;
            Vector2 sizeDelta = new Vector2(_plane.x1 - _plane.x0 - 2 * m, _plane.y1 - _plane.y0 - 2 * m);
            gameObject.GetComponent<RectTransform>().sizeDelta = sizeDelta;

            if (onUpdatePlane != null)
                onUpdatePlane(_plane, gameObject);


        }



    }


    public class Plane
    {

        // Class to describe a single plane in a (binairy tree) layout.


        public float x0, y0, x1, y1;
        public float splitValue;
        public DIRECTION splitDirection;

        public Plane parent;
        public Plane[] children;
        public string address;

        public GameObject gameObject;
        public Interface interFace;
        public Vector2 screenOffset=Vector2.zero;

        public Plane()
        {
        }

        public Plane(Plane _parent)
        {
            parent = _parent;
            address = parent.address + "x";
        }

        public Plane(float _x0, float _x1, float _y0, float _y1)
        {
            // Root only.
            x0 = _x0;
            x1 = _x1;
            y0 = _y0;
            y1 = _y1;
            address = "r";

        }

        public Plane SearchAddress(string _address){

            Plane returnPlane=null;

            if (children!=null){

                returnPlane=children[0].SearchAddress(_address);
                if (returnPlane!=null)
                    return returnPlane;
                
                returnPlane=children[1].SearchAddress(_address);

                if (returnPlane!=null)
                    return returnPlane;
                

            }else{

                if (address==_address)
                    returnPlane=this;

            }

            return returnPlane;


        }

        // Plane RecursiveSearch (Plane _plane, string _address){

        //    Plane 

        //}


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