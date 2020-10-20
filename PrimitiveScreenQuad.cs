using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
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
