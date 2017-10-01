using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameObject))]
public class GameObjectInspectorEx : Editor
{
    private Editor m_GameObjectInspector;
    
    private string[] m_PreviewModeDesc = new string[] {"模型", "UV1", "UV2", "UV3", "UV4"};
    
    private int m_PreviewMode;

    [SerializeField] private UVPreview m_UVPreview;

    void OnEnable()
    {
        System.Type gameObjectorInspectorType = typeof (Editor).Assembly.GetType("UnityEditor.GameObjectInspector");
        m_GameObjectInspector = Editor.CreateEditor(target, gameObjectorInspectorType);
        m_UVPreview = new UVPreview();
        if (target)
            m_UVPreview.Add((GameObject) target, true);
    }

    void OnDisable()
    {
        if (m_GameObjectInspector)
            DestroyImmediate(m_GameObjectInspector);
        m_GameObjectInspector = null;
        if (m_UVPreview != null)
            m_UVPreview.Release();
        m_UVPreview = null;
    }

    public override void OnInspectorGUI()
    {
        m_GameObjectInspector.OnInspectorGUI();
    }

    public override bool HasPreviewGUI()
    {
        return m_GameObjectInspector.HasPreviewGUI();
    }

    public override void DrawPreview(Rect previewArea)
    {
        GUI.Box(new Rect(previewArea.x, previewArea.y, previewArea.width, 17), string.Empty, GUI.skin.FindStyle("toolbar"));
        
        m_PreviewMode = GUI.Toolbar(new Rect(previewArea.x + 5, previewArea.y, 60*4, 17), m_PreviewMode,
            m_PreviewModeDesc, GUI.skin.FindStyle("toolbarbutton"));
        Rect previewRect = new Rect(previewArea.x, previewArea.y + 17, previewArea.width, previewArea.height - 17);
        if (m_PreviewMode == 0)
            m_GameObjectInspector.DrawPreview(previewRect);
        else
        {
            m_UVPreview.DrawPreview(previewRect, Color.green, (UVPreview.UVIndex) (m_PreviewMode - 1), false);
        }
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        m_GameObjectInspector.OnPreviewGUI(r, background);
    }

    public override string GetInfoString()
    {
        return m_GameObjectInspector.GetInfoString();
    }

    public override GUIContent GetPreviewTitle()
    {
        return m_GameObjectInspector.GetPreviewTitle();
    }

    public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
    {
        m_GameObjectInspector.OnInteractivePreviewGUI(r, background);
    }

    public override void OnPreviewSettings()
    {
        m_GameObjectInspector.OnPreviewSettings();
    }

    public override void ReloadPreviewInstances()
    {
        m_GameObjectInspector.ReloadPreviewInstances();
    }

    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        return m_GameObjectInspector.RenderStaticPreview(assetPath, subAssets, width, height);
    }

    public override bool RequiresConstantRepaint()
    {
        return m_GameObjectInspector.RequiresConstantRepaint();
    }

    public override bool UseDefaultMargins()
    {
        return m_GameObjectInspector.UseDefaultMargins();
    }
}
