using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    private Vector3[] vertices;
    private Color[] colors;
    private int[] triangles;
    
    private Mesh mesh;
    private int tris;

    private TileProcessor processor;

    private Vector3Int number;
    private Vector3Int chunkSize;
    private Vector3Int worldSize;

    private enum Planes
    {
        X,
        Y,
        Z
    }
    public void InitChunk(Vector3Int number, Vector3Int size, Vector3Int worldSize)
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        processor = FindObjectOfType<TileProcessor>();
        
        this.number = number;
        chunkSize = size;
        this.worldSize = worldSize;
        
        gameObject.name = $"Chunk[{number.x}][{number.y}][{number.z}]";
        
        Draw();
    }

    public void StateChange()
    {
        RawUpdate();
        if (number.x != (worldSize.x / chunkSize.x - 1))
            processor.GetChunkByNumber(number + Vector3Int.right).RawUpdate();
        if (number.x != 0)
            processor.GetChunkByNumber(number + Vector3Int.left).RawUpdate(); 
        
        if (number.y != (worldSize.y / chunkSize.y - 1))
            processor.GetChunkByNumber(number + Vector3Int.up).RawUpdate();
        if (number.y != 0)
            processor.GetChunkByNumber(number + Vector3Int.down).RawUpdate();
        
        if (number.z != (worldSize.z / chunkSize.z - 1))
            processor.GetChunkByNumber(number + Vector3Int.forward).RawUpdate();
        if (number.z != 0)
            processor.GetChunkByNumber(number + Vector3Int.back).RawUpdate();
    }

    public void RawUpdate()
    {
        Draw();
    }
    private void Draw()
    {
        vertices = new Vector3[(chunkSize.x + 1) * (chunkSize.y + 1) * (chunkSize.z + 1)];
        triangles = new int[chunkSize.x * chunkSize.y * chunkSize.z * 6];
        colors = new Color[(chunkSize.x + 1) * (chunkSize.y + 1) * (chunkSize.z + 1)];
        
        Tile[,,] tiles = processor.GetAllTiles();
        tris = 0;
        int v = 0;
        for (int y = number.y * chunkSize.y; y <= number.y * chunkSize.y + chunkSize.y; y++)
        {
            for (int z = number.z * chunkSize.z; z <= number.z * chunkSize.z + chunkSize.z; z++)
            {
                for (int x = number.x * chunkSize.x; x <= number.x * chunkSize.x + chunkSize.x; x++)
                {
                    vertices[v] = new Vector3(x, y, z);
                    colors[v] = Color.Lerp(Color.black, new Color(0.59f, 0.85f, 0.9f), (y - 20) / 40.5f);
                    v++;
                }
            }
        }
        
        for (int y = number.y * chunkSize.y; y < number.y * chunkSize.y + chunkSize.y; y++)
        {
            for (int z = number.z * chunkSize.z; z < number.z * chunkSize.z + chunkSize.z; z++)
            {
                for (int x = number.x * chunkSize.x; x < number.x * chunkSize.x + chunkSize.x; x++)
                {
                    ProcessTile(new Vector3Int(x, y, z), tiles);
                }
            }
        }
        UpdateMesh();
    }
    void ProcessTile(Vector3Int pos, Tile[,,] tiles)
    {
        if (!tiles[pos.x, pos.y, pos.z].IsSolid()) return;
        
        if (pos.x == 0)
        {
            CreateFace(pos, Planes.X, false);
        }
        if (pos.y == 0)
        {
            CreateFace(pos, Planes.Y, false);
        }
        if (pos.z == 0)
        {
            CreateFace(pos, Planes.Z, false);
        }
        if (pos.x < worldSize.x - 1 && !tiles[pos.x + 1, pos.y, pos.z].IsSolid()) //FORWARD
            CreateFace(pos + Vector3Int.right, Planes.X, true);
        if (pos.x > 0 && !tiles[pos.x - 1, pos.y, pos.z].IsSolid()) //BACK
            CreateFace(pos, Planes.X, false);
        
        if (pos.y < worldSize.y - 1 && !tiles[pos.x, pos.y + 1, pos.z].IsSolid()) //UP
            CreateFace(pos + Vector3Int.up, Planes.Y, true);
        if (pos.y > 0 && !tiles[pos.x, pos.y - 1, pos.z].IsSolid()) //DOWN
            CreateFace(pos, Planes.Y, false);
        
        if(pos.z < worldSize.z - 1 && !tiles[pos.x, pos.y, pos.z + 1].IsSolid()) //RIGHT
            CreateFace(pos + Vector3Int.forward, Planes.Z, true);
        if (pos.z > 0 && !tiles[pos.x, pos.y, pos.z - 1].IsSolid()) //LEFT
            CreateFace(pos, Planes.Z, false);
        
        if (pos.x == worldSize.x - 1) //END X
            CreateFace(pos + Vector3Int.right, Planes.X, true);
        if (pos.y == worldSize.y - 1) //END Y
            CreateFace(pos + Vector3Int.up, Planes.Y, true);
        if (pos.z == worldSize.z - 1) //END Z
            CreateFace(pos + Vector3Int.forward, Planes.Z, true);
    }
    
    void CreateFace(Vector3Int p, Planes plane, bool inside)
    {
        int corner = (p.y - number.y * chunkSize.y) * (chunkSize.x + 1) * (chunkSize.z + 1) 
                     + (p.z - number.z * chunkSize.z) * (chunkSize.x + 1)
                     + (p.x - number.x * chunkSize.x);
        switch (plane)
        {
            case Planes.X:
            {
                DefineTriangles(new []
                {
                    corner,
                    corner + chunkSize.x + 1,
                    corner + (chunkSize.x + 1) * (chunkSize.z + 1) + (chunkSize.x + 1),
                    corner + (chunkSize.x + 1) * (chunkSize.z + 1) + (chunkSize.x + 1),
                    corner + (chunkSize.x + 1) * (chunkSize.z + 1),
                    corner
                }, inside);
            } break;
            case Planes.Y:
            {
                DefineTriangles(new []
                {
                    corner,
                    corner + chunkSize.x + 2,
                    corner + chunkSize.x + 1,
                    corner,
                    corner + 1,
                    corner + chunkSize.x + 2
                }, inside);
            } break;
            case Planes.Z:
            {
                DefineTriangles(new []
                {
                    corner,
                    corner + (chunkSize.x + 1) * (chunkSize.z + 1),
                    corner + (chunkSize.x + 1) * (chunkSize.z + 1) + 1,
                    corner,
                    corner + (chunkSize.x + 1) * (chunkSize.z + 1) + 1,
                    corner + 1
                }, inside);
            } break;
        }
    }
    
    private void DefineTriangles(int[] tri, bool inside)
    {
        triangles[inside ? tris + 2 : tris] = tri[0];
        triangles[tris + 1] = tri[1];
        triangles[inside ? tris : tris + 2] = tri[2];
        triangles[inside ? tris + 5 : tris + 3] = tri[3];
        triangles[tris + 4] = tri[4];
        triangles[inside ? tris + 3 : tris + 5] = tri[5];
        
        tris += 6;
    }
    
    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        mesh.RecalculateNormals();
    }

}