using System.Collections.Generic;
using UnityEngine;

namespace JTRP.Utility
{
	public class PropPairs : ScriptableObject
	{
		[SerializeField] public Shader dstShader;
		[SerializeField] public List<string> src;
		[SerializeField] public List<string> dst;
	}
}