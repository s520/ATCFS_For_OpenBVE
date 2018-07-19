// The MIT License (MIT)
//
// Copyright (c) 2008 Ricardo Amores Hernandez
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using IniParser.Model;
using IniParser.Parser;

namespace IniParser
{
    /// <summary>
    ///     Represents an INI data parser for strings.
    ///     
    /// </summary>
    /// <remarks>
    ///     This class is deprecated and kept for backwards compatibility.
    ///     It's just a wrapper around <see cref="IniDataParser"/> class.
    ///     Please, replace your code.
    /// </remarks>
    [Obsolete("Use class IniDataParser instead. See remarks comments in this class.")]
    public class StringIniParser
    {
        /// <summary>
        ///     This instance will handle ini data parsing and writing
        /// </summary>
        public IniDataParser Parser { get; protected set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        public StringIniParser() : this (new IniDataParser()) {}

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="parser"></param>
        public StringIniParser(IniDataParser parser)
        {
            Parser = parser;
        }

        /// <summary>
        /// Parses a string containing data formatted as an INI file.
        /// </summary>
        /// <param name="dataStr">The string containing the data.</param>
        /// <returns>
        /// A new <see cref="IniData"/> instance with the data parsed from the string.
        /// </returns>
        public IniData ParseString(string dataStr)
        {
            return Parser.Parse(dataStr);
        }

        /// <summary>
        /// Creates a string from the INI data.
        /// </summary>
        /// <param name="iniData">An <see cref="IniData"/> instance.</param>
        /// <returns>
        /// A formatted string with the contents of the
        /// <see cref="IniData"/> instance object.
        /// </returns>
        public string WriteString(IniData iniData)
        {
            return iniData.ToString();
        }
    }
}
