//using StoryEngine;
using UnityEngine;

namespace StoryEngine.UI
{

    /*!
* \brief 
* Describes a single plane in a (binairy tree) Layout object
* 
* A plane can hold a reference to a single Interface
*/

    public class Plane
    {

        // Class to describe a single plane in a (binairy tree) layout.


        public float x0, y0, x1, y1;
        public float splitValue;
        public DIRECTION splitDirection;

        public Plane parent, root;
        public Plane[] children;
        public string address;

        //public Canvas canvas;
        public InterFace interFace;
        //public GameObject gameObject;
        //     public Vector2 screenOffset = Vector2.zero;


        public Plane()
        {
        }

        public Plane(Plane _parent)
        {
            parent = _parent;
            address = parent.address + "x";
            root = parent.root;
        }

        public Plane(float _x0, float _x1, float _y0, float _y1)
        {
            // Root only.
            x0 = _x0;
            x1 = _x1;
            y0 = _y0;
            y1 = _y1;
            address = "r";
            root = this;

        }

        public void Resize()
        {

            if (interFace != null)
                interFace.Resize();

        }

        public Plane SearchAddress(string _address)
        {

            Plane returnPlane = null;

            if (children != null)
            {

                returnPlane = children[0].SearchAddress(_address);
                if (returnPlane != null)
                    return returnPlane;

                returnPlane = children[1].SearchAddress(_address);

                if (returnPlane != null)
                    return returnPlane;


            }
            else
            {

                if (address == _address)
                    returnPlane = this;

            }

            return returnPlane;


        }

        // Plane RecursiveSearch (Plane _plane, string _address){

        //    Plane 

        //}

        public void AddInterface(InterFace _interface)
        {

            interFace = _interface;
            interFace.plane = this;

        }

        void AddChildren()
        {

            if (children == null)
            {
                children = new Plane[2];
                children[0] = new Plane(this);
                children[0].address = children[0].address.Replace('x', '0');

                children[1] = new Plane(this);
                children[1].address = children[1].address.Replace('x', '1');

            }
        }

        public bool PointInPlane(Vector2 _point)
        {
            // If the plane doesn't have dimensions, always return true.

            if (x0 == 0 && x1 == 0 && y0 == 0 && y1 == 0)
                return true;

            if (_point.x >= x0 && _point.x < x1 && _point.y >= y0 && _point.y < y1)
                return true;
            else
                return false;

        }

        public bool Split(float _value, DIRECTION _direction)
        {


            if (children != null && splitDirection != _direction)
            {

                // Already split, but in different direction. This shouldn't have been called.

                Debug.LogError("UI Plane already split but in different direction.");

                return false;
            }


            switch (_direction)
            {

                case DIRECTION.VERTICAL:

                    if (_value > x0 && _value < x1)
                    {

                        AddChildren();

                        splitValue = _value;
                        splitDirection = _direction;

                        children[0].x0 = x0;
                        children[0].x1 = _value;
                        children[0].y0 = y0;
                        children[0].y1 = y1;

                        children[1].x0 = _value;
                        children[1].x1 = x1;
                        children[1].y0 = y0;
                        children[1].y1 = y1;

                        return true;
                    }

                    break;

                case DIRECTION.HORIZONTAL:

                    AddChildren();

                    splitValue = _value;
                    splitDirection = _direction;

                    children[0].x0 = x0;
                    children[0].x1 = x1;
                    children[0].y0 = y0;
                    children[0].y1 = _value;

                    children[1].x0 = x0;
                    children[1].x1 = x1;
                    children[1].y0 = _value;
                    children[1].y1 = y1;

                    break;

                default:
                    break;

            }

            return false;

        }




    }
}