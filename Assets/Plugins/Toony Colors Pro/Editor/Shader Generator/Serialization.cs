using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using UnityEngine;

// Reflection-based serialization system: serialize simple value types, and specific classes (either those with the SerializeAs attribute, or special ones like Vector2, Vector3, ...)
// Used to serialize data and add it as a comment in generated shaders

namespace ToonyColorsPro
{
	namespace ShaderGenerator
	{
		public class Serialization
		{
			/// <summary>
			/// Declare a class or field as serializable, and set its serialized short name
			/// </summary>
			[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Property)]
			public class SerializeAsAttribute : Attribute
			{
				/// <summary>
				/// The short name to serialize that object, to reduce length of the serialized string.
				/// </summary>
				public string serializedName;

				/// <summary>
				/// Name of the field or property that will determine if the object can be serialized.
				/// Originally used to check if a Shader Property has been manually modified.
				/// </summary>
				public string conditionalField;

				public SerializeAsAttribute(string name, string conditionalField = null)
				{
					this.serializedName = name;
					this.conditionalField = conditionalField;
				}
			}

			/// <summary>
			/// Declare a method as a callback to deserialize an object manually
			/// </summary>
			[AttributeUsage(AttributeTargets.Method)]
			public class CustomDeserializeCallbackAttribute : Attribute
			{
				public CustomDeserializeCallbackAttribute() { }
			}

			/// <summary>
			/// Declare a method as a callback after an object has been deserialized
			/// </summary>
			[AttributeUsage(AttributeTargets.Method)]
			public class OnDeserializeCallbackAttribute : Attribute
			{
				public OnDeserializeCallbackAttribute() { }
			}

			//Will serialize an object as "type(field:value,field2:value,field3:value...)" provided that they have fields with the [SerializeAs] attribute
			public static string Serialize(object obj)
			{
				var output = "";

				//fetch class SerializedAs attribute
				var classAttributes = obj.GetType().GetCustomAttributes(typeof(SerializeAsAttribute), false);
				if (classAttributes != null && classAttributes.Length == 1)
				{
					var serializedAsAttribute = (classAttributes[0] as SerializeAsAttribute);

					//class has a conditional serialization?
					var conditionalFieldName = serializedAsAttribute.conditionalField;
					if (!string.IsNullOrEmpty(conditionalFieldName))
					{
						//try field
						var conditionalField = obj.GetType().GetField(conditionalFieldName);
						if (conditionalField != null)
						{
							if (!(bool)conditionalField.GetValue(obj))
							{
								return null;
							}
						}
						else
						{
							//try property
							var conditionalProperty = obj.GetType().GetProperty(conditionalFieldName);
							if (conditionalProperty != null)
							{
								if (!(bool)conditionalProperty.GetValue(obj, null))
								{
									return null;
								}
							}
							else
							{
								Debug.LogError(string.Format("Conditional field or property '{0}' doesn't exist for type '{1}'", conditionalFieldName, obj.GetType()));
							}
						}
					}

					var name = serializedAsAttribute.serializedName;
					output = name + "(";
				}

				// properties with [SerializeAs] attribute
				// note: only used for unityVersion currently; see Config.cs
				var properties = new List<PropertyInfo>(obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
				foreach (var prop in properties)
				{
					var attributes = prop.GetCustomAttributes(typeof(SerializeAsAttribute), true);
					if (attributes != null && attributes.Length == 1)
					{
						var name = (attributes[0] as SerializeAsAttribute).serializedName;
						output += string.Format("{0}:\"{1}\";", name, prop.GetValue(obj, null));
					}
				}

				//get all fields, and look for [SerializeAs] attribute
				var fields = new List<FieldInfo>(obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
				foreach (var field in fields)
				{
					var attributes = field.GetCustomAttributes(typeof(SerializeAsAttribute), true);
					if (attributes != null && attributes.Length == 1)
					{
						var name = (attributes[0] as SerializeAsAttribute).serializedName;

						//returns the value of an object as a string
						Func<object, string> GetStringValue = null;
						GetStringValue = @object =>
						{
							if (@object == null)
							{
								// Debug.LogError("Serialization error!\nTrying to get the string value of a null object.");
								return "__NULL__";
							}

							var type = @object.GetType();

							//object types
							if (!type.IsValueType && type != typeof(string))
							{
								//list
								if (@object is IList)
								{
									var list = @object as IList;
									var values = "list[";
									foreach (var value in list)
										values += GetStringValue(value) + ",";
									return values.TrimEnd(',') + "]";
								}
								//dictionary
								if (@object is IDictionary)
								{
									var dict = @object as IDictionary;
									var kvp = "dict[";
									foreach (DictionaryEntry entry in dict)
										kvp += entry.Key + "=" + GetStringValue(entry.Value) + ",";
									return kvp.TrimEnd(',') + "]";
								}
								//else try to serialize with this serializer
								var refAttributes = field.GetCustomAttributes(typeof(SerializeAsAttribute), true);
								if (refAttributes != null && refAttributes.Length == 1)
								{
									//serializable
									return Serialize(@object);
								}

								return null;
							}
							//string: enclose in quotes to prevent parsing errors (e.g. with parenthesis)
							if (type == typeof(string))
							{
								return string.Format("\"{0}\"", @object);
							}
							//value type: just return the toString version
							return string.Format(CultureInfo.InvariantCulture, "{0}", @object);
						};

						var val = GetStringValue(field.GetValue(obj));
						if (val == null)
							Debug.LogError(string.Format("Can't serialize this reference type: '{0}'\nFor field: '{1}'", field.FieldType, field.Name));
						else
							output += string.Format("{0}:{1};", name, val);
					}
				}

				output = output.TrimEnd(';');
				output += ")";

				return output;
			}

			//Deserialize without knowing type
			public static object Deserialize(string data, object[] args = null)
			{
				//extract serialized class name
				var index = data.IndexOf('(');
				var serializedClassName = data.Substring(0, index);

				//fetch all serialized classes names, and try to match it
				Type type = null;
				var allTypes = typeof(Serialization).Assembly.GetTypes();
				foreach (var t in allTypes)
				{
					var classAttributes = t.GetCustomAttributes(typeof(SerializeAsAttribute), false);
					if (classAttributes != null && classAttributes.Length == 1)
					{
						var name = (classAttributes[0] as SerializeAsAttribute).serializedName;
						if (name == serializedClassName)
						{
							//match!
							type = t;
						}
					}
				}

				if (type == null)
				{
					Debug.LogError(ShaderGenerator2.ErrorMsg("Can't find proper Type for serialized class named '<b>" + serializedClassName + "</b>'"));
					return null;
				}

				//return new object with correct type
				return Deserialize(data, type, args);
			}

			//Deserialize to a new object (needs a new() constructor, and valid arguments as 'args', if any)
			public static object Deserialize(string data, Type type, object[] args = null)
			{
				var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				foreach(var method in methods)
				{
					var deserializeCallbacks = method.GetCustomAttributes(typeof(CustomDeserializeCallbackAttribute), false);
					if (deserializeCallbacks.Length > 0)
					{
						return method.Invoke(null, new object[] { data, args });
					}
				}

				var obj = Activator.CreateInstance(type, args);
				return DeserializeTo(obj, data, type, args);
			}

			//Deserialize a specific type
			//'specialClasses': hook so that the caller can implement its own deserialization logic (used for Shader Property list in Config)
			public static object DeserializeTo(object obj, string data, Type type, object[] args = null, Dictionary<Type, Func<object, string, object>> specialClasses = null)
			{
				//extract parts of the input data, format should be "type(field:value;field2:value)"
				var index = data.IndexOf('(');

				var serializedClassName = data.Substring(0, index);
				var fieldsData = data.Substring(index + 1);
				fieldsData = fieldsData.Substring(0, fieldsData.Length - 1);    //remove trailing ')'

				//fetch class serialized name and check against specified T type
				var classAttributes = type.GetCustomAttributes(typeof(SerializeAsAttribute), false);
				if (classAttributes != null && classAttributes.Length == 1)
				{
					var name = (classAttributes[0] as SerializeAsAttribute).serializedName;
					if (name != serializedClassName)
					{
						Debug.LogError(string.Format("Class doesn't match serialized class name.\nExpected '{0}', got '{1}'.", serializedClassName, name));
						return null;
					}
				}

				//fetch all [SerializeAs] fields from that type
				var fields = new List<FieldInfo>(type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
				var serializedFields = new Dictionary<string, FieldInfo>();
				foreach (var field in fields)
				{
					var attributes = field.GetCustomAttributes(typeof(SerializeAsAttribute), true);
					if (attributes != null && attributes.Length == 1)
					{
						var name = (attributes[0] as SerializeAsAttribute).serializedName;
						serializedFields.Add(name, field);
					}
				}

				//converts a serialized string into a value
				Func<string, Type, object> StringToValue = null;
				StringToValue = (strValue, t) =>
				{
					//special classes: call the callback specified by caller
					if (specialClasses != null && specialClasses.ContainsKey(t))
					{
						return specialClasses[t].Invoke(obj, strValue);
					}

					//object types
					if (!t.IsValueType && t != typeof(string))
					{
						// handle null values
						if (strValue == "__NULL__")
						{
							return null;
						}

						//list
						if (typeof(IList).IsAssignableFrom(t))
						{
							//parse list values: remove 'list[' and ']' characters, and split on ','
							var serializedValues = SplitExcludingBlocks(strValue.Substring(5, strValue.Length - 6), ',', true, "()", "[]");

							//find what T is for this List<T>
							var itemType = t.GetGenericArguments()[0];

							//create new list with parsed serialized values
							var genericListType = typeof(List<>).MakeGenericType(itemType);
							var list = (IList)Activator.CreateInstance(genericListType);
							foreach (var item in serializedValues)
							{
								if (string.IsNullOrEmpty(item))
									continue;

								var v = StringToValue(item, itemType);
								if (v != null)
									list.Add(v);
							}

							//assign new list for obj
							return list;
						}

						//dict
						if (typeof(IDictionary).IsAssignableFrom(t))
						{
							//parse dict values: remove 'dict[' and ']' characters, and split on ','
							var serializedValues = SplitExcludingBlocks(strValue.Substring(5, strValue.Length - 6), ',', true, "()", "[]");

							//find what kind of KeyValuePair types are used
							var keyType = t.GetGenericArguments()[0];
							var valueType = t.GetGenericArguments()[1];

							//create new dictionary with parsed serialized values
							var genericDictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
							var dict = (IDictionary)Activator.CreateInstance(genericDictType);
							foreach (var item in serializedValues)
							{
								if (string.IsNullOrEmpty(item))
									continue;

								//gey key & value from format "key=value"
								var kv = item.Split('=');
								var key = kv[0];
								var value = kv[1];

								var k = StringToValue(key, keyType);
								var v = StringToValue(value, valueType);

								if (k != null && v != null)
									dict.Add(k, v);
							}

							//assign new list for obj
							return dict;
						}
						//else try to deserialize
						{
							var value = Deserialize(strValue, t, args);
							return value;
						}
					}

					//Unity value-type structs
					if (t == typeof(Vector2))
					{
						var v2data = strValue.Substring(1, strValue.Length - 2).Split(',');
						return new Vector2(float.Parse(v2data[0], CultureInfo.InvariantCulture), float.Parse(v2data[1], CultureInfo.InvariantCulture));
					}

					if (t == typeof(Vector3))
					{
						var v3data = strValue.Substring(1, strValue.Length - 2).Split(',');
						return new Vector3(float.Parse(v3data[0], CultureInfo.InvariantCulture), float.Parse(v3data[1], CultureInfo.InvariantCulture), float.Parse(v3data[2], CultureInfo.InvariantCulture));
					}

					if (t == typeof(Vector4))
					{
						var v4data = strValue.Substring(1, strValue.Length - 2).Split(',');
						return new Vector4(float.Parse(v4data[0], CultureInfo.InvariantCulture), float.Parse(v4data[1], CultureInfo.InvariantCulture), float.Parse(v4data[2], CultureInfo.InvariantCulture), float.Parse(v4data[3], CultureInfo.InvariantCulture));
					}

					if (t == typeof(Color))
					{
						var cData = strValue.Substring("RGBA(".Length, strValue.Length - "RGBA(".Length - 1).Split(',');
						return new Color(float.Parse(cData[0], CultureInfo.InvariantCulture), float.Parse(cData[1], CultureInfo.InvariantCulture), float.Parse(cData[2], CultureInfo.InvariantCulture), float.Parse(cData[3], CultureInfo.InvariantCulture));
					}

					//enums
					if (typeof(Enum).IsAssignableFrom(t))
					{
						return Enum.Parse(t, strValue);
					}

					//string: remove quotes to extract value
					if (t == typeof(string))
					{
						// handle null values
						if (strValue == "__NULL__")
						{
							return null;
						}

						return strValue.Trim('"');
					}

					//value type: automatic conversion
					return Convert.ChangeType(strValue, t, CultureInfo.InvariantCulture);
				};

				//iterate through entries in the source string
				var entries = SplitExcludingBlocks(fieldsData, ';', true, "()", "[]");
				foreach (var entry in entries)
				{
					var kvp = SplitExcludingBlocks(entry, ':', true, "()");
					var name = kvp[0];
					var strValue = kvp[1];

					if (serializedFields.ContainsKey(name))
					{
						var fieldInfo = serializedFields[name];
						var v = StringToValue(strValue, fieldInfo.FieldType);
						if (v != null)
							fieldInfo.SetValue(obj, v);
					}
				}

				//on deserialize callback, if any
				List<MethodInfo> methods = new List<MethodInfo>(type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
				foreach (var method in methods)
				{
					var deserializedAttributes = method.GetCustomAttributes(typeof(OnDeserializeCallbackAttribute), false);
					if (deserializedAttributes != null && deserializedAttributes.Length > 0)
					{
						//invoke the OnDeserialize callback
						method.Invoke(obj, null);
					}
				}

				return obj;
			}

			//Split a string excluding any characters found inside specified blocks
			//e.g.
			//  splitExcludingBlocks("list(a,b,c),list(d,e),list(f,g,h)", "()") will return
			//will return
			//  list(a,b,c)   list(d,e)   list(f,g,h)
			//and not
			// list(a   b   c)   list(d   e   list(f   g   h
			public static string[] SplitExcludingBlocks(string input, char separator, params string[] blocks) { return SplitExcludingBlocks(input, separator, false, false, blocks); }
			public static string[] SplitExcludingBlocks(string input, char separator, bool excludeQuotes, params string[] blocks) { return SplitExcludingBlocks(input, separator, excludeQuotes, false, blocks); }
			public static string[] SplitExcludingBlocks(string input, char separator, bool excludeQuotes, bool removeEmptyEntries, params string[] blocks)
			{
				foreach (var block in blocks)
				{
					if(block == "\"\"")
					{
						Debug.LogWarning("Using quotes block \"\" -> use excludeQuotes=true instead!");
					}
				}

				var insideBlock = 0;
				var insideQuotes = false;
				var i = 0;
				var currentWord = new StringBuilder();
				var words = new List<string>();

				//get opening/ending chars for blocks
				var openingChars = new List<char>(blocks.Length);
				var closingChars = new List<char>(blocks.Length);
				foreach (var block in blocks)
				{
					openingChars.Add(block[0]);
					closingChars.Add(block[1]);
				}

				while (i < input.Length)
				{
					if (!insideQuotes)
					{
						if (openingChars.Contains(input[i]))
							insideBlock++;
						else if (closingChars.Contains(input[i]))
							insideBlock--;
					}

					if (excludeQuotes && input[i] == '"')
					{
						insideQuotes = !insideQuotes;
						insideBlock += insideQuotes ? +1 : -1;
					}

					if (input[i] == separator && insideBlock == 0)
					{
						if (!removeEmptyEntries || currentWord.Length != 0)
						{
							words.Add(currentWord.ToString());
						}
						currentWord.Length = 0;
					}
					else
					{
						currentWord.Append(input[i]);
					}

					i++;
				}

				if (!removeEmptyEntries || currentWord.Length != 0)
				{
					words.Add(currentWord.ToString());
				}

				return words.ToArray();
			}
		}
	}
}