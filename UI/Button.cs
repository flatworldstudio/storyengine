using UnityEngine;
using UnityEngine.UI;


namespace StoryEngine.UI
{
    public delegate void OnTap();

    /*!
  * \brief
  * Holds an interactable object.
  * 
  * See sample scenes for examples of use. 
  * Basic brightness and animation for user feedback - can be expanded to be more versatile.
  */


    public class Button
    {
        string ID = "Button";
        //public int UID;
        public string name;
        public string callback, callbackDoubleTap;
        public GameObject gameObject;
        GameObject dragTarget, dragTargetHorizontal, dragTargetVertical;

        public OnTap onTap;

        Constraint constraint, constraintHorizontal, constraintVertical;

        public bool orthoDragging;

        public InterFace InterFace;

        public Image image;
        Color color;
        float brightness, targetBrightness, stepBrightness;

        Vector2 lastPosition, deltaPosition;
        float lastAngle, deltaAngle;

        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.
        void Log(string message) => StoryEngine.Log.Message(message, ID);
        void Warning(string message) => StoryEngine.Log.Warning(message, ID);
        void Error(string message) => StoryEngine.Log.Error(message, ID);
        void Verbose(string message) => StoryEngine.Log.Message(message, ID, LOGLEVEL.VERBOSE);

        //public Button()
        //{
        //    lastPosition = Vector2.zero;
        //}

     
            // single draggable button

        public Button(string _name)
        {
            Initialise(_name);
            constraint = Constraint.none;
            dragTarget = gameObject;
            lastPosition = Vector2.zero;
            orthoDragging = false;
        }

        public Button(GameObject obj)
        {
            Initialise(obj);
            constraint = Constraint.none;
            dragTarget = gameObject;
            lastPosition = Vector2.zero;
            orthoDragging = false;
        }

        // single lockable button

        public Button(string _name, bool locked)
        {
            Initialise(_name);

            if (locked)
            {
                constraint = null;
                dragTarget = null;
            }
            else
            {
                constraint = Constraint.none;
                dragTarget = gameObject;
            }

            lastPosition = Vector2.zero;
            orthoDragging = false;
        }

        public Button(GameObject _object, bool locked)
        {

            Initialise(_object);

            if (locked)
            {
                constraint = null;
                dragTarget = null;
            }
            else
            {
                constraint = Constraint.none;
                dragTarget = gameObject;
            }

            lastPosition = Vector2.zero;
            orthoDragging = false;
        
    }

        // button with dragtarget (so dragging drags the target not the button)

        public Button(string _name, GameObject _dragTarget)
        {
            Initialise(_name);
            dragTarget = _dragTarget;
            constraint = Constraint.none;
            lastPosition = Vector2.zero;
            orthoDragging = false;
        }

        // button with dragtarget (so dragging drags the target not the button) and drag constraint
        
        public Button(string _name, GameObject _dragTarget, Constraint _constraint)
        {
            Initialise(_name);
            dragTarget = _dragTarget;
            constraint = _constraint;
            lastPosition = Vector2.zero;
            orthoDragging = false;
        }

        public Button(GameObject obj , GameObject _dragTarget, Constraint _constraint)
        {
            Initialise(obj);
            dragTarget = _dragTarget;
            constraint = _constraint;
            lastPosition = Vector2.zero;
            orthoDragging = false;
        }


        public Button(string _name, GameObject _dragTargetHorizontal, Constraint _constraintHorizontal, GameObject _dragTargetVertical, Constraint _constraintVertical)
        {
            Initialise(_name);

            // Button with differentiated constraints for horizontal and vertical dragging. Dragging will snap to initial direction.

            constraint = Constraint.none;
            //dragTarget = gameObject;

            dragTargetHorizontal = _dragTargetHorizontal;
            constraintHorizontal = _constraintHorizontal;

            dragTargetVertical = _dragTargetVertical;
            constraintVertical = _constraintVertical;

            orthoDragging = true;

            lastPosition = Vector2.zero;
        }
        public Button(GameObject _go, GameObject _dragTargetHorizontal, Constraint _constraintHorizontal, GameObject _dragTargetVertical, Constraint _constraintVertical)
        {
            Initialise(_go);

            // Button with differentiated constraints for horizontal and vertical dragging. Dragging will snap to initial direction.

            constraint = Constraint.none;
            //dragTarget = gameObject;

            dragTargetHorizontal = _dragTargetHorizontal;
            constraintHorizontal = _constraintHorizontal;

            dragTargetVertical = _dragTargetVertical;
            constraintVertical = _constraintVertical;

            orthoDragging = true;

            lastPosition = Vector2.zero;
        }

        void Initialise(GameObject _go)
        {
            gameObject = _go;
            callback = "";
            callbackDoubleTap = "";
            name = gameObject.name;
            //UID = gameObject.GetInstanceID();

            //    color = new Color(1, 1, 1, 1);
            brightness = 1f;
            targetBrightness = 1f;
            stepBrightness = 1f / 0.25f;

            

                image = gameObject.GetComponent<Image>();
                if (image != null)
                {
                    color = image.color;
                }
                //   color = new Color(1, 1, 1, 1);
                //   ApplyBrightness();

                //if (image != null)
                //{
                //    image.color = brightness * color;
                //}
          
        }

        void Initialise(string _name)
        {

            Log("initialising buttons by object name is discouraged");

            gameObject = GameObject.Find(_name);

            if (gameObject != null)
                Initialise(gameObject);
            else
                Warning("Gameobject not found.");
           
            //image = gameObject.GetComponent<Image>();


           

            //   onTap = DefaultBlink;
        }

        public void SetColor(Color _color)
        {
            color = _color;
        }


        public void AddCallback(string _callBack)
        {

            callback = _callBack;

        }

        public void AddCallbackDoubleTap(string _callBack)
        {

            callbackDoubleTap = _callBack;

        }


        public void AddConstraint(Constraint _constraint)
        {
            constraint = _constraint;
        }

        public void AddOrthoConstraints(GameObject _dragTargetHorizontal, Constraint _constraintHorizontal, GameObject _dragTargetVertical, Constraint _constraintVertical)
        {
            constraint = Constraint.none;

            dragTargetHorizontal = _dragTargetHorizontal;
            constraintHorizontal = _constraintHorizontal;

            dragTargetVertical = _dragTargetVertical;
            constraintVertical = _constraintVertical;

            orthoDragging = true;

        }



        public void AddDefaultBlink()
        {
            SetBrightness(0.75f);
            onTap = DefaultBlink;
        }

        public void SetTargetBrightness(float _value)
        {
            targetBrightness = _value;
            Controller.instance.AddAnimatingButton(this);
        }

        public void SetTargetBrightness(float _value, float _step)
        {
            targetBrightness = _value;
            stepBrightness = _step;

            Controller.instance.AddAnimatingButton(this);
        }

        public void SetBrightness(float _value)
        {
            brightness = _value;
            targetBrightness = _value;

            //Controller.instance.AddAnimatingButton(this);

            ApplyBrightness();
        }

        public bool BrightnessChanging()
        {
            return !Mathf.Approximately(brightness, targetBrightness);
        }

        public void DefaultBlink()
        {

            brightness = 1f;
            targetBrightness = 0.75f;
            stepBrightness = 0.25f;

            Controller.instance.AddAnimatingButton(this); // list the button for animation

        }


	


		public void Tap()
        {
            //Debug.Log("tapped");

            onTap?.Invoke();

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



            if (brightness < targetBrightness)
            {
                brightness += stepBrightness * Time.deltaTime;

                if (brightness >= targetBrightness)
                {
                    brightness = targetBrightness;
                    //Controller.instance.RemoveAnimatingButton(this);
                }
            }

            if (brightness > targetBrightness)
            {
                brightness -= stepBrightness * Time.deltaTime;
                if (brightness <= targetBrightness)
                {
                    brightness = targetBrightness;
                    //Controller.instance.RemoveAnimatingButton(this);
                }
            }

            if (Mathf.Approximately(brightness, targetBrightness))
            {
                Controller.instance.RemoveAnimatingButton(this);

            }

            if (image != null)
                image.color = brightness * color;
            //else
            //Log("no image");
        }
    }
}