
using System;
using System.Collections.Generic;
//using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    public class Primitive2dQuadBuffer
    {
        List<VertexPositionNormalTexture> _verticeList = new List<VertexPositionNormalTexture>();
        VertexPositionNormalTexture[] _vertices;
        public void AddVertexRectangleToBuffer(Rectangle r, float depth)
        {
            //if (GraphicsDevice.RasterizerState != RasterizerState.CullClockwise)
            var normal = Vector3.Normalize(new Vector3(0, 0, depth));
            _verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Left, r.Top, depth), normal, new Vector2(0f, 0f))); ;  // p1
            _verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Left, r.Bottom, depth), normal, new Vector2(0f, 1f))); // p0
            _verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Right, r.Bottom, depth), normal, new Vector2(1f, 1f)));// p3

            _verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Right, r.Bottom, depth), normal, new Vector2(1f, 1f)));// p3
            _verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Right, r.Top, depth), normal, new Vector2(1f, 0f)));// p2
            _verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Left, r.Top, depth), normal, new Vector2(0f, 0f))); // p1

            _vertices = _verticeList.ToArray();
        }

        public void AlterVertexRectanglePositionInBuffer(int index, Rectangle r, float depth)
        {
            // Triangle 1
            _vertices[index + 0].Position = new Vector3(r.Left, r.Top, depth);  // p1
            _vertices[index + 1].Position = new Vector3(r.Left, r.Bottom, depth); // p0
            _vertices[index + 2].Position = new Vector3(r.Right, r.Bottom, depth); // p3
            // Triangle 2
            _vertices[index + 3].Position = new Vector3(r.Right, r.Bottom, depth);// p3
            _vertices[index + 4].Position = new Vector3(r.Right, r.Top, depth); // p2
            _vertices[index + 5].Position = new Vector3(r.Left, r.Top, depth); // p1
        }
        public void DrawQuadBuffer(GraphicsDevice device, Effect effect)
        {
            if (_vertices != null)
            {
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    int numberOfTriangles = _vertices.Length / 3;
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, numberOfTriangles);
                }
            }
        }
        public void DrawQuadRangeInBuffer(GraphicsDevice device, Effect effect, int startQuad, int quadDrawLength)
        {
            int startVertice = startQuad * 2 * 3;
            int numberOfTriangles = quadDrawLength * 2;
            if (_vertices != null)
            {
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, startVertice, numberOfTriangles);
                }
            }
        }
    }
}
