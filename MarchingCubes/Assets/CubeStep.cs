using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public class CubeStep
{
    static private BinaryFormatter binary_formatter = new BinaryFormatter();

    [System.Serializable]
    public struct Vertex
    {
        public Vector3 from;
        public int id_from;
        public Vector3 shift;
        public int id_to;
        
        public Vector3 GetPos(float[] weights)
        {
            return from + shift * weights[id_from] / (weights[id_from] - weights[id_to]);
        }

        public Vertex Rotated(int rot_id)
        {
            return new Vertex
            {
                from = IdManagement.RotateNode(from, rot_id),
                id_from = IdManagement.RotateNode(id_from, rot_id),
                shift = IdManagement.Rotate(shift, rot_id),
                id_to = IdManagement.RotateNode(id_to, rot_id)
            };
        }

        public Vertex Mirrired()
        {
            return new Vertex
            {
                from = IdManagement.MirrorNode(from),
                id_from = IdManagement.MirrorNode(id_from),
                shift = -shift,
                id_to = IdManagement.MirrorNode(id_to)
            };
        }
    }

    [SerializeField]
    public int id;
    [SerializeField]
    public Vertex[] vertices;
    [SerializeField]
    public Vector3Int[] triangles;

    public Mesh BuildMesh(float[] weights)
    {
        Mesh mesh = new Mesh();
        Assert.IsNotNull(vertices);
        Vector3[] vert_result = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; ++i)
        {
            vert_result[i] = vertices[i].GetPos(weights);
        }
        mesh.vertices = vert_result;
        int[] trngl_result = new int[triangles.Length * 3];
        for(int i = 0; i < triangles.Length; ++i)
        {
            trngl_result[3 * i + 0] = triangles[i].x;
            trngl_result[3 * i + 1] = triangles[i].y;
            trngl_result[3 * i + 2] = triangles[i].z;
        }
        mesh.triangles = trngl_result;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public CubeStep Rotated(int id_rot)
    {
        CubeStep result = new CubeStep();
        Vertex[] rot_vert = new Vertex[vertices.Length];
        for (int i = 0; i < rot_vert.Length; i++)
        {
            rot_vert[i] = vertices[i].Rotated(id_rot);
        }
        Vector3Int[] triangles_copy = new Vector3Int[triangles.Length];
        Array.Copy(triangles, triangles_copy, triangles.Length);
        result.vertices = rot_vert;
        result.triangles = triangles_copy;
        result.id = IdManagement.RotateCubeId(id, id_rot);
        return result;
    }

    public CubeStep Mirrored() 
    {
        CubeStep result = new CubeStep();
        Vertex[] mir_vert = new Vertex[vertices.Length];
        for(int i = 0;i < mir_vert.Length; ++i)
        {
            mir_vert[i] = vertices[i].Mirrired();
        }
        Vector3Int[] triangles_mir = new Vector3Int[triangles.Length];
        for( int i = 0; i < triangles.Length; ++i)
        {
            triangles_mir[i] = new Vector3Int
                (
                triangles[i].x,
                triangles[i].z,
                triangles[i].y
                );
        }
        result.vertices = mir_vert;
        result.triangles = triangles_mir;
        result.id = IdManagement.MirrorCubeId(id);
        return result;
    }

    // vvv to delete? vvv
     /*
    public byte[] Save() // 4 bytes of size + the data 
    {
        MemoryStream stream = new MemoryStream();
        stream.Write(new byte[sizeof(int)], 0, sizeof(int));
        binary_formatter.Serialize(stream, this);
        byte[] result = stream.ToArray();
        Array.Copy(BitConverter.GetBytes(result.Length - sizeof(int)), result, sizeof(int));
        return result;
    }

    private static void Save(string filename, byte[] data)
    {
        FileStream stream = new FileStream(filename, FileMode.Create);
        stream.Write(data, 0, data.Length);
    }

    public void Save(string filename)
    {
        Save(filename, Save());
    }

    static public byte[] Save(CubeStep[] cubes)
    {
        List<byte> data = new List<byte>();
        byte[] prefix = BitConverter.GetBytes((int)cubes.Length);
        data.AddRange(prefix);
        foreach (CubeStep cube in cubes)
        {
            data.AddRange(cube.Save());
        }
        return data.ToArray();
    }

    static public void Save(CubeStep[] cubes, string filename)
    {
        Save(filename, Save(cubes));
    }

    static public CubeStep Load(byte[] data) // 4 bytes of size + the data
    {
        if(data.Length < sizeof(int))
        {
            Debug.LogError("input data too small");
            throw new InvalidDataException();
        }
        int size = BitConverter.ToInt32(data, 0);
        if(data.Length != size + sizeof(int))
        {
            Debug.LogError("input data has incorrect size");
            throw new InvalidDataException();
        }
        MemoryStream stream = new MemoryStream(data, sizeof(int), size);
        return (CubeStep)binary_formatter.Deserialize(stream);
    }

    private static byte[] LoadBytes(string filename)
    {
        FileStream stream = new FileStream(filename, FileMode.Open);
        long file_lenght = stream.Length;
        byte[] bytes = new byte[file_lenght];
        int got_bytes = stream.Read(bytes, 0, bytes.Length);
        if (got_bytes != bytes.Length)
        {
            Debug.LogError("invalid input from file");
            throw new InvalidDataException();
        }
        return bytes;
    }

    static public CubeStep Load(string filename) 
    {
        return Load(LoadBytes(filename));
    }

    static public CubeStep[] LoadArray(byte[] data) 
    {
        if(data.Length < sizeof(int))
        {
            Debug.LogError("input data too small");
        }

        int count = BitConverter.ToInt32(data, 0);

        CubeStep[] result = new CubeStep[count];

        int offset = 0;

        for(int i = 0; i < count; ++i)
        {
            if (data.Length < sizeof(int) + offset)
            {
                Debug.LogError("remaining data too small");
            }
            int size = BitConverter.ToInt32(data, offset) + 4;
            byte[] chunk = new byte[size];
            for(int j = 0; j < size; ++j) 
            {
                chunk[j] = data[offset + j];
            }
            result[i] = Load(chunk);
            offset += size;
        }

        return result;
    }

    static public CubeStep[] LoadArray(string filename)
    {
        return LoadArray(LoadBytes(filename));
    }
     //*/
}
