using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class ReorderableLayoutList
{
	const float DRAG_WIDTH = 20;

	int bufferedDraggedElement = -1;
	int draggedElement = -1;
	Vector2 mouseDragOrigin;
	float mouseDragOffset;
	Rect[] elementRects;

	float yOffset;
	float lastSwappedHeight;    //last swapped element height
	float draggedHeight;        //currently dragged element height

	int swappedElementAnimation = -1;
	float swappedElementPosOffset = 0f;
	float swappedElementOffset = 0f;
	float swappedElementAnimationTime = 0f;
	const float kSwappedElementDuration = 0.2f;
	bool pendingReorderChange;

	public delegate void NeedRepaint();
	public static event NeedRepaint OnNeedRepaint;

	private static void Repaint()
	{
		if(OnNeedRepaint != null)
			OnNeedRepaint();
	}

	public bool DoLayoutList(Action<int, float> DrawElement, IList list, float dragWidth = DRAG_WIDTH)
	{
		return DoLayoutList(DrawElement, list, new RectOffset(0, 0, 0, 0), dragWidth);
	}

	Vector2 mousePosition;
	static GUIStyle _dragHandle;
	static GUIStyle dragHandle
	{
		get
		{
			if(_dragHandle == null)
			{
				_dragHandle = "RL DragHandle";
			}
			return _dragHandle;
		}
	}
	static readonly int hash_dragHandle = "RL DragHandle".GetHashCode();

	// Returns 'true' when elements have been reordered
	public bool DoLayoutList(Action<int, float> DrawElement, IList list, RectOffset padding, float dragWidth = DRAG_WIDTH)
	{
		bool canBeDragged = list != null && list.Count > 1;

		if(Event.current.type != EventType.Layout)
			mousePosition = Event.current.mousePosition;

		var guiColor = GUI.color;

		if(elementRects == null || elementRects.Length != list.Count)
		{
			elementRects = new Rect[list.Count];
		}

		//lambda function so that we can reorder drawing when one is selected
		Action<int> DrawListItem = i =>
		{
			float mouseDelta = 0;

			if(draggedElement == i)
			{
				//offset ui drawing based on mouse delta
				mouseDelta = mouseDragOrigin.y + mouseDragOffset - mousePosition.y;

				//block at the top/bottom of the ui
				float yMax = mouseDragOffset;
				float yMin = 0;
				for(var j = 0; j < list.Count; j++)
				{
					if(j < i)
						yMax += elementRects[j].height;
					else if(j > i)
						yMin -= elementRects[j].height;
				}
				mouseDelta = Mathf.Clamp(mouseDelta, yMin, yMax);

				//negative space to offset the ui freely
				GUILayout.Space(-mouseDelta);
			}
			else if(swappedElementAnimation == i)
			{
				//swapped element animation: slide towards target position
				float delta = Mathf.Clamp01((Time.realtimeSinceStartup - swappedElementAnimationTime) / kSwappedElementDuration);
				//simple easing animation (ease out quad)
				System.Func<float, float> animationEasing = (x) => { return -1f * x * (x-2); };
				swappedElementOffset = Mathf.Lerp(swappedElementPosOffset, 0, animationEasing(delta));
				GUILayout.Space(-swappedElementOffset);
			}

			//get dragging rect
			var dragRect = EditorGUILayout.BeginVertical();
			{
				if (draggedElement == i)
				{
					var c = EditorGUIUtility.isProSkin ? 0.2f : 0.75f;
					GUI.color *=  new Color(c, c, c, 0.85f);
					EditorGUI.DrawRect(dragRect, Color.white);
					GUI.color = guiColor;
				}

				//build array of draggable rectangle zones
				if (draggedElement < 0 && Event.current.type == EventType.Repaint)
				{
					elementRects[i] = dragRect;
				}

				dragRect.xMin += padding.left;
				dragRect.width = dragWidth - 2;
				dragRect.xMax -= padding.right;
				dragRect.yMin += padding.top;
				dragRect.yMax -= padding.bottom;

				//dragging zone UI
				var drawRect = dragRect;
				drawRect.yMin += 7;
				drawRect.yMax -= 4;

				//draw drag handle icons
				if (Event.current.type == EventType.Repaint)
				{
					//ui color to indicate we are dragging this implementation
					if (!canBeDragged)
					{
						GUI.color *= new Color(1, 1, 1, .25f);
					}
					if (draggedElement == i)
					{
						GUI.color *= new Color(.8f, .8f, .8f);
					}

					const float dragHeight = 6;
					var count = Mathf.FloorToInt(drawRect.height / dragHeight);
					var margin = drawRect.height - count*dragHeight;
					for (var j = 0; j < count; j++)
					{
						var dragIconRect = drawRect;
						dragIconRect.xMin += 5;
						dragIconRect.xMax -= 5;
						dragIconRect.height = dragHeight;
						dragIconRect.y = drawRect.y + (j*dragHeight) + margin/2f;
						dragHandle.Draw(dragIconRect, GUIContent.none, hash_dragHandle);
					}

					GUI.color = guiColor;
				}

				//change cursor when over drag zone
				if (canBeDragged)
				{
					if (draggedElement > -1)
						EditorGUIUtility.AddCursorRect(dragRect, MouseCursor.Pan);
					else
						EditorGUIUtility.AddCursorRect(dragRect, MouseCursor.MoveArrow);
				}

				//callback to GUI drawing, including margin
				DrawElement(i, dragWidth + 2);
			}
			EditorGUILayout.EndVertical();

			//listen to mouse drag events
			if (canBeDragged)
			{
				if (Event.current.type == EventType.MouseDown && dragRect.Contains(mousePosition))
				{
					bufferedDraggedElement = i;
					mouseDragOrigin = mousePosition;
					lastSwappedHeight = elementRects[i].height;
					draggedHeight = elementRects[i].height;
					GUIUtility.keyboardControl = 0;
					GUIUtility.hotControl = 0;
					Repaint();
				}
			}

			if(draggedElement == i)
			{
				//compensate offset
				GUILayout.Space(mouseDelta);
			}
			else if(swappedElementAnimation == i)
			{
				//swapped element animation: slide towards target position
				GUILayout.Space(swappedElementOffset);
				Repaint();
			}
		};

		// catch stop dragging events now before they could be used
		bool stopDrag = false;
		if (Event.current.type == EventType.MouseUp || Event.current.rawType == EventType.MouseUp)
		{
			stopDrag = true;
		}

		for (var i = 0; i < list.Count; i++)
		{
			if(draggedElement == i)
			{
				//leave space for dragged imp: will be drawn last
				GUILayout.Space(draggedHeight);
				if (Event.current.type == EventType.Layout)
				{
					yOffset = 0;
				}
			}
			else
			{
				DrawListItem(i);

				if (Event.current.type == EventType.Layout)
				{
					yOffset += elementRects[i].height;
				}
			}
		}

		//draw the dragged imp last so that it is in front of the other ones
		if(draggedElement > -1)
		{
			GUILayout.Space(-(yOffset + draggedHeight));
			DrawListItem(draggedElement);
			GUILayout.Space(yOffset);
		}

		//need to apply the dragged imp after the loop to prevent gui layout mismatch errors
		if(Event.current.isMouse)
		{
			draggedElement = bufferedDraggedElement;
		}

		//mouse drag event: swap the implementations if mouse is inside a particular imp rect
		if(draggedElement > -1 && Event.current.type == EventType.MouseDrag)
		{
			//repaint window
			Repaint();

			for(var i = 0; i < elementRects.Length; i++)
			{
				if(elementRects[i].Contains(mousePosition) && draggedElement != i)
				{
					//swap the list items
					var tmp = list[i];
					list[i] = list[draggedElement];
					list[draggedElement] = tmp;

					//compensate y diff for mouseOrigin
					var diff = elementRects[i].y - elementRects[draggedElement].y;
					mouseDragOrigin.y += diff;

					//compensate size difference between swapped implementations
					var heightDiff = lastSwappedHeight - elementRects[i].height;
					lastSwappedHeight = elementRects[i].height;
					mouseDragOffset -= heightDiff;

					//set the animated swapped element
					swappedElementAnimation = draggedElement;
					swappedElementAnimationTime = Time.realtimeSinceStartup;
					//going up
					if((draggedElement > i))
						swappedElementPosOffset = -mouseDragOffset - diff;
					//going down
					else
						swappedElementPosOffset = mouseDragOffset - elementRects[i].height;

					//swap current dragged imp
					bufferedDraggedElement = i;
					draggedElement = i;

					pendingReorderChange = true;
				}
			}
		}

		// stop dragging : needs to be at the end to prevent GUI mismatch errors
		if(stopDrag)
		{
			bufferedDraggedElement = -1;
			draggedElement = -1;
			mouseDragOffset = 0f;
			Repaint();

			if(pendingReorderChange)
			{
				pendingReorderChange = false;
				return true;
			}
		}

		return false;
	}
}
