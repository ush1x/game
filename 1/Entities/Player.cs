using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace _1
{
    public class Player
    {
        public Vector2 Position;
        private Vector2 velocity;
        private float moveSpeed = 200f;
        private float jumpSpeed = -400f;
        private float gravity = 900f;
        private int width;
        private int height;
        private Texture2D[] walkFrames;
        private int currentFrame;
        private float animationTimer;
        private float animationInterval = 0.2f;
        private bool isMoving;
        private KeyboardState prevKeyboardState;

        public Player(GraphicsDevice graphicsDevice, Texture2D[] textures)
        {
            walkFrames = textures;
            currentFrame = 0;
            width = 50;
            height = 65;
            Position = new Vector2(100, 300);
        }

        public Rectangle Bounds
        {
            get { return new Rectangle((int)Position.X, (int)Position.Y, width, height); }
        }

        public void Update(GameTime gameTime, List<Rectangle> platforms)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState keyboard = Keyboard.GetState();

            // Движение по горизонтали
            float moveInput = 0f;
            if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
                moveInput = -1f;
            if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
                moveInput = 1f;

            velocity.X = moveInput * moveSpeed;
            isMoving = moveInput != 0f;

            // Анимация
            if (isMoving)
            {
                animationTimer += deltaTime;
                if (animationTimer >= animationInterval)
                {
                    animationTimer = 0f;
                    currentFrame = (currentFrame + 1) % walkFrames.Length;
                }
            }
            else
            {
                currentFrame = 0;
                animationTimer = 0f;
            }

            // Прыжок
            bool jumpPressed = keyboard.IsKeyDown(Keys.Space) ||
                               keyboard.IsKeyDown(Keys.W) ||
                               keyboard.IsKeyDown(Keys.Up);
            bool wasJumpPressed = prevKeyboardState.IsKeyDown(Keys.Space) ||
                                  prevKeyboardState.IsKeyDown(Keys.W) ||
                                  prevKeyboardState.IsKeyDown(Keys.Up);

            bool isOnGround = false;
            Rectangle nextBounds = new Rectangle(
                (int)Position.X,
                (int)(Position.Y + 1),
                width, height);

            foreach (var platform in platforms)
            {
                if (nextBounds.Intersects(platform))
                {
                    isOnGround = true;
                    break;
                }
            }

            if (jumpPressed && !wasJumpPressed && isOnGround)
            {
                velocity.Y = jumpSpeed;
            }

            // Гравитация
            velocity.Y += gravity * deltaTime;

            // Движение по X
            Position = new Vector2(Position.X + velocity.X * deltaTime, Position.Y);

            // Столкновение по X
            foreach (var platform in platforms)
            {
                if (Bounds.Intersects(platform))
                {
                    if (velocity.X > 0)
                        Position = new Vector2(platform.Left - width, Position.Y);
                    else if (velocity.X < 0)
                        Position = new Vector2(platform.Right, Position.Y);
                    velocity.X = 0;
                }
            }

            // Движение по Y
            Position = new Vector2(Position.X, Position.Y + velocity.Y * deltaTime);

            // Столкновение по Y
            foreach (var platform in platforms)
            {
                if (Bounds.Intersects(platform))
                {
                    if (velocity.Y > 0)
                    {
                        Position = new Vector2(Position.X, platform.Top - height);
                        velocity.Y = 0;
                    }
                    else if (velocity.Y < 0)
                    {
                        Position = new Vector2(Position.X, platform.Bottom);
                        velocity.Y = 0;
                    }
                }
            }

            // Не даём вылететь за левый край
            if (Position.X < 0)
                Position = new Vector2(0, Position.Y);

            prevKeyboardState = keyboard;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(walkFrames[currentFrame], Bounds, Color.White);
        }
    }
}