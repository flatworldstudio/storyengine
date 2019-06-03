using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace StoryEngine.UI
{

    /*!
* \brief
* Handles user interaction, to be called every frame.
* 
* Returns a callback if user clicked on a button.
* Takes a Layout as argument, which can be contain Plane objects which can each hold an Interface.
* Maintains a stack of Event objects.
*/

    public class Controller
    {

        string ID = "Controller";

        Event activeUiEvent;

        int cycle = 0; // for debugging

        List<Event> uiEventStack;
        List<Button> AnimatingButtons;

        //   float currentScale = 1; // Not implemented right now...

        public static Controller instance;

        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.
        void Log(string message) => StoryEngine.Log.Message(message, ID);
        void Warning(string message) => StoryEngine.Log.Warning(message, ID);
        void Error(string message) => StoryEngine.Log.Error(message, ID);
        void Verbose(string message) => StoryEngine.Log.Message(message, ID, LOGLEVEL.VERBOSE);

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

        /*!\brief Main method to be called to run user interaction, returns callback.
         * 
         * Keeps a stack of ui Event objects to allow for intertia and springing.
         * Always has 1 active ui Event that contains current user activity
                   */



        public UserCallBack updateUi(Layout _layout)
        {
            cycle = (cycle + 1) % 1000;

            UserCallBack callBack = new UserCallBack();

            int stackSize = uiEventStack.Count;

            activeUiEvent.GetUserActivity(); // get (unscaled) mouse movement, touches, taps


            //activeUiEvent.plane = _layout.FindPlaneByPoint(activeUiEvent.position); // get the plane the user is active in

            // If a user touch/click just began, set targets and remove any old event targeting the same objects. 

            if (activeUiEvent.touch == TOUCH.BEGAN)
            {
                activeUiEvent.plane = _layout.FindPlaneByPoint(activeUiEvent.position); // get the plane the user is active in by screen coordinates

                //Debug.Log("targeting "+activeUiEvent.plane.address);

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

            // When we get user activity from mouse/touch we get screen coordinates.
            // Before we start applying drag etc to object, we need to scale the delta values.

            //float scale = (activeUiEvent.plane.interFace)
            //activeUiEvent.CorrectForCanvasScale();

            // now handle the stack of ui events. this way, events can play out (inertia, springing) while the user starts new interaction.
            // create empty callback object


            int i = 0;
            //Log("events "+uiEventStack.Count);
            while (i < uiEventStack.Count)
            {

                Event uiEvent = uiEventStack[i];

                //			Log.Message ( "handling event stack of size " + uiEventStack.Count);

                processUiEvent(uiEvent);

                // If an old event is no longer inert or springing, remove it.

                if (uiEvent != activeUiEvent && uiEvent.isInert == false && uiEvent.isSpringing == false)
                {

                    uiEventStack.RemoveAt(i);
                  Verbose("Removing event, total left " + uiEventStack.Count);




                    i--;

                }

                i++;

            }
            //foreach (Event e in uiEventStack)
            //{
            //    if (e.target2D != null)
            //        Verbose("event button " + e.target2D.name);
            //    else
            //        Verbose("event without target ");

            //}


            if (activeUiEvent.callback != "")
            {

                callBack.label = activeUiEvent.callback;
                callBack.sender = activeUiEvent.target2D;
                callBack.trigger = true;

                //Debug.Log ("callbackResult: " +  callBack.label);

            }

            if (activeUiEvent.touch == TOUCH.ENDED)
            {

                // the active event just ended. set inert and springing to true.

                activeUiEvent.touch = TOUCH.NONE;

               // if (activeUiEvent.action==ACTION.)

                activeUiEvent.isInert = true;

                if (activeUiEvent.targetButton != null)
                {
                    if (activeUiEvent.targetButton.GetDragTarget() != null)
                    {
                        activeUiEvent.isSpringing = true;
                    }
                  

                    // If we were dragging an ortho button, we want an event on the other direction to spring.

                    //if (activeUiEvent.targetButton.orthoDragging)
                    //{
                    //    if (activeUiEvent.direction != DIRECTION.FREE)
                    //    {
                    //        //    Debug.Log("adding springing event for " + (activeUiEvent.direction == DIRECTION.HORIZONTAL ? "vertical" :"horizontal"));

                    //        Event springEvent = activeUiEvent.clone();

                    //        springEvent.direction = activeUiEvent.direction == DIRECTION.HORIZONTAL ? DIRECTION.VERTICAL : DIRECTION.HORIZONTAL;
                    //        springEvent.isSpringing = true;
                    //        springEvent.action = ACTION.SINGLEDRAG;
                    //        Verbose("cloning event for springing");
                    //         uiEventStack.Add(springEvent);

                    //    }
                    //    else
                    //    {

                    //        //     Debug.Log("touch ended, ortho button, direction free");

                    //        // Direction was never set. We'll create springing events for both direcionts.
                    //        // ??? direction not set means it didn't go anywhere??
                    //        //activeUiEvent.direction = DIRECTION.HORIZONTAL;

                    //        //Event springEvent = activeUiEvent.clone();

                    //        //springEvent.direction = DIRECTION.VERTICAL;

                    //        //Verbose("cloning event for ortho springing");
                    //        //uiEventStack.Add(springEvent);

                    //    }
                    //}
                }

                if (activeUiEvent.action == ACTION.TAP)
                {

                    // tap must only be executed once, after that it becomes an inert singledrag event.

                    activeUiEvent.action = ACTION.SINGLEDRAG;
                    Log("tap -> singledrag");
                    //Debug.Log("tap -> singledrag");

                }

                // Create a new uievent to catch the next interaction 

                Event newEvent = new Event();
                uiEventStack.Add(newEvent);
                activeUiEvent = newEvent;

                Verbose("added new event");
            }

            int stackSizeNew = uiEventStack.Count;

            if (stackSize != stackSizeNew)
            {

                //			Log.Message ( cycle + " Event stack size changed: " + stackSizeNew);

            }

            if (stackSizeNew > 10)
                Warning("Ui event stack exceeds 10, potential overflow.");



            AnimateButtons();

            return callBack;

        }


        void AnimateButtons()
        {
            int i = AnimatingButtons.Count - 1;

            while (i >= 0)
            {
              
                Button button = AnimatingButtons[i];
                button.ApplyBrightness();

                i--;
            }

        }

        public void AddAnimatingButton(Button _button)
        {

            if (!AnimatingButtons.Contains(_button))
                AnimatingButtons.Add(_button);

        }

        public void RemoveAnimatingButton(Button _button)
        {

            AnimatingButtons.Remove(_button);

        }


        /*!\brief Move a Button to a given spring position.*/
        public void setSpringTarget(Button _button, int index, DIRECTION dir = DIRECTION.FREE)

        //public void setSpringTarget(Button button, int index, DIRECTION dir = DIRECTION.FREE)
        {

            // Moves an interface segment (dragtarget, so a parent object) to a given spring. 
            // it uses a button as a hook. checks if any event is targeting the same dragtarget to prevent interference.
            // If there is we just take over. Could delete and replace it as well...

            int i = uiEventStack.Count - 1;

            while (i >= 0)
            {

                Event uie = uiEventStack[i];

                if (uie.targetButton != null && uie.targetButton.GetDragTarget(uie.direction) == _button.GetDragTarget(dir))
                {

                    // the event explicitly targets the (explicit) target of the passed in button
                    //remove the event
                    uiEventStack.RemoveAt(i);
                    Log("removing event");
                }

                i--;

            }

            Log("Adding a springing event for spring, button "+_button.name);

            Event springEvent = new Event();

            springEvent.targetButton = _button;
            springEvent.target2D = _button.gameObject;
            springEvent.plane = _button.InterFace.plane;
            springEvent.action = ACTION.SINGLEDRAG;
            springEvent.isSpringing = true;
            springEvent.springIndex = index;
            springEvent.direction = dir;

            uiEventStack.Add(springEvent);

        }

        // ------------------------------------------------------------------------------------------------------------------------------------------

        void processUiEvent(Event ui)
        {

            if (ui.plane == null || ui.plane.interFace == null)
                return;

            ui.callback = "";

            UIArgs args = new UIArgs();

            args.delta = Vector3.zero;
            args.uiEvent = ui;

            InterFace interFace = ui.plane.interFace;

            switch (ui.action)
            {

                case ACTION.TAP:

                    ui.action = ACTION.SINGLEDRAG; 
                    
                    if (ui.target2D != null)
                        interFace.tap_2d(this, args);
                    else if (ui.target3D != null)
                        interFace.tap_3d(this, args);
                    else
                        interFace.tap_none(this, args);

                    break;

                case ACTION.SINGLEDRAG:

                    // If this is an orthodrag button, we need to set and lock the initial direction.
                    // We also get the appropriate targets and constraints for that direction and load them into the uievent.

                    if (ui.targetButton != null && ui.targetButton.orthoDragging && ui.direction == DIRECTION.FREE)
                    {
                        if (Mathf.Abs(ui.scaledDx) > Mathf.Abs(ui.scaledDy))
                        {
                            ui.direction = DIRECTION.HORIZONTAL;
                            Verbose("Locking to drag horizontally");
                        }
                        if (Mathf.Abs(ui.scaledDx) < Mathf.Abs(ui.scaledDy))
                        {
                            ui.direction = DIRECTION.VERTICAL;
                            Verbose("Locking to drag vertically");
                        }

                    }
              //      Verbose("singledrag");
                    args.delta = new Vector3(ui.scaledDx, ui.scaledDy, 0);

                    if (ui.target2D != null)
                        interFace.single_2d(this, args);
                    else if (ui.target3D != null)
                        interFace.single_3d(this, args);
                    else
                        interFace.single_none(this, args);

                   // interFace.AddMapping

              //      ui.isInert = false;
              //    ui.isSpringing = false;

                    break;

                case ACTION.DOUBLEDRAG:

                    args.delta = new Vector3(ui.scaledDx, ui.scaledDy, ui.scaledDd);

                    if (ui.target2D != null)
                        interFace.double_2d(this, args);
                    else if (ui.target3D != null)
                        interFace.double_3d(this, args);
                    else
                        interFace.double_none(this, args);

                    break;

                default:

                    interFace.none(this, args);

                    break;
            }


            ui.Inertia();

        }


        // ------------------------------------------------------------------------------------------------------------------------------------------


        void setRaycastActive(string name, bool value, InterFace activeInterface)
        {
            Button theButton;
            activeInterface.uiButtons.TryGetValue(name, out theButton);

            if (theButton != null)
                theButton.image.raycastTarget = value;

        }

        bool brightnessIsChanging(string name, InterFace activeInterface)
        {
            //bool result = false;
            Button theButton;
            activeInterface.uiButtons.TryGetValue(name, out theButton);

            if (theButton != null)
            {
                return theButton.BrightnessChanging();

            }

            return false;
        }

    }

}





