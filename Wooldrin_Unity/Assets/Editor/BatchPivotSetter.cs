using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.Sprites; // Required for ISpriteEditorDataProvider
using System.Collections.Generic;

public class BatchPivotSetter : EditorWindow
{
    private float customX = 0.5f;
    private float customY = 0.15f;

    [MenuItem("Wooldrin Tools/Set Custom Pivot (User Input)")]
    public static void ShowWindow()
    {
        GetWindow<BatchPivotSetter>("Pivot Setter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Set Custom Pivot for Selected Sprites", EditorStyles.boldLabel);

        customX = EditorGUILayout.FloatField("Pivot X (0-1):", customX);
        customY = EditorGUILayout.FloatField("Pivot Y (0-1):", customY);

        if (GUILayout.Button("Apply to Selected Textures"))
        {
            ApplyPivots();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Select your spritesheet texture in the Project window first, then click Apply. 0.5 is Center, 0 is Bottom/Left.", MessageType.Info);
    }

    private void ApplyPivots()
    {
        var selectedTextures = Selection.GetFiltered<Texture2D>(SelectionMode.DeepAssets);

        if (selectedTextures.Length == 0)
        {
            Debug.LogError("Wooldrin Tools: No textures selected! Please select your wall spritesheet in the Project window.");
            return;
        }

        Vector2 pivotValue = new Vector2(customX, customY);

        foreach (var texture in selectedTextures)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);

            if (dataProvider != null)
            {
                dataProvider.InitSpriteEditorDataProvider();

                var spriteRects = dataProvider.GetSpriteRects();

                for (int i = 0; i < spriteRects.Length; i++)
                {
                    spriteRects[i].alignment = SpriteAlignment.Custom;
                    spriteRects[i].pivot = pivotValue;
                }

                dataProvider.SetSpriteRects(spriteRects);
                dataProvider.Apply();

                importer.SaveAndReimport();

                Debug.Log($"Wooldrin Tools: Successfully updated {spriteRects.Length} sprites in {texture.name} to Custom Pivot {pivotValue}.");
            }
        }
    }
}