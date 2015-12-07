﻿//////////////////////////////////////////////////////////////////////////////////////////
// Every Day Space Station
// http://everydayspacestation.tumblr.com
//////////////////////////////////////////////////////////////////////////////////////////
// SceneChunk - Represents a chunk of the map. Usuaully going to be 16x16 blocks. Mainly should interact with SceneLevelManager and child SceneBlocks
// Is responsible for rendering blocks to allow batching and minimize meshes in an orderly, predictable fashion
// Created: December 4 2015
// CasualSimpleton <casualsimpleton@gmail.com>
// Last Modified: December 4 2015
// CasualSimpleton
//////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

using EveryDaySpaceStation;
using EveryDaySpaceStation.DataTypes;
using EveryDaySpaceStation.Utils;

public class SceneChunk : MonoBehaviour
{
    #region Vars
#if UNITY_EDITOR
    public bool ShowVertGizmos = false;
#endif

    public static float blockSize = 1f;

    public Transform _transform { get; private set; }

    [SerializeField]
    SceneBlock[] _chunkBlocks;

    [SerializeField]
    Vec2Int _chunkPos;

    [SerializeField]
    Mesh _mesh;

    [SerializeField]
    MeshRenderer _meshRenderer;

    [SerializeField]
    MeshFilter _meshFilter;

    [SerializeField]
    Vector3 _worldPosition;

    [SerializeField]
    Quaternion _rotation;

    [SerializeField]
    Material _material;

    [SerializeField]
    Vector3[] _verts;

    [SerializeField]
    Vector3[] _normals;

    [SerializeField]
    Vector2[] _uv;

    [SerializeField]
    int[] _tri;
    #endregion

    public void CreateChunk(Vector3 worldPosition, Vec2Int chunkPosition)
    {
        _worldPosition = worldPosition;
        _chunkPos = chunkPosition;

        _transform = this.gameObject.transform;

        this.gameObject.name = string.Format("Chunk-{0}", _chunkPos);

        _transform.position = _worldPosition;

        _verts = new Vector3[SceneLevelManager.Singleton.BlocksPerChuck * SceneLevelManager.Singleton.BlocksPerChuck * SceneBlock.FacesPerBlock * 4]; //Probably 16x16 or 8x8 blocks, with 6 faces, and 4 verts per face
        _normals = new Vector3[_verts.Length];
        _uv = new Vector2[_verts.Length];
        _tri = new int[SceneLevelManager.Singleton.BlocksPerChuck * SceneLevelManager.Singleton.BlocksPerChuck * SceneBlock.FacesPerBlock * 6];

        _mesh = new Mesh();
        _mesh.name = string.Format("Chunk-{0}-Mesh", _chunkPos);

        PrepareBlockElements();

        _meshFilter = this.gameObject.AddComponent<MeshFilter>();
        _meshRenderer = this.gameObject.AddComponent<MeshRenderer>();

        _meshRenderer.material = _material;

        _meshFilter.mesh = _mesh;

        _mesh.vertices = _verts;
        _mesh.normals = _normals;
        _mesh.uv = _uv;
        _mesh.triangles = _tri;

        _mesh.RecalculateBounds();
    }

    /// <summary>
    /// Creates each block and sets up all the verts, normals and triangles for that block
    /// </summary>
    void PrepareBlockElements()
    {
        //TODO Blocks can probably come from a pool
        _chunkBlocks = new SceneBlock[SceneLevelManager.Singleton.BlocksPerChuck * SceneLevelManager.Singleton.BlocksPerChuck];

        int x, z;
        int xx, zz;

        int len = _chunkBlocks.Length;
        int vertIndex = 0;
        int normIndex = 0;
        int uvIndex = 0;
        for (int i = 0; i < len; i++)
        {
            z = i / SceneLevelManager.Singleton.BlocksPerChuck;
            x = i - (z * SceneLevelManager.Singleton.BlocksPerChuck);
            xx = x + 1;
            zz = z + 1;

            //TODO this currently assumes only one total chunk
            Vec2Int pos = new Vec2Int(x, z);
            int index = Helpers.IndexFromVec2Int(pos, SceneLevelManager.Singleton.BlocksPerChuck);
            int[] vertFirstIndex = new int[SceneBlock.FacesPerBlock];

            #region Verts, Norms, UVs and Triangles
            //Forward Face - Z+ - Clockwise LOOKING AT face - normal point Z+
            //Vert 0
            vertFirstIndex[(int)GameData.GameBlockData.BlockFaces.FaceZForward] = vertIndex;
            _verts[vertIndex++] = new Vector3((x * blockSize) + 0f, blockSize, zz * blockSize);
            //Vert 1
            _verts[vertIndex++] = new Vector3((x * blockSize) + 0f, 0f, zz * blockSize);
            //Vert 2
            _verts[vertIndex++] = new Vector3(xx * blockSize, 0f, zz * blockSize);
            //Vert 3
            _verts[vertIndex++] = new Vector3(xx * blockSize, blockSize, zz * blockSize);            

            _normals[normIndex++] = new Vector3(0f, 0f, 1f);
            _normals[normIndex++] = new Vector3(0f, 0f, 1f);
            _normals[normIndex++] = new Vector3(0f, 0f, 1f);
            _normals[normIndex++] = new Vector3(0f, 0f, 1f);

            //TODO - Fix this
            _uv[uvIndex++] = new Vector2(1f, 1f);
            _uv[uvIndex++] = new Vector2(1f, 0);
            _uv[uvIndex++] = new Vector2(0, 0);
            _uv[uvIndex++] = new Vector2(0, 1f);

            //Right Face - X+ - Clockwise LOOKING AT face - normal point X+
            vertFirstIndex[(int)GameData.GameBlockData.BlockFaces.FaceXForward] = vertIndex;
            _verts[vertIndex++] = new Vector3(xx * blockSize, blockSize, zz * blockSize);
            _verts[vertIndex++] = new Vector3(xx * blockSize, 0f, zz * blockSize);
            _verts[vertIndex++] = new Vector3(xx * blockSize, 0f, (z * blockSize) + 0f);
            _verts[vertIndex++] = new Vector3(xx * blockSize, blockSize, (z * blockSize) + 0f);

            _normals[normIndex++] = new Vector3(1f, 0f, 0f);
            _normals[normIndex++] = new Vector3(1f, 0f, 0f);
            _normals[normIndex++] = new Vector3(1f, 0f, 0f);
            _normals[normIndex++] = new Vector3(1f, 0f, 0f);

            //TODO - Fix this
            _uv[uvIndex++] = new Vector2(1f, 1f);
            _uv[uvIndex++] = new Vector2(1f, 0);
            _uv[uvIndex++] = new Vector2(0, 0);
            _uv[uvIndex++] = new Vector2(0, 1f);

            //Back Face - Z- - Clockwise LOOKING at face - normal pointing Z-
            vertFirstIndex[(int)GameData.GameBlockData.BlockFaces.FaceZBack] = vertIndex;
            _verts[vertIndex++] = new Vector3(xx * blockSize, blockSize, (z * blockSize) + 0f);
            _verts[vertIndex++] = new Vector3(xx * blockSize, 0f, (z * blockSize) + 0f);
            _verts[vertIndex++] = new Vector3((x * blockSize) + 0f, 0f, (z * blockSize) + 0f);
            _verts[vertIndex++] = new Vector3((x * blockSize) + 0f, blockSize, (z * blockSize) + 0f);

            _normals[normIndex++] = new Vector3(0f, 0f, -1f);
            _normals[normIndex++] = new Vector3(0f, 0f, -1f);
            _normals[normIndex++] = new Vector3(0f, 0f, -1f);
            _normals[normIndex++] = new Vector3(0f, 0f, -1f);

            //TODO - Fix this
            _uv[uvIndex++] = new Vector2(1f, 1f);
            _uv[uvIndex++] = new Vector2(1f, 0);
            _uv[uvIndex++] = new Vector2(0, 0);
            _uv[uvIndex++] = new Vector2(0, 1f);

            //Left Face - X- - Clockwise LOOKING at face - normal pointing X-
            vertFirstIndex[(int)GameData.GameBlockData.BlockFaces.FaceXBack] = vertIndex;
            _verts[vertIndex++] = new Vector3((x * blockSize) + 0f, blockSize, (z * blockSize) + 0f);
            _verts[vertIndex++] = new Vector3((x * blockSize) + 0f, 0f, (z * blockSize) + 0f);
            _verts[vertIndex++] = new Vector3((x * blockSize) + 0f, 0f, zz * blockSize);
            _verts[vertIndex++] = new Vector3((x * blockSize) + 0, blockSize, zz * blockSize);

            _normals[normIndex++] = new Vector3(1f, 0f, 0f);
            _normals[normIndex++] = new Vector3(1f, 0f, 0f);
            _normals[normIndex++] = new Vector3(1f, 0f, 0f);
            _normals[normIndex++] = new Vector3(1f, 0f, 0f);

            //TODO - Fix this
            _uv[uvIndex++] = new Vector2(1f, 1f);
            _uv[uvIndex++] = new Vector2(1f, 0);
            _uv[uvIndex++] = new Vector2(0, 0);
            _uv[uvIndex++] = new Vector2(0, 1f);

            //Top Face - Y+ - since this will be seen from a top down view (for the time being)
            vertFirstIndex[(int)GameData.GameBlockData.BlockFaces.FaceTop] = vertIndex;
            _verts[vertIndex++] = new Vector3(xx * blockSize, blockSize, zz * blockSize);
            _verts[vertIndex++] = new Vector3(xx * blockSize, blockSize, (z * blockSize) + 0f);
            _verts[vertIndex++] = new Vector3((x * blockSize) + 0f, blockSize, (z * blockSize) + 0f);
            _verts[vertIndex++] = new Vector3((x * blockSize) + 0f, blockSize, zz * blockSize);

            _normals[normIndex++] = new Vector3(0f, 1f, 0f);
            _normals[normIndex++] = new Vector3(0f, 1f, 0f);
            _normals[normIndex++] = new Vector3(0f, 1f, 0f);
            _normals[normIndex++] = new Vector3(0f, 1f, 0f);

            //TODO - Fix this
            _uv[uvIndex++] = new Vector2(1f, 1f);
            _uv[uvIndex++] = new Vector2(1f, 0);
            _uv[uvIndex++] = new Vector2(0, 0);
            _uv[uvIndex++] = new Vector2(0, 1f);

            //Bottom Face - Y+ - Since this is the floor, it needs to point up as well
            vertFirstIndex[(int)GameData.GameBlockData.BlockFaces.FaceBottom] = vertIndex;
            _verts[vertIndex++] = new Vector3(xx * blockSize, 0f, zz * blockSize);
            _verts[vertIndex++] = new Vector3(xx * blockSize, 0f, (z * blockSize) + 0f);
            _verts[vertIndex++] = new Vector3((x * blockSize) + 0f, 0f, (z * blockSize) + 0f);
            _verts[vertIndex++] = new Vector3((x * blockSize) + 0f, 0f, zz * blockSize);
            
            _normals[normIndex++] = new Vector3(0f, -1f, 0f);
            _normals[normIndex++] = new Vector3(0f, -1f, 0f);
            _normals[normIndex++] = new Vector3(0f, -1f, 0f);
            _normals[normIndex++] = new Vector3(0f, -1f, 0f);

            //TODO - Fix this
            _uv[uvIndex++] = new Vector2(1f, 1f);
            _uv[uvIndex++] = new Vector2(1f, 0);
            _uv[uvIndex++] = new Vector2(0, 0);
            _uv[uvIndex++] = new Vector2(0, 1f);
            #endregion

            #region Triangle Order
            //Start point is determined since we're only going to be doing that one cube
            int startPoint = i * SceneBlock.FacesPerBlock * 6;
            int triLen = startPoint + (SceneBlock.FacesPerBlock * 6); //Only going to do 6 faces
            int vertCount = 0 + (i * SceneBlock.FacesPerBlock * 4);
            int[] faceFirstTri = new int[SceneBlock.FacesPerBlock];
            int faceIndex = 0;
            for (int j = startPoint; j < triLen; j += 6)
            {
                faceFirstTri[faceIndex] = j;
                _tri[j] = vertCount;
                _tri[j + 1] = vertCount + 1;
                _tri[j + 2] = vertCount + 2;

                _tri[j + 3] = vertCount + 2;
                _tri[j + 4] = vertCount + 3;
                _tri[j + 5] = vertCount;

                vertCount += 4;
                faceIndex++;
            }
            #endregion

            _chunkBlocks[i] = new SceneBlock();
            _chunkBlocks[i].Create(pos, pos, index, index, faceFirstTri, vertFirstIndex, this);
        }
    }

    public void UpdateBlock(MapData.MapTileData blockData)
    {
        SceneBlock block = null;

        //TODO This should be calculatable and not required for a search. But too tired to figure it out at the moment
        //-CasualSimpleton
        for (int i = 0; i < _chunkBlocks.Length; i++)
        {
            if (_chunkBlocks[i].WorldPos == blockData.TilePosition)
            {
                block = _chunkBlocks[i];
                break;
            }
        }

        if (block == null)
        {
            Debug.LogWarning(string.Format("Can't find a sceneblock for maptile: {0}", blockData.TilePosition));
            return;
        }

        block.UpdateFaces(blockData);
    }

    #region Triangle and Face Modification
    public void ModifyTriangles(int triIndex, int vertOneIndex, int vertTwoIndex, int vertThreeIndex, int vertFourIndex)
    {
        ModifyTrianglesNoUpdate(triIndex, vertOneIndex, vertTwoIndex, vertThreeIndex, vertFourIndex);

        UpdateMesh();
    }

    /// <summary>
    /// Same functionality as ModifyTriangles but doesn't update the mesh. Useful if modifying large number of faces at once
    /// </summary>
    public void ModifyTrianglesNoUpdate(int triIndex, int vertOneIndex, int vertTwoIndex, int vertThreeIndex, int vertFourIndex)
    {
        _tri[triIndex] = vertOneIndex;
        _tri[triIndex + 1] = vertTwoIndex;
        _tri[triIndex + 2] = vertThreeIndex;

        _tri[triIndex + 3] = vertThreeIndex;
        _tri[triIndex + 4] = vertFourIndex;
        _tri[triIndex + 5] = vertOneIndex;

#if DEBUGCLIENT
        if (_tri[triIndex] > _verts.Length || 
            _tri[triIndex + 1] > _verts.Length ||
            _tri[triIndex + 2] > _verts.Length ||
            _tri[triIndex + 3] > _verts.Length ||
            _tri[triIndex + 4] > _verts.Length ||
            _tri[triIndex + 5] > _verts.Length)
        {
            Debug.LogError("Triangle refers to out of bounds vert");
        }
#endif
    }
    #endregion

    #region Texturing and Materials
    public void ModifyUV(int uvIndex, Vector4 uv)
    {
        ModifyUV(uvIndex, uv);

        UpdateUV();
    }

    public void ModifyUVNoUpdate(int uvIndex, Vector4 uv)
    {
        _uv[uvIndex] = new Vector2(uv.x + uv.z, uv.y);
        _uv[uvIndex + 1] = new Vector2(uv.x + uv.z, uv.y - uv.w);
        _uv[uvIndex + 2] = new Vector2(uv.x, uv.y - uv.w);
        _uv[uvIndex + 3] = new Vector2(uv.x, uv.y);

#if DEBUGCLIENT
        if (uvIndex < 0 || uvIndex > _uv.Length - 1)
        {
            Debug.LogError(string.Format("UV index {0} is out of bounds", uvIndex));
        }
#endif
    }
    #endregion
    /// <summary>
    /// Updates the mesh's assigned verts and triangles. This has some cost associated with it, so doing it too often can cause performance issues
    /// </summary>
    public void UpdateMesh()
    {
        _mesh.vertices = _verts;
        _mesh.triangles = _tri;
    }

    /// <summary>
    /// Updates the mesh's assigned uvs. This has some cost associated with it, so doing it too often can cause performance issues
    /// </summary>
    public void UpdateUV()
    {
        _mesh.uv = _uv;
    }

    #region Materials
    public void AssignMaterial(EDSSSprite sprite)
    {
        //TODO Currently can only do one material per chunk
        //if (_meshRenderer.material != null && _meshRenderer.material != sprite.SpriteSheet.Material)
        //{
        //    return;
        //}

        _meshRenderer.material = sprite.SpriteSheet.Material;
    }
    #endregion

    void OnDrawGizmosSelected()
    {
        if(ShowVertGizmos)
        {
            Gizmos.color = Color.red;
            int len = _verts.Length;
            for(int i = 0; i< len; i++)
            {
                Gizmos.DrawWireSphere(_verts[i], 0.15f);
            }
        }
    }
}