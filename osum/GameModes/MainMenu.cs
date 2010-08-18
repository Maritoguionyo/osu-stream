﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.GameModes;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using OpenTK;
using OpenTK.Graphics;
using System.Drawing;

namespace osum.GameModes
{
    class MainMenu : GameMode
    {
		pSprite osuLogo;

        int sampleTest;
		
		internal override void Initialize()
        {
            pSprite menuBackground =
                new pSprite(SkinManager.Load("menu-background"), FieldTypes.StandardSnapCentre, OriginTypes.Centre,
                            ClockTypes.Game, Vector2.Zero, 0, true, Color.White);
            spriteManager.Add(menuBackground);

            osuLogo = new pSprite(SkinManager.Load("menu-osu"), FieldTypes.Standard, OriginTypes.Centre, ClockTypes.Game, new Vector2(GameBase.StandardSizeHalf.Width,GameBase.StandardSizeHalf.Height), 1, true, Color4.White);
            spriteManager.Add(osuLogo);
			
			osuLogo.Transform(new Transformation(TransformationType.Rotation,0,200,0,200000));
			
			//osuLogo.Transform(new Transformation(new Vector2(0,0),new Vector2(1024,768),0,5000));

            sampleTest = GameBase.Instance.soundEffectPlayer.Load("Skins/Default/normal-hitclap.wav");

            InputManager.OnDown += new InputHandler(InputManager_OnDown);
        }

        void InputManager_OnDown(InputSource source)
        {
            GameBase.Instance.soundEffectPlayer.PlayBuffer(sampleTest);
        }

        public override void Update()
        {
            if (InputManager.IsTracking)
				osuLogo.Position = InputManager.MainPointerPosition;
			
			base.Update();
        }

        public override void Draw()
        {
            base.Draw();
			
			osuLogo.ScaleScalar = 1+GameBase.Instance.backgroundAudioPlayer.CurrentVolume/100;
        }
    }
}