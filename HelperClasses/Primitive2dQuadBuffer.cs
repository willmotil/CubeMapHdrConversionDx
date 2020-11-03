
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
        public void AddVertexRectangleToBuffer(GraphicsDevice gd, Rectangle r, float depth, bool isPerspective)
        {
            var scalar = new Vector3(1, 1, 1);
            if( isPerspective == false)
            {
                //if (GraphicsDevice.RasterizerState != RasterizerState.CullClockwise)
                //var scalar = new Vector3(1f / (gd.Viewport.Width /2), 1f / (gd.Viewport.Height / 2), 1f);
                scalar = new Vector3(1f / (gd.Viewport.Width), 1f / (gd.Viewport.Height), 1f);
            }
            var normal = Vector3.Normalize(new Vector3(0, 0, depth));
            _verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Left, r.Top, depth) * scalar, normal, new Vector2(0f, 0f))); ;  // p1
            _verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Left, r.Bottom, depth) * scalar, normal, new Vector2(0f, 1f))); // p0
            _verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Right, r.Bottom, depth) * scalar, normal, new Vector2(1f, 1f)));// p3

            _verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Right, r.Bottom, depth) * scalar, normal, new Vector2(1f, 1f)));// p3
            _verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Right, r.Top, depth) * scalar, normal, new Vector2(1f, 0f)));// p2
            _verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Left, r.Top, depth) * scalar, normal, new Vector2(0f, 0f))); // p1

            _vertices = _verticeList.ToArray();
        }

        public void AlterVertexRectanglePositionInBuffer(GraphicsDevice gd, int index, Rectangle r, float depth)
        {
            var scalar = new Vector3(1f / (gd.Viewport.Width / 2), 1f / (gd.Viewport.Height / 2), 1f);
            //scalar = new Vector3(1, 1, 1);
            // Triangle 1
            _vertices[index + 0].Position = new Vector3(r.Left, r.Top, depth)* scalar;  // p1
            _vertices[index + 1].Position = new Vector3(r.Left, r.Bottom, depth) * scalar; // p0
            _vertices[index + 2].Position = new Vector3(r.Right, r.Bottom, depth) * scalar; // p3
            // Triangle 2
            _vertices[index + 3].Position = new Vector3(r.Right, r.Bottom, depth) * scalar;// p3
            _vertices[index + 4].Position = new Vector3(r.Right, r.Top, depth) * scalar; // p2
            _vertices[index + 5].Position = new Vector3(r.Left, r.Top, depth) * scalar; // p1
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
