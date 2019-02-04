using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;


namespace StoryEngine.UI
{

    /*!
   * \brief 
   * Contains a single interaction event and updates it depending on the platform.
   * 
   * Multiple events can occur simultaneously, eg when an event is still inert and another begins.
   * The Controller maintains a stack of events.
   *   
   */
    
    public class Event
    {
        // holds user interaction description. note that x and y are NOT continuously updated for touch.
        string ID = "Event";

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

        public Plane plane; // the plane (and possibly interface) that this event plays out in.

        //public InterFace interFace;

        Vector2 __position;

        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.
        void Log(string message) => StoryEngine.Log.Message(message, ID);
        void Warning(string message) => StoryEngine.Log.Warning(message, ID);
        void Error(string message) => StoryEngine.Log.Error(message, ID);
        void Verbose(string message) => StoryEngine.Log.Message(message, ID, LOGLEVEL.VERBOSE);

        public Vector2 position
        {

            get
            {
                __position.x = x;
                __position.y = y;
                return __position;
            }
            set
            {
                Warning("Can't set this value.");

            }

        }

        public Event()
        {
            __position = Vector2.zero;

            target2D = null;
            target3D = null;
            targetButton = null;
            px = 0;
            py = 0;
            firstFrame = true;
            action = ACTION.VOID;
            touch = TOUCH.NONE;
            direction = DIRECTION.FREE;
            callback="";

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

        //void applyUserInteraction(Event ui)

        public void GetUserActivity()
        {

            //void applyUserInteraction(Event ui, Interface theUiState)
            //{

            //cycle++;
            //cycle = cycle % 1000;

            // check user mouse/touch interaction and populate this passed-in UIEVENT accordingly

            //#if UNITY_IOS
            //                        Event storeUi = ui.clone();
            //#endif

            float lastdd = dd;

            touch = TOUCH.NONE;

            dd = 0;


#if UNITY_EDITOR || UNITY_STANDALONE

            //      if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer) {

            // if we're on macos .... or windows?

            x = Input.mousePosition.x;
            y = Input.mousePosition.y;
            dx = x - px;
            //            ui.dx = ui.dx / currentScale;

            dy = y - py;
            //         ui.dy = ui.dy / currentScale;

            px = x;
            py = y;

            if (Input.GetButton("Fire1"))
            {

                //ui.x = Input.mousePosition.x;
                //ui.y = Input.mousePosition.y;
                //ui.dx = ui.x - ui.px;
                //ui.dx = ui.dx / currentScale;

                //dy = ui.y - ui.py;
                //ui.dy = ui.dy / currentScale;

                //ui.px = ui.x;
                //ui.py = ui.y;

                if (!firstFrame)
                { // skip first frame because delta value will jump.

                    action = ACTION.SINGLEDRAG;
                    touch = TOUCH.TOUCHING;

                    if (Input.GetKey(KeyCode.Space))
                    {
                        // equivalent to doubletouch dragging only
                        action = ACTION.DOUBLEDRAG;
                    }
                    if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                    {
                        // equivalent to doubletouch pinching only
                        action = ACTION.DOUBLEDRAG;
                        dd = dx;
                        dx = 0;
                        dy = 0;
                    }
                }
                else
                {
                    firstFrame = false;
                    touch = TOUCH.BEGAN;
                    dx = 0;
                    dy = 0;
                }

                trackTap();
            }

            if (Input.GetButtonUp("Fire1"))
            {

                touch = TOUCH.ENDED;

                if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                {
                    // equivalent to doubletouch pinching only
                    action = ACTION.DOUBLEDRAG;
                    dd = dx;
                    dx = 0;
                    dy = 0;
                }


                if (wasTap())
                {

                    action = ACTION.TAP;

                }
                else
                {

                }

                firstFrame = true;

            }
            //      }

#endif


#if UNITY_IOS
            
//      if (Application.platform == RuntimePlatform.IPhonePlayer) {

            // if we're on ios

            if (Input.touchCount == 1) {
                // review single touch
                Vector2 tp = Input.GetTouch (0).position;

                switch (Input.GetTouch (0).phase) {
                case TouchPhase.Began:
                    x = tp.x;
                    y = tp.y;
                    trackTap ();
                    touch = TOUCH.BEGAN;
                    break;

                case TouchPhase.Moved:
                    Vector2 touchDelta = Input.GetTouch (0).deltaPosition;

//      dx = touchDelta.x / Screen.height * 720f;
//                  dy = touchDelta.y / Screen.height * 720f;

        //dx = touchDelta.x /currentScale;
        //dy = touchDelta.y /currentScale;


                        dx = touchDelta.x;
                        dy = touchDelta.y;

                        action = ACTION.SINGLEDRAG;
                    touch = TOUCH.TOUCHING;
                    trackTap ();
                    break;

                case TouchPhase.Stationary:
                    dx = 0;
                    dy = 0;
                    action = ACTION.SINGLEDRAG;
                    touch = TOUCH.TOUCHING;
                    trackTap ();
                    break;

                case TouchPhase.Ended:

                    touch = TOUCH.ENDED;

                    if (wasTap ()) {
                        action = ACTION.TAP;
                    } else {

                    }
                    break;

                default:
                    break;
                }
            }

            if (Input.touchCount == 2) {
                // review double touch 
                TouchPhase phase0 = Input.GetTouch (0).phase;
                TouchPhase phase1 = Input.GetTouch (1).phase;

                Vector2 tp0 = Input.GetTouch (0).position;
                Vector2 tp1 = Input.GetTouch (1).position;

                if ((phase0 == TouchPhase.Moved || phase0 == TouchPhase.Stationary) && phase1 == TouchPhase.Began) {
                    // if one finger was touching, we leave the targets untouched, and initialise the d value
                    d = Vector2.Distance (tp0, tp1);
                    touch = TOUCH.TOUCHING;

                } else if (phase0 == TouchPhase.Began && phase1 == TouchPhase.Began) {
                    // if both start at the same time, aim in between to set targets and initialise the d value
                    x = (tp0.x + tp1.x) / 2f;
                    y = (tp0.y + tp1.y) / 2f;
                    d = Vector2.Distance (tp0, tp1);
                    touch = TOUCH.BEGAN;

                } else if (phase0 == TouchPhase.Ended && phase1 == TouchPhase.Began) {
                    // unlikely but could happen: flicking fingers in a single frame. catch the start of a new single touch.
                    x = tp1.x;
                    y = tp1.y;
                    touch = TOUCH.BEGAN;

                } else if ((phase0 == TouchPhase.Moved || phase0 == TouchPhase.Stationary) && (phase0 == TouchPhase.Moved || phase0 == TouchPhase.Stationary)) {
                    // dragging
                    Vector2 touchDelta0 = Input.GetTouch (0).deltaPosition;
                    Vector2 touchDelta1 = Input.GetTouch (1).deltaPosition;

//                  dx = (touchDelta0.x + touchDelta1.x) / 2f / Screen.height * 720f; 
//                  dy = (touchDelta0.y + touchDelta1.y) / 2f / Screen.height * 720f; 

        //dx = (touchDelta0.x + touchDelta1.x) / 2f /currentScale; 
        //dy = (touchDelta0.y + touchDelta1.y) / 2f /currentScale;

                    dx = (touchDelta0.x + touchDelta1.x) / 2f ;
                    dy = (touchDelta0.y + touchDelta1.y) / 2f ;

                    d = Vector2.Distance (tp0, tp1);
                    dd = d - lastdd;

                    action = ACTION.DOUBLEDRAG;
                    touch = TOUCH.TOUCHING;
                }
            }

//      }
#endif
        }

        void trackTap()
        {
            tapCount += Time.deltaTime;
        }

        bool wasTap()
        {
            bool result = false;

            if (dx > -10f && dx < 10f && dy > -10f && dy < 10f && tapCount > 0 && tapCount < 0.25f)
            {
                result = true;
                //              Log.Message ( cycle + " tap detected");
            }
            tapCount = 0;
            return result;
        }

        public void SetTargets()
        {

            if (plane == null || plane.interFace == null)
                return;


            // finds gameobject (2D and 3D) for possible manipulation, by raycasting. objects are registred in the UiEvent.

            RaycastHit hit;

            Vector2 uiPosition = new Vector2(x, y);

            Vector2 uiPositionOffset = uiPosition - plane.interFace.GetAnchorOffset();


            // Correct for the layout of the plane, and possibly in the plane. The offset can be controlled via the plane drawing delegate.

            //Vector2 anchorPos = _plane.interFace.gameObject.GetComponent<RectTransform>().anchoredPosition;

            //uiPosition -= anchorPos;
            //uiPosition -= _plane.screenOffset;

            //Vector3 screenPosition =new Vector3(uiPosition.x,uiPosition.y,0); 

            // cast a 3d ray from the camera we are controlling to find any 3D objects.

            target3D = null;

            if (plane.interFace.uiCam3D != null)
            {

                UnityEngine.Camera activeCamera = plane.interFace.uiCam3D.camera;

                Ray ray = activeCamera.ScreenPointToRay(uiPositionOffset);

                Debug.DrawRay(ray.origin, 10f * ray.direction, Color.red, 3f, false);

                int layerMask = 1 << 8; // only check colliders in layer '8'. 

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {

                    target3D = hit.transform.gameObject;
                    Verbose("raycast hit: " + hit.transform.gameObject.name);

                }

            }

            // cast a 2d ray in the canvas we're controlling.

            GraphicRaycaster gfxRayCaster = plane.interFace.canvasObject.GetComponent<GraphicRaycaster>();

            //Create the PointerEventData with null for the EventSystem

            PointerEventData pointerEventData = new PointerEventData(null);

            //Set required parameters, in this case, mouse position

            pointerEventData.position = uiPosition;

            //uiPosition-=_plane.interFace.GetAnchorOffset();

            //Create list to receive all results

            List<RaycastResult> results = new List<RaycastResult>();

            //Raycast it

            gfxRayCaster.Raycast(pointerEventData, results);

            if (results.Count > 0)
            {

                target2D = results[0].gameObject;

                Verbose("Targeting object2d " + target2D.name);
                // find out if this 2d object is a button.

                Button checkButton = null;
                if (plane.interFace.uiButtons.TryGetValue(target2D.transform.name, out checkButton))
                {
                    Verbose("targeting button " + checkButton.name);
                }

                targetButton = checkButton;


                // Set ui event drag target and constraint. We don't have any ortho direction as this point, so we get the 'free dragging' one.

                //      ui.dragTarget = checkButton.GetDragTarget();
                //     ui.dragConstraint = checkButton.GetDragConstraint();


            }
            else
            {

                target2D = null;

            }
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




}