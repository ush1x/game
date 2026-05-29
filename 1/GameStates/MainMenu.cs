using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FontStashSharp;
using System;
using System.IO;

namespace _1
{
    public class MainMenu
    {
        private Texture2D pixelTexture;
        private Texture2D backgroundTexture;
        private SpriteFontBase font;
        private SpriteFontBase titleFont;

        private Rectangle playButton;
        private Rectangle settingsButton;
        private Rectangle exitButton;

        private string playText = "ИГРАТЬ";
        private string settingsText = "НАСТРОЙКИ";
        private string exitText = "ВЫХОД";
        private string titleLine1 = "ПРОЕКТ";
        private string titleLine2 = "РАЗГРОМ";

        public enum MenuAction { None, Play, Levels, Settings, Exit }
        public MenuAction SelectedAction { get; private set; }

        private MouseState prevMouseState;

        public MainMenu(GraphicsDevice graphicsDevice, Texture2D bgTexture = null)
        {
            pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
            
            backgroundTexture = bgTexture;

            // Загружаем шрифты
            try
            {
                string fontPath = @"Content\arial.ttf";
                if (!File.Exists(fontPath))
                    fontPath = Path.Combine(AppContext.BaseDirectory, "Content", "arial.ttf");
                
                var fontSystem = new FontSystem();
                fontSystem.AddFont(File.ReadAllBytes(fontPath));
                font = fontSystem.GetFont(24);
                titleFont = fontSystem.GetFont(56);
            }
            catch
            {
                font = null;
                titleFont = null;
            }

            int centerX = 400;
            int buttonWidth = 250;
            int buttonHeight = 60;
            int startY = 240;
            int spacing = 80;

            playButton = new Rectangle(centerX - buttonWidth / 2, startY, buttonWidth, buttonHeight);
            settingsButton = new Rectangle(centerX - buttonWidth / 2, startY + spacing, buttonWidth, buttonHeight);
            exitButton = new Rectangle(centerX - buttonWidth / 2, startY + spacing * 2, buttonWidth, buttonHeight);

            SelectedAction = MenuAction.None;
        }

        public void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();
            SelectedAction = MenuAction.None;

            if (mouse.LeftButton == ButtonState.Released && prevMouseState.LeftButton == ButtonState.Pressed)
            {
                if (playButton.Contains(mouse.Position))
                    SelectedAction = MenuAction.Play;
                else if (settingsButton.Contains(mouse.Position))
                    SelectedAction = MenuAction.Settings;
                else if (exitButton.Contains(mouse.Position))
                    SelectedAction = MenuAction.Exit;
            }
            prevMouseState = mouse;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Фон
            if (backgroundTexture != null)
                spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, 800, 480), Color.White);
            else
                spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, 800, 480), Color.DarkSlateGray);

            // Полупрозрачная панель сверху для названия
            spriteBatch.Draw(pixelTexture, new Rectangle(150, 40, 500, 130), Color.Black * 0.7f);
            spriteBatch.Draw(pixelTexture, new Rectangle(155, 45, 490, 120), Color.Red * 0.3f);

            // Название игры
            if (titleFont != null)
            {
                var size1 = titleFont.MeasureString(titleLine1);
                var size2 = titleFont.MeasureString(titleLine2);
                
                spriteBatch.DrawString(titleFont, titleLine1, 
                    new Vector2(400 - size1.X / 2, 55), Color.White);
                spriteBatch.DrawString(titleFont, titleLine2, 
                    new Vector2(400 - size2.X / 2, 105), Color.Red);
            }

            // Полупрозрачная панель под кнопками
            spriteBatch.Draw(pixelTexture, new Rectangle(230, 210, 340, 280), Color.Black * 0.6f);

            // Кнопки
            spriteBatch.Draw(pixelTexture, playButton, Color.Green);
            spriteBatch.Draw(pixelTexture, settingsButton, Color.Orange);
            spriteBatch.Draw(pixelTexture, exitButton, Color.DarkRed);

            // Текст на кнопках
            if (font != null)
            {
                DrawButtonText(spriteBatch, playButton, playText);
                DrawButtonText(spriteBatch, settingsButton, settingsText);
                DrawButtonText(spriteBatch, exitButton, exitText);
            }
        }

        private void DrawButtonText(SpriteBatch spriteBatch, Rectangle button, string text)
        {
            var size = font.MeasureString(text);
            float x = button.X + button.Width / 2 - size.X / 2;
            float y = button.Y + button.Height / 2 - size.Y / 2;
            spriteBatch.DrawString(font, text, new Vector2(x, y), Color.White);
        }
    }
}