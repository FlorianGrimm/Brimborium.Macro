// 
// ToStringHelper.cs
//  
// Author:
//       Mikayla Hutchinson <m.j.hutchinson@gmail.com>
// 
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Reflection;

namespace Microsoft.VisualStudio.TextTemplating;

	public static class ToStringHelper
	{
    private static readonly object[] formatProviderAsParameterArray;
    private static IFormatProvider formatProvider = System.Globalization.CultureInfo.InvariantCulture;
		
		static ToStringHelper ()
		{
			formatProviderAsParameterArray = new object[] { formatProvider };
		}

		public static string ToStringWithCulture (object objectToConvert)
		{
			if (objectToConvert == null) {
            throw new ArgumentNullException (nameof (objectToConvert));
        }

        if (objectToConvert is IConvertible conv) {
            return conv.ToString (formatProvider);
        }

        if (objectToConvert is string str) {
            return str;
        }

        //TODO: implement a cache of types and DynamicMethods
        MethodInfo mi = objectToConvert.GetType ().GetMethod ("ToString", new Type[] { typeof (IFormatProvider) });
			if (mi != null) {
            return (string) mi.Invoke (objectToConvert, formatProviderAsParameterArray);
        }

        return objectToConvert.ToString ();
		}
		
		public static IFormatProvider FormatProvider {
			get { return (IFormatProvider)formatProviderAsParameterArray[0]; }
			set { formatProviderAsParameterArray[0] = formatProvider = value; }
		}
	}
