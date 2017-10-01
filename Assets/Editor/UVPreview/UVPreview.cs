using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[System.Serializable]
public class UVPreview
{
    /// <summary>
    /// UV索引
    /// </summary>
    public enum UVIndex
    {
        UV,
        UV2,
        UV3,
        UV4,
    }

    /// <summary>
    /// UVData列表
    /// </summary>
    public List<UVData> UVDatas { get { return m_UVDatas; } }
    /// <summary>
    /// UV托盘坐标
    /// </summary>
    public Vector2 uvPanelPosition = Vector2.zero;
    /// <summary>
    /// UV托盘缩放
    /// </summary>
    public Vector2 uvPanelScale = Vector2.one;
    /// <summary>
    /// 当前光照贴图索引
    /// </summary>
    public int LightMapIndex { get; private set; }
    /// <summary>
    /// 是否绘制方向光照贴图
    /// </summary>
    public bool DirectionalLightMap { get; private set; }
    /// <summary>
    /// 光照贴图布局模式：默认为仅绘制物体UV，开启光照贴图布局将使用LightMapOffsetScale来显示UV在光照贴图上的位置
    /// </summary>
    public bool lightMapLayoutMode;
    /// <summary>
    /// 棋盘格：UV托盘背景
    /// </summary>
    [SerializeField]
    private Mesh m_CheckerBoard;
    /// <summary>
    /// 棋盘格材质
    /// </summary>
    [SerializeField]
    private Material m_CheckerBoardMaterial;

    private bool m_IsDragging;
    /// <summary>
    /// 描边材质
    /// TODO OpenGL es2.0渲染模式下可能不支持
    /// </summary>
    [SerializeField]
    private Material m_BoardLineMaterial;

    [SerializeField]
    private List<UVData> m_UVDatas = new List<UVData>();

    private const string kCheckerBoardShader = "Hidden/Internal/GUI/CheckerBoard";
    private const string kBoardLineShader = "Hidden/Internal/GUI/BoardLine";

    public UVPreview(bool lightMapMode = false)
    {
        m_CheckerBoardMaterial = new Material(Shader.Find(kCheckerBoardShader));
        m_CheckerBoardMaterial.hideFlags = HideFlags.HideAndDontSave;

        m_BoardLineMaterial = new Material(Shader.Find(kBoardLineShader));
        m_BoardLineMaterial.hideFlags = HideFlags.HideAndDontSave;

        m_CheckerBoard = new Mesh();
        m_CheckerBoard.vertices = new Vector3[]
        {new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0)};
        m_CheckerBoard.uv = new Vector2[]
        {new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0)};
        m_CheckerBoard.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        m_CheckerBoard.hideFlags = HideFlags.HideAndDontSave;
        this.lightMapLayoutMode = lightMapMode;
    }

    public void Release()
    {
        Clear();
        if (m_CheckerBoard)
            Object.DestroyImmediate(m_CheckerBoard);
        if (m_CheckerBoardMaterial)
            Object.DestroyImmediate(m_CheckerBoardMaterial);
        if (m_BoardLineMaterial)
            Object.DestroyImmediate(m_BoardLineMaterial);
        m_CheckerBoard = null;
        m_CheckerBoardMaterial = null;
        m_CheckerBoardMaterial = null;
    }

    public void Add(GameObject gameObject, bool containChilds)
    {
        if (containChilds)
        {
            var collect = GetRenderers(gameObject);
            if (collect != null)
            {
                for (int i = 0; i < collect.Count; i++)
                {
                    var go = GetRender(collect[i]);
                    CreateUVData(go);
                }
            }
        }
        else
        {
            GameObject go = GetRender(gameObject);
            CreateUVData(go);
        }
    }

    public void Clear()
    {
        foreach (UVData uvdata in m_UVDatas)
        {
            uvdata.Release();
        }
        m_UVDatas.Clear();
        m_UVDatas = null;
    }

    /// <summary>
    /// 绘制UV界面
    /// </summary>
    /// <param name="rect"></param>
    public void DrawPreview(Rect rect, Color color, UVIndex uvIndex, bool drawVertexColor)
    {
        DrawCheckerBoard(rect);//绘制棋盘格
        if (drawVertexColor)
            DrawVertexColor(rect, (int)uvIndex);//绘制顶点色
        DrawUV(rect, (int)uvIndex, color);//绘制UV
        ListenEvent(rect);
    }

    /// <summary>
    /// 清除UV托盘的贴图（包括光照图）
    /// </summary>
    public void ClearTexture()
    {
        m_CheckerBoardMaterial.SetFloat("_Alpha", 0);
    }

    /// <summary>
    /// 改变贴图
    /// </summary>
    /// <param name="texture"></param>
    public void SetTexture(Texture2D texture)
    {
        if (texture)
        {
            m_CheckerBoardMaterial.SetTexture("_MainTex", texture);
            m_CheckerBoardMaterial.SetFloat("_Alpha", 1);
        }
    }

    /// <summary>
    /// 改变光照贴图
    /// </summary>
    /// <param name="lightMapIndex">光照贴图索引</param>
    /// <param name="directional">是否为方向光照贴图</param>
    public void SetLightMap(int lightMapIndex, bool directional)
    {
        LightMapIndex = lightMapIndex;
        DirectionalLightMap = directional;
        if (LightMapIndex >= 0 && LightMapIndex < LightmapSettings.lightmaps.Length)
        {
            LightmapData md = LightmapSettings.lightmaps[LightMapIndex];
            lightMapLayoutMode = true;
            if (DirectionalLightMap)
                SetTexture(md.lightmapDir);
            else
                SetTexture(md.lightmapColor);
        }
    }

    /// <summary>
    /// 重置UV托盘位置
    /// </summary>
    public void ResetUVPanel()
    {
        uvPanelPosition = Vector2.zero;
        uvPanelScale = Vector2.one;
    }

    /// <summary>
    /// 绘制棋盘格
    /// </summary>
    /// <param name="rect"></param>
    private void DrawCheckerBoard(Rect rect)
    {
        //该矩阵将UV托盘中的坐标转换为一个无量纲空间，其坐标在托盘内xy范围均为0-1，用于裁剪托盘外渲染的物体
        m_CheckerBoardMaterial.SetMatrix("clipMatrix", GetGUIClipMatrix(rect));

        //该矩阵用于实际绘制棋盘格
        Matrix4x4 matrix = RefreshMatrix(rect);
        m_CheckerBoardMaterial.SetPass(0);
        Graphics.DrawMeshNow(m_CheckerBoard, matrix);
    }

    private void DrawUV(Rect rect, int uvID, Color color)
    {
        m_BoardLineMaterial.SetColor("_Color", color);
        DrawUVMesh(rect, uvID, 0);
    }

    private void DrawVertexColor(Rect rect, int uvID)
    {
        DrawUVMesh(rect, uvID, 1);
    }

    /// <summary>
    /// 绘制UV
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="uvID">UVID</param>
    private void DrawUVMesh(Rect rect, int uvID, int pass)
    {

        Matrix4x4 matrix = default(Matrix4x4);
        //计算并设置裁剪矩阵
        m_BoardLineMaterial.SetMatrix("clipMatrix", GetGUIClipMatrix(rect));

        //非光照贴图布局模式下直接计算绘制矩阵
        if (!lightMapLayoutMode)
            matrix = RefreshMatrix(rect);
        for (int i = 0; i < m_UVDatas.Count; i++)
        {
            if (m_UVDatas[i].disable)
                continue;
            if (m_UVDatas[i].target == null)
                continue;
            m_BoardLineMaterial.SetPass(pass);
            Mesh mesh = null;
            if (lightMapLayoutMode)
            {
                //光照贴图布局模式下需要根据每个Renderer的LightMapScaleOffset来计算绘制矩阵
                Renderer renderer = m_UVDatas[i].target.GetComponent<Renderer>();
                if (renderer.lightmapIndex != LightMapIndex)
                    continue;
                Vector4 lmST = renderer.lightmapScaleOffset;

                matrix =
                    RefreshMatrix(rect, lmST.z, lmST.w, lmST.x, lmST.y);
                mesh = m_UVDatas[i].uvMeshs[1];
                if (mesh == null)
                    mesh = m_UVDatas[i].uvMeshs[0];
            }
            else
            {
                mesh = m_UVDatas[i].uvMeshs[uvID];
            }
            if (mesh)
                Graphics.DrawMeshNow(mesh, matrix);
        }
    }

    private void CreateUVData(GameObject gameObject)
    {
        if (gameObject && gameObject.activeSelf)
        {
            UVData uvData = UVData.CreateUVData(gameObject);
            if (uvData != null)
                m_UVDatas.Add(uvData);
        }
    }

    private GameObject GetRender(GameObject gameObject)
    {
        SkinnedMeshRenderer skinned = gameObject.GetComponent<SkinnedMeshRenderer>();
        if (skinned)
            return gameObject;
        MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        if (mf)
            return gameObject;
        return null;
    }

    private List<GameObject> GetRenderers(GameObject gameObject, List<GameObject> collect = null)
    {
        if (gameObject == null)
            return collect;
        var go = GetRender(gameObject);
        if (go != null)
        {
            if (collect == null)
                collect = new List<GameObject>();
            collect.Add(go);
        }
        if (gameObject.transform.childCount > 0)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                var child = gameObject.transform.GetChild(i);
                collect = GetRenderers(child.gameObject, collect);
            }
        }
        return collect;
    }

    private void ListenEvent(Rect rect)
    {
        if (Event.current.type == EventType.MouseDown && Event.current.button == 2)
        {
            if (rect.Contains(Event.current.mousePosition))
            {
                m_IsDragging = true;
                Event.current.Use();
            }
        }
        if (Event.current.type == EventType.ScrollWheel)
        {
            if (rect.Contains(Event.current.mousePosition))
            {
                float dt = Event.current.delta.y;
                uvPanelScale -= Vector2.one * dt * 0.03f;
                Event.current.Use();
            }
        }
        if (Event.current.type == EventType.MouseDrag && m_IsDragging)
        {
            uvPanelPosition += Event.current.delta;
            Event.current.Use();
        }
        if (Event.current.type == EventType.MouseUp && m_IsDragging)
        {
            m_IsDragging = false;
            Event.current.Use();
        }
    }

    /// <summary>
    /// 绘制矩阵计算
    /// </summary>
    /// <param name="r"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <returns></returns>
    private Matrix4x4 RefreshMatrix(Rect r, float x = 0, float y = 0, float w = 1, float h = 1)
    {
        //该矩阵会在绘制区域r发生变化时更新，并根据r的宽高比自适应绘制区域
        r.x = r.x - (uvPanelScale.x - 1) * r.width / 2;
        r.y = r.y - (uvPanelScale.y - 1) * r.height / 2;
        r.x += uvPanelPosition.x;
        r.y += uvPanelPosition.y;
        r.width *= uvPanelScale.x;
        r.height *= uvPanelScale.y;
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
