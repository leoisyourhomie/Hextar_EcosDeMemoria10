namespace Expiria3DSpace
{
    using UnityEngine;
    using UnityEditor;
    using System.IO;
    using System.IO.Compression; // Para manejar archivos ZIP

    public class ArchiveAssetPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (string assetPath in importedAssets)
            {
                if (assetPath.EndsWith(".zip"))
                {
                    UnzipFile(assetPath);
                }
            }
        }

        static void UnzipFile(string assetPath)
        {
            string fullPath = Path.GetFullPath(assetPath);
            string extractPath = Path.GetDirectoryName(fullPath);

            try
            {
                // Descomprimir el archivo en el directorio actual
                ZipFile.ExtractToDirectory(fullPath, extractPath);

                // Eliminar el archivo ZIP original using Unity's AssetDatabase
                if (!AssetDatabase.DeleteAsset(assetPath))
                {
                    Debug.LogWarning($"Failed to delete asset at path '{assetPath}'");
                }

                // Refrescar el AssetDatabase para que Unity reconozca los nuevos archivos
                AssetDatabase.Refresh();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error al descomprimir el archivo '{assetPath}': {ex.Message}");
            }
        }
    }
}