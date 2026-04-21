// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz

using UnityEditor;
using UnityEngine;

namespace realvirtual
{
    public static class MigrateTrailMaterial
    {
        #if REALVIRTUAL_DEV
        [MenuItem("realvirtual DEV/Migrate Trail Materials to SoftTrail Shader")]
        #endif
        public static void Migrate()
        {
            var shader = Shader.Find("realvirtual/Trail/SoftTrail");
            if (shader == null)
            {
                Debug.LogError("Shader 'realvirtual/Trail/SoftTrail' not found. Make sure SoftTrail.shader is compiled.");
                return;
            }

            string[] materialPaths = new[]
            {
                "Assets/realvirtual/Professional/PathTracer/Materials/TCPTrailMaterial.mat",
                "Assets/realvirtual/Professional/IK/Materials/TCPTrailMaterial.mat"
            };

            int migrated = 0;
            foreach (var path in materialPaths)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    Debug.LogWarning($"Material not found at {path}");
                    continue;
                }

                if (material.shader == shader)
                {
                    Debug.Log($"Material already uses SoftTrail shader: {path}");
                    continue;
                }

                var oldBaseColor = material.HasColor("_BaseColor") ? material.GetColor("_BaseColor") : new Color(0, 1, 1, 0.8f);
                var oldOutlineColor = material.HasColor("_OutlineColor") ? material.GetColor("_OutlineColor") : Color.black;
                var oldOutlineWidth = material.HasFloat("_OutlineWidth") ? material.GetFloat("_OutlineWidth") : 0.1f;

                material.shader = shader;
                material.SetColor("_BaseColor", oldBaseColor);
                material.SetFloat("_Softness", 0.3f);
                material.SetColor("_OutlineColor", oldOutlineColor);
                material.SetFloat("_OutlineWidth", Mathf.Clamp(oldOutlineWidth, 0f, 0.5f));
                material.renderQueue = 3000;

                EditorUtility.SetDirty(material);
                migrated++;
                Debug.Log($"Migrated material to SoftTrail shader: {path}");
            }

            if (migrated > 0)
            {
                AssetDatabase.SaveAssets();
                Debug.Log($"Migration complete: {migrated} material(s) updated.");
            }
            else
            {
                Debug.Log("No materials needed migration.");
            }
        }
    }
}
