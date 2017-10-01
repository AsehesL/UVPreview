using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MatTexTest : EditorWindow
{
    private Material m_Material;

    private MaterialEditor m_MatEditor;

    private MaterialProperty[] m_Properties;

    void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        m_Material =
            EditorGUI.ObjectField(new Rect(0, 0, 100, 20), "Mat:", m_Material, typeof (Material), false) as Material;
        if (EditorGUI.EndChangeCheck())
        {
            if (m_Material == null && m_MatEditor)
            {
                DestroyImmediate(m_MatEditor);
                m_MatEditor = null;
                m_Properties = null;
            }
            else if (m_Material)
            {
                if (m_MatEditor)
                    DestroyImmediate(m_MatEditor);
                m_MatEditor = Editor.CreateEditor(m_Material, typeof (MaterialEditor)) as MaterialEditor;
                m_Properties = MaterialEditor.GetMaterialProperties(new Object[] { m_Material });
            }
        }

        if (m_Properties != null)
        {
            for (int i = 0; i < m_Properties.Length; i++)
            {
                //m_Properties[i].textureDimension = UnityEngine.Rendering.TextureDimension.
            }
        }
    }
}
