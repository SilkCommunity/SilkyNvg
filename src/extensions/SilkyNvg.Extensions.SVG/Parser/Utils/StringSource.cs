using System.Globalization;

namespace SilkyNvg.Extensions.Svg.Parser.Utils
{
    internal sealed class StringSource : IEquatable<StringSource?>
    {

        private readonly int _last;

        internal string Content { get; }

        internal int Index { get; private set; }

        internal char Current { get; private set; }

        internal bool IsDone => Current == Symbols.EOF;

        internal StringSource(string? content)
        {
            Content = content ?? string.Empty;
            _last = Content.Length - 1;
            Index = 0;
            Current = (_last == -1) ? Symbols.EOF : Content[0];
        }

        internal char Next()
        {
            if (Index == _last)
            {
                Current = Symbols.EOF;
                Index = Content.Length;
            }
            else if (Index < Content.Length)
            {
                Current = Content[++Index];
            }
            return Current;
        }

        internal char Back()
        {
            if (Index > 0)
            {
                Current = Content[--Index];
            }
            return Current;
        }

        internal char BackTo(int index)
        {
            int diff = Index - index;
            char current = Symbols.NULL;

            while (diff > 0)
            {
                current = Back();
                diff--;
            }

            return current;
        }

        internal string ConsumeEscape()
        {
            char current = Current;
            if (current.IsHex())
            {
                bool isHex = true;
                char[] escape = new char[6];
                int length = 0;

                while (isHex && (length < escape.Length))
                {
                    escape[length++] = current;
                    current = Next();
                    isHex = current.IsHex();
                }

                if (!current.IsSpaceCharacter())
                {
                    Back();
                }

                int code = int.Parse(new string(escape, 0, length), NumberStyles.HexNumber);
                if (!code.IsInvalid())
                {
                    return char.ConvertFromUtf32(code);
                }
                current = Symbols.REPLACEMENT;
            }
            return current.ToString();
        }

        internal bool IsValidEscape()
        {
            if (Current == Symbols.BACKSLASH)
            {
                Current = Next();
                Back();

                return (Current != Symbols.EOF) && !Current.IsLineBreak();
            }
            return false;
        }

        public override bool Equals(object? obj)
        {
            return (obj is StringSource source) && Equals(source);
        }

        public bool Equals(StringSource? other)
        {
            return (other is not null) && (Content == other.Content);
        }

        public override int GetHashCode()
        {
            return Content.GetHashCode();
        }

        public static bool operator ==(StringSource left, StringSource right)
            => left.Equals(right);

        public static bool operator !=(StringSource left, StringSource right)
            => !left.Equals(right);

    }
}
