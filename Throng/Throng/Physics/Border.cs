using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

using Gnomic.Core;
using Gnomic.Anim;

namespace Throng
{
    public class Border
    {
        private Body _anchor;

        public Vertices Corners{ get; set; }
        
        public Border(World world, Vector2 worldSize)
        {
            float simWidth = ConvertUnits.ToSimUnits(worldSize.X);
            float simHeight = ConvertUnits.ToSimUnits(worldSize.Y);

            Corners = new Vertices(4);
            Corners.Add(new Vector2(0f, 0f));
            Corners.Add(new Vector2(simWidth, 0f));
            Corners.Add(new Vector2(simWidth, simHeight));
            Corners.Add(new Vector2(0f, simHeight));

            _anchor = BodyFactory.CreateLoopShape(world, Corners);
            _anchor.CollisionCategories =
                (Category)CharacterEntity.CollisionCategory.Environment;
            _anchor.CollidesWith =
                (Category)CharacterEntity.CollidesWith.Environment;
        }
    }
}