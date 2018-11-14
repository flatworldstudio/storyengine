
using UnityEngine;
using UnityEngine.UI;


namespace StoryEngine.UI
{
    
    public class Responsive : MonoBehaviour

    {
        // Use this behaviour if plane layout needs to be responsive / dynamic.

        float lastWidth, lastHeight;
        Layout watchLayout;

        void Start()
        {

            lastWidth = Screen.width;
            lastHeight = Screen.height;

        }

        public void WatchLayout(Layout _layout){
            
            watchLayout=_layout;

        }

        void Update()
        {

            if (lastWidth != Screen.width || lastHeight != Screen.height)

            {
                lastWidth = Screen.width;
                lastHeight = Screen.height;

                if (watchLayout != null)
                {
                    watchLayout.UpdateLayout(lastWidth, lastHeight);
                }

            }

        }

    }

}


