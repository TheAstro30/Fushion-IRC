//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE.
//
//  License: GNU Lesser General Public License (LGPLv3)
//
//  Email: pavel_torgashov@ukr.net
//
//  Copyright (C) Pavel Torgashov, 2011-2016.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using ircScript.Controls.SyntaxHighlight.Helpers;
using ircScript.Controls.SyntaxHighlight.Highlight.Descriptor;
using ircScript.Controls.SyntaxHighlight.Styles;

namespace ircScript.Controls.SyntaxHighlight.Highlight
{
    public enum Language
    {
        Custom = 0,
        CSharp = 1,
        Vb = 2,
        Html = 3,
        Xml = 4,
        Sql = 5,
        Php = 6,
        Js = 7,
        Lua = 8
    }

    public class SyntaxHighlighter : IDisposable
    {
        /* Nested type: XmlFoldingTag */
        private class XmlFoldingTag
        {
            public string Name { private get; set; }
            public int Id { private get; set; }
            public int StartLine { get; set; }

            public string Marker
            {
                get { return Name + Id; }
            }
        }

        /* Styles */
        protected static readonly Platform PlatformType = Helpers.PlatformType.GetOperationSystemPlatform();
        public readonly Style BlackStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);
        public readonly Style BlueBoldStyle = new TextStyle(Brushes.Blue, null, FontStyle.Bold);
        public readonly Style BlueStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
        public readonly Style BoldStyle = new TextStyle(null, null, FontStyle.Bold | FontStyle.Underline);
        public readonly Style BrownStyle = new TextStyle(Brushes.Brown, null, FontStyle.Italic);

        protected readonly Dictionary<string, SyntaxDescriptor> DescByXmLfileNames =
            new Dictionary<string, SyntaxDescriptor>();

        public readonly Style GrayStyle = new TextStyle(Brushes.Gray, null, FontStyle.Regular);
        public readonly Style GreenStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);
        public readonly Style MagentaStyle = new TextStyle(Brushes.Magenta, null, FontStyle.Regular);
        public readonly Style MaroonStyle = new TextStyle(Brushes.Maroon, null, FontStyle.Regular);
        public readonly Style RedStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);

        protected readonly List<Style> ResilientStyles = new List<Style>(5);

        protected Regex CSharpAttributeRegex,
                        CSharpClassNameRegex;

        protected Regex CSharpCommentRegex1,
                        CSharpCommentRegex2,
                        CSharpCommentRegex3;

        protected Regex CSharpKeywordRegex;
        protected Regex CSharpNumberRegex;
        protected Regex CSharpStringRegex;

        protected Regex HtmlAttrRegex,
                        HtmlAttrValRegex,
                        HtmlCommentRegex1,
                        HtmlCommentRegex2;

        protected Regex HtmlEndTagRegex;

        protected Regex HtmlEntityRegex,
                        HtmlTagContentRegex;

        protected Regex HtmlTagNameRegex;
        protected Regex HtmlTagRegex;

        protected Regex JScriptCommentRegex1,
                        JScriptCommentRegex2,
                        JScriptCommentRegex3;

        protected Regex JScriptKeywordRegex;
        protected Regex JScriptNumberRegex;
        protected Regex JScriptStringRegex;

        protected Regex LuaCommentRegex1,
                        LuaCommentRegex2,
                        LuaCommentRegex3;

        protected Regex LuaFunctionsRegex;

        protected Regex LuaKeywordRegex;
        protected Regex LuaNumberRegex;
        protected Regex LuaStringRegex;

        protected Regex PhpCommentRegex1,
                        PhpCommentRegex2,
                        PhpCommentRegex3;

        protected Regex PhpKeywordRegex1,
                        PhpKeywordRegex2,
                        PhpKeywordRegex3;

        protected Regex PhpNumberRegex;
        protected Regex PhpStringRegex;
        protected Regex PhpVarRegex;

        protected Regex SqlCommentRegex1,
                        SqlCommentRegex2,
                        SqlCommentRegex3,
                        SqlCommentRegex4;

        protected Regex SqlFunctionsRegex;
        protected Regex SqlKeywordsRegex;
        protected Regex SqlNumberRegex;
        protected Regex SqlStatementsRegex;
        protected Regex SqlStringRegex;
        protected Regex SqlTypesRegex;
        protected Regex SqlVarRegex;
        protected Regex VbClassNameRegex;
        protected Regex VbCommentRegex;
        protected Regex VbKeywordRegex;
        protected Regex VbNumberRegex;
        protected Regex VbStringRegex;

        protected Regex XmlAttrRegex,
                        XmlAttrValRegex;

        protected Regex XmlcDataRegex;

        protected Regex XmlCommentRegex1,
                        XmlCommentRegex2;

        protected Regex XmlEndTagRegex;

        protected Regex XmlEntityRegex;
        protected Regex XmlFoldingRegex;
        protected Regex XmlTagContentRegex;

        protected Regex XmlTagNameRegex;
        protected Regex XmlTagRegex;

        protected FastColoredTextBox CurrentTextBox;


        /* Styles */
        public Style StringStyle { get; set; }
        public Style CommentStyle { get; set; }
        public Style NumberStyle { get; set; }
        public Style AttributeStyle { get; set; }
        public Style ClassNameStyle { get; set; }
        public Style KeywordStyle { get; set; }
        public Style CommentTagStyle { get; set; }
        public Style AttributeValueStyle { get; set; }
        public Style TagBracketStyle { get; set; }
        public Style TagNameStyle { get; set; }
        public Style HtmlEntityStyle { get; set; }
        public Style XmlAttributeStyle { get; set; }
        public Style XmlAttributeValueStyle { get; set; }
        public Style XmlTagBracketStyle { get; set; }
        public Style XmlTagNameStyle { get; set; }
        public Style XmlEntityStyle { get; set; }
        public Style XmlCDataStyle { get; set; }
        public Style VariableStyle { get; set; }
        public Style KeywordStyle2 { get; set; }
        public Style KeywordStyle3 { get; set; }
        public Style StatementsStyle { get; set; }
        public Style FunctionsStyle { get; set; }
        public Style TypesStyle { get; set; }

        public SyntaxHighlighter(FastColoredTextBox currentTextBox)
        {
            CurrentTextBox = currentTextBox;
        }

        public static RegexOptions RegexCompiledOption
        {
            get {
                return PlatformType == Platform.X86 ? RegexOptions.Compiled : RegexOptions.None;
            }
        }
        public void Dispose()
        {
            foreach (var desc in DescByXmLfileNames.Values)
            {
                desc.Dispose();
            }
        }

        public virtual void HighlightSyntax(Language language, Range range)
        {
            switch (language)
            {
                case Language.CSharp:
                    CSharpSyntaxHighlight(range);
                    break;

                case Language.Vb:
                    VbSyntaxHighlight(range);
                    break;

                case Language.Html:
                    HtmlSyntaxHighlight(range);
                    break;

                case Language.Xml:
                    XmlSyntaxHighlight(range);
                    break;

                case Language.Sql:
                    SqlSyntaxHighlight(range);
                    break;

                case Language.Php:
                    PhpSyntaxHighlight(range);
                    break;

                case Language.Js:
                    JScriptSyntaxHighlight(range);
                    break;

                case Language.Lua:
                    LuaSyntaxHighlight(range);
                    break;
            }
        }

        public virtual void HighlightSyntax(string xmLdescriptionFile, Range range)
        {
            SyntaxDescriptor desc;
            if (!DescByXmLfileNames.TryGetValue(xmLdescriptionFile, out desc))
            {
                var doc = new XmlDocument();
                var file = xmLdescriptionFile;
                if (!File.Exists(file))
                {
                    var path = Path.GetFileName(file);
                    if (path != null)
                    {
                        file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                    }
                }
                doc.LoadXml(File.ReadAllText(file));
                desc = ParseXmlDescription(doc);
                DescByXmLfileNames[xmLdescriptionFile] = desc;
            }
            HighlightSyntax(desc, range);
        }

        public virtual void AutoIndentNeeded(object sender, AutoIndentEventArgs args)
        {
            var tb = (sender as FastColoredTextBox);
            if (tb == null)
            {
                return;
            }
            var language = tb.Language;
            switch (language)
            {
                case Language.CSharp:
                    CSharpAutoIndentNeeded(sender, args);
                    break;

                case Language.Vb:
                    VbAutoIndentNeeded(sender, args);
                    break;

                case Language.Html:
                    HtmlAutoIndentNeeded(sender, args);
                    break;

                case Language.Xml:
                    XmlAutoIndentNeeded(sender, args);
                    break;

                case Language.Sql:
                    SqlAutoIndentNeeded(sender, args);
                    break;
                case Language.Php:
                    PhpAutoIndentNeeded(sender, args);
                    break;
                case Language.Js:
                    CSharpAutoIndentNeeded(sender, args);
                    break; /* JS like C# */

                case Language.Lua:
                    LuaAutoIndentNeeded(sender, args);
                    break;
            }
        }

        protected void PhpAutoIndentNeeded(object sender, AutoIndentEventArgs args)
        {
            /* Block {} */
            if (Regex.IsMatch(args.LineText, @"^[^""']*\{.*\}[^""']*$"))
            {
                return;
            }
            /* Start of block {} */
            if (Regex.IsMatch(args.LineText, @"^[^""']*\{"))
            {
                args.ShiftNextLines = args.TabLength;
                return;
            }
            /* End of block {} */
            if (Regex.IsMatch(args.LineText, @"}[^""']*$"))
            {
                args.Shift = -args.TabLength;
                args.ShiftNextLines = -args.TabLength;
                return;
            }
            /* Is unclosed operator in previous line ? */
            if (Regex.IsMatch(args.PrevLineText, @"^\s*(if|for|foreach|while|[\}\s]*else)\b[^{]*$"))
            {
                if (!Regex.IsMatch(args.PrevLineText, @"(;\s*$)|(;\s*//)")) /* Operator is unclosed */
                {
                    args.Shift = args.TabLength;
                    return;
                }
            }
        }

        protected void SqlAutoIndentNeeded(object sender, AutoIndentEventArgs args)
        {
            var tb = sender as FastColoredTextBox;
            if (tb != null)
            {
                tb.CalcAutoIndentShiftByCodeFolding(sender, args);
            }
        }

        protected void HtmlAutoIndentNeeded(object sender, AutoIndentEventArgs args)
        {
            var tb = sender as FastColoredTextBox;
            if (tb != null)
            {
                tb.CalcAutoIndentShiftByCodeFolding(sender, args);
            }
        }

        protected void XmlAutoIndentNeeded(object sender, AutoIndentEventArgs args)
        {
            var tb = sender as FastColoredTextBox;
            if (tb != null)
            {
                tb.CalcAutoIndentShiftByCodeFolding(sender, args);
            }
        }

        protected void VbAutoIndentNeeded(object sender, AutoIndentEventArgs args)
        {
            /* End of block */
            if (Regex.IsMatch(args.LineText, @"^\s*(End|EndIf|Next|Loop)\b", RegexOptions.IgnoreCase))
            {
                args.Shift = -args.TabLength;
                args.ShiftNextLines = -args.TabLength;
                return;
            }
            /* Start of declaration */
            if (Regex.IsMatch(args.LineText,
                              @"\b(Class|Property|Enum|Structure|Sub|Function|Namespace|Interface|Get)\b|(Set\s*\()",
                              RegexOptions.IgnoreCase))
            {
                args.ShiftNextLines = args.TabLength;
                return;
            }
            /* Then ... */
            if (Regex.IsMatch(args.LineText, @"\b(Then)\s*\S+", RegexOptions.IgnoreCase))
            {
                return;
            }
            /* Start of operator block */
            if (Regex.IsMatch(args.LineText, @"^\s*(If|While|For|Do|Try|With|Using|Select)\b", RegexOptions.IgnoreCase))
            {
                args.ShiftNextLines = args.TabLength;
                return;
            }
            /* Statements else, elseif, case etc */
            if (Regex.IsMatch(args.LineText, @"^\s*(Else|ElseIf|Case|Catch|Finally)\b", RegexOptions.IgnoreCase))
            {
                args.Shift = -args.TabLength;
                return;
            }
            /* Char _ */
            if (!args.PrevLineText.TrimEnd().EndsWith("_"))
            {
                return;
            }
            args.Shift = args.TabLength;
            return;
        }

        protected void CSharpAutoIndentNeeded(object sender, AutoIndentEventArgs args)
        {
            /* Block {} */
            if (Regex.IsMatch(args.LineText, @"^[^""']*\{.*\}[^""']*$"))
            {
                return;
            }
            /* Start of block {} */
            if (Regex.IsMatch(args.LineText, @"^[^""']*\{"))
            {
                args.ShiftNextLines = args.TabLength;
                return;
            }
            /* End of block {} */
            if (Regex.IsMatch(args.LineText, @"}[^""']*$"))
            {
                args.Shift = -args.TabLength;
                args.ShiftNextLines = -args.TabLength;
                return;
            }
            /* Label */
            if (Regex.IsMatch(args.LineText, @"^\s*\w+\s*:\s*($|//)") &&
                !Regex.IsMatch(args.LineText, @"^\s*default\s*:"))
            {
                args.Shift = -args.TabLength;
                return;
            }
            /* Some statements: case, default */
            if (Regex.IsMatch(args.LineText, @"^\s*(case|default)\b.*:\s*($|//)"))
            {
                args.Shift = -args.TabLength/2;
                return;
            }
            /* Is unclosed operator in previous line ? */
            if (!Regex.IsMatch(args.PrevLineText, @"^\s*(if|for|foreach|while|[\}\s]*else)\b[^{]*$"))
            {
                return;
            }
            if (Regex.IsMatch(args.PrevLineText, @"(;\s*$)|(;\s*//)"))
            {
                return;
            }
            args.Shift = args.TabLength;
            return;
        }

        public virtual void AddXmlDescription(string descriptionFileName, XmlDocument doc)
        {
            SyntaxDescriptor desc = ParseXmlDescription(doc);
            DescByXmLfileNames[descriptionFileName] = desc;
        }

        public virtual void AddResilientStyle(Style style)
        {
            if (ResilientStyles.Contains(style)) return;
            CurrentTextBox.CheckStylesBufferSize(); /* Prevent buffer overflow */
            ResilientStyles.Add(style);
        }

        public static SyntaxDescriptor ParseXmlDescription(XmlDocument doc)
        {
            var desc = new SyntaxDescriptor();
            var brackets = doc.SelectSingleNode("doc/brackets");
            if (brackets != null && brackets.Attributes != null)
            {
                if (brackets.Attributes["left"] == null || brackets.Attributes["right"] == null ||
                    brackets.Attributes["left"].Value == "" || brackets.Attributes["right"].Value == "")
                {
                    desc.LeftBracket = '\x0';
                    desc.RightBracket = '\x0';
                }
                else
                {
                    desc.LeftBracket = brackets.Attributes["left"].Value[0];
                    desc.RightBracket = brackets.Attributes["right"].Value[0];
                }
                if (brackets.Attributes["left2"] == null || brackets.Attributes["right2"] == null ||
                    brackets.Attributes["left2"].Value == "" || brackets.Attributes["right2"].Value == "")
                {
                    desc.LeftBracket2 = '\x0';
                    desc.RightBracket2 = '\x0';
                }
                else
                {
                    desc.LeftBracket2 = brackets.Attributes["left2"].Value[0];
                    desc.RightBracket2 = brackets.Attributes["right2"].Value[0];
                }

                if (brackets.Attributes["strategy"] == null || brackets.Attributes["strategy"].Value == "")
                {
                    desc.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy2;
                }
                else
                {
                    desc.BracketsHighlightStrategy =
                        (BracketsHighlightStrategy)
                        Enum.Parse(typeof (BracketsHighlightStrategy), brackets.Attributes["strategy"].Value);
                }
            }
            var styleByName = new Dictionary<string, Style>();

            var xmlNodeList = doc.SelectNodes("doc/style");
            if (xmlNodeList != null)
            {
                foreach (XmlNode style in xmlNodeList)
                {
                    var s = ParseStyle(style);
                    if (style.Attributes != null)
                    {
                        styleByName[style.Attributes["name"].Value] = s;
                    }
                    desc.Styles.Add(s);
                }
                var selectNodes = doc.SelectNodes("doc/rule");
                if (selectNodes != null)
                {
                    foreach (XmlNode rule in selectNodes)
                    {
                        desc.Rules.Add(ParseRule(rule, styleByName));
                    }
                }
                var nodeList = doc.SelectNodes("doc/folding");
                if (nodeList != null)
                {
                    foreach (XmlNode folding in nodeList)
                    {
                        desc.Foldings.Add(ParseFolding(folding));
                    }
                }
            }
            return desc;
        }

        protected static FoldingDesc ParseFolding(XmlNode foldingNode)
        {
            if (foldingNode.Attributes != null)
            {
                var folding = new FoldingDesc
                                  {
                                      StartMarkerRegex = foldingNode.Attributes["start"].Value,
                                      FinishMarkerRegex = foldingNode.Attributes["finish"].Value
                                  };
                var optionsA = foldingNode.Attributes["options"];
                if (optionsA != null)
                {
                    folding.Options = (RegexOptions) Enum.Parse(typeof (RegexOptions), optionsA.Value);
                }
                return folding;
            }
            return null;
        }

        protected static RuleDesc ParseRule(XmlNode ruleNode, Dictionary<string, Style> styles)
        {
            var rule = new RuleDesc
                           {
                               Pattern = ruleNode.InnerText
                           };
            if (ruleNode.Attributes != null)
            {
                var styleA = ruleNode.Attributes["style"];
                var optionsA = ruleNode.Attributes["options"];
                if (styleA == null)
                {
                    throw new Exception("Rule must contain style name.");
                }
                if (!styles.ContainsKey(styleA.Value))
                {
                    throw new Exception("Style '" + styleA.Value + "' is not found.");
                }
                rule.Style = styles[styleA.Value];
                if (optionsA != null)
                {
                    rule.Options = (RegexOptions) Enum.Parse(typeof (RegexOptions), optionsA.Value);
                }
            }
            return rule;
        }

        protected static Style ParseStyle(XmlNode styleNode)
        {
            if (styleNode.Attributes != null)
            {
                var colorA = styleNode.Attributes["color"];
                var backColorA = styleNode.Attributes["backColor"];
                var fontStyleA = styleNode.Attributes["fontStyle"];
                /* Colors */
                SolidBrush foreBrush = null;
                if (colorA != null)
                {
                    foreBrush = new SolidBrush(ParseColor(colorA.Value));
                }
                SolidBrush backBrush = null;
                if (backColorA != null)
                {
                    backBrush = new SolidBrush(ParseColor(backColorA.Value));
                }
                /* FontStyle */
                var fontStyle = FontStyle.Regular;
                if (fontStyleA != null)
                {
                    fontStyle = (FontStyle) Enum.Parse(typeof (FontStyle), fontStyleA.Value);
                }
                return new TextStyle(foreBrush, backBrush, fontStyle);
            }
            return new TextStyle(null, null, FontStyle.Regular);
        }

        protected static Color ParseColor(string s)
        {
            if (!s.StartsWith("#"))
            {
                return Color.FromName(s);
            }
            if (s.Length <= 7)
            {
                return Color.FromArgb(255,
                                      Color.FromArgb(Int32.Parse(s.Substring(1), NumberStyles.AllowHexSpecifier)));
            }
            return Color.FromArgb(Int32.Parse(s.Substring(1), NumberStyles.AllowHexSpecifier));
        }

        public void HighlightSyntax(SyntaxDescriptor desc, Range range)
        {
            /* Set style order */
            range.tb.ClearStylesBuffer();
            for (var i = 0; i < desc.Styles.Count; i++)
            {
                range.tb.Styles[i] = desc.Styles[i];
            }
            /* Add resilient styles */
            var l = desc.Styles.Count;
            for (var i = 0; i < ResilientStyles.Count; i++)
            {
                range.tb.Styles[l + i] = ResilientStyles[i];
            }
            /* Brackets */
            char[] oldBrackets = RememberBrackets(range.tb);
            range.tb.LeftBracket = desc.LeftBracket;
            range.tb.RightBracket = desc.RightBracket;
            range.tb.LeftBracket2 = desc.LeftBracket2;
            range.tb.RightBracket2 = desc.RightBracket2;
            /* Clear styles of range */
            range.ClearStyle(desc.Styles.ToArray());
            /* Highlight syntax */
            foreach (RuleDesc rule in desc.Rules)
            {
                range.SetStyle(rule.Style, rule.Regex);
            }
            /* Clear folding */
            range.ClearFoldingMarkers();
            /* Folding markers */
            foreach (var folding in desc.Foldings)
            {
                range.SetFoldingMarkers(folding.StartMarkerRegex, folding.FinishMarkerRegex, folding.Options);
            }
            RestoreBrackets(range.tb, oldBrackets);
        }

        protected void RestoreBrackets(FastColoredTextBox tb, char[] oldBrackets)
        {
            tb.LeftBracket = oldBrackets[0];
            tb.RightBracket = oldBrackets[1];
            tb.LeftBracket2 = oldBrackets[2];
            tb.RightBracket2 = oldBrackets[3];
        }

        protected char[] RememberBrackets(FastColoredTextBox tb)
        {
            return new[] {tb.LeftBracket, tb.RightBracket, tb.LeftBracket2, tb.RightBracket2};
        }

        protected void InitCShaprRegex()
        {
            CSharpStringRegex =
                new Regex(
                    @"
                            # Character definitions:
                            '
                            (?> # disable backtracking
                              (?:
                                \\[^\r\n]|    # escaped meta char
                                [^'\r\n]      # any character except '
                              )*
                            )
                            '?
                            |
                            # Normal string & verbatim strings definitions:
                            (?<verbatimIdentifier>@)?         # this group matches if it is an verbatim string
                            ""
                            (?> # disable backtracking
                              (?:
                                # match and consume an escaped character including escaped double quote ("") char
                                (?(verbatimIdentifier)        # if it is a verbatim string ...
                                  """"|                         #   then: only match an escaped double quote ("") char
                                  \\.                         #   else: match an escaped sequence
                                )
                                | # OR
            
                                # match any char except double quote char ("")
                                [^""]
                              )*
                            )
                            ""
                        ",
                    RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace |
                    RegexCompiledOption
                    ); /* Thanks to rittergig for this regex */

            CSharpCommentRegex1 = new Regex(@"//.*$", RegexOptions.Multiline | RegexCompiledOption);
            CSharpCommentRegex2 = new Regex(@"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline | RegexCompiledOption);
            CSharpCommentRegex3 = new Regex(@"(/\*.*?\*/)|(.*\*/)",
                                            RegexOptions.Singleline | RegexOptions.RightToLeft | RegexCompiledOption);
            CSharpNumberRegex = new Regex(@"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b",
                                          RegexCompiledOption);
            CSharpAttributeRegex = new Regex(@"^\s*(?<range>\[.+?\])\s*$", RegexOptions.Multiline | RegexCompiledOption);
            CSharpClassNameRegex = new Regex(@"\b(class|struct|enum|interface)\s+(?<range>\w+?)\b", RegexCompiledOption);
            CSharpKeywordRegex =
                new Regex(
                    @"\b(abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|virtual|void|volatile|while|add|alias|ascending|descending|dynamic|from|get|global|group|into|join|let|orderby|partial|remove|select|set|value|var|where|yield)\b|#region\b|#endregion\b",
                    RegexCompiledOption);
        }

        public void InitStyleSchema(Language lang)
        {
            switch (lang)
            {
                case Language.CSharp:
                    StringStyle = BrownStyle;
                    CommentStyle = GreenStyle;
                    NumberStyle = MagentaStyle;
                    AttributeStyle = GreenStyle;
                    ClassNameStyle = BoldStyle;
                    KeywordStyle = BlueStyle;
                    CommentTagStyle = GrayStyle;
                    break;

                case Language.Vb:
                    StringStyle = BrownStyle;
                    CommentStyle = GreenStyle;
                    NumberStyle = MagentaStyle;
                    ClassNameStyle = BoldStyle;
                    KeywordStyle = BlueStyle;
                    break;

                case Language.Html:
                    CommentStyle = GreenStyle;
                    TagBracketStyle = BlueStyle;
                    TagNameStyle = MaroonStyle;
                    AttributeStyle = RedStyle;
                    AttributeValueStyle = BlueStyle;
                    HtmlEntityStyle = RedStyle;
                    break;

                case Language.Xml:
                    CommentStyle = GreenStyle;
                    XmlTagBracketStyle = BlueStyle;
                    XmlTagNameStyle = MaroonStyle;
                    XmlAttributeStyle = RedStyle;
                    XmlAttributeValueStyle = BlueStyle;
                    XmlEntityStyle = RedStyle;
                    XmlCDataStyle = BlackStyle;
                    break;

                case Language.Js:
                    StringStyle = BrownStyle;
                    CommentStyle = GreenStyle;
                    NumberStyle = MagentaStyle;
                    KeywordStyle = BlueStyle;
                    break;

                case Language.Lua:
                    StringStyle = BrownStyle;
                    CommentStyle = GreenStyle;
                    NumberStyle = MagentaStyle;
                    KeywordStyle = BlueBoldStyle;
                    FunctionsStyle = MaroonStyle;
                    break;

                case Language.Php:
                    StringStyle = RedStyle;
                    CommentStyle = GreenStyle;
                    NumberStyle = RedStyle;
                    VariableStyle = MaroonStyle;
                    KeywordStyle = MagentaStyle;
                    KeywordStyle2 = BlueStyle;
                    KeywordStyle3 = GrayStyle;
                    break;

                case Language.Sql:
                    StringStyle = RedStyle;
                    CommentStyle = GreenStyle;
                    NumberStyle = MagentaStyle;
                    KeywordStyle = BlueBoldStyle;
                    StatementsStyle = BlueBoldStyle;
                    FunctionsStyle = MaroonStyle;
                    VariableStyle = MaroonStyle;
                    TypesStyle = BrownStyle;
                    break;
            }
        }

        public virtual void CSharpSyntaxHighlight(Range range)
        {
            range.tb.CommentPrefix = "//";
            range.tb.LeftBracket = '(';
            range.tb.RightBracket = ')';
            range.tb.LeftBracket2 = '{';
            range.tb.RightBracket2 = '}';
            range.tb.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy2;
            range.tb.AutoIndentCharsPatterns = @"^\s*[\w\.]+(\s\w+)?\s*(?<range>=)\s*(?<range>[^;]+);^\s*(case|default)\s*[^:]*(?<range>:)\s*(?<range>[^;]+);";
            /* Clear style of changed range */
            range.ClearStyle(StringStyle, CommentStyle, NumberStyle, AttributeStyle, ClassNameStyle, KeywordStyle);
            if (CSharpStringRegex == null)
            {
                InitCShaprRegex();
            }
            /* String highlighting */
            range.SetStyle(StringStyle, CSharpStringRegex);
            /* Comment highlighting */
            range.SetStyle(CommentStyle, CSharpCommentRegex1);
            range.SetStyle(CommentStyle, CSharpCommentRegex2);
            range.SetStyle(CommentStyle, CSharpCommentRegex3);
            /* Number highlighting */
            range.SetStyle(NumberStyle, CSharpNumberRegex);
            /* Attribute highlighting */
            range.SetStyle(AttributeStyle, CSharpAttributeRegex);
            /* Class name highlighting */
            range.SetStyle(ClassNameStyle, CSharpClassNameRegex);
            /* Keyword highlighting */
            range.SetStyle(KeywordStyle, CSharpKeywordRegex);
            /* Find document comments */
            foreach (Range r in range.GetRanges(@"^\s*///.*$", RegexOptions.Multiline))
            {
                /* Remove C# highlighting from this fragment */
                r.ClearStyle(StyleIndex.All);
                /* Do XML highlighting */
                if (HtmlTagRegex == null)
                {
                    InitHtmlRegex();
                }
                r.SetStyle(CommentStyle);
                foreach (var rr in r.GetRanges(HtmlTagContentRegex))
                {
                    rr.ClearStyle(StyleIndex.All);
                    rr.SetStyle(CommentTagStyle);
                }
                /* Prefix '///' */
                foreach (var rr in r.GetRanges(@"^\s*///", RegexOptions.Multiline))
                {
                    rr.ClearStyle(StyleIndex.All);
                    rr.SetStyle(CommentTagStyle);
                }
            }
            /* Clear folding markers */
            range.ClearFoldingMarkers();
            /* Set folding markers */
            range.SetFoldingMarkers("{", "}"); //allow to collapse brackets block
            range.SetFoldingMarkers(@"#region\b", @"#endregion\b"); //allow to collapse #region blocks
            range.SetFoldingMarkers(@"/\*", @"\*/"); //allow to collapse comment block
        }

        protected void InitVbRegex()
        {
            VbStringRegex = new Regex(@"""""|"".*?[^\\]""", RegexCompiledOption);
            VbCommentRegex = new Regex(@"'.*$", RegexOptions.Multiline | RegexCompiledOption);
            VbNumberRegex = new Regex(@"\b\d+[\.]?\d*([eE]\-?\d+)?\b", RegexCompiledOption);
            VbClassNameRegex = new Regex(@"\b(Class|Structure|Enum|Interface)[ ]+(?<range>\w+?)\b",
                                         RegexOptions.IgnoreCase | RegexCompiledOption);
            VbKeywordRegex =
                new Regex(
                    @"\b(AddHandler|AddressOf|Alias|And|AndAlso|As|Boolean|ByRef|Byte|ByVal|Call|Case|Catch|CBool|CByte|CChar|CDate|CDbl|CDec|Char|CInt|Class|CLng|CObj|Const|Continue|CSByte|CShort|CSng|CStr|CType|CUInt|CULng|CUShort|Date|Decimal|Declare|Default|Delegate|Dim|DirectCast|Do|Double|Each|Else|ElseIf|End|EndIf|Enum|Erase|Error|Event|Exit|False|Finally|For|Friend|Function|Get|GetType|GetXMLNamespace|Global|GoSub|GoTo|Handles|If|Implements|Imports|In|Inherits|Integer|Interface|Is|IsNot|Let|Lib|Like|Long|Loop|Me|Mod|Module|MustInherit|MustOverride|MyBase|MyClass|Namespace|Narrowing|New|Next|Not|Nothing|NotInheritable|NotOverridable|Object|Of|On|Operator|Option|Optional|Or|OrElse|Overloads|Overridable|Overrides|ParamArray|Partial|Private|Property|Protected|Public|RaiseEvent|ReadOnly|ReDim|REM|RemoveHandler|Resume|Return|SByte|Select|Set|Shadows|Shared|Short|Single|Static|Step|Stop|String|Structure|Sub|SyncLock|Then|Throw|To|True|Try|TryCast|TypeOf|UInteger|ULong|UShort|Using|Variant|Wend|When|While|Widening|With|WithEvents|WriteOnly|Xor|Region)\b|(#Const|#Else|#ElseIf|#End|#If|#Region)\b",
                    RegexOptions.IgnoreCase | RegexCompiledOption);
        }

        public virtual void VbSyntaxHighlight(Range range)
        {
            range.tb.CommentPrefix = "'";
            range.tb.LeftBracket = '(';
            range.tb.RightBracket = ')';
            range.tb.LeftBracket2 = '\x0';
            range.tb.RightBracket2 = '\x0';
            range.tb.AutoIndentCharsPatterns = @"^\s*[\w\.\(\)]+\s*(?<range>=)\s*(?<range>.+)";
            /* Clear style of changed range */
            range.ClearStyle(StringStyle, CommentStyle, NumberStyle, ClassNameStyle, KeywordStyle);
            if (VbStringRegex == null)
            {
                InitVbRegex();
            }
            /* String highlighting */
            range.SetStyle(StringStyle, VbStringRegex);
            /* Comment highlighting */
            range.SetStyle(CommentStyle, VbCommentRegex);
            /* Number highlighting */
            range.SetStyle(NumberStyle, VbNumberRegex);
            /* Class name highlighting */
            range.SetStyle(ClassNameStyle, VbClassNameRegex);
            /* Keyword highlighting */
            range.SetStyle(KeywordStyle, VbKeywordRegex);
            /* Clear folding markers */
            range.ClearFoldingMarkers();
            /* Set folding markers */
            range.SetFoldingMarkers(@"#Region\b", @"#End\s+Region\b", RegexOptions.IgnoreCase);
            range.SetFoldingMarkers(@"\b(Class|Property|Enum|Structure|Interface)[ \t]+\S+",
                                    @"\bEnd (Class|Property|Enum|Structure|Interface)\b", RegexOptions.IgnoreCase);
            range.SetFoldingMarkers(@"^\s*(?<range>While)[ \t]+\S+", @"^\s*(?<range>End While)\b",
                                    RegexOptions.Multiline | RegexOptions.IgnoreCase);
            range.SetFoldingMarkers(@"\b(Sub|Function)[ \t]+[^\s']+", @"\bEnd (Sub|Function)\b", RegexOptions.IgnoreCase);
            /* This declared separately because Sub and Function can be unclosed */
            range.SetFoldingMarkers(@"(\r|\n|^)[ \t]*(?<range>Get|Set)[ \t]*(\r|\n|$)", @"\bEnd (Get|Set)\b",
                                    RegexOptions.IgnoreCase);
            range.SetFoldingMarkers(@"^\s*(?<range>For|For\s+Each)\b", @"^\s*(?<range>Next)\b",
                                    RegexOptions.Multiline | RegexOptions.IgnoreCase);
            range.SetFoldingMarkers(@"^\s*(?<range>Do)\b", @"^\s*(?<range>Loop)\b",
                                    RegexOptions.Multiline | RegexOptions.IgnoreCase);
        }

        protected void InitHtmlRegex()
        {
            HtmlCommentRegex1 = new Regex(@"(<!--.*?-->)|(<!--.*)", RegexOptions.Singleline | RegexCompiledOption);
            HtmlCommentRegex2 = new Regex(@"(<!--.*?-->)|(.*-->)",
                                          RegexOptions.Singleline | RegexOptions.RightToLeft | RegexCompiledOption);
            HtmlTagRegex = new Regex(@"<|/>|</|>", RegexCompiledOption);
            HtmlTagNameRegex = new Regex(@"<(?<range>[!\w:]+)", RegexCompiledOption);
            HtmlEndTagRegex = new Regex(@"</(?<range>[\w:]+)>", RegexCompiledOption);
            HtmlTagContentRegex = new Regex(@"<[^>]+>", RegexCompiledOption);
            HtmlAttrRegex =
                new Regex(
                    @"(?<range>[\w\d\-]{1,20}?)='[^']*'|(?<range>[\w\d\-]{1,20})=""[^""]*""|(?<range>[\w\d\-]{1,20})=[\w\d\-]{1,20}",
                    RegexCompiledOption);
            HtmlAttrValRegex =
                new Regex(
                    @"[\w\d\-]{1,20}?=(?<range>'[^']*')|[\w\d\-]{1,20}=(?<range>""[^""]*"")|[\w\d\-]{1,20}=(?<range>[\w\d\-]{1,20})",
                    RegexCompiledOption);
            HtmlEntityRegex = new Regex(@"\&(amp|gt|lt|nbsp|quot|apos|copy|reg|#[0-9]{1,8}|#x[0-9a-f]{1,8});",
                                        RegexCompiledOption | RegexOptions.IgnoreCase);
        }

        public virtual void HtmlSyntaxHighlight(Range range)
        {
            range.tb.CommentPrefix = null;
            range.tb.LeftBracket = '<';
            range.tb.RightBracket = '>';
            range.tb.LeftBracket2 = '(';
            range.tb.RightBracket2 = ')';
            range.tb.AutoIndentCharsPatterns = @"";
            /* Clear style of changed range */
            range.ClearStyle(CommentStyle, TagBracketStyle, TagNameStyle, AttributeStyle, AttributeValueStyle,
                             HtmlEntityStyle);
            if (HtmlTagRegex == null)
            {
                InitHtmlRegex();
            }
            /* Comment highlighting */
            range.SetStyle(CommentStyle, HtmlCommentRegex1);
            range.SetStyle(CommentStyle, HtmlCommentRegex2);
            /* Tag brackets highlighting */
            range.SetStyle(TagBracketStyle, HtmlTagRegex);
            /* Tag name */
            range.SetStyle(TagNameStyle, HtmlTagNameRegex);
            /* End of tag */
            range.SetStyle(TagNameStyle, HtmlEndTagRegex);
            /* Attributes */
            range.SetStyle(AttributeStyle, HtmlAttrRegex);
            /* Attribute values */
            range.SetStyle(AttributeValueStyle, HtmlAttrValRegex);
            /* html entity */
            range.SetStyle(HtmlEntityStyle, HtmlEntityRegex);
            /* Clear folding markers */
            range.ClearFoldingMarkers();
            /* Set folding markers */
            range.SetFoldingMarkers("<head", "</head>", RegexOptions.IgnoreCase);
            range.SetFoldingMarkers("<body", "</body>", RegexOptions.IgnoreCase);
            range.SetFoldingMarkers("<table", "</table>", RegexOptions.IgnoreCase);
            range.SetFoldingMarkers("<form", "</form>", RegexOptions.IgnoreCase);
            range.SetFoldingMarkers("<div", "</div>", RegexOptions.IgnoreCase);
            range.SetFoldingMarkers("<script", "</script>", RegexOptions.IgnoreCase);
            range.SetFoldingMarkers("<tr", "</tr>", RegexOptions.IgnoreCase);
        }

        protected void InitXmlRegex()
        {
            XmlCommentRegex1 = new Regex(@"(<!--.*?-->)|(<!--.*)", RegexOptions.Singleline | RegexCompiledOption);
            XmlCommentRegex2 = new Regex(@"(<!--.*?-->)|(.*-->)",
                                         RegexOptions.Singleline | RegexOptions.RightToLeft | RegexCompiledOption);
            XmlTagRegex = new Regex(@"<\?|<|/>|</|>|\?>", RegexCompiledOption);
            XmlTagNameRegex = new Regex(@"<[?](?<range1>[x][m][l]{1})|<(?<range>[!\w:]+)", RegexCompiledOption);
            XmlEndTagRegex = new Regex(@"</(?<range>[\w:]+)>", RegexCompiledOption);
            XmlTagContentRegex = new Regex(@"<[^>]+>", RegexCompiledOption);
            XmlAttrRegex =
                new Regex(
                    @"(?<range>[\w\d\-\:]+)[ ]*=[ ]*'[^']*'|(?<range>[\w\d\-\:]+)[ ]*=[ ]*""[^""]*""|(?<range>[\w\d\-\:]+)[ ]*=[ ]*[\w\d\-\:]+",
                    RegexCompiledOption);
            XmlAttrValRegex =
                new Regex(
                    @"[\w\d\-]+?=(?<range>'[^']*')|[\w\d\-]+[ ]*=[ ]*(?<range>""[^""]*"")|[\w\d\-]+[ ]*=[ ]*(?<range>[\w\d\-]+)",
                    RegexCompiledOption);
            XmlEntityRegex = new Regex(@"\&(amp|gt|lt|nbsp|quot|apos|copy|reg|#[0-9]{1,8}|#x[0-9a-f]{1,8});",
                                       RegexCompiledOption | RegexOptions.IgnoreCase);
            XmlcDataRegex = new Regex(@"<!\s*\[CDATA\s*\[(?<text>(?>[^]]+|](?!]>))*)]]>",
                                      RegexCompiledOption | RegexOptions.IgnoreCase);
                /* http://stackoverflow.com/questions/21681861/i-need-a-regex-that-matches-cdata-elements-in-html */
            XmlFoldingRegex = new Regex(@"<(?<range>/?\w+)\s[^>]*?[^/]>|<(?<range>/?\w+)\s*>",
                                        RegexOptions.Singleline | RegexCompiledOption);
        }

        public virtual void XmlSyntaxHighlight(Range range)
        {
            range.tb.CommentPrefix = null;
            range.tb.LeftBracket = '<';
            range.tb.RightBracket = '>';
            range.tb.LeftBracket2 = '(';
            range.tb.RightBracket2 = ')';
            range.tb.AutoIndentCharsPatterns = @"";
            /* Clear style of changed range */
            range.ClearStyle(CommentStyle, XmlTagBracketStyle, XmlTagNameStyle, XmlAttributeStyle,
                             XmlAttributeValueStyle,
                             XmlEntityStyle, XmlCDataStyle);
            if (XmlTagRegex == null)
            {
                InitXmlRegex();
            }
            /* xml CData */
            range.SetStyle(XmlCDataStyle, XmlcDataRegex);
            /* Comment highlighting */
            range.SetStyle(CommentStyle, XmlCommentRegex1);
            range.SetStyle(CommentStyle, XmlCommentRegex2);
            /* tag brackets highlighting */
            range.SetStyle(XmlTagBracketStyle, XmlTagRegex);
            /* tag name */
            range.SetStyle(XmlTagNameStyle, XmlTagNameRegex);
            /* end of tag */
            range.SetStyle(XmlTagNameStyle, XmlEndTagRegex);
            /* attributes */
            range.SetStyle(XmlAttributeStyle, XmlAttrRegex);
            /* attribute values */
            range.SetStyle(XmlAttributeValueStyle, XmlAttrValRegex);
            /* xml entity */
            range.SetStyle(XmlEntityStyle, XmlEntityRegex);
            /* clear folding markers */
            range.ClearFoldingMarkers();
            /* set folding markers */
            XmlFolding(range);
        }

        private void XmlFolding(Range range)
        {
            var stack = new Stack<XmlFoldingTag>();
            var id = 0;
            var fctb = range.tb;
            /* extract opening and closing tags (exclude open-close tags: <TAG/>) */
            foreach (var r in range.GetRanges(XmlFoldingRegex))
            {
                var tagName = r.Text;
                var iLine = r.Start.Line;
                /* if it is opening tag... */
                if (tagName[0] != '/')
                {
                    /* ...push into stack */
                    var tag = new XmlFoldingTag {Name = tagName, Id = id++, StartLine = r.Start.Line};
                    stack.Push(tag);
                    /* if this line has no markers - set marker */
                    if (string.IsNullOrEmpty(fctb[iLine].FoldingStartMarker))
                    {
                        fctb[iLine].FoldingStartMarker = tag.Marker;
                    }
                }
                else
                {
                    /* if it is closing tag - pop from stack */
                    if (stack.Count > 0)
                    {
                        var tag = stack.Pop();
                        /* compare line number */
                        if (iLine == tag.StartLine)
                        {
                            /* remove marker, because same line can not be folding */
                            if (fctb[iLine].FoldingStartMarker == tag.Marker)
                            {
                                fctb[iLine].FoldingStartMarker = null; /* was it our marker? */
                            }
                        }
                        else
                        {
                            /* set end folding marker */
                            if (string.IsNullOrEmpty(fctb[iLine].FoldingEndMarker))
                            {
                                fctb[iLine].FoldingEndMarker = tag.Marker;
                            }
                        }
                    }
                }
            }
        }

        protected void InitSqlRegex()
        {
            SqlStringRegex = new Regex(@"""""|''|"".*?[^\\]""|'.*?[^\\]'", RegexCompiledOption);
            SqlNumberRegex = new Regex(@"\b\d+[\.]?\d*([eE]\-?\d+)?\b", RegexCompiledOption);
            SqlCommentRegex1 = new Regex(@"--.*$", RegexOptions.Multiline | RegexCompiledOption);
            SqlCommentRegex2 = new Regex(@"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline | RegexCompiledOption);
            SqlCommentRegex3 = new Regex(@"(/\*.*?\*/)|(.*\*/)",
                                         RegexOptions.Singleline | RegexOptions.RightToLeft | RegexCompiledOption);
            SqlCommentRegex4 = new Regex(@"#.*$", RegexOptions.Multiline | RegexCompiledOption);
            SqlVarRegex = new Regex(@"@[a-zA-Z_\d]*\b", RegexCompiledOption);
            SqlStatementsRegex =
                new Regex(
                    @"\b(ALTER APPLICATION ROLE|ALTER ASSEMBLY|ALTER ASYMMETRIC KEY|ALTER AUTHORIZATION|ALTER BROKER PRIORITY|ALTER CERTIFICATE|ALTER CREDENTIAL|ALTER CRYPTOGRAPHIC PROVIDER|ALTER DATABASE|ALTER DATABASE AUDIT SPECIFICATION|ALTER DATABASE ENCRYPTION KEY|ALTER ENDPOINT|ALTER EVENT SESSION|ALTER FULLTEXT CATALOG|ALTER FULLTEXT INDEX|ALTER FULLTEXT STOPLIST|ALTER FUNCTION|ALTER INDEX|ALTER LOGIN|ALTER MASTER KEY|ALTER MESSAGE TYPE|ALTER PARTITION FUNCTION|ALTER PARTITION SCHEME|ALTER PROCEDURE|ALTER QUEUE|ALTER REMOTE SERVICE BINDING|ALTER RESOURCE GOVERNOR|ALTER RESOURCE POOL|ALTER ROLE|ALTER ROUTE|ALTER SCHEMA|ALTER SERVER AUDIT|ALTER SERVER AUDIT SPECIFICATION|ALTER SERVICE|ALTER SERVICE MASTER KEY|ALTER SYMMETRIC KEY|ALTER TABLE|ALTER TRIGGER|ALTER USER|ALTER VIEW|ALTER WORKLOAD GROUP|ALTER XML SCHEMA COLLECTION|BULK INSERT|CREATE AGGREGATE|CREATE APPLICATION ROLE|CREATE ASSEMBLY|CREATE ASYMMETRIC KEY|CREATE BROKER PRIORITY|CREATE CERTIFICATE|CREATE CONTRACT|CREATE CREDENTIAL|CREATE CRYPTOGRAPHIC PROVIDER|CREATE DATABASE|CREATE DATABASE AUDIT SPECIFICATION|CREATE DATABASE ENCRYPTION KEY|CREATE DEFAULT|CREATE ENDPOINT|CREATE EVENT NOTIFICATION|CREATE EVENT SESSION|CREATE FULLTEXT CATALOG|CREATE FULLTEXT INDEX|CREATE FULLTEXT STOPLIST|CREATE FUNCTION|CREATE INDEX|CREATE LOGIN|CREATE MASTER KEY|CREATE MESSAGE TYPE|CREATE PARTITION FUNCTION|CREATE PARTITION SCHEME|CREATE PROCEDURE|CREATE QUEUE|CREATE REMOTE SERVICE BINDING|CREATE RESOURCE POOL|CREATE ROLE|CREATE ROUTE|CREATE RULE|CREATE SCHEMA|CREATE SERVER AUDIT|CREATE SERVER AUDIT SPECIFICATION|CREATE SERVICE|CREATE SPATIAL INDEX|CREATE STATISTICS|CREATE SYMMETRIC KEY|CREATE SYNONYM|CREATE TABLE|CREATE TRIGGER|CREATE TYPE|CREATE USER|CREATE VIEW|CREATE WORKLOAD GROUP|CREATE XML INDEX|CREATE XML SCHEMA COLLECTION|DELETE|DISABLE TRIGGER|DROP AGGREGATE|DROP APPLICATION ROLE|DROP ASSEMBLY|DROP ASYMMETRIC KEY|DROP BROKER PRIORITY|DROP CERTIFICATE|DROP CONTRACT|DROP CREDENTIAL|DROP CRYPTOGRAPHIC PROVIDER|DROP DATABASE|DROP DATABASE AUDIT SPECIFICATION|DROP DATABASE ENCRYPTION KEY|DROP DEFAULT|DROP ENDPOINT|DROP EVENT NOTIFICATION|DROP EVENT SESSION|DROP FULLTEXT CATALOG|DROP FULLTEXT INDEX|DROP FULLTEXT STOPLIST|DROP FUNCTION|DROP INDEX|DROP LOGIN|DROP MASTER KEY|DROP MESSAGE TYPE|DROP PARTITION FUNCTION|DROP PARTITION SCHEME|DROP PROCEDURE|DROP QUEUE|DROP REMOTE SERVICE BINDING|DROP RESOURCE POOL|DROP ROLE|DROP ROUTE|DROP RULE|DROP SCHEMA|DROP SERVER AUDIT|DROP SERVER AUDIT SPECIFICATION|DROP SERVICE|DROP SIGNATURE|DROP STATISTICS|DROP SYMMETRIC KEY|DROP SYNONYM|DROP TABLE|DROP TRIGGER|DROP TYPE|DROP USER|DROP VIEW|DROP WORKLOAD GROUP|DROP XML SCHEMA COLLECTION|ENABLE TRIGGER|EXEC|EXECUTE|REPLACE|FROM|INSERT|MERGE|OPTION|OUTPUT|SELECT|TOP|TRUNCATE TABLE|UPDATE|UPDATE STATISTICS|WHERE|WITH|INTO|IN|SET)\b",
                    RegexOptions.IgnoreCase | RegexCompiledOption);
            SqlKeywordsRegex =
                new Regex(
                    @"\b(ADD|ALL|AND|ANY|AS|ASC|AUTHORIZATION|BACKUP|BEGIN|BETWEEN|BREAK|BROWSE|BY|CASCADE|CHECK|CHECKPOINT|CLOSE|CLUSTERED|COLLATE|COLUMN|COMMIT|COMPUTE|CONSTRAINT|CONTAINS|CONTINUE|CROSS|CURRENT|CURRENT_DATE|CURRENT_TIME|CURSOR|DATABASE|DBCC|DEALLOCATE|DECLARE|DEFAULT|DENY|DESC|DISK|DISTINCT|DISTRIBUTED|DOUBLE|DUMP|ELSE|END|ERRLVL|ESCAPE|EXCEPT|EXISTS|EXIT|EXTERNAL|FETCH|FILE|FILLFACTOR|FOR|FOREIGN|FREETEXT|FULL|FUNCTION|GOTO|GRANT|GROUP|HAVING|HOLDLOCK|IDENTITY|IDENTITY_INSERT|IDENTITYCOL|IF|INDEX|INNER|INTERSECT|IS|JOIN|KEY|KILL|LIKE|LINENO|LOAD|NATIONAL|NOCHECK|NONCLUSTERED|NOT|NULL|OF|OFF|OFFSETS|ON|OPEN|OR|ORDER|OUTER|OVER|PERCENT|PIVOT|PLAN|PRECISION|PRIMARY|PRINT|PROC|PROCEDURE|PUBLIC|RAISERROR|READ|READTEXT|RECONFIGURE|REFERENCES|REPLICATION|RESTORE|RESTRICT|RETURN|REVERT|REVOKE|ROLLBACK|ROWCOUNT|ROWGUIDCOL|RULE|SAVE|SCHEMA|SECURITYAUDIT|SHUTDOWN|SOME|STATISTICS|TABLE|TABLESAMPLE|TEXTSIZE|THEN|TO|TRAN|TRANSACTION|TRIGGER|TSEQUAL|UNION|UNIQUE|UNPIVOT|UPDATETEXT|USE|USER|VALUES|VARYING|VIEW|WAITFOR|WHEN|WHILE|WRITETEXT)\b",
                    RegexOptions.IgnoreCase | RegexCompiledOption);
            SqlFunctionsRegex =
                new Regex(
                    @"(@@CONNECTIONS|@@CPU_BUSY|@@CURSOR_ROWS|@@DATEFIRST|@@DATEFIRST|@@DBTS|@@ERROR|@@FETCH_STATUS|@@IDENTITY|@@IDLE|@@IO_BUSY|@@LANGID|@@LANGUAGE|@@LOCK_TIMEOUT|@@MAX_CONNECTIONS|@@MAX_PRECISION|@@NESTLEVEL|@@OPTIONS|@@PACKET_ERRORS|@@PROCID|@@REMSERVER|@@ROWCOUNT|@@SERVERNAME|@@SERVICENAME|@@SPID|@@TEXTSIZE|@@TRANCOUNT|@@VERSION)\b|\b(ABS|ACOS|APP_NAME|ASCII|ASIN|ASSEMBLYPROPERTY|AsymKey_ID|ASYMKEY_ID|asymkeyproperty|ASYMKEYPROPERTY|ATAN|ATN2|AVG|CASE|CAST|CEILING|Cert_ID|Cert_ID|CertProperty|CHAR|CHARINDEX|CHECKSUM_AGG|COALESCE|COL_LENGTH|COL_NAME|COLLATIONPROPERTY|COLLATIONPROPERTY|COLUMNPROPERTY|COLUMNS_UPDATED|COLUMNS_UPDATED|CONTAINSTABLE|CONVERT|COS|COT|COUNT|COUNT_BIG|CRYPT_GEN_RANDOM|CURRENT_TIMESTAMP|CURRENT_TIMESTAMP|CURRENT_USER|CURRENT_USER|CURSOR_STATUS|DATABASE_PRINCIPAL_ID|DATABASE_PRINCIPAL_ID|DATABASEPROPERTY|DATABASEPROPERTYEX|DATALENGTH|DATALENGTH|DATEADD|DATEDIFF|DATENAME|DATEPART|DAY|DB_ID|DB_NAME|DECRYPTBYASYMKEY|DECRYPTBYCERT|DECRYPTBYKEY|DECRYPTBYKEYAUTOASYMKEY|DECRYPTBYKEYAUTOCERT|DECRYPTBYPASSPHRASE|DEGREES|DENSE_RANK|DIFFERENCE|ENCRYPTBYASYMKEY|ENCRYPTBYCERT|ENCRYPTBYKEY|ENCRYPTBYPASSPHRASE|ERROR_LINE|ERROR_MESSAGE|ERROR_NUMBER|ERROR_PROCEDURE|ERROR_SEVERITY|ERROR_STATE|EVENTDATA|EXP|FILE_ID|FILE_IDEX|FILE_NAME|FILEGROUP_ID|FILEGROUP_NAME|FILEGROUPPROPERTY|FILEPROPERTY|FLOOR|fn_helpcollations|fn_listextendedproperty|fn_servershareddrives|fn_virtualfilestats|fn_virtualfilestats|FORMATMESSAGE|FREETEXTTABLE|FULLTEXTCATALOGPROPERTY|FULLTEXTSERVICEPROPERTY|GETANSINULL|GETDATE|GETUTCDATE|GROUPING|HAS_PERMS_BY_NAME|HOST_ID|HOST_NAME|IDENT_CURRENT|IDENT_CURRENT|IDENT_INCR|IDENT_INCR|IDENT_SEED|IDENTITY\(|INDEX_COL|INDEXKEY_PROPERTY|INDEXPROPERTY|IS_MEMBER|IS_OBJECTSIGNED|IS_SRVROLEMEMBER|ISDATE|ISDATE|ISNULL|ISNUMERIC|Key_GUID|Key_GUID|Key_ID|Key_ID|KEY_NAME|KEY_NAME|LEFT|LEN|LOG|LOG10|LOWER|LTRIM|MAX|MIN|MONTH|NCHAR|NEWID|NTILE|NULLIF|OBJECT_DEFINITION|OBJECT_ID|OBJECT_NAME|OBJECT_SCHEMA_NAME|OBJECTPROPERTY|OBJECTPROPERTYEX|OPENDATASOURCE|OPENQUERY|OPENROWSET|OPENXML|ORIGINAL_LOGIN|ORIGINAL_LOGIN|PARSENAME|PATINDEX|PATINDEX|PERMISSIONS|PI|POWER|PUBLISHINGSERVERNAME|PWDCOMPARE|PWDENCRYPT|QUOTENAME|RADIANS|RAND|RANK|REPLICATE|REVERSE|RIGHT|ROUND|ROW_NUMBER|ROWCOUNT_BIG|RTRIM|SCHEMA_ID|SCHEMA_ID|SCHEMA_NAME|SCHEMA_NAME|SCOPE_IDENTITY|SERVERPROPERTY|SESSION_USER|SESSION_USER|SESSIONPROPERTY|SETUSER|SIGN|SignByAsymKey|SignByCert|SIN|SOUNDEX|SPACE|SQL_VARIANT_PROPERTY|SQRT|SQUARE|STATS_DATE|STDEV|STDEVP|STR|STUFF|SUBSTRING|SUM|SUSER_ID|SUSER_NAME|SUSER_SID|SUSER_SNAME|SWITCHOFFSET|SYMKEYPROPERTY|symkeyproperty|sys\.dm_db_index_physical_stats|sys\.fn_builtin_permissions|sys\.fn_my_permissions|SYSDATETIME|SYSDATETIMEOFFSET|SYSTEM_USER|SYSTEM_USER|SYSUTCDATETIME|TAN|TERTIARY_WEIGHTS|TEXTPTR|TODATETIMEOFFSET|TRIGGER_NESTLEVEL|TYPE_ID|TYPE_NAME|TYPEPROPERTY|UNICODE|UPDATE\(|UPPER|USER_ID|USER_NAME|USER_NAME|VAR|VARP|VerifySignedByAsymKey|VerifySignedByCert|XACT_STATE|YEAR)\b",
                    RegexOptions.IgnoreCase | RegexCompiledOption);
            SqlTypesRegex =
                new Regex(
                    @"\b(BIGINT|NUMERIC|BIT|SMALLINT|DECIMAL|SMALLMONEY|INT|TINYINT|MONEY|FLOAT|REAL|DATE|DATETIMEOFFSET|DATETIME2|SMALLDATETIME|DATETIME|TIME|CHAR|VARCHAR|TEXT|NCHAR|NVARCHAR|NTEXT|BINARY|VARBINARY|IMAGE|TIMESTAMP|HIERARCHYID|TABLE|UNIQUEIDENTIFIER|SQL_VARIANT|XML)\b",
                    RegexOptions.IgnoreCase | RegexCompiledOption);
        }

        public virtual void SqlSyntaxHighlight(Range range)
        {
            range.tb.CommentPrefix = "--";
            range.tb.LeftBracket = '(';
            range.tb.RightBracket = ')';
            range.tb.LeftBracket2 = '\x0';
            range.tb.RightBracket2 = '\x0';
            range.tb.AutoIndentCharsPatterns = @"";
            /* clear style of changed range */
            range.ClearStyle(CommentStyle, StringStyle, NumberStyle, VariableStyle, StatementsStyle, KeywordStyle,
                             FunctionsStyle, TypesStyle);
            if (SqlStringRegex == null)
            {
                InitSqlRegex();
            }
            /* comment highlighting */
            range.SetStyle(CommentStyle, SqlCommentRegex1);
            range.SetStyle(CommentStyle, SqlCommentRegex2);
            range.SetStyle(CommentStyle, SqlCommentRegex3);
            range.SetStyle(CommentStyle, SqlCommentRegex4);
            /* string highlighting */
            range.SetStyle(StringStyle, SqlStringRegex);
            /* number highlighting */
            range.SetStyle(NumberStyle, SqlNumberRegex);
            /* types highlighting */
            range.SetStyle(TypesStyle, SqlTypesRegex);
            /* var highlighting */
            range.SetStyle(VariableStyle, SqlVarRegex);
            /* statements */
            range.SetStyle(StatementsStyle, SqlStatementsRegex);
            /* keywords */
            range.SetStyle(KeywordStyle, SqlKeywordsRegex);
            /* functions */
            range.SetStyle(FunctionsStyle, SqlFunctionsRegex);
            /* clear folding markers */
            range.ClearFoldingMarkers();
            /* set folding markers */
            range.SetFoldingMarkers(@"\bBEGIN\b", @"\bEND\b", RegexOptions.IgnoreCase);
            /* allow to collapse BEGIN..END blocks */
            range.SetFoldingMarkers(@"/\*", @"\*/"); /* allow to collapse comment block */
        }

        protected void InitPhpRegex()
        {
            PhpStringRegex = new Regex(@"""""|''|"".*?[^\\]""|'.*?[^\\]'", RegexCompiledOption);
            PhpNumberRegex = new Regex(@"\b\d+[\.]?\d*\b", RegexCompiledOption);
            PhpCommentRegex1 = new Regex(@"(//|#).*$", RegexOptions.Multiline | RegexCompiledOption);
            PhpCommentRegex2 = new Regex(@"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline | RegexCompiledOption);
            PhpCommentRegex3 = new Regex(@"(/\*.*?\*/)|(.*\*/)",
                                         RegexOptions.Singleline | RegexOptions.RightToLeft | RegexCompiledOption);
            PhpVarRegex = new Regex(@"\$[a-zA-Z_\d]*\b", RegexCompiledOption);
            PhpKeywordRegex1 =
                new Regex(
                    @"\b(die|echo|empty|exit|eval|include|include_once|isset|list|require|require_once|return|print|unset)\b",
                    RegexCompiledOption);
            PhpKeywordRegex2 =
                new Regex(
                    @"\b(abstract|and|array|as|break|case|catch|cfunction|class|clone|const|continue|declare|default|do|else|elseif|enddeclare|endfor|endforeach|endif|endswitch|endwhile|extends|final|for|foreach|function|global|goto|if|implements|instanceof|interface|namespace|new|or|private|protected|public|static|switch|throw|try|use|var|while|xor)\b",
                    RegexCompiledOption);
            PhpKeywordRegex3 = new Regex(@"__CLASS__|__DIR__|__FILE__|__LINE__|__FUNCTION__|__METHOD__|__NAMESPACE__",
                                         RegexCompiledOption);
        }

        public virtual void PhpSyntaxHighlight(Range range)
        {
            range.tb.CommentPrefix = "//";
            range.tb.LeftBracket = '(';
            range.tb.RightBracket = ')';
            range.tb.LeftBracket2 = '{';
            range.tb.RightBracket2 = '}';
            range.tb.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy2;
            /* clear style of changed range */
            range.ClearStyle(StringStyle, CommentStyle, NumberStyle, VariableStyle, KeywordStyle, KeywordStyle2,
                             KeywordStyle3);
            range.tb.AutoIndentCharsPatterns = @"^\s*\$[\w\.\[\]\'\""]+\s*(?<range>=)\s*(?<range>[^;]+);";
            if (PhpStringRegex == null)
            {
                InitPhpRegex();
            }
            /* string highlighting */
            range.SetStyle(StringStyle, PhpStringRegex);
            /* comment highlighting */
            range.SetStyle(CommentStyle, PhpCommentRegex1);
            range.SetStyle(CommentStyle, PhpCommentRegex2);
            range.SetStyle(CommentStyle, PhpCommentRegex3);
            /* number highlighting */
            range.SetStyle(NumberStyle, PhpNumberRegex);
            /* var highlighting */
            range.SetStyle(VariableStyle, PhpVarRegex);
            /* keyword highlighting */
            range.SetStyle(KeywordStyle, PhpKeywordRegex1);
            range.SetStyle(KeywordStyle2, PhpKeywordRegex2);
            range.SetStyle(KeywordStyle3, PhpKeywordRegex3);
            /* clear folding markers */
            range.ClearFoldingMarkers();
            /* set folding markers */
            range.SetFoldingMarkers("{", "}"); /* allow to collapse brackets block */
            range.SetFoldingMarkers(@"/\*", @"\*/"); /* allow to collapse comment block */
        }

        protected void InitJScriptRegex()
        {
            JScriptStringRegex = new Regex(@"""""|''|"".*?[^\\]""|'.*?[^\\]'", RegexCompiledOption);
            JScriptCommentRegex1 = new Regex(@"//.*$", RegexOptions.Multiline | RegexCompiledOption);
            JScriptCommentRegex2 = new Regex(@"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline | RegexCompiledOption);
            JScriptCommentRegex3 = new Regex(@"(/\*.*?\*/)|(.*\*/)",
                                             RegexOptions.Singleline | RegexOptions.RightToLeft | RegexCompiledOption);
            JScriptNumberRegex = new Regex(@"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b",
                                           RegexCompiledOption);
            JScriptKeywordRegex =
                new Regex(
                    @"\b(true|false|break|case|catch|const|continue|default|delete|do|else|export|for|function|if|in|instanceof|new|null|return|switch|this|throw|try|var|void|while|with|typeof)\b",
                    RegexCompiledOption);
        }

        public virtual void JScriptSyntaxHighlight(Range range)
        {
            range.tb.CommentPrefix = "//";
            range.tb.LeftBracket = '(';
            range.tb.RightBracket = ')';
            range.tb.LeftBracket2 = '{';
            range.tb.RightBracket2 = '}';
            range.tb.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy2;
            range.tb.AutoIndentCharsPatterns = @"^\s*[\w\.]+(\s\w+)?\s*(?<range>=)\s*(?<range>[^;]+);";
            /* clear style of changed range */
            range.ClearStyle(StringStyle, CommentStyle, NumberStyle, KeywordStyle);
            if (JScriptStringRegex == null)
            {
                InitJScriptRegex();
            }
            /* string highlighting */
            range.SetStyle(StringStyle, JScriptStringRegex);
            /* comment highlighting */
            range.SetStyle(CommentStyle, JScriptCommentRegex1);
            range.SetStyle(CommentStyle, JScriptCommentRegex2);
            range.SetStyle(CommentStyle, JScriptCommentRegex3);
            /* number highlighting */
            range.SetStyle(NumberStyle, JScriptNumberRegex);
            /* keyword highlighting */
            range.SetStyle(KeywordStyle, JScriptKeywordRegex);
            /* clear folding markers */
            range.ClearFoldingMarkers();
            /* set folding markers */
            range.SetFoldingMarkers("{", "}"); /* allow to collapse brackets block */
            range.SetFoldingMarkers(@"/\*", @"\*/"); /* allow to collapse comment block */
        }

        protected void InitLuaRegex()
        {
            LuaStringRegex = new Regex(@"""""|''|"".*?[^\\]""|'.*?[^\\]'", RegexCompiledOption);
            LuaCommentRegex1 = new Regex(@"--.*$", RegexOptions.Multiline | RegexCompiledOption);
            LuaCommentRegex2 = new Regex(@"(--\[\[.*?\]\])|(--\[\[.*)", RegexOptions.Singleline | RegexCompiledOption);
            LuaCommentRegex3 = new Regex(@"(--\[\[.*?\]\])|(.*\]\])",
                                         RegexOptions.Singleline | RegexOptions.RightToLeft | RegexCompiledOption);
            LuaNumberRegex = new Regex(@"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b",
                                       RegexCompiledOption);
            LuaKeywordRegex =
                new Regex(
                    @"\b(and|break|do|else|elseif|end|false|for|function|if|in|local|nil|not|or|repeat|return|then|true|until|while)\b",
                    RegexCompiledOption);

            LuaFunctionsRegex =
                new Regex(
                    @"\b(assert|collectgarbage|dofile|error|getfenv|getmetatable|ipairs|load|loadfile|loadstring|module|next|pairs|pcall|print|rawequal|rawget|rawset|require|select|setfenv|setmetatable|tonumber|tostring|type|unpack|xpcall)\b",
                    RegexCompiledOption);
        }

        public virtual void LuaSyntaxHighlight(Range range)
        {
            range.tb.CommentPrefix = "--";
            range.tb.LeftBracket = '(';
            range.tb.RightBracket = ')';
            range.tb.LeftBracket2 = '{';
            range.tb.RightBracket2 = '}';
            range.tb.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy2;
            range.tb.AutoIndentCharsPatterns = @"^\s*[\w\.]+(\s\w+)?\s*(?<range>=)\s*(?<range>.+)";
            /* clear style of changed range */
            range.ClearStyle(StringStyle, CommentStyle, NumberStyle, KeywordStyle, FunctionsStyle);
            if (LuaStringRegex == null)
            {
                InitLuaRegex();
            }
            /* string highlighting */
            range.SetStyle(StringStyle, LuaStringRegex);
            /* comment highlighting */
            range.SetStyle(CommentStyle, LuaCommentRegex1);
            range.SetStyle(CommentStyle, LuaCommentRegex2);
            range.SetStyle(CommentStyle, LuaCommentRegex3);
            /* number highlighting */
            range.SetStyle(NumberStyle, LuaNumberRegex);
            /* keyword highlighting */
            range.SetStyle(KeywordStyle, LuaKeywordRegex);
            /* functions highlighting */
            range.SetStyle(FunctionsStyle, LuaFunctionsRegex);
            /* clear folding markers */
            range.ClearFoldingMarkers();
            /* set folding markers */
            range.SetFoldingMarkers("{", "}"); /* allow to collapse brackets block */
            range.SetFoldingMarkers(@"--\[\[", @"\]\]"); /* allow to collapse comment block */
        }

        protected void LuaAutoIndentNeeded(object sender, AutoIndentEventArgs args)
        {
            /* end of block */
            if (Regex.IsMatch(args.LineText, @"^\s*(end|until)\b"))
            {
                args.Shift = -args.TabLength;
                args.ShiftNextLines = -args.TabLength;
                return;
            }
            /* then ... */
            if (Regex.IsMatch(args.LineText, @"\b(then)\s*\S+"))
            {
                return;
            }
            /* start of operator block */
            if (Regex.IsMatch(args.LineText, @"^\s*(function|do|for|while|repeat|if)\b"))
            {
                args.ShiftNextLines = args.TabLength;
                return;
            }
            /* Statements else, elseif, case etc */
            if (!Regex.IsMatch(args.LineText, @"^\s*(else|elseif)\b", RegexOptions.IgnoreCase))
            {
                return;
            }
            args.Shift = -args.TabLength;
            return;
        }
    }
}