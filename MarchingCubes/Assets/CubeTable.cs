using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CubeTable : MonoBehaviour
{
    [SerializeField]
    private CubeStep[] table = new CubeStep[256];
    [SerializeField]
    private int complete_count = 0;

    public CubeStep GetCube(int index)
    {  
        return table[index].Rotated(0); 
    }

    private static CubeTable the_main_table = null;

    static public CubeTable GetMain()
    {
        return the_main_table;
    }

    void Start()
    {
        Assert.IsTrue(table.Length == 256);
        if(complete_count == 0)
        {
            Debug.LogWarning("table is empty. clearing all content");
            for (int i = 0; i < table.Length; ++i)
            {
                table[i] = null;
            }
        } 
        else if (complete_count == 256)
        {
            for (int i = 0; i < table.Length; ++i)
            {
                Assert.IsNotNull(table[i]);
            }
        }
        else
        {
            Debug.LogError("u cant init with half-complete CubeTable, cuz Unity will corrupt null elements with default value");
        }
    }

    void Awake()
    {
        if (the_main_table == null) 
        {
            Debug.Log("CubeTable is set");
            the_main_table = this;
        }
        else
        {
            Debug.LogWarning("table is not single. U can set  some table to be main with private method ForceToBeMain");
        }
    }

    private void ForceToBeMain()
    {
        the_main_table = this;
    }

    public int? GetUndefinedId() 
    {
        for (int i = 0; i < table.Length; ++i)
        {
            if (table[i] == null)
            {
                return i;
            }
        }
        return null;
    }

    public CubeStep GetUndefinedRaw() // returns cube without setted triandles or null
    {
        int? id = GetUndefinedId();
        if (id == null) 
        {
            return null;
        }
        CubeStep cube = new CubeStep();
        cube.id = id.Value;

        bool[] material_flags = IdManagement.CubeIdToFlags(id.Value);

        List<CubeStep.Vertex> vertices = new List<CubeStep.Vertex>();
        for (int i = 0; i < 8; ++i)
        {
            if (material_flags[i])
            {
                foreach (var dim in new IdManagement.Dimention[] { IdManagement.Dimention.X, IdManagement.Dimention.Y, IdManagement.Dimention.Z })
                {
                    int other_id = IdManagement.cube_node_to_id[IdManagement.id_to_cube_node[i] + IdManagement.GetShift(i, dim)];
                    if (!material_flags[other_id])
                    {
                        CubeStep.Vertex added = new CubeStep.Vertex();
                        added.id_from = i;
                        added.from = IdManagement.id_to_cube_node[i];
                        added.shift = IdManagement.GetShift(i, dim);
                        added.id_to = IdManagement.cube_node_to_id[added.from + added.shift];
                        vertices.Add(added);
                    }
                }
            }
        }

        cube.vertices = vertices.ToArray();
        Debug.Log($"{cube.vertices.Length}, {vertices.Count}");
        cube.triangles = new Vector3Int[0];

        return cube;
    }

    public void AddAllVariations(CubeStep cube, bool overwrite = true)
    {
        AddAllRotations(cube, overwrite);
        AddAllRotations(cube.Mirrored(), overwrite);
    }

    private void AddAllRotations(CubeStep cube, bool overwrite)
    {
        if(complete_count == table.Length)
        {
            Debug.LogWarning("attempt to edit complete table om marching cube cases");
            throw new System.InvalidOperationException();
        }
        for(int i = 0; i < IdManagement.id_to_rotation.Length; ++i) 
        {
            CubeStep rotated = cube.Rotated(i);
            if (table[rotated.id] == null)
            {
                table[rotated.id] = rotated;
                ++complete_count;
            }
            else if (overwrite)
            {
                table[rotated.id] = rotated;
            }
        }
    }

    public Mesh BuildMesh(float[] weights)
    {
        Assert.IsTrue(weights.Length == 8);
        bool[] bools = new bool[weights.Length];
        for(int i = 0; i < weights.Length; ++i)
        {
            bools[i] = weights[i] > 0;
        }
        int id = IdManagement.BoolsToCubeId(bools);
        return table[id].BuildMesh(weights);
    }
}
