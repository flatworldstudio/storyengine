using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;


namespace StoryEngine.UI
{

    public delegate void InitialiseInterface(Interface _interface);
    public delegate void ResizeInterface(Interface _interface);

    public class Interface
    {
        // Describes a collection of interactive objects and how it is interacted with.

        public List<GameObject> selectedObjects;
        public Dictionary<string, Button> uiButtons;
        public Material editMat, defaultMat;

        public string tapNoneCallback = "";

        Mapping mapping;

        public GameObject canvasObject, gameObject;

        public Plane plane;

        public UiCam3D uiCam3D;

      public  Vector2 anchorPosition;
        //public  Vector2 anchor,offset;

        event InitialiseInterface initialise;
        event ResizeInterface resize;

        public Interface(GameObject _canvasObject)
        {
            canvasObject=_canvasObject;

            //if (_initialise!=null)
            //initialise+=_initialise;

            Initvars();

        }

        //public Interface (InitialiseInterface _initialise){
            
        //    initialise+=_initialise;
        //    Initvars();

        //    Initialise();

        //}

        void Initvars(){

            editMat = Resources.Load("materials/white", typeof(Material)) as Material;
            defaultMat = Resources.Load("materials/black", typeof(Material)) as Material;

            uiButtons = new Dictionary<string, Button>();
            selectedObjects = new List<GameObject>();

            anchorPosition = Vector2.zero;

        }



        public void Initialise()
        {
            if (initialise!=null){
                initialise.Invoke(this);
            }

        }

        public void Resize()
        {
            if (resize!=null){
                resize.Invoke(this);
            }

        }

        public void AddInitialiser (InitialiseInterface _initialiser){

            initialise+=_initialiser;

        }

        public void AddResizer (ResizeInterface _resizer){

            resize+=_resizer;

        }

        public void AddMapping(Mapping _mapping)
        {

            mapping = _mapping;

        }

        public void AddUiCam3D(UiCam3D _cam)
        {

            uiCam3D = _cam;

        }

        public Vector2 GetAnchorOffset()
        {

            return anchorPosition;


        }

        public void none(object sender, UIArgs args)
        {

            mapping.none(sender, args);

        }


        public void tap_2d(object sender, UIArgs args)
        {

            mapping.tap_2d(sender, args);

        }

        public void tap_3d(object sender, UIArgs args)
        {

            mapping.tap_3d(sender, args);

        }

        public void tap_none(object sender, UIArgs args)
        {

            mapping.tap_none(sender, args);

        }

        public void single_2d(object sender, UIArgs args)
        {

            mapping.single_2d(sender, args);

        }

        public void single_3d(object sender, UIArgs args)
        {

            mapping.single_3d(sender, args);

        }

        public void single_none(object sender, UIArgs args)
        {

            mapping.single_none(sender, args);

        }

        public void double_2d(object sender, UIArgs args)
        {

            mapping.double_2d(sender, args);

        }

        public void double_3d(object sender, UIArgs args)
        {

            mapping.double_3d(sender, args);

        }

        public void double_none(object sender, UIArgs args)
        {

            mapping.double_none(sender, args);

        }


        public string[] getButtonNames()
        {

            Dictionary<string, Button>.KeyCollection allButtons = uiButtons.Keys;

            string[] allButtonNames = new string[allButtons.Count];

            allButtons.CopyTo(allButtonNames, 0);

            return allButtonNames;
        }



        public void addButton(Button button)
        {

            uiButtons.Remove(button.name);
            uiButtons.Add(button.name, button);

        }

    }


    public class UiCam3D
    {

        public GameObject cameraObject, cameraReference, cameraInterest;
        public Camera camera;
        public Constraint constraint;
        public CAMERACONTROL control;

        public UiCam3D()
        {

        }

        public UiCam3D(GameObject theCameraObject)
        {

            // assumes that reference's parent is interest and that camera is component on reference or child

            if (theCameraObject == null)
                return;

            cameraObject = theCameraObject;

            if (cameraObject.transform.parent != null)
            {

                cameraInterest = theCameraObject.transform.parent.gameObject;

            }

            camera = cameraObject.GetComponentInChildren<Camera>();

            if (camera != null)
            {

                cameraReference = camera.gameObject;

            }

        }

        public void AddContraint(Constraint _constraint)
        {

            constraint = _constraint;

        }



    }


    public class Mapping
    {

        public event UIEventHandler ux_none, ux_tap_2d, ux_tap_3d, ux_tap_none, ux_single_2d, ux_single_3d, ux_single_none, ux_double_2d, ux_double_3d, ux_double_none;


        public Mapping()
        {

        }

        public void none(object sender, UIArgs args)
        {
            if (ux_none != null)
                ux_none(sender, args);

        }


        public void tap_2d(object sender, UIArgs args)
        {
            if (ux_tap_2d != null)
                ux_tap_2d(sender, args);

        }

        public void tap_3d(object sender, UIArgs args)
        {
            if (ux_tap_3d != null)
                ux_tap_3d(sender, args);

        }

        public void tap_none(object sender, UIArgs args)
        {

            if (ux_tap_none != null)
                ux_tap_none(sender, args);

        }

        public void single_2d(object sender, UIArgs args)
        {
            if (ux_single_2d != null)
                ux_single_2d(sender, args);

        }

        public void single_3d(object sender, UIArgs args)
        {
            if (ux_single_3d != null)
                ux_single_3d(sender, args);

        }

        public void single_none(object sender, UIArgs args)
        {
            if (ux_single_none != null)
                ux_single_none(sender, args);

        }

        public void double_2d(object sender, UIArgs args)
        {
            if (ux_double_2d != null)
                ux_double_2d(sender, args);

        }

        public void double_3d(object sender, UIArgs args)
        {
            if (ux_double_3d != null)
                ux_double_3d(sender, args);

        }

        public void double_none(object sender, UIArgs args)
        {
            if (ux_double_none != null)
                ux_double_none(sender, args);

        }



    }





    // ------------------------------------------------------------------------------------------------------------------------------------------
    // turn physical mouse/touch interaction into a ui event




    public delegate void UIEventHandler(object sender, UIArgs args);


    public class UIArgs : EventArgs
    {

        public Event uiEvent;
        public Interface activeInterface;
        public Vector3 delta;
        // also in uievent

        public UIArgs() : base() // extend the constructor 
        {

        }




    }

}