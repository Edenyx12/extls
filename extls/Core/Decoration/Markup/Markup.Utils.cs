using System.Text;

namespace extls.Core.Decoration;

public static partial class Markup
{
    public static string SafeBackslash(string text)
    {
        int index = text.IndexOf('\\');

        if (index < 0) return text;

        var fix = new StringBuilder(text.Length + 1);

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] is '\\')
                fix.Append(@"\\");

            fix.Append(text[i]);
        }

        return fix.ToString();
    }

    public static int FindStopToken(string str, int index, string stopToken)
    {
        int stop = str.IndexOf(stopToken, index, StringComparison.Ordinal);
        return stop == -1 ? -1 : stop - index;
    }

    public static bool Match(string str, int index, string target)
    {
        if (index < 0 || index + target.Length > str.Length) return false;

        ReadOnlySpan<char> slice = str.AsSpan(index, target.Length);
        return slice.SequenceEqual(target);
    }
}