﻿using System;
using Microsoft.Xna.Framework;
using GameArchitectureExample.StateManagement;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace GameArchitectureExample.Screens
{
    // The loading screen coordinates transitions between the menu system and the
    // game itself. Normally one screen will transition off at the same time as
    // the next screen is transitioning on, but for larger transitions that can
    // take a longer time to load their data, we want the menu system to be entirely
    // gone before we start loading the game. This is done as follows:
    // 
    // - Tell all the existing screens to transition off.
    // - Activate a loading screen, which will transition on at the same time.
    // - The loading screen watches the state of the previous screens.
    // - When it sees they have finished transitioning off, it activates the real
    //   next screen, which may take a long time to load its data. The loading
    //   screen will be the only thing displayed while this load is taking place.
    public class LoadingScreen : GameScreen
    {
        private SpriteFont font;
        private readonly bool _loadingIsSlow;
        private bool _otherScreensAreGone;
        private readonly GameScreen[] _screensToLoad;
        Texture2D _background;

        // Constructor is private: loading screens should be activated via the static Load method instead.
        private LoadingScreen(ScreenManager screenManager, bool loadingIsSlow, GameScreen[] screensToLoad)
        {
            _loadingIsSlow = loadingIsSlow;
            _screensToLoad = screensToLoad;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
        }

        public override void Activate()
        {
            _background = new ContentManager(ScreenManager.Game.Services, "Content").Load<Texture2D>("splash_sky");
            font = new ContentManager(ScreenManager.Game.Services, "Content").Load<SpriteFont>("bangers");
            base.Activate();
        }

        // Activates the loading screen.
        public static void Load(ScreenManager screenManager, bool loadingIsSlow,
                                PlayerIndex? controllingPlayer, params GameScreen[] screensToLoad)
        {
            // Tell all the current screens to transition off.
            foreach (var screen in screenManager.GetScreens())
                screen.ExitScreen();

            // Create and activate the loading screen.
            var loadingScreen = new LoadingScreen(screenManager, loadingIsSlow, screensToLoad);

            screenManager.AddScreen(loadingScreen, controllingPlayer);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // If all the previous screens have finished transitioning
            // off, it is time to actually perform the load.
            if (_otherScreensAreGone)
            {
                ScreenManager.RemoveScreen(this);

                foreach (var screen in _screensToLoad)
                {
                    if (screen != null)
                        ScreenManager.AddScreen(screen, ControllingPlayer);
                }

                // Once the load has finished, we use ResetElapsedTime to tell
                // the  game timing mechanism that we have just finished a very
                // long frame, and that it should not try to catch up.
                ScreenManager.Game.ResetElapsedTime();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            // If we are the only active screen, that means all the previous screens
            // must have finished transitioning off. We check for this in the Draw
            // method, rather than in Update, because it isn't enough just for the
            // screens to be gone: in order for the transition to look good we must
            // have actually drawn a frame without them before we perform the load.
            if (ScreenState == ScreenState.Active && ScreenManager.GetScreens().Length == 1)
                _otherScreensAreGone = true;

            // The gameplay screen takes a while to load, so we display a loading
            // message while that is going on, but the menus load very quickly, and
            // it would look silly if we flashed this up for just a fraction of a
            // second while returning from the game to the menus. This parameter
            // tells us how long the loading is going to take, so we know whether
            // to bother drawing the message.
            if (_loadingIsSlow)
            {
                var spriteBatch = ScreenManager.SpriteBatch;
                const string message = "Loading...";

                // Center the text in the viewport.
                var viewport = ScreenManager.GraphicsDevice.Viewport;
                var viewportSize = new Vector2(viewport.Width, viewport.Height);
                var textSize = font.MeasureString(message);
                var textPosition = (viewportSize - textSize) / 2;

                var color = Color.Black * TransitionAlpha;

                // Draw the text.
                spriteBatch.Begin();
                ScreenManager.SpriteBatch.Draw(_background, Vector2.Zero, Color.White * TransitionAlpha);
                spriteBatch.DrawString(font, message, textPosition, color);
                spriteBatch.End();
            }
        }
    }
}
