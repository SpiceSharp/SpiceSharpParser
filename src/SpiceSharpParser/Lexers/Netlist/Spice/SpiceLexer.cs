using System.Collections.Generic;

namespace SpiceSharpParser.Lexers.Netlist.Spice
{
    /// <summary>
    /// A lexer for SPICE netlists.
    /// </summary>
    public class SpiceLexer
    {
        private LexerGrammar<SpiceLexerState> grammar;
        private SpiceLexerOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceLexer"/> class.
        /// </summary>
        /// <param name="options">options for lexer.</param>
        public SpiceLexer(SpiceLexerOptions options)
        {
            this.options = options ?? throw new System.ArgumentNullException(nameof(options));
            BuildGrammar();
        }

        /// <summary>
        /// Gets tokens for SPICE netlist.
        /// </summary>
        /// <param name="netlistText">A string with SPICE netlist.</param>
        /// <returns>
        /// An enumerable of tokens.
        /// </returns>
        public IEnumerable<SpiceToken> GetTokens(string netlistText)
        {
            var state = new SpiceLexerState() { LexerOptions = new LexerOptions(true, '+', '\\') };
            var lexer = new Lexer<SpiceLexerState>(grammar, state.LexerOptions);

            foreach (var token in lexer.GetTokens(netlistText, state))
            {
                yield return new SpiceToken((SpiceTokenType)token.TokenType, token.Lexem, state.LineNumber);
            }
        }

        /// <summary>
        /// Builds SPICE lexer grammar.
        /// </summary>
        private void BuildGrammar()
        {
            var builder = new LexerGrammarBuilder<SpiceLexerState>();
            builder.AddRule(new LexerInternalRule("LETTER", "[a-zA-Z]"));
            builder.AddRule(new LexerInternalRule("CHARACTER", "[a-zA-Z0-9\\-+]"));
            builder.AddRule(new LexerInternalRule("DIGIT", "[0-9]"));
            builder.AddRule(new LexerInternalRule("SPECIAL", "[\\\\\\[\\]_\\.\\:\\!%\\#\\-;\\<>\\^+/\\*]"));
            builder.AddRule(new LexerInternalRule("SPECIAL_WITHOUT_BACKSLASH", "[\\[\\]_\\.\\:\\!%\\#\\-;\\<>\\^+/\\*]"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.WHITESPACE,
                "A whitespace characters that will be ignored",
                "[ \t]*",
                (SpiceLexerState state, string lexem) =>
                {
                    return LexerRuleReturnState.IgnoreToken;
                }));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.TITLE,
                "The title - first line of SPICE token",
                @"[^\r\n]+",
                null,
                (SpiceLexerState state, string lexem) =>
                {
                    if (state.LineNumber == 1 && options.HasTitle)
                    {
                        return LexerRuleHandleState.Use;
                    }

                    return LexerRuleHandleState.Next;
                }));

            builder.AddRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceTokenType.DOT,
                    "A dot character",
                    "\\."));

            builder.AddRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceTokenType.COMMA,
                    "A comma character",
                    ","));

            builder.AddRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceTokenType.DELIMITER,
                    "A delimeter character",
                    @"(\(|\)|\|)",
                    (SpiceLexerState state, string lexem) =>
                     {
                         return LexerRuleReturnState.ReturnToken;
                     }));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.COM_START,
                "An block comment start",
                @"#COM",
                (SpiceLexerState state, string lexem) =>
                {
                    state.InCommentBlock = true;
                    return LexerRuleReturnState.IgnoreToken;
                },
                (SpiceLexerState state, string lexem) =>
                {
                    if (state.PreviousReturnedTokenType == (int)SpiceTokenType.NEWLINE || state.PreviousReturnedTokenType == 0)
                    {
                        return LexerRuleHandleState.Use;
                    }

                    return LexerRuleHandleState.Next;
                },
                ignoreCase: true));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
              (int)SpiceTokenType.COM_END,
              "An block comment end",
              "#ENDCOM",
              (SpiceLexerState state, string lexem) =>
              {
                  state.InCommentBlock = false;
                  return LexerRuleReturnState.IgnoreToken;
              },
              (SpiceLexerState state, string lexem) =>
              {
                  if (state.InCommentBlock)
                  {
                      return LexerRuleHandleState.Use;
                  }

                  return LexerRuleHandleState.Next;
              },
              ignoreCase: true));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
               (int)SpiceTokenType.COM_CONTENT,
               "An block comment content",
               @".*",
               (SpiceLexerState state, string lexem) =>
               {
                   return LexerRuleReturnState.IgnoreToken;
               },
               (SpiceLexerState state, string lexem) =>
               {
                   if (state.InCommentBlock)
                   {
                       return LexerRuleHandleState.Use;
                   }

                   return LexerRuleHandleState.Next;
               }));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
              (int)SpiceTokenType.EQUAL,
              "An equal character",
              @"="));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.NEWLINE,
                "A new line characters",
                @"(\r\n|\n|\r)",
                (SpiceLexerState state, string lexem) =>
                {
                    state.LineNumber++;

                    if (state.InCommentBlock)
                    {
                        return LexerRuleReturnState.IgnoreToken;
                    }

                    return LexerRuleReturnState.ReturnToken;
                }));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.CONTINUE,
                "A continuation token",
                @"((\r\n\+|\n\+|\r\+|\\\r|\\\n|\\\r\n))",
                (SpiceLexerState state, string lexem) =>
                {
                    state.LineNumber++;
                    return LexerRuleReturnState.IgnoreToken;
                }));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.ENDS,
                ".ENDS keyword",
                ".ENDS",
                ignoreCase: options.IgnoreCaseDotStatements));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.END,
                ".END keyword",
                ".END",
                ignoreCase: options.IgnoreCaseDotStatements));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
               (int)SpiceTokenType.ENDL,
               ".ENDL keyword",
               ".ENDL",
               ignoreCase: options.IgnoreCaseDotStatements));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
              (int)SpiceTokenType.BOOLEAN_EXPRESSION,
              "An boolean expression token",
              @"\(.*\)",
              null,
              (SpiceLexerState state, string lexem) =>
               {
                   if (state.PreviousReturnedTokenType == (int)SpiceTokenType.IF
                   || state.PreviousReturnedTokenType == (int)SpiceTokenType.ELSE_IF)
                   {
                       return LexerRuleHandleState.Use;
                   }

                   return LexerRuleHandleState.Next;
               }));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
              (int)SpiceTokenType.IF,
              ".IF keyword",
              ".IF",
              ignoreCase: options.IgnoreCaseDotStatements));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
              (int)SpiceTokenType.ENDIF,
              ".ENDIF keyword",
              ".ENDIF",
              ignoreCase: options.IgnoreCaseDotStatements));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
              (int)SpiceTokenType.ELSE,
              ".ELSE keyword",
              ".ELSE",
              ignoreCase: options.IgnoreCaseDotStatements));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
              (int)SpiceTokenType.ELSE_IF,
              ".ELSEIF keyword",
              ".ELSEIF",
              ignoreCase: options.IgnoreCaseDotStatements));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
               (int)SpiceTokenType.VALUE,
               "A value with comma seperator",
               @"([+-]?((<DIGIT>)+(,(<DIGIT>)*)?|\.(<DIGIT>)+)(e(\+|-)?(<DIGIT>)+)?[tgmkunpf]?(<LETTER>)*)",
               null,
               (SpiceLexerState state, string lexem) =>
               {
                   if (state.PreviousReturnedTokenType == (int)SpiceTokenType.EQUAL
                    || state.PreviousReturnedTokenType == (int)SpiceTokenType.VALUE)
                   {
                       return LexerRuleHandleState.Use;
                   }

                   return LexerRuleHandleState.Next;
               },
               ignoreCase: true));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.VALUE,
                "A value with dot seperator",
                @"([+-]?((<DIGIT>)+(\.(<DIGIT>)*)?|\.(<DIGIT>)+)(e(\+|-)?(<DIGIT>)+)?[tgmkunpf]?(<LETTER>)*)",
                null,
                (SpiceLexerState state, string lexem) =>
                {
                    return LexerRuleHandleState.Use;
                },
                ignoreCase: true));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
              (int)SpiceTokenType.PERCENT,
              "A percent value with comma seperator",
              @"([+-]?((<DIGIT>)+(,(<DIGIT>)*)?|\.(<DIGIT>)+)(e(\+|-)?(<DIGIT>)+)?[tgmkunpf]?(<LETTER>)*)%",
              null,
              (SpiceLexerState state, string lexem) =>
              {
                  if (state.PreviousReturnedTokenType == (int)SpiceTokenType.EQUAL
                   || state.PreviousReturnedTokenType == (int)SpiceTokenType.VALUE
                   || state.PreviousReturnedTokenType == (int)SpiceTokenType.START)
                  {
                      return LexerRuleHandleState.Use;
                  }

                  return LexerRuleHandleState.Next;
              },
              ignoreCase: true));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.PERCENT,
                "A percent value with dot seperator",
                @"([+-]?((<DIGIT>)+(\.(<DIGIT>)*)?|\.(<DIGIT>)+)(e(\+|-)?(<DIGIT>)+)?[tgmkunpf]?(<LETTER>)*)%",
                null,
                (SpiceLexerState state, string lexem) =>
                {
                    return LexerRuleHandleState.Use;
                },
                ignoreCase: true));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
             (int)SpiceTokenType.COMMENT_HSPICE,
             "A comment - HSpice style",
             @"\$[^\r\n]*",
             (SpiceLexerState state, string lexem) =>
             {
                 return LexerRuleReturnState.IgnoreToken;
             }));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
             (int)SpiceTokenType.COMMENT_PSPICE,
             "A comment - PSpice style",
             @";[^\r\n]*",
             (SpiceLexerState state, string lexem) =>
             {
                 return LexerRuleReturnState.IgnoreToken;
             }));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.COMMENT,
                "A full line comment",
                @"\*[^\r\n]*",
                null,
                (SpiceLexerState state, string lexem) =>
                {
                    if (state.PreviousReturnedTokenType == (int)SpiceTokenType.NEWLINE
                    || (state.LineNumber == 1 && options.HasTitle == false))
                    {
                        return LexerRuleHandleState.Use;
                    }

                    return LexerRuleHandleState.Next;
                },
                ignoreCase: true));

            builder.AddRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceTokenType.DOUBLE_QUOTED_STRING,
                    "A string with double quotation marks",
                    "\"(?:[^\"\\\\]|\\\\.)*\""));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
             (int)SpiceTokenType.EXPRESSION_SINGLE_QUOTES,
             "A mathematical expression in single quotes",
             "'[^']*'",
             null,
             (SpiceLexerState state, string lexem) =>
             {
                 if (state.PreviousReturnedTokenType == (int)SpiceTokenType.EQUAL)
                 {
                     return LexerRuleHandleState.Use;
                 }

                 return LexerRuleHandleState.Next;
             },
             ignoreCase: options.IgnoreCaseDotStatements));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.EXPRESSION_BRACKET,
                "A mathematical expression in brackets",
                "{[^{}]*}",
                ignoreCase: true));

            builder.AddRule(
              new LexerTokenRule<SpiceLexerState>(
                  (int)SpiceTokenType.SINGLE_QUOTED_STRING,
                  "A string with single quotation marks",
                  "'[^']*'"));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.REFERENCE,
                "A reference",
                "@(<CHARACTER>(<CHARACTER>|<SPECIAL>)*)",
                null,
                (SpiceLexerState state, string lexem) =>
                {
                    if (state.LexerOptions.CurrentLineContinuationCharacter.HasValue
                        && lexem.EndsWith(state.LexerOptions.CurrentLineContinuationCharacter.Value.ToString(), System.StringComparison.Ordinal)
                        && state.BeforeLineBreak)
                    {
                        return LexerRuleHandleState.Next;
                    }

                    return LexerRuleHandleState.Use;
                },
                ignoreCase: true));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.WORD,
                "A word",
                "(<LETTER>(<CHARACTER>|<SPECIAL>)*)",
                null,
                (SpiceLexerState state, string lexem) =>
                {
                    if (state.LexerOptions.CurrentLineContinuationCharacter.HasValue
                        && lexem.EndsWith(state.LexerOptions.CurrentLineContinuationCharacter.Value.ToString(), System.StringComparison.Ordinal)
                        && state.BeforeLineBreak)
                    {
                        return LexerRuleHandleState.Next;
                    }

                    return LexerRuleHandleState.Use;
                },
                ignoreCase: true));

            builder.AddRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceTokenType.IDENTIFIER,
                    "An identifier",
                    "((<CHARACTER>|_|\\*)(<CHARACTER>|<SPECIAL>)*)",
                    null,
                    (SpiceLexerState state, string lexem) =>
                    {
                        if (state.LexerOptions.CurrentLineContinuationCharacter.HasValue
                            && lexem.EndsWith(state.LexerOptions.CurrentLineContinuationCharacter.Value.ToString(), System.StringComparison.Ordinal)
                            && state.BeforeLineBreak)
                        {
                            return LexerRuleHandleState.Next;
                        }

                        return LexerRuleHandleState.Use;
                    },
                    ignoreCase: true));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.REFERENCE,
                "A reference (without ending backslash)",
                "@(<CHARACTER>(<CHARACTER>|<SPECIAL>)*(<CHARACTER>|<SPECIAL_WITHOUT_BACKSLASH>)+)",
                ignoreCase: true));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.WORD,
                "A word (without ending backslash)",
                "(<LETTER>(<CHARACTER>|<SPECIAL>)*(<CHARACTER>|<SPECIAL_WITHOUT_BACKSLASH>)+)",
                ignoreCase: true));

            builder.AddRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceTokenType.IDENTIFIER,
                    "An identifier (without ending backslash)",
                    "((<CHARACTER>|_|\\*)(<CHARACTER>|<SPECIAL>)*(<CHARACTER>|<SPECIAL_WITHOUT_BACKSLASH>)+)",
                    ignoreCase: true));

            builder.AddRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.ASTERIKS,
                "An asteriks character",
                "\\*"));

            grammar = builder.GetGrammar();
        }
    }
}
