using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    public static class MatrixHelpers
    {
        // https://docs.microsoft.com/en-us/windows/win32/direct3d9/d3dxmatrixperspectiverh

        public static Matrix ViewMatrixForPerspectiveSpriteBatch(float width, float height, float _fov, Vector3 forward, Vector3 up)
        {
            var pos = new Vector3(width / 2, height / 2, -((1f / (float)Math.Tan(_fov / 2)) * (height / 2)));
            return Matrix.Invert(Matrix.CreateWorld(pos, forward + pos, up));
        }
        public static Matrix CameraMatrixForPerspectiveSpriteBatch(float width, float height, float _fov, Vector3 forward, Vector3 up)
        {
            var pos = new Vector3(width / 2, height / 2, -((1f / (float)Math.Tan(_fov / 2)) * (height / 2)));
            return Matrix.CreateWorld(pos, forward + pos, up);
        }
        public static Vector3 CameraPositionVectorForPerspectiveSpriteBatch(float width, float height, float _fov)
        {
            var pos = new Vector3(width / 2, height / 2, -((1f / (float)Math.Tan(_fov / 2)) * (height / 2)));
            return pos;
        }

        /// <summary>
        /// Retuns a spritebatch aligned perspective projection matrix. The default is right handed for a Dx monogame project.
        /// This matrix as is cannot properly rotate however the depth will however act properly this for 2d stuff can give a good effect.
        /// </summary>
        public static Matrix CreateSpriteBatchAlignedLockedPerspectiveOffsetRhLh(float width, float height, float nearPlaneDistance, float farPlaneDistance, bool isRightHanded)
        {
            return Matrix.CreateTranslation(-width / 2, height / 2, 0) * DXCreatePerspectiveRHLH(width, height, nearPlaneDistance, farPlaneDistance, isRightHanded);
        }

        public static Matrix CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
        {
            return DXCreatePerspectiveFieldOfViewRHLH(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance, true);
        }

        public static Matrix DXCreatePerspectiveFieldOfViewRHLH(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance, bool isRightHanded)
        {
            /* RH
             m11= xscale           m12= 0                   m13= 0                m14=  0
             m21= 0                  m22= yscale            m23= 0                m24= 0
             m31= 0                  0                            m33= f/(f-n)          m34= -1
             m41= 0                  m42= 0                   m43= n*f/(n-f)      m44= 0
             where:
             yScale = cot(fovY/2)
             xScale = yScale / aspect ratio
             
             */
            if ((fieldOfView <= 0f) || (fieldOfView >= 3.141593f))
            {
                throw new ArgumentException("fieldOfView <= 0 or >= PI");
            }
            Matrix result = new Matrix();
            float yscale = 1f / ((float)Math.Tan((double)(fieldOfView * 0.5f)));
            float xscale = yscale / aspectRatio;
            result.M11 = xscale;
            result.M12 = result.M13 = result.M14 = 0;
            result.M22 = yscale;
            result.M21 = result.M23 = result.M24 = 0;
            result.M31 = result.M32 = 0f;
            if (isRightHanded)
            {
                result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
                result.M34 = -1;
                result.M43 = (nearPlaneDistance * farPlaneDistance) / (nearPlaneDistance - farPlaneDistance);
            }
            else
            {
                result.M33 = farPlaneDistance / (farPlaneDistance - nearPlaneDistance);
                result.M34 = 1;
                result.M43 = (-nearPlaneDistance * farPlaneDistance) / (farPlaneDistance - nearPlaneDistance);
            }
            result.M41 = result.M42 = result.M44 = 0;

            return result;
        }

        /// <summary>
        /// D3DXMatrixPerspectiveOffCenterLH 
        /// </summary>
        public static Matrix CreatePerspective(Rectangle viewingVolume, float nearPlaneDistance, float farPlaneDistance)
        {
            float w = viewingVolume.Right - viewingVolume.Left;
            float h = viewingVolume.Bottom - viewingVolume.Top;
            if (w < 0)
                w = -w;
            if (h < 0)
                h = -h;
            return DXCreatePerspectiveRHLH(w, h, nearPlaneDistance, farPlaneDistance, true);
        }

        public static Matrix DXCreatePerspectiveRHLH(float width, float height, float nearPlaneDistance, float farPlaneDistance, bool isRightHanded)
        {
            /* RH
             m11= 2*n/w           m12= 0                   m13= 0                m14=  0
             m21= 0                  m22= 2*n/(t-b)        m23= 0                m24= 0
             m31= 0                  0                            m33= f/(f-n)          m34= 1
             m41= 0                  m42= 0                   m43= n*f/(n-f)      m44= 0

             m31 m32 m33 and m34 determines handedness
             */

            /* Im not sold that all of these actually are invalid
                if (nearPlaneDistance <= 0f)
                {
                    throw new ArgumentException("nearPlaneDistance <= 0");
                }
                if (farPlaneDistance <= 0f)
                {
                    throw new ArgumentException("farPlaneDistance <= 0");
                }
                if (nearPlaneDistance >= farPlaneDistance)
                {
                    throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
                }
             */

            Matrix result = new Matrix();

            result.M11 = (2f * nearPlaneDistance) / width;
            result.M12 = result.M13 = result.M14 = 0;

            result.M22 = (2f * nearPlaneDistance) / height;
            result.M21 = result.M23 = result.M24 = 0;

            if (isRightHanded)
            {
                result.M31 = result.M32 = 0;
                result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
                result.M34 = -1;
            }
            else
            {
                result.M31 = result.M32 = 0;
                result.M33 = farPlaneDistance / (farPlaneDistance - nearPlaneDistance);
                result.M34 = 1;
            }

            result.M43 = (nearPlaneDistance * farPlaneDistance) / (nearPlaneDistance - farPlaneDistance);
            result.M41 = result.M42 = result.M44 = 0;
            return result;
        }

        /// <summary>
        /// D3DXMatrixPerspectiveOffCenterLH 
        /// </summary>
        public static Matrix CreatePerspectiveOffCenter(Rectangle viewingVolume, float nearPlaneDistance, float farPlaneDistance)
        {
            return DXPerspectiveOffCenterRHLH(viewingVolume.Left, viewingVolume.Right, viewingVolume.Bottom, viewingVolume.Top, nearPlaneDistance, farPlaneDistance, true);
        }

        public static Matrix DXPerspectiveOffCenterRHLH(float left, float right, float bottom, float top, float nearPlaneDistance, float farPlaneDistance, bool isRightHanded)
        {
            /* RH
             m11= 2*n/(r-l)       m12= 0                  m13= 0               m14=  0
             m21= 0                 m22= 2*n/(t-b)       m23= 0               m24= 0
             m31= (l+r)/(r-l)     m32= (t+b)/(t-b)    m33= f/(n-f)         m34= -1
             m41= 0                 m42= 0                 m43= n*f/(n-f)     m44= 0

             m31 m32 m33 and m34 determines handedness
             */

            /* Im not sold that all of these actually are invalid
                if (nearPlaneDistance <= 0f)
                {
                    throw new ArgumentException("nearPlaneDistance <= 0");
                }
                if (farPlaneDistance <= 0f)
                {
                    throw new ArgumentException("farPlaneDistance <= 0");
                }
                if (nearPlaneDistance >= farPlaneDistance)
                {
                    throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
                }
             */

            Matrix result = new Matrix();

            result.M11 = (2f * nearPlaneDistance) / (right - left);
            result.M12 = result.M13 = result.M14 = 0;

            result.M22 = (2f * nearPlaneDistance) / (top - bottom);
            result.M21 = result.M23 = result.M24 = 0;

            if (isRightHanded)
            {
                result.M31 = (left + right) / (right - left);
                result.M32 = (top + bottom) / (top - bottom);
                result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
                result.M34 = -1;
            }
            else
            {
                result.M31 = (left + right) / (left - right);
                result.M32 = (top + bottom) / (bottom - top);
                result.M33 = farPlaneDistance / (farPlaneDistance - nearPlaneDistance);
                result.M34 = 1;
            }
            result.M43 = (nearPlaneDistance * farPlaneDistance) / (nearPlaneDistance - farPlaneDistance);
            result.M41 = result.M42 = result.M44 = 0;
            return result;
        }

        //// Generates a viewport transform matrix for rendering sprites using x-right y-down screen pixel coordinates.
        //XMMATRIX SpriteBatch::Impl::GetViewportTransform(_In_ ID3D11DeviceContext* deviceContext, DXGI_MODE_ROTATION rotation)
        //{
        //    // Look up the current viewport.
        //    if (!mSetViewport)
        //    {
        //        UINT viewportCount = 1;

        //        deviceContext->RSGetViewports(&viewportCount, &mViewPort);

        //        if (viewportCount != 1)
        //            throw std::exception("No viewport is set");
        //    }

        //    // Compute the matrix.
        //    float xScale = (mViewPort.Width > 0) ? 2.0f / mViewPort.Width : 0.0f;
        //    float yScale = (mViewPort.Height > 0) ? 2.0f / mViewPort.Height : 0.0f;

        //    switch (rotation)
        //    {
        //        case DXGI_MODE_ROTATION_ROTATE90:
        //            return XMMATRIX
        //            (
        //                 0, -yScale, 0, 0,
        //                 -xScale, 0, 0, 0,
        //                 0, 0, 1, 0,
        //                 1, 1, 0, 1
        //            );

        //        case DXGI_MODE_ROTATION_ROTATE270:
        //            return XMMATRIX
        //            (
        //                 0, yScale, 0, 0,
        //                 xScale, 0, 0, 0,
        //                 0, 0, 1, 0,
        //                -1, -1, 0, 1
        //            );

        //        case DXGI_MODE_ROTATION_ROTATE180:
        //            return XMMATRIX
        //            (
        //                -xScale, 0, 0, 0,
        //                 0, yScale, 0, 0,
        //                 0, 0, 1, 0,
        //                 1, -1, 0, 1
        //            );

        //        default:
        //            return XMMATRIX
        //            (
        //                 xScale, 0, 0, 0,
        //                 0, -yScale, 0, 0,
        //                 0, 0, 1, 0,
        //                -1, 1, 0, 1
        //            );
        //    }
        //}
    }
}
