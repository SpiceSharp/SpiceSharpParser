namespace SpiceGrammar
{
    public enum SpiceTokenType
    {
        EOF = -1,
        ASTERIKS = 1,
        MINUS = 2,
        DOT = 3,
        COMMA = 4,
        DELIMITER = 5,
        NEWLINE = 6,
        ENDS = 7,
        END = 8,
        VALUE = 9,
        COMMENT = 10,
        EXPRESSION = 11,
        REFERENCE = 12,
        WORD = 13,
        WHITESPACE = 14,
        PLUS = 15,
        IDENTIFIER = 16,
        STRING = 17,
        TITLE = 18,
        CONTINUE = 19,
        EQUAL = 20
    }
}
