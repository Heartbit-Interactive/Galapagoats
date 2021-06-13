using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GalapaGoats
{

    public class InputHelper
    {
        private List<GamePadState> _currentGamePadState;
        private List<GamePadState> _lastGamePadState;
                

#if WINDOWS
        public static int keyboard_index = 0;
        public Vector2 keyboard_player_pos;
        public bool keyboard_split_mode = false;
#endif
        /// <summary>
        ///   Constructs a new input state.
        /// </summary>
        public InputHelper()
        {
            _currentGamePadState = new List<GamePadState>();
            _lastGamePadState = new List<GamePadState>();
            
            for (int i = 0; i < 4; i++)
            {
                _lastGamePadState.Add(new GamePadState());
                _currentGamePadState.Add(new GamePadState());
            }
        }
        public int selected_index = 0;
        public GamePadState GamePadState
        {
            get {
#if WINDOWS
                if (selected_index == keyboard_index)
                    return _currentKeyPadState;
                else
#endif
                    return _currentGamePadState[selected_index]; 
            
            }
        }



        public GamePadState PreviousGamePadState
        {
            get
            {
#if WINDOWS
                if (selected_index == keyboard_index)
                    return _lastKeyPadState;
                else
#endif
                return _lastGamePadState[selected_index]; 
            }
        }
        
        int check_connected_timeout = 90;
        int check_connected_timer = 90;
        public bool[] Connected_Pads = new bool[] { true, false, false, false };

        /// <summary>
        ///   Reads the latest state of the keyboard and gamepad and mouse/touchpad.
        /// </summary>
        public void Update(GameTime gameTime)
        {

            check_connected_timer--;
            if (check_connected_timer <= 0)
            {
#if WINDOWS
                keyboard_index = 0;
#endif
                check_connected_timer = check_connected_timeout;
                for (int i = 0; i < 4; i++)
                {
                    Connected_Pads[i] = GamePad.GetState((PlayerIndex)i).IsConnected;
#if WINDOWS
                    if(Connected_Pads[i] && keyboard_index == i)
                        keyboard_index++;
#endif
                }
#if WINDOWS
                if(keyboard_index<4)
                    Connected_Pads[keyboard_index] = true;
#endif
            }
            for (int i = 0; i < 4; i++)
            {
                if (Connected_Pads[i])
                {
                    _lastGamePadState[i] = _currentGamePadState[i];
                    _currentGamePadState[i] = GamePad.GetState((PlayerIndex)i);
                }                
            }
            
#if WINDOWS
            update_keyboard_gamepad();
#endif
        }
#if WINDOWS
        private GamePadState _currentKeyPadState=new GamePadState();
        private GamePadState _lastKeyPadState=new GamePadState();
        private KeyboardState _currentKeyboarsState=new KeyboardState();
        private KeyboardState _lastKeyboarsState;
        public static Vector2 mouse_pos;

        private void update_keyboard_gamepad()
        {
            _lastKeyPadState = _currentKeyPadState;
            _lastKeyboarsState=_currentKeyboarsState;
            _currentKeyboarsState=Keyboard.GetState();
            _currentKeyPadState = get_virtual_state(_currentKeyboarsState);
        }

        private GamePadState get_virtual_state(KeyboardState keyboardState)
        {
            if (!keyboard_split_mode)
            {
                var mouse = Mouse.GetState();
                mouse_pos.X = mouse.X; mouse_pos.Y = mouse.Y; 
                
                Vector2 left_stick =new Vector2(keyboardState.IsKeyDown(Keys.A) ? -1 : keyboardState.IsKeyDown(Keys.D) ? 1 : 0,
                    keyboardState.IsKeyDown(Keys.S) ? -1 : keyboardState.IsKeyDown(Keys.W) ? 1 : 0);

                Vector2 right_stick = Vector2.Normalize(new Vector2(mouse.X, mouse.Y) - keyboard_player_pos * 16);


                right_stick.Y *= -1;
                float right_trigger = 0;

                List<Buttons> buttons = new List<Buttons>(12);
                if (keyboardState.IsKeyDown(Keys.Enter))
                    buttons.Add(Buttons.Start);
                if (keyboardState.IsKeyDown(Keys.Escape))
                    buttons.Add(Buttons.Back);
                if (keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.K))
                    buttons.Add(Buttons.A);
                //if (keyboardState.IsKeyDown(Keys.E))
                //    buttons.Add(Buttons.Y);
                //if (mouse.LeftButton == ButtonState.Pressed)
                //{
                //    right_trigger = 1;
                //    buttons.Add(Buttons.RightTrigger);
                //}
                //if (mouse.RightButton == ButtonState.Pressed)
                //{
                //    buttons.Add(Buttons.RightShoulder);
                //    buttons.Add(Buttons.B);
                //}

                if (keyboardState.IsKeyDown(Keys.LeftShift))
                    buttons.Add(Buttons.B);
                if (keyboardState.IsKeyDown(Keys.E))
                    buttons.Add(Buttons.Y);
                if (keyboardState.IsKeyDown(Keys.P))
                    buttons.Add(Buttons.X);

                return new GamePadState(left_stick, right_stick, 0, right_trigger, buttons.ToArray());
            }
            else
            {
                Vector2 left_stick = new Vector2(keyboardState.IsKeyDown(Keys.W) ? 1 : keyboardState.IsKeyDown(Keys.S) ? -1 : 0,
                    keyboardState.IsKeyDown(Keys.D) ? -1 : keyboardState.IsKeyDown(Keys.A) ? 1 : 0);

                Vector2 right_stick = new Vector2(keyboardState.IsKeyDown(Keys.Down) ? 1 : keyboardState.IsKeyDown(Keys.Up) ? -1 : 0,
                    keyboardState.IsKeyDown(Keys.Left) ? -1 : keyboardState.IsKeyDown(Keys.Right) ? 1 : 0);
                
                List<Buttons> buttons = new List<Buttons>(12);

                if (keyboardState.IsKeyDown(Keys.Enter))
                    buttons.Add(Buttons.Start);
                if (keyboardState.IsKeyDown(Keys.Escape))
                    buttons.Add(Buttons.Back);

                //left playa
                if (keyboardState.IsKeyDown(Keys.F))
                    buttons.Add(Buttons.DPadRight);
                if (keyboardState.IsKeyDown(Keys.B))
                    buttons.Add(Buttons.DPadDown);
                if (keyboardState.IsKeyDown(Keys.C))
                    buttons.Add(Buttons.DPadUp);
                if (keyboardState.IsKeyDown(Keys.V))
                    buttons.Add(Buttons.DPadLeft);


                //right playa
                if (keyboardState.IsKeyDown(Keys.K))
                    buttons.Add(Buttons.A);
                if (keyboardState.IsKeyDown(Keys.L))
                    buttons.Add(Buttons.B);
                if (keyboardState.IsKeyDown(Keys.P))
                    buttons.Add(Buttons.Y);
                if (keyboardState.IsKeyDown(Keys.O))
                    buttons.Add(Buttons.X);

                return new GamePadState(left_stick, right_stick, 0, 0, buttons.ToArray());
            }
        }
        public static void Fix_Mouse()
        {            
            mouse_pos.X=Mathf.Clamp(mouse_pos.X, 0, 1280);
            mouse_pos.Y=Mathf.Clamp(mouse_pos.Y, 0, 720);

            Mouse.SetPosition((int)mouse_pos.X, (int)mouse_pos.Y);
        }
		
		        internal bool IsNewKeyPress(Keys keys)
        {
            return _currentKeyboarsState.IsKeyDown(keys) && _lastKeyboarsState.IsKeyUp(keys);
        }
        internal bool IsKeyPress(Keys keys)
        {
            return _currentKeyboarsState.IsKeyDown(keys);
        }
#endif
        /// <summary>
        ///   Helper for checking if a button was newly pressed during this update.
        /// </summary>
        public bool IsNewButtonPress(Buttons button)
        {
            return (GamePadState.IsButtonDown(button) &&
                    PreviousGamePadState.IsButtonUp(button));
        }

        public bool IsNewButtonRelease(Buttons button)
        {
            return (PreviousGamePadState.IsButtonDown(button) &&
                    GamePadState.IsButtonUp(button));
        }

        public bool IsButtonPress(Buttons button)
        {
            return (GamePadState.IsButtonDown(button));
        }

        /// <summary>
        ///   Checks for a "menu select" input action.
        /// </summary>
        public bool IsMenuSelect()
        {
            return IsNewButtonPress(Buttons.A) ||
                   IsNewButtonPress(Buttons.Start) ;
        }

        public bool IsMenuPressed()
        {
            return GamePadState.IsButtonDown(Buttons.A) ||
                   GamePadState.IsButtonDown(Buttons.Start);
        }

        public bool IsMenuReleased()
        {
            return IsNewButtonRelease(Buttons.A) ||
                   IsNewButtonRelease(Buttons.Start) ;
        }

        /// <summary>
        ///   Checks for a "menu cancel" input action.
        /// </summary>
        public bool IsMenuCancel()
        {
            return IsNewButtonPress(Buttons.Back);
        }

    }
}