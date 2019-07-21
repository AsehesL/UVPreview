using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class UVPreview
{
    /// <summary>
    /// 棋盘格材质
    /// </summary>
    private static Material sCheckerBoardMaterial;
    /// <summary>
    /// UV材质
    /// </summary>
    private static Material sUVMaterial;

    /// <summary>
    /// 棋盘格：UV托盘背景
    /// </summary>
    private static Mesh sCheckerBoard;

    private static Material CheckerBoardMaterial
    {
        get
        {
            if (sCheckerBoardMaterial == null)
            {
                sCheckerBoardMaterial = new Material(EditorGUIUtility.Load(kCheckerBoardShader) as Shader);
                sCheckerBoardMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            return sCheckerBoardMaterial;
        }
    }

    private static Material UVMaterial
    {
        get
        {
            if (sUVMaterial == null)
            {
                sUVMaterial = new Material(EditorGUIUtility.Load(kUVShader) as Shader);
                sUVMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            return sUVMaterial;
        }
    }

    private static Mesh CheckerBoard
    {
        get {
            if (sCheckerBoard == null)
            {
                sCheckerBoard = new Mesh();
                sCheckerBoard.vertices = new Vector3[]
                {new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0)};
                sCheckerBoard.uv = new Vector2[]
                {new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0)};
                sCheckerBoard.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
                sCheckerBoard.hideFlags = HideFlags.HideAndDontSave;
            }

            return sCheckerBoard;
        }
    }

    private const string kCheckerBoardShader = "InternalShaders/GUI/CheckerBoard.shader";
    private const string kUVToTextureShader = "InternalShaders/UVPreview/UVToTexture.shader";
    private const string kUVShader = "InternalShaders/GUI/UVRender.shader";

    private class UVPreviewStateInfo
    {
        public bool isDragging = false;
    }

    public static Vector4 DrawUV(Rect rect, Vector4 offsetScale, GameObject gameObject, int uvIndex, bool lightMapLayoutMode, int lightMapIndex, Color color, Texture2D texture, bool drawVertexColor)
    {
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        var state = (UVPreviewStateInfo)GUIUtility.GetStateObject(
            typeof(UVPreviewStateInfo),
            controlID);

        switch (Event.current.GetTypeForControl(controlID))
        {
            case EventType.Repaint:
                DrawCheckerBoard(rect, offsetScale, texture);//绘制棋盘格
                if (drawVertexColor)
                    DrawVertexColorMesh(rect, offsetScale, gameObject, uvIndex, lightMapLayoutMode, lightMapIndex);//绘制顶点色
                DrawUVMesh(rect, offsetScale, gameObject, uvIndex, lightMapLayoutMode, lightMapIndex, color);//绘制UV
                break;
            case EventType.MouseDown:
                if (Event.current.button == 2 && rect.Contains(Event.current.mousePosition))
                {
                    state.isDragging = true;
                }
                break;
            case EventType.ScrollWheel:
                if (rect.Contains(Event.current.mousePosition))
                {
                    float dt = Event.current.delta.y;
                    offsetScale.z -= dt * 0.03f;
                    offsetScale.w -= dt * 0.03f;
                    Event.current.Use();
                }
                break;
            case EventType.MouseDrag:
                if (state.isDragging)
                {
                    offsetScale.x += Event.current.delta.x;
                    offsetScale.y += Event.current.delta.y;
                    Event.current.Use();
                }
                break;
            case EventType.MouseUp:
                if (state.isDragging)
                {
                    state.isDragging = false;
                    Event.current.Use();
                }
                break;
        }

        return offsetScale;
    }

    public static Texture2D RenderUV(GameObject gameObject, int uvIndex, Color color)
    {
        Mesh mesh = GetMesh(gameObject);
        if (!mesh)
            return null;

        Material renderMat = new Material(EditorGUIUtility.Load(kUVToTextureShader) as Shader);
        renderMat.SetColor("_Color", color);

        if (!CheckUV(mesh, uvIndex))
            return null;
        renderMat.SetFloat("_UVIndex", 0.5f + uvIndex);

        renderMat.SetPass(0);

        Texture2D result = new Texture2D(1024, 1024);

        RenderTexture rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGBFloat);
        RenderTexture active = RenderTexture.active;

        RenderTexture.active = rt;
        GL.Clear(true, true, new Color(0, 0, 0, 0));
        Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
        result.ReadPixels(new Rect(0, 0, 1024, 1024), 0, 0);
        result.Apply();
        RenderTexture.active = active;

        Object.DestroyImmediate(renderMat);
        Object.DestroyImmediate(rt);

        return result;

    }

    /// <summary>
    /// 绘制棋盘格
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="offsetScale"></param>
    private static void DrawCheckerBoard(Rect rect, Vector4 offsetScale, Texture2D backGround)
    {
        //该矩阵将UV托盘中的坐标转换为一个无量纲空间，其坐标在托盘内xy范围均为0-1，用于裁剪托盘外渲染的物体
        CheckerBoardMaterial.SetMatrix("clipMatrix", GetGUIClipMatrix(rect));
        if (backGround)
        {
            CheckerBoardMaterial.SetTexture("_MainTex", backGround);
            CheckerBoardMaterial.SetFloat("_Alpha", 1.0f);
        }
        else
        {
            CheckerBoardMaterial.SetTexture("_MainTex", null);
            CheckerBoardMaterial.SetFloat("_Alpha", 0.0f);
        }

        //该矩阵用于实际绘制棋盘格
        Matrix4x4 matrix = GetDrawMatrix(rect, offsetScale);
        CheckerBoardMaterial.SetPass(0);
        Graphics.DrawMeshNow(CheckerBoard, matrix);
    }

    private static void DrawUVMesh(Rect rect, Vector4 offsetScale, GameObject gameObject, int uvID, bool lightMapLayoutMode, int lightMapIndex, Color color)
    {
        UVMaterial.SetColor("_Color", color);
        DrawMesh(rect, offsetScale, gameObject, uvID, lightMapLayoutMode, lightMapIndex, 0);
    }

    private static void DrawVertexColorMesh(Rect rect, Vector4 offsetScale, GameObject gameObject, int uvID, bool lightMapLayoutMode, int lightMapIndex)
    {
        DrawMesh(rect, offsetScale, gameObject, uvID, lightMapLayoutMode, lightMapIndex, 1);
    }

    /// <summary>
    /// 绘制Mesh
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="uvID">UVID</param>
    private static void DrawMesh(Rect rect, Vector4 offsetScale, GameObject gameObject, int uvID, bool lightMapLayoutMode, int lightMapIndex, int pass)
    {
        Mesh mesh = GetMesh(gameObject);
        if(!mesh)
            return;
        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (!renderer)
            return;

        Matrix4x4 matrix = default(Matrix4x4);
        //计算并设置裁剪矩阵
        UVMaterial.SetMatrix("clipMatrix", GetGUIClipMatrix(rect));

        //UVMaterial.SetPass(pass);
        //matrix = GetDrawMatrix(rect, offsetScale);
        //Graphics.DrawMeshNow(mesh, matrix);

        //非光照贴图布局模式下直接计算绘制矩阵
        if (!lightMapLayoutMode)
            matrix = GetDrawMatrix(rect, offsetScale);
        UVMaterial.SetPass(pass);
        

        if (lightMapLayoutMode)
        {
            //光照贴图布局模式下需要根据每个Renderer的LightMapScaleOffset来计算绘制矩阵
            if (renderer.lightmapIndex != lightMapIndex)
                return;
            Vector4 lmST = renderer.lightmapScaleOffset;

            matrix =
                GetDrawMatrix(rect, offsetScale, lmST.z, lmST.w, lmST.x, lmST.y);
            if (CheckUV(mesh, 1))
            {
                UVMaterial.SetFloat("_UVIndex", 1.5f);
            }
            else
            {
                if (CheckUV(mesh, 0))
                {
                    UVMaterial.SetFloat("_UVIndex", 0.5f);
                }
                else
                {
                    return;
                }
            }
        }
        else
        {
            if (!CheckUV(mesh, uvID))
                return;
            UVMaterial.SetFloat("_UVIndex", 0.5f + uvID);
        }
        if (mesh)
            Graphics.DrawMeshNow(mesh, matrix);
        
    }

    private static Mesh GetMesh(GameObject gameObject)
    {
        if (gameObject == null)
            return null;
        SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
        Mesh mesh = null;
        if (!skinnedMeshRenderer)
        {
            MeshFilter mf = gameObject.GetComponent<MeshFilter>();
            mesh = mf ? mf.sharedMesh : null;
        }
        else
        {
            mesh = skinnedMeshRenderer.sharedMesh;
        }

        return mesh;
    }

    private static bool CheckUV(Mesh mesh, int uvIndex)
    {
        if (uvIndex < 0 || uvIndex >= 4)
            return false;
        if (!mesh)
            return false;
        if (uvIndex == 0 && mesh.uv.Length > 0)
            return true;
        if (uvIndex == 1 && mesh.uv2.Length > 0)
            return true;
        if (uvIndex == 2 && mesh.uv3.Length > 0)
            return true;
        if (uvIndex == 3 && mesh.uv4.Length > 0)
            return true;
        return false;
    }

    /// <summary>
    /// 计算绘制区域矩阵
    /// </summary>
    /// <param name="r"></param>
    /// <param name="offsetScale"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <returns></returns>
    private static Matrix4x4 GetDrawMatrix(Rect r, Vector4 offsetScale, float x = 0, float y = 0, float w = 1, float h = 1)
    {
        //该矩阵会在绘制区域r发生变化时更新，并根据r的宽高比自适应绘制区域
        r.x = r.x - (offsetScale.z - 1) * r.width / 2;
        r.y = r.y - (offsetScale.w - 1) * r.height / 2;
        r.x += offsetScale.x;
        r.y += offsetScale.y;
        r.width *= offsetScale.z;
        r.height *= offsetScale.w;
        return GetGUISquareMatrix(r, x, y, w, h);
    }

    private static Matrix4x4 GetGUISquareMatrix(Rect r, float x = 0, float y = 0, float w = 1, float h = 1)
    {
        //该矩阵会在绘制区域r发生变化时更新，并根据r的宽高比自适应绘制区域
        Matrix4x4 m_Matrix = new Matrix4x4();
        float aspect = r.width / r.height;
        if (aspect > 1)
        {
            m_Matrix.m00 = r.height * w;
            m_Matrix.m03 = r.x + r.width / 2 - r.height / 2 + r.height * x;
            m_Matrix.m11 = -r.height * h;
            m_Matrix.m13 = r.y + r.height - y * r.height;
        }
        else
        {
            m_Matrix.m00 = r.width * w;
            m_Matrix.m03 = r.x + x * r.width;
            m_Matrix.m11 = -r.width * h;
            m_Matrix.m13 = r.y + r.height / 2 + r.width / 2 - y * r.width;
        }
        m_Matrix.m33 = 1;
        return m_Matrix;
    }

    /// <summary>
    /// 通过GUI绘制区域获取GUI裁剪矩阵计算
    /// </summary>
    /// <param name="r">绘制区域</param>
    /// <returns></returns>
    public static Matrix4x4 GetGUIClipMatrix(Rect r)
    {
        //fx = (x-r.x)/r.width;
        //fy = (y-r.y)/r.height;
        Matrix4x4 matrix = new Matrix4x4();
        matrix.m00 = 1 / r.width;
        matrix.m03 = -r.x / r.width;
        matrix.m11 = 1 / r.height;
        matrix.m13 = -r.y / r.height;
        matrix.m33 = 1;
        return matrix;
    }
}
