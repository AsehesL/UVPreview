using UnityEngine;
using System.Collections;

[System.Serializable]
public class UVData
{

    /// <summary>
    /// 目标对象
    /// </summary>
    public GameObject target { get { return m_Target; } }
    /// <summary>
    /// 各UV对应的Mesh
    /// </summary>
    public Mesh[] uvMeshs { get { return m_UvMeshes; } }
    /// <summary>
    /// 绘制开关-是否在UVPreview中绘制
    /// </summary>
    public bool disable;

    [SerializeField]
    private Mesh[] m_UvMeshes;

    [SerializeField]
    private GameObject m_Target;

    private UVData() { }

    public static UVData CreateUVData(GameObject target)
    {
        Mesh targetMesh = null;
        SkinnedMeshRenderer skin = target.GetComponent<SkinnedMeshRenderer>();
        MeshFilter meshFilter = target.GetComponent<MeshFilter>();
        if (skin)
            targetMesh = skin.sharedMesh;
        else if (meshFilter)
            targetMesh = meshFilter.sharedMesh;
        if (targetMesh == null)
            return null;
        UVData dt = new UVData();
        dt.m_Target = target;
        dt.m_UvMeshes = new Mesh[4];
        dt.m_UvMeshes[0] = CreateUVMesh(targetMesh, 0);
        dt.m_UvMeshes[1] = CreateUVMesh(targetMesh, 1);
        dt.m_UvMeshes[2] = CreateUVMesh(targetMesh, 2);
        dt.m_UvMeshes[3] = CreateUVMesh(targetMesh, 3);
        return dt;
    }

    public void Release()
    {
        for (int i = 0; i < m_UvMeshes.Length; i++)
        {
            if (m_UvMeshes[i])
                Object.DestroyImmediate(m_UvMeshes[i]);
        }
        m_UvMeshes = null;
    }

    private static Mesh CreateUVMesh(Mesh targetMesh, int uvChannel)
    {
        Vector2[] uv = null;
        if (uvChannel == 0)
            uv = targetMesh.uv;
        else if (uvChannel == 1)
            uv = targetMesh.uv2;
        else if (uvChannel == 2)
            uv = targetMesh.uv3;
        else if (uvChannel == 3)
            uv = targetMesh.uv4;
        if (uv == null || uv.Length <= 0)
            return null;
        Mesh result = new Mesh();
        result.hideFlags = HideFlags.HideAndDontSave;
        Vector3[] vertexList = new Vector3[targetMesh.vertexCount];
        Color[] targetColors = targetMesh.colors;
        Color[] colors = new Color[targetColors.Length];
        for (int i = 0; i < targetMesh.vertexCount; i++)
        {
            vertexList[i] = new Vector3(uv[i].x, uv[i].y, 0);
        }
        for (int i = 0; i < targetColors.Length; i++)
        {
            colors[i] = targetColors[i];
        }
        result.vertices = vertexList;
        result.colors = colors;
        for (int j = 0; j < targetMesh.subMeshCount; j++)
        {
            result.SetIndices(targetMesh.GetIndices(j), MeshTopology.Triangles, j);
        }
        return result;
    }
}
