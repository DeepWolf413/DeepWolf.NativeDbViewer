using System;
using System.Text;
using DeepWolf.NativeDbViewer.Models;
using Prism.Mvvm;

namespace DeepWolf.NativeDbViewer.ViewModels
{
    public class NativeViewModel : BindableBase
    {
        private Native representedNative;

        public NativeViewModel(Native nativeToRepresent)
        {
            representedNative = nativeToRepresent;
        }

        public string FullNativeString => representedNative.ToString();

        public string Namespace => representedNative.Namespace;

        public string Name => representedNative.Name;

        public string Hash => representedNative.Hash;

        public string Comment => representedNative.Comment;

        public string CommentTruncated
        {
            get
            {
                string truncated = representedNative.Comment;
                int newLineIndex = truncated.IndexOf("\n", StringComparison.Ordinal);

                if (newLineIndex == -1)
                { return truncated; }

                truncated = truncated.Substring(0, newLineIndex) + "...";

                return truncated;
            }
        }

        public string ScriptUsage => representedNative.ScriptUsage;

        public string Build => representedNative.Build;

        public string Parameters
        {
            get
            {
                StringBuilder paramBuilder = new StringBuilder();
                foreach (var nativeParam in representedNative.Parameters)
                {
                    paramBuilder.Append($"{nativeParam.ToString()}, ");
                }

                return paramBuilder.ToString().TrimEnd().TrimEnd(',');
            }
        }

        public string ReturnType => representedNative.ReturnType;

        public bool HasComment => !string.IsNullOrEmpty(Comment);

        public bool HasScriptUsage => !string.IsNullOrEmpty(ScriptUsage);
    }
}
