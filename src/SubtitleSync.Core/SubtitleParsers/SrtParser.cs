using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SubtitleSync.Core.Models;

namespace SubtitleSync.Core.SubtitleParsers
{
    /// <summary>
    /// Parser for SubRip Text (SRT) subtitle format.
    /// </summary>
    public class SrtParser : SubtitleParserBase
    {
        private static readonly Regex TimingRegex = new Regex(
            @