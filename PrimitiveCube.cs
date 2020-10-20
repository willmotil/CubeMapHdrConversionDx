using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    public class PrimitiveCube
    {
        public static Matrix matrixNegativeX = Matrix.CreateWorld(Vector3.Zero, new Vector3(-1.0f, 0, 0), Vector3.Up);
        public static Matrix matrixNegativeZ = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, 0, -1.0f), Vector3.Up);
        public static Matrix matrixPositiveX = Matrix.CreateWorld(Vector3.Zero, new Vector3(1.0f, 0, 0), Vector3.Up);
        public static Matrix matrixPositiveZ = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, 0, 1.0f), Vector3.Up);
        public static Matrix matrixPositiveY = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, 1.0f, 0), Vector3.Backward);
        public static Matrix matrixNegativeY = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, -1.0f, 0), Vector3.Forward);

        public VertexPositionNormalTexture[] cubesFaces;

        public PrimitiveCube()
        {
            CreatePrimitiveCube(1, false, true, true);
        }
        public PrimitiveCube(float scale, bool clockwise, bool invert, bool directionalFaces)
        {
            CreatePrimitiveCube(scale, clockwise, invert, directionalFaces);
        }

        public void CreatePrimitiveCube(float scale, bool clockwise, bool invert, bool directionalFaces)
        {
            var r = new Rectangle(-1, -1, 2, 2);
            cubesFaces = new VertexPositionNormalTexture[36];

            float depth = -scale;
            if (invert)
                depth = -depth;

            var p0 = new Vector3(r.Left * scale, r.Top * scale, depth);
            var p1 = new Vector3(r.Left * scale, r.Bottom * scale, depth);
            var p2 = new Vector3(r.Right * scale, r.Bottom * scale, depth);
            var p3 = new Vector3(r.Right * scale, r.Bottom * scale, depth);
            var p4 = new Vector3(r.Right * scale, r.Top * scale, depth);
            var p5 = new Vector3(r.Left * scale, r.Top * scale, depth);

            int i = 0;
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                if (clockwise == false)
                {
                    //t1
                    cubesFaces[i + 0] = GetVertice(p0, faceIndex, directionalFaces, depth, new Vector2(0f, 0f)); // p0
                    cubesFaces[i + 1] = GetVertice(p1, faceIndex, directionalFaces, depth, new Vector2(0f, 1f)); // p1
                    cubesFaces[i + 2] = GetVertice(p2, faceIndex, directionalFaces, depth, new Vector2(1f, 1f)); // p2
                    //t2                                                                                                                                                    
                    cubesFaces[i + 3] = GetVertice(p3, faceIndex, directionalFaces, depth, new Vector2(1f, 1f)); // p3
                    cubesFaces[i + 4] = GetVertice(p4, faceIndex, directionalFaces, depth, new Vector2(1f, 0f)); // p4
                    cubesFaces[i + 5] = GetVertice(p5, faceIndex, directionalFaces, depth, new Vector2(0f, 0f)); // p5
                }
                else
                {
                    //t1
                    cubesFaces[i + 0] = GetVertice(p0, faceIndex, directionalFaces, depth, new Vector2(0f, 0f)); // 0-p0
                    cubesFaces[i + 2] = GetVertice(p1, faceIndex, directionalFaces, depth, new Vector2(0f, 1f)); // 2-p1
                    cubesFaces[i + 1] = GetVertice(p2, faceIndex, directionalFaces, depth, new Vector2(1f, 1f)); // 1-p2
                    //t2                                                                                                                                                      
                    cubesFaces[i + 4] = GetVertice(p3, faceIndex, directionalFaces, depth, new Vector2(1f, 1f)); // 4-p3
                    cubesFaces[i + 3] = GetVertice(p4, faceIndex, directionalFaces, depth, new Vector2(1f, 0f)); // 3-p4
                    cubesFaces[i + 5] = GetVertice(p5, faceIndex, directionalFaces, depth, new Vector2(0f, 0f)); // 5-p5
                }
                i += 6;
            }
        }

        private VertexPositionNormalTexture GetVertice(Vector3 v, int faceIndex, bool directionalFaces, float depth, Vector2 uv)
        {
            return new VertexPositionNormalTexture(Vector3.Transform(v, GetWorldFaceMatrix(faceIndex)), FlatFaceOrDirectional(v, faceIndex, directionalFaces, depth), uv);
        }

        private Vector3 FlatFaceOrDirectional(Vector3 v, int faceIndex, bool directionalFaces, float depth)
        {
            if (directionalFaces == false)
                v = new Vector3(0, 0, depth);
            v = Vector3.Normalize(v);
            return Vector3.Transform(v, GetWorldFaceMatrix(faceIndex));
        }

        public void DrawPrimitiveCubeFace(GraphicsDevice gd, Effect effect, TextureCube cubeTexture, int cubeFaceToRender)
        {
            effect.Parameters["CubeMap"].SetValue(cubeTexture);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, cubesFaces, cubeFaceToRender * 6, 2, VertexPositionNormalTexture.VertexDeclaration);
            }
        }
        public void DrawPrimitiveCube(GraphicsDevice gd, Effect effect, TextureCube cubeTexture)
        {
            effect.Parameters["CubeMap"].SetValue(cubeTexture);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, cubesFaces, 0, 12, VertexPositionNormalTexture.VertexDeclaration);
            }
        }

        public static Matrix GetWorldFaceMatrix(int i)
        {
            switch (i)
            {
                case (int)CubeMapFace.NegativeX: // FACE_LEFT
                    return matrixNegativeX;
                case (int)CubeMapFace.NegativeZ: // FACE_FORWARD
                    return matrixNegativeZ;
                case (int)CubeMapFace.PositiveX: // FACE_RIGHT
                    return matrixPositiveX;
                case (int)CubeMapFace.PositiveZ: // FACE_BACK
                    return matrixPositiveZ;
                case (int)CubeMapFace.PositiveY: // FACE_TOP
                    return matrixPositiveY;
                case (int)CubeMapFace.NegativeY: // FACE_BOTTOM
                    return matrixNegativeY;
                default:
                    return matrixNegativeZ;
            }
        }
    }
}
