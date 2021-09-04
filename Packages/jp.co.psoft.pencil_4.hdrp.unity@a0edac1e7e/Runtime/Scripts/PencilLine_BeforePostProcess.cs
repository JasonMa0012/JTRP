using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Pencil_4.HDRP
{

    [Serializable, VolumeComponentMenu("Post-processing/Pencil+ 4/Line (Before Post Process)")]
    public sealed class PencilLine_BeforePostProcess : PencilLineBase
    {
        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.BeforePostProcess;
    }
}