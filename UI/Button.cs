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
        public string name;
        public string callback;
        public GameObject gameObject;

        Button dragTargetButton, dragTargetButtonHor, dragTargetButtonVer;

        public OnTap onTap;

        Constraint constraint;

        public bool orthoDragging;

        public InterFace InterFace;

        public Image image;
        Color color;
        float brightness, targetBrightness, stepBrightness;

        Vector2 lastPosition, deltaPosition;
        float lastAngle, deltaAngle;

        Vector2 speed=Vector2.zero;
        Vector2 currentacc = Vector2.zero;
        float eVel0 = 0;
        float eVel1 = 0;
        float eVel2 = 0;
        float eVel3 = 0;

        float vx = 0;
        float vy = 0;

        int SpringIndex = -1;

        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.
        void Log(string message) => StoryEngine.Log.Message(message, ID);
        void Warning(string message) => StoryEngine.Log.Warning(message, ID);
        void Error(string message) => StoryEngine.Log.Error(message, ID);
        void Verbose(string message) => StoryEngine.Log.Message(message, ID, LOGLEVEL.VERBOSE);
          

        public void AddDragTarget (Button _target)
        {
            dragTargetButton = _target;
            orthoDragging = false;
        }

        public void AddDragTargets(Button _hor,Button _ver)
        {
            dragTargetButtonHor = _hor;
            dragTargetButtonVer = _ver;
            orthoDragging = true;
        }

        public void BeginInertia(Vector2 _speed,DIRECTION _direction)
        {
            // inertia applies to what we were dragging

            switch (_direction)
            {
               
                case DIRECTION.HORIZONTAL:
                    _speed.y = 0;
                    if (dragTargetButtonHor != null) dragTargetButtonHor.SetSpeed(_speed);
                    if (dragTargetButtonVer != null) dragTargetButtonVer.SetSpeed(Vector2.zero);

                    break;

                case DIRECTION.VERTICAL:
                    _speed.x = 0;
                    if (dragTargetButtonHor != null) dragTargetButtonHor.SetSpeed(Vector2.zero);
                    if (dragTargetButtonVer != null) dragTargetButtonVer.SetSpeed(_speed);

                    break;

                case DIRECTION.FREE:
                default:

                    if (dragTargetButton != null) dragTargetButton.SetSpeed(_speed);
                   
                    break;


            }
        


        }

        public void SetSpeed (Vector2 _speed)
        {
            currentacc = Vector2.zero;
            speed = _speed;
             eVel0 = 0;
             eVel1 = 0;
             eVel2 = 0;
             eVel3 = 0;
            vx = 0;
            vy = 0;
            SpringIndex = -1;
        }

        public void Move()
        {
           
            speed = Vector2.SmoothDamp(speed, Vector2.zero, ref currentacc, 0.25f);

            // Passing in both speed and acceleration for bouncing. So speed gets inversed, so does current acc.

            if (orthoDragging)
            {
                if (speed.x < float.Epsilon)
                {
                    Methods.Translate2D(this, ref speed,ref currentacc, constraint);
                }

                if (speed.y < float.Epsilon)
                {
                   Methods.Translate2D(this, ref speed,ref currentacc, constraint);
                }
                
            }
            else
            {
                Methods.Translate2D(this, ref speed, ref currentacc, constraint);
            }

            // Spring
            if (constraint == null) return;
            
            Vector3 anchor = gameObject.GetComponent<RectTransform>().anchoredPosition;

            if (constraint.edgeSprings)
            {
                //Log("spring");

                if (anchor.x < constraint.edgeSpringMin.x)
                {
                    anchor.x = Mathf.SmoothDamp(anchor.x, constraint.edgeSpringMin.x, ref eVel0, 0.25f);
                }
                else if (anchor.x > constraint.edgeSpringMax.x)
                {
                    anchor.x = Mathf.SmoothDamp(anchor.x, constraint.edgeSpringMax.x, ref eVel1, 0.25f);
                }

                if (anchor.y < constraint.edgeSpringMin.y)
                {
                   
                    anchor.y = Mathf.SmoothDamp(anchor.y, constraint.edgeSpringMin.y, ref eVel2, 0.25f);
                }

                else if (anchor.y > constraint.edgeSpringMax.y)
                {
                    anchor.y = Mathf.SmoothDamp(anchor.y, constraint.edgeSpringMax.y, ref eVel3, 0.25f);
                }
               
            }

            if (constraint.springs)
            {
                              
                int closestSpring = -1;
                float closest = 9999999999f;
                

                if (SpringIndex >= 0)
                {
                    // there is a target index we're moving to
                    closestSpring = SpringIndex;

                }
                else
                {

                    for (int i = 0; i < constraint.springPositions.Length; i++)
                    {

                        // find nearest spring

                        Vector3 theSpringPosition = constraint.springPositions[i];
                        float distance = Vector2.Distance(anchor, theSpringPosition);
                        if (distance < closest)
                        {
                            closest = distance;
                            closestSpring = i;
                        }
                    }

                }
                
                anchor.x = Mathf.SmoothDamp(anchor.x, constraint.springPositions[closestSpring].x, ref vx, 0.25f);
                anchor.y = Mathf.SmoothDamp(anchor.y, constraint.springPositions[closestSpring].y, ref vy, 0.25f);
                            
            }

            gameObject.GetComponent<RectTransform>().anchoredPosition = anchor;
        }

        public Button (GameObject _object)
        {
            InitialiseByObject(_object);
            
        }

        public Button(string _name)
        {
            Warning("broken");
        }

        public Button(string _name, GameObject _dragTarget)
        {
            Warning("broken");
        }
        //public Button(string _name)
        //{
        //    Initialise(_name);
        //    constraint = Constraint.none;
        //    dragTarget = gameObject;
        //    lastPosition = Vector2.zero;
        //    orthoDragging = false;
        //}

        //public Button(string _name, bool locked)
        //{
        //    Initialise(_name);

        //    if (locked)
        //    {
        //        constraint = null;
        //        dragTarget = null;
        //    }
        //    else
        //    {
        //        constraint = Constraint.none;
        //        dragTarget = gameObject;
        //    }

        //    lastPosition = Vector2.zero;
        //    orthoDragging = false;
        //}


        //public Button(string _name, GameObject _dragTarget)
        //{
        //    Initialise(_name);
        //    dragTarget = _dragTarget;
        //    constraint = Constraint.none;
        //    lastPosition = Vector2.zero;
        //    orthoDragging = false;
        //}

        //public Button(string _name, GameObject _dragTarget, Constraint _constraint)
        //{
        //    Initialise(_name);
        //    dragTarget = _dragTarget;
        //    constraint = _constraint;
        //    lastPosition = Vector2.zero;
        //    orthoDragging = false;
        //}

        //public Button(string _name, GameObject _dragTargetHorizontal, Constraint _constraintHorizontal, GameObject _dragTargetVertical, Constraint _constraintVertical)
        //{
        //    Initialise(_name);

        //    // Button with differentiated constraints for horizontal and vertical dragging. Dragging will snap to initial direction.

        //    constraint = Constraint.none;
        //    dragTarget = gameObject;

        //    dragTargetHorizontal = _dragTargetHorizontal;
        //    constraintHorizontal = _constraintHorizontal;

        //    dragTargetVertical = _dragTargetVertical;
        //    constraintVertical = _constraintVertical;

        //    orthoDragging = true;

        //    lastPosition = Vector2.zero;
        //}

        //void Initialise(string _name)
        //{
        //    callback = "";
        //    name = _name;
        //    color = new Color(1, 1, 1, 1);
        //    brightness = 1f;
        //    targetBrightness = 1f;
        //    stepBrightness = 1f / 0.25f;

        //    gameObject = GameObject.Find(_name);


        //    //image = gameObject.GetComponent<Image>();


        //    if (gameObject != null)
        //    {

        //        image = gameObject.GetComponent<Image>();
        //        //   ApplyBrightness();

        //        //if (image != null)
        //        //{
        //        //    image.color = brightness * color;
        //        //}
        //    }
        //    else
        //    {
        //        // catch exception
        //        Error("Gameobject not found: " + _name);
        //    }

        //    //   onTap = DefaultBlink;
        //}

        void InitialiseByObject(GameObject _object)
        {
            callback = "";
            gameObject = _object;
            name = gameObject.name;
            color = new Color(1, 1, 1, 1);
            brightness = 1f;
            targetBrightness = 1f;
            stepBrightness = 1f / 0.25f;

            image = gameObject.GetComponent<Image>();

            Log("created button " + name);
   
        }

        public void AddCallback(string _callBack)
        {

            callback = _callBack;

        }

        public void AddConstraint(Constraint _constraint)
        {
            constraint = _constraint;
        }

        public void AddOrthoConstraints(GameObject _dragTargetHorizontal, Constraint _constraintHorizontal, GameObject _dragTargetVertical, Constraint _constraintVertical)
        {
            Warning("broken");
        }
            //public void AddOrthoConstraints(GameObject _dragTargetHorizontal, Constraint _constraintHorizontal, GameObject _dragTargetVertical, Constraint _constraintVertical)
            //{
            //    constraint = Constraint.none;

            //    dragTargetHorizontal = _dragTargetHorizontal;
            //    constraintHorizontal = _constraintHorizontal;

            //    dragTargetVertical = _dragTargetVertical;
            //    constraintVertical = _constraintVertical;

            //    orthoDragging = true;

            //}



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


        public Button GetDragTarget(DIRECTION dir )
        {
            // Retrieve drag target object based on drag direction

            switch (dir)
            {
                case DIRECTION.HORIZONTAL:
                    return dragTargetButtonHor;

                case DIRECTION.VERTICAL:
                    return dragTargetButtonVer;

                case DIRECTION.FREE:
                default:
                    return dragTargetButton;

            }
        }

        //public bool HasDragTarget(GameObject target)
        //{

        //    // Check if this button targets a given gameobject as its dragtargets.

        //    if (!orthoDragging && dragTarget == target)
        //        return true;

        //    if (orthoDragging && (dragTargetHorizontal == target || dragTargetVertical == target))
        //        return true;

        //    return false;

        //}

        public bool HasDragTarget(Button target)
        {

            // Check if this button targets a given button as its dragtargets.

            if (!orthoDragging && dragTargetButton == target)
                return true;

            if (orthoDragging && (dragTargetButtonHor == target || dragTargetButtonVer == target))
                return true;

            return false;

        }


        public Constraint GetConstraint()
        {
            // Retrieve  constraint  based on drag direction

          
                    return constraint;

            
        }

        //public Constraint GetConstraint(DIRECTION dir = DIRECTION.FREE)
        //{
        //    // Retrieve  constraint  based on drag direction

        //    switch (dir)
        //    {
        //        case DIRECTION.HORIZONTAL:
        //            return constraintHorizontal;

        //        case DIRECTION.VERTICAL:
        //            return constraintVertical;

        //        case DIRECTION.FREE:
        //        default:
        //            return constraint;

        //    }
        //}


        public float GetDeltaAngle()
        {

            Warning("this may be broken");
            Vector3 anchor = dragTargetButton.gameObject.GetComponent<RectTransform>().anchoredPosition;
            Vector2 relativePosition = new Vector2(anchor.x, anchor.y) - constraint.anchor;

            float angle = Mathf.Atan2(relativePosition.y, relativePosition.x);

            deltaAngle = lastAngle - angle;
            lastAngle = angle;

            return deltaAngle;
        }


        public Vector2 GetDeltaPosition()
        {
            Warning("this may be broken");
            deltaPosition.x = gameObject.transform.position.x - lastPosition.x;
            deltaPosition.y = gameObject.transform.position.y - lastPosition.y;

            lastPosition.x = gameObject.transform.position.x;
            lastPosition.y = gameObject.transform.position.y;

            return deltaPosition;
        }

        public void SetSpringTarget (int _index)
        {

            SpringIndex = _index;

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