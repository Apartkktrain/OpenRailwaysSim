﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class Axle : MapObject
{

    public const string KEY_ON_TRACK = "ON_TRACK";
    public const string KEY_ON_DIST = "ON_DIST";
    public const string KEY_SPEED = "SPEED";
    public const string KEY_WHEEL_DIA = "WHEEL_DIA";
    public const string KEY_ROT_X = "ROT_X";

    public Track onTrack { get; protected set; }

    protected float _onDist = 0;

    public float onDist
    {
        get { return _onDist; }
        set
        {
            if (onTrack.length < value)
            {
                if (onTrack.connectingNextTrack == -1)
                {
                    _onDist = onTrack.length;
                    speed = 0;
                }
                else
                {
                    Track oldTrack = onTrack;
                    onTrack = oldTrack.nextTracks[oldTrack.connectingNextTrack];
                    if ((oldTrack is Curve
                            ? ((Curve) oldTrack).getRotation(1)
                            : oldTrack.rot) == oldTrack.nextTracks[oldTrack.connectingNextTrack].rot)
                    {
                        onDist = value - oldTrack.length;
                        oldTrack = oldTrack.nextTracks[oldTrack.connectingNextTrack];
                    }
                    else
                    {
                        speed = -speed;
                        onDist = oldTrack.nextTracks[oldTrack.connectingNextTrack].length - value + oldTrack.length;
                    }
                }
            }
            else if (value < 0)
            {
                if (onTrack.connectingPrevTrack == -1)
                {
                    _onDist = 0;
                    speed = 0;
                }
                else
                {
                    Track oldTrack = onTrack;
                    onTrack = oldTrack.prevTracks[oldTrack.connectingPrevTrack];
                    if (oldTrack.rot == (oldTrack.prevTracks[oldTrack.connectingPrevTrack] is Curve
                            ? ((Curve) oldTrack.prevTracks[oldTrack.connectingPrevTrack]).getRotation(1)
                            : oldTrack.prevTracks[oldTrack.connectingPrevTrack].rot))
                    {
                        onDist = oldTrack.prevTracks[oldTrack.connectingPrevTrack].length + value;
                    }
                    else
                    {
                        speed = -speed;
                        onDist = -value;
                    }
                }
            }
            else
                _onDist = value;
        }
    }

    public float speed;

    public float wheelDia;

    public float rotX;

    public GameObject modelObj;

    public float lastFixed = -1;

    public Axle(Map map, Track onTrack, float onDist) : base(map)
    {
        this.onTrack = onTrack;
        this.onDist = onDist;
        speed = 5;
        wheelDia = 0.86f;
        rotX = 0;
        Vector3 a = onTrack is Curve
            ? ((Curve) onTrack).getRotation(onDist / onTrack.length).eulerAngles
            : onTrack.rot.eulerAngles;
        pos = onTrack.getPoint(onDist / onTrack.length) + Quaternion.Euler(a) * Vector3.up * wheelDia / 2;
        a.x = rotX;
        rot = Quaternion.Euler(a);
    }

    protected Axle(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        onTrack = (Track) info.GetValue(KEY_ON_TRACK, typeof(Track));
        _onDist = info.GetSingle(KEY_ON_DIST);
        speed = info.GetSingle(KEY_SPEED);
        wheelDia = info.GetSingle(KEY_WHEEL_DIA);
        rotX = info.GetSingle(KEY_ROT_X);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_ON_TRACK, onTrack);
        info.AddValue(KEY_ON_DIST, _onDist);
        info.AddValue(KEY_SPEED, speed);
        info.AddValue(KEY_WHEEL_DIA, wheelDia);
        info.AddValue(KEY_ROT_X, rotX);
    }

    public override void generate()
    {
        if (entity == null)
            (entity = new GameObject("axle").AddComponent<MapEntity>()).init(this);
        else
            reloadEntity();
    }

    public override void update()
    {
        reloadEntity();
    }

    public override void fixedUpdate()
    {
        fixedMove();
    }

    public void fixedMove()
    {
        if (lastFixed == Time.fixedTime)
            return;
        float a = speed * 10 * Time.deltaTime / 36;
        onDist += a;
        rotX += a * 360 / Mathf.PI * wheelDia;
        lastFixed = Time.fixedTime;
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        Vector3 a = onTrack is Curve
            ? ((Curve) onTrack).getRotation(onDist / onTrack.length).eulerAngles
            : onTrack.rot.eulerAngles;
        pos = onTrack.getPoint(onDist / onTrack.length) + Quaternion.Euler(a) * Vector3.up * wheelDia / 2;
        a.x = rotX;
        rot = Quaternion.Euler(a);

        if (modelObj == null)
        {
            (modelObj = GameObject.Instantiate(Main.main.axleModel)).transform.parent = entity.transform;
            modelObj.transform.localPosition = Vector3.zero;
            modelObj.transform.localEulerAngles = Vector3.zero;
        }

        if (useSelectingMat)
        {
            Renderer[] b = modelObj.GetComponentsInChildren<Renderer>();
            foreach (var c in b)
            {
                if (c.sharedMaterials[c.sharedMaterials.Length - 1] != Main.main.selecting_track_mat)
                {
                    Material[] d = new Material[c.sharedMaterials[c.sharedMaterials.Length - 1] == Main.main.focused_track_mat
                        ? c.sharedMaterials.Length
                        : c.sharedMaterials.Length + 1];
                    for (int e = 0; e < d.Length - 1; e++)
                        d[e] = c.sharedMaterials[e];
                    d[d.Length - 1] = Main.main.selecting_track_mat;
                    c.sharedMaterials = d;
                }
            }
        }
        else if (Main.focused == this)
        {
            Renderer[] b = modelObj.GetComponentsInChildren<Renderer>();
            foreach (var c in b)
            {
                if (c.sharedMaterials[c.sharedMaterials.Length - 1] != Main.main.focused_track_mat)
                {
                    Material[] d = new Material[c.sharedMaterials[c.sharedMaterials.Length - 1] == Main.main.selecting_track_mat
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
            Renderer[] b = modelObj.GetComponentsInChildren<Renderer>();
            foreach (var c in b)
            {
                if (c.sharedMaterials.Length >= 1 && (c.sharedMaterials[c.sharedMaterials.Length - 1] == Main.main.selecting_track_mat ||
                                                c.sharedMaterials[c.sharedMaterials.Length - 1] == Main.main.focused_track_mat))
                {
                    Material[] d = new Material[c.sharedMaterials.Length - 1];
                    for (int e = 0; e < d.Length; e++)
                        d[e] = c.sharedMaterials[e];
                    c.sharedMaterials = d;
                }
            }
        }

        reloadCollider();

        base.reloadEntity();
    }

    public void reloadCollider()
    {
        BoxCollider collider = entity.GetComponent<BoxCollider>();
        if (collider == null)
            collider = entity.gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.center = Vector3.up * wheelDia / 2;
        collider.size = new Vector3(wheelDia, wheelDia, wheelDia);
    }
}