﻿/*
MIT License

Copyright (c) 2023 Multitask - Unified - Suite -for-Expts

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files(the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/



using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using USE_ExperimentTemplate_Classes;


namespace USE_Settings
{
	public class Settings
	{
		public string FilePath { get; }

		private string Name;

		//public string warning = "";
		private Dictionary<string, object> SettingDict;
		private List<string> StringList;

		public Settings(string name, string path)
		{
			Name = name;
			FilePath = path;
			SettingDict = new Dictionary<string, object>();
			StringList = new List<string>();
		}

		public bool CheckString(string key)
		{
			return StringList.Contains(key);
		}

		public void AddSetting(string nameString, object setting)
		{
			SettingDict.Add(nameString, setting);
		}

		public void AddSetting(string typeString, string key, string stringValue)
		{
			if (typeString.ToLower() == "string")
			{
				StringList.Add(key);
			}

			Type type = TypeDict[typeString];
			try
			{
				if (typeString.ToLower() == "monitordetails")
				{
					AddSetting(key, JsonConvert.DeserializeObject<USE_DisplayManagement.MonitorDetails>(stringValue));
				}
				else if (typeString.ToLower() == "screendetails")
				{
					AddSetting(key, JsonConvert.DeserializeObject<USE_DisplayManagement.ScreenDetails>(stringValue));
				}
				// else if (typeString.ToLower() == "exptparameters")
				// {
				// 	AddSetting(key, JsonConvert.DeserializeObject<FLU_Common_Namespace.ExptParameters>(stringValue));
				// }
				else if (typeString.ToLower() == "string")
				{
					AddSetting(key, stringValue);
				}
				// else if (typeString.ToLower() == "blockdef[]")
				// {
				// 	AddSetting(key, JsonConvert.DeserializeObject<FLU_Common_Namespace.BlockDef[]>(stringValue));
				// }
				else if (typeString.ToLower() == "vector3")
				{
					if (StartsOrEndsWithBrackets(stringValue))
						stringValue = stringValue.Substring(1, stringValue.Length - 2);
					string[] sArray = stringValue.Split(',');
					AddSetting(key,new Vector3(float.Parse(sArray[0].Trim()), float.Parse(sArray[1].Trim()), float.Parse(sArray[2].Trim())));
				}
				else if (typeString.ToLower() == "vector2")
				{
					if (StartsOrEndsWithBrackets(stringValue))
						stringValue = stringValue.Substring(1, stringValue.Length - 2);
					string[] sArray = stringValue.Split(',');
					AddSetting(key, new Vector2(float.Parse(sArray[0].Trim()), float.Parse(sArray[1].Trim())));
				}
				else if (typeString.ToLower() == "vector3[]")
				{
					AddSetting(key, (Vector3[]) SessionSettings.ConvertStringToType<Vector3[]>(stringValue));
				}
				else if (typeString.ToLower() == "vector2[]")
				{
					AddSetting(key, (Vector2[])SessionSettings.ConvertStringToType<Vector2[]>(stringValue));
				}
				else if (typeString.ToLower() == "list<vector3>")
				{
					AddSetting(key, (List<Vector3>)SessionSettings.ConvertStringToType<List<Vector3>>(stringValue));
				}
				else if (typeString.ToLower() == "list<vector2>")
				{
					AddSetting(key, (List<Vector2>)SessionSettings.ConvertStringToType<List<Vector2>>(stringValue));
				}
				else if (typeString.ToLower() == "float[]")
				{
					AddSetting(key, (float[])SessionSettings.ConvertStringToType<float[]>(stringValue));
				}
				else if (typeString.ToLower() == "list<string>")
				{
					if(StartsOrEndsWithBrackets(stringValue))
                        stringValue = stringValue.Substring(1, stringValue.Length - 2);
					string[] sArray = stringValue.Split(',');
					for (int sCount = 0; sCount < sArray.Length; sCount++)
					{
						sArray[sCount] = sArray[sCount].Replace("\"", "");
						sArray[sCount] = sArray[sCount].Trim();
					}
					AddSetting(key, sArray.ToList());
				}
				else if (typeString.ToLower() == "dictionary<string,string>")
				{
                    if (StartsOrEndsWithBrackets(stringValue))
                        stringValue = stringValue.Substring(1, stringValue.Length - 2);
                    string[] sArray = stringValue.Split(',');
                    Dictionary<string, string> pairs = new Dictionary<string, string>();
					for (int sCount = 0; sCount < sArray.Length; sCount++)
					{
						sArray[sCount] = sArray[sCount].Replace("\"", "");
						sArray[sCount] = sArray[sCount].Trim();
						string[] sArray2 = sArray[sCount].Split(':');
						pairs.Add(sArray2[0].Trim(), sArray2[1].Trim());
					}
					AddSetting(key, pairs);
				}
				else if (typeString.ToLower() == "ordereddictionary<string,string>")
				{
                    if (StartsOrEndsWithBrackets(stringValue))
                        stringValue = stringValue.Substring(1, stringValue.Length - 2);
                    string[] sArray = stringValue.Split(',');
                    OrderedDictionary pairs = new OrderedDictionary();
					for (int sCount = 0; sCount < sArray.Length; sCount++)
					{
						sArray[sCount] = sArray[sCount].Replace("\"", "");
						sArray[sCount] = sArray[sCount].Trim();
						string[] sArray2 = sArray[sCount].Split(':');
						pairs.Add(sArray2[0].Trim(), sArray2[1].Trim());
					}
					AddSetting(key, pairs);
				}
				else if (type != null)
					AddSetting(key, Convert.ChangeType(stringValue, type));
				else
					throw new Exception("Attempted to add setting of type \"" + typeString + "\" " +
										"to Setting " + key + "in Settings list " + Name +
										" but this type is not recognized.");
			}
			catch (Exception e)
			{
                Debug.Log("Tried to convert string \"" + stringValue + "\" to type \""
							+ typeString + "\" to add to Setting " + key + " in Settings List " + Name +
							" but the conversion failed.");

                throw new ArgumentException(e.Message + "\t" + e.StackTrace);
            }

        }

		public bool StartsOrEndsWithBrackets(string s)
		{
			if (s.StartsWith("(", StringComparison.Ordinal) &&
				s.EndsWith(")", StringComparison.Ordinal) ||
				s.StartsWith("{", StringComparison.Ordinal) &&
				s.EndsWith("}", StringComparison.Ordinal) ||
				s.StartsWith("[", StringComparison.Ordinal) &&
				s.EndsWith("]", StringComparison.Ordinal))
			{
				return true;
			}
			else
				return false;
        }

		public bool SettingExists(string key)
		{
			return SettingDict.ContainsKey(key);
		}

		public object Get(string key)
		{
			if (SettingDict.ContainsKey(key))
				return SettingDict[key];
			else
				throw new Exception("Tried to access value of \"" + key + "\"from Settings " + Name +
				                    " but this key does not exist in this Settings object.");
		}

		private Dictionary<string, Type> TypeDict = new Dictionary<string, Type>
		{
			{ "List<string>", typeof(List<string>) },
			{ "List<bool>", typeof(List<bool>) },
			{ "List<int>", typeof(List<int>) },
			{ "List<float>", typeof(List<float>) },
			{ "string", typeof(string) },
			{ "bool", typeof(bool) },
			{ "int", typeof(int) },
			{ "float", typeof(float) },
			{ "string[]", typeof(string[]) },
			{ "bool[]", typeof(bool[]) },
			{ "int[]", typeof(int[]) },
			{ "float[]", typeof(float[]) },
			{ "Dictionary<string,List<string>>", typeof(Dictionary<string, List<string>>) },
			{ "Dictionary<string,int>", typeof(Dictionary<string, int>) },
			{ "Dictionary<string,string>", typeof(Dictionary<string, string>) },
			{ "OrderedDictionary<string,string>", typeof(OrderedDictionary) },
			{ "SortedList<string,List<string>>", typeof(SortedList<string, List<string>>) },
			{ "List<string[]>", typeof(List<string[]>) },
			{ "MonitorDetails", typeof(USE_DisplayManagement.MonitorDetails) },
			{ "ScreenDetails", typeof(USE_DisplayManagement.ScreenDetails) },
			// { "ExptParameters", typeof(FLU_Common_Namespace.ExptParameters) },
			// { "BlockDef[]", typeof(FLU_Common_Namespace.BlockDef[]) },
			{ "Vector3", typeof(Vector3) },
			{ "Vector2", typeof(Vector2) },
			{ "Vector3[]", typeof(Vector3[]) },
			{ "Vector2[]", typeof(Vector2[]) },
			{ "List<Vector3>", typeof(List<Vector3>) },
			{ "List<Vector2>", typeof(List<Vector2>) },
			{"MazeDef[]", typeof(MazeGame_Namespace.MazeDef[])}
		};
};


	public static class SessionSettings
	{
		public static Dictionary<string, Settings> allSettings = new Dictionary<string, Settings>();
		private static Dictionary<string, Settings> savedSettings = new Dictionary<string, Settings>();

		//public static void Reset()
		//{
		//    allSettings = new Dictionary<string, Settings>();
		//}

		public static void Save()
		{
			// Perform shallow copy
			savedSettings = new Dictionary<string, Settings>(allSettings);
		}

		public static void Restore()
		{
			// Perform shallow copy
			allSettings = new Dictionary<string, Settings>(savedSettings);
		}

		public static bool SettingClassExists(string key)
		{
			return allSettings.ContainsKey(key);
		}

		public static bool SettingExists(string key, string settingKey)
		{
			if (allSettings.ContainsKey(key))
				return allSettings[key].SettingExists(settingKey);
			else
				return false;
		}


		public static object Get(string key, string settingKey = "")
		{
			if (settingKey == "")
			{
				settingKey = key;
			}
			if (allSettings.ContainsKey(key))
			{
				if (allSettings[key].CheckString(settingKey))
				{
					string temp = (string)allSettings[key].Get(settingKey);
					return temp.Substring(1, temp.Length - 2);//strings are padded with \" at each end during object conversion
				}
				else
					return allSettings[key].Get(settingKey);
			}
			else
			{
				//throw new Exception("Settings not found: " + key);
				return null;
			}
		}


		public static object Get<T>(string key, string settingKey = "")
		{
			if (settingKey == "")
				settingKey = key;
			if (!allSettings.ContainsKey(key))
				throw new Exception("Settings not found: " + key);
			else
			{
				try
				{
					if (typeof(T) == typeof(string))
					{
						string temp = (string)allSettings[key].Get(settingKey);
						return temp.Substring(1, temp.Length - 2);//strings are padded with \" at each end during object conversion
					}
					else
						return (T)Convert.ChangeType(allSettings[key].Get(settingKey), typeof(T));
					
				}
				catch (Exception e)
				{
					Debug.Log("Tried to access value of \"" + key + "\" with type of " +
							typeof(T) + "from Settings " + key +
							" but there was a problem.");
					throw new ArgumentException(e.Message + "\t" + e.StackTrace);
				}
			}
		}

		public static string GetPath(string key, string settingKey)
		{
			if (!allSettings.ContainsKey(key))
				throw new Exception("Settings not found: " + key);
			else
				return allSettings[key].FilePath;
		}





		public static void ImportSettings_SingleTypeJSON<T>(string settingsCategory, string settingsPath, string serverFileString = null, string dictName = "")
		{
			Debug.Log("Attempting to load settings file " + settingsPath + ".");
			if (dictName == "")
				dictName = settingsCategory;

			Settings settings = new Settings(dictName, settingsPath);
			string dataAsJsonString = serverFileString == null ? File.ReadAllText(settingsPath) : serverFileString;

            try
            {
                settings.AddSetting(settingsCategory, JsonConvert.DeserializeObject<T>(dataAsJsonString));
            }
            catch (Exception e)
            {
                Debug.Log("Error adding JSON file \"" + settingsPath + "\" to Settings \"" + settingsCategory + "\".");
                throw new Exception(e.Message + "\t" + e.StackTrace);
            }
			allSettings.Add(dictName, settings);
		}


		public static void ImportSettings_SingleTypeArray<T>(string settingsCategory, string settingsPath, string serverFileString = null, string dictName = "", char delimiter = '\t')
		{
			Settings settings = new Settings(dictName, settingsPath);

			Debug.Log("Attempting to load settings file " + settingsPath);
			if (dictName == "")
				dictName = settingsCategory;


			//THIS WAS BREAKING SOME STUFF. MIGHT NEED TO BE CHANGED/FINALIZED!
			if(serverFileString == null)
			//if (!SessionValues.WebBuild)
			{
				if (!File.Exists(settingsPath))
					return;
			}


			string[] lines;

			if(serverFileString != null)
			{
				string[] splitLines = serverFileString.Split('\n');
				List<string> stringList = new List<string>();
				foreach(var line in splitLines)
				{
                    if (string.IsNullOrEmpty(line.Trim()) || line.Trim().StartsWith("//", StringComparison.Ordinal)) //Skip empty and/or commented out lines
                        continue;
					stringList.Add(line);
                }
				lines = stringList.ToArray();
            }
			else
				lines = ReadSettingsFile(settingsPath, "//", "...");


			T[] settingsArray = new T[lines.Length - 1];

			string[] fieldNames = lines[0].Split(delimiter);

			string tempFieldName = "";
			foreach (string fieldName in fieldNames)
			{
				tempFieldName = fieldName.Trim();
				if (typeof(T).GetProperty(tempFieldName) == null & typeof(T).GetField(tempFieldName) == null)
				{
					throw new Exception("Settings file \"" + settingsCategory + "\" contains the header \""
						+ tempFieldName + "\" but this is not a public property or field of the provided type "
						+ typeof(T) + ".");
				}
			}

			Type ft = null;
			FieldInfo myFieldInfo = null;
			for (int iLine = 1; iLine < lines.Length; iLine++)
			{
				settingsArray[iLine - 1] = (T)Activator.CreateInstance(typeof(T));
				string[] values = lines[iLine].Split(delimiter);
				for (int iVal = 0; iVal < fieldNames.Length; iVal++)
				{
					string fieldName = fieldNames[iVal].Trim();
					try
					{
						if (typeof(T).GetProperty(fieldName) != null)
							Debug.Log("TypeOf GetProperty of " + fieldName + "is not null!");
						else if (typeof(T).GetField(fieldName) != null)
						{
							myFieldInfo = typeof(T).GetField(fieldName);
							ft = myFieldInfo.FieldType;
							if (ft == typeof(string))
								myFieldInfo.SetValue(settingsArray[iLine - 1], values[iVal]);
							else if (ft == typeof(bool))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (bool)ConvertStringToType<bool>(values[iVal]));
							else if (ft == typeof(int))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (int)ConvertStringToType<int>(values[iVal]));
							else if (ft == typeof(float))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (float)ConvertStringToType<float>(values[iVal]));
							else if (ft == typeof(bool?))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (bool?)ConvertStringToType<bool>(values[iVal]));
							else if (ft == typeof(int?))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (int?)ConvertStringToType<int>(values[iVal]));
							else if (ft == typeof(float?))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (float?)ConvertStringToType<float>(values[iVal]));
							else if (ft == typeof(Vector2))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (Vector2)ConvertStringToType<Vector2>(values[iVal]));
							else if (ft == typeof(Vector3))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (Vector3)ConvertStringToType<Vector3>(values[iVal]));
							else if (ft == typeof(string[]))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (string[])ConvertStringToType<string[]>(values[iVal]));
							else if (ft == typeof(bool[]))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (bool[])ConvertStringToType<bool[]>(values[iVal]));
							else if (ft == typeof(int[]))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (int[])ConvertStringToType<int[]>(values[iVal]));
							else if (ft == typeof(float[]))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (float[])ConvertStringToType<float[]>(values[iVal]));
							else if (ft == typeof(bool?[]))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (bool?[])ConvertStringToType<bool[]>(values[iVal]));
							else if (ft == typeof(int?[]))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (int?[])ConvertStringToType<int[]>(values[iVal]));
							else if (ft == typeof(float?[]))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (float?[])ConvertStringToType<float[]>(values[iVal]));
							else if (ft == typeof(Vector2[]))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (Vector2[])ConvertStringToType<Vector2[]>(values[iVal]));
							else if (ft == typeof(Vector3[]))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (Vector3[])ConvertStringToType<Vector3[]>(values[iVal]));
							else if (ft == typeof(Reward[]))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (Reward[])ConvertStringToType<Reward[]>(values[iVal]));
                            else if (ft == typeof(string[][]))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (string[][])ConvertStringToType<string[][]>(values[iVal]));
							else if (ft == typeof(bool[][]))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (bool[][])ConvertStringToType<bool[][]>(values[iVal]));
							else if (ft == typeof(int[][]))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (int[][])ConvertStringToType<int[][]>(values[iVal]));
							else if (ft == typeof(float[][]))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (float[][])ConvertStringToType<float[][]>(values[iVal]));
							else if (ft == typeof(bool?[][]))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (bool?[][])ConvertStringToType<bool[][]>(values[iVal]));
							else if (ft == typeof(int?[][]))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (int?[][])ConvertStringToType<int[][]>(values[iVal]));
							else if (ft == typeof(float?[][]))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (float?[][])ConvertStringToType<float[][]>(values[iVal]));
							else if (ft == typeof(Reward[][]))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (Reward[][])ConvertStringToType<Reward[][]>(values[iVal]));
                            else if (ft == typeof(Color))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (Color)ConvertStringToType<Color>(values[iVal]));
							else if (ft == typeof(MazeGame_Namespace.MazeDef[]))
								myFieldInfo.SetValue(settingsArray[iLine - 1], (MazeGame_Namespace.MazeDef[])ConvertStringToType<MazeGame_Namespace.MazeDef[]>(values[iVal]));
							else
								Debug.LogError("Attempted to convert value " + values[iVal] + " with header " + fieldName +
									" to type " + ft + " but there is no conversion specified for this type.");
							//				{ "bool",typeof(bool)},
							//{ "int",typeof(int)},
							//{ "float",typeof(float)},
							//{ "string[]",typeof(string[])},
							//{ "bool[]",typeof(bool[])},
							//{ "int[]",typeof(int[])},
							//{ "float[]",typeof(float[])},
							//{ "List<string>",typeof(List<string>)},
							//{ "List<bool>",typeof(List<bool>)},
							//{ "List<int>",typeof(List<int>)},
							//{ "List<float>",typeof(List<float>)},
							//Convert.ChangeType(values[iVal], typeof(T).GetField(fieldName).FieldType));
							//settingsArray[iLine-1].typeof(T).GetField(fieldName) = Convert.ChangeType(values[iVal], typeof(T).GetField(fieldName).FieldType);
						}
					}
					catch (Exception e)
					{
						Debug.Log(fieldNames[iVal] + ": " + values[iVal]);
						Debug.Log("Error adding TDF file \"" + settingsPath + "\" to Settings \"" + settingsCategory + "\".");
						throw new Exception(e.Message + "\t" + e.StackTrace);
					}
				}
			}
			settings.AddSetting(settingsCategory, settingsArray);
			allSettings.Add(dictName, settings);
		}


        public static void ImportSettings_MultipleType(string settingsCategory, string settingsPath, string serverFileString = null, char delimiter = '\t')
		{
            Debug.Log("Attempting to load settings file " + settingsPath + ".");
			Settings settings = new Settings(settingsCategory, settingsPath);
			string[] lines;

			if(serverFileString != null)
				lines = serverFileString.Split('\n');
			else
				lines = ReadSettingsFile(settingsPath, "//", "...");
            
			foreach(string line in lines)
			{
                if (string.IsNullOrEmpty(line.Trim()) || line.Trim().StartsWith("//", StringComparison.Ordinal)) //Skip empty and/or commented out lines
                    continue;                
                string[] splitString = line.Split(delimiter);
                try
                {
                    settings.AddSetting(splitString[0], splitString[1], splitString[2]);
                }
                catch (Exception e)
                {
                    Debug.Log("Attempted to import Settings file \"" + settingsPath + "\" but line \"" + line + "\" has " + line.Length + " entries, 3 expected.");
                    throw new Exception(e.Message + "\t" + e.StackTrace);
                }
            }
			allSettings.Add(settingsCategory, settings);
		}

		private static string[] ReadSettingsFile(string settingsPath, string commentPrefix = "", string continueSuffix = "")
		{
			List<string> outputList = new List<string>();
			//read the file
			StreamReader textFile;
			try
			{
				//read in all data and parse it
				textFile = new StreamReader(settingsPath);
            }
            catch (Exception e)
			{
				Debug.Log("The settings file could not be read:" + settingsPath);
				throw new Exception(e.Message + "\t" + e.StackTrace);
			}

			if (!(textFile == null || textFile == default(StreamReader)))
			{
				string line;
				while ((line = textFile.ReadLine()) != null)
				{
					if (string.IsNullOrEmpty(line) || (!string.IsNullOrEmpty(commentPrefix) && line.StartsWith(commentPrefix, StringComparison.Ordinal)))
						continue;
					while (!string.IsNullOrEmpty(continueSuffix) && line.EndsWith(continueSuffix, StringComparison.Ordinal))
					{
						line = line.Remove(line.Length - continueSuffix.Length);
						string newLine = textFile.ReadLine().Trim();
						while (string.IsNullOrEmpty(newLine) || (!string.IsNullOrEmpty(commentPrefix) && newLine.StartsWith(commentPrefix, StringComparison.Ordinal)))
						{
							newLine = textFile.ReadLine().Trim();
						}
						line = line + newLine;
					}
					outputList.Add(line);
				}
			}
			return outputList.ToArray();
		}



		public static string ReadSettingsFileAsString(string settingsPath, string commentPrefix = "", string continueSuffix = "")
		{
			return string.Join("", ReadSettingsFile(settingsPath, commentPrefix, continueSuffix));
		}


		public static void StoreSettings(string dataPath)
		{
			foreach (Settings settings in allSettings.Values)
			{
				StoreSettings(dataPath, settings);
				//File.Copy(settings.FilePath, dataPath + Path.DirectorySeparatorChar + Path.GetFileName(settings.FilePath), true);
			}
		}

		public static void StoreSettings(string dataPath, string key)
		{
			Settings settings = allSettings[key];
			StoreSettings(dataPath, settings);
		}

		public static void StoreSettings(string dataPath, Settings settings)
		{
			Directory.CreateDirectory(dataPath);
			File.Copy(settings.FilePath, dataPath + Path.DirectorySeparatorChar + Path.GetFileName(settings.FilePath), true);
		}

		public static string[] GetStringArray(string s)
		{
			return (string[])JsonConvert.DeserializeObject(s, typeof(string[]));
		}
		public static string[][] GetStringArrayofArrays(string s)
		{
			return (string[][])JsonConvert.DeserializeObject(s, typeof(string[][]));
		}

		//public static object[][] ConvertStringJaggedArray<T>(string s)
		//{
		//	string[] initialArray = GetStringArray(s);
		//	object[][] finalArray = new object[initialArray.Length][];
		//}

		public static object ConvertStringArray<T>(string s)
		{
			if (typeof(T) == typeof(int))
			{
				string[] sArray = GetStringArray(s);
				int[] finalArray = new int[sArray.Length];
				for (int iVal = 0; iVal < sArray.Length; iVal++)
					finalArray[iVal] = int.Parse(sArray[iVal]);
				return finalArray;
			}
			else if (typeof(T) == typeof(float))
			{
				string[] sArray = GetStringArray(s);
				float[] finalArray = new float[sArray.Length];
				for (int iVal = 0; iVal < sArray.Length; iVal++)
					finalArray[iVal] = float.Parse(sArray[iVal]);
				return finalArray;
			}
			else if (typeof(T) == typeof(string))
			{
				return GetStringArray(s);
			}
			else if (typeof(T) == typeof(Vector2))
			{
				float[] floatArray = (float[])ConvertStringArray<float>(s);
				return new Vector2(floatArray[0], floatArray[1]);
			}
			else if (typeof(T) == typeof(Vector3))
			{
				float[] floatArray = (float[])ConvertStringArray<float>(s);
				return new Vector3(floatArray[0], floatArray[1], floatArray[2]);
			}
			else if (typeof(T) == typeof(Vector2[]))
			{
				string[][] sArray = GetStringArrayofArrays(s);
				Vector2[] finalArray = new Vector2[sArray.Length];
				for (int iVal = 0; iVal < sArray.Length; iVal++)
				{
					finalArray[iVal] = new Vector2(float.Parse(sArray[iVal][0]), float.Parse(sArray[iVal][1]));
				}
				return finalArray;
			}
			else if (typeof(T) == typeof(Vector3[]))
			{
				string[][] sArray = GetStringArrayofArrays(s);
				Vector3[] finalArray = new Vector3[sArray.Length];
				for (int iVal = 0; iVal < sArray.Length; iVal++)
				{
					finalArray[iVal] = new Vector3(float.Parse(sArray[iVal][0]), float.Parse(sArray[iVal][1]), float.Parse(sArray[iVal][2]));
				}
				return finalArray;
			}
			else
			{
				return GetStringArray(s);
			}
		}

		public static object ConvertStringJaggedArray<T>(string s)
		{
			if (typeof(T) == typeof(int))
			{
				string[][] outerArray = GetStringArrayofArrays(s);
				int[][] finalArray = new int[outerArray.Length][];
				for (int iOuter = 0; iOuter < outerArray.Length; iOuter++)
				{
					finalArray[iOuter] = Array.ConvertAll(outerArray[iOuter], str => int.Parse(str));
					//string[] innerArray = GetStringArray(outerArray[iOuter]);
					//finalArray[iOuter] = new int[innerArray.Length];
					//for (int iInner = 0; iInner < innerArray.Length; iInner++)
						//finalArray[iOuter][iInner] = int.Parse(innerArray[iInner]);
				}
				return finalArray;
			}
			else if (typeof(T) == typeof(float))
			{
				string[][] outerArray = GetStringArrayofArrays(s);
				float[][] finalArray = new float[outerArray.Length][];
				for (int iOuter = 0; iOuter < outerArray.Length; iOuter++)
				{
					finalArray[iOuter] = Array.ConvertAll(outerArray[iOuter], str => float.Parse(str));
					//string[] innerArray = GetStringArray(outerArray[iOuter]);
					//finalArray[iOuter] = new float[innerArray.Length];
					//for (int iInner = 0; iInner < innerArray.Length; iInner++)
					//finalArray[iOuter][iInner] = float.Parse(innerArray[iInner]);
				}
				return finalArray;
			}
			else if (typeof(T) == typeof(string))
			{
				return GetStringArrayofArrays(s);
			}
			else if (typeof(T) == typeof(bool))
			{
				string[][] outerArray = GetStringArrayofArrays(s);
				bool[][] finalArray = new bool[outerArray.Length][];
				for (int iOuter = 0; iOuter < outerArray.Length; iOuter++)
				{
					finalArray[iOuter] = Array.ConvertAll(outerArray[iOuter], str => bool.Parse(str));
				}
				return finalArray;
			}
			else
				return new object[0][];
		}

		public static object ConvertStringToType<T>(string s)
		{
			if (typeof(T) == typeof(string))
				return s;
			else if (typeof(T) == typeof(Vector2))
			{
				try
				{
					return (Vector2)ConvertStringArray<Vector2>(s);
				}
				catch (Exception e)
				{
					Debug.LogError("Tried to convert string \"" + s + "\" to type \""
						+ typeof(T).Name + " but the conversion failed.");

					throw new ArgumentException(e.Message + "\t" + e.StackTrace);
				}
			}
			else if (typeof(T) == typeof(Vector3))
			{
				try
				{
					return (Vector3)ConvertStringArray<Vector3>(s);
				}
				catch (Exception e)
				{
					Debug.LogError("Tried to convert string \"" + s + "\" to type \""
						+ typeof(T).Name + " but the conversion failed.");

					throw new ArgumentException(e.Message + "\t" + e.StackTrace);
				}
			}
			else if (typeof(T) == typeof(Vector2[]))
			{
				try
				{
					return (Vector2[])ConvertStringArray<Vector2[]>(s);
				}
				catch (Exception e)
				{
					Debug.LogError("Tried to convert string \"" + s + "\" to type \""
						+ typeof(T).Name + " but the conversion failed.");

					throw new ArgumentException(e.Message + "\t" + e.StackTrace);
				}
			}
			else if (typeof(T) == typeof(Vector3[]))
			{
				try
				{
					return (Vector3[])ConvertStringArray<Vector3[]>(s);
				}
				catch (Exception e)
				{
					Debug.LogError("Tried to convert string \"" + s + "\" to type \""
						+ typeof(T).Name + " but the conversion failed.");

					throw new ArgumentException(e.Message + "\t" + e.StackTrace);
				}
			}
			else if (typeof(T) == typeof(Reward))
			{
				try
				{
					return (Reward)JsonConvert.DeserializeObject(s, typeof(Reward));
				}
				catch (Exception e)
				{
					Debug.LogError("Tried to convert string \"" + s + "\" to type \""
						+ typeof(T).Name + " but the conversion failed.");

					throw new ArgumentException(e.Message + "\t" + e.StackTrace);
				}
			}
			else if (typeof(T) == typeof(Reward[]))
			{
				try
				{
					return (Reward[])JsonConvert.DeserializeObject(s, typeof(Reward[]));
				}
				catch (Exception e)
				{
					Debug.LogError("Tried to convert string \"" + s + "\" to type \""
						+ typeof(T).Name + " but the conversion failed.");

					throw new ArgumentException(e.Message + "\t" + e.StackTrace);
				}
			}
			else if (typeof(T) == typeof(Reward[][]))
			{
				try
				{
					return (Reward[][])JsonConvert.DeserializeObject(s, typeof(Reward[][]));
				}
				catch (Exception e)
				{
					Debug.LogError("Tried to convert string \"" + s + "\" to type \""
						+ typeof(T).Name + " but the conversion failed.");

					throw new ArgumentException(e.Message + "\t" + e.StackTrace);
				}
			}
            else if (typeof(T) == typeof(float[]))
			{
				try
				{
					return (float[])ConvertStringArray<float>(s);
				}
				catch (Exception e)
				{
					Debug.LogError("Tried to convert string \"" + s + "\" to type \""
						+ typeof(T).Name + " but the conversion failed.");

					throw new ArgumentException(e.Message + "\t" + e.StackTrace);
				}
			}
			else if (typeof(T) == typeof(int[]))
			{
				try
				{
					return (int[])ConvertStringArray<int>(s);
				}
				catch (Exception e)
				{
					Debug.LogError("Tried to convert string \"" + s + "\" to type \""
						+ typeof(T).Name + " but the conversion failed.");

					throw new ArgumentException(e.Message + "\t" + e.StackTrace);
				}
			}
			else if (typeof(T) == typeof(int[][]))
			{

				try
				{// Remove the parentheses

					return (int[][])ConvertStringJaggedArray<int>(s);
				}
				catch (Exception e)
				{
					Debug.LogError("Tried to convert string \"" + s + "\" to type \""
						+ typeof(T).Name + " but the conversion failed.");

					throw new ArgumentException(e.Message + "\t" + e.StackTrace);
				}
			}
			else if (typeof(T) == typeof(float[][]))
			{

				try
				{// Remove the parentheses

					return (float[][])ConvertStringJaggedArray<float>(s);
				}
				catch (Exception e)
				{
					Debug.LogError("Tried to convert string \"" + s + "\" to type \""
						+ typeof(T).Name + " but the conversion failed.");

					throw new ArgumentException(e.Message + "\t" + e.StackTrace);
				}
			}
			else if (typeof(T) == typeof(bool[][]))
			{

				try
				{// Remove the parentheses

					return (bool[][])ConvertStringJaggedArray<bool>(s);
				}
				catch (Exception e)
				{
					Debug.LogError("Tried to convert string \"" + s + "\" to type \""
						+ typeof(T).Name + " but the conversion failed.");

					throw new ArgumentException(e.Message + "\t" + e.StackTrace);
				}
			}
			else if (typeof(T) == typeof(Color))
			{
				try
				{
					return (Color)ConvertStringJaggedArray<Color>(s);
				}
				catch (Exception e)
				{
					Debug.LogError("Tried to convert string \"" + s + "\" to type \""
						+ typeof(T).Name + " but the conversion failed.");

					throw new ArgumentException(e.Message + "\t" + e.StackTrace);
				}
			}

			else if (typeof(T) != null)
			{
				try
				{
					//can add custom conversion instructions for particular typeStrings if needed
					return Convert.ChangeType(s, typeof(T));
				}
				catch (Exception e)
				{
					Debug.LogError("Tried to convert string \"" + s + "\" to type \""
						+ typeof(T).Name + " but the conversion failed.");

					throw new ArgumentException(e.Message + "\t" + e.StackTrace);
				}
			}
			else
			{
				throw new ArgumentException("Tried to convert string \"" + s + "\" to type \""
						+ typeof(T).Name + " but the type was not recognized.");
			}
		}


		private static Dictionary<string, Type> TypeDict = new Dictionary<string, Type> {
			{"string",typeof(string)},
			{"bool",typeof(bool)},
			{"int",typeof(int)},
			{"float",typeof(float)},
			{"string[]",typeof(string[])},
			{"bool[]",typeof(bool[])},
			{"int[]",typeof(int[])},
			{"float[]",typeof(float[])},
			{"List<string>",typeof(List<string>)},
			{"List<bool>",typeof(List<bool>)},
			{"List<int>",typeof(List<int>)},
			{"List<float>",typeof(List<float>)},
			{"Dictionary<string,List<string>>",typeof(Dictionary<string,List<string>>)},
			{"Dictionary<string,int>",typeof(Dictionary<string,int>)},
			{"SortedList<string,List<string>>",typeof(SortedList<string,List<string>>)},
			{"List<string[]>",typeof(List<string[]>)},
			{"MonitorDetails",typeof(USE_DisplayManagement.MonitorDetails)},
			{"ScreenDetails",typeof(USE_DisplayManagement.ScreenDetails)},
			// {"ExptParameters",typeof(FLU_Common_Namespace.ExptParameters)},
			// {"BlockDef[]", typeof(FLU_Common_Namespace.BlockDef[])},
			{"Vector3", typeof(Vector3) },
			{"Vector2", typeof(Vector2) },
			{"Color", typeof(Color) } };
	}
}
