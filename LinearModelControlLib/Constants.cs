using Optimization.Solver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace LinearModelControlLib
{
    /// <summary>
    /// Contains some parametrical information used by the linear model control.
    /// </summary>
    public class Constants
    {
        #region Formatting

        /// <summary>
        /// The formatter to use for string conversions from and to numbers.
        /// </summary>
        internal static readonly CultureInfo FORMATTER = CultureInfo.InvariantCulture;
        /// <summary>
        /// An example number to show formatting with.
        /// </summary>
        internal const double EXAMPLE_NUMBER = 1.2;
        /// <summary>
        /// All characters that are allowed to be input into decimal textboxes.
        /// </summary>
        internal static readonly HashSet<Key> ALLOWED_CHARACTERS = new HashSet<Key>()
        {
            Key.D0, Key.NumPad0,
            Key.D1, Key.NumPad1,
            Key.D2, Key.NumPad2,
            Key.D3, Key.NumPad3,
            Key.D4, Key.NumPad4,
            Key.D5, Key.NumPad5,
            Key.D6, Key.NumPad6,
            Key.D7, Key.NumPad7,
            Key.D8, Key.NumPad8,
            Key.D9, Key.NumPad9,
            Key.OemMinus, Key.Subtract,
            Key.OemPeriod, Key.Decimal,
        };

        #endregion

        #region I/O

        /// <summary>
        /// The formatter to use for I/O operations.
        /// </summary>
        internal static readonly CultureInfo IO_FORMATTER = CultureInfo.InvariantCulture;
        /// <summary>
        /// The delimiter to use to split the different values.
        /// </summary>
        internal const char IO_DELIMITER = ' ';
        /// <summary>
        /// The file ending used for serializing instances.
        /// </summary>
        public const string IO_FILE_ENDING = "tgmod";
        /// <summary>
        /// Indicates that this line is a comment.
        /// </summary>
        public const string IO_COMMENT = "//";
        /// <summary>
        /// Indicates that a block keyword is following.
        /// </summary>
        internal const string IO_BLOCK_START = "#";
        /// <summary>
        /// Indicates that the variable block is following.
        /// </summary>
        internal const string IO_BLOCK_IDENT_VARIABLES = "Variables";
        /// <summary>
        /// Indicates that the constraint block is following.
        /// </summary>
        internal const string IO_BLOCK_IDENT_CONSTRAINTS = "Constraints";

        #endregion

        #region Width / height definitions

        /// <summary>
        /// The width of a standard column.
        /// </summary>
        internal const int COL_WIDTH = 70;
        /// <summary>
        /// The width of the first column containing some description.
        /// </summary>
        internal const int COL_WIDTH_HEADER = 50;
        /// <summary>
        /// The width of the column specifying the type of the contraints.
        /// </summary>
        internal const int COL_WIDTH_CONSTRAINT_TYPE = 50;
        /// <summary>
        /// The width of the RHS column.
        /// </summary>
        internal const int COL_WIDTH_RHS = 95;
        /// <summary>
        /// The height of a row.
        /// </summary>
        internal const int ROW_HEIGHT = 20;

        #endregion

        #region UI-element index definitions

        /// <summary>
        /// The position info of the corresponding information.
        /// </summary>
        internal static readonly PositionInfo POS_VAR_UB = new PositionInfo(0, false);
        /// <summary>
        /// The position info of the corresponding information.
        /// </summary>
        internal static readonly PositionInfo POS_VAR_LB = new PositionInfo(1, false);
        /// <summary>
        /// The position info of the corresponding information.
        /// </summary>
        internal static readonly PositionInfo POS_VAR_TYPE = new PositionInfo(2, false);
        /// <summary>
        /// The position info of the corresponding information.
        /// </summary>
        internal static readonly PositionInfo POS_VAR_NAME = new PositionInfo(3, false);
        /// <summary>
        /// The position info of the corresponding information.
        /// </summary>
        internal static readonly PositionInfo POS_VAR_OBJ_COEFF = new PositionInfo(0, true);
        /// <summary>
        /// The position info of the corresponding information.
        /// </summary>
        internal static readonly PositionInfo POS_VAR_VALUE = new PositionInfo(1, true);

        /// <summary>
        /// The position info of the corresponding information.
        /// </summary>
        internal static readonly PositionInfo POS_CON_NAME = new PositionInfo(0, false);
        /// <summary>
        /// The position info of the corresponding information.
        /// </summary>
        internal static readonly PositionInfo POS_CON_TYPE = new PositionInfo(0, true);
        /// <summary>
        /// The position info of the corresponding information.
        /// </summary>
        internal static readonly PositionInfo POS_CON_RHS = new PositionInfo(1, true);
        /// <summary>
        /// The position info of the corresponding information.
        /// </summary>
        internal static readonly PositionInfo POS_CON_DUAL = new PositionInfo(2, true);

        #endregion

        #region Default values

        /// <summary>
        /// The default value for the corresponding type of information.
        /// </summary>
        internal const double DEFAULT_VALUE_LB = 0;
        /// <summary>
        /// The default value for the corresponding type of information.
        /// </summary>
        internal const double DEFAULT_VALUE_UB = double.PositiveInfinity;
        /// <summary>
        /// The default value for the corresponding type of information.
        /// </summary>
        internal const double DEFAULT_VALUE_VALUE = double.NaN;
        /// <summary>
        /// The default value for the corresponding type of information.
        /// </summary>
        internal const double DEFAULT_VALUE_DUAL = double.NaN;
        /// <summary>
        /// The default value for the corresponding type of information.
        /// </summary>
        internal const double DEFAULT_VALUE_RHS = 10;
        /// <summary>
        /// The default value for the corresponding type of information.
        /// </summary>
        internal const double DEFAULT_VALUE_OBJ_COEFF = 1;
        /// <summary>
        /// The default value for the corresponding type of information.
        /// </summary>
        internal const double DEFAULT_VALUE_CON_COEFF = 1;
        /// <summary>
        /// The default value for the corresponding type of information.
        /// </summary>
        internal const string DEFAULT_VALUE_VAR_NAME_PREFIX = "x";
        /// <summary>
        /// The default value for the corresponding type of information.
        /// </summary>
        internal const string DEFAULT_VALUE_CON_NAME_PREFIX = "c";

        #endregion

        #region Solution status

        public static readonly Dictionary<ModelStatus, SolidColorBrush> ModelStatusColors = new Dictionary<ModelStatus, SolidColorBrush>()
        {
            { ModelStatus.Unknown, Brushes.Gray },
            { ModelStatus.Feasible, Brushes.Green },
            { ModelStatus.Infeasible, Brushes.Red },
            { ModelStatus.InfOrUnbd, Brushes.Orange},
            { ModelStatus.Unbounded, Brushes.Yellow },
        };

        public static readonly Dictionary<SolutionStatus, SolidColorBrush> SolutionStatusColors = new Dictionary<SolutionStatus, SolidColorBrush>()
        {
            { SolutionStatus.Feasible, Brushes.Yellow },
            { SolutionStatus.FeasibleContinuousRelaxation, Brushes.Yellow },
            { SolutionStatus.LocalOptimal, Brushes.LightGoldenrodYellow },
            { SolutionStatus.NoSolutionValues, Brushes.Gray },
            { SolutionStatus.Optimal, Brushes.Green },
            { SolutionStatus.OptimalContinuousRelaxation, Brushes.LightGreen },
            { SolutionStatus.ProbablyLocalOptimal, Brushes.YellowGreen },
        };

        #endregion

        #region Constraint type conversion helper

        /// <summary>
        /// Contains all conversions from enumeration item to string for constraint types.
        /// </summary>
        private static Dictionary<ConstraintType, string> _typeStringTranslations = new Dictionary<ConstraintType, string>()
        {
            { ConstraintType.Eq, "==" },
            { ConstraintType.Le, "<=" },
            { ConstraintType.Ge, ">=" },
        };
        /// <summary>
        /// Contains all conversions from string to enumeration item for constraint types.
        /// </summary>
        private static Dictionary<string, ConstraintType> _stringTypeTranslations = null;
        /// <summary>
        /// Converts the constraint type to a string used by the GUI elements.
        /// </summary>
        /// <param name="type">The type to convert.</param>
        /// <returns>The string representing the specific type.</returns>
        public static string ConvertConstraintTypeName(ConstraintType type) { return _typeStringTranslations[type]; }
        /// <summary>
        /// Converts a string used by the GUI elements to a constraint type.
        /// </summary>
        /// <param name="type">The string to convert.</param>
        /// <returns>The type as specified by the string.</returns>
        public static ConstraintType ConvertConstraintTypeName(string type)
        {
            if (_stringTypeTranslations == null) _stringTypeTranslations = _typeStringTranslations.ToDictionary(k => k.Value, v => v.Key);
            return _stringTypeTranslations[type];
        }

        #endregion
    }

    /// <summary>
    /// Exposes information about the position of informative elements.
    /// </summary>
    internal struct PositionInfo
    {
        /// <summary>
        /// Creates a new instance of this struct.
        /// </summary>
        /// <param name="index">The index at which the information shal be located.</param>
        /// <param name="below">Indicates whether the information will be located above or below the main matrix.</param>
        public PositionInfo(int index, bool below) { Index = index; Below = below; }
        /// <summary>
        /// The index at which the information shal be located.
        /// </summary>
        public int Index;
        /// <summary>
        /// Indicates whether the information will be located above or below the main matrix.
        /// </summary>
        public bool Below;
    }
}
