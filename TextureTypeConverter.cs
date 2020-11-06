using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Microsoft.Xna.Framework
{
    public static class TextureTypeConverter
    {
        private static PrimitiveScreenQuad screenQuad = new PrimitiveScreenQuad(false, 1);

        public static Texture2D ConvertSphericalTexture2DToSphericalTexture2D(GraphicsDevice gd, Effect _textureCubeBuildEffect, Texture2D source, bool generateMips, bool useHdrFormat, int sizeSquarePerFace)
        {
            var pixelformat = SurfaceFormat.Color;
            if (useHdrFormat)
                pixelformat = SurfaceFormat.Vector4;
            Texture2D destinationMap = new Texture2D(gd, sizeSquarePerFace, sizeSquarePerFace / 2, generateMips, pixelformat);
            RenderSphericalTexture2DToSphericalTexture2D(gd, _textureCubeBuildEffect, "SphericalToIlluminationSpherical", source, ref destinationMap, generateMips, pixelformat, sizeSquarePerFace);
            return destinationMap;
        }

        public static TextureCube ConvertSphericalTexture2DToTextureCube(GraphicsDevice gd, Effect _textureCubeBuildEffect, Texture2D source, bool generateMips, bool useHdrFormat, int sizeSquarePerFace)
        {
            var pixelformat = SurfaceFormat.Color;
            if (useHdrFormat)
                pixelformat = SurfaceFormat.Vector4;
            TextureCube textureCubeDestinationMap = new TextureCube(gd,sizeSquarePerFace, generateMips, pixelformat);
            RenderSphericalTexture2DToTextureCube(gd, _textureCubeBuildEffect, "SphericalToCubeMap", source, ref textureCubeDestinationMap, generateMips, pixelformat, sizeSquarePerFace);
            return textureCubeDestinationMap;
        }

        public static Texture2D[] ConvertSphericalTexture2DToTexture2DArray(GraphicsDevice gd, Effect _textureCubeBuildEffect, Texture2D source, bool generateMips, bool useHdrFormat, int sizeSquarePerFace)
        {
            var pixelformat = SurfaceFormat.Color;
            if (useHdrFormat)
                pixelformat = SurfaceFormat.Vector4;
            Texture2D[] destinationMap = new Texture2D[6];
            RenderSphericalTexture2DToTexture2DArray(gd, _textureCubeBuildEffect, "SphericalToCubeMap", source, ref destinationMap, generateMips, pixelformat, sizeSquarePerFace);
            return destinationMap;
        }

        public static TextureCube ConvertTextureCubeToTextureCube(GraphicsDevice gd, Effect _textureCubeBuildEffect, TextureCube source, bool generateMips, bool useHdrFormat, int sizeSquarePerFace)
        {
            var pixelformat = SurfaceFormat.Color;
            if (useHdrFormat)
                pixelformat = SurfaceFormat.Vector4;
            TextureCube destination = new TextureCube(gd, sizeSquarePerFace, generateMips, pixelformat);
            RenderTextureCubeToTextureCube(gd, _textureCubeBuildEffect, "CubeMapToTexture", source, ref destination, generateMips, pixelformat, sizeSquarePerFace);
            return destination;
        }

        public static TextureCube ConvertTextureCubeToIrradianceMap(GraphicsDevice gd, Effect _textureCubeBuildEffect, TextureCube source, bool generateMips, bool useHdrFormat, int sizeSquarePerFace)
        {
            var pixelformat = SurfaceFormat.Color;
            if (useHdrFormat)
                pixelformat = SurfaceFormat.Vector4;
            TextureCube destination = new TextureCube(gd, sizeSquarePerFace, generateMips, pixelformat);
            RenderTextureCubeToTextureCube(gd, _textureCubeBuildEffect, "CubemapToDiffuseIlluminationCubeMap", source, ref destination, generateMips, pixelformat, sizeSquarePerFace);
            return destination;
        }

        public static Texture2D ConvertTextureCubeToSphericalTexture2D(GraphicsDevice gd, Effect _textureCubeBuildEffect, TextureCube source, bool generateMips, bool useHdrFormat, int sizeSquarePerFace)
        {
            var pixelformat = SurfaceFormat.Color;
            if (useHdrFormat)
                pixelformat = SurfaceFormat.Vector4;
            Texture2D destinationMap = new Texture2D(gd, sizeSquarePerFace, sizeSquarePerFace /2, generateMips, pixelformat);
            RenderTextureCubeToSphericalTexture2D(gd, _textureCubeBuildEffect, "CubeMapToSpherical", source, ref destinationMap, generateMips, pixelformat, sizeSquarePerFace);
            return destinationMap;
        }

        public static Texture2D[] ConvertTextureCubeToTexture2DArray(GraphicsDevice gd, Effect _textureCubeBuildEffect, TextureCube source, bool generateMips, bool useHdrFormat, int sizeSquarePerFace)
        {
            var pixelformat = SurfaceFormat.Color;
            if (useHdrFormat)
                pixelformat = SurfaceFormat.Vector4;
            Texture2D[] destinationMap = new Texture2D[6];
            //for (int i = 0; i < 6; i++)
            //    destinationMap[i] = new Texture2D(gd, sizeSquarePerFace, sizeSquarePerFace, generateMips, pixelformat);
            RenderTextureCubeToTexture2DArray(gd, _textureCubeBuildEffect, "CubeMapToTexture", source, ref destinationMap, generateMips, pixelformat, sizeSquarePerFace);
            return destinationMap;
        }

        public static Texture2D ConvertTexture2DArrayToSphericalTexture2D(GraphicsDevice gd, Effect _textureCubeBuildEffect, Texture2D[] source, bool generateMips, bool useHdrFormat, int sizeSquarePerFace)
        {
            var pixelformat = SurfaceFormat.Color;
            if (useHdrFormat)
                pixelformat = SurfaceFormat.Vector4;
            Texture2D destinationMap = new Texture2D(gd, sizeSquarePerFace, sizeSquarePerFace / 2, generateMips, pixelformat);
            RenderTexture2DArrayToSphericalTexture2D(gd, _textureCubeBuildEffect, "TextureFacesToSpherical", source, ref destinationMap, generateMips, pixelformat, sizeSquarePerFace);
            return destinationMap;
        }

        public static TextureCube ConvertTexture2DArrayToTextureCube(GraphicsDevice gd, Effect _textureCubeBuildEffect, Texture2D[] source, bool generateMips, bool useHdrFormat, int sizeSquarePerFace)
        {
            var pixelformat = SurfaceFormat.Color;
            if (useHdrFormat)
                pixelformat = SurfaceFormat.Vector4;
            TextureCube destination = new TextureCube(gd, sizeSquarePerFace, generateMips, pixelformat);
            RenderTexture2DArrayToTextureCube(gd, _textureCubeBuildEffect, "TextureFacesToCubeFaces", source, ref destination, generateMips, pixelformat, sizeSquarePerFace);
            return destination;
        }

        //_________________________________________________
        //_________________________________________________
        //

        #region private methods

        private static void RenderSphericalTexture2DToSphericalTexture2D(GraphicsDevice gd, Effect _hdrEffect, string Technique, Texture2D sourceTextureSpherical, ref Texture2D textureDestinationMap, bool generateMips, SurfaceFormat pixelformat, int sizeSquarePerFace)
        {
            gd.RasterizerState = RasterizerState.CullNone;
            _hdrEffect.CurrentTechnique = _hdrEffect.Techniques[Technique];
            _hdrEffect.Parameters["Texture"].SetValue(sourceTextureSpherical);
            var renderTarget2D = new RenderTarget2D(gd, sizeSquarePerFace, sizeSquarePerFace / 2, generateMips, pixelformat, DepthFormat.None);
            gd.SetRenderTarget(renderTarget2D);
            foreach (EffectPass pass in _hdrEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, screenQuad.vertices, 0, 2);
            }
            //var textureSpherical = new RenderTarget2D(gd, sizeSquarePerFace, sizeSquarePerFace, generateMips, pixelformat, DepthFormat.None);
            textureDestinationMap = renderTarget2D; // set the render to the specified texture.
            gd.SetRenderTarget(null);
        }

        /// <summary>
        /// Renders the hdr texture to a TextureCube.
        /// The ref is used to pass the ref variable directly thru here, its not a ref copy i guess.
        /// </summary>
        private static void RenderSphericalTexture2DToTextureCube(GraphicsDevice gd, Effect _hdrEffect, string Technique, Texture2D sourceTextureSpherical, ref TextureCube textureCubeDestinationMap, bool generateMips, SurfaceFormat pixelformat, int sizeSquarePerFace)
        {
            gd.RasterizerState = RasterizerState.CullNone;
            var renderTargetCube = new RenderTargetCube(gd, sizeSquarePerFace, generateMips, pixelformat, DepthFormat.None);
            _hdrEffect.CurrentTechnique = _hdrEffect.Techniques[Technique];
            _hdrEffect.Parameters["Texture"].SetValue(sourceTextureSpherical);
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
        /// Renders the hdr spherical map to a array of 6 texture2D faces using the designated effect and technique.
        /// </summary>
        private static void RenderSphericalTexture2DToTexture2DArray(GraphicsDevice gd, Effect _hdrEffect, string Technique, Texture2D source, ref Texture2D[] textureFaceArray, bool generateMips, SurfaceFormat pixelformat, int sizeSquarePerFace)
        {
            gd.RasterizerState = RasterizerState.CullNone;
            textureFaceArray = new Texture2D[6];
            _hdrEffect.CurrentTechnique = _hdrEffect.Techniques[Technique];
            _hdrEffect.Parameters["Texture"].SetValue(source);
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
                textureFaceArray[i] = renderTarget2D; 
            }
            gd.SetRenderTarget(null);
        }

        /// <summary>
        /// Renders the hdr TextureCube to a array of 6 texture2D faces using the designated effect and technique.
        /// </summary>
        private static void RenderTextureCubeToSphericalTexture2D(GraphicsDevice gd, Effect _hdrEffect, string Technique, TextureCube sourceTextureCube, ref Texture2D textureSpherical, bool generateMips, SurfaceFormat pixelformat, int sizeSquarePerFace)
        {
            gd.RasterizerState = RasterizerState.CullNone;
            _hdrEffect.CurrentTechnique = _hdrEffect.Techniques[Technique];
            _hdrEffect.Parameters["CubeMap"].SetValue(sourceTextureCube);
            for (int i = 0; i < 6; i++)
            {
                var renderTarget2D = new RenderTarget2D(gd, sizeSquarePerFace, sizeSquarePerFace /2, generateMips, pixelformat, DepthFormat.None);
                gd.SetRenderTarget(renderTarget2D);
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
        /// Renders the hdr TextureCube to a array of 6 texture2D faces using the designated effect and technique.
        /// </summary>
        private static void RenderTextureCubeToTexture2DArray(GraphicsDevice gd, Effect _hdrEffect, string Technique, TextureCube sourceTextureCube, ref Texture2D[] textureFaceArray, bool generateMips, SurfaceFormat pixelformat, int sizeSquarePerFace)
        {
            gd.RasterizerState = RasterizerState.CullNone;
            textureFaceArray = new Texture2D[6];
            _hdrEffect.CurrentTechnique = _hdrEffect.Techniques[Technique];
            _hdrEffect.Parameters["CubeMap"].SetValue(sourceTextureCube);
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
        private static void RenderTexture2DArrayToTextureCube(GraphicsDevice gd, Effect _hdrEffect, string Technique, Texture2D[] sourceTextureFaceArray, ref TextureCube textureCubeDestinationMap, bool generateMips, SurfaceFormat pixelformat, int sizeSquarePerFace)
        {
            gd.RasterizerState = RasterizerState.CullNone;
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

        private static void RenderTexture2DArrayToSphericalTexture2D(GraphicsDevice gd, Effect _hdrEffect, string Technique, Texture2D[] sourceTextureFaceArray, ref Texture2D textureSpherical, bool generateMips, SurfaceFormat pixelformat, int sizeSquarePerFace)
        {
            gd.RasterizerState = RasterizerState.CullNone;
            _hdrEffect.CurrentTechnique = _hdrEffect.Techniques[Technique];
            var renderTarget2D = new RenderTarget2D(gd, sizeSquarePerFace, sizeSquarePerFace / 2, generateMips, pixelformat, DepthFormat.None);
            gd.SetRenderTarget(renderTarget2D);
            for (int i = 0; i < 6; i++)
            {
                _hdrEffect.Parameters["Texture"].SetValue(sourceTextureFaceArray[i]);
                _hdrEffect.Parameters["FaceToMap"].SetValue(i); 
                foreach (EffectPass pass in _hdrEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserPrimitives(PrimitiveType.TriangleList, screenQuad.vertices, 0, 2);
                }
            }
            textureSpherical = renderTarget2D;
            gd.SetRenderTarget(null);
        }

        /// <summary>
        /// Renders the hdr TextureCube to another TextureCube using the designated effect and technique.
        /// </summary>
        private static void RenderTextureCubeToTextureCube(GraphicsDevice gd, Effect _hdrEffect, string Technique, TextureCube sourceTextureCube, ref TextureCube textureCubeDestinationMap, bool generateMips, SurfaceFormat pixelformat, int sizeSquarePerFace)
        {
            gd.RasterizerState = RasterizerState.CullNone;
            var renderTargetCube = new RenderTargetCube(gd, sizeSquarePerFace, generateMips, pixelformat, DepthFormat.None);
            _hdrEffect.CurrentTechnique = _hdrEffect.Techniques[Technique];
            _hdrEffect.Parameters["CubeMap"].SetValue(sourceTextureCube);
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
                _hdrEffect.Parameters["FaceToMap"].SetValue(i);
                foreach (EffectPass pass in _hdrEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserPrimitives(PrimitiveType.TriangleList, screenQuad.vertices, 0, 2);
                }
            }
            textureCubeDestinationMap = renderTargetCube;
            gd.SetRenderTarget(null);
        }

        public static void SaveTexture2D(string path, Texture2D t)
        {
            using (System.IO.Stream fs = System.IO.File.OpenWrite(path))
            {
                t.SaveAsPng(fs, t.Width, t.Height);
            }
        }

        #endregion

        private class PrimitiveScreenQuad
        {
            public VertexPositionTexture[] vertices;

            public PrimitiveScreenQuad(bool clockwise, float depth)
            {
                float left = -1;
                float right = 1;
                float top = -1;
                float bottom = 1;

                bool flipY = false; // this really has very little effect on anything as this is only used for the rendertarget duh.

                vertices = new VertexPositionTexture[6];
                //
                if (clockwise)
                {
                    vertices[0] = GetVertex(new Vector3(left, top, depth), flipY);  // p1
                    vertices[1] = GetVertex(new Vector3(left, bottom, depth), flipY); // p0
                    vertices[2] = GetVertex(new Vector3(right, bottom, depth), flipY); // p3

                    vertices[3] = GetVertex(new Vector3(right, bottom, depth), flipY); // p3
                    vertices[4] = GetVertex(new Vector3(right, top, depth), flipY); // p2
                    vertices[5] = GetVertex(new Vector3(left, top, depth), flipY); // p1
                }
                else
                {
                    vertices[0] = GetVertex(new Vector3(left, top, depth), flipY);  // p1
                    vertices[2] = GetVertex(new Vector3(left, bottom, depth), flipY); // p0
                    vertices[1] = GetVertex(new Vector3(right, bottom, depth), flipY); // p3

                    vertices[4] = GetVertex(new Vector3(right, bottom, depth), flipY); // p3
                    vertices[3] = GetVertex(new Vector3(right, top, depth), flipY); // p2
                    vertices[5] = GetVertex(new Vector3(left, top, depth), flipY); // p1
                }
            }
        }
        public static VertexPositionTexture GetVertex(Vector3 p, bool flipY)
        {
            var uv = (new Vector2(p.X, p.Y) + Vector2.One) / 2f;
            if (flipY)
            {
                //p.Y = -p.Y;
                uv.Y = 1.0f - uv.Y;
            }
            return new VertexPositionTexture(p, uv);
        }
    }
}
