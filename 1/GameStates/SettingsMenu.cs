using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FontStashSharp;
using System;
using System.IO;

namespace _1
{
    public class SettingsMenu
    {
        private Texture2D pixelTexture;
        private Texture2D backgroundTexture;
        private SpriteFontBase font;

        private Rectangle sliderTrack;
        private Rectangle sliderThumb;
        private float volume = 0.5f;
        private bool isDragging;

        private Rectangle backButton;
        public bool BackPressed { get; private set; }

        private MouseState prevMouseState;

        public SettingsMenu(GraphicsDevice graphicsDevice, Texture2D bgTexture = null)
        {
            pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
            backgroundTexture = bgTexture;

            // Загружаем шрифт
            try
            {
                string fontPath = @"Content\arial.ttf";
                if (!File.Exists(fontPath))
                    fontPath = Path.Combine(AppContext.BaseDirectory, "Content", "arial.ttf");
                
                var fontSystem = new FontSystem();
                fontSystem.AddFont(File.ReadAllBytes(fontPath));
                font = fontSystem.GetFont(24);
            }
            catch
            {
                font = null;
            }

            int centerX = 400;
            int sliderWidth = 300;
            int sliderHeight = 20;
            int sliderY = 230;

            sliderTrack = new Rectangle(centerX - sliderWidth / 2, sliderY, sliderWidth, sliderHeight);
            sliderThumb = new Rectangle(centerX - 10, sliderY - 10, 20, 40);

            backButton = new Rectangle(300, 380, 200, 50);
            BackPressed = false;
        }

        public void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (isDragging || sliderThumb.Contains(mouse.Position))
                {
                    isDragging = true;
                    int newX = MathHelper.Clamp(mouse.X - sliderThumb.Width / 2,
                        sliderTrack.Left - sliderThumb.Width / 2,
                        sliderTrack.Right - sliderThumb.Width / 2);
                    sliderThumb = new Rectangle(newX, sliderThumb.Y, sliderThumb.Width, sliderThumb.Height);

                    float minX = sliderTrack.Left - sliderThumb.Width / 2;
                    float maxX = sliderTrack.Right - sliderThumb.Width / 2;
                    volume = (newX - minX) / (maxX - minX);
                }
            }
            else
            {
                isDragging = false;
            }

            if (mouse.LeftButton == ButtonState.Released && prevMouseState.LeftButton == ButtonState.Pressed)
            {
                if (backButton.Contains(mouse.Position))
                    BackPressed = true;
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

            // Окно настроек
            spriteBatch.Draw(pixelTexture, new Rectangle(150, 100, 500, 320), Color.Black * 0.8f);
            spriteBatch.Draw(pixelTexture, new Rectangle(155, 105, 490, 310), Color.Gray * 0.3f);

            // Заголовок
            if (font != null)
            {
                string title = "Настройки звука";
                var titleSize = font.MeasureString(title);
                spriteBatch.DrawString(font, title, 
                    new Vector2(400 - titleSize.X / 2, 120), Color.White);
            }

            // Дорожка ползунка
            spriteBatch.Draw(pixelTexture, sliderTrack, Color.Gray);
            
            // Активная часть
            float activeWidth = sliderThumb.X - sliderTrack.X + sliderThumb.Width / 2;
            if (activeWidth > 0)
                spriteBatch.Draw(pixelTexture,
                    new Rectangle(sliderTrack.X, sliderTrack.Y, (int)activeWidth, sliderTrack.Height),
                    Color.Green);
            
            // Ручка
            spriteBatch.Draw(pixelTexture, sliderThumb, Color.White);

            // Уровень громкости
            if (font != null)
            {
                int percent = (int)(volume * 100);
                string volumeText = $"{percent}%";
                var volSize = font.MeasureString(volumeText);
                spriteBatch.DrawString(font, volumeText,
                    new Vector2(400 - volSize.X / 2, 270), Color.LimeGreen);
            }

            // Кнопка "На главный экран"
            spriteBatch.Draw(pixelTexture, backButton, Color.DarkGray);
            spriteBatch.Draw(pixelTexture, 
                new Rectangle(backButton.X + 2, backButton.Y + 2, backButton.Width - 4, backButton.Height - 4), 
                Color.Black * 0.5f);
            
            if (font != null)
            {
                string backText = "На главный экран";
                var backSize = font.MeasureString(backText);
                spriteBatch.DrawString(font, backText,
                    new Vector2(400 - backSize.X / 2, 390), Color.White);
            }
        }

        public float GetVolume()
        {
            return volume;
        }
    }
}