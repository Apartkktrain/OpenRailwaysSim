﻿using System;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// 連結器
/// </summary>
[Serializable]
public class Coupler : MapObject
{

    public const string KEY_BODY = "BODY";
    public const string KEY_IS_FRONT = "IS_FRONT";
    public const string KEY_HEIGHT = "HEIGHT";
    public const string KEY_LENGTH = "LENGTH";
    public const string KEY_CONNECTING_COUPLER = "CONNECTING_COUPLER";
    public const string KEY_LOCAL_ROT = "LOCAL_ROT";

    public const float COLLIDER_WIDTH = 0.28f;
    public const float COLLIDER_HEIGHT = 0.28f;
    public const float COLLIDER_DEPTH = 0.92f;

    public Body body;
    public bool isFront;
    public float height;
    public float length;
    public Coupler connectingCoupler;
    public Quaternion localRot;

    public GameObject modelObj;
    public float lastMoved = -1f;

    public Coupler(Map map) : base(map)
    {
        isFront = true;
        height = 0.845f;
        length = 0.92f;
        localRot = Quaternion.identity;
    }

    protected Coupler(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        body = (Body)info.GetValue(KEY_BODY, typeof(Body));
        isFront = info.GetBoolean(KEY_IS_FRONT);
        height = info.GetSingle(KEY_HEIGHT);
        length = info.GetSingle(KEY_LENGTH);
        connectingCoupler = (Coupler)info.GetValue(KEY_CONNECTING_COUPLER, typeof(Coupler));
        localRot = ((SerializableQuaternion)info.GetValue(KEY_LOCAL_ROT, typeof(SerializableQuaternion))).toQuaternion();
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_BODY, body);
        info.AddValue(KEY_IS_FRONT, isFront);
        info.AddValue(KEY_HEIGHT, height);
        info.AddValue(KEY_LENGTH, length);
        info.AddValue(KEY_CONNECTING_COUPLER, connectingCoupler);
        info.AddValue(KEY_LOCAL_ROT, new SerializableQuaternion(localRot));
    }

    public override void generate()
    {
        if (entity)
            reloadEntity();
        else
            (entity = new GameObject("Coupler").AddComponent<MapEntity>()).init(this);
    }

    public override void update()
    {
        snapTo();
        snapFrom();
        reloadEntity();
    }

    /// <summary>
    /// 連結器を車体と相手の連結器に合わせる
    /// </summary>
    public void snapTo()
    {
        if (lastMoved == Time.time)
            return;
        lastMoved = Time.time;

        body.snapToBogieFrame();
        pos = body.pos + body.rot * new Vector3(0, height - body.bogieHeight, (body.carLength / 2 - length) * (isFront ? 1 : -1));

        if (connectingCoupler == null)
        {
            var e = Quaternion.Euler((isFront ? body.rot.eulerAngles : body.rot.eulerAngles + Vector3.up * -180) + localRot.eulerAngles);
            rot = e;
        }
        else
        {
            var f = connectingCoupler.pos - pos;
            if (f.sqrMagnitude != 0f)
                rot = Quaternion.LookRotation(f, Quaternion.Lerp(body.rot, connectingCoupler.rot, 0.5f) * Vector3.up);
        }
    }

    /// <summary>
    /// 車体を連結器に合わせる
    /// </summary>
    public void snapFrom()
    {
        body.pos = pos + body.rot * new Vector3(0, body.bogieHeight - height, (length - body.carLength / 2) * (isFront ? 1 : -1));
        if (connectingCoupler != null)
            body.speed = (body.speed + connectingCoupler.body.speed);
        body.snapFromBogieFrame();
    }

    public override void reloadEntity()
    {
        if (entity == null)
            return;

        if (modelObj == null)
        {
            (modelObj = GameObject.Instantiate(Main.INSTANCE.couplerModel)).transform.parent = entity.transform;
            modelObj.transform.localPosition = Vector3.zero;
            modelObj.transform.localEulerAngles = Vector3.zero;
            reloadCollider();
        }

        reloadMaterial(modelObj);

        base.reloadEntity();
    }

    public void reloadCollider()
    {
        var collider = entity.GetComponent<BoxCollider>();
        if (collider == null)
            collider = entity.gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.center = Vector3.forward * COLLIDER_DEPTH / 2;
        collider.size = new Vector3(COLLIDER_WIDTH, COLLIDER_HEIGHT, COLLIDER_DEPTH);
    }
}
