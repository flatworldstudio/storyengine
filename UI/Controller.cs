using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace StoryEngine.UI
{

    public class Controller
    {

        string me = "UiController";

        Event activeUiEvent;
        int cycle = 0;
        List<Event> uiEventStack;

        // Scale currently not implementened, working with fixed size....

     //   float currentScale = 1;


        public Controller()
        {
            reset();
        }


        public void reset()
        {

            activeUiEvent = new Event();
            uiEventStack = new List<Event>();
            uiEventStack.Add(activeUiEvent);
        }

        //public UserCallBack updateUi(Interface activeInterface){

        // Main UX method. Checks for user interaction and updates ux elements. Return the result of the user interaction.
        // Set a float for the current scale applied by the canvas. We need this to convert screen pixels to ui pixels.

        public UserCallBack updateUi(Layout _layout)
        {

            UserCallBack callBack = new UserCallBack();

            Interface activeInterface = null;

            //currentScale = activeInterface.canvasObject.GetComponent<Canvas>().scaleFactor;

      //      currentScale = _layout.canvas.scaleFactor;

            // log current stack count to track changes.
            // apply this frame's user interaction to the 'active' ui event.

            int stackSize = uiEventStack.Count;

            //applyUserInteraction(activeUiEvent, activeInterface);

            applyUserInteraction(activeUiEvent);

            Plane activePlane = _layout.FindPlaneByPoint(activeUiEvent.position);

            if (activePlane != null)
            {

                activeInterface=activePlane.interFace;


            }




            //if (activePlane != null)
            //{
            //    //Debug.Log(activeUiEvent.position + "Pointing at " + pointingAt.address);
            // //   activeInterface = _layout.GetInterfaceByAddress(activePlane.address);

            //    activeInterface=activePlane.interFace;

            //    //Vector3 planeOffset = _layout.GetOffsetForPlane(pointingAt);

            //}

            // if a user action started, perform additional handling before processing. (If it ended we process first, then perform additional handling, see below).

            if (activeUiEvent.touch == TOUCH.BEGAN)
            {

                // a user action just began. find any objects and buttons the event is targeting. 

                setUiTargetObjects(activeUiEvent, activePlane);

                // if this new action is targeting the same object or button as an event already on the stack, the old event should be removed.

                int e = uiEventStack.Count - 1;

                while (e >= 0)
                {

                    Event checkEvent = uiEventStack[e];

                    if (checkEvent != activeUiEvent)
                    {

                        bool removeThis = false;

                        if (activeUiEvent.targetButton != null)
                        {

                            //  we check all the new events button targets against the actual single target of the check event.

                            if (checkEvent.targetButton != null && activeUiEvent.targetButton.HasDragTarget(checkEvent.targetButton.GetDragTarget(checkEvent.direction)))

                            {

                                removeThis = true;
                                //         Debug.Log("Removing ui event targeting the same dragtarget");

                            }


                        }
                        else if (activeUiEvent.target3D != null)
                        {

                            // if same 3d target, remove old.

                            if (checkEvent.target3D == activeUiEvent.target3D)
                            {

                                removeThis = true;
                            }


                        }
                        else
                        {

                            // if both have no targets at all, remove old. 

                            if (checkEvent.targetButton == null && checkEvent.target2D == null && checkEvent.target3D == null)
                            {

                                removeThis = true;

                            }

                        }
                        if (removeThis)
                        {

                            uiEventStack.RemoveAt(e);

                        }

                    }

                    e--;
                }

            }

            // now handle the stack of ui events. this way, events can play out (inertia, springing) while the user starts new interaction.
            // create empty callback object




            int i = 0;

            while (i < uiEventStack.Count)
            {

                Event uiEvent = uiEventStack[i];

                //			Log.Message ( "handling event stack of size " + uiEventStack.Count);

                processUiEvent(uiEvent, activeInterface); // note that this may not work as expected

                // If an old event is no longer inert or springing, remove it.

                if (uiEvent != activeUiEvent && uiEvent.isInert == false && uiEvent.isSpringing == false)
                {

                    uiEventStack.RemoveAt(i);
                    //      Debug.Log("removing event, total left" + uiEventStack.Count);

                    i--;

                }

                i++;

            }

            if (activeUiEvent.callback != "")
            {

                callBack.label = activeUiEvent.callback;
                callBack.sender = activeUiEvent.target2D;
                callBack.trigger = true;

                //Debug.Log ("callbackResult: " + callbackResult);

            }

            if (activeUiEvent.touch == TOUCH.ENDED)
            {

                // the active event just ended. set inert and springing to true.

                activeUiEvent.touch = TOUCH.NONE;

                activeUiEvent.isInert = true;


                if (activeUiEvent.targetButton != null)
                {

                    activeUiEvent.isSpringing = true;

                    // If we were dragging an ortho button, we want an event on the other direction to spring.

                    if (activeUiEvent.targetButton.orthoDragging)
                    {
                        if (activeUiEvent.direction != DIRECTION.FREE)
                        {
                            //    Debug.Log("adding springing event for " + (activeUiEvent.direction == DIRECTION.HORIZONTAL ? "vertical" :"horizontal"));

                            Event springEvent = activeUiEvent.clone();
                            springEvent.direction = activeUiEvent.direction == DIRECTION.HORIZONTAL ? DIRECTION.VERTICAL : DIRECTION.HORIZONTAL;
                            springEvent.isSpringing = true;
                            springEvent.action = ACTION.SINGLEDRAG;
                            uiEventStack.Add(springEvent);

                        }
                        else
                        {

                            //     Debug.Log("touch ended, ortho button, direction free");
                            // Direction was never set. We'll create springing events for both direcionts.

                            activeUiEvent.direction = DIRECTION.HORIZONTAL;
                            Event springEvent = activeUiEvent.clone();

                            springEvent.direction = DIRECTION.VERTICAL;
                            uiEventStack.Add(springEvent);

                        }
                    }
                }

                if (activeUiEvent.action == ACTION.TAP)
                {

                    // tap must only be executed once, after that it becomes an inert singledrag event.

                    activeUiEvent.action = ACTION.SINGLEDRAG;
                    Debug.Log("tap -> singledrag");

                }

                // Create a new uievent to catch the next interaction 

                Event newEvent = new Event();
                uiEventStack.Add(newEvent);
                activeUiEvent = newEvent;

            }

            int stackSizeNew = uiEventStack.Count;

            if (stackSize != stackSizeNew)
            {

                //			Log.Message ( cycle + " Event stack size changed: " + stackSizeNew);

            }

            if (stackSizeNew > 10)
                Log.Warning("Ui event stack exceeds 10, potential overflow.", me);

            // apply brightness for all button objects

            if (activeInterface != null)
            {

                Dictionary<string, Button>.ValueCollection allButtons = activePlane.interFace.uiButtons.Values;

                foreach (Button button in allButtons)
                {
                    button.ApplyColour();
                }

            }

            return callBack;

        }

        //	public void setSpringTarget (string target,UiConstraint constraint, int index){

        public void setSpringTarget(Button button, int index, DIRECTION dir = DIRECTION.FREE)
        {

            // Moves an interface segment (dragtarget, so a parent object) to a given spring. 
            // it uses a button as a hook. checks if any event is targeting the same dragtarget to prevent interference.
            // If there is we just take over. Could delete and replace it as well...

            int i = uiEventStack.Count - 1;

            while (i >= 0)
            {

                Event uie = uiEventStack[i];

                if (uie.targetButton != null && uie.targetButton.GetDragTarget(uie.direction) == button.GetDragTarget(dir))
                {

                    // the event explicitly targets the (explicit) target of the passed in button
                    //remove the event
                    uiEventStack.RemoveAt(i);

                }

                i--;

            }


            Log.Message("Adding a springing event for spring target call.", me);

            Event springEvent = new Event();

            springEvent.targetButton = button;
            springEvent.action = ACTION.SINGLEDRAG;
            springEvent.target2D = button.gameObject;
            springEvent.isSpringing = true;
            springEvent.springIndex = index;

            uiEventStack.Add(springEvent);


        }


        // lower level
        void applyUserInteraction(Event ui)
        {

            //void applyUserInteraction(Event ui, Interface theUiState)
            //{

            cycle++;
            cycle = cycle % 1000;

            // check user mouse/touch interaction and populate this passed-in UIEVENT accordingly

#if UNITY_IOS
            Event storeUi = ui.clone();
#endif

            ui.touch = TOUCH.NONE;

            ui.dd = 0;


#if UNITY_EDITOR || UNITY_STANDALONE

            //		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer) {

            // if we're on macos .... or windows?

            ui.x = Input.mousePosition.x;
            ui.y = Input.mousePosition.y;
            ui.dx = ui.x - ui.px;
//            ui.dx = ui.dx / currentScale;

            ui.dy = ui.y - ui.py;
   //         ui.dy = ui.dy / currentScale;

            ui.px = ui.x;
            ui.py = ui.y;

            if (Input.GetButton("Fire1"))
            {

                //ui.x = Input.mousePosition.x;
                //ui.y = Input.mousePosition.y;
                //ui.dx = ui.x - ui.px;
                //ui.dx = ui.dx / currentScale;

                //ui.dy = ui.y - ui.py;
                //ui.dy = ui.dy / currentScale;

                //ui.px = ui.x;
                //ui.py = ui.y;

                if (!ui.firstFrame)
                { // skip first frame because delta value will jump.

                    ui.action = ACTION.SINGLEDRAG;
                    ui.touch = TOUCH.TOUCHING;

                    if (Input.GetKey(KeyCode.Space))
                    {
                        // equivalent to doubletouch dragging only
                        ui.action = ACTION.DOUBLEDRAG;
                    }
                    if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                    {
                        // equivalent to doubletouch pinching only
                        ui.action = ACTION.DOUBLEDRAG;
                        ui.dd = ui.dx;
                        ui.dx = 0;
                        ui.dy = 0;
                    }
                }
                else
                {
                    ui.firstFrame = false;
                    ui.touch = TOUCH.BEGAN;
                    ui.dx = 0;
                    ui.dy = 0;
                }
                trackTap(ui);
            }

            if (Input.GetButtonUp("Fire1"))
            {

                ui.touch = TOUCH.ENDED;

                if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                {
                    // equivalent to doubletouch pinching only
                    ui.action = ACTION.DOUBLEDRAG;
                    ui.dd = ui.dx;
                    ui.dx = 0;
                    ui.dy = 0;
                }


                if (wasTap(ui))
                {

                    ui.action = ACTION.TAP;

                }
                else
                {

                }

                ui.firstFrame = true;

            }
            //		}

#endif


#if UNITY_IOS

//		if (Application.platform == RuntimePlatform.IPhonePlayer) {

			// if we're on ios

			if (Input.touchCount == 1) {
				// review single touch
				Vector2 tp = Input.GetTouch (0).position;

				switch (Input.GetTouch (0).phase) {
				case TouchPhase.Began:
					ui.x = tp.x;
					ui.y = tp.y;
					trackTap (ui);
					ui.touch = TOUCH.BEGAN;
					break;

				case TouchPhase.Moved:
					Vector2 touchDelta = Input.GetTouch (0).deltaPosition;

//		ui.dx = touchDelta.x / Screen.height * 720f;
//					ui.dy = touchDelta.y / Screen.height * 720f;

		ui.dx = touchDelta.x /currentScale;
		ui.dy = touchDelta.y /currentScale;


					ui.action = ACTION.SINGLEDRAG;
					ui.touch = TOUCH.TOUCHING;
					trackTap (ui);
					break;

				case TouchPhase.Stationary:
					ui.dx = 0;
					ui.dy = 0;
					ui.action = ACTION.SINGLEDRAG;
					ui.touch = TOUCH.TOUCHING;
					trackTap (ui);
					break;

				case TouchPhase.Ended:

					ui.touch = TOUCH.ENDED;

					if (wasTap (ui)) {
						ui.action = ACTION.TAP;
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
					ui.d = Vector2.Distance (tp0, tp1);
					ui.touch = TOUCH.TOUCHING;

				} else if (phase0 == TouchPhase.Began && phase1 == TouchPhase.Began) {
					// if both start at the same time, aim in between to set targets and initialise the d value
					ui.x = (tp0.x + tp1.x) / 2f;
					ui.y = (tp0.y + tp1.y) / 2f;
					ui.d = Vector2.Distance (tp0, tp1);
					ui.touch = TOUCH.BEGAN;

				} else if (phase0 == TouchPhase.Ended && phase1 == TouchPhase.Began) {
					// unlikely but could happen: flicking fingers in a single frame. catch the start of a new single touch.
					ui.x = tp1.x;
					ui.y = tp1.y;
					ui.touch = TOUCH.BEGAN;

				} else if ((phase0 == TouchPhase.Moved || phase0 == TouchPhase.Stationary) && (phase0 == TouchPhase.Moved || phase0 == TouchPhase.Stationary)) {
					// dragging
					Vector2 touchDelta0 = Input.GetTouch (0).deltaPosition;
					Vector2 touchDelta1 = Input.GetTouch (1).deltaPosition;

//					ui.dx = (touchDelta0.x + touchDelta1.x) / 2f / Screen.height * 720f; 
//					ui.dy = (touchDelta0.y + touchDelta1.y) / 2f / Screen.height * 720f; 

		ui.dx = (touchDelta0.x + touchDelta1.x) / 2f /currentScale; 
		ui.dy = (touchDelta0.y + touchDelta1.y) / 2f /currentScale; 

					ui.d = Vector2.Distance (tp0, tp1);
					ui.dd = ui.d - storeUi.d;

					ui.action = ACTION.DOUBLEDRAG;
					ui.touch = TOUCH.TOUCHING;
				}
			}

//		}
#endif
        }

        void trackTap(Event ui)
        {
            ui.tapCount += Time.deltaTime;
        }

        bool wasTap(Event ui)
        {
            bool result = false;

            if (ui.dx > -10f && ui.dx < 10f && ui.dy > -10f && ui.dy < 10f && ui.tapCount > 0 && ui.tapCount < 0.25f)
            {
                result = true;
                //				Log.Message ( cycle + " tap detected");
            }
            ui.tapCount = 0;
            return result;
        }


        void setUiTargetObjects(Event _uiEvent, Plane _plane)
        {

            if (_plane.interFace == null)
                return;


            // finds gameobject (2D and 3D) for possible manipulation, by raycasting. objects are registred in the UiEvent.

            RaycastHit hit;

            Vector2 uiPosition = new Vector2(_uiEvent.x, _uiEvent.y);

            uiPosition-=_plane.interFace.GetAnchorOffset();


            // Correct for the layout of the plane, and possibly in the plane. The offset can be controlled via the plane drawing delegate.

            //Vector2 anchorPos = _plane.interFace.gameObject.GetComponent<RectTransform>().anchoredPosition;

            //uiPosition -= anchorPos;
            //uiPosition -= _plane.screenOffset;

            //Vector3 screenPosition =new Vector3(uiPosition.x,uiPosition.y,0); 

            // cast a 3d ray from the camera we are controlling to find any 3D objects.

            _uiEvent.target3D = null;

            if (_plane.interFace.uiCam3D != null)
            {

                UnityEngine.Camera activeCamera = _plane.interFace.uiCam3D.camera;

                Ray ray = activeCamera.ScreenPointToRay(uiPosition);

                //Debug.DrawRay(ray.origin, 10f * ray.direction, Color.red, 3f, false);

                int layerMask = 1 << 8; // only check colliders in layer '8'. 

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {

                    _uiEvent.target3D = hit.transform.gameObject;
                    Debug.Log("raycast hit: " + hit.transform.gameObject.name);

                }

            }

            // cast a 2d ray in the canvas we're controlling.

            GraphicRaycaster gfxRayCaster = _plane.interFace.canvasObject.GetComponent<GraphicRaycaster>();

            //Create the PointerEventData with null for the EventSystem

            PointerEventData pointerEventData = new PointerEventData(null);

            //Set required parameters, in this case, mouse position

            pointerEventData.position = uiPosition;

            //Create list to receive all results

            List<RaycastResult> results = new List<RaycastResult>();

            //Raycast it

            gfxRayCaster.Raycast(pointerEventData, results);

            if (results.Count > 0)
            {

                _uiEvent.target2D = results[0].gameObject;

                // find out if this 2d object is a button.

                Button checkButton = null;
                _plane.interFace.uiButtons.TryGetValue(_uiEvent.target2D.transform.name, out checkButton);

                _uiEvent.targetButton = checkButton;

                // Set ui event drag target and constraint. We don't have any ortho direction as this point, so we get the 'free dragging' one.

                //      ui.dragTarget = checkButton.GetDragTarget();
                //     ui.dragConstraint = checkButton.GetDragConstraint();


            }
            else
            {

                _uiEvent.target2D = null;

            }
        }



        // ------------------------------------------------------------------------------------------------------------------------------------------


        void processUiEvent(Event ui, Interface activeInterface)
        {

            // this handles the result of user interaction, taking the info in the active UIEVENT and taking action by checking it against the UISTATE

            ui.callback = "";

            UIArgs args = new UIArgs();

            args.activeInterface = activeInterface;
            args.delta = Vector3.zero;
            args.uiEvent = ui;

            switch (ui.action)
            {



                case ACTION.TAP:

                    ui.action = ACTION.SINGLEDRAG;

                    if (activeInterface == null)
                        break;

                    if (ui.target2D != null)
                        activeInterface.tap_2d(this, args);
                    else if (ui.target3D != null)
                        activeInterface.tap_3d(this, args);
                    else
                        activeInterface.tap_none(this, args);

                    break;

                // 

                case ACTION.SINGLEDRAG:

                    // If this is an orthodrag button, we need to set and lock the initial direction.
                    // We also get the appropriate targets and constraints for that direction and load them into the uievent.

                    if (ui.targetButton != null && ui.targetButton.orthoDragging && ui.direction == DIRECTION.FREE)
                    {
                        if (Mathf.Abs(ui.dx) > Mathf.Abs(ui.dy))
                        {
                            ui.direction = DIRECTION.HORIZONTAL;

                            //        Debug.Log("Locking to drag horizontally");
                        }
                        if (Mathf.Abs(ui.dx) < Mathf.Abs(ui.dy))
                        {
                            ui.direction = DIRECTION.VERTICAL;

                            //       Debug.Log("Locking to drag vertically");
                        }




                    }

                    //			Debug.Log ("single drag");


                    if (activeInterface == null)
                        break;

                    args.delta = new Vector3(ui.dx, ui.dy, 0);

                    if (ui.target2D != null)
                        activeInterface.single_2d(this, args);
                    else if (ui.target3D != null)
                        activeInterface.single_3d(this, args);
                    else
                        activeInterface.single_none(this, args);

                    break;

                case ACTION.DOUBLEDRAG:

                    if (activeInterface == null)
                        break;

                    args.delta = new Vector3(ui.dx, ui.dy, ui.dd);

                    if (ui.target2D != null)
                        activeInterface.double_2d(this, args);
                    else if (ui.target3D != null)
                        activeInterface.double_3d(this, args);
                    else
                        activeInterface.double_none(this, args);

                    break;

                default:

                    if (activeInterface == null)
                        break;

                    activeInterface.none(this, args);

                    break;
            }

            if (ui.isInert)
            {

                float iVel = 0f;

                if (Mathf.Abs(ui.dd) < 1f)
                    ui.dd = 0;
                else
                    ui.dd = Mathf.SmoothDamp(ui.dd, 0, ref iVel, 0.075f);

                if (Mathf.Abs(ui.dx) < 1f)
                    ui.dx = 0;
                else
                    ui.dx = Mathf.SmoothDamp(ui.dx, 0, ref iVel, 0.075f);

                if (Mathf.Abs(ui.dy) < 1f)
                    ui.dy = 0;
                else
                    ui.dy = Mathf.SmoothDamp(ui.dy, 0, ref iVel, 0.075f);

                if (ui.dx == 0 && ui.dy == 0 && ui.dd == 0)
                {
                    ui.isInert = false;
                }


            }



            //			if (ui.touch == UITOUCH.NONE && ui.target2D != null) {
            //				// no touch, lettings springs run out while we have a target: apply a singledrag with delta 0
            //				singleDrag2D (state, ui, Vector2.zero);
            //			}

            //			if (ui.action == UIACTION.INERTIA && ui.target2D != null) {
            //				// no touch, lettings springs run out while we have a target: apply a singledrag with delta 0
            //				singleDrag2D (state, ui, Vector2.zero);
            //			}


        }


        // ------------------------------------------------------------------------------------------------------------------------------------------
        // execution of ui events


        void setRaycastActive(string name, bool value, Interface activeInterface)
        {
            Button theButton;
            activeInterface.uiButtons.TryGetValue(name, out theButton);

            if (theButton != null)
                theButton.image.raycastTarget = value;

        }

        bool brightnessIsChanging(string name, Interface activeInterface)
        {
            bool result = false;
            Button theButton;
            activeInterface.uiButtons.TryGetValue(name, out theButton);

            if (theButton != null)
            {
                if (theButton.brightness != theButton.targetBrightness)
                {
                    result = true;

                }
            }

            return result;
        }









    }

}





