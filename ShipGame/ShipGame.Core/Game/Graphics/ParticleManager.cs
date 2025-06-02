#region Using Statements

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace ShipGame.Core.Game.Graphics;

public class ParticleManager : IDisposable
{
    // linked list of nodes to delete from the particle systems list
    private readonly List<LinkedListNode<ParticleSystem>> deleteSystems;

    private Effect effect; // effect

    private EffectParameter effectEndColor; // effect end color parameter

    private EffectParameter effectPointSize; // effect point size parameter

    private EffectParameter effectStartColor; // effect start color parameter

    private EffectTechnique effectTechnique; // effect technique

    private EffectParameter effectTexture; // effect texture parameter

    private EffectParameter effectTimes; // effect times parameter

    private EffectParameter effectVelocityScale; // effect velocity scale parameter

    private EffectParameter effectWorldViewProjection; // effect world view proj parameter

    // linked list of active particle systems
    private readonly LinkedList<ParticleSystem> systems;

    private VertexBuffer vertexBuffer; // the vertex buffer

    private int vertexCount; // number of vertices in vertex buffer

    private VertexDeclaration vertexDeclaration; // the vertex declaration

    // the vertices array
    private readonly VertexPositionNormalTexture[] vertices;

    /// <summary>Create a new particle system manager</summary>
    public ParticleManager()
    {
        vertices      = new VertexPositionNormalTexture[GameOptions.MaxParticles];
        systems       = new LinkedList<ParticleSystem>();
        deleteSystems = new List<LinkedListNode<ParticleSystem>>();
    }

    /// <summary>Add a new particle system</summary>
    public void Add(ParticleSystem ps)
    {
        // add to list of active particle systems
        systems.AddLast(ps);

        // set particles to vertex array
        int count = ps.AddToVertArray(vertices,
                                      vertexCount,
                                      GameOptions.MaxParticles - vertexCount);

        // if any particles created
        if (count > 0)
        {
            // set new particles to vertex buffer
            vertexBuffer.SetData(vertices);

            // add the number particles created 
            // (one vertex per particle as we are using point sprites)
            vertexCount += count;
        }
    }

    /// <summary>Update all particle systems</summary>
    public void Update(float elapsedTime)
    {
        // empty deleted particle systems list
        deleteSystems.Clear();

        // for each particle system
        LinkedListNode<ParticleSystem> Node = systems.First;

        while (Node != null)
        {
            // update animated sprite
            bool running = Node.Value.Update(elapsedTime);

            // if finished running add to delete list
            if (running == false)
            {
                deleteSystems.Add(Node);
            }

            // move to next node
            Node = Node.Next;
        }

        // delete all nodes in delete list
        foreach (LinkedListNode<ParticleSystem> s in deleteSystems)
        {
            systems.Remove(s);
        }

        // if any particle systems deleted
        if (deleteSystems.Count > 0)
        {
            // re-cretae vertex array
            vertexCount = 0;

            foreach (ParticleSystem ps in systems)
            {
                vertexCount += ps.AddToVertArray(vertices,
                                                 vertexCount,
                                                 GameOptions.MaxParticles - vertexCount);
            }

            // set vertex buffer
            if (vertexCount > 0)
            {
                vertexBuffer.SetData(vertices,
                                     startIndex: 0,
                                     vertexCount);
            }
        }
    }

    /// <summary>Draw all particle systems</summary>
    public void Draw(GraphicsDevice gd, Matrix viewProjection)
    {
        if (gd == null)
        {
            throw new ArgumentNullException(paramName: "gd");
        }

        // if no particle systems or no vertices, return
        if (systems.Count  == 0
            || vertexCount == 0)
        {
            return;
        }

        // enable alpha blending and disable depth write
        gd.BlendState        = BlendState.AlphaBlend;
        gd.DepthStencilState = DepthStencilState.DepthRead;

        // enable point sprite
        //gd.RenderState.PointSpriteEnable = true;
        //gd.RenderState.PointSizeMax = 128;

        // select technique
        effect.CurrentTechnique = effectTechnique;

        // set vertex buffer and declaration
        gd.SetVertexBuffer(vertexBuffer);

        // for each particle system
        int vertexPosition = 0;

        foreach (ParticleSystem ps in systems)
        {
            // set effect parameters
            DrawMode mode = ps.SetEffect(effectWorldViewProjection,
                                         effectTexture,
                                         effectStartColor,
                                         effectEndColor,
                                         effectTimes,
                                         effectPointSize,
                                         effectVelocityScale,
                                         viewProjection);

            // if additive blend
            if (((int)mode & 1) != 0)

                // set additive blend
            {
                gd.BlendState = BlendState.Additive;
            }
            else

                // set alpha blend
            {
                gd.BlendState = BlendState.AlphaBlend;
            }

            // if glow enabled
            //if (((int)mode & 2) != 0)
            //    gd.RenderState.AlphaSourceBlend = Blend.One;
            //else
            //    gd.RenderState.AlphaSourceBlend = Blend.Zero;

            // get number of particles in this particle system
            int numberVertices = ps.RenderCount;

            if (numberVertices > 0)
            {
                // draw the point sprites
                //gd.DrawPrimitives(PrimitiveType.PointList, vertexPosition,
                //    numberVertices);

                // apply effect pass
                effect.CurrentTechnique.Passes[index: 0].Apply();

                gd.DrawPrimitives(PrimitiveType.LineList,
                                  vertexPosition,
                                  numberVertices);

                // update vertex buffer position
                vertexPosition += numberVertices;
            }
            else

                // if negative count, particle system is disabled
            if (numberVertices < 0)

                // skip all vertices 
            {
                vertexPosition += -numberVertices;
            }
        }

        // reset vertex buffer and declaration
        gd.SetVertexBuffer(vertexBuffer: null);

        // reset blend and depth write
        gd.BlendState        = BlendState.Opaque;
        gd.DepthStencilState = DepthStencilState.Default;

        // disable point sprite
        //gd.RenderState.PointSpriteEnable = false;
    }

    /// <summary>Load content</summary>
    public void LoadContent(GraphicsDevice gd,
                            ContentManager content)
    {
        if (gd == null)
        {
            throw new ArgumentNullException(paramName: "gd");
        }

        // load effect
        effect = content.Load<Effect>(assetName: "shaders/Particle");

        // get techinque
        effectTechnique = effect.Techniques[name: "Particle"];

        // get parameters
        effectWorldViewProjection = effect.Parameters[name: "WorldViewProj"];
        effectTexture             = effect.Parameters[name: "Texture"];
        effectStartColor          = effect.Parameters[name: "StartColor"];
        effectEndColor            = effect.Parameters[name: "EndColor"];
        effectTimes               = effect.Parameters[name: "Times"];
        effectPointSize           = effect.Parameters[name: "PointSize"];
        effectVelocityScale       = effect.Parameters[name: "VelocityScale"];

        // create the vertex buffer
        vertexBuffer = new VertexBuffer(gd,
                                        typeof(VertexPositionNormalTexture),
                                        GameOptions.MaxParticles,
                                        BufferUsage.WriteOnly | BufferUsage.None);

        // create the vertex declaration
        vertexDeclaration =
            new VertexDeclaration(VertexPositionNormalTexture.VertexDeclaration.GetVertexElements());

        // if any particles in vertex array set them to vertex buffer
        if (vertexCount > 0)
        {
            vertexBuffer.SetData(vertices,
                                 startIndex: 0,
                                 vertexCount);
        }
    }

    /// <summary>Unload content</summary>
    public void UnloadContent()
    {
        // unload effect and parameters
        effectWorldViewProjection = null;
        effectTexture             = null;
        effectStartColor          = null;
        effectEndColor            = null;
        effectTimes               = null;
        effectPointSize           = null;
        effectVelocityScale       = null;
        effectTechnique           = null;
        effect                    = null;

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
            UnloadContent();
        }
    }

    #endregion
}
