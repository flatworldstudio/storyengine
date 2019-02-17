namespace StoryEngine.UI
{
    public delegate void UIEventHandler(object sender, UIArgs args);

    /*!
* \brief 
* Used to create different sets of handlers for user interactions.
* 
* Add a mapping object to an interface.
* Ie. zoom for two-finger drag in 2D, rotate for one-finger drag in 3D, etc.
*/

    public class Mapping
    {

        public event UIEventHandler ux_none, ux_tap_2d, ux_tap_3d, ux_tap_none, ux_single_2d, ux_single_3d, ux_single_none, ux_double_2d, ux_double_3d, ux_double_none;
        static Mapping __empty;

        public Mapping()
        {

        }
        public static Mapping Empty
        {
            get
            {
                if (__empty == null)
                    __empty = new Mapping();

                return __empty;
            }

            set
            {

            }

        }

        public Mapping Clone()
        {
            Mapping clone = new Mapping();

            Log.Warning("clone is untested");

            clone.ux_none += this.ux_none;
            clone.ux_tap_2d += this.ux_tap_2d;
            clone.ux_tap_3d += this.ux_tap_3d;
            clone.ux_tap_none += this.ux_tap_none;
            clone.ux_single_2d += this.ux_single_2d;
            clone.ux_single_3d += this.ux_single_3d;
            clone.ux_single_none += this.ux_single_none;
            clone.ux_double_2d += this.ux_double_2d;
            clone.ux_double_3d += this.ux_double_3d;
            clone.ux_double_none += this.ux_double_none;

            return clone;
        }

        public void none(object sender, UIArgs args)
        {
            ux_none?.Invoke(sender, args);
        }

        public void tap_2d(object sender, UIArgs args)
        {
            ux_tap_2d?.Invoke(sender, args);
        }

        public void tap_3d(object sender, UIArgs args)
        {
            ux_tap_3d?.Invoke(sender, args);
        }

        public void tap_none(object sender, UIArgs args)
        {
            ux_tap_none?.Invoke(sender, args);
        }

        public void single_2d(object sender, UIArgs args)
        {
            ux_single_2d?.Invoke(sender, args);
        }

        public void single_3d(object sender, UIArgs args)
        {
            ux_single_3d?.Invoke(sender, args);
        }

        public void single_none(object sender, UIArgs args)
        {
            ux_single_none?.Invoke(sender, args);
        }

        public void double_2d(object sender, UIArgs args)
        {
            ux_double_2d?.Invoke(sender, args);
        }

        public void double_3d(object sender, UIArgs args)
        {
            ux_double_3d?.Invoke(sender, args);
        }

        public void double_none(object sender, UIArgs args)
        {
            ux_double_none?.Invoke(sender, args);
        }
    }
}