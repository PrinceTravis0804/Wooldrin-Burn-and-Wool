using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CompositeCollider2D))]
public class TilemapShadowGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    public bool clearOldShadows = true;

    [Header("Visual Tweaks")]
    [Tooltip("Nudge the entire shadow group up or down to align with your mounds.")]
    public float verticalNudge = 0f;

    public void GenerateShadowsFromCollider()
    {
        CompositeCollider2D compositeCollider = GetComponent<CompositeCollider2D>();

        if (compositeCollider.pathCount == 0)
        {
            Debug.LogError("Wooldrin Tools: CompositeCollider2D has 0 paths! Make sure your Tilemap has tiles and the collider is 'Used by Composite'.");
            return;
        }

        if (clearOldShadows)
        {
            // Specifically look for both "Shadow_Shape_" and "Grid_Shadow_" naming conventions
            List<GameObject> childrenToDelete = new List<GameObject>();
            for (int i = 0; i < transform.childCount; i++)
            {
                string childName = transform.GetChild(i).name;
                if (childName.StartsWith("Shadow_Shape_") || childName.StartsWith("Grid_Shadow_"))
                {
                    childrenToDelete.Add(transform.GetChild(i).gameObject);
                }
            }

            foreach (var child in childrenToDelete)
            {
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }
        }

        // Loop through every distinct path (shape) in the composite collider
        for (int i = 0; i < compositeCollider.pathCount; i++)
        {
            Vector2[] pathVertices = new Vector2[compositeCollider.GetPathPointCount(i)];
            compositeCollider.GetPath(i, pathVertices);

            // Create a new visible child object for each separate wall section
            GameObject shadowObject = new GameObject("Shadow_Shape_" + i);
            shadowObject.transform.SetParent(transform);

            // Apply the nudge to keep it separate from physics logic
            shadowObject.transform.localPosition = new Vector3(0, verticalNudge, 0);
            shadowObject.hideFlags = HideFlags.None;

#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(shadowObject, "Create Shadow Shape");
#endif

            ShadowCaster2D shadowCaster = shadowObject.AddComponent<ShadowCaster2D>();
            shadowCaster.useRendererSilhouette = false;
            shadowCaster.selfShadows = false;

            // Use Reflection to set the shape path, as it's not exposed in the standard API
            var fieldInfo = typeof(ShadowCaster2D).GetField("m_ShapePath",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            Vector3[] pathVerticesV3 = new Vector3[pathVertices.Length];
            for (int j = 0; j < pathVertices.Length; j++)
                pathVerticesV3[j] = new Vector3(pathVertices[j].x, pathVertices[j].y, 0);

            fieldInfo.SetValue(shadowCaster, pathVerticesV3);

            // Force a hash update so the shadow volume geometry updates in the editor
            var hashField = typeof(ShadowCaster2D).GetField("m_ShapePathHash",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            if (hashField != null) hashField.SetValue(shadowCaster, Random.Range(1, 99999));
        }

        Debug.Log($"Wooldrin Tools: Generated {compositeCollider.pathCount} shadow shapes based on CompositeCollider2D paths!");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TilemapShadowGenerator))]
public class TilemapShadowGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TilemapShadowGenerator gen = (TilemapShadowGenerator)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Shadows from Collider", GUILayout.Height(30)))
        {
            gen.GenerateShadowsFromCollider();
            SceneView.RepaintAll();
        }
        EditorGUILayout.HelpBox("This creates a shadow shape for every island of tiles in your Composite Collider. It's much cleaner than the grid-scan method.", MessageType.Info);
    }
}
#endif