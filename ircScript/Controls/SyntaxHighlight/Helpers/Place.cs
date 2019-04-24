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

namespace ircScript.Controls.SyntaxHighlight.Helpers
{
    public struct Place : IEquatable<Place>
    {
        public int Char;
        public int Line ;

        public Place(int iChar, int iLine)
        {
            Char = iChar;
            Line = iLine;
        }

        public void Offset(int dx, int dy)
        {
            Char += dx;
            Line += dy;
        }

        public bool Equals(Place other)
        {
            return other.Char == Char && other.Line == Line;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is Place && Equals((Place) obj);
        }

        public override int GetHashCode()
        {
            return Char.GetHashCode() ^ Line.GetHashCode();
        }

        public static bool operator !=(Place p1, Place p2)
        {
            return !p1.Equals(p2);
        }

        public static bool operator ==(Place p1, Place p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator <(Place p1, Place p2)
        {
            if (p1.Line < p2.Line)
            {
                return true;
            }
            if (p1.Line > p2.Line)
            {
                return false;
            }
            return p1.Char < p2.Char;
        }

        public static bool operator <=(Place p1, Place p2)
        {
            if (p1.Equals(p2))
            {
                return true;
            }
            if (p1.Line < p2.Line)
            {
                return true;
            }
            if (p1.Line > p2.Line)
            {
                return false;
            }
            return p1.Char < p2.Char;
        }

        public static bool operator >(Place p1, Place p2)
        {
            if (p1.Line > p2.Line)
            {
                return true;
            }
            if (p1.Line < p2.Line)
            {
                return false;
            }
            return p1.Char > p2.Char;
        }

        public static bool operator >=(Place p1, Place p2)
        {
            if (p1.Equals(p2))
            {
                return true;
            }
            if (p1.Line > p2.Line)
            {
                return true;
            }
            if (p1.Line < p2.Line)
            {
                return false;
            }
            return p1.Char > p2.Char;
        }

        public static Place operator +(Place p1, Place p2)
        {
            return new Place(p1.Char + p2.Char, p1.Line + p2.Line);
        }

        public static Place Empty
        {
            get { return new Place(); }
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", Char, Line);
        }
    }
}
