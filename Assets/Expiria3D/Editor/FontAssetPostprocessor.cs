namespace Expiria3DSpace
{
    using UnityEngine;
    using UnityEditor;
    using System.IO;
    using TMPro;

    public class FontAssetPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (string assetPath in importedAssets)
            {
                // Verifica si el archivo importado es una fuente TrueType (.ttf) u OpenType (.otf)
                if (assetPath.EndsWith(".ttf") || assetPath.EndsWith(".otf"))
                {
                    // Obtiene la fuente importada
                    Font font = AssetDatabase.LoadAssetAtPath<Font>(assetPath);
                    if (font != null)
                    {
                        string fontAssetPath = Path.ChangeExtension(assetPath, ".asset");

                        // Verifica si ya existe un TMP Font Asset para esta fuente
                        TMP_FontAsset existingFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
                        if (existingFontAsset == null)
                        {
                            // Crea un nuevo TMP Font Asset
                            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(font);

                            // Guarda el TMP Font Asset en el mismo directorio que la fuente original
                            AssetDatabase.CreateAsset(fontAsset, fontAssetPath);
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
            }
        }
    }
}