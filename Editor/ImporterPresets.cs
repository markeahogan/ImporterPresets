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
            string[] presets = Directory.GetFiles(directory, "Default_*.preset", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < presets.Length; i++)
            {
                var preset = AssetDatabase.LoadAssetAtPath<Preset>(presets[i]);
                if (preset.ApplyTo(importer)) { return true; }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the folder has a parent directory and sets the contents of 'directory' to the parent
        /// </summary>
        private bool GetParentDirectory(ref string directory)
        {
            if (directory.LastIndexOf('\\') is int slash && slash != -1)
            {
                directory = directory.Substring(0, slash);
                return true;
            }
            return false;
        }
    }
}