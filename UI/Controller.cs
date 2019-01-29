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

        int cycle = 0; // for debugging

        List<Event> uiEventStack;
        List<Button> AnimatingButtons;

        //   float currentScale = 1; // Not implemented right now...

        public static Controller instance;

        public Controller()
        {
            instance = this;
            reset();
        }

        public void reset()
        {

            activeUiEvent = new Event();
            uiEventStack = new List<Event>();
            uiEventStack.Add(activeUiEvent);
            AnimatingButtons = new List<Button>();

        }

        // ------------------------------------ MAIN UPDATE METHOD -------------------------------------------------------------------
       
        // Checks and applies user interaction. Keeps a stack of ui events to allow for inertia, springing.
        // NOT IMPLEMENTED: Set a float for the current scale applied by the canvas. We need this to convert screen pixels to ui pixels.

     


        public UserCallBack updateUi(Layout _layout)
        {
            cycle=(cycle+1)%1000;

            UserCallBack callBack = new UserCallBack();

            int stackSize = uiEventStack.Count;

            activeUiEvent.GetUserActivity(); // get mouse movement, touches, taps

            //activeUiEvent.plane = _layout.FindPlaneByPoint(activeUiEvent.position); // get the plane the user is active in

            // If a user touch/click just began, set targets and remove any old event targeting the same objects. 

            if (activeUiEvent.touch == TOUCH.BEGAN)
            {
                activeUiEvent.plane = _layout.FindPlaneByPoint(activeUiEvent.position); // get the plane the user is active in

                activeUiEvent.SetTargets();

                int e = uiEventStack.Count - 1;

                while (e >= 0)
                {

                    Event oldEvent = uiEventStack[e];

                    if (oldEvent != activeUiEvent)
                    {

                        bool removeOld = false;

                        if (activeUiEvent.targetButton != null)
                        {

                            //  we check all the new events button targets against the actual single target of the check event.

                            if (oldEvent.targetButton != null && activeUiEvent.targetButton.HasDragTarget(oldEvent.targetButton.GetDragTarget(oldEvent.direction)))

                            {

                                removeOld = true;
                                //         Debug.Log("Removing ui event targeting the same dragtarget");

                            }


                        }
                        else if (activeUiEvent.target3D != null)
                        {

                            // if same 3d target, remove old.

                            if (oldEvent.target3D == activeUiEvent.target3D)
                            {

                                removeOld = true;
                            }


                        }
                        else
                        {

                            // if both have no targets at all, remove old. 

                            if (oldEvent.targetButton == null && oldEvent.target2D == null && oldEvent.target3D == null)
                            {

                                removeOld = true;

                            }

                        }
                        if (removeOld)
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

                processUiEvent(uiEvent);

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

                Debug.Log ("callbackResult: " +  callBack.label);

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



            AnimateButtons();

            return callBack;

        }


        void AnimateButtons()
        {
            int i=AnimatingButtons.Count-1;

            while (i>=0){
                
                Button button=AnimatingButtons[i];
                button.ApplyBrightness();

                i--;
            }

        }

        public void AddAnimatingButton (Button _button){

            if (!AnimatingButtons.Contains(_button))
                AnimatingButtons.Add(_button);
            
        }

        public void RemoveAnimatingButton (Button _button){

                AnimatingButtons.Remove(_button);

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

        // ------------------------------------------------------------------------------------------------------------------------------------------

        void processUiEvent(Event ui)
        {

            //ui.callback = "";

            if (ui.plane == null || ui.plane.interFace == null)
                return;

            ui.callback = "";
           
            UIArgs args = new UIArgs();

            //args.activeInterface = activeInterface;

            args.delta = Vector3.zero;
            args.uiEvent = ui;

            InterFace interFace = ui.plane.interFace;

            switch (ui.action)
            {

                case ACTION.TAP:

                    ui.action = ACTION.SINGLEDRAG;

                    //if (interFace == null)
                    //break;

                    if (ui.target2D != null)
                        interFace.tap_2d(this, args);
                    else if (ui.target3D != null)
                        interFace.tap_3d(this, args);
                    else
                        interFace.tap_none(this, args);

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


                    //if (activeInterface == null)
                    //break;

                    args.delta = new Vector3(ui.dx, ui.dy, 0);

                    if (ui.target2D != null)
                        interFace.single_2d(this, args);
                    else if (ui.target3D != null)
                        interFace.single_3d(this, args);
                    else
                        interFace.single_none(this, args);

                    break;

                case ACTION.DOUBLEDRAG:

                    //if (activeInterface == null)
                    //break;

                    args.delta = new Vector3(ui.dx, ui.dy, ui.dd);

                    if (ui.target2D != null)
                        interFace.double_2d(this, args);
                    else if (ui.target3D != null)
                        interFace.double_3d(this, args);
                    else
                        interFace.double_none(this, args);

                    break;

                default:

                    //if (activeInterface == null)
                    //break;

                    interFace.none(this, args);

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


        void setRaycastActive(string name, bool value, InterFace activeInterface)
        {
            Button theButton;
            activeInterface.uiButtons.TryGetValue(name, out theButton);

            if (theButton != null)
                theButton.image.raycastTarget = value;

        }

        bool brightnessIsChanging(string name, InterFace activeInterface)
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





