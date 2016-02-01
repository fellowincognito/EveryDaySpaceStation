﻿//////////////////////////////////////////////////////////////////////////////////////////
// Every Day Space Station
// http://everydayspacestation.tumblr.com
//////////////////////////////////////////////////////////////////////////////////////////
// VoxelChunk - Voxel Chunk, contains voxel blocks
// Created: January 31 2016
// CasualSimpleton <casualsimpleton@gmail.com>
// Last Modified: January 31 2016
// CasualSimpleton
//////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using EveryDaySpaceStation;
using EveryDaySpaceStation.Utils;
using EveryDaySpaceStation.Json;
using EveryDaySpaceStation.DataTypes;

namespace EveryDaySpaceStation
{
    [System.Serializable]
    public class VoxelChunk
    {
        protected VoxelBlock[, ,] _blocks;
        protected UniqueList<ChunkRenderer> _chunkRenderers;
        public VoxelChunkOrganizer ChunkGameObject { get; private set; }

        public bool IsDirty { get; set; }

        public VoxelChunk(int xWidth, int yHeight, int zLength)
        {
            _blocks = new VoxelBlock[xWidth, yHeight, zLength];
            IsDirty = false;
            _chunkRenderers = new UniqueList<ChunkRenderer>();
        }

        public void SetChunkGameObject(VoxelChunkOrganizer chunkOrg)
        {
            ChunkGameObject = chunkOrg;
        }

        public void TestRandomChunkData()
        {
            int xw = _blocks.GetLength(0);
            int yh = _blocks.GetLength(1);
            int zl = _blocks.GetLength(2);

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            for (int x = 0; x < xw; x++)
            {
                for (int y = 0; y < yh; y++)
                {
                    for (int z = 0; z < zl; z++)
                    {
                        Random.seed = 10 + x + y + z;
                        int random = Random.Range(0, 3);

                        _blocks[x, y, z] = new VoxelBlock((ushort)random, this);
                        //Debug.Log("X " + x + " Y " + y + " Z " + z + " Random " + random);
                    }
                }
            }
            timer.Stop();
            Debug.Log("Created " + (xw * yh * zl) + " blocks in " + timer.ElapsedMilliseconds);

            IsDirty = true;
        }

        public void LoadChunkData(ushort[, ,] data)
        {
            int xw = _blocks.GetLength(0);
            int yh = _blocks.GetLength(1);
            int zl = _blocks.GetLength(2);

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            for (int x = 0; x < xw; x++)
            {
                for (int y = 0; y < yh; y++)
                {
                    for (int z = 0; z < zl; z++)
                    {
                        _blocks[x, y, z] = new VoxelBlock(data[x, y, z], this);
                        //Debug.Log("X " + x + " Y " + y + " Z " + z + " Random " + random);
                    }
                }
            }
            timer.Stop();
            Debug.Log("Created " + (xw * yh * zl) + " blocks in " + timer.ElapsedMilliseconds);

            IsDirty = true;
        }

        public ChunkRenderer GetChunkRenderer(ushort uid)
        {
            List<ChunkRenderer> cr = _chunkRenderers.List;
            for (int i = 0; i < cr.Count; i++)
            {
                if (cr[i].ChunkMaterialUID == uid)
                {
                    return cr[i];
                }
            }

            //ChunkRenderer newCR = new ChunkRenderer();
            ChunkRenderer newCR = ChunkRenderer.NewChunkRenderer(ChunkGameObject.transform);
            newCR.Init(uid, DefaultFiles.Singleton.testMaterials[uid - 1], this);

            _chunkRenderers.AddUnique(newCR);

            return newCR;
        }

        public void ChunkLateUpdate()
        {
            List<ChunkRenderer> crs = _chunkRenderers.List;

            for (int i = 0; i < crs.Count; i++)
            {
                crs[i].OurRender();
            }
        }

        public void ChunkUpdate()
        {
            if (!IsDirty)
            {
                return;
            }

            int xw = _blocks.GetLength(0);
            int yh = _blocks.GetLength(1);
            int zl = _blocks.GetLength(2);

            ChunkRenderer lastCR = null;

            for (int x = 0; x < xw; x++)
            {
                for (int y = 0; y < yh; y++)
                {
                    for (int z = 0; z < zl; z++)
                    {
                        VoxelBlock block = _blocks[x, y, z];

                        //Don't draw blocks "0"
                        if (block.BlockType == 0)
                        {
                            continue;
                        }

                        ChunkRenderer cr = lastCR;

                        if (cr == null || cr.ChunkMaterialUID != block.BlockType)
                        {
                            cr = GetChunkRenderer(block.BlockType);
                        }                        

                        UpdateChunkRendererBlock(cr, block, x, y, z, xw, yh, zl);

                        lastCR = cr;
                    }
                }
            }

            List<ChunkRenderer> crs = _chunkRenderers.List;

            for (int i = 0; i < crs.Count; i++)
            {
                crs[i].BuildMesh();
            }

            IsDirty = false;
        }

        protected void UpdateChunkRendererBlock(ChunkRenderer chunkRenderer, VoxelBlock block, 
            int curX, int curY, int curZ,
            int maxX, int maxY, int maxZ)
        {
            //Empty block
            if (block.BlockType == 0)
            {
                return;
            }

            #region Neighbor Checks
            bool needBottomFace = false;
            bool needTopFace = false;
            bool needFrontFace = false;
            bool needBackFace = false;
            bool needRightFace = false;
            bool needLeftFace = false;

            //Check bottom
            if (curY == 0)
            {
                //It's at the bottom, so add bottom face
                needBottomFace = true;
            }
            else if (_blocks[curX, curY - 1, curZ].BlockType == 0)
            {
                //Bottom neighbor is empty
                needBottomFace = true;
            }

            //Check top
            if (curY == maxY - 1)
            {
                //It's the top, so add top face
                needTopFace = true;
            }
            else if (_blocks[curX, curY + 1, curZ].BlockType == 0)
            {
                //Top neighbor is empty
                needTopFace = true;
            }

            //Check front
            if (curZ == maxZ - 1)
            {
                //It's the front (Z+) most block, so add front face
                needFrontFace = true;
            }
            else if (_blocks[curX, curY, curZ + 1].BlockType == 0)
            {
                //Front neighbor is empty
                needFrontFace = true;
            }

            //Check back
            if (curZ == 0)
            {
                //It's the back most block, so add back face
                needBackFace = true;
            }
            else if (_blocks[curX, curY, curZ - 1].BlockType == 0)
            {
                //Back neighbor is empty
                needBackFace = true;
            }

            //Check right
            if (curX == maxX - 1)
            {
                //It's the right most block, so add right face
                needRightFace = true;
            }
            else if (_blocks[curX + 1, curY, curZ].BlockType == 0)
            {
                //Right neighbor is empty
                needRightFace = true;
            }

            //Check left
            if (curX == 0)
            {
                //It's the left most block, so add left face
                needLeftFace = true;
            }
            else if (_blocks[curX - 1, curY, curZ].BlockType == 0)
            {
                //Left neighbor is empty
                needLeftFace = true;
            }
            #endregion

            #region Face Construction
            if (needTopFace)
            {
                int firstIndex = chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX),
                    (VoxelBlock.DefaultBlockSize.y * curY) + (VoxelBlock.DefaultBlockSize.y),
                    (VoxelBlock.DefaultBlockSize.z * curZ)),
                    Vector3.forward, new Vector2(0f, 0f));

                chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX),
                    (VoxelBlock.DefaultBlockSize.y * curY) + (VoxelBlock.DefaultBlockSize.y),
                    (VoxelBlock.DefaultBlockSize.z * curZ) + (VoxelBlock.DefaultBlockSize.z)),
                    Vector3.forward, new Vector2(0f, 1f));

                chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX) + (VoxelBlock.DefaultBlockSize.x),
                    (VoxelBlock.DefaultBlockSize.y * curY) + (VoxelBlock.DefaultBlockSize.y),
                    (VoxelBlock.DefaultBlockSize.z * curZ) + (VoxelBlock.DefaultBlockSize.z)),
                    Vector3.forward, new Vector2(1f, 1f));

                chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX) + (VoxelBlock.DefaultBlockSize.x),
                    (VoxelBlock.DefaultBlockSize.y * curY) + (VoxelBlock.DefaultBlockSize.y),
                    (VoxelBlock.DefaultBlockSize.z * curZ)),
                    Vector3.forward, new Vector2(1f, 0f));

                chunkRenderer.AddQuadFace(firstIndex, firstIndex, firstIndex + 1, firstIndex + 2, firstIndex + 3, true);
            }

            if (needBottomFace)
            {
                int firstIndex = chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX),
                    (VoxelBlock.DefaultBlockSize.y * curY),
                    (VoxelBlock.DefaultBlockSize.z * curZ)),
                    Vector3.forward, new Vector2(0f, 1f));

                chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX),
                    (VoxelBlock.DefaultBlockSize.y * curY),
                    (VoxelBlock.DefaultBlockSize.z * curZ) + (VoxelBlock.DefaultBlockSize.z)),
                    Vector3.forward, new Vector2(0f, 0f));

                chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX) + (VoxelBlock.DefaultBlockSize.x),
                    (VoxelBlock.DefaultBlockSize.y * curY),
                    (VoxelBlock.DefaultBlockSize.z * curZ) + (VoxelBlock.DefaultBlockSize.z)),
                    Vector3.forward, new Vector2(1f, 0f));

                chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX) + (VoxelBlock.DefaultBlockSize.x),
                    (VoxelBlock.DefaultBlockSize.y * curY),
                    (VoxelBlock.DefaultBlockSize.z * curZ)),
                    Vector3.forward, new Vector2(1f, 1f));

                chunkRenderer.AddQuadFace(firstIndex, firstIndex + 2, firstIndex + 1, firstIndex, firstIndex + 3, true);
            }

            if (needFrontFace)
            {
                int firstIndex = chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX),
                    (VoxelBlock.DefaultBlockSize.y * curY) + (VoxelBlock.DefaultBlockSize.y),
                    (VoxelBlock.DefaultBlockSize.z * curZ) + (VoxelBlock.DefaultBlockSize.z)),
                    -Vector3.up, new Vector2(0f, 0f));

                chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX),
                    (VoxelBlock.DefaultBlockSize.y * curY),
                    (VoxelBlock.DefaultBlockSize.z * curZ) + (VoxelBlock.DefaultBlockSize.z)),
                    -Vector3.up, new Vector2(0f, 1f));

                chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX) + (VoxelBlock.DefaultBlockSize.x),
                    (VoxelBlock.DefaultBlockSize.y * curY),
                    (VoxelBlock.DefaultBlockSize.z * curZ) + (VoxelBlock.DefaultBlockSize.z)),
                    -Vector3.up, new Vector2(1f, 1f));

                chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX) + (VoxelBlock.DefaultBlockSize.x),
                    (VoxelBlock.DefaultBlockSize.y * curY) + (VoxelBlock.DefaultBlockSize.y),
                    (VoxelBlock.DefaultBlockSize.z * curZ) + (VoxelBlock.DefaultBlockSize.z)),
                    -Vector3.up, new Vector2(1f, 0f));

                chunkRenderer.AddQuadFace(firstIndex, firstIndex, firstIndex + 1, firstIndex + 2, firstIndex + 3, true);
            }

            if (needBackFace)
            {
                int firstIndex = chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX),
                    (VoxelBlock.DefaultBlockSize.y * curY) + (VoxelBlock.DefaultBlockSize.y),
                    (VoxelBlock.DefaultBlockSize.z * curZ)),
                    -Vector3.up, new Vector2(0f, 1f));

                chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX),
                    (VoxelBlock.DefaultBlockSize.y * curY),
                    (VoxelBlock.DefaultBlockSize.z * curZ)),
                    -Vector3.up, new Vector2(0f, 0f));

                chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX) + (VoxelBlock.DefaultBlockSize.x),
                    (VoxelBlock.DefaultBlockSize.y * curY),
                    (VoxelBlock.DefaultBlockSize.z * curZ)),
                    -Vector3.up, new Vector2(1f, 0f));

                chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX) + (VoxelBlock.DefaultBlockSize.x),
                    (VoxelBlock.DefaultBlockSize.y * curY) + (VoxelBlock.DefaultBlockSize.y),
                    (VoxelBlock.DefaultBlockSize.z * curZ)),
                    -Vector3.up, new Vector2(1f, 1f));

                chunkRenderer.AddQuadFace(firstIndex, firstIndex + 2, firstIndex + 1, firstIndex, firstIndex + 3, true);
            }

            if (needRightFace)
            {
                int firstIndex = chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX) + (VoxelBlock.DefaultBlockSize.x),
                    (VoxelBlock.DefaultBlockSize.y * curY) + (VoxelBlock.DefaultBlockSize.y),
                    (VoxelBlock.DefaultBlockSize.z * curZ)),
                    Vector3.forward, new Vector2(0f, 0f));

                chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX) + (VoxelBlock.DefaultBlockSize.x),
                    (VoxelBlock.DefaultBlockSize.y * curY) + (VoxelBlock.DefaultBlockSize.y),
                    (VoxelBlock.DefaultBlockSize.z * curZ) + (VoxelBlock.DefaultBlockSize.z)),
                    Vector3.forward, new Vector2(0f, 1f));

                chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX) + (VoxelBlock.DefaultBlockSize.x),
                    (VoxelBlock.DefaultBlockSize.y * curY),
                    (VoxelBlock.DefaultBlockSize.z * curZ) + (VoxelBlock.DefaultBlockSize.z)),
                    Vector3.forward, new Vector2(1f, 1f));

                chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX) + (VoxelBlock.DefaultBlockSize.x),
                    (VoxelBlock.DefaultBlockSize.y * curY),
                    (VoxelBlock.DefaultBlockSize.z * curZ)),
                    Vector3.forward, new Vector2(1f, 0f));

                chunkRenderer.AddQuadFace(firstIndex, firstIndex, firstIndex + 1, firstIndex + 2, firstIndex + 3, true);
            }

            if (needLeftFace)
            {
                int firstIndex = chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX),
                    (VoxelBlock.DefaultBlockSize.y * curY) + (VoxelBlock.DefaultBlockSize.y),
                    (VoxelBlock.DefaultBlockSize.z * curZ)),
                    Vector3.forward, new Vector2(0f, 0f));

                chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX),
                    (VoxelBlock.DefaultBlockSize.y * curY) + (VoxelBlock.DefaultBlockSize.y),
                    (VoxelBlock.DefaultBlockSize.z * curZ) + (VoxelBlock.DefaultBlockSize.z)),
                    Vector3.forward, new Vector2(0f, 1f));

                chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX),
                    (VoxelBlock.DefaultBlockSize.y * curY),
                    (VoxelBlock.DefaultBlockSize.z * curZ) + (VoxelBlock.DefaultBlockSize.z)),
                    Vector3.forward, new Vector2(1f, 1f));

                chunkRenderer.AddVertexAndUV(new Vector3(
                    (VoxelBlock.DefaultBlockSize.x * curX),
                    (VoxelBlock.DefaultBlockSize.y * curY),
                    (VoxelBlock.DefaultBlockSize.z * curZ)),
                    Vector3.forward, new Vector2(1f, 0f));

                chunkRenderer.AddQuadFace(firstIndex, firstIndex + 2, firstIndex + 1, firstIndex, firstIndex + 3, true);
            }
            #endregion
        }
    }
}