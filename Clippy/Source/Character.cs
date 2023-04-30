using System;
using System.Drawing;
using System.Net.Http;
using System.Windows.Forms;
using static System.Diagnostics.Process;
using Newtonsoft.Json;

//TODO: Add support for...
// - Images
// - Choices (radio buttons)

namespace Clippy
{
    #region Animations
    public enum Animation : byte
    {
        Atomic,
        BicycleOut,
        BicycleIn,
        Box,
        Check,
        Chill,
        ExclamationPoint,
        FadeIn,
        FadeOut,
        FeelingDown,
        Headset,
        LookingBottomLeft,
        LookingBottomRight,
        LookingDown,
        LookingUpperLeft,
        LookingUpperRight,
        LookingLeftAndRight,
        LookingUp,
        Plane,
        PointingDown,
        PointingLeft,
        PointingRight,
        PointingUp,
        Poke,
        Reading,
        RollPaper,
        ScrachingHead,
        Shovel,
        Telescope,
        Tornado,
        Toy,
        Writing
    }
    #endregion

    #region Character
    /// <summary>
    /// The assistant.
    /// </summary>
    static class Character
    {
        // A reference to the parent form that summons thee.
        static MainForm CharacterForm;

        // PictureBox reference.
        static PictureBox PictureFrame;

        public static void Initialize(MainForm form)
        {
            CharacterForm = form;
            PictureFrame = form.Controls["picAssistant"] as PictureBox;

            AnimationSystem.Initialize();
        }

        public static void Prompt()
        {
            DialogSystem.Prompt();
        }

        public static void Say(string text)
        {
            DialogSystem.Say(text);
        }

        public static void SayRandom()
        {
            DialogSystem.SayRandom();
        }

        public static void PlayAnimation(Animation a)
        {
            AnimationSystem.Play(a);
        }

        public static void PlayRandomAnimation()
        {
            AnimationSystem.PlayRandom();
        }

        public static void ProcessInput(string userInput)
        {
            PlayRandomAnimation();

            var client = new HttpClient();
            var url = "https://free.churchless.tech/v1/chat/completions";
            var content = new StringContent(
                "{\"model\": \"gpt-3.5-turbo\",\"messages\": [{\"role\": \"user\", \"content\": \"" + userInput + "\"}]}",
                System.Text.Encoding.UTF8,
                "application/json"
            );
            var responseTask = client.PostAsync(url, content);
            responseTask.Wait();
            var response = responseTask.Result;
            var resultTask = response.Content.ReadAsStringAsync();
            resultTask.Wait();
            var result = resultTask.Result;

            var json = JsonConvert.DeserializeObject<RootAnswer>(result);

            Say(json.choices[0].message.content);
        }

        private static void DelayExit()
        {
            Timer a = new Timer() { Interval = 1000 };
            a.Tick += (s, e) => { CharacterForm.Exit(); };
            a.Start();
        }
        #endregion

    #region Dialog system
    public static class DialogSystem
    {
        //TODO: #11 Re-use BubbleForm in Character.DialogSystem

        // The current active bubble text.
        static Form BubbleForm;
        // Defaults
        static readonly Color BubbleColor = Color.FromArgb(255, 255, 204);
        static readonly Font DefaultFont = new Font("Segoe UI", 9);
        static readonly Image BubbleTail = Utils.LoadEmbeddedImage("Bubble.Tail.png");

        private static Control[] _prompt;

        public static void Init()
        {
            _prompt = GetPrompt();
        }
            
        /// <summary>
        /// Prompt the user for information.
        /// </summary>
        public static void Prompt()
        {
            Utils.Log("Prompt");

            if (BubbleForm != null)
                BubbleForm.SuspendLayout();

            BubbleForm = GetBaseForm(GetPrompt());
            BubbleForm.ResumeLayout();

            BubbleForm.Show();
        }

        static Control[] GetPrompt(string answer = "")
        {
            Control[] ca = new Control[2];

            ca[0] = new Label()
            {
                Location = new Point(4, 6),
                AutoSize = true,
                MaximumSize = new Size(192, 0),
                Font = DefaultFont,
                Text = answer == "" ? "What would you like to ask?" : answer,
            };
            ca[1] = new TextBox()
            {
                Size = new Size(190, 36),
                Font = DefaultFont,
                Location = new Point(4, ca[0].PreferredSize.Height + 16),
                Multiline = true
            };
            ca[1].KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) // Includes Return
                {
                    e.SuppressKeyPress = true;
                    ProcessInput((s as TextBox).Text);
                }
            };

            return ca;
        }

        /// <summary>
        /// Say something to the user.
        /// </summary>
        /// <param name="text">Text.</param>
        internal static void Say(string text)
        {
            Utils.Log($"Say::{text}");

            if (BubbleForm != null)
            {
                BubbleForm.Close();
                BubbleForm = null;
                GC.Collect(3, GCCollectionMode.Forced);
            }

            //BubbleForm = GetBaseForm(GetSay(text));
            BubbleForm = GetBaseForm(GetPrompt(text));

            BubbleForm.Show();
        }

        internal static void SayRandom()
        {
            string[] s = { // Split these into different arrays? for each os?
// Tips
"Did you know Steam™ mostly works with protocols, like steam://AddNonSteamGame?",
"Want to get to the Start menu startup folder? shell:startup",
"Remember to do your backups!",
@"Typing ""cmd"" or ""powershell"" in File Explorer will start a prompt at the directory.",
// Jokes
"So... You come here often?",
"(this isFor ThE fAnS and G GaMerGirls)",
"Welcome.\nWelcome to City 17.",
"I can see you, but can you see me?",
"Do you need help looking at the screen?",
"I'm not as fun as BonziBuddy, but at least I'm not spyware, right?",
"ＭＡＸＩＭＵＭ ＡＲＭＯＲ",
"Are you sure you want to click that?",
"Seems like you're shitting out a letter...",
"The program '[3440] Clippy.exe' has exited with code -1 (0xFFFFFFFFFFFFFFFF).",
"<3?",
"Hi, I'm a vegan.",
"_suffer();",
"Deleting your files... I mean, sending your files to the NSA...",
"Hey, mind if I use more memory?\nMade ya think!",
"Bazinga!",
"Trouble with Windows? Re-install it!",
"hey..\n\n\nit me",
":-)",
"ed, vi, vim, nvim, what's next? smvimod?",
"Number 15, Burger Foot lettuce",
"Virtual Hugs included"
            };

            Say(s[Utils.R.Next(0, s.Length)]);
        }
            
        static Control[] GetSay(string text)
        {
            Control[] ca = new Control[1];

            ca[0] = new Label()
            {
                Location = new Point(4, 6),
                AutoSize = true,
                MaximumSize = new Size(192, 0),
                Font = DefaultFont,
                Text = text
            };
            return ca;
        }

        private static BubbleForm GetBaseForm(Control[] subControls)
        {
            if (BubbleForm != null)
                BubbleForm.Close();

            BubbleForm f = new BubbleForm();

            /* Bubble body */
            Panel p = new Panel();
            p.Controls.AddRange(subControls);
            p.AutoSize = true;
            p.MaximumSize = new Size(200, 0);
            p.BackColor = BubbleColor;
            p.BorderStyle = BorderStyle.FixedSingle;

            /* Bubble tail */
            PictureBox pb = new PictureBox();
            pb.Size = new Size(BubbleTail.Width, BubbleTail.Height);
            pb.Image = BubbleTail;

            p.ClientSizeChanged += (s, e) =>
            { // Because the form autosizes.
                pb.Location =
                    new Point((int)(f.ClientSize.Width / 1.62), p.Height - 1);
            };

            // Important order.
            f.Controls.Add(pb);
            f.Controls.Add(p);

            f.Location =
                new Point(CharacterForm.Location.X - (f.Size.Width / 2),
                CharacterForm.Location.Y - (f.Size.Height));

            return f;
        }

        public static void BubbleFormShow()
        {
            BubbleForm.Show();
        }
        public static void BubbleFormHide()
        {
            BubbleForm.Hide();
        }
        }
    #endregion

    #region Animation system
    static class AnimationSystem
    {
        public static void Initialize()
        {
            Idle = Utils.LoadEmbeddedImage("Clippy.Idle.png");

            NumberOfAnimations = Enum.GetNames(typeof(Animation)).Length;

            AnimationTimer = new Timer() { Interval = TimerInterval };
            AnimationTimer.Tick += (s, e) =>
            {
                if (CurrentFrame < MaxFrame)
                {
                    PictureFrame.Image = GetNextFrame();
                        
                    // Calls the GC every 10 frame in an optimized fashion
                    if (CurrentFrame % 10 == 0)
                        GC.Collect(1, GCCollectionMode.Optimized);
                }
                else
                {
                    StopAnimation();
                    PictureFrame.Image = Idle;
                    GC.Collect(3, GCCollectionMode.Forced);
                }
            };
        }
            
        // Default AnimationTimer interval.
        const int TimerInterval = 100;
        static Timer AnimationTimer;
        static Animation CurrentAnimation;
        static int CurrentFrame, MaxFrame, NumberOfAnimations;
        static Image Idle;
        static bool IsPlaying => AnimationTimer.Enabled;

        /// <summary>
        /// Play an animation. Ignores if one is already playing.
        /// </summary>
        /// <param name="anim">Name of the animation.</param>
        public static void Play(Animation anim)
        {
            if (IsPlaying) return;

            Utils.Log("Playing animation: " + anim);

            CurrentAnimation = anim;
            CurrentFrame = 0;

            switch (anim)
            {
                case Animation.Atomic: MaxFrame = 35; break;
                case Animation.BicycleOut: MaxFrame = 32; break;
                case Animation.BicycleIn: MaxFrame = 28; break;
                case Animation.Box: MaxFrame = 39; break;
                case Animation.Check: MaxFrame = 19; break;
                case Animation.Chill: MaxFrame = 85; break;
                case Animation.ExclamationPoint: MaxFrame = 10;  break;
                case Animation.FadeIn: MaxFrame = 3; break;
                case Animation.FadeOut: MaxFrame = 3; break;
                case Animation.FeelingDown: MaxFrame = 46; break;
                case Animation.Headset: MaxFrame = 32; break;
                case Animation.LookingBottomLeft: MaxFrame = 5; break;
                case Animation.LookingBottomRight: MaxFrame = 12; break;
                case Animation.LookingDown: MaxFrame = 5; break;
                case Animation.LookingUpperLeft: MaxFrame = 5;  break;
                case Animation.LookingUpperRight: MaxFrame = 10; break;
                case Animation.LookingLeftAndRight: MaxFrame = 18; break;
                case Animation.LookingUp: MaxFrame = 5; break;
                case Animation.Plane: MaxFrame = 57; break;
                case Animation.PointingDown: MaxFrame = 13; break;
                case Animation.PointingLeft: MaxFrame = 9; break;
                case Animation.PointingRight: MaxFrame = 11; break;
                case Animation.PointingUp: MaxFrame = 10; break;
                case Animation.Poke: MaxFrame = 15; break;
                case Animation.Reading: MaxFrame = 53; break;
                case Animation.RollPaper: MaxFrame = 49; break;
                case Animation.ScrachingHead: MaxFrame = 17; break;
                case Animation.Shovel: MaxFrame = 37; break;
                case Animation.Telescope: MaxFrame = 55; break;
                case Animation.Tornado: MaxFrame = 31; break;
                case Animation.Toy: MaxFrame = 13; break;
                case Animation.Writing: MaxFrame = 59; break;
                default:
                    Utils.Log("Animation aborted");
                    return;
            }

            AnimationTimer.Start();
        }

        /// <summary>
        /// Stop the current animation.
        /// </summary>
        static void StopAnimation()
        {
            AnimationTimer.Stop();
        }

        static Image GetFrame(int frame)
        {
            return Utils.LoadEmbeddedImage(
                $"Clippy.Animations.{CurrentAnimation}.{frame}.png"
            );
        }

        static Image GetNextFrame()
        {
            return GetFrame(CurrentFrame++);
        }

        /// <summary>
        /// Play a random animation.
        /// </summary>
        public static void PlayRandom()
        {
            Play((Animation)Utils.R.Next(0, NumberOfAnimations));
        }
    }
}
#endregion
}
