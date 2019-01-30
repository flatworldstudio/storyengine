using UnityEngine;
using UnityEngine.UI;


namespace StoryEngine.UI
{
    /*!
  * \brief
  * Holds an interactable object, including contraints, dragtarget, callback on click.
  * 
  * Draggable by default, use constraints to limit dragging in different ways.
  * Dragtarget can be different, ie a higher level so user drags a group of objects.
  * Callback is the storyline to be launched on click.
  * Basic brightness and animation for user feedback - can be expanded to be more versatile.
  */
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


        public void AddCallback(string _callBack)
        {

            callback = _callBack;

        }

        public void AddDefaultBlink()
        {
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
}