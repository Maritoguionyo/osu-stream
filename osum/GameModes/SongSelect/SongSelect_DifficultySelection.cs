﻿using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using osum.Audio;
using osum.GameModes;
using osum.GameplayElements.Beatmaps;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using osum.GameModes.SongSelect;
using OpenTK.Graphics;
using osum.GameModes.Play.Components;
using osum.Graphics.Drawables;
using osum.GameplayElements;
using System.Threading;
using osum.Graphics.Renderers;
using osu_common.Libraries.Osz2;

namespace osum.GameModes
{
    public partial class SongSelectMode : GameMode
    {
        private pSprite s_ModeButtonStream;

        private pSprite s_ModeArrowLeft;
        private pSprite s_ModeArrowRight;
        private pSprite s_ModeButtonEasy;
        private pSprite s_ModeButtonExpert;
        private pText s_ModeDescriptionText;

        /// <summary>
        /// True when expert mode is not yet unlocked for the current map.
        /// </summary>
        bool mapRequiresUnlock
        {
            get { return false; }
        }

        private void showDifficultySelection()
        {
            if (State != SelectState.LoadingPreview) return;

            AudioEngine.Music.Play();
            AudioEngine.Music.Volume = 0;
            AudioEngine.Music.SeekTo(30000);

            //do a second callback so we account for lost gametime due to the above audio load.
            GameBase.Scheduler.Add(delegate {

                if (State != SelectState.LoadingPreview) return;

                if (s_ModeButtonStream == null)
                {

                    tabController = new pTabController();
    
                    initializeTabPlay();
                    initializeTabRank();
                    initializeTabOptions();
                }
    
                tabController.Show();
    
    
                //preview has finished loading.
                State = SelectState.DifficultySelect;
    
                foreach (pDrawable s in SelectedPanel.Sprites)
                    s.MoveTo(new Vector2(0, 0), 500, EasingTypes.InDouble);
    
                tabController.Sprites.ForEach(s => s.Transform(new Transformation(new Vector2(0, -100), new Vector2(0, -100), Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In)));
                tabController.Sprites.ForEach(s => s.Transform(new Transformation(new Vector2(0, 0), new Vector2(0, BeatmapPanel.PANEL_HEIGHT), Clock.ModeTime + 400, Clock.ModeTime + 1000, EasingTypes.In)));
    
                s_Header.Transform(new Transformation(Vector2.Zero, new Vector2(0, -63), Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
                s_Header.Transform(new Transformation(TransformationType.Rotation, s_Header.Rotation, 0.03f, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
    
                s_Footer.Transform(new Transformation(new Vector2(-60, -105), Vector2.Zero, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
                s_Footer.Transform(new Transformation(TransformationType.Rotation, 0.04f, 0, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
    
                updateModeSelectionArrows();
            },true);
        }

        private void initializeTabOptions()
        {
            List<pDrawable> sprites = new List<pDrawable>();

            pSprite text = new pText("Local record goes here.", 30, new Vector2(0, 0), new Vector2(GameBase.BaseSizeFixedWidth.Width, 96), 1, true, Color4.White, true) { Field = FieldTypes.StandardSnapCentre, Origin = OriginTypes.Centre, TextAlignment = TextAlignment.Centre };
            sprites.Add(text);


            s_TabBarOther = tabController.Add(OsuTexture.songselect_tab_bar_other, sprites);
        }

        private void initializeTabRank()
        {
            List<pDrawable> sprites = new List<pDrawable>();

            pSprite text = new pText("Online ranking goes here.", 30, new Vector2(0, 0), new Vector2(GameBase.BaseSizeFixedWidth.Width, 96), 1, true, Color4.White, true) { Field = FieldTypes.StandardSnapCentre, Origin = OriginTypes.Centre, TextAlignment = TextAlignment.Centre };
            sprites.Add(text);

            s_TabBarRank = tabController.Add(OsuTexture.songselect_tab_bar_rank, sprites);
        }

        private void initializeTabPlay()
        {
            const float yOffset = -40;
            List<pDrawable> sprites = new List<pDrawable>();

            s_ModeButtonStream = new pSprite(TextureManager.Load(OsuTexture.songselect_mode_stream), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, 0), 0.4f, true, Color4.White) { Offset = new Vector2(0, yOffset), HandleClickOnUp = true };
            s_ModeButtonStream.OnClick += onModeButtonClick;
            sprites.Add(s_ModeButtonStream);

            s_ModeButtonEasy = new pSprite(TextureManager.Load(OsuTexture.songselect_mode_easy), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, 0), 0.4f, true, Color4.White) { Offset = new Vector2(-mode_button_width, yOffset), HandleClickOnUp = true };
            s_ModeButtonEasy.OnClick += onModeButtonClick;
            sprites.Add(s_ModeButtonEasy);

            s_ModeButtonExpert = new pSprite(TextureManager.Load(OsuTexture.songselect_mode_expert), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, 0), 0.4f, true, mapRequiresUnlock ? Color4.Gray : Color4.White) { Offset = new Vector2(mode_button_width, yOffset), HandleClickOnUp = true };
            s_ModeButtonExpert.OnClick += onModeButtonClick;
            sprites.Add(s_ModeButtonExpert);

            const float arrow_spread = 180;

            s_ModeArrowLeft = new pSprite(TextureManager.Load(OsuTexture.songselect_mode_arrow), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(-arrow_spread, yOffset), 0.45f, true, Color4.White);
            s_ModeArrowLeft.OnHover += delegate { s_ModeArrowLeft.ScaleTo(1.2f, 100, EasingTypes.In); };
            s_ModeArrowLeft.OnHoverLost += delegate { s_ModeArrowLeft.ScaleTo(1f, 100, EasingTypes.In); };
            s_ModeArrowLeft.OnClick += onSelectPreviousMode;

            sprites.Add(s_ModeArrowLeft);

            s_ModeArrowRight = new pSprite(TextureManager.Load(OsuTexture.songselect_mode_arrow), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(arrow_spread, yOffset), 0.45f, true, Color4.DarkGray);
            s_ModeArrowRight.OnHover += delegate { s_ModeArrowRight.ScaleTo(1.2f, 100, EasingTypes.In); };
            s_ModeArrowRight.OnHoverLost += delegate { s_ModeArrowRight.ScaleTo(1f, 100, EasingTypes.In); };
            s_ModeArrowRight.OnClick += onSelectNextMode;

            s_ModeArrowRight.Rotation = 1;
            sprites.Add(s_ModeArrowRight);

            s_ModeDescriptionText = new pText(string.Empty, 30, new Vector2(0, 110), new Vector2(GameBase.BaseSizeFixedWidth.Width, 0), 1, true, Color4.White, true) { Field = FieldTypes.StandardSnapCentre, Origin = OriginTypes.Centre, TextAlignment = TextAlignment.Centre };
            sprites.Add(s_ModeDescriptionText);

            s_TabBarPlay = tabController.Add(OsuTexture.songselect_tab_bar_play, sprites);
        }

        void onModeButtonClick(object sender, EventArgs e)
        {
            pDrawable d = sender as pDrawable;
            if (d == null) return;

            Difficulty newDifficulty;

            if (sender == s_ModeButtonEasy)
                newDifficulty = Difficulty.Easy;
            else if (sender == s_ModeButtonExpert)
                newDifficulty = Difficulty.Expert;
            else
                newDifficulty = Difficulty.Normal;

            if (newDifficulty == Player.Difficulty)
                onStartButtonPressed(sender, e);
            else
            {
                SetDifficulty(newDifficulty);
            }
        }

        private void SetDifficulty(Difficulty newDifficulty)
        {
            if (Player.Difficulty != newDifficulty)
            {
                string versions = Player.Beatmap.Package.GetMetadata(MapMetaType.Version);
                if (versions != null && !versions.Contains(newDifficulty.ToString()))
                    GameBase.Notify("This difficulty has not yet been mapped!");
                else if (Player.Difficulty == Difficulty.Expert && mapRequiresUnlock)
                {
                    GameBase.Notify("Unlock Expert by passing this song on Stream mode first!");


                    //todo: show an alert that this needs an unlock.
                }
                else
                    Player.Difficulty = newDifficulty;
            }
            updateModeSelectionArrows();
        }

        void onSelectPreviousMode(object sender, EventArgs e)
        {
            switch (Player.Difficulty)
            {
                case Difficulty.Normal:
                    SetDifficulty(Difficulty.Easy);
                    break;
                case Difficulty.Expert:
                    SetDifficulty(Difficulty.Normal);
                    break;
            }
        }

        void onSelectNextMode(object sender, EventArgs e)
        {
            switch (Player.Difficulty)
            {
                case Difficulty.Easy:
                    SetDifficulty(Difficulty.Normal);
                    break;
                case Difficulty.Normal:
                    SetDifficulty(Difficulty.Expert);
                    break;
            }
        }

        const float mode_button_width = 300;
        private pTabController tabController;
        private pDrawable s_TabBarPlay;
        private pDrawable s_TabBarRank;
        private pDrawable s_TabBarOther;

        /// <summary>
        /// Updates the states of mode selection arrows depending on the current mode selection.
        /// </summary>
        private void updateModeSelectionArrows()
        {
            bool hasPrevious = false;
            bool hasNext = false;

            string text = null;

            switch (Player.Difficulty)
            {
                case Difficulty.Easy:
                    hasNext = true;
                    difficultySelectOffset = mode_button_width;
                    text = "You can't fail.";
                    break;
                case Difficulty.Normal:
                    hasPrevious = true;
                    hasNext = !mapRequiresUnlock;
                    difficultySelectOffset = 0;
                    text = "Dynamic stream switching!";
                    break;
                case Difficulty.Expert:
                    hasPrevious = true;
                    difficultySelectOffset = -mode_button_width;
                    text = "Not for the faint-hearted!";
                    break;
            }

            s_ModeArrowLeft.Colour = hasPrevious ? Color4.White : Color4.DarkGray;
            s_ModeArrowRight.Colour = hasNext ? Color4.White : Color4.DarkGray;

            if (s_ModeDescriptionText.Text != text)
            {
                pSprite clone = s_ModeDescriptionText.Clone();
                clone.FadeOut(200);
                clone.AlwaysDraw = false;
                spriteManager.Add(clone);

                s_ModeDescriptionText.Text = text;
                s_ModeDescriptionText.Alpha = 0;
                s_ModeDescriptionText.FadeInFromZero(200);
            }
        }

        private void leaveDifficultySelection(object sender, EventArgs args)
        {
            touchingBegun = false;
            velocity = 0;

            State = SelectState.SongSelect;

            InitializeBgm();

            if (SelectedPanel != null)
            {
                SelectedPanel.s_BackingPlate2.FadeColour(Color4.Transparent, 150);
            }

            GameBase.Scheduler.Add(delegate
            {
                foreach (BeatmapPanel p in panels)
                {
                    p.s_BackingPlate.HandleInput = true;

                    foreach (pDrawable d in p.Sprites)
                        d.FadeIn(500);
                }

                if (tabController != null) tabController.Hide();

                s_Header.Transform(new Transformation(s_Header.Position, Vector2.Zero, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
                s_Header.Transform(new Transformation(TransformationType.Rotation, s_Header.Rotation, 0, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));

                s_Footer.Transform(new Transformation(s_Footer.Position, new Vector2(-60, -105), Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
                s_Footer.Transform(new Transformation(TransformationType.Rotation, s_Footer.Rotation, 0.04f, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
            }, true);
        }

        private void onStartButtonPressed(object sender, EventArgs args)
        {
            if (State == SelectState.Starting)
                return;

            AudioEngine.PlaySample(OsuSamples.MenuHit);

            State = SelectState.Starting;

            pDrawable activatedSprite = null;

            switch (Player.Difficulty)
            {
                case Difficulty.Easy:
                    s_ModeButtonStream.FadeOut(200);
                    s_ModeButtonExpert.FadeOut(200);
                    activatedSprite = s_ModeButtonEasy;
                    break;
                case Difficulty.Normal:
                    s_ModeButtonEasy.FadeOut(200);
                    s_ModeButtonExpert.FadeOut(200);
                    activatedSprite = s_ModeButtonStream;
                    break;
                case Difficulty.Expert:
                    s_ModeButtonStream.FadeOut(200);
                    s_ModeButtonEasy.FadeOut(200);
                    activatedSprite = s_ModeButtonExpert;
                    break;
            }

            activatedSprite.AdditiveFlash(800, 0.8f).Transform(new Transformation(TransformationType.Scale, 1, 1.5f, Clock.Time, Clock.Time + 800, EasingTypes.In));

            s_ModeArrowLeft.FadeOut(200);
            s_ModeArrowRight.FadeOut(200);

            s_Footer.AdditiveFlash(500, 0.5f);

            GameBase.Scheduler.Add(delegate
            {
                Director.ChangeMode(OsuMode.Play);
            }, 800);
        }
    }
}
