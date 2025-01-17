﻿//////////////////////////////////////////////////////////////////////////////////////////
// Every Day Space Station
// http://everydayspacestation.tumblr.com
//////////////////////////////////////////////////////////////////////////////////////////
// PoolManager - A Singleton'd Unity Monobehaviour based class for coordinating all pool stuff
// Should start disabled as component
// Created: December 7 2015
// CasualSimpleton <casualsimpleton@gmail.com>
// Last Modified: December 7 2015
// CasualSimpleton
//////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

using EveryDaySpaceStation;
using EveryDaySpaceStation.DataTypes;
using EveryDaySpaceStation.Utils;

public class PoolManager : MonoBehaviour
{
    #region Singleton
    protected static PoolManager m_singleton = null;
    public static PoolManager Singleton
    {
        get
        {
            return m_singleton;
        }
    }

    void Awake()
    {
        m_singleton = this;
        _transform = this.transform;
    }
    #endregion

    #region Vars
    public Transform _transform { get; private set; }

    public bool IsInit { get; private set; }

    protected PoolSceneChunkRenderer _sceneChunkRendererPool;
    protected PoolSceneChunkScaffoldRenderer _sceneChunkScaffoldRendererPool;
    protected PoolEntitySpriteGameObject _entitySpritesGameObjectPool;
    protected PoolEntitySpriteGraphics _entitySpritesGraphicPool;
    protected PoolMeshQuad _meshQuadPool;
    protected PoolCubeCollider _cubeColliderPool;
    protected PoolBoundsDrawing _boundsDrawerPool;
    #endregion

    #region Scene Chunk Renderers
    public SceneChunkRenderer RequestSceneChunkRenderer()
    {
        return _sceneChunkRendererPool.RequestObject();
    }

    public void ReturnSceneChunkRenderer(SceneChunkRenderer scr)
    {
        _sceneChunkRendererPool.ReturnObject(scr);
    }
    #endregion

    #region Scene Chunk Scaffold Renderers
    public SceneChunkScaffoldRenderer RequestSceneChunkScaffolderRenderer()
    {
        return _sceneChunkScaffoldRendererPool.RequestObject();
    }

    public void ReturnSceneChunkRenderer(SceneChunkScaffoldRenderer sscr)
    {
        _sceneChunkScaffoldRendererPool.ReturnObject(sscr);
    }
    #endregion

    #region Entity GOs
    public EntitySpriteGameObject RequestEntitySpriteGameObject()
    {
        return _entitySpritesGameObjectPool.RequestObject();
    }

    public void ReturnEntitySpriteGameObject(EntitySpriteGameObject es)
    {
        _entitySpritesGameObjectPool.ReturnObject(es);
    }
    #endregion

    #region Entity Sprites GOs
    public EntitySpriteGraphics RequestEntitySpriteGraphics()
    {
        return _entitySpritesGraphicPool.RequestObject();
    }

    public void ReturnEntitySpriteGraphic(EntitySpriteGraphics es)
    {
        _entitySpritesGraphicPool.ReturnObject(es);
    }
    #endregion

    #region Mesh Quads
    public MeshQuad RequestMeshQuad()
    {
        return _meshQuadPool.RequestObject();
    }

    public void ReturnMeshQuad(MeshQuad mq)
    {
        _meshQuadPool.ReturnObject(mq);
    }
    #endregion

    #region Cube Colliders
    public CubeCollider RequestCubeCollider()
    {
        return _cubeColliderPool.RequestObject();
    }

    public void ReturnCubeCollider(CubeCollider cc)
    {
        _cubeColliderPool.ReturnObject(cc);
    }
    #endregion

    #region Bounds Line Drawer
    public BoundsDrawing RequestBoundsDrawer()
    {
        return _boundsDrawerPool.RequestObject();
    }

    public void ReturnCubeCollider(BoundsDrawing bd)
    {
        _boundsDrawerPool.ReturnObject(bd);
    }
    #endregion

    public void EarlyInit()
    {
    }

    public void Init()
    {
        _cubeColliderPool = new PoolCubeCollider();
        _cubeColliderPool.Init(10, Vector3.one, 0.1f, 2, 0.1f);
        
        _entitySpritesGameObjectPool = new PoolEntitySpriteGameObject();
        _entitySpritesGameObjectPool.Init(10, 0.1f, 2, 0.1f);

        _entitySpritesGraphicPool = new PoolEntitySpriteGraphics();
        _entitySpritesGraphicPool.Init(10, 0.1f, 2, 0.1f);

        _boundsDrawerPool = new PoolBoundsDrawing();
        _boundsDrawerPool.Init(10, 0.1f, 2, 0.1f);
    }

    public void LateInit()
    {
        _sceneChunkRendererPool = new PoolSceneChunkRenderer();
        _sceneChunkScaffoldRendererPool = new PoolSceneChunkScaffoldRenderer();
        _meshQuadPool = new PoolMeshQuad();

        _sceneChunkRendererPool.Init(4, 0.1f, 5, 0.25f);
        _sceneChunkScaffoldRendererPool.Init(4, 0.1f, 5, 0.25f);
        _meshQuadPool.Init(1, 0.1f, 5, 0.25f);

        IsInit = true;

        //This component should start off, otherwise Update() could be called before Init() and that'd be less than good
        this.enabled = true;
    }

    public void Update()
    {
        _sceneChunkRendererPool.Maintenance();
        _sceneChunkScaffoldRendererPool.Maintenance();
        _entitySpritesGameObjectPool.Maintenance();
        _entitySpritesGraphicPool.Maintenance();
        _meshQuadPool.Maintenance();
        _cubeColliderPool.Maintenance();
        _boundsDrawerPool.Maintenance();
    }

    public void Cleanup()
    {
        try
        {
            _sceneChunkRendererPool.Dispose();
            _sceneChunkScaffoldRendererPool.Dispose();
            _entitySpritesGameObjectPool.Dispose();
            _entitySpritesGraphicPool.Dispose();
            _meshQuadPool.Dispose();
            _cubeColliderPool.Dispose();
            _boundsDrawerPool.Dispose();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(string.Format("Problem with PoolManager.CleanUP() : '{0}'", ex.Message.ToString()));
        }
    }
}