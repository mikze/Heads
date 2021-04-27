﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Glowki.UDP
{
    [Flags]
    public enum Input
    {
        none = 1,
        up = 2,
        right = 4,
        left = 8,
        kick = 16,
        kickUp = 32,
        enemy = 64
    }
}
