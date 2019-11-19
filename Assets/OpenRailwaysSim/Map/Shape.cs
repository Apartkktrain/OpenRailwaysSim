﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// 平面曲線と縦断曲線を設定する軌道
/// </summary>
[Serializable]
public class Shape : Track
{

    public const string KEY_CURVE_LENGTH = "CURVE_L";
    public const string KEY_CURVE_RADIUS = "CURVE_R";
    public const string KEY_VERTICAL_CURVE_LENGTH = "V_CURVE_L";
    public const string KEY_VERTICAL_CURVE_RADIUS = "V_CURVE_R";
    public const float FINENESS_DISTANCE = 5f;

    public List<float> curveLength;
    public List<float> curveRadius;
    public List<float> verticalCurveLength;
    public List<float> verticalCurveRadius;
    public BoxCollider[] colliders = new BoxCollider[0];

    public Shape(Map map, Vector3 pos) : base(map, pos)
    {
        curveLength = new List<float>();
        curveRadius = new List<float>();
        verticalCurveLength = new List<float>();
        verticalCurveRadius = new List<float>();
    }

    public Shape(Map map, Vector3 pos, Quaternion rot) : base(map, pos, rot)
    {
        curveLength = new List<float>();
        curveRadius = new List<float>();
        verticalCurveLength = new List<float>();
        verticalCurveRadius = new List<float>();
    }

    protected Shape(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        curveLength = (List<float>)info.GetValue(KEY_CURVE_LENGTH, typeof(List<float>));
        curveRadius = (List<float>)info.GetValue(KEY_CURVE_RADIUS, typeof(List<float>));
        verticalCurveLength = (List<float>)info.GetValue(KEY_VERTICAL_CURVE_LENGTH, typeof(List<float>));
        verticalCurveRadius = (List<float>)info.GetValue(KEY_VERTICAL_CURVE_RADIUS, typeof(List<float>));
        reloadLength();
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(KEY_CURVE_LENGTH, curveLength);
        info.AddValue(KEY_CURVE_RADIUS, curveRadius);
        info.AddValue(KEY_VERTICAL_CURVE_LENGTH, verticalCurveLength);
        info.AddValue(KEY_VERTICAL_CURVE_RADIUS, verticalCurveRadius);
    }

    public override void generate()
    {
        if (entity)
            reloadEntity();
        else
            (entity = new GameObject("Shape").AddComponent<MapEntity>()).init(this);
    }

    public override void reloadTrackRendererPositions()
    {
        int l = Mathf.CeilToInt(_length / FINENESS_DISTANCE);
        Vector3[] p = new Vector3[l + 1];
        p[0] = pos;
        for (int a = 1; a <= l; a++)
            p[a] = getPoint((float)a / (float)l);
        trackRenderer.positionCount = p.Length;
        trackRenderer.SetPositions(p);
    }

    public override void reloadRailRenderers()
    {
        if (railRenderers != null)
            foreach (var r in railRenderers)
                GameObject.Destroy(r.gameObject);
        if (!Main.INSTANCE.runPanel.isShowing() && Main.showGuide)
        {
            railRenderers = new LineRenderer[2];
            for (int a = 0; a < 2; a++)
            {
                GameObject o = new GameObject();
                railRenderers[a] = o.AddComponent<LineRenderer>();
                o.transform.parent = entity.transform;
                railRenderers[a].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                railRenderers[a].receiveShadows = false;
                railRenderers[a].endWidth = railRenderers[a].startWidth = RAIL_RENDER_WIDTH;
                railRenderers[a].endColor = railRenderers[a].startColor = Color.white;
                if (useSelectingMat)
                    railRenderers[a].sharedMaterial = Main.INSTANCE.selecting_track_mat;
                else if (Main.focused == this)
                    railRenderers[a].sharedMaterial = Main.INSTANCE.focused_track_mat;
                else
                    railRenderers[a].sharedMaterial = Main.INSTANCE.rail_mat;

                int l = Mathf.CeilToInt(_length / FINENESS_DISTANCE);
                Vector3[] p = new Vector3[l + 1];
                p[0] = pos + rot * Vector3.right * (a == 0 ? -gauge / 2f : gauge / 2f);
                for (int b = 1; b <= l; b++)
                    p[b] = getPoint((float)b / (float)l) + getRotation((float)b / (float)l) * Vector3.right * (a == 0 ? -gauge / 2f : gauge / 2f);
                railRenderers[a].positionCount = p.Length;
                railRenderers[a].SetPositions(p);
            }
        }
        else
            railRenderers = null;
    }

    public override void reloadModels()
    {
        if (railModelObjs != null)
            foreach (var r in railModelObjs)
                GameObject.Destroy(r.gameObject);
        var r_ = Quaternion.Inverse(rot);
        railModelObjs = new GameObject[Mathf.CeilToInt(_length / RAIL_MODEL_INTERVAL) * 2];
        GameObject b;
        for (int a = 0; a < railModelObjs.Length / 2; a++)
        {
            railModelObjs[a] = b = GameObject.Instantiate(Main.INSTANCE.railLModel);
            b.transform.parent = entity.transform;
            setLOD(b, LOD_DISTANCE);
            var d = (float)a / (railModelObjs.Length / 2);
            var p = getPoint(d);
            b.transform.localPosition = r_ * (p - pos);
            var f = getPoint(((float)a + 1) / (railModelObjs.Length / 2)) - p;
            if (f.sqrMagnitude != 0f)
                b.transform.localRotation = r_ * Quaternion.LookRotation(f);
        }
        for (int a = 0; a < railModelObjs.Length / 2; a++)
        {
            railModelObjs[a + railModelObjs.Length / 2] = b = GameObject.Instantiate(Main.INSTANCE.railRModel);
            b.transform.parent = entity.transform;
            setLOD(b, LOD_DISTANCE);
            var d = (float)a / (railModelObjs.Length / 2);
            var p = getPoint(d);
            b.transform.localPosition = r_ * (p - pos);
            var f = getPoint(((float)a + 1) / (railModelObjs.Length / 2)) - p;
            if (f.sqrMagnitude != 0f)
                b.transform.localRotation = r_ * Quaternion.LookRotation(f);
        }

        if (tieModelObjs != null)
            foreach (var r in tieModelObjs)
                GameObject.Destroy(r.gameObject);
        tieModelObjs = new GameObject[Mathf.CeilToInt(_length / TIE_MODEL_INTERVAL)];
        for (int a = 0; a < tieModelObjs.Length; a++)
        {
            (tieModelObjs[a] = GameObject.Instantiate(Main.INSTANCE.tieModel)).transform.parent = entity.transform;
            setLOD(tieModelObjs[a], LOD_DISTANCE);
            var d = (float)a / tieModelObjs.Length;
            tieModelObjs[a].transform.localPosition = r_ * (getPoint(d) - pos);
            tieModelObjs[a].transform.localRotation = r_ * getRotation(d);
        }
    }

    public override void reloadCollider()
    {
        int l = Mathf.CeilToInt(_length / FINENESS_DISTANCE);
        if (colliders.Length != l)
        {
            for (int a = 0; a < colliders.Length; a++)
                if (colliders[a])
                    GameObject.Destroy(colliders[a].gameObject);
            colliders = new BoxCollider[l];
        }
        for (int a = 0; a < l; a++)
        {
            if (colliders[a] == null)
            {
                GameObject o = new GameObject();
                colliders[a] = o.AddComponent<BoxCollider>();
                o.transform.parent = entity.transform;
            }
            colliders[a].isTrigger = true;

            Quaternion b = Quaternion.Inverse(rot);
            Vector3 c = b * (getPoint((float)a / (float)l) - pos);
            Vector3 d = b * (getPoint(((float)a + 1) / (float)l) - pos);
            colliders[a].transform.localPosition = (c + d) / 2;
            colliders[a].transform.localRotation = b * getRotation(((float)a + 1f / 2) / (float)l);
            colliders[a].size = new Vector3(COLLIDER_WIDTH, COLLIDER_HEIGHT, Vector3.Distance(c, d));
            colliders[a].enabled = enableCollider;
        }
    }

    /// <summary>
    /// 軌道の座標を返す
    /// </summary>
    /// <param name="a">平面における位置</param>
    public override Vector3 getPoint(float a)
    {
        var p = pos;
        var r = rot;

        var l1 = 0f;
        foreach (var l_ in curveLength)
            l1 += l_;

        // 平面曲線を計算
        var rad = 0f;
        var l2 = 0f;
        bool b = true;
        float c;
        float l3;
        for (var n = 0; n < curveLength.Count; n++)
        {
            b = l1 * a <= l2 + curveLength[n];
            c = b ? (l1 * a - l2) / curveLength[n] : 1f;
            l3 = curveLength[n] * c;
            if ((rad = curveRadius[n]) == 0f)
                p += r * Vector3.forward * l3;
            else
            {
                var d1 = l3 / Mathf.Abs(rad);
                p += Quaternion.Euler(0f, r.eulerAngles.y, 0f) * new Vector3((1f - Mathf.Cos(d1)) * rad, Mathf.Sin(-r.eulerAngles.x * Mathf.Deg2Rad) * l3, Mathf.Sin(d1) * Mathf.Abs(rad));
                r = Quaternion.Euler(0f, r.eulerAngles.y, 0f) * Quaternion.Euler(r.eulerAngles.x, l3 * Mathf.Rad2Deg / rad, 0f);
            }
            l2 += l3;

            if (b)
                break;
        }

        // 縦断曲線を計算
        r = rot;
        rad = 0f;
        l2 = 0f;
        for (var n = 0; n < verticalCurveLength.Count; n++)
        {
            b = l1 * a <= l2 + verticalCurveLength[n];
            c = b ? (l1 * a - l2) / verticalCurveLength[n] : 1f;
            l3 = verticalCurveLength[n] * c;
            if ((rad = verticalCurveRadius[n]) == 0f)
                p.y += Mathf.Tan(-r.eulerAngles.x * Mathf.Deg2Rad) * l3;
            else
            {
                var t = -r.eulerAngles.x * Mathf.Deg2Rad; // 縦曲線始点の角度。負数なら始点は下り勾配
                var l4 = Mathf.Tan(t) * rad; // 曲線中心を基準とした縦曲線始点の横位置
                var l5 = l4 + l3; // 曲線中心を基準とした縦曲線終点の横位置

                var r1 = Mathf.Abs(rad);
                var h = Mathf.Sqrt(r1 * r1 - l4 * l4); // 曲線中心と縦曲線始点の高低差の絶対値
                if (!float.IsNaN(h))
                {
                    var h1 = Mathf.Sqrt(r1 * r1 - l5 * l5); // 曲線中心と縦曲線終点の高低差の絶対値
                    if (!float.IsNaN(h1))
                    {
                        p.y += rad < 0f ? h1 - h : h - h1;

                        var t1 = Mathf.Asin(l5 / rad); // 曲線中心から縦曲線終点までの角度
                        r *= Quaternion.Euler(-(t1 - t) * Mathf.Rad2Deg, 0f, 0f);
                    }
                }
            }
            l2 += l3;

            if (b)
                break;
        }
        if (!b)
            p.y += Mathf.Tan(-r.eulerAngles.x * Mathf.Deg2Rad) * (l1 * a - l2);

        return p;
    }

    /// <summary>
    /// 軌道の回転を返す
    /// </summary>
    /// <param name="a">平面における位置</param>
    public override Quaternion getRotation(float a)
    {
        var r = rot;

        var l1 = 0f;
        foreach (var l_ in curveLength)
            l1 += l_;

        // 平面曲線を計算
        var rad = 0f;
        var l2 = 0f;
        bool b;
        float c;
        float l3;
        for (var n = 0; n < curveLength.Count; n++)
        {
            b = l1 * a <= l2 + curveLength[n];
            c = b ? (l1 * a - l2) / curveLength[n] : 1f;
            l3 = curveLength[n] * c;
            if ((rad = curveRadius[n]) != 0f)
                r = Quaternion.Euler(0f, r.eulerAngles.y, 0f) * Quaternion.Euler(r.eulerAngles.x, l3 * Mathf.Rad2Deg / rad, 0f);
            l2 += l3;

            if (b)
                break;
        }

        // 縦断曲線を計算
        var vr = rot;
        rad = 0f;
        l2 = 0f;
        for (var n = 0; n < verticalCurveLength.Count; n++)
        {
            b = l1 * a <= l2 + verticalCurveLength[n];
            c = b ? (l1 * a - l2) / verticalCurveLength[n] : 1f;
            l3 = verticalCurveLength[n] * c;
            if ((rad = verticalCurveRadius[n]) != 0f)
            {
                var t = -vr.eulerAngles.x * Mathf.Deg2Rad;
                var l5 = Mathf.Tan(t) * rad + l3;

                var f = l5 / rad;
                if (-1f <= f && f <= 1f)
                {
                    var t1 = Mathf.Asin(l5 / rad);
                    vr *= Quaternion.Euler(-(t1 - t) * Mathf.Rad2Deg, 0f, 0f);
                }
            }
            l2 += l3;

            if (b)
                break;
        }

        return Quaternion.Euler(0f, r.eulerAngles.y, 0f) * Quaternion.Euler(vr.eulerAngles.x, 0f, 0f);
    }

    /// <summary>
    /// 軌道の実際の長さを更新する
    /// </summary>
    public void reloadLength()
    {
        var l = 0f;

        var l1 = 0f;
        foreach (var l_ in curveLength)
            l1 += l_;

        var vr = rot;
        var rad = 0f;
        var l2 = 0f;
        for (var n = 0; n < verticalCurveLength.Count; n++)
        {
            l2 += verticalCurveLength[n];
            if ((rad = verticalCurveRadius[n]) == 0f)
                l += verticalCurveLength[n] / Mathf.Cos(vr.eulerAngles.x * Mathf.Deg2Rad);
            else
            {
                var t = -vr.eulerAngles.x * Mathf.Deg2Rad;
                var l5 = Mathf.Tan(t) * rad + verticalCurveLength[n];

                var f = l5 / rad;
                if (-1f <= f && f <= 1f)
                {
                    var r1 = Mathf.Abs(rad);
                    var t1 = Mathf.Asin(l5 / r1);
                    vr *= Quaternion.Euler(-(t1 - t) * Mathf.Rad2Deg, 0f, 0f);
                    var t2 = Mathf.Abs(Mathf.Repeat(Mathf.Abs(t1 - t) + Mathf.PI / 2f, Mathf.PI) - Mathf.PI / 2f);
                    l += t2 * r1 * 2f;
                }
            }
        }

        l += (l1 - l2) / Mathf.Cos(vr.eulerAngles.x * Mathf.Deg2Rad);
        length = l;
    }
}
