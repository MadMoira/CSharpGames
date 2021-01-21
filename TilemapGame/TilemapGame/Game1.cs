using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TilemapGame
{
    public enum Direction
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3,
    }
    
    interface IBlock
    {
        public void DrawSelf(SpriteBatch sb, Texture2D tiles, Vector2 pos, Vector2 size);
        public bool Push(int from);
    }
    
    public class block_solid : IBlock
    {
        public void DrawSelf(SpriteBatch sb, Texture2D tiles, Vector2 pos, Vector2 size)
        {
            sb.Draw(tiles, pos*size, new Rectangle(0, 0, (int) size.X, (int) size.Y), Color.White);
        }

        public bool Push(int from)
        {
            return false;
        }
    }

    public class block_player : IBlock
    {
        public void DrawSelf(SpriteBatch sb, Texture2D tiles, Vector2 pos, Vector2 size)
        {
            sb.Draw(tiles, pos*size, new Rectangle(32, 0, (int) size.X, (int) size.Y), Color.White);
        }

        public bool Push(int from)
        {
            return true;
        }
    }

    public class block_simple : IBlock
    {
        public void DrawSelf(SpriteBatch sb, Texture2D tiles, Vector2 pos, Vector2 size)
        {
            sb.Draw(tiles, pos*size, new Rectangle(64, 0, (int) size.X, (int) size.Y), Color.White);
        }

        public bool Push(int from)
        {
            return true;
        }
    }
    
    public class block_horizontal : IBlock
    {
        public void DrawSelf(SpriteBatch sb, Texture2D tiles, Vector2 pos, Vector2 size)
        {
            sb.Draw(tiles, pos*size, new Rectangle(64, 0, (int) size.X, (int) size.Y), Color.White);
        }

        public bool Push(int from)
        {
            return @from == (int)Direction.East || @from == (int)Direction.West;
        }
    }
    
    public class block_vertical : IBlock
    {
        public void DrawSelf(SpriteBatch sb, Texture2D tiles, Vector2 pos, Vector2 size)
        {
            sb.Draw(tiles, pos*size, new Rectangle(64, 0, (int) size.X, (int) size.Y), Color.White);
        }

        public bool Push(int from)
        {
            return @from == (int)Direction.North || @from == (int)Direction.South;
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
            #........++....#
            #..............#
            #........#.....#
            #...|....#.....#
            #..............#
            #....P.........#
            #...-.....-....#
            #..............#
            ################
        ";

        private readonly List<IBlock> lLevel = new List<IBlock>();
        private readonly Vector2 vLevelSize = new Vector2(16, 15);
        private readonly Vector2 vTileSize = new Vector2(32, 32);
        private Vector2 vPlayer;
        private double lastAction;
        private const double timeBetweenActions = 150;

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
                            vPlayer = new Vector2(x, y);
                            break;
                        
                        case '+':
                            lLevel.Add(new block_simple());
                            break;
                        
                        case '-':
                            lLevel.Add(new block_horizontal());
                            break;
                        
                        case '|':
                            lLevel.Add(new block_vertical());
                            break;
                        
                        default:
                            lLevel.Add(null);
                            break;
                    }
                }
            }
        }

        public static void Swap<T>(IList<T> list, int indexA, int indexB)
        {
            var tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }
        
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferHeight = 480;
            _graphics.PreferredBackBufferWidth = 512;
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
            int Id(Vector2 pos) => (int) (pos.Y * vLevelSize.X + pos.X);
            
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var bPushing = false;
            Direction dirPush = Direction.North;
            
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                dirPush = Direction.North;
                bPushing = true;
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                dirPush = Direction.South;
                bPushing = true;
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                dirPush = Direction.West;
                bPushing = true;
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                dirPush = Direction.East;
                bPushing = true;
            }

            if (bPushing && gameTime.TotalGameTime.TotalMilliseconds > lastAction + timeBetweenActions)
            {
                var vBlock = vPlayer;

                var bAllowPush = false;
                var bTest = true;

                while (bTest)
                {
                    if (lLevel[Id(vBlock)] != null)
                    {
                        if (lLevel[Id(vBlock)].Push(((int) dirPush + 2) % 4))
                        {
                            switch (dirPush)
                            {
                                case Direction.North: vBlock.Y--; break;
                                case Direction.South: vBlock.Y++; break;
                                case Direction.East: vBlock.X++; break;
                                case Direction.West: vBlock.X--; break;
                            }
                        }
                        else
                        {
                            bTest = false;
                        }
                    }
                    else
                    {
                        bAllowPush = true;
                        bTest = false;
                    }
                }

                if (bAllowPush)
                {
                    lastAction = gameTime.TotalGameTime.TotalMilliseconds;
                    while (vBlock != vPlayer)
                    {
                        var vSource = vBlock;
                        switch (dirPush)
                        {
                            case Direction.North: vSource.Y++; break;
                            case Direction.South: vSource.Y--; break;
                            case Direction.East: vSource.X--; break;
                            case Direction.West: vSource.X++; break;
                        }
                        Swap(lLevel, Id(vSource), Id(vBlock));
                        vBlock = vSource;
                    }
                    
                    switch (dirPush)
                    {
                        case Direction.North: vPlayer.Y--; break;
                        case Direction.South: vPlayer.Y++; break;
                        case Direction.East: vPlayer.X++; break;
                        case Direction.West: vPlayer.X--; break;
                    }
                }
            }
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