using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Speller
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Keyboard Declarations
        KeyboardState prevKeyState, currKeyState;

        // Main Game Declarations
        SpriteFont letterFont;
        Texture2D playerSquare;

        Vector2 playerPosition;
        Vector2 moveDirection;
        int playerScore;

        Random rand = new Random();

        string currentWord = "NONE";
        int currentLetterIndex = 99;

        class GameLetter
        {
            public string Letter;
            public Vector2 Position;
            public bool WasHit;
        };

        List<GameLetter> letters = new List<GameLetter>();
        float playerSpeed = 150.0f;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            playerScore = 0;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            letterFont = Content.Load<SpriteFont>("Segoe14");
            playerSquare = Content.Load<Texture2D>("square");

            CheckForNewWord();
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            Vector2 moveDir = Vector2.Zero;
            currKeyState = Keyboard.GetState();

            // --------------------------------------------------------------------------------------------------


            if (prevKeyState.IsKeyDown(Keys.Escape) && currKeyState.IsKeyUp(Keys.Escape))
                this.Exit();

            if (currKeyState.IsKeyDown(Keys.Up))
                moveDir += new Vector2(0, -1);

            if (currKeyState.IsKeyDown(Keys.Down))
                moveDir += new Vector2(0, 1);

            if (currKeyState.IsKeyDown(Keys.Left))
                moveDir += new Vector2(-1, 0);

            if (currKeyState.IsKeyDown(Keys.Right))
                moveDir += new Vector2(1, 0);

            if (moveDir != Vector2.Zero)
            {
                moveDir.Normalize();
                moveDirection = moveDir;
            }

            playerPosition += (moveDirection * playerSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds);

            playerPosition = new Vector2(MathHelper.Clamp(playerPosition.X, 0, this.Window.ClientBounds.Width - 16),
                                         MathHelper.Clamp(playerPosition.Y, 0, this.Window.ClientBounds.Height - 16));

            prevKeyState = currKeyState;


            // --------------------------------------------------------------------------------------------------


            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            CheckCollisions();
            CheckForNewWord();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            spriteBatch.Draw(playerSquare, playerPosition, Color.White);

            foreach (GameLetter letter in letters)
            {
                Color letterColor = Color.White;

                if (letter.WasHit)
                    letterColor = Color.Red;

                spriteBatch.DrawString(
                    letterFont,
                    letter.Letter,
                    letter.Position,
                    letterColor);
            }

            spriteBatch.DrawString(
                letterFont,
                "Spell: ",
                new Vector2(
                    this.Window.ClientBounds.Width / 2 - 100,
                    this.Window.ClientBounds.Height - 25),
                Color.White);

            string beforeWord = currentWord.Substring(0, currentLetterIndex);
            string currentLetter = currentWord.Substring(currentLetterIndex, 1);
            string afterWord = "";

            if (currentWord.Length > currentLetterIndex)
                afterWord = currentWord.Substring(currentLetterIndex + 1);

            spriteBatch.DrawString(
                letterFont,
                beforeWord,
                new Vector2(
                    this.Window.ClientBounds.Width / 2,
                    this.Window.ClientBounds.Height - 25),
                Color.Green);

            spriteBatch.DrawString(
                letterFont,
                currentLetter,
                new Vector2(
                    this.Window.ClientBounds.Width / 2 +
                        letterFont.MeasureString(beforeWord).X,
                    this.Window.ClientBounds.Height - 25),
                Color.Yellow);

            spriteBatch.DrawString(
                letterFont,
                afterWord,
                new Vector2(
                    this.Window.ClientBounds.Width / 2 +
                        letterFont.MeasureString(beforeWord + currentLetterIndex).X,
                    this.Window.ClientBounds.Height - 25),
                Color.LightBlue);

            spriteBatch.DrawString(
                letterFont,
                "Score: " + playerScore.ToString(),
                Vector2.Zero,
                Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private string PickAWord()
        {
            switch (rand.Next(15))
            {
                case 0: return "CAT";
                case 1: return "DOG";
                case 2: return "MILK";
                case 3: return "SUN";
                case 4: return "SKY";
                case 5: return "RAIN";
                case 6: return "SNOW";
                case 7: return "FAR";
                case 8: return "NEAR";
                case 9: return "FRIEND";
                case 10: return "GAME";
                case 11: return "XNA";
                case 12: return "PLAY";
                case 13: return "RUN";
                case 14: return "FUN";
            }

            return "BUG";
        }

        private void FillLetters(string word)
        {
            Rectangle safeArea = new Rectangle(
                this.Window.ClientBounds.Width / 2 - playerSquare.Width,
                this.Window.ClientBounds.Height / 2 - playerSquare.Height,
                playerSquare.Width * 2,
                playerSquare.Height * 2);

            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            List<Vector2> locations = new List<Vector2>();

            for (int x = 25;
                x < this.Window.ClientBounds.Width - 50;
                x += 50)
            {
                for (int y = 25;
                    y < this.Window.ClientBounds.Height - 50;
                    y += 50)
                {
                    Rectangle locationRect = new Rectangle(
                        x,
                        y,
                        (int)letterFont.MeasureString("W").X,
                        (int)letterFont.MeasureString("W").Y);

                    if (!safeArea.Intersects(locationRect))
                        locations.Add(new Vector2(x, y));
                }
            }

            letters.Clear();
            for (int x = 0; x < 20; x++)
            {
                GameLetter thisLetter = new GameLetter();

                if (x < word.Length)
                    thisLetter.Letter = word.Substring(x, 1);
                else
                    thisLetter.Letter = alphabet.Substring(
                        rand.Next(0, 26), 1);

                int location = rand.Next(0, locations.Count);
                thisLetter.Position = locations[location];
                thisLetter.WasHit = false;
                locations.RemoveAt(location);

                letters.Add(thisLetter);
            }
        }

        private void CheckForNewWord()
        {
            if (currentLetterIndex >= currentWord.Length)
            {
                playerPosition = new Vector2(
                    this.Window.ClientBounds.Width / 2,
                    this.Window.ClientBounds.Height / 2);
                currentWord = PickAWord();
                currentLetterIndex = 0;
                FillLetters(currentWord);
            }
        }

        private void CheckCollisions()
        {
            for (int x = letters.Count - 1; x >= 0; x--)
            {
                if (new Rectangle(
                    (int)letters[x].Position.X,
                    (int)letters[x].Position.Y,
                    (int)letterFont.MeasureString(
                        letters[x].Letter).X,
                    (int)letterFont.MeasureString(
                        letters[x].Letter).Y).Intersects(
                            new Rectangle(
                                (int)playerPosition.X,
                                (int)playerPosition.Y,
                                playerSquare.Width,
                                playerSquare.Height)))
                {
                    if (letters[x].Letter == currentWord.Substring(
                        currentLetterIndex, 1))
                    {
                        if (playerSpeed < 210)
                            playerSpeed += 10.0f;
                        playerScore += 1;
                        letters.RemoveAt(x);
                        currentLetterIndex++;
                    }
                    else
                    {
                        if (!letters[x].WasHit)
                        {
                            if (playerSpeed > 150.0f)
                                playerSpeed -= 10.0f;
                            else if (playerSpeed <= 150.0f)
                                playerSpeed = 150.0f;
                            playerScore -= 1;
                            letters[x].WasHit = true;
                        }
                    }
                }
                else
                {
                    letters[x].WasHit = false;
                }
            }
        }
    }
}
