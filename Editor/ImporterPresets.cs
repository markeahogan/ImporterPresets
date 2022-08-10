using System.IO;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace PopupAsylum.ImporterPresets
{
    /// <summary>
    /// Uses a naming convention to find Presets for the asset type being imported
    /// Presets should be named "Default_{something}.preset" where {something} describes what the default is/does
    /// </summary>
    public class ImporterPresets : AssetPostprocessor
    {
        /// <summary>
        /// Finds and applies a 'default' Preset to the asset importer
        /// </summary>
        private void OnPreprocessAsset()
        {
            // only apply if the asset hasn't been imported before
            if (assetImporter.importSettingsMissing)
            {
                var directory = Path.GetDirectoryName(assetPath);
                do if (ApplyDefaultPreset(assetImporter, directory)) { return; }
                while (GetParentDirectory(ref directory)); // iterate up the folders till we reach 'Assets'
            }
        }

        /// <summary>
        /// Loads and Applys a preset like "Default_TextureImporter.preset" in the directory, returns true if successful
        /// </summary>
        private static bool ApplyDefaultPreset(Object importer, string directory)
        {
            if (TryGetDefaultPreset(importer, directory, out var preset))
            {
                preset.ApplyTo(importer);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Loads and Applys a preset like "Default_TextureImporter.preset" in the directory, returns true if successful
        /// </summary>
        private static bool TryGetDefaultPreset(Object importer, string directory, out Preset preset)
        {
            string[] presets = Directory.GetFiles(directory, "Default_*.preset", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < presets.Length; i++)
            {
                preset = AssetDatabase.LoadAssetAtPath<Preset>(presets[i]);
                if (preset.CanBeAppliedTo(importer)) { return true; }
            }
            preset = null;
            return false;
        }

        /// <summary>
        /// Returns true if the folder has a parent directory and sets the contents of 'directory' to the parent
        /// </summary>
        private static bool GetParentDirectory(ref string directory)
        {
            if (directory.LastIndexOf('\\') is int slash && slash != -1)
            {
                directory = directory.Substring(0, slash);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Shows in the ProjectWindow ContextMenu and applies the preset to any assets for which it is the default
        /// </summary>
        [MenuItem("Assets/Reapply Default Preset to Folder")]
        private static void ApplyPresetToFolder()
        {
            if (Selection.activeObject is Preset preset)
            {
                ApplyDefaultPresetToFolders(preset);
            }
        }

        [MenuItem("Assets/Reapply Default Preset to Folder", true)]
        private static bool ApplyPresetToFolderValidation()
        {
            return Selection.activeObject is Preset && Selection.activeObject.name.StartsWith("Default_");
        }

        /// <summary>
        /// Shows in the ProjectWindow ContextMenu and applies the preset to any assets for which it is the default
        /// </summary>
        [MenuItem("CONTEXT/Preset/Reapply Default Preset to Folder")]
        static void ApplyPresetToFolder(MenuCommand command)
        {
            if (command.context is Preset preset)
            {
                ApplyDefaultPresetToFolders(preset);
            }
        }

        [MenuItem("CONTEXT/Preset/Reapply Default Preset to Folder", true)]
        private static bool ApplyPresetToFolderValidation(MenuCommand command)
        {
            return command.context is Preset preset && preset.name.StartsWith("Default_");
        }

        /// <summary>
        /// Applies the preset to any assets for which it is the default
        /// </summary>
        /// <param name="preset"></param>
        private static void ApplyDefaultPresetToFolders(Preset preset)
        {
            var presetPath = AssetDatabase.GetAssetPath(preset);
            var presetDirectory = Path.GetDirectoryName(presetPath);
            var projectPath = Path.GetFullPath(Application.dataPath + "/../");
            var projectPathLength = projectPath.Length;

            string[] fileEntries = Directory.GetFiles(projectPath + presetDirectory, "*", SearchOption.AllDirectories);

            foreach (string fileName in fileEntries)
            {
                string assetPath = fileName.Substring(projectPathLength);

                var importer = AssetImporter.GetAtPath(assetPath);
                if (importer == null) continue;

                var importerDirectory = Path.GetDirectoryName(assetPath);

                do if (TryGetDefaultPreset(importer, importerDirectory, out var defaultPreset))
                    {
                        if (defaultPreset == preset) preset.ApplyTo(importer);
                        if (defaultPreset == preset)
                        {
                            preset.ApplyTo(importer);
                            importer.SaveAndReimport();
                        }
                        break;
                    }
                while (GetParentDirectory(ref importerDirectory)); // iterate up the folders till we reach 'Assets'
            }
        }
    }
}