using UnityEngine;

namespace StoryEngine.UI
{

    /*!
* \brief 
* Describes a single plane in a  Layout object
* 
* A plane has a size equal on the recttransform of its gameobject reference.
* If no object or recttransform is present, it assumes full screen size.
*/

    public class Plane
    {
        string ID = "Plane";

        public float x0, y0, x1, y1;

        public InterFace interFace;
        GameObject sceneObject,anchorObject;
        RectTransform rt;

        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.
        void Log(string message) => StoryEngine.Log.Message(message, ID);
        void Warning(string message) => StoryEngine.Log.Warning(message, ID);
        void Error(string message) => StoryEngine.Log.Error(message, ID);
        void Verbose(string message) => StoryEngine.Log.Message(message, ID, LOGLEVEL.VERBOSE);

        public Plane()
        {

        }

        public Plane(GameObject _object)
        {
            sceneObject = _object;

            if ((rt = sceneObject.GetComponent<RectTransform>()) == null)
                Warning("No recttransform on Plane gameobject");
        }
        public void AddOffsetRect (GameObject _object)
        {
            anchorObject = _object;
        }

        public Vector2 GetOffsetPosition(Vector2 _pos)
        {
            if (anchorObject != null)
            {
                RectTransform rect = anchorObject.GetComponent<RectTransform>();

                if (rect != null )
                {

             Vector2 relative=      rect.InverseTransformPoint(_pos) + new Vector3(rect.rect.width/2f,rect.rect.height/2f);

                    return relative;
                }
            }
            return _pos;
        }


        public float Scale
        {
            get
            {
                //lossyScale is global scale, so including any scaling along the hierarchy
                return rt == null ? 1f : 1f / rt.lossyScale.x;

            }
        }

        void GetSizeFromScene()
        {

            // Each corner provides its world space value. The returned array of 4 vertices is clockwise. 
            // It starts bottom left and rotates to top left, then top right, and finally bottom right.

            if (rt == null)
            {
                // If no refs, assume full screen (ie, the root plane)
                x0 = 0;
                x1 = Screen.width;
                y0 = 0;
                y1 = Screen.height;

            }
            else
            {
                Vector3[] corners = new Vector3[4];
                rt.GetWorldCorners(corners);

                x0 = corners[0].x;
                y0 = corners[0].y;
                x1 = corners[2].x;
                y1 = corners[2].y;

            }
        }

        public void AddInterface(InterFace _interface)
        {
            interFace = _interface;
            interFace.plane = this;

            if (rt == null)
            {
                // there was no gameobject or somehow no recttransform on it, so we set the canvas rt as ref
                rt = interFace.canvasObject.GetComponent<RectTransform>();

            }
        }

        public bool WorldCoordinateInPlane(Vector2 _point)
        {

            GetSizeFromScene();

            if (_point.x >= x0 && _point.x < x1 && _point.y >= y0 && _point.y < y1)
                return true;

            return false;

        }

    }
}