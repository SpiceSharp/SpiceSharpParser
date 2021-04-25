namespace SpiceSharpParser.Lexers.Netlist.Spice
{
    /// <summary>
    /// Types of terminals in SPICE grammar.
    /// </summary>
    public enum SpiceTokenType
    {
        /// <summary>
        /// End of file token
        /// </summary>
        EOF = -1,

        /// <summary>
        /// Start token (hidden)
        /// </summary>
        START = 0,

        /// <summary>
        /// * character token
        /// </summary>
        ASTERIKS = 1,

        /// <summary>
        /// - character token
        /// </summary>
        MINUS = 2,

        /// <summary>
        /// . character token
        /// </summary>
        DOT = 3,

        /// <summary>
        /// , character token
        /// </summary>
        COMMA = 4,

        /// <summary>
        /// Delimiter characters token
        /// </summary>
        DELIMITER = 5,

        /// <summary>
        /// new line token
        /// </summary>
        NEWLINE = 6,

        /// <summary>
        /// .ENDS token
        /// </summary>
        ENDS = 7,

        /// <summary>
        /// .END token
        /// </summary>
        END = 8,

        /// <summary>
        /// value token
        /// </summary>
        VALUE = 9,

        /// <summary>
        /// token with comment line
        /// </summary>
        COMMENT = 10,

        /// <summary>
        /// expression with bracket token.
        /// </summary>
        EXPRESSION_BRACKET = 11,

        /// <summary>
        /// reference token
        /// </summary>
        REFERENCE = 12,

        /// <summary>
        /// word token
        /// </summary>
        WORD = 13,

        /// <summary>
        /// whitespace token
        /// </summary>
        WHITESPACE = 14,

        /// <summary>
        /// + character token
        /// </summary>
        PLUS = 15,

        /// <summary>
        /// identifier token
        /// </summary>
        IDENTIFIER = 16,

        /// <summary>
        /// double quoted sting token
        /// </summary>
        DOUBLE_QUOTED_STRING = 17,

        /// <summary>
        /// double quoted sting token
        /// </summary>
        SINGLE_QUOTED_STRING = 18,

        /// <summary>
        /// title token
        /// </summary>
        TITLE = 19,

        /// <summary>
        /// = character
        /// </summary>
        EQUAL = 21,

        /// <summary>
        /// ; style comment
        /// </summary>
        COMMENT_PSPICE = 22,

        /// <summary>
        /// $ style comment
        /// </summary>
        COMMENT_HSPICE = 23,

        /// <summary>
        /// .ENDL token
        /// </summary>
        ENDL = 24,

        /// <summary>
        /// #COM token - start of a block comment in SmartSpice
        /// </summary>
        COM_START = 25,

        /// <summary>
        /// #ENDCOM token - end of a block comment in SmartSpice
        /// </summary>
        COM_END = 26,

        /// <summary>
        /// Content of a block comment in SmartSpice
        /// </summary>
        COM_CONTENT = 27,

        /// <summary>
        /// .IF token
        /// </summary>
        IF = 28,

        /// <summary>
        /// .ELSE token
        /// </summary>
        ELSE = 29,

        /// <summary>
        /// .ELSEIF token
        /// </summary>
        ELSE_IF = 30,

        /// <summary>
        /// .ENDIF token
        /// </summary>
        ENDIF = 31,

        /// <summary>
        /// Boolean expression token with delimiters
        /// </summary>
        BOOLEAN_EXPRESSION = 32,

        /// <summary>
        /// expression token
        /// </summary>
        EXPRESSION_SINGLE_QUOTES = 33,

        /// <summary>
        /// percent token
        /// </summary>
        PERCENT = 34,

        /// <summary>
        ///  continuation current line
        /// </summary>
        CONTINUATION_CURRENT_LINE = 35,

        /// <summary>
        /// continuation next line
        /// </summary>
        CONTINUATION_NEXT_LINE = 36,

        /// <summary>
        /// model token
        /// </summary>
        MODEL = 37,

        /// <summary>
        /// expression with bracket token.
        /// </summary>
        EXPRESSION = 38,

        PREFIX_SINGLE = 39,

        PREFIX_COMPLEX = 40,

        SUFFIX = 41,
    }
}