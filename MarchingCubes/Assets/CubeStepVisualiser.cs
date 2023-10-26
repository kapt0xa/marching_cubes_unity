using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CubeStepVisualiser : MonoBehaviour
{
    [SerializeField]
    public CubeStep cube;

    [SerializeField]
    private CubeTable table;
    [SerializeField]
    private bool export_flag;
    [SerializeField]
    private bool gizmos_flag = false;
    [SerializeField]
    private bool get_undefined_flag = false;
    [SerializeField]
    private bool recalculate_weights_flag = false;

    [SerializeField]
    private int id_to_get = 0;
    [SerializeField]
    private bool get_by_id_flag = false;

    [SerializeField]
    private float[] weights = new float[8];
    [SerializeField]
    private bool[] hilight_vertices = new bool[0];

    [System.Serializable]
    struct SerialisableArray
    {
        [SerializeField]
        private int[] inside_arr;
        public static SerialisableArray[] Cast(int[][] scource)
        {
            SerialisableArray[] result = new SerialisableArray[scource.Length];
            for (int i = 0; i < scource.Length; ++i)
            {
                result[i] = new SerialisableArray { inside_arr = scource[i] };
            }
            return result;
        }
    }

    [SerializeField]
    private SerialisableArray[] classification;

    // Start is called before the first frame update
    void Start()
    {
        table = CubeTable.GetMain();
        Assert.IsTrue(table != null);
        gizmos_flag = true;
        classification = SerialisableArray.Cast(IdManagement.GetGroupClassification());
    }

    // Update is called once per frame
    void Update()
    {
        if (export_flag)
        {
            export_flag = false;
            table.AddAllVariations(cube);
        }
        if(get_by_id_flag)
        {
            get_by_id_flag = false;
            cube = table.GetCube(id_to_get);
            RecalculateWeights();
        }
        if(get_undefined_flag) 
        {
            get_undefined_flag = false;
            cube = table.GetUndefinedRaw();
            RecalculateWeights();
        }
        if(recalculate_weights_flag)
        {
            recalculate_weights_flag = false;
            RecalculateWeights();
        }
    }

    private void OnDrawGizmos()
    {
        if(gizmos_flag && cube != null)
        {
            for(int i = 0; i < 8; ++i)
            {
                Gizmos.color = (weights[i] > 0) ? Color.black : Color.white;
                Gizmos.DrawSphere(IdManagement.id_to_cube_node[i], 0.05f * weights[i]);
            }

            Mesh mesh = cube.BuildMesh(weights);

            if (mesh != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawMesh(mesh);
                Gizmos.DrawWireMesh(mesh);
            }

            if(hilight_vertices.Length != mesh.vertexCount)
            {
                hilight_vertices = new bool[mesh.vertexCount];
            }

            for (int i = 0; i < mesh.vertexCount; ++i)
            {
                Gizmos.color = hilight_vertices[i]? Color.red : Color.green;
                Gizmos.DrawSphere(mesh.vertices[i], 0.05f);
            }
        }
    }

    void RecalculateWeights()
    {
        if(cube != null)
        {
            bool[] material_flags = IdManagement.CubeIdToFlags(cube.id);
            for (int i = 0; i < 8; ++i)
            {
                weights[i] = material_flags[i]? 1 : -1;
            }
        }
        else
        {
            for (int i = 0; i < 8; ++i)
            {
                weights[i] = -1;
            }
        }
    }
}
