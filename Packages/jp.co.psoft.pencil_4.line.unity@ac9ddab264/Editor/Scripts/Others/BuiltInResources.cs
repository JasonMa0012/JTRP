using UnityEngine;
using UnityEditor;

namespace Pcl4Editor
{
    public static class BuiltInResources
    {

        public static Texture PencilNodeIconResource
        {
            get
            {
                return EditorGUIUtility.IconContent("GameObject Icon").image;
            }
        }

        public static GUIStyle VisibleToggleButtonStyle
        {
            get
            {
                return EditorStyles.miniButtonLeft;
            }
            
        }


        public static GUIStyle HiddenToggleButtonStyle
        {
            get
            {
                return EditorStyles.miniButtonRight;
            }
        }
        
    }
}
