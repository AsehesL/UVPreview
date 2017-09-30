using UnityEngine;
using System.Collections;

[System.Serializable]
public class UVData
{

    /// <summary>
    /// 目标对象
    /// </summary>
    public GameObject target;
    /// <summary>
    /// 各UV对应的Mesh
    /// </summary>
    public Mesh[] uvMeshs;
    /// <summary>
    /// UVPreview中绘制的颜色
    /// </summary>
    public Color color;
    /// <summary>
    /// 绘制开关-是否在UVPreview中绘制
    /// </summary>
    public bool disable;

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
        dt.color = Color.HSVToRGB(Random.Range(0.0f, 1.0f), 1.0f, 1.0f);
        dt.target = target;
        dt.uvMeshs = new Mesh[4];
        dt.uvMeshs[0] = CreateUVMesh(targetMesh, targetMesh.uv);
        dt.uvMeshs[1] = CreateUVMesh(targetMesh, targetMesh.uv2);
        dt.uvMeshs[2] = CreateUVMesh(targetMesh, targetMesh.uv3);
        dt.uvMeshs[3] = CreateUVMesh(targetMesh, targetMesh.uv4);
        return dt;
    }

    public void Release()
    {
        for (int i = 0; i < uvMeshs.Length; i++)
        {
            if (uvMeshs[i])
                Object.DestroyImmediate(uvMeshs[i]);
        }
        uvMeshs = null;
    }

    private static Mesh CreateUVMesh(Mesh targetMesh, Vector2[] uv)
    {
        if (uv.Length <= 0)
            return null;
        Mesh result = new Mesh();
        result.hideFlags = HideFlags.HideAndDontSave;
        Vector3[] vertexList = new Vector3[targetMesh.vertexCount];
        for (int i = 0; i < targetMesh.vertexCount; i++)
        {
            vertexList[i] = new Vector3(uv[i].x, uv[i].y, 0);
        }
        result.vertices = vertexList;
        for (int j = 0; j < targetMesh.subMeshCount; j++)
        {
            result.SetIndices(targetMesh.GetIndices(j), MeshTopology.Triangles, j);
        }
        return result;
    }
}
