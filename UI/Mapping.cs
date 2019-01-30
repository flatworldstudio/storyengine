namespace StoryEngine.UI
{
    /*!
* \brief 
* Used to create different sets of eventhandlers for user events.
* 
* Ie. zoom for two-finger drag in 2D, rotate for one-finger drag in 3D, etc.
*/

    public class Mapping
    {



        public event UIEventHandler ux_none, ux_tap_2d, ux_tap_3d, ux_tap_none, ux_single_2d, ux_single_3d, ux_single_none, ux_double_2d, ux_double_3d, ux_double_none;


        public Mapping()
        {

        }

        public void none(object sender, UIArgs args)
        {
            if (ux_none != null)
                ux_none(sender, args);

        }


        public void tap_2d(object sender, UIArgs args)
        {
            if (ux_tap_2d != null)
                ux_tap_2d(sender, args);

        }

        public void tap_3d(object sender, UIArgs args)
        {
            if (ux_tap_3d != null)
                ux_tap_3d(sender, args);

        }

        public void tap_none(object sender, UIArgs args)
        {

            if (ux_tap_none != null)
                ux_tap_none(sender, args);

        }

        public void single_2d(object sender, UIArgs args)
        {
            if (ux_single_2d != null)
                ux_single_2d(sender, args);

        }

        public void single_3d(object sender, UIArgs args)
        {
            if (ux_single_3d != null)
                ux_single_3d(sender, args);

        }

        public void single_none(object sender, UIArgs args)
        {
            if (ux_single_none != null)
                ux_single_none(sender, args);

        }

        public void double_2d(object sender, UIArgs args)
        {
            if (ux_double_2d != null)
                ux_double_2d(sender, args);

        }

        public void double_3d(object sender, UIArgs args)
        {
            if (ux_double_3d != null)
                ux_double_3d(sender, args);

        }

        public void double_none(object sender, UIArgs args)
        {
            if (ux_double_none != null)
                ux_double_none(sender, args);

        }



    }
}