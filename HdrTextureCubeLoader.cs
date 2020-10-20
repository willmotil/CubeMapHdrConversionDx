using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    public static class HdrTextureCubeLoader
    {
        private static PrimitiveScreenQuad screenQuad = new PrimitiveScreenQuad(false);

        /// <summary>
        /// Renders the hdr texture to a TextureCube.
        /// The ref is used to pass the ref variable directly thru here, its not a ref copy i guess.
        /// </summary>
        public static void RenderToSceneFaces(GraphicsDevice gd, Effect _hdrEffect, string Technique, Texture2D sourceHdrLdrEquaRectangularMap, ref TextureCube textureCubeDestinationMap, bool generateMips, bool useHdrFormat, int sizeSquarePerFace)
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

            Console.WriteLine($" SphericalTex2D as source ... Technique: {Technique}  resulting TextureCube.Format: {textureCubeDestinationMap.Format}  Mip LevelCount: {textureCubeDestinationMap.LevelCount}");

            //return textureCubeDestinationMap;
        }

        /// <summary>
        /// Renders the hdr TextureCube to another TextureCube using the designated effect and technique.
        /// The ref was used to pass the ref variable directly thru here not a ref copy i guess.
        /// </summary>
        public static void RenderToSceneFaces(GraphicsDevice gd, Effect _hdrEffect, string Technique, TextureCube sourceHdrLdrEnvMap, ref TextureCube textureCubeDestinationMap, bool generateMips, bool useHdrFormat, int sizeSquarePerFace)
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

            Console.WriteLine($" Cubemap as source ... Technique: {Technique}  resulting TextureCube.Format: {textureCubeDestinationMap.Format}  Mip LevelCount: {textureCubeDestinationMap.LevelCount}");
        }
    }
}
