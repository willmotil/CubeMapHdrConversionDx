using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    //
    // eh too tired to do this right now 
    // todo's
    // make a mesh on the list per face need to pass width height
    // extending normals tangents and uv's is all par for the course no problems there.
    // however besides the parameter methods that i need to pass.
    // i need to be able to pass image data for nasa map data i want to add that in.
    //
    // We want this to be nice and clean i put this off for like a year and didn't take the time to redo it.
    // So take a little time to ...
    // Make it clean simple extendable and reusable into the future.
    //

    public class PrimitiveSphere
    {
        public static Matrix matrixNegativeX = Matrix.CreateWorld(Vector3.Zero, new Vector3(-1.0f, 0, 0), Vector3.Up);
        public static Matrix matrixNegativeZ = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, 0, -1.0f), Vector3.Up);
        public static Matrix matrixPositiveX = Matrix.CreateWorld(Vector3.Zero, new Vector3(1.0f, 0, 0), Vector3.Up);
        public static Matrix matrixPositiveZ = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, 0, 1.0f), Vector3.Up);
        public static Matrix matrixPositiveY = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, 1.0f, 0), Vector3.Backward);
        public static Matrix matrixNegativeY = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, -1.0f, 0), Vector3.Forward);

        public VertexPositionNormalTexture[] cubesFaceVertices;
        public int[] cubesFacesIndices;

        public PrimitiveSphere()
        {
            CreatePrimitiveSphere(0, 0, 1f, false, true, true);
        }

        public PrimitiveSphere(int subdivisionWidth, int subdividsionHeight ,float scale, bool clockwise, bool invert, bool directionalFaces)
        {
            CreatePrimitiveSphere(subdivisionWidth, subdividsionHeight, scale, clockwise, invert, directionalFaces);
        }

        public void CreatePrimitiveSphere(int subdivisionWidth, int subdividsionHeight, float scale, bool clockwise, bool invert, bool directionalFaces)
        {
            List<VertexPositionNormalTexture> cubesFaceMeshLists = new List<VertexPositionNormalTexture>();
            List<int> cubeFaceMeshIndexLists = new List<int>();

            if (subdivisionWidth < 2)
                subdivisionWidth = 2;
            if (subdividsionHeight < 2)
                subdividsionHeight = 2;

            float depth = -scale;
            if (invert)
                depth = -depth;

            float left = -1f;
            float right = +1f;
            float top = -1f;
            float bottom = +1f;

            int v = 0;
            int i = 0;
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                for (int y = 0; y < subdividsionHeight; y++)
                {
                    float perY = y / (subdividsionHeight - 1);
                    // 
                    for (int x = 0; x < subdivisionWidth; x++)
                    {
                        float perX = x / (subdivisionWidth - 1);

                        float X = Interpolate(left, right, perX);
                        float Y = Interpolate(top, bottom, perY);
                        if( (X ==0 && Y ==0) || X == float.NaN || Y == float.NaN || X == float.NegativeInfinity || Y == float.NegativeInfinity || X == float.PositiveInfinity || Y == float.PositiveInfinity)
                        {
                            System.Console.WriteLine("nan");
                        }

                        var p0 = new Vector3(X * scale, Y * scale, depth);
                        var uv0 = new Vector2(perX, perY);
                        var v0 = GetVertice(p0, faceIndex, directionalFaces, depth, uv0);

                        cubesFaceMeshLists.Add(v0);
                        v += 1;
                    }
                }

                for (int y = 0; y < subdividsionHeight; y++)
                {
                    for (int x = 0; x < subdivisionWidth; x++)
                    {
                        cubeFaceMeshIndexLists.Add(i);
                        cubeFaceMeshIndexLists.Add(i + subdivisionWidth);
                        cubeFaceMeshIndexLists.Add(i + subdivisionWidth + 1);

                        cubeFaceMeshIndexLists.Add(i + subdivisionWidth + 1);
                        cubeFaceMeshIndexLists.Add(i + 1);
                        cubeFaceMeshIndexLists.Add(i);
                        i += 6;
                    }
                }
            }
            cubesFaceVertices = cubesFaceMeshLists.ToArray();
            cubesFacesIndices = cubeFaceMeshIndexLists.ToArray();
        }

        public void DrawPrimitiveSphere(GraphicsDevice gd, Effect effect, TextureCube cubeTexture)
        {
            effect.Parameters["CubeMap"].SetValue(cubeTexture);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                int faceOffset = 0 * 6;
                int primCount = cubesFacesIndices.Length / 3;
                //gd.DrawUserPrimitives(PrimitiveType.TriangleList, cubesFaces, 0, 12, VertexPositionNormalTexture.VertexDeclaration);
                //gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, cubesFaceVertices, 0, cubesFaceVertices.Length, cubesFacesIndices, 0, cubesFacesIndices.Length /3,  VertexPositionNormalTexture.VertexDeclaration);
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, cubesFaceVertices, 0, cubesFaceVertices.Length, cubesFacesIndices, faceOffset, primCount, VertexPositionNormalTexture.VertexDeclaration);
            }
        }

        private float Interpolate(float A, float B, float t)
        {
            return ((B - A) * t) + A;
        }

        private VertexPositionNormalTexture GetVertice(Vector3 v, int faceIndex, bool directionalFaces, float depth, Vector2 uv)
        {
            var v2 = Vector3.Transform(v, GetWorldFaceMatrix(faceIndex));
            return new VertexPositionNormalTexture(v2, FlatFaceOrDirectional(v, faceIndex, directionalFaces, depth), uv);
        }

        private Vector3 FlatFaceOrDirectional(Vector3 v, int faceIndex, bool directionalFaces, float depth)
        {
            if (directionalFaces == false)
                v = new Vector3(0, 0, depth);
            v = Vector3.Normalize(v);
            return Vector3.Transform(v, GetWorldFaceMatrix(faceIndex));
        }

        public static Matrix GetWorldFaceMatrix(int i)
        {
            switch (i)
            {
                case (int)CubeMapFace.NegativeX: // 1 FACE_LEFT
                    return matrixNegativeX;
                case (int)CubeMapFace.NegativeZ: // 5 FACE_FORWARD
                    return matrixNegativeZ;
                case (int)CubeMapFace.PositiveX: // 0 FACE_RIGHT
                    return matrixPositiveX;
                case (int)CubeMapFace.PositiveZ: // 4 FACE_BACK
                    return matrixPositiveZ;
                case (int)CubeMapFace.PositiveY: // 2 FACE_TOP
                    return matrixPositiveY;
                case (int)CubeMapFace.NegativeY: // 3 FACE_BOTTOM
                    return matrixNegativeY;
                default:
                    return matrixNegativeZ;
            }
        }

        public void DrawPrimitiveSphereFace(GraphicsDevice gd, Effect effect, TextureCube cubeTexture, int cubeFaceToRender)
        {
            effect.Parameters["CubeMap"].SetValue(cubeTexture);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, cubesFaceVertices, cubeFaceToRender * 6, 2, VertexPositionNormalTexture.VertexDeclaration);
            }
        }

    }
}
