﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Enums
{
    public enum DecimalPointCorrectionMode
    {
        /// <summary>
        /// (Default) No correction is applied, and any style
        /// inherited setting may influence the correction behavior.
        /// </summary>
        Inherits,

        /// <summary>
        /// Enable the decimal-point correction for generic numbers.
        /// </summary>
        Number,

        /// <summary>
        /// Enable the decimal-point correction for currency numbers.
        /// </summary>
        Currency,

        /// <summary>
        /// Enable the decimal-point correction for percent-numbers.
        /// </summary>
        Percent,
    }
}
