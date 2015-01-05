using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace Sle {
    public static class MathMlWriter {
        private static StringBuilder m_mathMl = new StringBuilder();

        private static string ML_PLUS = "<mo>&#43;<!--PLUS--></mo>";
        private static string ML_MINUS = "<mo>&#45;<!--MINU--></mo>";
        private static string ML_EQUALS = "<mo>&#61;<!--EQUALS--></mo>";
        private static string ML_NAME_OPEN = "<mn>";
        private static string ML_NAME_CLOSE = "</mn>";
        private static string ML_NUMBER_OPEN = "<mi>";
        private static string ML_NUMBER_CLOSE = "</mi>";
        private static string ML_START = "<mtr><mtd>";
        private static string ML_END = "</mtr></mtd>";

        public static string wrapInTable(string rows) {
            return "<math xmlns=\"http://www.w3.org/1998/Math/MathML\"><mo>&#123;</mo><mtable>" + rows + "</mtable></math>";
        }

        public static void init() {
            m_mathMl.Clear();
        }

        public static void writeStart() {
            m_mathMl.Append(ML_START);
        }

        public static string getMathMl() {          
            return m_mathMl.ToString();
           
        }

        public static void writeEnd() {
            m_mathMl.Append(ML_END);
        }

        public static void writeSign(bool plus) {
            m_mathMl.Append(plus ? ML_PLUS : ML_MINUS);
        }

        public static void writeNumber(double number) {
            m_mathMl.Append(ML_NUMBER_OPEN);
            m_mathMl.Append(Math.Abs(number));
            m_mathMl.Append(ML_NUMBER_CLOSE);
        }

        public static void writeName(string name) {
            m_mathMl.Append(ML_NAME_OPEN);
            m_mathMl.Append(name);
            m_mathMl.Append(ML_NAME_CLOSE);
        }

        public static void writeEquals() {
            m_mathMl.Append(ML_EQUALS);
        }
    }
}