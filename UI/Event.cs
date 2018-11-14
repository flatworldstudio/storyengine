using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;


namespace StoryEngine.UI
{


    public class Event
    {
        // holds user interaction description. note that x and y are NOT continuously updated for touch.

        public float dx, dy, dd, x, y, d, px, py;
        public bool firstFrame;

        public ACTION action;
        public TOUCH touch;

        public DIRECTION direction;

        public GameObject target2D, target3D;

        public Button targetButton;

        public bool isInert, isSpringing;
        public int springIndex;

        public float tapCount;
        public string callback;

        Vector2 __position;

        public Vector2 position {

            get
            {
                __position.x=x;
                __position.y=y;
                return __position;
            }
            set
            {
                Debug.LogWarning("Can't set this value.");

            }


        }

        public Event()
        {
            __position=Vector2.zero;

            target2D = null;
            target3D = null;
            targetButton = null;
            px = 0;
            py = 0;
            firstFrame = true;
            action = ACTION.VOID;
            touch = TOUCH.NONE;
            direction = DIRECTION.FREE;

            isInert = false;
            isSpringing = false;
            springIndex = -1;
            tapCount = 0;

        }

        public Event clone()
        {

            Event result = new Event();
            result.dx = this.dx;
            result.dy = this.dy;
            result.dd = this.dd;
            result.x = this.x;
            result.y = this.y;
            result.d = this.d;
            result.px = this.px;
            result.py = this.py;

            result.firstFrame = this.firstFrame;
            result.action = this.action;
            result.touch = this.touch;
            result.target2D = this.target2D;
            result.target3D = this.target3D;
            result.targetButton = this.targetButton;
            result.isInert = this.isInert;
            result.isSpringing = this.isSpringing;
            result.tapCount = this.tapCount;
            return result;
        }

        public string toString()
        {
            string result = "UI event: ";

            switch (action)
            {
                case ACTION.SINGLEDRAG:
                    result += "single ";
                    break;
                case ACTION.DOUBLEDRAG:
                    result += "double ";
                    break;
                case ACTION.TAP:
                    result += "tap ";
                    break;

                case ACTION.VOID:
                default:
                    result += "void ";
                    break;
            }

            result += "dx: " + dx + " dy: " + dy + " dd: " + dd;
            if (target2D != null)
            {
                result += " 2d: " + target2D.transform.name;
            }

       
            return result;
        }
    }



    public delegate void OnTap(Button theButton);


    public class Button
    {
        public string name;
        public string callback;
        public GameObject gameObject;
        GameObject dragTarget, dragTargetHorizontal, dragTargetVertical;

      public  OnTap onTap;

        Constraint constraint, constraintHorizontal, constraintVertical;

        public bool orthoDragging;

        public Image image;
        public Color color;
        public float brightness, targetBrightness, stepBrightness;

        Vector2 lastPosition, deltaPosition;
        float lastAngle, deltaAngle;

        void Initialise(string _name)
        {
            callback = "";
            name = _name;
            color = new Color(1, 1, 1, 1);
            brightness = 0f;
            targetBrightness = 0.75f;
            stepBrightness = 1f / 0.25f;

            gameObject = GameObject.Find(_name);

            if (gameObject != null)
            {
                image = gameObject.GetComponent<Image>();
                if (image!=null){
                image.color = brightness * color;
                }
            }
            else
            {
                // catch exception
                Log.Error("ERROR: uibutton gameobject not found: " + _name);
            }

            onTap = DefaultBlink;
        }

        void DefaultBlink(Button _button)
        {

            _button.brightness = 1f;
            _button.targetBrightness = 0.75f;
            _button.stepBrightness = 0.25f;

        }

        public void Tap()
        {
            if (onTap != null)
                onTap(this);

        }

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
          

        public void ApplyColour()
        {
            if (brightness < targetBrightness)
            {
                brightness += stepBrightness * Time.deltaTime;
                if (brightness >= targetBrightness)
                {
                    brightness = targetBrightness;
                }
            }
            if (brightness > targetBrightness)
            {
                brightness -= stepBrightness * Time.deltaTime;
                if (brightness <= targetBrightness)
                {
                    brightness = targetBrightness;
                }
            }

            if (image!=null)
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

}