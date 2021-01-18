using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TilemapGame
{
    public interface IBlock
    {
        void DrawSelf(SpriteBatch sb, Texture2D tiles, Vector2 pos, Vector2 size);
    }
    
    public class block_solid : IBlock
    {
        public void DrawSelf(SpriteBatch sb, Texture2D tiles, Vector2 pos, Vector2 size)
        {
            sb.Draw(tiles, pos*size, new Rectangle(0, 0, (int) size.X, (int) size.Y), Color.White);
        }
    }

    public class block_player : IBlock
    {
        public void DrawSelf(SpriteBatch sb, Texture2D tiles, Vector2 pos, Vector2 size)
        {
            sb.Draw(tiles, pos*size, new Rectangle(32, 0, (int) size.X, (int) size.Y), Color.White);
        }
    }

    public class block_simple : IBlock
    {
        public void DrawSelf(SpriteBatch sb, Texture2D tiles, Vector2 pos, Vector2 size)
        {
            sb.Draw(tiles, pos*size, new Rectangle(64, 0, (int) size.X, (int) size.Y), Color.White);
        }
    }
    
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D tilemap;

        private const string sLevel = @"
            ################
            #..............#
            #..............#
            #...####.......#
            #...#..........#
            #...#..........#
            #........+.....#
            #..............#
            #........#.....#
            #........#.....#
            #..............#
            #....P.........#
            #..............#
            #..............#
            ################
        ";

        private List<IBlock> lLevel = new List<IBlock>();
        private readonly Vector2 vLevelSize = new Vector2(16, 15);
        private Vector2 vTileSize = new Vector2(16, 16);

        private void LoadLevel(int n)
        {
            var cleaned = sLevel.Replace("\n", "").Replace("\r", "").Replace(" ", "");
            
            for (var y = 0; y < vLevelSize.Y; y++)
            {
                for (var x = 0; x < vLevelSize.X; x++)
                {
                    switch (cleaned[(int) (y*vLevelSize.X + x)])
                    {
                        case '#':
                            lLevel.Add(new block_solid());
                            break;
                        
                        case 'P':
                            lLevel.Add(new block_player());
                            break;
                        
                        case '+':
                            lLevel.Add(new block_simple());
                            break;
                        
                        default:
                            lLevel.Add(null);
                            break;
                    }
                }
            }
        }
        
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferHeight = 460;
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.ApplyChanges();
            
            LoadLevel(0);
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            tilemap = Content.Load<Texture2D>("tiles_plain");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            int Id(Vector2 pos) => (int) (pos.Y * vLevelSize.X + pos.X);

            GraphicsDevice.Clear(Color.White);
            _spriteBatch.Begin();
            var vTilePos = new Vector2(0, 0);
            for (vTilePos.Y = 0; vTilePos.Y < vLevelSize.Y; vTilePos.Y++)
            {
                for (vTilePos.X = 0; vTilePos.X < vLevelSize.X; vTilePos.X++)
                {
                    var b = lLevel[Id(vTilePos)];
                    b?.DrawSelf(_spriteBatch, tilemap, vTilePos, vTileSize);
                }
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}