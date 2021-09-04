// Toony Colors Pro 2
// (c) 2014-2020 Jean Moreno

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ToonyColorsPro.Utilities;

//Extended GUILayout for Shader Generator 2

namespace ToonyColorsPro
{
	namespace ShaderGenerator
	{
		static class SGUILayout
		{
			public static float Indent = 0f;

			//--------------------------------------------------------------------------------------------------------------------------------
			// UI Constants

			public static class Constants
			{
				public const string screenSpaceUVLabel = "Screen Space";
				public const string worldPosUVLabel = "World Position";
				public const string shaderPropertyUVLabel = "Other Shader Property";

				public static readonly string[] DefaultTextureValues =
				{
					"white",
					"black",
					"gray",
					"bump"
				};

				public static readonly string[] UvChannelOptions =
				{
					"texcoord0",
					"texcoord1",
					"texcoord2",
					"texcoord3",
					screenSpaceUVLabel,
					worldPosUVLabel,
					shaderPropertyUVLabel
				};

				public static readonly string[] UvChannelOptionsVertex =
				{
					"texcoord0",
					"texcoord1",
					"texcoord2",
					"texcoord3",
					worldPosUVLabel,
					shaderPropertyUVLabel
				};

				public static string[] LockedUvChannelOptions =
				{
					"computed in shader"
				};

				public static readonly string[] UvAnimationOptions =
				{
					"Off",
					"Scrolling",
					"Random Offset"
				};
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			// GUIStyles

			internal static class Styles
			{
				static GUIStyle _GrayLabel;
				internal static GUIStyle GrayLabel
				{
					get
					{
						if(_GrayLabel == null)
						{
							var color = EditorGUIUtility.isProSkin ? new Color32(130, 130, 130, 255) : new Color32(100, 100, 100, 255);
							_GrayLabel = new GUIStyle(EditorStyles.label);
							_GrayLabel.normal.textColor = color;
							_GrayLabel.active.textColor = color;
							_GrayLabel.focused.textColor = color;
							_GrayLabel.hover.textColor = color;
						}
						return _GrayLabel;
					}
				}

				static GUIStyle _OrangeBoldLabel;
				internal static GUIStyle OrangeBoldLabel
				{
					get
					{
						if(_OrangeBoldLabel == null)
						{
							var color = EditorGUIUtility.isProSkin ? new Color32(250, 130, 0, 255) : new Color32(220, 100, 0, 255);
							_OrangeBoldLabel = new GUIStyle(EditorStyles.label);
							_OrangeBoldLabel.normal.textColor = color;
							_OrangeBoldLabel.active.textColor = color;
							_OrangeBoldLabel.focused.textColor = color;
							_OrangeBoldLabel.hover.textColor = color;
							_OrangeBoldLabel.fontStyle = FontStyle.Bold;
						}
						return _OrangeBoldLabel;
					}
				}

				static GUIStyle _OrangeHeader;
				internal static GUIStyle OrangeHeader
				{
					get
					{
						if(_OrangeHeader == null)
						{
							_OrangeHeader = new GUIStyle(OrangeBoldLabel);
							_OrangeHeader.fontSize = 16;
						}
						return _OrangeHeader;
					}
				}

				static GUIStyle _GrayBoldLabel;
				internal static GUIStyle GrayBoldLabel
				{
					get
					{
						if(_GrayBoldLabel == null)
						{
							_GrayBoldLabel = new GUIStyle(GrayLabel);
							_GrayBoldLabel.fontStyle = FontStyle.Bold;
						}
						return _GrayBoldLabel;
					}
				}

				static GUIStyle _GrayMiniLabel;
				internal static GUIStyle GrayMiniLabel
				{
					get
					{
						if(_GrayMiniLabel == null)
						{
							_GrayMiniLabel = new GUIStyle("ShurikenLabel")
							{
								fixedHeight = 13,
								padding = new RectOffset(2, 4, 0, 0),
								fontSize = shurikenFontSize
							};
							var c = EditorGUIUtility.isProSkin ? .7f : .3f;
							_GrayMiniLabel.normal.textColor = new Color(c, c, c, 1.0f);
						}
						return _GrayMiniLabel;
					}
				}

				static GUIStyle _GrayMiniLabelWrap;
				internal static GUIStyle GrayMiniLabelWrap
				{
					get
					{
						if (_GrayMiniLabelWrap == null)
						{
							_GrayMiniLabelWrap = new GUIStyle(GrayMiniLabel)
							{
								wordWrap = true,
								fixedHeight = 0,
								stretchHeight = false,
								stretchWidth = false
							};
						}
						return _GrayMiniLabelWrap;
					}
				}

				static GUIStyle _GrayMiniLabelWrapHighlighted;
				internal static GUIStyle GrayMiniLabelWrapHighlighted
				{
					get
					{
						if (_GrayMiniLabelWrapHighlighted == null)
						{
							_GrayMiniLabelWrapHighlighted = new GUIStyle(GrayMiniLabelWrap)
							{
								fontStyle = FontStyle.Bold
							};
							var textColor = EditorGUIUtility.isProSkin ? new Color(0.0f, 0.574f, 0.488f) : new Color(0.03f, 0.46f, 0.4f);
							_GrayMiniLabelWrapHighlighted.normal.textColor = textColor;
						}
						return _GrayMiniLabelWrapHighlighted;
					}
				}


				static GUIStyle _GrayMiniBoldLabel;
				internal static GUIStyle GrayMiniBoldLabel
				{
					get
					{
						if(_GrayMiniBoldLabel == null)
						{
							_GrayMiniBoldLabel = new GUIStyle(GrayMiniLabel)
							{
								fontStyle = FontStyle.Bold
							};
						}
						return _GrayMiniBoldLabel;
					}
				}

				static GUIStyle _GrayMiniLabelHighlighted;
				internal static GUIStyle GrayMiniLabelHighlighted
				{
					get
					{
						if (_GrayMiniLabelHighlighted == null)
						{
							_GrayMiniLabelHighlighted = new GUIStyle(GrayMiniLabel)
							{
								fontStyle = FontStyle.Bold
							};

							var textColor = EditorGUIUtility.isProSkin ? new Color(0.0f, 0.574f, 0.488f) : new Color(0.03f, 0.46f, 0.4f);
							_GrayMiniLabelHighlighted.normal.textColor = textColor;
						}
						return _GrayMiniLabelHighlighted;
					}
				}

				private static GUIStyle _GrayMiniFoldout;
				public static GUIStyle GrayMiniFoldout
				{
					get
					{
						if (_GrayMiniFoldout == null)
						{
							_GrayMiniFoldout = new GUIStyle(EditorStyles.foldout);

							var grayMiniLabel = GrayMiniLabel;
							_GrayMiniFoldout.alignment = grayMiniLabel.alignment;
							_GrayMiniFoldout.font = grayMiniLabel.font;
							_GrayMiniFoldout.fontStyle = grayMiniLabel.fontStyle;
							_GrayMiniFoldout.margin = grayMiniLabel.margin;
							_GrayMiniFoldout.padding = new RectOffset(16, 0, 0, 0);
							_GrayMiniFoldout.richText = grayMiniLabel.richText;
							_GrayMiniFoldout.stretchHeight = grayMiniLabel.stretchHeight;
							_GrayMiniFoldout.stretchWidth = grayMiniLabel.stretchWidth;
							_GrayMiniFoldout.fixedHeight = 0;
							_GrayMiniFoldout.fixedWidth = 0;

							_GrayMiniFoldout.normal.textColor = grayMiniLabel.normal.textColor;
							_GrayMiniFoldout.onNormal.textColor = grayMiniLabel.normal.textColor;
							_GrayMiniFoldout.focused.textColor = grayMiniLabel.normal.textColor;
							_GrayMiniFoldout.onFocused.textColor = grayMiniLabel.normal.textColor;
							_GrayMiniFoldout.hover.textColor = grayMiniLabel.normal.textColor;
							_GrayMiniFoldout.onHover.textColor = grayMiniLabel.normal.textColor;

							var gray = EditorGUIUtility.isProSkin ? 0.4f : 0.45f;
							var textColorActive = new Color(gray, gray, gray);
							_GrayMiniFoldout.active.textColor = textColorActive;
							_GrayMiniFoldout.onActive.textColor = textColorActive;

							_GrayMiniFoldout.normal.background = TCP2_GUI.GetCustomTexture("TCP2_FoldoutArrowRight");
							_GrayMiniFoldout.active.background = _GrayMiniFoldout.normal.background;
							_GrayMiniFoldout.focused.background = _GrayMiniFoldout.normal.background;
							_GrayMiniFoldout.hover.background = _GrayMiniFoldout.normal.background;

							_GrayMiniFoldout.onNormal.background = TCP2_GUI.GetCustomTexture("TCP2_FoldoutArrowDown");
							_GrayMiniFoldout.onActive.background = _GrayMiniFoldout.onNormal.background;
							_GrayMiniFoldout.onFocused.background = _GrayMiniFoldout.onNormal.background;
							_GrayMiniFoldout.onHover.background = _GrayMiniFoldout.onNormal.background;

						}
						return _GrayMiniFoldout;
					}
				}

				static GUIStyle _GrayMiniFoldoutHighlighted;
				internal static GUIStyle GrayMiniFoldoutHighlighted
				{
					get
					{
						if (_GrayMiniFoldoutHighlighted == null)
						{
							_GrayMiniFoldoutHighlighted = new GUIStyle(GrayMiniFoldout)
							{
								fontStyle = FontStyle.Bold,
							};

							var textColor = EditorGUIUtility.isProSkin ? new Color(0.0f, 0.574f, 0.488f) : new Color(0.03f, 0.46f, 0.4f);
							_GrayMiniFoldoutHighlighted.normal.textColor = textColor;
							_GrayMiniFoldoutHighlighted.active.textColor = textColor;
							_GrayMiniFoldoutHighlighted.focused.textColor = textColor;
							_GrayMiniFoldoutHighlighted.hover.textColor = textColor;
							_GrayMiniFoldoutHighlighted.onNormal.textColor = textColor;
							_GrayMiniFoldoutHighlighted.onActive.textColor = textColor;
							_GrayMiniFoldoutHighlighted.onFocused.textColor = textColor;
							_GrayMiniFoldoutHighlighted.onHover.textColor = textColor;
						}
						return _GrayMiniFoldoutHighlighted;
					}
				}

				static GUIStyle _GrayInlineLabel;
				internal static GUIStyle GrayInlineLabel
				{
					get
					{
						if(_GrayInlineLabel == null)
						{
							_GrayInlineLabel = new GUIStyle(GrayLabel);
						}
						return _GrayInlineLabel;
					}
				}

				static GUIStyle _LineStyle;
				internal static GUIStyle LineStyle
				{
					get
					{
						if(_LineStyle == null)
						{
							_LineStyle = new GUIStyle();
							_LineStyle.normal.background = EditorGUIUtility.whiteTexture;
							_LineStyle.stretchWidth = true;
						}

						return _LineStyle;
					}
				}

				// ----------------------------------------------------------------
				// SHURIKEN STYLES OVERRIDES

				const int shurikenFontSize = 10;

				static GUIStyle _ShurikenValue;
				internal static GUIStyle ShurikenValue
				{
					get
					{
						if (_ShurikenValue == null)
						{
							_ShurikenValue = new GUIStyle("ShurikenValue")
							{
								fontSize = shurikenFontSize
							};
						}
						return _ShurikenValue;
					}
				}

				static GUIStyle _ShurikenPopup;
				internal static GUIStyle ShurikenPopup
				{
					get
					{
						if (_ShurikenPopup == null)
						{
							_ShurikenPopup = new GUIStyle("ShurikenPopup")
							{
								fontSize = shurikenFontSize
							};
						}
						return _ShurikenPopup;
					}
				}

				static GUIStyle _ShurikenToggle;
				internal static GUIStyle ShurikenToggle
				{
					get
					{
						if (_ShurikenToggle == null)
						{
							_ShurikenToggle = new GUIStyle("ShurikenToggle")
							{
								fontSize = shurikenFontSize
							};
						}
						return _ShurikenToggle;
					}
				}

				static GUIStyle _ShurikenTextArea;
				internal static GUIStyle ShurikenTextArea
				{
					get
					{
						if (_ShurikenTextArea == null)
						{
							_ShurikenTextArea = new GUIStyle(ShurikenValue)
							{
								fixedHeight = 0,
								alignment = TextAnchor.UpperLeft
							};
						}
						return _ShurikenTextArea;
					}
				}

				static GUIStyle _ShurikenObjectField;
				internal static GUIStyle ShurikenObjectField
				{
					get
					{
						if (_ShurikenObjectField == null)
						{
							_ShurikenObjectField = new GUIStyle(EditorStyles.objectField)
							{
								fixedHeight = 13,
								fontSize = shurikenFontSize
							};
						}
						return _ShurikenObjectField;
					}
				}
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			// GUILayout-like Methods

			static string RGBAOptions = "RGBA";
			public static char RGBASelector(char currentChannel)
			{
				return GenericSelector(RGBAOptions, currentChannel);
			}
			public static string RGBASelector(string currentChannel)
			{
				return RGBASelector(currentChannel[0]).ToString();
			}

			static string XYZWOptions = "XYZW";
			public static char XYZWSelector(char currentChannel)
			{
				return GenericSelector(XYZWOptions, currentChannel);
			}
			public static string XYZWSelector(string currentChannel)
			{
				return XYZWSelector(currentChannel[0]).ToString();
			}

			static string XYZOptions = "XYZ";
			public static char XYZSelector(char currentChannel)
			{
				return GenericSelector(XYZOptions, currentChannel);
			}
			public static string XYZSelector(string currentChannel)
			{
				return XYZSelector(currentChannel[0]).ToString();
			}

			public static string GenericSelector(string options, string current, float buttonWidth = 25)
			{
				return GenericSelector(options, current[0], buttonWidth).ToString();
			}
			public static char GenericSelector(string options, char current, float buttonWidth = 25)
			{
				var upperCurrent = char.ToUpperInvariant(current);
				var selected = options.IndexOf(upperCurrent);
				if(selected < 0) selected = 0;

				var w = buttonWidth;
				for(var i = 0; i < options.Length; i++)
				{
					var rect = GUILayoutUtility.GetRect(GUIContent.none, TCP2_GUI.ShurikenMiniButton, GUILayout.Height(15), GUILayout.Width(w));
					rect.height = 12;
					rect.y -= 1; //small hack to align with the shuriken ui components

					//button style
					var style = TCP2_GUI.ShurikenMiniButton;
					if(options.Length == 2)
						style = (i == 0) ? TCP2_GUI.ShurikenMiniButtonLeft : TCP2_GUI.ShurikenMiniButtonRight;
					else if(options.Length > 1)
						style = (i == 0) ? TCP2_GUI.ShurikenMiniButtonLeft : (i == (options.Length-1) ? TCP2_GUI.ShurikenMiniButtonRight : TCP2_GUI.ShurikenMiniButtonMid);

					if(GUI.Toggle(rect, selected == i, options[i].ToString(), style))
						selected = i;
				}
				return options[selected];
			}

			public static string RGBASwizzle(string selected, int channelsCount)
			{
				return GenericSwizzle(selected, channelsCount, "RGBA");
			}

			public static string XYZWSwizzle(string selected, int channelsCount)
			{
				return GenericSwizzle(selected, channelsCount, "XYZW");
			}

			public static string XYZSwizzle(string selected, int channelsCount)
			{
				return GenericSwizzle(selected, channelsCount, "XYZ");
			}

			public static string GenericSwizzle(string selected, int channelsCount, string options, float width = 50, bool showAvailableChannels = true)
			{
				EditorGUI.BeginChangeCheck();
				var newSelected = EditorGUILayout.DelayedTextField(selected, Styles.ShurikenValue, GUILayout.Width(width));
				if(EditorGUI.EndChangeCheck())
				{
					// not enough characters
					if (newSelected.Length < channelsCount)
					{
						return selected;
					}

					// remove extra characters
					if (newSelected.Length > channelsCount)
					{
						newSelected = newSelected.Substring(0, channelsCount);
					}

					newSelected = newSelected.ToUpperInvariant();
					foreach(var c in newSelected)
					{
						for(var i = 0; i < options.Length; i++)
							if(!options.Contains(c.ToString()))
							{
								return selected;
							}
					}
				}

				if (showAvailableChannels)
				{
					GUILayout.Space(4);
					GUILayout.Label(string.Format("(available channels: {0})", options), Styles.GrayMiniLabel);
				}

				return newSelected.ToUpperInvariant();
			}

			public static bool Foldout(bool foldout, string label, string tooltip = null, bool highlighted = false)
			{
				return Foldout(foldout, TCP2_GUI.TempContent(label, tooltip), highlighted);
			}
			public static bool Foldout(bool foldout, string label, bool highlighted)
			{
				return Foldout(foldout, TCP2_GUI.TempContent(label), highlighted);
			}

			public static bool Foldout(bool foldout, GUIContent label, bool highlighted = false, float width = 130)
			{
				GUILayout.Space(Indent);
				var rect = GUILayoutUtility.GetRect(label, highlighted ? Styles.GrayMiniFoldoutHighlighted : Styles.GrayMiniFoldout, GUILayout.Height(13), GUILayout.Width(width));
				return EditorGUI.Foldout(rect, foldout, label, true, highlighted ? Styles.GrayMiniFoldoutHighlighted : Styles.GrayMiniFoldout);
			}

			public static void InlineLabel(string label, string tooltip = null, bool highlight = false)
			{
				InlineLabel(TCP2_GUI.TempContent(label, tooltip), highlight);
			}
			public static void InlineLabel(string label, bool highlight)
			{
				InlineLabel(TCP2_GUI.TempContent(label), highlight);
			}
			public static void InlineLabel(GUIContent label, bool highlight = false, float width = 130)
			{
				GUILayout.Space(Indent);
				var rect = GUILayoutUtility.GetRect(label, highlight ? Styles.GrayMiniLabelHighlighted : Styles.GrayMiniLabel, GUILayout.Height(13), GUILayout.Width(width));
				GUI.Label(rect, label, highlight ? Styles.GrayMiniLabelHighlighted : Styles.GrayMiniLabel);
			}

			public static void InlineHeader(string label, string tooltip = null)
			{
				InlineHeader(TCP2_GUI.TempContent(label, tooltip));
			}
			public static void InlineHeader(GUIContent label)
			{
				GUILayout.Space(Indent);
				var rect = GUILayoutUtility.GetRect(label, Styles.GrayMiniBoldLabel);
				rect.y -= 2;
				GUI.Label(rect, label, Styles.GrayMiniBoldLabel);
			}

			//Property fields for Shader Property: UI is harmonized and easy to update
			public static Enum EnumPopup(Enum enm) { return EditorGUILayout.EnumPopup(enm, Styles.ShurikenPopup, GUILayout.MinWidth(248)); }
			public static int Popup(int index, string[] values) { return EditorGUILayout.Popup(index, values, Styles.ShurikenPopup, GUILayout.MinWidth(248)); }
			public static string TextField(string str, bool delayed = false)
			{
				if (delayed)
				{
					return EditorGUILayout.DelayedTextField(GUIContent.none, str, Styles.ShurikenValue, GUILayout.MinWidth(248));
				}
				else
				{
					return EditorGUILayout.TextField(GUIContent.none, str, Styles.ShurikenValue, GUILayout.MinWidth(248));
				}
			}
			public static string TextFieldShaderVariable(string str)
			{
				//special version with that only accepts alphanumerical and underscore
				var result = TextField(str);
				var authChars = new List<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_0123456789".ToCharArray());
				for(var i = result.Length-1; i >= 0; i--)
					if(!authChars.Contains(result[i]))
						result = result.Remove(i, 1);
				return result;
			}

			public static string TextArea(string str, float height = 0)
			{
				return height > 0 ?
					EditorGUILayout.TextArea(str, Styles.ShurikenTextArea, GUILayout.MinWidth(248), GUILayout.Height(height)) :
					EditorGUILayout.TextArea(str, Styles.ShurikenTextArea, GUILayout.MinWidth(248));
			}
			public static T ObjectField<T>(T obj) where T : UnityEngine.Object
			{
				//return DrawProObjectField<T>(obj);
				return (T)EditorGUILayout.ObjectField(GUIContent.none, obj, typeof(T), false, GUILayout.MinWidth(248), GUILayout.Height(13));
			}

			public static T DrawProObjectField<T>(T obj, params GUILayoutOption[] options) where T : UnityEngine.Object
			{
				int pickerID = "ShurikenObjectField".GetHashCode();

				var rect = EditorGUILayout.GetControlRect(false, 13, Styles.ShurikenValue, options);
				var btnRect = rect;
				btnRect.width = 20;
				rect.xMax -= btnRect.width;
				btnRect.x += rect.width;

				GUI.Label(rect, TCP2_GUI.TempContent(obj != null ? obj.name : "None (" + typeof(T).ToString() + ")"), Styles.ShurikenValue);
				if (GUI.Button(btnRect, "...", "MiniToolbarButton"))
				{
					EditorGUIUtility.ShowObjectPicker<T>(obj, false, "", pickerID);
				}
				if (Event.current.commandName == "ObjectSelectorUpdated")
				{
					if (EditorGUIUtility.GetObjectPickerControlID() == pickerID)
					{
						obj = EditorGUIUtility.GetObjectPickerObject() as T;
					}
				}
				return obj;
			}

			public static bool ButtonPopup(string label) { return GUILayout.Button(label, Styles.ShurikenPopup, GUILayout.MinWidth(248), GUILayout.MinHeight(16)); }
			public static int IntField(int value) { return EditorGUILayout.IntField(value, Styles.ShurikenValue); }
			public static int IntField(int value, int min, int max) { return Mathf.Clamp(EditorGUILayout.IntField(value, Styles.ShurikenValue), min, max); }
			public static float FloatField(float value) { return EditorGUILayout.FloatField(value, Styles.ShurikenValue); }
			public static Vector2 Vector2Field(Vector2 v2) { return VectorFieldCustomStyle(v2, 2); }
			public static Vector3 Vector3Field(Vector3 v3) { return VectorFieldCustomStyle(v3, 3); }
			public static Vector4 Vector4Field(Vector4 v4) { return VectorFieldCustomStyle(v4, 4); }
			public static Color ColorField(Color c, bool alpha, bool hdr = false)
			{
				//small hacks to align with the shuriken ui components
				var rect = EditorGUILayout.GetControlRect(GUILayout.Height(16), GUILayout.MinWidth(248 - 4));
				rect.height = 13;
				rect.x -= 4;
				rect.width += 8;
				rect.y -= 2;
#if UNITY_2018_1_OR_NEWER
				return EditorGUI.ColorField(rect, GUIContent.none, c, false, alpha, hdr);
#else
				return EditorGUI.ColorField(rect, GUIContent.none, c, false, alpha, hdr, new ColorPickerHDRConfig(0f, 99f, 0.01010101f, 3f));
#endif
			}
			public static bool Toggle(bool toggle)
			{
				var rect = EditorGUILayout.GetControlRect(false, 16, Styles.ShurikenToggle, GUILayout.MinWidth(248));
				return EditorGUI.Toggle(rect, GUIContent.none, toggle, Styles.ShurikenToggle);
			}

			static Vector4 VectorFieldCustomStyle(Vector4 vec, int channels)
			{
				EditorGUILayout.BeginHorizontal();
				if(channels > 0)
				{
					GUILayout.Label("x", Styles.GrayMiniLabel, GUILayout.ExpandWidth(false));
					vec.x = FloatField(vec.x);
				}
				if(channels > 1)
				{
					GUILayout.Label("y", Styles.GrayMiniLabel, GUILayout.ExpandWidth(false));
					vec.y = FloatField(vec.y);
				}
				if(channels > 2)
				{
					GUILayout.Label("z", Styles.GrayMiniLabel, GUILayout.ExpandWidth(false));
					vec.z = FloatField(vec.z);
				}
				if(channels > 3)
				{
					GUILayout.Label("w", Styles.GrayMiniLabel, GUILayout.ExpandWidth(false));
					vec.w = FloatField(vec.w);
				}
				EditorGUILayout.EndHorizontal();

				return vec;
			}

			public static void DrawLine()
			{
				var c = EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f, 1.0f) : new Color(0.5f, 0.5f, 0.5f, 1.0f);
				DrawLine(c);
			}

			public static void DrawLine(Color color)
			{
				var rect = GUILayoutUtility.GetRect(GUIContent.none, Styles.LineStyle, GUILayout.Height(1));
				if(Event.current.type == EventType.Repaint)
				{
					var guiColor = GUI.color;
					GUI.color *= color;
					Styles.LineStyle.Draw(rect, GUIContent.none, "line".GetHashCode());
					GUI.color = guiColor;
				}
			}

			static readonly GUIContent gcInspectorLock = EditorGUIUtility.IconContent("InspectorLock");
			public static void DrawLockIcon(Color color)
			{
				if (gcInspectorLock != null)
				{
					var c = GUI.color;
					GUI.color *= color;
					var lockIconRect = EditorGUILayout.GetControlRect(false, 14, GUILayout.Width(14));
					GUI.DrawTexture(lockIconRect, gcInspectorLock.image);
					GUI.color = c;
				}
			}

			public static class Utils
			{
				public static string RemoveWhitespaces(string input)
				{
					return input.Replace(" ", "");
				}

				public static string VariableNameToReadable(string input)
				{
					string output = "";

					int start = 0;
					if (input[0] == '_') start = 1;

					bool lastWasLowercase = false;
					for(int i = start; i < input.Length; i++)
					{
						if ((Char.IsUpper(input[i]) || Char.IsDigit(input[i])) && lastWasLowercase && output.Length > 0)
						{
							output += " ";
						}

						char c = input[i];
						if (c == '_') c = ' ';

						output += c;
						lastWasLowercase = Char.IsLower(input[i]);
					}

					return output;
				}
			}

			public class IndentedLine : IDisposable
			{
				public IndentedLine(float indent = -1)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Space(indent < 0 ? Indent : indent);
				}

				public void Dispose()
				{
					GUILayout.EndHorizontal();
				}
			}
		}
	}
}