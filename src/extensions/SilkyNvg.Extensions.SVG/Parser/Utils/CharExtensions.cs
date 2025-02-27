namespace SilkyNvg.Extensions.Svg.Parser.Utils
{
    internal static class CharExtensions
    {

        internal static int FromHex(this char c)
            => char.IsDigit(c) ? (c - 0x30) : (c - (c.IsLowercaseAscii() ? 0x57 : 0x37));

        internal static string ToHex(this byte num)
        {
            Span<char> chars = stackalloc char[2];
            int rem = num >> 4;
            chars[0] = (char)(rem + (rem < 10 ? 48 : 55));
            rem = rem - 16 * rem;
            chars[1] = (char)(rem + (rem < 10 ? 48 : 55));
            return chars.ToString();
        }

        internal static string ToHex(this char c)
            => ((int)c).ToString("x");

        internal static bool IsInRange(this char c, int lower, int upper)
            => (c >= lower) && (c <= upper);

        internal static bool IsNormalQueryChar(this char c)
        {
            return c.IsInRange(0x21, 0x7e) && (c != Symbols.DOUBLE_QUOTE) && (c != Symbols.CURVED_QUOTE)
                && (c != Symbols.NUM) && (c != Symbols.LT) && (c != Symbols.GT);
        }

        internal static bool IsNormalPathCharacter(this char c)
        {
            return c.IsInRange(0x20, 0x7e) && (c != Symbols.DOUBLE_QUOTE) && (c != Symbols.CURVED_QUOTE)
                && (c != Symbols.NUM) && (c != Symbols.LT) && (c != Symbols.GT) && (c != Symbols.SPACE) && (c != Symbols.QUESTION_MARK);
        }

        internal static bool IsUppercaseAscii(this char c) => char.IsAsciiLetterUpper(c);

        internal static bool IsLowercaseAscii(this char c) => char.IsAsciiLetterLower(c);

        internal static bool IsAlphanumericAscii(this char c) => char.IsLetterOrDigit(c);

        internal static bool IsHex(this char c) => char.IsAsciiHexDigit(c);

        internal static bool IsNonAscii(this char c) => !char.IsAscii(c) && char.IsLetter(c);

        internal static bool IsNonPrintable(this char c)
            => (c >= 0x0 && c <= 0x8) || (c >= 0xe && c <= 0x1f) || (c >= 0x7f && c <= 0x9f);

        internal static bool IsLetter(this char c) => char.IsLetter(c);

        internal static bool IsName(this char c)
            => c.IsNonAscii() || c.IsLetter() || (c == Symbols.UNDERSCORE) || (c == Symbols.MINUS) || char.IsDigit(c);

        internal static bool IsCustomElementName(this char c)
        {
            return (c == Symbols.UNDERSCORE) || (c == Symbols.MINUS) || (c == Symbols.DOT) || char.IsDigit(c) || c.IsLowercaseAscii() || (c == 0xb7) ||
                c.IsInRange(0xc0, 0xd6) || c.IsInRange(0xd8, 0xf6) || c.IsInRange(0xf8, 0x37d) || c.IsInRange(0x37f, 0x1fff) ||
                c.IsInRange(0x200c, 0x200d) || c.IsInRange(0x203f, 0x2040) || c.IsInRange(0x2070, 0x218f) || c.IsInRange(0x2c00, 0x2fef) ||
                c.IsInRange(0x3001, 0xd7ff) || c.IsInRange(0xf900, 0xfdcf) || c.IsInRange(0xfdf0, 0xfffd) || c.IsInRange(0x10000, 0x1effff);
        }

        internal static bool IsNameStart(this char c)
            => c.IsNonAscii() || c.IsUppercaseAscii() || c.IsLowercaseAscii() || (c == Symbols.UNDERSCORE);

        internal static bool IsLineBreak(this char c)
            => (c == Symbols.LINE_FEED) || (c == Symbols.CARRIAGE_RETURN);

        internal static bool IsSpaceCharacter(this char c)
            => c == (Symbols.SPACE) || (c == Symbols.TAB) || (c == Symbols.LINE_FEED) || (c == Symbols.CARRIAGE_RETURN) || (c == Symbols.FORM_FEED);

        internal static bool IsWhiteSpaceCharacter(this char c) => char.IsWhiteSpace(c);

        internal static bool IsDigit(this char c) => char.IsDigit(c);

        internal static bool IsUrlCodePoint(this char c) =>
            c.IsAlphanumericAscii()     || c == Symbols.EXCLAMATION_MARK    || c == Symbols.DOLLAR          || c == Symbols.AMPERSAND ||
            c == Symbols.SINGLE_QUOTE   || c == Symbols.OPEN_PARENS         || c == Symbols.CLOSE_PARENS    ||
            c == Symbols.ASTERISK       || c == Symbols.PLUS                || c == Symbols.MINUS           || c == Symbols.COMMA ||
            c == Symbols.DOT            || c == Symbols.SLASH               || c == Symbols.COLON           || c == Symbols.SEMICOLON ||
            c == Symbols.EQUALITY       || c == Symbols.QUESTION_MARK       || c == Symbols.AT              || c == Symbols.UNDERSCORE ||
            c == Symbols.TILDE          ||
            c.IsInRange(0xa0, 0xd7ff)     || c.IsInRange(0xe000, 0xfdcf)   || c.IsInRange(0xfdf0, 0xfffd)   ||
            c.IsInRange(0x10000, 0x1FFFD) || c.IsInRange(0x20000, 0x2fffd) || c.IsInRange(0x30000, 0x3fffd) || c.IsInRange(0x40000, 0x4fffd) ||
            c.IsInRange(0x50000, 0x5fffd) || c.IsInRange(0x60000, 0x6fffd) || c.IsInRange(0x70000, 0x7fffd) || c.IsInRange(0x80000, 0x8fffd) ||
            c.IsInRange(0x90000, 0x9fffd) || c.IsInRange(0xa0000, 0xafffd) || c.IsInRange(0xb0000, 0xbfffd) || c.IsInRange(0xc0000, 0xcfffd) ||
            c.IsInRange(0xd0000, 0xdfffd) || c.IsInRange(0xe0000, 0xefffd) || c.IsInRange(0xf0000, 0xffffd) || c.IsInRange(0x100000, 0x10fffd);

        internal static bool IsInvalid(this int c)
            => (c == 0) || (c > Symbols.MAXIMUM_CP) || (c > 0xD800 && c < 0xDFFF);

    }
}
