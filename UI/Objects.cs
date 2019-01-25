using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;


namespace StoryEngine.UI
{

    public delegate void InitialiseInterface(InterFace _interface);
    public delegate void ResizeInterface(InterFace _interface);

    public class InterFace
    {
        // Describes a collection of interactive objects and how it is interacted with.

        public string name;
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

        public InterFace(GameObject _canvasObject,string _name ="Unnamed")
        {
            canvasObject=_canvasObject;
            name=_name;

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



    public delegate void OnTap(Button theButton);


    public class Button
    {
        public string name;
        public string callback;
        public GameObject gameObject;
        GameObject dragTarget, dragTargetHorizontal, dragTargetVertical;

        public OnTap onTap;

        Constraint constraint, constraintHorizontal, constraintVertical;

        public bool orthoDragging;

        public Image image;
        public Color color;
        public float brightness, targetBrightness, stepBrightness;

        Vector2 lastPosition, deltaPosition;
        float lastAngle, deltaAngle;
              

        public Button()
        {
            lastPosition = Vector2.zero;
        }

        public Button(string _name)
        {
            Initialise(_name);
            constraint = new Constraint();
            dragTarget = gameObject;
            lastPosition = Vector2.zero;
            orthoDragging = false;
        }

        public Button(string _name, GameObject _dragTarget)
        {
            Initialise(_name);
            dragTarget = _dragTarget;
            constraint = new Constraint();
            lastPosition = Vector2.zero;
            orthoDragging = false;
        }

        public Button(string _name, GameObject _dragTarget, Constraint _constraint)
        {
            Initialise(_name);
            dragTarget = _dragTarget;
            constraint = _constraint;
            lastPosition = Vector2.zero;
            orthoDragging = false;
        }

        public Button(string _name, GameObject _dragTargetHorizontal, Constraint _constraintHorizontal, GameObject _dragTargetVertical, Constraint _constraintVertical)
        {
            Initialise(_name);

            // Button with differentiated constraints for horizontal and vertical dragging. Dragging will snap to initial direction.

            constraint = new Constraint();
            dragTarget = gameObject;

            dragTargetHorizontal = _dragTargetHorizontal;
            constraintHorizontal = _constraintHorizontal;

            dragTargetVertical = _dragTargetVertical;
            constraintVertical = _constraintVertical;

            orthoDragging = true;

            lastPosition = Vector2.zero;
        }

        void Initialise(string _name)
        {
            callback = "";
            name = _name;
            color = new Color(1, 1, 1, 1);
            brightness = 0.75f;
            targetBrightness = 0.75f;
            stepBrightness = 1f / 0.25f;

            gameObject = GameObject.Find(_name);

            if (gameObject != null)
            {
                //image = gameObject.GetComponent<Image>();
                //if (image != null)
                //{
                //    image.color = brightness * color;
                //}
            }
            else
            {
                // catch exception
                Log.Error("ERROR: uibutton gameobject not found: " + _name);
            }

         //   onTap = DefaultBlink;
        }


        public void AddCallback(string _callBack){
            
            callback=_callBack;

        }

        public void AddDefaultBlink(){
            onTap = DefaultBlink;
        }



        void DefaultBlink(Button _button)
        {
            //Debug.Log("blink");
            _button.brightness = 1f;
            _button.targetBrightness = 0.75f;
            _button.stepBrightness = 0.25f;

            Controller.instance.AddAnimatingButton(this); // list the button for animation

        }

        public void Tap()
        {
            //Debug.Log("tapped");
            if (onTap != null)
                onTap(this);

        }


        public GameObject GetDragTarget(DIRECTION dir = DIRECTION.FREE)
        {
            // Retrieve drag target object based on drag direction

            switch (dir)
            {
                case DIRECTION.HORIZONTAL:
                    return dragTargetHorizontal;

                case DIRECTION.VERTICAL:
                    return dragTargetVertical;

                case DIRECTION.FREE:
                default:
                    return dragTarget;

            }
        }

        public bool HasDragTarget(GameObject target)
        {

            // Check if this button targets a given gameobject as its dragtargets.

            if (!orthoDragging && dragTarget == target)
                return true;

            if (orthoDragging && (dragTargetHorizontal == target || dragTargetVertical == target))
                return true;

            return false;

        }


        public Constraint GetConstraint(DIRECTION dir = DIRECTION.FREE)
        {
            // Retrieve  constraint  based on drag direction

            switch (dir)
            {
                case DIRECTION.HORIZONTAL:
                    return constraintHorizontal;

                case DIRECTION.VERTICAL:
                    return constraintVertical;

                case DIRECTION.FREE:
                default:
                    return constraint;

            }
        }


        public float GetDeltaAngle()
        {

            Vector3 anchor = dragTarget.GetComponent<RectTransform>().anchoredPosition;
            Vector2 relativePosition = new Vector2(anchor.x, anchor.y) - constraint.anchor;

            float angle = Mathf.Atan2(relativePosition.y, relativePosition.x);

            deltaAngle = lastAngle - angle;
            lastAngle = angle;

            return deltaAngle;
        }


        public Vector2 GetDeltaPosition()
        {

            deltaPosition.x = gameObject.transform.position.x - lastPosition.x;
            deltaPosition.y = gameObject.transform.position.y - lastPosition.y;

            lastPosition.x = gameObject.transform.position.x;
            lastPosition.y = gameObject.transform.position.y;

            return deltaPosition;
        }


        public void ApplyBrightness()
        {

            if (brightness == targetBrightness)
            {
                Controller.instance.RemoveAnimatingButton(this);
                return;
            }

            if (brightness < targetBrightness)
            {
                brightness += stepBrightness * Time.deltaTime;

                if (brightness >= targetBrightness)
                {
                    brightness = targetBrightness;
                    Controller.instance.RemoveAnimatingButton(this);
                }
            }

            if (brightness > targetBrightness)
            {
                brightness -= stepBrightness * Time.deltaTime;
                if (brightness <= targetBrightness)
                {
                    brightness = targetBrightness;
                    Controller.instance.RemoveAnimatingButton(this);
                }
            }

            if (image != null)
                image.color = brightness * color;
        }
    }


    public class Constraint
    {
        public Vector3 hardClampMin, hardClampMax;
        public bool hardClamp;

        public Vector3 edgeSpringMin, edgeSpringMax;
        public bool edgeSprings;

        public Vector2[] springPositions;
        public bool springs;

        public Vector2 anchor;
        public float radiusClampMin, radiusClampMax;
        public bool radiusClamp;

        public bool pitchClamp;
        public float pitchClampMin, pitchClampMax;
        static Constraint __empty;


        public Constraint()
        {
            hardClamp = false;
            edgeSprings = false;
            springs = false;
            radiusClamp = false;
            pitchClamp = false;
        }

        public void AddPitchClamp(float _min, float _max)
        {

            pitchClampMin = _min;
            pitchClampMax = _max;
            pitchClamp = true;

        }

        public void AddHardClamp(Vector3 _min, Vector3 _max)
        {

            hardClampMin = _min;
            hardClampMax = _max;
            hardClamp = true;

        }

        static public Constraint empty
        {

            get
            {
                if (__empty == null)
                    __empty = new Constraint();

                return __empty;
            }
            set
            {

            }


        }




    }




    // ------------------------------------------------------------------------------------------------------------------------------------------
    // turn physical mouse/touch interaction into a ui event




    public delegate void UIEventHandler(object sender, UIArgs args);


    public class UIArgs : EventArgs
    {

        public Event uiEvent;
        //public InterFace activeInterface;
        public Vector3 delta;
        // also in uievent

        public UIArgs() : base() // extend the constructor 
        {

        }




    }

}