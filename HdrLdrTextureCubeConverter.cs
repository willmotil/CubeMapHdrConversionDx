using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    public static class HdrLdrTextureCubeConverter
    {
        private static PrimitiveScreenQuad screenQuad = new PrimitiveScreenQuad(false);

        /// <summary>
        /// Renders the hdr texture to a TextureCube.
        /// The ref is used to pass the ref variable directly thru here, its not a ref copy i guess.
        /// </summary>
        public static void RenderSpherical2DToTextureCube(GraphicsDevice gd, Effect _hdrEffect, string Technique, Texture2D sourceHdrLdrEquaRectangularMap, ref TextureCube textureCubeDestinationMap, bool generateMips, bool useHdrFormat, int sizeSquarePerFace)
        {
            gd.RasterizerState = RasterizerState.CullNone;
            var pixelformat = SurfaceFormat.Color;
            if (useHdrFormat)
                pixelformat = SurfaceFormat.Vector4;
            var renderTargetCube = new RenderTargetCube(gd, sizeSquarePerFace, generateMips, pixelformat, DepthFormat.None);
            _hdrEffect.CurrentTechnique = _hdrEffect.Techniques[Technique];
            _hdrEffect.Parameters["Texture"].SetValue(sourceHdrLdrEquaRectangularMap);
            for (int i = 0; i < 6; i++)
            {
                switch (i)
                {
                    case (int)CubeMapFace.NegativeX: // FACE_LEFT
                        gd.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeX);
                        break;
                    case (int)CubeMapFace.NegativeZ: // FACE_FORWARD
                        gd.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeZ);
                        break;
                    case (int)CubeMapFace.PositiveX: // FACE_RIGHT
                        gd.SetRenderTarget(renderTargetCube, CubeMapFace.PositiveX);
                        break;
                    case (int)CubeMapFace.PositiveZ: // FACE_BACK
                        gd.SetRenderTarget(renderTargetCube, CubeMapFace.PositiveZ);
                        break;
                    case (int)CubeMapFace.PositiveY: // FACE_TOP
                        gd.SetRenderTarget(renderTargetCube, CubeMapFace.PositiveY);
                        break;
                    case (int)CubeMapFace.NegativeY: // FACE_BOTTOM
                        gd.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeY);
                        break;
                }
                _hdrEffect.Parameters["FaceToMap"].SetValue(i); // render screenquad to face.
                foreach (EffectPass pass in _hdrEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserPrimitives(PrimitiveType.TriangleList, screenQuad.vertices, 0, 2);
                }
            }
            textureCubeDestinationMap = renderTargetCube; // set the render to the specified texture cube.
            gd.SetRenderTarget(null);
        }

        /// <summary>
        /// Renders the hdr TextureCube to a array of 6 texture2D faces using the designated effect and technique.
        /// </summary>
        public static void RenderTextureCubeToSpherical(GraphicsDevice gd, Effect _hdrEffect, string Technique, TextureCube sourceHdrLdrEnvMap, ref Texture2D textureSpherical, bool generateMips, bool useHdrFormat, int sizeSquarePerFace)
        {
            gd.RasterizerState = RasterizerState.CullNone;
            var pixelformat = SurfaceFormat.Color;
            if (useHdrFormat)
                pixelformat = SurfaceFormat.Vector4;
            _hdrEffect.CurrentTechnique = _hdrEffect.Techniques[Technique];
            _hdrEffect.Parameters["CubeMap"].SetValue(sourceHdrLdrEnvMap);
            for (int i = 0; i < 6; i++)
            {
                var renderTarget2D = new RenderTarget2D(gd, sizeSquarePerFace, sizeSquarePerFace /2, generateMips, pixelformat, DepthFormat.None);
                gd.SetRenderTarget(renderTarget2D);
                _hdrEffect.Parameters["FaceToMap"].SetValue(i); // render screenquad to face.
                foreach (EffectPass pass in _hdrEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserPrimitives(PrimitiveType.TriangleList, screenQuad.vertices, 0, 2);
                }
                textureSpherical = renderTarget2D; // set the render to the specified texture.
            }
            gd.SetRenderTarget(null);
        }

        /// <summary>
        /// Renders the hdr TextureCube to another TextureCube using the designated effect and technique.
        /// </summary>
        public static void RenderTextureCubeToTextureCube(GraphicsDevice gd, Effect _hdrEffect, string Technique, TextureCube sourceHdrLdrEnvMap, ref TextureCube textureCubeDestinationMap, bool generateMips, bool useHdrFormat, int sizeSquarePerFace)
        {
            gd.RasterizerState = RasterizerState.CullNone;
            var pixelformat = SurfaceFormat.Color;
            if (useHdrFormat)
                pixelformat = SurfaceFormat.Vector4;
            var renderTargetCube = new RenderTargetCube(gd, sizeSquarePerFace, generateMips, pixelformat, DepthFormat.None);
            _hdrEffect.CurrentTechnique = _hdrEffect.Techniques[Technique];
            _hdrEffect.Parameters["CubeMap"].SetValue(sourceHdrLdrEnvMap);
            for (int i = 0; i < 6; i++)
            {
                switch (i)
                {
                    case (int)CubeMapFace.NegativeX: // FACE_LEFT
                        gd.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeX);
                        break;
                    case (int)CubeMapFace.NegativeZ: // FACE_FORWARD
                        gd.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeZ);
                        break;
                    case (int)CubeMapFace.PositiveX: // FACE_RIGHT
                        gd.SetRenderTarget(renderTargetCube, CubeMapFace.PositiveX);
                        break;
                    case (int)CubeMapFace.PositiveZ: // FACE_BACK
                        gd.SetRenderTarget(renderTargetCube, CubeMapFace.PositiveZ);
                        break;
                    case (int)CubeMapFace.PositiveY: // FACE_TOP
                        gd.SetRenderTarget(renderTargetCube, CubeMapFace.PositiveY);
                        break;
                    case (int)CubeMapFace.NegativeY: // FACE_BOTTOM
                        gd.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeY);
                        break;
                }
                _hdrEffect.Parameters["FaceToMap"].SetValue(i); // render screenquad to face.
                foreach (EffectPass pass in _hdrEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserPrimitives(PrimitiveType.TriangleList, screenQuad.vertices, 0, 2);
                }
            }
            textureCubeDestinationMap = renderTargetCube; // set the render to the specified texture cube.
            gd.SetRenderTarget(null);
        }

        /// <summary>
        /// Renders the hdr TextureCube to a array of 6 texture2D faces using the designated effect and technique.
        /// </summary>
        public static void RenderTextureCubeToTexture2DArray(GraphicsDevice gd, Effect _hdrEffect, string Technique, TextureCube sourceHdrLdrEnvMap, ref Texture2D[] textureFaceArray, bool generateMips, bool useHdrFormat, int sizeSquarePerFace)
        {
            gd.RasterizerState = RasterizerState.CullNone;
            var pixelformat = SurfaceFormat.Color;
            if (useHdrFormat)
                pixelformat = SurfaceFormat.Vector4;
            textureFaceArray = new Texture2D[6];
            _hdrEffect.CurrentTechnique = _hdrEffect.Techniques[Technique];
            _hdrEffect.Parameters["CubeMap"].SetValue(sourceHdrLdrEnvMap);
            for (int i = 0; i < 6; i++)
            {
                var renderTarget2D = new RenderTarget2D(gd, sizeSquarePerFace, sizeSquarePerFace, generateMips, pixelformat, DepthFormat.None);
                 gd.SetRenderTarget(renderTarget2D); 
                _hdrEffect.Parameters["FaceToMap"].SetValue(i); // render screenquad to face.
                foreach (EffectPass pass in _hdrEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserPrimitives(PrimitiveType.TriangleList, screenQuad.vertices, 0, 2);
                }
                textureFaceArray[i] = renderTarget2D; // set the render to the specified texture cube.
            }
            gd.SetRenderTarget(null);
        }

        /// <summary>
        /// Renders the  array of 6 texture2D faces to a TextureCube using the designated effect and technique.
        /// </summary>
        public static void RenderTexture2DArrayToTextureCube(GraphicsDevice gd, Effect _hdrEffect, string Technique, Texture2D[] sourceTextureFaceArray, ref TextureCube textureCubeDestinationMap, bool generateMips, bool useHdrFormat, int sizeSquarePerFace)
        {
            gd.RasterizerState = RasterizerState.CullNone;
            var pixelformat = SurfaceFormat.Color;
            if (useHdrFormat)
                pixelformat = SurfaceFormat.Vector4;
            var renderTargetCube = new RenderTargetCube(gd, sizeSquarePerFace, generateMips, pixelformat, DepthFormat.None);
            _hdrEffect.CurrentTechnique = _hdrEffect.Techniques[Technique];
            for (int i = 0; i < 6; i++)
            {
                _hdrEffect.Parameters["Texture"].SetValue(sourceTextureFaceArray[i]);
                switch (i)
                {
                    case (int)CubeMapFace.NegativeX: // FACE_LEFT
                        gd.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeX);
                        break;
                    case (int)CubeMapFace.NegativeZ: // FACE_FORWARD
                        gd.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeZ);
                        break;
                    case (int)CubeMapFace.PositiveX: // FACE_RIGHT
                        gd.SetRenderTarget(renderTargetCube, CubeMapFace.PositiveX);
                        break;
                    case (int)CubeMapFace.PositiveZ: // FACE_BACK
                        gd.SetRenderTarget(renderTargetCube, CubeMapFace.PositiveZ);
                        break;
                    case (int)CubeMapFace.PositiveY: // FACE_TOP
                        gd.SetRenderTarget(renderTargetCube, CubeMapFace.PositiveY);
                        break;
                    case (int)CubeMapFace.NegativeY: // FACE_BOTTOM
                        gd.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeY);
                        break;
                }
                _hdrEffect.Parameters["FaceToMap"].SetValue(i); // render screenquad to face.
                foreach (EffectPass pass in _hdrEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserPrimitives(PrimitiveType.TriangleList, screenQuad.vertices, 0, 2);
                }
            }
            textureCubeDestinationMap = renderTargetCube; // set the render to the specified texture cube.
            gd.SetRenderTarget(null);
        }

        public class PrimitiveScreenQuad
        {
            public VertexPositionTexture[] vertices;

            public PrimitiveScreenQuad(bool clockwise)
            {
                var r = new Rectangle(-1, -1, 2, 2);
                vertices = new VertexPositionTexture[6];
                //
                if (clockwise)
                {
                    vertices[0] = new VertexPositionTexture(new Vector3(r.Left, r.Top, 0f), new Vector2(0f, 0f));  // p1
                    vertices[1] = new VertexPositionTexture(new Vector3(r.Left, r.Bottom, 0f), new Vector2(0f, 1f)); // p0
                    vertices[2] = new VertexPositionTexture(new Vector3(r.Right, r.Bottom, 0f), new Vector2(1f, 1f));// p3

                    vertices[3] = new VertexPositionTexture(new Vector3(r.Right, r.Bottom, 0f), new Vector2(1f, 1f));// p3
                    vertices[4] = new VertexPositionTexture(new Vector3(r.Right, r.Top, 0f), new Vector2(1f, 0f));// p2
                    vertices[5] = new VertexPositionTexture(new Vector3(r.Left, r.Top, 0f), new Vector2(0f, 0f)); // p1
                }
                else
                {
                    vertices[0] = new VertexPositionTexture(new Vector3(r.Left, r.Top, 0f), new Vector2(0f, 0f));  // p1
                    vertices[2] = new VertexPositionTexture(new Vector3(r.Left, r.Bottom, 0f), new Vector2(0f, 1f)); // p0
                    vertices[1] = new VertexPositionTexture(new Vector3(r.Right, r.Bottom, 0f), new Vector2(1f, 1f));// p3

                    vertices[4] = new VertexPositionTexture(new Vector3(r.Right, r.Bottom, 0f), new Vector2(1f, 1f));// p3
                    vertices[3] = new VertexPositionTexture(new Vector3(r.Right, r.Top, 0f), new Vector2(1f, 0f));// p2
                    vertices[5] = new VertexPositionTexture(new Vector3(r.Left, r.Top, 0f), new Vector2(0f, 0f)); // p1
                }
            }
        }
    }
}
