﻿using System;
using System.Drawing;
using System.Windows.Forms;
using static System.Diagnostics.Process;

//TODO: Add support for...
// - Images
// - Choices (radio buttons?)

namespace FuckingClippy
{
    public enum Animation : byte
    {
        Atomic,
        Bicycle,
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

    /// <summary>
    /// The assistant.
    /// </summary>
    static class Character
    {
        // A reference to the parent form that summons thee.
        static MainForm CharacterForm;

        public static void Initialize(MainForm form, PictureBox pb)
        {
            CharacterForm = form;

            AnimationSystem.Initialize(pb);
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
            string[] u = userInput.Split(' ');

            if (u.Length > 0 && userInput.Length > 0)
                switch (u[0].ToLower())
                {
                    /*
                     * Main commands
                     */

                    case "run":
                        if (u.Length > 1)
                            try
                            {
                                Start(userInput.Substring(4));
                            }
                            catch (Exception e)
                            {
                                Say($"I couldn't run that, sorry.\n({e.GetType().Name})");
                            }
                        else
                            Say("I can't run, buddy.");
                        break;

                    case "say":
                        if (u.Length > 1)
                            Say(userInput.Substring(4));
                        else
                            Say("Say what?");
                        break;

                    case "search":
                        if (u.Length > 1)
                            Start(
                        $"https://www.google.com/search?q={Uri.EscapeDataString(userInput.Substring(7))}"
                            );
                        else
                            Say("Search for what again?");
                        break;

                    case "random":
                        SayRandom();
                        break;

                    /*
                     * Help section
                     */

                    case "help":
                        if (u.Length > 1)
                            switch (u[1].ToLower())
                            {
                            case "me":
                                if (u.Length > 2)
                                    switch (u[2].ToLower())
                                    {
                                    case "suicide":
                                    case "die":
                                        Say("Please seek professional help.");
                                        break;

                                    case "kill":
                                        if (u.Length > 3)
                                            switch (u[3].ToLower())
                                            {
                                            case "myself":
                                                Say("Please seek professional help.");
                                                break;
                                            default:
                                                Say("I won't do your dirty job.");
                                                break;
                                            }
                                        else
                                            Say("WHO?");
                                        break;

                                    default:
                                        Say("Can't help you with that yet.");
                                        break;
                                    }
                                else
                                    Say(@"Try the ""help"" command!");
                                break;

                            case "yourself":
                                Say("How kind! But no, I'm fine");
                                break;

                            default:
                                Say("WHO");
                                break;
                            }
                        else
                            Say(
@"Here are some commands:

run <t> - Run an app from PATH.
say <t> - Make me say something.
search <t> - Search on Google.com.
random - I'll tell you something randomly."
                            );
                        break;

                    /*
                     * Non-serious, small talk.
                     */

                    case "screw":
                    case "fuck":
                        if (u.Length > 1)
                            switch (u[1].ToLower())
                            {
                            case "me":
                                Say("No thanks, I'll pass.");
                                break;
                            case "you":
                                Say("Hey buddy I can always shutdown your computer.");
                                break;
                            default:
                                Say("WHO?");
                                break;
                            }
                        else
                            Say("WHO?");
                        break;

                    case "exit":
                    case "close":
                    case "die":
                        Say("Okay!");

                        // Dirty solution
                        Timer a = new Timer();
                        a.Interval = 1000;
                        a.Tick += (s, e) => { CharacterForm.Exit(); };
                        a.Start();
                        break;

                    default:
                        Say("Are you okay?");
                        break;
                }
            else
                Say("Is anyone there?");
        }

        #region Dialog system
        static class DialogSystem
        {
            // The current active bubble text.
            static Form CurrentBubbleForm;
            // Defaults
            static Color BubbleColor = Color.FromArgb(255, 255, 204);
            static Font DefaultFont = new Font("Segoe UI", 9);
            static Image BubbleTail =
                Utils.LoadEmbeddedImage("Bubble.Tail.png");
            
            /// <summary>
            /// Prompt the user for information.
            /// </summary>
            internal static void Prompt()
            {
                Utils.Log($"Prompt");

                if (CurrentBubbleForm != null)
                    CurrentBubbleForm.SuspendLayout();

                CurrentBubbleForm = GetBaseForm(GetPrompt());
                CurrentBubbleForm.ResumeLayout();

                CurrentBubbleForm.Show();
            }

            static Control[] GetPrompt()
            {
                Control[] ca = new Control[2];

                ca[0] = new Label();
                ca[0].AutoSize = true;
                ca[0].Text = "What would you like to do?";
                ca[0].Location = new Point(4, 8);
                ca[0].Font = DefaultFont;

                ca[1] = new TextBox();
                (ca[1] as TextBox).Multiline = true;
                ca[1].Size = new Size(190, 36);
                ca[1].Font = DefaultFont;
                ca[1].Location = new Point(4, 30);
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
                Utils.Log($"Say - {text}");

                if (CurrentBubbleForm != null)
                {
                    CurrentBubbleForm.Close();
                    CurrentBubbleForm = null;
                }

                CurrentBubbleForm = GetBaseForm(GetSay(text));

                CurrentBubbleForm.Show();
            }

            internal static void SayRandom()
            {
                string[] s =
                {
"So, you come here often?",
"Would you like help with hugging yourself?",
"(this isFor ThE fAnS and G GaMerGirls)",
"Welcome. Welcome to City 17.",
"I can see you, but can you see me?",
"Do you need help looking at that screen?",
"I'm not as fun as BonziBuddy, but at least I'm not spyware, right?",
"It would be a shame if something happened to these fil-- OOOPSS!",
"［ ＭＡＸＩＭＵＭ ＡＲＭＯＲ ］",
"Are you sure you want to click that?",
"0x4E4F4246\nDid I spook you?",
"I am not an AI, just a bunch of CIL instructions.",
"Seems like you need help living your life there buddy.",
"The program '[3440] FuckingClippy.exe' has exited with code 0 (0x0).",
"<3?",
"Did you know that I'm a vegan?",
"SUFFER();",
"rawrrr x33",
"I still have transparency and form autosizing issues on Mono!",
"Deleting your files...",
"S-sorry, senpai..",
"Hey do you mind if I use more memory?",
"Bazinga!",
"Hey it looks like you're writing a letter, need help to give a kiss?"
                };

                Say(s[Utils.Random.Next(0, s.Length)]);
            }

            static Control[] GetSay(string text)
            {
                Control[] ca = new Control[1];

                ca[0] = new Label();
                ca[0].Location = new Point(4, 6);
                ca[0].AutoSize = true;
                ca[0].MaximumSize = new Size(192, 0);
                ca[0].Font = DefaultFont;
                ca[0].Text = text;

                return ca;
            }
            static BubbleForm GetBaseForm(Control[] subControls)
            {
                if (CurrentBubbleForm != null)
                    CurrentBubbleForm.Close();

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
                {
                    pb.Location =
                        new Point((int)(f.ClientSize.Width / 1.62), p.Height);
                };

                f.Controls.Add(p);
                f.Controls.Add(pb);

                f.Location =
                    new Point(CharacterForm.Location.X - (f.Size.Width / 2),
                    CharacterForm.Location.Y - (f.Size.Height));

                return f;
            }
        }
        #endregion

        #region Animation system
        static class AnimationSystem
        {
            public static void Initialize(PictureBox picbox)
            {
                Frame = picbox;

                Idle = Utils.LoadEmbeddedImage("Clippy.Idle.png");

                NumberOfAnimations = Enum.GetNames(typeof(Animation)).Length;

                AnimationTimer = new Timer();
                AnimationTimer.Interval = DefaultInterval;
                AnimationTimer.Tick += (s, e) =>
                {
                    if (CurrentFrame < MaxFrame)
                    {
                        Frame.Image = GetNextFrame();

                        // Every 5 frames. Guarantees it calls it at least once.
                        if (CurrentFrame % 5 == 0)
                            GC.Collect(1);
                    }
                    else
                    {
                        Stop();
                        Frame.Image = Idle;
                    }
                };
            }

            // PictureBox reference.
            static PictureBox Frame;
            
            // Default AnimationTimer interval.
            const int DefaultInterval = 100;
            static Timer AnimationTimer;
            static Animation CurrentAnimation;
            static int CurrentFrame, MaxFrame, NumberOfAnimations;

            /// <summary>
            /// Default idle frame.
            /// </summary>
            static Image Idle;
            
            static bool IsPlaying => AnimationTimer.Enabled;

            /// <summary>
            /// Play an animation. Ignores if one is already playing.
            /// </summary>
            /// <param name="name">Name of the animation.</param>
            public static void Play(Animation name)
            {
                if (IsPlaying) return;

                Utils.Log("Playing animation: " + name);

                CurrentAnimation = name;

                CurrentFrame = 0;

                string path = "Images.Clippy.Animations." + name;

                if (!Utils.EmbeddedItemExist(path))
                    throw new ArgumentException("Embedded files for animation not found: " + name);

                MaxFrame = Utils.GetNumberOfEmbeddedItems(path);

                AnimationTimer.Start();
            }

            /// <summary>
            /// Stop the current animation.
            /// </summary>
            static void Stop()
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
                Play((Animation)Utils.Random.Next(0, NumberOfAnimations));
            }
        }
        #endregion
    }
}
