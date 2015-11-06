using System;
using System.Runtime.InteropServices;

using SDL2;
using OpenGL;
using System.Collections.Generic;

namespace Presentation
{
    public static class Window
    {
        private static IntPtr window, glContext;
        private static byte[] mouseState = new byte[256];

        /// <summary>
        /// The main thread ID, which is the thread ID that the OpenGL context was created on.
        /// This is the thread ID that must be used for all future OpenGL calls.
        /// </summary>
        public static int MainThreadID { get; private set; }

        /// <summary>
        /// Creates an OpenGL context and associated Window via the
        /// cross-platform SDL library.  Will clear the screen to black
        /// as quickly as possible by calling glClearColor and glClear.
        /// </summary>
        /// <param name="title"></param>
        public static void CreateWindow(string title)
        {
            // check if a window already exists
            if (window != IntPtr.Zero || glContext != IntPtr.Zero)
            {
                Console.WriteLine("There is already a valid window or OpenGL context.");
                return;
            }

            // create an openGL context (must be done before detecting OS, since we need to make calls to wglSwapInterval)
            Console.WriteLine("Initializing SDL and OpenGL.");

            // initialize SDL and set a few defaults for the OpenGL context
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DOUBLEBUFFER, 1);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DEPTH_SIZE, 24);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_ALPHA_SIZE, 8);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_STENCIL_SIZE, 8);

            // capture the rendering thread ID
            MainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;

            // create the window which should be able to have a valid OpenGL context and is resizable
            var flags = SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
            if (Program.Fullscreen) flags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
            window = SDL.SDL_CreateWindow(title, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, Program.Width, Program.Height, flags);
            if (window == IntPtr.Zero)
            {
                Console.WriteLine("Could not initialize a window using SDL.");
                return;
            }

            // create a valid OpenGL context within the newly created window
            glContext = SDL.SDL_GL_CreateContext(window);
            if (glContext == IntPtr.Zero)
            {
                Console.WriteLine("Could not get a valid OpenGL context.");
                return;
            }

            // initialize the screen to black as soon as possible
            Gl.ClearColor(0f, 0f, 0f, 1f);
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            SwapBuffers();

            OnReshapeCallbacks.Add(() => Console.WriteLine("Reshape to {0}x{1}", Program.Width, Program.Height));
        }

        /// <summary>
        /// Swap the OpenGL buffer and bring the back buffer to the screen.
        /// </summary>
        public static void SwapBuffers()
        {
            SDL.SDL_GL_SwapWindow(window);
        }

        #region Apply Preferences
        public static void SetScreenMode()
        {
            if (Program.Fullscreen)
            {
                // we need to switch to windowed mode, then set size, and then fullscreen
                // simply setting the displaymode doesn't update the resolution until
                // the window loses focus and is then refocused
                SDL.SDL_SetWindowFullscreen(window, 0);
                SDL.SDL_SetWindowSize(window, Program.Width, Program.Height);

                SDL.SDL_SetWindowFullscreen(window, 1);
            }
            else
            {
                SDL.SDL_SetWindowFullscreen(window, 0);
                SDL.SDL_SetWindowSize(window, Program.Width, Program.Height);
                SDL.SDL_SetWindowPosition(window, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED);
            }

            OnReshape(Program.Width, Program.Height);
        }
        #endregion

        #region Event Handling
        private static SDL.SDL_Event sdlEvent;

        public delegate void OnMouseWheelDelegate(uint wheel, int direction, int x, int y);

        public static OnMouseWheelDelegate OnMouseWheel { get; set; }

        public static void HandleEvents()
        {
            while (SDL.SDL_PollEvent(out sdlEvent) != 0)
            {
                switch (sdlEvent.type)
                {
                    case SDL.SDL_EventType.SDL_KEYDOWN:
                        OnKeyboardDown(sdlEvent.key.keysym.sym);
                        break;
                    case SDL.SDL_EventType.SDL_KEYUP:
                        OnKeyboardUp(sdlEvent.key.keysym.sym);
                        break;
                    case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                        // keep track of mouse state internally due to a bug in SDL
                        // https://bugzilla.libsdl.org/show_bug.cgi?id=2195
                        if (mouseState[sdlEvent.button.button] == sdlEvent.button.state) break;
                        mouseState[sdlEvent.button.button] = sdlEvent.button.state;
                        if (sdlEvent.button.y == 0 || sdlEvent.button.x == 0) mouseState[sdlEvent.button.button] = 0;

                        OnMouse(sdlEvent.button.button, sdlEvent.button.state, sdlEvent.button.x, sdlEvent.button.y);
                        break;
                    case SDL.SDL_EventType.SDL_MOUSEMOTION:
                        OnMovePassive(sdlEvent.motion.x, sdlEvent.motion.y);
                        break;
                    case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                        //OnMouseWheel(sdlEvent.wheel.which, sdlEvent.wheel.y, 0, 0);
                        if (OnMouseWheel != null) OnMouseWheel(sdlEvent.wheel.which, sdlEvent.wheel.y, 0, 0);
                        break;
                    case SDL.SDL_EventType.SDL_WINDOWEVENT:
                        switch (sdlEvent.window.windowEvent)
                        {
                            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                                OnReshape(sdlEvent.window.data1, sdlEvent.window.data2);
                                break;
                            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                                OnClose();
                                break;
                        }
                        break;
                }
            }
        }
        #endregion

        #region OnReshape and OnClose
        public static List<Action> OnReshapeCallbacks = new List<Action>();

        public static void OnReshape(int width, int height)
        {
            // for whatever reason, SDL does not give accurate sizes in its event when windowed,
            // so we just need to query the window size when in windowed mode
            if (!Program.Fullscreen)
                SDL.SDL_GetWindowSize(window, out width, out height);

            if (width % 2 == 1) width--;
            if (height % 2 == 1) height--;

            Program.Width = width;
            Program.Height = height;

            foreach (var callback in OnReshapeCallbacks) callback();
        }

        public static void OnClose()
        {
            SDL.SDL_GL_DeleteContext(glContext);
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
            Environment.Exit(0);
        }
        #endregion

        #region Mouse Callbacks
        public static void MouseMove(int lx, int ly, int x, int y)
        {
        }

        public static void MouseMovePassive(int lx, int ly, int x, int y)
        {
        }

        private static void OnMouse(int button, int state, int x, int y)
        {
            if (state == 1) Program.NextSlide();
        }

        private static void OnMovePassive(int x, int y)
        {
        }

        public static void OnMouseWheelTemp(uint wheel, int direction, int x, int y)
        {
        }
        #endregion

        #region Keyboard Callbacks
        private static void OnKeyboardDown(SDL.SDL_Keycode sym)
        {
            try
            {
                if ((char)sym == 27) Program.RunPresentation = false;
                else if ((char)sym == 80) Program.PrevSlide();
                else if ((char)sym == 79) Program.NextSlide();
                else Console.WriteLine("Keyboard press: " + (int)(char)sym);
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error while calling Input.AddKey.  Error: " + e.Message);
            }
        }

        private static void OnKeyboardUp(SDL.SDL_Keycode sym)
        {
            try
            {
                
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error while calling Input.RemoveKey.  Error: " + e.Message);
            }
        }
        #endregion
    }
}
