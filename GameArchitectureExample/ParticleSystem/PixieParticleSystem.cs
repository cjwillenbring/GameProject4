﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameArchitectureExample.ParticleSystem
{
    public class PixieParticleSystem : ParticleSystem
    {
        IParticleEmitter _emitter;

        public PixieParticleSystem(Game game, IParticleEmitter emitter) : base(game, 2000)
        {
            _emitter = emitter;
        }

        protected override void InitializeConstants()
        {
            textureFilename = "particle";

            minNumParticles = 2;
            maxNumParticles = 5;

            blendState = BlendState.Additive;
            DrawOrder = AdditiveBlendDrawOrder;
        }

        protected override void InitializeParticle(ref Particle p, Vector2 where)
        {
            var velocity = _emitter.Velocity * -.5f;

            var acceleration = Vector2.UnitY * 50;

            var scale = RandomHelper.NextFloat(.1f, .5f);

            var lifetime = RandomHelper.NextFloat(.1f, .3f);

            p.Initialize(where, velocity, acceleration, Color.Goldenrod, scale: scale, lifetime: lifetime);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            AddParticles(_emitter.EmmitterPosition);
        }
    }
}
