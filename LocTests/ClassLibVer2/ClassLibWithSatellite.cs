// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Resources;
using System.Globalization;

namespace ClassLibWithSatellite
{
    class ClassLibWithSatellite
    {
        static public string Describe(string lang)
        {
            ResourceManager rm = new ResourceManager(typeof(ClassLibStrings));

            CultureInfo ci = CultureInfo.CreateSpecificCulture(lang);

            return rm.GetString("Describe", ci);
        }
    }

    class ClassLibStrings
    {
    }
}
