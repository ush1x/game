using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace _1
{
    public enum EnemyType { Turret, Chaser }

    public class Enemy
    {
        public Rectangle Bounds { get; private set; }
        public EnemyType Type { get; private set; }

        private Texture2D[] textures;
        private Texture2D bulletTexture;
        private int currentFrame;
        private float animationTimer;
        private float animationInterval = 0.2f;

        // Для Chaser
        private float speed;

        // Для Turret
        private float shootTimer;
        private float shootInterval;
        public Rectangle BulletBounds { get; private set; }
        public bool HasBullet { get; private set; }
        private Vector2 bulletVelocity;
        private float bulletSpeed = 225f;

        public Enemy(GraphicsDevice graphicsDevice, Vector2 position, int width, int height, 
                     EnemyType type, float shootIntervalSeconds = 1f, float moveSpeed = 100f,
                     Texture2D[] enemyTextures = null)
        {
            textures = enemyTextures;
            
            bulletTexture = new Texture2D(graphicsDevice, 1, 1);
            bulletTexture.SetData(new[] { Color.Yellow });

            Bounds = new Rectangle((int)position.X, (int)position.Y, width, height);
            Type = type;
            speed = moveSpeed;
            shootInterval = shootIntervalSeconds;
            shootTimer = 0f;
            HasBullet = false;
        }

        public void Update(GameTime gameTime, Vector2 playerPosition, List<Rectangle> platforms)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Анимация
            if (textures != null && textures.Length > 1)
            {
                animationTimer += deltaTime;
                if (animationTimer >= animationInterval)
                {
                    animationTimer = 0f;
                    currentFrame = (currentFrame + 1) % textures.Length;
                }
            }

            if (Type == EnemyType.Chaser)
            {
                float direction = playerPosition.X > Bounds.Center.X ? 1f : -1f;

                int newX = Bounds.X + (int)(direction * speed * deltaTime);
                Rectangle newBounds = new Rectangle(newX, Bounds.Y, Bounds.Width, Bounds.Height);

                bool blocked = false;
                foreach (var platform in platforms)
                {
                    if (newBounds.Intersects(platform))
                    {
                        blocked = true;
                        break;
                    }
                }

                if (!blocked)
                    Bounds = newBounds;
            }
            else if (Type == EnemyType.Turret)
            {
                shootTimer += deltaTime;

                if (shootTimer >= shootInterval && !HasBullet)
                {
                    shootTimer = 0f;
                    HasBullet = true;

                    BulletBounds = new Rectangle(
                        Bounds.Center.X - 5,
                        Bounds.Center.Y - 4,
                        10, 8
                    );

                    Vector2 direction = playerPosition - new Vector2(Bounds.Center.X, Bounds.Center.Y);
                    if (direction != Vector2.Zero)
                        direction.Normalize();
                    bulletVelocity = direction * bulletSpeed;
                }

                if (HasBullet)
                {
                    Vector2 bulletCenter = new Vector2(BulletBounds.Center.X, BulletBounds.Center.Y);
                    Vector2 toPlayer = playerPosition - bulletCenter;
                    
                    if (toPlayer != Vector2.Zero)
                    {
                        toPlayer.Normalize();
                        bulletVelocity = Vector2.Lerp(bulletVelocity, toPlayer * bulletSpeed, 3f * deltaTime);
                    }

                    int newX = BulletBounds.X + (int)(bulletVelocity.X * deltaTime);
                    int newY = BulletBounds.Y + (int)(bulletVelocity.Y * deltaTime);
                    
                    BulletBounds = new Rectangle(newX, newY, BulletBounds.Width, BulletBounds.Height);

                    if (BulletBounds.X < -50 || BulletBounds.X > 850 || 
                        BulletBounds.Y < -50 || BulletBounds.Y > 500)
                        HasBullet = false;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D tex = (textures != null && textures.Length > 0) 
                ? textures[currentFrame] : null;
                
            if (tex != null)
                spriteBatch.Draw(tex, Bounds, Color.White);
            else
                spriteBatch.Draw(bulletTexture, Bounds, 
                    Type == EnemyType.Turret ? Color.Orange : Color.Red);

            if (HasBullet)
            {
                spriteBatch.Draw(bulletTexture, BulletBounds, Color.Yellow);
            }
        }
    }
}