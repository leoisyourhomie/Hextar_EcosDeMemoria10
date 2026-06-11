namespace Expiria3DSpace
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class ToolbarSearchBar
    {
        public static GUIStyle SearchBarStyle
        {
            get
            {
                GUIStyle searchSt = GUI.skin.FindStyle("ToolbarSeachTextField");

                if (searchSt == null)
                    searchSt = GUI.skin.FindStyle("ToolbarSearchTextField");

                return searchSt;
            }
        }

        public static GUIStyle CancelBtnStyle
        {
            get
            {
                GUIStyle canlceBtnSt = GUI.skin.FindStyle("ToolbarSeachCancelButton");

                if (canlceBtnSt == null)
                    canlceBtnSt = GUI.skin.FindStyle("ToolbarSearchCancelButton");

                return canlceBtnSt;
            }
        }
    }
}