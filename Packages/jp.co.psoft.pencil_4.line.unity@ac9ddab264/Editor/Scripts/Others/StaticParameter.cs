using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Pcl4Editor
{
	public class StaticParameter : ScriptableSingleton<StaticParameter>
	{
		public string BridgeLastOpenedPath;
        public bool IsLineFunctionsFoldoutOpen;
        public bool IsDoubleSidedMaterialFoldoutOpen;
        public bool IsIgnoreObjectFoldoutOpen;
        public bool IsLineGroupListFoldoutOpen;
    }
}
