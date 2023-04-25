﻿using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using static System.Reflection.Assembly;

namespace Clippy
{
    static class Utils
    {
        /// <summary>
        /// Random number generator, best results are static classes.
        /// </summary>
        public static Random R = new Random();

        #region Runtime
        public static readonly Assembly Project = GetExecutingAssembly();
        public static readonly string ProjectName = Project.GetName().Name;
        public static readonly bool RunningMono = Type.GetType("Mono.Runtime") != null;
        #endregion

        #region Assembly
        public static Stream LoadEmbedded(string path)
        {
            return Project.GetManifestResourceStream($"{ProjectName}.{path}");
        }

        public static Image LoadEmbeddedImage(string path)
        {
            return Image.FromStream(
                Project.GetManifestResourceStream(
                    $"{ProjectName}.Images.{path}"
                )
            );
        }
        #endregion

        public static void Log(string text)
        {
            Console.WriteLine(text);
        }
    }
}
