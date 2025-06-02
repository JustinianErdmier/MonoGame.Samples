#region Using Statements

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace ShipGame.Core.Game.Graphics;

// supported render techniques
public enum BlurTechnique
{
    Color = 0, // plain color

    ColorTexture, // plain texture mapping

    BlurHorizontal, // horizontal blur

    BlurVertical, // vertical blur

    BlurHorizontalSplit // horizontal split screen blur
}

public class BlurManager : IDisposable
{
    // blur effect
    private readonly Effect blurEffect;

    private readonly EffectParameter paramColor; // color 

    private readonly EffectParameter paramColorMap; // color texture

    private readonly EffectParameter paramPixelSize; // pixel size

    // parameters
    private readonly EffectParameter paramWorldViewProjection; // world * view * proj matrix

    // normalized pixel size (1.0/size)
    private readonly Vector2 pixelSize;

    // render target resolution
    private readonly int sizeX;

    private readonly int sizeY;

    private VertexBuffer vertexBuffer;

    // screen quad vertex declaration and buffer
    private VertexDeclaration vertexDeclaration;

    // 2D ortho view projection matrix
    private readonly Matrix viewProjection;

    /// <summary>Create a new blur manager</summary>
    public BlurManager(GraphicsDevice gd, Effect effect, int sizex, int sizey)
    {
        if (gd == null)
        {
            throw new ArgumentNullException(paramName: "gd");
        }

        if (effect == null)
        {
            throw new ArgumentNullException(paramName: "effect");
        }

        blurEffect = effect; // save effect
        sizeX      = sizey; // save horizontal buffer size
        sizeY      = sizex; // save verical buffer size

        // get effect parameters
        paramWorldViewProjection = blurEffect.Parameters[name: "g_WorldViewProj"];
        paramColorMap            = blurEffect.Parameters[name: "g_ColorMap"];
        paramColor               = blurEffect.Parameters[name: "g_Color"];
        paramPixelSize           = blurEffect.Parameters[name: "g_PixelSize"];

        pixelSize      = new Vector2(1.0f / sizeX, 1.0f / sizeY);
        viewProjection = Matrix.CreateOrthographicOffCenter(left: 0, sizeX, bottom: 0, sizeY, zNearPlane: -1, zFarPlane: 1);

        // create vertex buffer
        vertexBuffer = new VertexBuffer(gd,
                                        typeof(VertexPositionTexture),
                                        vertexCount: 6,
                                        BufferUsage.WriteOnly);

        // create vertex declaration
        vertexDeclaration =
            new VertexDeclaration(VertexPositionTexture.VertexDeclaration.GetVertexElements());

        // create vertex data
        SetVertexData();
    }

    /// <summary>Set vertex data with textureCube vertex normals (used for cubemap blur option only)</summary>
    public void SetVertexData()
    {
        VertexPositionTexture[] data = new VertexPositionTexture[6];

        data[0] = new VertexPositionTexture(new Vector3(x: 0, y: 0, z: 0), new Vector2(x: 0, y: 1));
        data[1] = new VertexPositionTexture(new Vector3(sizeX, sizeY, z: 0), new Vector2(x: 1, y: 0));
        data[2] = new VertexPositionTexture(new Vector3(sizeX, y: 0, z: 0), new Vector2(x: 1, y: 1));
        data[3] = new VertexPositionTexture(new Vector3(x: 0, y: 0, z: 0), new Vector2(x: 0, y: 1));
        data[4] = new VertexPositionTexture(new Vector3(x: 0, sizeY, z: 0), new Vector2(x: 0, y: 0));
        data[5] = new VertexPositionTexture(new Vector3(sizeX, sizeY, z: 0), new Vector2(x: 1, y: 0));

        vertexBuffer.SetData(data);

        data = null;
    }

    /// <summary>Render a screen aligned quad used to process the horizontal and vertical blur operations</summary>
    public void RenderScreenQuad(GraphicsDevice gd,
                                 BlurTechnique  technique,
                                 Texture2D      texture,
                                 Vector4        color)
    {
        if (gd == null)
        {
            throw new ArgumentNullException(paramName: "gd");
        }

        gd.SetVertexBuffer(vertexBuffer);

        blurEffect.CurrentTechnique = blurEffect.Techniques[(int)technique];

        paramWorldViewProjection.SetValue(viewProjection);
        paramPixelSize.SetValue(pixelSize);
        paramColorMap.SetValue(texture);
        paramColor.SetValue(color);

        blurEffect.CurrentTechnique.Passes[index: 0].Apply();
        gd.DrawPrimitives(PrimitiveType.TriangleList, vertexStart: 0, primitiveCount: 2);

        gd.SetVertexBuffer(vertexBuffer: null);
    }

    /// <summary>Render a screen aligned quad used to process the horizontal and vertical blur operations</summary>
    public void RenderScreenQuad(GraphicsDevice gd,
                                 BlurTechnique  technique,
                                 Texture2D      texture,
                                 Vector4        color,
                                 float          scale)
    {
        if (gd == null)
        {
            throw new ArgumentNullException(paramName: "gd");
        }

        gd.SetVertexBuffer(vertexBuffer);

        blurEffect.CurrentTechnique = blurEffect.Techniques[(int)technique];

        Matrix m = Matrix.CreateTranslation(-sizeX / 2, -sizeY / 2, zPosition: 0)
                   * Matrix.CreateScale(scale, scale, zScale: 1)
                   * Matrix.CreateTranslation(sizeX / 2, sizeY / 2, zPosition: 0);

        paramWorldViewProjection.SetValue(m * viewProjection);
        paramPixelSize.SetValue(pixelSize);
        paramColorMap.SetValue(texture);
        paramColor.SetValue(color);

        blurEffect.CurrentTechnique.Passes[index: 0].Apply();
        gd.DrawPrimitives(PrimitiveType.TriangleList, vertexStart: 0, primitiveCount: 2);

        gd.SetVertexBuffer(vertexBuffer: null);
    }

    #region IDisposable Members

    public bool IsDisposed { get; } = false;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing && !IsDisposed)
        {
            if (vertexBuffer != null)
            {
                vertexBuffer.Dispose();
                vertexBuffer = null;
            }

            if (vertexDeclaration != null)
            {
                vertexDeclaration.Dispose();
                vertexDeclaration = null;
            }
        }
    }

    #endregion
}
