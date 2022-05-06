using System.Collections.Generic;
using UnityEngine;

namespace JTRP.Editor.Utilities
{
	public class PropPairs : ScriptableObject
	{
		[SerializeField] public Shader dstShader;
		[SerializeField] public List<string> src;
		[SerializeField] public List<string> dst;
	}
}