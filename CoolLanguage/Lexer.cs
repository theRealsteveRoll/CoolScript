﻿using System;
using System.Collections.Generic;

namespace CoolLanguage
{

    class Lexer
    {
        static Dictionary<string, bool> keywords = new Dictionary<string, bool>
        {
            {"var", true},
            {"function", true },
            {"return", true },

            {"if", true },
            {"else", true },
            {"while", true },

            {"true", true },
            {"false", true },
            {"null", true },
        };

        private int currentPos = 0;

        private string code;

        private int currentLine = 1;

        public bool reachedEnd { get; private set; } = false;

        static bool isNameChar(char c)
        {
            return char.IsLetter(c) || c == '_';
        }

        static bool isAlNum(char c)
        {
            return char.IsDigit(c) || isNameChar(c);
        }

        static bool isBinaryOperator(string c)
        {
            switch (c)
            {
                case "+": case "-": case "*": case "/": case "^": case "%":
                case "..":
                case "&&": case "||":
                case "==": case "!=": case ">": case "<": case ">=": case "<=":
                    return true;
            }
            return false;
        }

        static bool isUnaryOperator(string c)
        {
            switch (c)
            {
                case "!":
                    return true;
            }
            return false;
        }

        static Dictionary<string, bool> operatorGroups = new Dictionary<string, bool>
        {
            {"&&", true },
            {"||", true },
            {"==", true },
            {"!=", true },
            {">=", true },
            {"<=", true },
            {"..", true }
        };

        static Dictionary<char, char> escapeChars = new Dictionary<char, char>
        {
            {'"', '"' },
            {'\'', '\'' },
            {'n', '\n' },
        };

        public Lexer(string theCode)
        {
            code = theCode;
            if (code.Length == 0)
            {
                reachedEnd = true;
            }
        }

        private char currentChar
        {
            get => currentPos < code.Length ? code[currentPos] : '\0';
        }

        public Token nextToken()
        {
            TokenType doing = TokenType.None;
            string tokenValue = "";

            if (currentPos == code.Length)
            {
                reachedEnd = true;
            }

            if (reachedEnd)
            {
                doing = TokenType.EndOfFile;
            }
            else
            {
                while (char.IsWhiteSpace(code, currentPos) || currentChar == '\r' || currentChar == '\n') // get past all the spaces and newlines
                {
                    if (currentChar == '\n')
                        currentLine++;

                    currentPos++;

                    if (currentPos == code.Length)
                    {
                        doing = TokenType.EndOfFile;
                        reachedEnd = true;
                        break;
                    }
                }

                if (doing != TokenType.EndOfFile)
                {
                    if (isAlNum(currentChar))
                    {
                        doing = char.IsDigit(code, currentPos) ? TokenType.Number : TokenType.Identifier;

                        do
                        {
                            tokenValue += currentChar;
                            currentPos++;
                        } while (currentPos < code.Length && (isAlNum(currentChar) || (doing == TokenType.Number && currentChar == '.')));

                        if (keywords.ContainsKey(tokenValue))
                        {
                            doing = TokenType.Keyword;
                        }
                    }
                    else if (currentChar == '"' || currentChar == '\'')
                    {
                        char stringStart = currentChar;

                        doing = TokenType.String;

                        //Console.WriteLine("string start");

                        while (true)
                        {
                            currentPos++;
                            //Console.WriteLine(currentPos);
                            if (currentPos >= code.Length)
                            {
                                throw new SyntaxErrorException(currentLine, "unfinished string");
                            }
                            else if (currentChar == stringStart)
                            {
                                currentPos++;
                                break;
                            }
                            else if (currentChar == '\\')
                            {
                                currentPos++;
                                if (escapeChars.TryGetValue(currentChar, out char escapeChar))
                                {
                                    tokenValue += escapeChar;
                                }
                                else
                                {
                                    throw new SyntaxErrorException(currentLine, "invalid escape sequence");
                                }
                            }
                            else
                            {
                                tokenValue += currentChar;
                                //Console.WriteLine("!" + tokenValue);
                            }
                        }
                    }
                    else
                    {
                        do
                        {
                            tokenValue += currentChar;
                            currentPos++;
                        } while (operatorGroups.ContainsKey(code[currentPos - 1].ToString() + currentChar));

                        doing = isUnaryOperator(tokenValue) ? TokenType.UnaryOperator : (isBinaryOperator(tokenValue) ? TokenType.BinaryOperator : TokenType.Punctuation);
                    }
                }
            }

            return new Token(doing, tokenValue, currentLine);
        }

        private char Peek()
        {
            return currentPos < code.Length - 1 ? code[currentPos + 1] : '\0';
        }
    }
}
