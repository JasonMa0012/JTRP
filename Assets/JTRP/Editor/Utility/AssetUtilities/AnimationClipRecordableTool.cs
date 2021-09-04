using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;
using UnityEditor.Timeline.Actions;

namespace UnityEditor.Timeline
{
	[MenuEntry("Convert To Recordable Clip Track", MenuPriority.CustomTrackActionSection.customTrackAction), UsedImplicitly]
	class ConvertToClipModeAction : TrackAction
	{
		public override bool Execute(IEnumerable<TrackAsset> tracks)
		{
			foreach (var animTrack in tracks.OfType<AnimationTrack>())
			{

				ConvertToRecordableClip(animTrack);
			}

			TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);

			return true;
		}

		public override ActionValidity Validate(IEnumerable<TrackAsset> tracks)
		{
			if (tracks.Any(t => !t.GetType().IsAssignableFrom(typeof(AnimationTrack))))
				return ActionValidity.NotApplicable;

			if (tracks.Any(t => t.lockedInHierarchy))
				return ActionValidity.Invalid;

			if (tracks.OfType<AnimationTrack>().All(a => a.GetClips().All(a => !a.recordable)))
				return ActionValidity.Valid;

			return ActionValidity.NotApplicable;
		}

		private void ConvertToRecordableClip(AnimationTrack track)
		{
			if (track == null || !track.hasClips) return;

			UndoExtensions.RegisterTrack(track, L10n.Tr("ConvertToRecordableClip"));

			var clip = track.GetClips().First();
			var delta = (float)clip.start;
			var duration = clip.duration;

			var animationAsset = clip.asset as AnimationPlayableAsset;
			if (animationAsset == null) return;
			var animationClipSource = animationAsset.clip;
			var animationName = animationClipSource.name;

			foreach (var c in track.GetClips())
				track.DeleteClip(c);

			var recordableClip = track.CreateRecordableClip(animationName);

			recordableClip.start = delta;
			recordableClip.duration = duration;
			var newAnimationClip = (recordableClip.asset as AnimationPlayableAsset).clip;


			newAnimationClip.name = animationName;
			var setting = AnimationUtility.GetAnimationClipSettings(animationClipSource);
			AnimationUtility.SetAnimationClipSettings(newAnimationClip, setting);
			newAnimationClip.frameRate = animationClipSource.frameRate;
			EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(animationClipSource);
			for (int i = 0; i < curveBindings.Length; i++)
			{
				AnimationUtility.SetEditorCurve(newAnimationClip, curveBindings[i], 
					AnimationUtility.GetEditorCurve(animationClipSource, curveBindings[i]));
			}


			EditorUtility.SetDirty(track);
		}
	}

}
