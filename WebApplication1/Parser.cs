﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Sle.Parser {
    class LEParseException : Exception {
        public readonly int Position = -1;
        public readonly String Current;
        public LEParseException(String expected, int position, String current)
            : base(LEParser.m_input+": " + expected +" is expected at " + position + " position but '" + current + "' is encouneterd." + graph(position)) {
            Position = position;
            Current = current;
        }

        public LEParseException(String expected, int position, char current)
            : this(expected, position, new String(current, 1)) { }

        public LEParseException(String likeTerm)
            : base(LEParser.m_input + ": a like term " + likeTerm + " is encountered. You should combine like terms manually.") { }

        public static string graph(int pos) {
            StringBuilder sb = new StringBuilder();
            sb.Append("<br/>");
            for (int i = 0; i < pos; i++) {
                sb.Append(LEParser.m_input[i]);
            }
            sb.Append("<span style=\"color:red;font-weight: bold; font-style: italic; text-decoration: underline; \">"+
                (pos >= LEParser.m_input.Length ? ' '  : LEParser.m_input[pos])
                +"</span>");
            return sb.ToString();
        }

    }

    public class LEParser {

        private const string END_OF_LINE = "EOL";
        private const string DIGIT_LETER = "DIGIT OR LETTER";
        private const string SIGN = "SIGN";
        private const string AT_LEAST_ONE_LETTER_SIGN = "AT LEAST 1 LETTER OR SIGN";
        private const char DOT = '.';

        private static IDictionary<String, double> m_terms = new Dictionary<String, double>();
        private static bool m_EOLIsOk = false;
        public static String m_input; //TODO change to private instance
        private static int POS = 0;
        private static void skipWs() {
            while (POS < m_input.Length && Char.IsWhiteSpace(m_input[POS])) {
                POS++;
            }
        }

        private static bool tryParseSign(char signChar, out double sign) {
            sign = 1;
            if (signChar == '+' || signChar == '-') {
                if (signChar == '-') {
                    sign = -1;
                }
                readNextChar();
                return true;
            }
            return false;
        }

        private static char readNextChar() {
            return m_input[POS++];
        }

        private static char peek(String expected) {
            try {
                return m_input[POS];
            } catch (IndexOutOfRangeException e) {
                if (m_EOLIsOk) {                  
                    return ' ';
                }
                throw new LEParseException(expected, POS, END_OF_LINE);
            }
        }

        private static void addTerm(Tuple<double, string> next) {
            try {
                m_terms.Add(next.Item2, next.Item1);
            } catch (ArgumentException e) {
                throw new LEParseException(next.Item1 + next.Item2);
            }                              
        }

        private static void init(string input) {
            POS = 0; m_EOLIsOk = false;
            m_input = input;
            m_terms.Clear();
        }

        public static Tuple<SortedDictionary<String, double>, double> parse(String input) {
            init(input);
            skipWs();
            while (peek("=") != '=') {
                addTerm(parseTerm());
                skipWs();
            }
            readNextChar();// read '='
            skipWs();
            m_EOLIsOk = true;
            double free = parseCoefficient(canSkipSign: true,atLeastOne: true,oneIfEmpty: false);
            //TODO we can have extra unparsed characters at the end of input           
            return new Tuple<SortedDictionary<string, double>, double>(new SortedDictionary<String, double> (m_terms), free);
        }

        private static Tuple<double, String> parseTerm() {
            double coefficient;
            coefficient = parseCoefficient(canSkipSign: m_terms.Count == 0, atLeastOne: false, oneIfEmpty: true);
            if (coefficient == 0) {
                //TODO exception or ignore ?
            }
            var name = parseName();
            return new Tuple<double, string>(coefficient, name);
        }

        private static double parseCoefficient(bool canSkipSign, bool atLeastOne, bool oneIfEmpty) {
            double sign; double coefficient = 0;
            char signChar = peek(DIGIT_LETER);
            if ((tryParseSign(signChar, out sign)) || canSkipSign) {
                skipWs();
                var chars = parseGroup(DIGIT_LETER, Char.IsDigit, atLeastOne);
                if (oneIfEmpty && chars.Count == 0) {
                    coefficient = 1;
                } else {
                    coefficient = Double.Parse(new String(chars.ToArray<char>()));
                    if (peek("FRACTION") == DOT) {
                        readNextChar();
                        chars = parseGroup("DIGIT", Char.IsDigit, atLeastOne : true);
                        chars.AddFirst(DOT);
                        coefficient += Double.Parse(new String(chars.ToArray<char>()));
                    }
                }
                return sign * coefficient;
            } else {
                throw new LEParseException(SIGN, POS, signChar);
            }
        }

        private static string parseName() {
            return new String(parseGroup(AT_LEAST_ONE_LETTER_SIGN, Char.IsLetter, atLeastOne: true).ToArray<char>());
        }

        private static LinkedList<char> parseGroup(string expected, Func<char, bool> charPredicate, bool atLeastOne) {
            LinkedList<char> chars = new LinkedList<char>();
            if (atLeastOne) {
                char groupChar = peek(expected);
                if (!charPredicate(groupChar)) {
                    throw new LEParseException(expected, POS, groupChar);
                }
                chars.AddLast(readNextChar());
            }
            while (charPredicate(peek(expected))) {
                chars.AddLast(readNextChar());
            }
            return chars;

        }






        class Program {
            static void Main(string[] args) {

                  Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator = ".";
                //Console.SetWindowSize(100, 100);

                Console.WriteLine(Double.Parse(".05"));


                while (true) {
                    var tokenizer = new LEParser();
                    Console.WriteLine("in");



                    try {
                        var result = LEParser.parse(Console.ReadLine());
                        foreach (var cur in result.Item1.Keys) {
                            Console.Write(result.Item1[cur] + cur);
                            Console.Write(" ");
                        }
                        Console.WriteLine("free: " + result.Item2);
                    } catch (Exception e) {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }
                }

                // GaussianElimination.main(null);

                Console.ReadLine();

            }
        }
    }
}
