using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OculusRiftSample
{
    class ManyCubes
    {
        GraphicsDevice gd;
        VertexBuffer vertexBuffer;
        BasicEffect effect;

        public ManyCubes(GraphicsDevice graphicsDevice)
        {
            gd = graphicsDevice;

            effect = new BasicEffect(gd);
            effect.EnableDefaultLighting();

            //DepthStencilState..DepthBufferEnable = true;
            //DepthStencilState.Default.DepthBufferWriteEnable = true;
            gd.DepthStencilState = DepthStencilState.DepthRead;
            vertexBuffer = new VertexBuffer(gd, VertexPositionNormalTexture.VertexDeclaration, 36, BufferUsage.WriteOnly);
            vertexBuffer.SetData(BuildCubeVertices());
        }

        public void Draw(Matrix view, Matrix projection)
        {
            effect.View = view;
            effect.Projection = projection;

            gd.SetVertexBuffer(vertexBuffer);

            float area = 5; 
            float pyramidSize = 1f;
            float pyramidHeight = 1.5f;
            float stepHeight = 0.12f;
            float stepShrink = 0.75f;       
            float pyramidDist = 1.5f;

            Matrix world = Matrix.CreateScale(pyramidSize, stepHeight*2, pyramidSize);

            for (float y = 0; y <= pyramidHeight; y += stepHeight)
            {
                for (float x = -area; x <= area; x += pyramidDist)
                {
                    for (float z = -area; z <= area; z += pyramidDist)
                    {
                        world.Translation = new Vector3(x, y, z);
                        effect.World = world;

                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 12);
                        }
                    }
                }

                pyramidSize *= stepShrink;
                world = Matrix.CreateScale(pyramidSize, stepHeight, pyramidSize);
            }
        }

        VertexPositionNormalTexture[] BuildCubeVertices()
        {
            var vertices = new VertexPositionNormalTexture[36];

            float s = 0.5f;

            // Calculate the position of the vertices on the top face.
            Vector3 topLeftFront = new Vector3(-s, s, -s);
            Vector3 topLeftBack = new Vector3(-s, s, s);
            Vector3 topRightFront = new Vector3(s, s, -s);
            Vector3 topRightBack = new Vector3(s, s, s);

            // Calculate the position of the vertices on the bottom face.
            Vector3 btmLeftFront = new Vector3(-s, -s, -s);
            Vector3 btmLeftBack = new Vector3(-s, -s, s);
            Vector3 btmRightFront = new Vector3(s, -s, -s);
            Vector3 btmRightBack = new Vector3(s, -s, s);

            // Normal vectors for each face (needed for lighting / display)
            Vector3 normalFront = new Vector3(0, 0, s);
            Vector3 normalBack = new Vector3(0, 0, -s);
            Vector3 normalTop = new Vector3(0, s, 0);
            Vector3 normalBottom = new Vector3(0, -s, 0);
            Vector3 normalLeft = new Vector3(-s, 0, 0);
            Vector3 normalRight = new Vector3(s, 0, 0);

            // UV texture coordinates
            Vector2 textureTopLeft = new Vector2(s, 0);
            Vector2 textureTopRight = new Vector2(0, 0);
            Vector2 textureBottomLeft = new Vector2(s, s);
            Vector2 textureBottomRight = new Vector2(0, s);

            // Add the vertices for the FRONT face.
            vertices[0] = new VertexPositionNormalTexture(topLeftFront, normalFront, textureTopLeft);
            vertices[1] = new VertexPositionNormalTexture(btmLeftFront, normalFront, textureBottomLeft);
            vertices[2] = new VertexPositionNormalTexture(topRightFront, normalFront, textureTopRight);
            vertices[3] = new VertexPositionNormalTexture(btmLeftFront, normalFront, textureBottomLeft);
            vertices[4] = new VertexPositionNormalTexture(btmRightFront, normalFront, textureBottomRight);
            vertices[5] = new VertexPositionNormalTexture(topRightFront, normalFront, textureTopRight);

            // Add the vertices for the BACK face.
            vertices[6] = new VertexPositionNormalTexture(topLeftBack, normalBack, textureTopRight);
            vertices[7] = new VertexPositionNormalTexture(topRightBack, normalBack, textureTopLeft);
            vertices[8] = new VertexPositionNormalTexture(btmLeftBack, normalBack, textureBottomRight);
            vertices[9] = new VertexPositionNormalTexture(btmLeftBack, normalBack, textureBottomRight);
            vertices[10] = new VertexPositionNormalTexture(topRightBack, normalBack, textureTopLeft);
            vertices[11] = new VertexPositionNormalTexture(btmRightBack, normalBack, textureBottomLeft);

            // Add the vertices for the TOP face.
            vertices[12] = new VertexPositionNormalTexture(topLeftFront, normalTop, textureBottomLeft);
            vertices[13] = new VertexPositionNormalTexture(topRightBack, normalTop, textureTopRight);
            vertices[14] = new VertexPositionNormalTexture(topLeftBack, normalTop, textureTopLeft);
            vertices[15] = new VertexPositionNormalTexture(topLeftFront, normalTop, textureBottomLeft);
            vertices[16] = new VertexPositionNormalTexture(topRightFront, normalTop, textureBottomRight);
            vertices[17] = new VertexPositionNormalTexture(topRightBack, normalTop, textureTopRight);

            // Add the vertices for the BOTTOM face. 
            vertices[18] = new VertexPositionNormalTexture(btmLeftFront, normalBottom, textureTopLeft);
            vertices[19] = new VertexPositionNormalTexture(btmLeftBack, normalBottom, textureBottomLeft);
            vertices[20] = new VertexPositionNormalTexture(btmRightBack, normalBottom, textureBottomRight);
            vertices[21] = new VertexPositionNormalTexture(btmLeftFront, normalBottom, textureTopLeft);
            vertices[22] = new VertexPositionNormalTexture(btmRightBack, normalBottom, textureBottomRight);
            vertices[23] = new VertexPositionNormalTexture(btmRightFront, normalBottom, textureTopRight);

            // Add the vertices for the LEFT face.
            vertices[24] = new VertexPositionNormalTexture(topLeftFront, normalLeft, textureTopRight);
            vertices[25] = new VertexPositionNormalTexture(btmLeftBack, normalLeft, textureBottomLeft);
            vertices[26] = new VertexPositionNormalTexture(btmLeftFront, normalLeft, textureBottomRight);
            vertices[27] = new VertexPositionNormalTexture(topLeftBack, normalLeft, textureTopLeft);
            vertices[28] = new VertexPositionNormalTexture(btmLeftBack, normalLeft, textureBottomLeft);
            vertices[29] = new VertexPositionNormalTexture(topLeftFront, normalLeft, textureTopRight);

            // Add the vertices for the RIGHT face. 
            vertices[30] = new VertexPositionNormalTexture(topRightFront, normalRight, textureTopLeft);
            vertices[31] = new VertexPositionNormalTexture(btmRightFront, normalRight, textureBottomLeft);
            vertices[32] = new VertexPositionNormalTexture(btmRightBack, normalRight, textureBottomRight);
            vertices[33] = new VertexPositionNormalTexture(topRightBack, normalRight, textureTopRight);
            vertices[34] = new VertexPositionNormalTexture(topRightFront, normalRight, textureTopLeft);
            vertices[35] = new VertexPositionNormalTexture(btmRightBack, normalRight, textureBottomRight);

            return vertices;
        }
    }
}
