using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Enums
{
    /// <summary>
    /// Enum NumericInput which indicates what input is allowed for NumericUpdDown.
    /// </summary>
    [Flags]
    public enum NumericInput
    {
        /// <summary>
        /// Only numbers are allowed
        /// </summary>
        Numbers = 1 << 1, // Only Numbers
        /// <summary>
        /// Numbers with decimal point and allowed scientific input
        /// </summary>
        Decimal = 2 << 1,
        /// <summary>
        /// All is allowed
        /// </summary>
        All = Numbers | Decimal
    }
}
