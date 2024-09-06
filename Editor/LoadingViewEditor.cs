using System.IO;
using ContextLoaderService.Runtime;
using UnityEditor;
using UnityEngine;
using Logger = DCLogger.Runtime.Logger;

namespace Editor
{
    [CustomEditor(typeof(LoadingView))]

    public class LoadingViewEditor : UnityEditor.Editor
    {
        public string className = "LoadingViewTypes"; 
        public string namespaceName = "ContextLoader.Scripts";
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            LoadingView loadingView = (LoadingView)target;

            if (GUILayout.Button("Generate LoadingViewTypes.cs"))
            {
                GenerateStringConstants(loadingView);
            }
        }

        private void GenerateStringConstants(LoadingView loadingView)
        {
            if (loadingView == null)
            {
#if DC_LOGGING
                Logger.LogWarning("LoadingView reference is missing.", ContextLoaderLogChannels.Default);
#else
                Debug.LogWarning("LoadingView reference is missing.");
#endif
                return;
            }
            
            string classString = $"namespace {namespaceName}\n{{\n";
            classString += $"    public static class {className}\n    {{\n";
            
            foreach (string type in loadingView.LoadingViewTypes)
            {
                classString += $"        public const string {ToVariableName(type)} = \"{type}\";\n";
            }

            classString += "    }\n}\n";
            string directoryPath = Path.Combine(Application.dataPath, "ContextLoader", "Scripts");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string filePath = Path.Combine(directoryPath,$"{className}.cs");;
            File.WriteAllText(filePath, classString);

            AssetDatabase.Refresh();
        }

        private string ToVariableName(string input)
        {
            input = input.Replace(" ", "_");
            input = input.Replace("-", "_");
            input = char.ToUpper(input[0]) + input.Substring(1);
            return input;
        }
    }
}