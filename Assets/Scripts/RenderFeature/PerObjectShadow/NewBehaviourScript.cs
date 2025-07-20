/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
            foreach (var render in characterBodyObjs)
            {
                SkinnedMeshRenderer r = render.Key;
                if (r.gameObject.layer == m_HideLayer)
                    continue;

                //在当前渲染列表里 不在视椎和不满足距离剔除
                bool InCamera = false;
                var bound = r.bounds;
                InCamera = IsVisibleByCamera(bound.center, bound.extents, main);
                float distance = Vector3.Distance(r.transform.position, main.transform.position);
                bool InDistance = distance < maxDistance;

                if (currenCullingOBJ.Contains(r))
                {
                    if (InCamera) //在视锥内
                    {
                        if (InDistance == false) //同时满足距离
                        {
                            currenCullingOBJ.Remove(r);
                        }
                    }
                    else
                    {
                        currenCullingOBJ.Remove(r);
                    }
                }
                else //不在渲染列表满足在视锥和 距离同时超过10按最近距离替换
                {
                    if (InDistance) //同时满足距离
                    {
                        if (currenCullingOBJ.Count < 10)
                        {
                            if (InCamera)
                            {
                                currenCullingOBJ.Add(r);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < currenCullingOBJ.Count; i++)
                            {
                                float disA = Vector3.Distance(currenCullingOBJ[i].transform.position,
                                    main.transform.position);
                                if (distance < disA)
                                {
                                    currenCullingOBJ[i] = r;
                                    return;
                                }
                            }
                        }
                    }
                }
            }


        private bool IsVisibleByCamera(float3 c, float3 e, Camera mainCamera)
        {
            var VP = mainCamera.projectionMatrix * mainCamera.worldToCameraMatrix;

            float3 fa = VP.m30 - new float3(VP.m00, VP.m10, VP.m20);
            float3 fb = VP.m31 - new float3(VP.m01, VP.m11, VP.m21);
            float3 fc = VP.m32 - new float3(VP.m02, VP.m12, VP.m22);
            float3 fd = VP.m33 - new float3(VP.m03, VP.m13, VP.m23);

            float3 fe = VP.m30 + new float3(VP.m00, VP.m10, VP.m20);
            float3 ff = VP.m31 + new float3(VP.m01, VP.m11, VP.m21);
            float3 fg = VP.m32 + new float3(VP.m02, VP.m12, VP.m22);
            float3 fh = VP.m33 + new float3(VP.m03, VP.m13, VP.m23);

            float3 d012 = fa * c.x + fb * c.y + fc * c.z + fd;
            float3 d345 = fe * c.x + ff * c.y + fg * c.z + fh;

            float3 r012 = math.abs(fa) * e.x + math.abs(fb) * e.y + math.abs(fc) * e.z;
            float3 r345 = math.abs(fe) * e.x + math.abs(ff) * e.y + math.abs(fg) * e.z;

            float3 f012 = d012 + r012;
            float3 f345 = d345 + r345;

            return math.all(f012 > 0) && math.all(f345 > 0);


            // 检查物体的渲染器边界是否在相机视锥体内
            //return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(mainCamera), renderer.bounds);
        }


        private Matrix4x4 GetMainLightViewMatrix(Light mainLight, Camera mainCamera)
        {
            Matrix4x4 view = Matrix4x4.identity;
            Vector3 up = Vector3.up;
            Vector3 lightDirection = -mainLight.transform.forward;
            Vector3 lightPosition = mainCamera.transform.position + mainLight.transform.position * m_ShadowDistanceBisa;

            Vector3 right = Vector3.Cross(up, lightDirection).normalized;

            view.SetColumn(0, new Vector4(right.x, up.x, lightDirection.x, 0));
            view.SetColumn(1, new Vector4(right.y, up.y, lightDirection.y, 0));
            view.SetColumn(2, new Vector4(right.z, up.z, lightDirection.z, 0));
            view.SetColumn(3,
                new Vector4(-Vector3.Dot(right, lightPosition), -Vector3.Dot(up, lightPosition),
                    -Vector3.Dot(lightDirection, lightPosition), 1));

            return view;
        }
    
    
    public void UpdateAABB(SkinnedMeshRenderer obj, CharacterShadowData data)
        {
            Bounds bounds = obj.bounds;
            //float boundScale = skinmeshrender.Value.LocalBoundsScale;
            float x = bounds.extents.x * data.LocalBoundsScale * data.LocalBoundsScaleX;
            float y = bounds.extents.y * data.LocalBoundsScale * data.LocalBoundsScaleY;
            float z = bounds.extents.z * data.LocalBoundsScale * data.LocalBoundsScaleZ;
            // if (TestGC2)
            // {
            // }
            // else
            // {
            NativeArray<Vector3> boundsPointsList = new NativeArray<Vector3>(8, Allocator.Temp);
            boundsPointsList[0] = new Vector3(x, y, z);
            boundsPointsList[1] = new Vector3(x, -y, z);
            boundsPointsList[2] = new Vector3(x, y, -z);
            boundsPointsList[3] = new Vector3(x, -y, -z);
            boundsPointsList[4] = new Vector3(-x, y, z);
            boundsPointsList[5] = new Vector3(-x, -y, z);
            boundsPointsList[6] = new Vector3(-x, y, -z);
            boundsPointsList[7] = new Vector3(-x, -y, -z);


            for (int i = 0; i < boundsPointsList.Length; i++)
            {
                //  vertexRenderer.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere).GetComponent<SkinnedMeshRenderer>());
#if UNITY_EDITOR
                if (OpenEditorBallBounds)
                {
                    //Debug.Log(vertexRenderer.Count);
                    vertexRenderer[obj][i].transform.position = boundsPointsList[i] + bounds.center;
                    vertexRenderer[obj][i].sharedMaterial.SetColor("_BaseColor", Color.cyan);
                    vertexRenderer[obj][i].transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    vertexRenderer[obj][i].shadowCastingMode = ShadowCastingMode.Off;
                }
#endif
                boundsPointsList[i] = boundsPointsList[i] + bounds.center;
            }

            Boundpoints_List.Add(boundsPointsList);
            //}
        }

        public void fitToScene(NativeArray<Vector3> boundsPoints, int index)
        {
            if (index > 10)
            {
                return;
            }

            if (boundsPoints.Length < 1)
            {
                ViewMatrixs[index] = Matrix4x4.identity;
                ProjMatrixs[index] = Matrix4x4.identity;
                return;
            }

            float xmin = float.MaxValue, xmax = float.MinValue;
            float ymin = float.MaxValue, ymax = float.MinValue;
            float zmin = float.MaxValue, zmax = float.MinValue;


            foreach (var vertex in boundsPoints)
            {
                Vector3 vertexLS = mainLight.transform.worldToLocalMatrix.MultiplyPoint(vertex);
                xmin = Mathf.Min(xmin, vertexLS.x);
                xmax = Mathf.Max(xmax, vertexLS.x);
                ymin = Mathf.Min(ymin, vertexLS.y);
                ymax = Mathf.Max(ymax, vertexLS.y);
                zmin = Mathf.Min(zmin, vertexLS.z);
                zmax = Mathf.Max(zmax, vertexLS.z);
            }

            viewMatrix = mainLight.transform.worldToLocalMatrix;


            float clipDistance = 1;
            // if (SystemInfo.usesReversedZBuffer)
            // {
            //     viewMatrix.m20 = viewMatrix.m20;
            //     viewMatrix.m21 = viewMatrix.m21;
            //     viewMatrix.m22 = viewMatrix.m22;
            //     viewMatrix.m23 = viewMatrix.m23;
            //     clipDistance = shadowClipDistance;
            // }
            // else
            // {
            viewMatrix.m20 = -viewMatrix.m20;
            viewMatrix.m21 = -viewMatrix.m21;
            viewMatrix.m22 = -viewMatrix.m22;
            viewMatrix.m23 = -viewMatrix.m23;
            clipDistance = -shadowClipDistance;
            //}

            zmax += clipDistance;
            Vector4 row0 = new Vector4(2 / (xmax - xmin), 0, 0, -(xmax + xmin) / (xmax - xmin));
            Vector4 row1 = new Vector4(0, 2 / (ymax - ymin), 0, -(ymax + ymin) / (ymax - ymin));
            Vector4 row2 = new Vector4(0, 0, -2 / (zmax - zmin), -(zmax + zmin) / (zmax - zmin));
            Vector4 row3 = new Vector4(0, 0, 0, 1);
            //float orthoSize = Mathf.Max(xmax - xmin, ymax - ymin);
            //Matrix4x4 projMatrix = Matrix4x4.Ortho(xmin, xmax, ymin, ymax, zmin, clipDistance);
            Matrix4x4 projMatrix = Matrix4x4.identity;
            projMatrix.SetRow(0, row0);
            projMatrix.SetRow(1, row1);
            projMatrix.SetRow(2, row2);
            projMatrix.SetRow(3, row3);

            //UniversalRenderPipeline.projMatrix = projMatrix;
            //Matrix4x4 VP = projMatrix * viewMatrix;

            ViewMatrixs[index] = viewMatrix;

            ProjMatrixs[index] = projMatrix;
        }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {

        if (m_HighQualityShadow==null)
        {
            return;
        }
        
        var renderType = renderingData.cameraData.renderType;
        if (_CustomSettings == null || _CustomSettings.CustomSettings == null)
        {
            return;
        }

        if (renderType == CameraRenderType.Overlay)
        {
            return;
        }
//         
        for (int i = 0; i < m_HighQualityShadow.ProjMatrixs.Length; i++)
        {


            
            int offSetX=(i % 2) * RenderSize;
            int offsetY=(i / 2) * RenderSize;
            
             // Debug.Log(i +"----X----:"+offSetX);
             // Debug.Log(i +"----Y----:"+offsetY);
            var viewMatrix = m_HighQualityShadow.ViewMatrixs[i];
            var projMatrix = m_HighQualityShadow.ProjMatrixs[i];
            
            m_ShadowSliceDatas[i].offsetX = offSetX ;
            m_ShadowSliceDatas[i].offsetY = offsetY ;
            m_ShadowSliceDatas[i].viewMatrix = viewMatrix;
            m_ShadowSliceDatas[i].projectionMatrix = projMatrix;

                m_ShadowSliceDatas[i].shadowTransform=GetShadowTransform(projMatrix,viewMatrix);
                m_ShadowSliceDatas[i].resolutionX = offSetX ;
                m_ShadowSliceDatas[i].resolutionY = offsetY  ;
                ApplySliceTransform(ref m_ShadowSliceDatas[i], Width, Height,i);



            m_CharacterMartixArrary[i] = m_ShadowSliceDatas[i].shadowTransform;
             if (viewMatrix ==Matrix4x4.identity && i!=0 || viewMatrix==Matrix4x4.zero)
             {
                 m_CharacterMartixArrary[i]= m_CharacterMartixArrary[0];
                 m_CharacterUVClamp[i]=m_CharacterUVClamp[0];
                

            }


        }

        cmd.SetGlobalVectorArray(_CharacterUVClamp,m_CharacterUVClamp);
        cmd.SetGlobalMatrixArray(_CharacterShadowMatrix, m_CharacterMartixArrary);

    }

    public  void ApplySliceTransform(ref CharacterShadowData shadowSliceData, int atlasWidth, int atlasHeight,int index)
    {
        Matrix4x4 sliceTransform = Matrix4x4.identity;
        float oneOverAtlasWidth = 1.0f / atlasWidth;   // 1 / 1024
        float oneOverAtlasHeight = 1.0f / atlasHeight; //  1/ 2560
        sliceTransform.m00 = RenderSize * oneOverAtlasWidth;//每一块tile的宽度
        sliceTransform.m11 = RenderSize * oneOverAtlasHeight;//每一块tile 的高度
        sliceTransform.m03 = shadowSliceData.resolutionX * oneOverAtlasWidth ;
        sliceTransform.m13 = shadowSliceData.resolutionY * oneOverAtlasHeight ;
        // Apply shadow slice scale and offset
        shadowSliceData.shadowTransform = sliceTransform * shadowSliceData.shadowTransform;
         m_CharacterUVClamp[index] = new Vector4(sliceTransform.m03,sliceTransform.m03 + 0.5F,sliceTransform.m13 ,sliceTransform.m13 + 0.2F);

    }
    public  Matrix4x4 GetShadowTransform(Matrix4x4 proj, Matrix4x4 view)
    {
        // Currently CullResults ComputeDirectionalShadowMatricesAndCullingPrimitives doesn't
        // apply z reversal to projection matrix. We need to do it manually here.
        if (SystemInfo.usesReversedZBuffer)
        {
            proj.m20 = -proj.m20;
            proj.m21 = -proj.m21;
            proj.m22 = -proj.m22;
            proj.m23 = -proj.m23;
        }

        Matrix4x4 worldToShadow = proj * view;

        var textureScaleAndBias = Matrix4x4.identity;
        textureScaleAndBias.m00 = 0.5f;
        textureScaleAndBias.m11 = 0.5f;
        textureScaleAndBias.m22 = 0.5f;
        textureScaleAndBias.m03 = 0.5f;
        textureScaleAndBias.m23 = 0.5f;
        textureScaleAndBias.m13 = 0.5f;

        // Apply texture scale and offset to save a MAD in shader.
        return textureScaleAndBias * worldToShadow;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        // if (renderingData.cameraData.cameraType==CameraType.Preview ||
        //     renderingData.cameraData.cameraType==CameraType.Reflection||
        //     renderingData.cameraData.cameraType==CameraType.SceneView
        //     )
        // {
        //   return;  
        // }

        if (m_HighQualityShadow==null)
            return;
        
        if (_CustomSettings == null || _CustomSettings.CustomSettings == null)
            //|| renderingData.cameraData.cameraType != CameraType.Game)
        {
            return;
        }

        var cullResults = renderingData.cullResults;
        //var cameraData = renderingData.cameraData;
        //var camera = cameraData.camera;
        
        //var srcViewMatrix = cameraData.camera.worldToCameraMatrix;
        // var tempProjMatrix = cameraData.camera.projectionMatrix;
        // tempProjMatrix.m00 *= 0.5F;
        // tempProjMatrix.m11 *= 0.5F;
        // if (camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters))
        // {
        //     GeometryUtility.CalculateFrustumPlanes(tempProjMatrix * srcViewMatrix, _cullingPlanes);
        //     for (int p = 0; p < cullingParameters.cullingPlaneCount; p++)
        //     {
        //         cullingParameters.SetCullingPlane(p, _cullingPlanes[p]);
        //     }
        //
        //     cullResults = context.Cull(ref cullingParameters);
        // }




        CommandBuffer cmd = CommandBufferPool.Get();

         for (int i = 0; i < m_ShadowSliceDatas.Length; i++)
         {
            var orgViewMatrix = m_ShadowSliceDatas[i].viewMatrix;
            var orgProjMatrix = m_ShadowSliceDatas[i].projectionMatrix;
            

            
            cmd.SetViewMatrix(orgViewMatrix);
            cmd.SetViewProjectionMatrices(orgViewMatrix, orgProjMatrix);
            cmd.SetViewport(new Rect(m_ShadowSliceDatas[i].offsetX+2, m_ShadowSliceDatas[i].offsetY+2, 
                RenderSize-4, RenderSize-4));
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            for (int j = 0; j < _CustomSettings.CustomSettings.Length; j++)
            {
                var setting = _CustomSettings.CustomSettings[j];
                DrawingSettings drawingSettings = CreateDrawingSettings(
                    new ShaderTagId(setting.ShaderTag), ref renderingData,
                    setting.IsOpaque ? SortingCriteria.CommonOpaque : SortingCriteria.CommonTransparent);
                FilteringSettings filteringSettings = new FilteringSettings(
                    new RenderQueueRange(setting.QueueMin, setting.QueueMax),
                    _CustomSettings.CustomSettings[j].Layer,(uint)2<<i);
                context.DrawRenderers(cullResults, ref drawingSettings, ref filteringSettings);
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

         }
        CommandBufferPool.Release(cmd);
        CommandBuffer cmd1 = CommandBufferPool.Get();
        //cmd1.SetViewProjectionMatrices(srcViewMatrix, srcProjMatrix);
        cmd1.SetGlobalTexture(_CharacterShadowTexture, m_CharaShadowMap);
        context.ExecuteCommandBuffer(cmd1);
        CommandBufferPool.Release(cmd1);
    }
}
*/
