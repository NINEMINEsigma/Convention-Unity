using System;
using System.Collections.Generic;
using UnityEngine;

namespace Convention
{
    public static class CameraUtility
    {
        /// <summary>
        /// 从摄像机出发, 通过视锥选取视野中指定的物体
        /// </summary>
        /// <param name="camera">目标摄像机</param>
        /// <param name="certer">目标中心点</param>
        /// <param name="width">中心点展开平面的宽度</param>
        /// <param name="height">中心点展开屏幕的高度</param>
        /// <param name="targets">可被选取的集合</param>
        /// <returns></returns>
        public static GameObject[] ViewFrustumHit(Camera camera,Vector3 certer,float width,float height, params GameObject[] targets)
        {
            if (camera == null || targets == null || targets.Length == 0)
            {
                return new GameObject[0];
            }

            // 计算摄像机的视锥平面
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
            
            // 用于存储符合条件的物体
            System.Collections.Generic.List<GameObject> hitObjects = new System.Collections.Generic.List<GameObject>();

            foreach (GameObject target in targets)
            {
                if (target == null)
                {
                    continue;
                }

                // 获取物体的包围盒
                Renderer renderer = target.GetComponent<Renderer>();
                if (renderer == null)
                {
                    continue;
                }

                Bounds bounds = renderer.bounds;

                // 检查物体是否在视锥范围内
                if (!GeometryUtility.TestPlanesAABB(frustumPlanes, bounds))
                {
                    continue;
                }

                // 计算从摄像机到中心点的方向（摄像机视线方向）
                Vector3 cameraToCenter = (certer - camera.transform.position).normalized;
                
                // 获取摄像机的右向量和上向量
                Vector3 cameraRight = camera.transform.right;
                Vector3 cameraUp = camera.transform.up;
                
                // 计算物体包围盒中心相对于中心点的向量
                Vector3 toTarget = bounds.center - certer;
                
                // 将向量投影到垂直于摄像机视线的平面上
                // 使用摄像机的right和up方向作为平面的两个轴
                float rightOffset = Vector3.Dot(toTarget, cameraRight);
                float upOffset = Vector3.Dot(toTarget, cameraUp);
                
                // 检查物体是否在指定的宽度和高度范围内（世界坐标）
                if (Mathf.Abs(rightOffset) <= width / 2f && Mathf.Abs(upOffset) <= height / 2f)
                {
                    hitObjects.Add(target);
                }
            }

            return hitObjects.ToArray();
        }

        /// <summary>
        /// 从摄像机出发, 透过指定UI选取视野中指定的物体
        /// </summary>
        /// <param name="camera">目标摄像机</param>
        /// <param name="rect">目标视锥UI平面</param>
        /// <param name="targets">可被选取的集合</param>
        /// <returns></returns>
        public static GameObject[] ThroughImageHit(Camera camera,RectTransform rect,params GameObject[] targets)
        {
            if (camera == null || rect == null || targets == null || targets.Length == 0)
            {
                return Array.Empty<GameObject>();
            }

            // 计算摄像机的视锥平面
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
            
            // 用于存储符合条件的物体
            List<GameObject> hitObjects = new();

            foreach (GameObject target in targets)
            {
                if (target == null)
                {
                    continue;
                }

                // 获取物体的包围盒
                Renderer renderer = target.GetComponent<Renderer>();
                if (renderer == null)
                {
                    continue;
                }

                Bounds bounds = renderer.bounds;

                // 检查物体是否在视锥范围内
                if (!GeometryUtility.TestPlanesAABB(frustumPlanes, bounds))
                {
                    continue;
                }

                // 将物体包围盒的中心点转换为屏幕坐标
                Vector3 screenPoint = camera.WorldToScreenPoint(bounds.center);

                // 检查屏幕坐标是否在RectTransform的矩形区域内
                // Overlay模式传null; Camera/World Space模式需传入Canvas的渲染相机
                Canvas canvas = rect.GetComponentInParent<Canvas>();
                Camera rectCamera = null;
                if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    rectCamera = canvas.worldCamera != null ? canvas.worldCamera : camera;
                }
                if (RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, rectCamera))
                {
                    hitObjects.Add(target);
                }
            }

            return hitObjects.ToArray();
        }
    }
}
