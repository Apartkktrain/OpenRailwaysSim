﻿using System;
using System.Runtime.Serialization;
using UnityEngine;

//マップ上に存在するオブジェクトを管理するクラス
[Serializable]
public class MapObject : ISerializable
{
    public const string KEY_POS = "POS";
    public const string KEY_ROTATION = "ROT";

    public MapEntity entity;

    [NonSerialized] private Map _map;

    public Map map
    {
        get { return _map; }
        set
        {
            if (_map == null)
                _map = value;
        }
    }

    private Vector3 _pos;

    public Vector3 pos
    {
        get { return _pos; }
        set
        {
            _pos = value;
            if (entity != null)
                entity.transform.position = pos;
        }
    }

    private Quaternion _rot;

    public Quaternion rot
    {
        get { return _rot; }
        set
        {
            _rot = value;
            if (entity != null)
                entity.transform.rotation = rot;
        }
    }

    public bool useSelectingMat = false;

    public MapObject(Map map) : this(map, new Vector3(), Quaternion.identity)
    {
    }

    public MapObject(Map map, Vector3 pos) : this(map, pos, Quaternion.identity)
    {
    }

    public MapObject(Map map, Vector3 pos, Quaternion rot)
    {
        this.map = map;
        this._pos = pos;
        this._rot = rot;
    }

    protected MapObject(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException("info");
        _pos = ((SerializableVector3)info.GetValue(KEY_POS, typeof(SerializableVector3))).toVector3();
        _rot = ((SerializableQuaternion)info.GetValue(KEY_ROTATION, typeof(SerializableQuaternion))).toQuaternion();
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException("info");
        SyncFromEntity();
        info.AddValue(KEY_POS, new SerializableVector3(_pos));
        info.AddValue(KEY_ROTATION, new SerializableQuaternion(_rot));
    }

    public virtual void generate()
    {
        if (entity)
            reloadEntity();
        else
            (entity = new GameObject("MapObj").AddComponent<MapEntity>()).init(this);
    }

    public virtual void update()
    {
    }

    public virtual void fixedUpdate()
    {
    }

    public virtual void destroy()
    {
        entity = null;
    }

    public virtual void reloadEntity()
    {
        if (!entity)
            return;
        entity.transform.position = _pos;
        entity.transform.rotation = _rot;
    }

    public void reloadMaterial(GameObject obj)
    {
        if (useSelectingMat && !GameCanvas.runPanel.isShowing())
        {
            Renderer[] b = obj.GetComponentsInChildren<Renderer>();
            foreach (var c in b)
            {
                if (c.sharedMaterials[c.sharedMaterials.Length - 1] != Main.main.selecting_track_mat)
                {
                    Material[] d =
                        new Material[c.sharedMaterials[c.sharedMaterials.Length - 1] == Main.main.focused_track_mat
                            ? c.sharedMaterials.Length
                            : c.sharedMaterials.Length + 1];
                    for (int e = 0; e < d.Length - 1; e++)
                        d[e] = c.sharedMaterials[e];
                    d[d.Length - 1] = Main.main.selecting_track_mat;
                    c.sharedMaterials = d;
                }
            }
        }
        else if (Main.focused == this && !GameCanvas.runPanel.isShowing())
        {
            Renderer[] b = obj.GetComponentsInChildren<Renderer>();
            foreach (var c in b)
            {
                if (c.sharedMaterials[c.sharedMaterials.Length - 1] != Main.main.focused_track_mat)
                {
                    Material[] d =
                        new Material[c.sharedMaterials[c.sharedMaterials.Length - 1] == Main.main.selecting_track_mat
                            ? c.sharedMaterials.Length
                            : c.sharedMaterials.Length + 1];
                    for (int e = 0; e < d.Length - 1; e++)
                        d[e] = c.sharedMaterials[e];
                    d[d.Length - 1] = Main.main.focused_track_mat;
                    c.sharedMaterials = d;
                }
            }
        }
        else
        {
            Renderer[] b = obj.GetComponentsInChildren<Renderer>();
            foreach (var c in b)
            {
                if (c.sharedMaterials.Length >= 1 &&
                    (c.sharedMaterials[c.sharedMaterials.Length - 1] == Main.main.selecting_track_mat ||
                     c.sharedMaterials[c.sharedMaterials.Length - 1] == Main.main.focused_track_mat))
                {
                    Material[] d = new Material[c.sharedMaterials.Length - 1];
                    for (int e = 0; e < d.Length; e++)
                        d[e] = c.sharedMaterials[e];
                    c.sharedMaterials = d;
                }
            }
        }
    }

    //時間が経過するメソッド。ticksには経過時間を指定。
    public virtual void TimePasses(long ticks)
    {
    }

    public virtual void SyncFromEntity()
    {
        if (entity)
        {
            _pos = entity.transform.position;
            _rot = entity.transform.rotation;
        }
    }
}
