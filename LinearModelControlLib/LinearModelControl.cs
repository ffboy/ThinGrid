using Optimization;
using Optimization.Interfaces;
using Optimization.Solver;
using Optimization.Solver.GLPK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LinearModelControlLib
{
    public class LinearModelControl : Grid
    {
        public LinearModelControl() { InitSample(); }

        #region Init

        /// <summary>
        /// Initializes an empty control.
        /// </summary>
        private void Init()
        {
            // --> Init minimal rows and cols
            // Upper bounds
            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(Constants.ROW_HEIGHT) });
            // Lower bounds
            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(Constants.ROW_HEIGHT) });
            // Variable type
            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(Constants.ROW_HEIGHT) });
            // Variable name
            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(Constants.ROW_HEIGHT) });
            // Objective coefficient
            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(Constants.ROW_HEIGHT) });
            // Solution value
            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(Constants.ROW_HEIGHT) });
            // Constraint name
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(Constants.COL_WIDTH_HEADER) });
            // Constraint type
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(Constants.COL_WIDTH_CONSTRAINT_TYPE) });
            // Constraint RHS
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(Constants.COL_WIDTH_RHS) });
            // Constraint dual value
            // TODO support duals
            //ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(Constants.COL_WIDTH) });
            // Init UI-element list
            _uiElements = new List2D<UIElement>(RowDefinitions.Count, ColumnDefinitions.Count);
            // --> Init indices
            _colIndexFirstElementSectionTwo = 1;
            _colIndexFirstElementSectionThree = 1;
            _rowIndexFirstElementSectionTwo = 4;
            _rowIndexFirstElementSectionThree = 4;
            // --> Add meta elements
            AddMetaElements();
        }

        private void InitSample()
        {
            // Init base
            Init();
            // Init sample
            // --> Add first variables
            AddVariable();
            AddVariable();
            AddVariable();
            // --> Add first constraints
            AddConstraint();
        }

        /// <summary>
        /// Clears the control.
        /// </summary>
        private void Clear()
        {
            RowDefinitions.Clear(); ColumnDefinitions.Clear();
            Children.Clear();
            _uiElements = null;
            _variables.Clear(); _constraints.Clear();
            _colIndexFirstElementSectionTwo = 0; _colIndexFirstElementSectionThree = 0;
            _rowIndexFirstElementSectionTwo = 0; _rowIndexFirstElementSectionThree = 0;
        }

        #endregion

        #region Meta fields

        /// <summary>
        /// The action used for logging.
        /// </summary>
        private Action<string> _logger;
        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        private void Log(string msg) { _logger?.Invoke(msg); }
        /// <summary>
        /// The action used for logging.
        /// </summary>
        public Action<string> Logger { get { return _logger; } set { _logger = value; } }

        #endregion

        #region Meta UI elements

        /// <summary>
        /// Adds all descriptive and other elements.
        /// </summary>
        private void AddMetaElements()
        {
            // Add LHS descriptions
            TextBlock tbUB = new TextBlock() { Text = "UB" };
            int indexUB = CalcRowIndex(Constants.POS_VAR_UB);
            _uiElements[indexUB, 0] = tbUB;
            SetColumn(tbUB, 0); SetRow(tbUB, indexUB);
            TextBlock tbLB = new TextBlock() { Text = "LB" };
            int indexLB = CalcRowIndex(Constants.POS_VAR_LB);
            _uiElements[indexLB, 0] = tbLB;
            SetColumn(tbLB, 0); SetRow(tbLB, indexLB);
            TextBlock tbType = new TextBlock() { Text = "Type" };
            int indexType = CalcRowIndex(Constants.POS_VAR_TYPE);
            _uiElements[indexType, 0] = tbType;
            SetColumn(tbType, 0); SetRow(tbType, indexType);
            TextBlock tbName = new TextBlock() { Text = "Name" };
            int indexName = CalcRowIndex(Constants.POS_VAR_NAME);
            _uiElements[indexName, 0] = tbName;
            SetColumn(tbName, 0); SetRow(tbName, indexName);
            TextBlock tbObj = new TextBlock() { Text = "Obj" };
            int indexObj = CalcRowIndex(Constants.POS_VAR_OBJ_COEFF);
            _uiElements[indexObj, 0] = tbObj;
            SetColumn(tbObj, 0); SetRow(tbObj, indexObj);
            TextBlock tbSol = new TextBlock() { Text = "Sol" };
            int indexSol = CalcRowIndex(Constants.POS_VAR_VALUE);
            _uiElements[indexSol, 0] = tbSol;
            SetColumn(tbSol, 0); SetRow(tbSol, indexSol);
            AddChildren(tbUB, tbLB, tbType, tbName, tbObj, tbSol);
            // Add RHS descriptions
            int indexRHSRow = _rowIndexFirstElementSectionTwo - 1;
            TextBlock tbRHS = new TextBlock() { Text = "RHS" };
            int indexRHSCol = CalcColIndex(Constants.POS_CON_RHS);
            _uiElements[indexRHSRow, indexRHSCol] = tbRHS;
            SetColumn(tbRHS, indexRHSCol); SetRow(tbRHS, indexRHSRow);
            // TODO support duals
            //int indexDualRow = _rowIndexFirstElementSectionTwo - 1;
            //TextBlock tbDual = new TextBlock() { Text = "Dual" };
            //int indexDualCol = CalcColIndex(Constants.POS_CON_DUAL);
            //_uiElements[indexDualRow, indexDualCol] = tbDual;
            //SetColumn(tbDual, indexDualCol); SetRow(tbDual, indexDualRow);
            AddChildren(tbRHS/*, tbDual*/);
            // Add overall status
            int indexModelStatusDescrRow = 0;
            int indexModelStatusDescrCol = _colIndexFirstElementSectionThree;
            TextBlock tbModelStatusDescr = new TextBlock() { Text = "Model:" };
            _uiElements[indexModelStatusDescrRow, indexModelStatusDescrCol] = tbModelStatusDescr;
            SetColumn(tbModelStatusDescr, indexModelStatusDescrCol); SetRow(tbModelStatusDescr, indexModelStatusDescrRow);
            int indexSolStatusRow = 0;
            int indexSolStatusCol = _colIndexFirstElementSectionThree + 1;
            _textBlockModelStatus = new TextBlock() { Text = SolutionStatus.NoSolutionValues.ToString(), Background = Constants.SolutionStatusColors[SolutionStatus.NoSolutionValues] };
            _uiElements[indexSolStatusRow, indexSolStatusCol] = _textBlockModelStatus;
            SetColumn(_textBlockModelStatus, indexSolStatusCol); SetRow(_textBlockModelStatus, indexSolStatusRow);
            SetColumnSpan(_textBlockModelStatus, 2);
            int indexSolStatusDescrRow = 0 + 1;
            int indexSolStatusDescrCol = _colIndexFirstElementSectionThree;
            TextBlock tbSolutionStatusDescr = new TextBlock() { Text = "Solution:" };
            _uiElements[indexSolStatusDescrRow, indexSolStatusDescrCol] = tbSolutionStatusDescr;
            SetColumn(tbSolutionStatusDescr, indexSolStatusDescrCol); SetRow(tbSolutionStatusDescr, indexSolStatusDescrRow);
            int indexModelStatusRow = 0 + 1;
            int indexModelStatusCol = _colIndexFirstElementSectionThree + 1;
            _textBlockSolutionStatus = new TextBlock() { Text = ModelStatus.Unknown.ToString(), Background = Constants.ModelStatusColors[ModelStatus.Unknown] };
            _uiElements[indexModelStatusRow, indexModelStatusCol] = _textBlockSolutionStatus;
            SetColumn(_textBlockSolutionStatus, indexModelStatusCol); SetRow(_textBlockSolutionStatus, indexModelStatusRow);
            SetColumnSpan(_textBlockSolutionStatus, 2);
            AddChildren(_textBlockModelStatus, tbModelStatusDescr, _textBlockSolutionStatus, tbSolutionStatusDescr);
            // Add objective information
            int indexObjValueDescrRow = _rowIndexFirstElementSectionThree;
            int indexObjValueDescrCol = _colIndexFirstElementSectionThree;
            TextBlock tbObjValueDescr = new TextBlock() { Text = "z = ", TextAlignment = TextAlignment.Right };
            _uiElements[indexObjValueDescrRow, indexObjValueDescrCol] = tbObjValueDescr;
            SetColumn(tbObjValueDescr, indexObjValueDescrCol); SetRow(tbObjValueDescr, indexObjValueDescrRow);
            int indexObjValueRow = _rowIndexFirstElementSectionThree;
            int indexObjValueCol = _colIndexFirstElementSectionThree + 1;
            _textBoxObjValue = new TextBox() { Text = double.NaN.ToString(Constants.FORMATTER) };
            _uiElements[indexObjValueRow, indexObjValueCol] = _textBoxObjValue;
            SetColumn(_textBoxObjValue, indexObjValueCol); SetRow(_textBoxObjValue, indexObjValueRow);
            AddChildren(tbObjValueDescr, _textBoxObjValue);
            // Add objective sense
            int indexObjSenseDescrRow = _rowIndexFirstElementSectionThree + 1;
            int indexObjSenseDescrCol = _colIndexFirstElementSectionThree;
            TextBlock tbObjSenseDescr = new TextBlock() { Text = "Sense:", TextAlignment = TextAlignment.Left };
            _uiElements[indexObjSenseDescrRow, indexObjSenseDescrCol] = tbObjSenseDescr;
            SetColumn(tbObjSenseDescr, indexObjSenseDescrCol); SetRow(tbObjSenseDescr, indexObjSenseDescrRow);
            int indexObjSenseRow = _rowIndexFirstElementSectionThree + 1;
            int indexObjSenseCol = _colIndexFirstElementSectionThree + 1;
            _comboBoxOptimizationSense = new ComboBox() { ItemsSource = Enum.GetValues(typeof(OptimizationDirection)).Cast<OptimizationDirection>().Select(t => t.ToString()), SelectedIndex = 0 };
            _uiElements[indexObjSenseRow, indexObjSenseCol] = _comboBoxOptimizationSense;
            SetColumn(_comboBoxOptimizationSense, indexObjSenseCol); SetRow(_comboBoxOptimizationSense, indexObjSenseRow);
            AddChildren(tbObjSenseDescr, _comboBoxOptimizationSense);
        }

        /// <summary>
        /// Represents the corresponding information.
        /// </summary>
        private TextBlock _textBlockSolutionStatus;
        /// <summary>
        /// Represents the corresponding information.
        /// </summary>
        private TextBlock _textBlockModelStatus;
        /// <summary>
        /// Represents the corresponding information.
        /// </summary>
        private TextBox _textBoxObjValue;
        /// <summary>
        /// The sense of the objective.
        /// </summary>
        private ComboBox _comboBoxOptimizationSense;
        /// <summary>
        /// The sense of the objective.
        /// </summary>
        private OptimizationDirection OptimizationDirection
        {
            get
            {
                OptimizationDirection direction = OptimizationDirection.Maximize;
                Dispatcher.Invoke(() => { direction = (OptimizationDirection)Enum.GetValues(typeof(OptimizationDirection)).GetValue(_comboBoxOptimizationSense.SelectedIndex); });
                return direction;
            }
        }

        #endregion

        #region Data management

        /// <summary>
        /// All variables in index-correct order.
        /// </summary>
        List<LinVariable> _variables = new List<LinVariable>();
        /// <summary>
        /// All constraints in index-correct order.
        /// </summary>
        List<LinConstraint> _constraints = new List<LinConstraint>();

        #endregion

        #region Index management

        /// <summary>
        /// The index of the first element of the coefficient matrix (columns).
        /// </summary>
        private int _colIndexFirstElementSectionTwo;
        /// <summary>
        /// The index of the first element of the section below the coefficient matrix (columns).
        /// </summary>
        private int _colIndexFirstElementSectionThree;
        /// <summary>
        /// The index of the first element of the coefficient matrix (rows).
        /// </summary>
        private int _rowIndexFirstElementSectionTwo;
        /// <summary>
        /// The index of the first element of the section right of the coefficient matrix (rows).
        /// </summary>
        private int _rowIndexFirstElementSectionThree;
        /// <summary>
        /// All UI-elements of this control.
        /// </summary>
        List2D<UIElement> _uiElements;
        /// <summary>
        /// Adds a range of UI-elements to the children collection.
        /// </summary>
        /// <param name="uiElements">The UI-elements to add.</param>
        private void AddChildren(params UIElement[] uiElements) { foreach (var ele in uiElements) Children.Add(ele); }
        /// <summary>
        /// Calculates the row index depending on the given info for the information type.
        /// </summary>
        /// <param name="info">The position information of the given type of information.</param>
        /// <returns>The index to use for the grid.</returns>
        private int CalcRowIndex(PositionInfo info) { return info.Below ? _rowIndexFirstElementSectionThree + info.Index : info.Index; }
        /// <summary>
        /// Calculates the column index depending on the given info for the information type.
        /// </summary>
        /// <param name="info">The position information of the given type of information.</param>
        /// <returns>The index to use for the grid.</returns>
        private int CalcColIndex(PositionInfo info) { return info.Below ? _colIndexFirstElementSectionThree + info.Index : info.Index; }

        #endregion

        #region Add / remove variables and constraints

        public void AddVariable(string line = null)
        {
            // Update indices
            int index = _colIndexFirstElementSectionThree;
            _colIndexFirstElementSectionThree++;
            // Add new column definition
            ColumnDefinitions.Insert(index, new ColumnDefinition() { Width = new GridLength(Constants.COL_WIDTH) });
            // Add basic content elements
            List<UIElement> uiElementCol = Enumerable.Repeat(default(UIElement), RowDefinitions.Count).ToList();
            TextBox tbUB = new TextBox() { Text = Constants.DEFAULT_VALUE_UB.ToString(Constants.FORMATTER) };
            tbUB.KeyDown += VerifyCharacter;
            uiElementCol[CalcRowIndex(Constants.POS_VAR_UB)] = tbUB;
            TextBox tbLB = new TextBox() { Text = Constants.DEFAULT_VALUE_LB.ToString(Constants.FORMATTER) };
            tbLB.KeyDown += VerifyCharacter;
            uiElementCol[CalcRowIndex(Constants.POS_VAR_LB)] = tbLB;
            ComboBox cbType = new ComboBox() { ItemsSource = Enum.GetNames(typeof(VariableType)), SelectedIndex = 0 };
            uiElementCol[CalcRowIndex(Constants.POS_VAR_TYPE)] = cbType;
            TextBox tbName = new TextBox() { Text = Constants.DEFAULT_VALUE_VAR_NAME_PREFIX + (_colIndexFirstElementSectionThree - _colIndexFirstElementSectionTwo).ToString(Constants.FORMATTER) };
            uiElementCol[CalcRowIndex(Constants.POS_VAR_NAME)] = tbName;
            TextBox tbCoeff = new TextBox() { Text = Constants.DEFAULT_VALUE_OBJ_COEFF.ToString(Constants.FORMATTER) };
            tbCoeff.KeyDown += VerifyCharacter;
            uiElementCol[CalcRowIndex(Constants.POS_VAR_OBJ_COEFF)] = tbCoeff;
            TextBox tbValue = new TextBox() { Text = Constants.DEFAULT_VALUE_VALUE.ToString(Constants.FORMATTER), IsReadOnly = true };
            uiElementCol[CalcRowIndex(Constants.POS_VAR_VALUE)] = tbValue;
            // Create variable
            LinVariable variable = line != null ?
                new LinVariable(tbName, tbLB, tbUB, cbType, tbCoeff, tbValue, line) :
                new LinVariable(tbName, tbLB, tbUB, cbType, tbCoeff, tbValue);
            _variables.Add(variable);
            // Add constraint specific content elements
            for (int con = 0; con < _constraints.Count; con++)
            {
                int conUIIndex = _rowIndexFirstElementSectionTwo + con;
                TextBox tbConCoeff = new TextBox() { Text = Constants.DEFAULT_VALUE_CON_COEFF.ToString(Constants.FORMATTER) };
                tbConCoeff.KeyDown += VerifyCharacter;
                uiElementCol[conUIIndex] = tbConCoeff;
                _constraints[con].AddVariable(variable, tbConCoeff);
            }
            // Add Ui-elements
            _uiElements.InsertColumn(index, uiElementCol);
            foreach (var uiEle in uiElementCol)
                Children.Add(uiEle);
            // --> Arrange all elements
            UpdateGridPositions();
        }

        public void RemVariable()
        {
            // If no variables available, do nothing
            if (_variables.Count == 0)
                return;
            // Set indices
            int uiIndex = _colIndexFirstElementSectionThree - 1;
            _colIndexFirstElementSectionThree--;
            int index = _variables.Count - 1;
            // Remove ui elements
            foreach (var ele in _uiElements.GetColumnElements(uiIndex))
                Children.Remove(ele);
            ColumnDefinitions.RemoveAt(uiIndex);
            _uiElements.RemoveColumn(uiIndex);
            // Remove variable itself
            _variables.RemoveAt(_variables.Count - 1);
            // --> Arrange all elements
            UpdateGridPositions();
        }

        public void AddConstraint(string line = null)
        {
            // Update indices
            int index = _rowIndexFirstElementSectionThree;
            _rowIndexFirstElementSectionThree++;
            // Add new row definition
            RowDefinitions.Insert(index, new RowDefinition() { Height = new GridLength(Constants.ROW_HEIGHT) });
            // Add basic content elements
            List<UIElement> uiElementRow = Enumerable.Repeat(default(UIElement), ColumnDefinitions.Count).ToList();
            TextBox tbName = new TextBox() { Text = Constants.DEFAULT_VALUE_CON_NAME_PREFIX + (_rowIndexFirstElementSectionThree - _rowIndexFirstElementSectionTwo).ToString(Constants.FORMATTER) };
            uiElementRow[CalcColIndex(Constants.POS_CON_NAME)] = tbName;
            ComboBox cbType = new ComboBox() { ItemsSource = Enum.GetValues(typeof(ConstraintType)).Cast<ConstraintType>().Select(t => Constants.ConvertConstraintTypeName(t)), SelectedIndex = 0 };
            uiElementRow[CalcColIndex(Constants.POS_CON_TYPE)] = cbType;
            TextBox tbRHS = new TextBox() { Text = Constants.DEFAULT_VALUE_RHS.ToString(Constants.FORMATTER) };
            tbRHS.KeyDown += VerifyCharacter;
            uiElementRow[CalcColIndex(Constants.POS_CON_RHS)] = tbRHS;
            // TODO support dual values
            //TextBox tbDual = new TextBox() { Text = Constants.DEFAULT_VALUE_VALUE.ToString(Constants.FORMATTER), IsReadOnly = true };
            //uiElementRow[CalcColIndex(Constants.POS_CON_DUAL)] = tbDual;
            // Create constraint
            List<TextBox> variableCoeffControls = _variables.Select(v =>
            {
                TextBox tb = new TextBox() { Text = Constants.DEFAULT_VALUE_CON_COEFF.ToString(Constants.FORMATTER) }; tb.KeyDown += VerifyCharacter; return tb;
            }).ToList();
            LinConstraint constraint = line != null ?
                new LinConstraint(tbName, cbType, tbRHS, /*tbDual,*/ _variables, variableCoeffControls, line) :
                new LinConstraint(tbName, cbType, tbRHS, /*tbDual,*/ _variables, variableCoeffControls);
            _constraints.Add(constraint);
            for (int v = 0; v < _variables.Count; v++)
                uiElementRow[_colIndexFirstElementSectionTwo + v] = variableCoeffControls[v];
            // Add Ui-elements
            _uiElements.InsertRow(index, uiElementRow);
            foreach (var uiEle in uiElementRow)
                Children.Add(uiEle);
            // --> Arrange all elements
            UpdateGridPositions();
        }

        public void RemConstraint()
        {
            // If no constraints available, do nothing
            if (_constraints.Count == 0)
                return;
            // Set indices
            int uiIndex = _rowIndexFirstElementSectionThree - 1;
            _rowIndexFirstElementSectionThree--;
            int index = _constraints.Count - 1;
            // Remove ui elements
            foreach (var ele in _uiElements.GetRowElements(uiIndex))
                Children.Remove(ele);
            RowDefinitions.RemoveAt(uiIndex);
            _uiElements.RemoveRow(uiIndex);
            // Remove variable itself
            _constraints.RemoveAt(index);
            // --> Arrange all elements
            UpdateGridPositions();
        }

        #endregion

        #region Solve

        /// <summary>
        /// Contains the currently active solver.
        /// </summary>
        private GLPKSolver _activeSolver = null;
        /// <summary>
        /// Solves the model.
        /// </summary>
        public void Solve()
        {
            // --> Sanity check
            if (_activeSolver != null)
                throw new InvalidOperationException("Solve process already in progress!s");
            if (!_variables.All(v => _variables.Where(otherV => otherV != v).All(otherV => otherV.Name != v.Name)))
                throw new LinModelException("Names of the variables have to be unique!");
            if (!_constraints.All(c => _constraints.Where(otherC => otherC != c).All(otherC => otherC.Name != c.Name)))
                throw new LinModelException("Names of the costraints have to be unique!");
            // --> Build model
            Model model = new Model();
            Dictionary<LinVariable, Variable> variableDict = new Dictionary<LinVariable, Variable>();
            // Sanity check for any valid variable
            if (!_variables.Any())
            {
                // This should never happen, but might happen for a nosy user
                throw new LinModelException("I can unfortunately not optimize a model without any variables!");
            }
            // Sanity check for any invalid bounds
            if (_variables.Any(v => v.LB > v.UB))
            {
                // This should never happen, but might happen for a nosy user
                throw new LinModelException("Please supply valid bounds - LB of variable " + _variables.First(v => v.LB > v.UB).Name + " is greater than it's UB!");
            }
            // Generate and add variables
            foreach (var variable in _variables)
            {
                Variable modelVar = new Variable(variable.Name, variable.LB, variable.UB, variable.Type == VariableType.Con ? Optimization.VariableType.Continuous : Optimization.VariableType.Integer);
                variableDict[variable] = modelVar;
            }
            // Add constraints
            Dictionary<LinConstraint, Constraint> constraintDict = new Dictionary<LinConstraint, Constraint>();
            foreach (var constraint in _constraints)
            {
                if (variableDict.Keys.All(v => constraint.GetCoeff(v) == 0))
                {
                    // Constraint is void
                    constraintDict[constraint] = null;
                }
                else
                {
                    // It's a real constraint - handle it
                    Constraint con = null;
                    switch (constraint.Type)
                    {
                        case ConstraintType.Le: con = Optimization.Expression.Sum(variableDict.Keys.Select(v => constraint.GetCoeff(v) * variableDict[v])) <= constraint.RHS; break;
                        case ConstraintType.Ge: con = Optimization.Expression.Sum(variableDict.Keys.Select(v => constraint.GetCoeff(v) * variableDict[v])) >= constraint.RHS; break;
                        case ConstraintType.Eq: con = Optimization.Expression.Sum(variableDict.Keys.Select(v => constraint.GetCoeff(v) * variableDict[v])) == constraint.RHS; break;
                        default: throw new LinModelException("Unknown constraint type: " + constraint.Type.ToString());
                    }
                    constraintDict[constraint] = con;
                    model.AddConstraint(con, constraint.Name);
                }
            }
            // Sanity check for any valid constraint
            if (constraintDict.Values.All(v => v == null))
            {
                // This should never happen, but might happen for a nosy user
                throw new LinModelException("I can unfortunately not optimize a model without any meaningful constraints!");
            }
            // Add objective
            if (!variableDict.Keys.All(v => v.Coeff == 0))
                model.AddObjective(
                    Optimization.Expression.Sum(variableDict.Keys.Select(v => v.Coeff * variableDict[v])),
                    "OBJ",
                    OptimizationDirection == OptimizationDirection.Maximize ? ObjectiveSense.Maximize : ObjectiveSense.Minimize);
            // Init and solve
            _activeSolver = _logger != null ? new GLPKSolver(_logger) : new GLPKSolver();
            Solution solution = _activeSolver.Solve(model);
            Dispatcher.Invoke(() =>
            {
                _textBlockModelStatus.Background = Constants.ModelStatusColors[solution.ModelStatus];
                _textBlockModelStatus.Text = solution.ModelStatus.ToString();
                _textBlockSolutionStatus.Background = Constants.SolutionStatusColors[solution.Status];
                _textBlockSolutionStatus.Text = solution.Status.ToString();
                _textBoxObjValue.Text = double.NaN.ToString(Constants.FORMATTER);
            });
            // Feed back solution
            if (solution.Status != SolutionStatus.NoSolutionValues)
            {
                // Get values
                IDictionary<string, double> valueDict = solution.VariableValues;
                foreach (var variable in variableDict)
                    variable.Key.Value = valueDict != null && valueDict.ContainsKey(variable.Key.Name) ? valueDict[variable.Key.Name] : double.NaN;
                //// Get duals
                // TODO support duals
                //IDictionary<string, double> dualValueDict = solution.DualVariableValues;
                //foreach (var constraint in constraintDict)
                //    constraint.Key.Dual = constraint.Value != null && dualValueDict != null && dualValueDict.ContainsKey(constraint.Key.Name) ? dualValueDict[constraint.Key.Name] : double.NaN;
                // Get objective
                _textBoxObjValue.Dispatcher.Invoke(() =>
                {
                    _textBoxObjValue.Text = solution.ObjectiveValues.Any() ? solution.ObjectiveValues.Single().Value.ToString(Constants.FORMATTER) : double.NaN.ToString(Constants.FORMATTER);
                });
            }
            // Mark solver inactive
            _activeSolver = null;
        }
        /// <summary>
        /// Stops the currently active solver, if there is one.
        /// </summary>
        public void StopSolve() { _activeSolver?.Abort(); }

        #endregion

        #region I/O

        public void Read(string file)
        {
            // --> Clear any old stuff
            Clear();
            Init();
            // --> Read file
            // Simply read all lines per blocks first
            Dictionary<string, List<string>> blocks = new Dictionary<string, List<string>>();
            using (StreamReader sr = new StreamReader(file))
            {
                // Read blocks
                string currentBlock = "";
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    // Skip comments and empty lines
                    if (line.StartsWith(Constants.IO_COMMENT) || string.IsNullOrWhiteSpace(line)) { continue; }
                    // If it is a block start to set everything up
                    if (line.StartsWith(Constants.IO_BLOCK_START))
                    {
                        line = line.Replace(Constants.IO_BLOCK_START, "").Trim();
                        if (line.Equals(Constants.IO_BLOCK_IDENT_VARIABLES)) { blocks[Constants.IO_BLOCK_IDENT_VARIABLES] = new List<string>(); currentBlock = Constants.IO_BLOCK_IDENT_VARIABLES; continue; }
                        else if (line.Equals(Constants.IO_BLOCK_IDENT_CONSTRAINTS)) { blocks[Constants.IO_BLOCK_IDENT_CONSTRAINTS] = new List<string>(); currentBlock = Constants.IO_BLOCK_IDENT_CONSTRAINTS; continue; }
                        else { continue; }
                    }
                    // Store every line of block
                    blocks[currentBlock].Add(line);
                }
            }
            // Parse variables
            if (blocks.ContainsKey(Constants.IO_BLOCK_IDENT_VARIABLES))
            {
                foreach (var variableLine in blocks[Constants.IO_BLOCK_IDENT_VARIABLES])
                    AddVariable(variableLine);
            }
            else { throw new LinModelException("Cannot find a variable block in the given file!"); }
            // Parse constraints
            if (blocks.ContainsKey(Constants.IO_BLOCK_IDENT_CONSTRAINTS))
            {
                foreach (var constraintLine in blocks[Constants.IO_BLOCK_IDENT_CONSTRAINTS])
                    AddConstraint(constraintLine);
            }
            else { throw new LinModelException("Cannot find a constraint block in the given file!"); }
        }

        /// <summary>
        /// Saves the model to a file.
        /// </summary>
        /// <param name="file">The file to save the model to.</param>
        public void Write(string file)
        {
            // Always save files with default ending
            if (!file.EndsWith("." + Constants.IO_FILE_ENDING))
                file += "." + Constants.IO_FILE_ENDING;
            // Sanity check variable names
            if (_variables.Select(v => v.Name).Distinct().Count() != _variables.Count)
                throw new LinModelException("Names of the variables have to be unique!");
            // Write it
            using (StreamWriter sw = new StreamWriter(file))
            {
                sw.WriteLine(Constants.IO_BLOCK_START + Constants.IO_BLOCK_IDENT_VARIABLES);
                foreach (var variable in _variables)
                    sw.WriteLine(variable.ToLine());
                sw.WriteLine(Constants.IO_BLOCK_START + Constants.IO_BLOCK_IDENT_CONSTRAINTS);
                foreach (var constraint in _constraints)
                    sw.WriteLine(constraint.ToLine(_variables));
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Updates all positions of all UI-elements within the grid.
        /// </summary>
        public void UpdateGridPositions()
        {
            int tabIndex = 0;
            for (int i = 0; i < _uiElements.RowCount; i++)
                for (int j = 0; j < _uiElements.ColCount; j++)
                    if (_uiElements[i, j] != null)
                    {
                        SetRow(_uiElements[i, j], i);
                        SetColumn(_uiElements[i, j], j);
                        if (_uiElements[i, j] is Control && !(_uiElements[i, j] is TextBox && (_uiElements[i, j] as TextBox).IsReadOnly))
                            (_uiElements[i, j] as Control).TabIndex = tabIndex++;
                    }
        }

        /// <summary>
        /// Verifies that no keys are pressed that lead to unwanted characters in a decimal textbox.
        /// </summary>
        /// <param name="sender">The textbox sending the event.</param>
        /// <param name="e">The event arguments.</param>
        private void VerifyCharacter(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Ignore control keys
            if (e.Key == System.Windows.Input.Key.Tab ||
                e.Key == System.Windows.Input.Key.LeftShift ||
                e.Key == System.Windows.Input.Key.RightShift ||
                e.Key == System.Windows.Input.Key.LeftCtrl ||
                e.Key == System.Windows.Input.Key.RightCtrl ||
                e.Key == System.Windows.Input.Key.LeftAlt ||
                e.Key == System.Windows.Input.Key.RightAlt)
                return;
            // See whether it's an allowed character
            if (!Constants.ALLOWED_CHARACTERS.Contains(e.Key))
            {
                // Reject character and notify user
                e.Handled = true;
                SystemSounds.Beep.Play();
            }
        }

        #endregion
    }

    /// <summary>
    /// Defines one variable.
    /// </summary>
    public class LinVariable
    {
        /// <summary>
        /// Creates a new variable.
        /// </summary>
        /// <param name="tbName">The control element for the corresponding information.</param>
        /// <param name="tbLB">The control element for the corresponding information.</param>
        /// <param name="tbUB">The control element for the corresponding information.</param>
        /// <param name="cbType">The control element for the corresponding information.</param>
        /// <param name="tbCoeff">The control element for the corresponding information.</param>
        /// <param name="tbValue">The control element for the corresponding information.</param>
        public LinVariable(TextBox tbName, TextBox tbLB, TextBox tbUB, ComboBox cbType, TextBox tbCoeff, TextBox tbValue)
        {
            _textBoxName = tbName;
            _textBoxLB = tbLB;
            _textBoxUB = tbUB;
            _textBoxCoeff = tbCoeff;
            _textBoxValue = tbValue;
            _comboBoxType = cbType;
        }

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        /// <param name="tbName">The control element for the corresponding information.</param>
        /// <param name="tbLB">The control element for the corresponding information.</param>
        /// <param name="tbUB">The control element for the corresponding information.</param>
        /// <param name="cbType">The control element for the corresponding information.</param>
        /// <param name="tbCoeff">The control element for the corresponding information.</param>
        /// <param name="tbValue">The control element for the corresponding information.</param>
        /// <param name="line">The line containing the initial values.</param>
        public LinVariable(TextBox tbName, TextBox tbLB, TextBox tbUB, ComboBox cbType, TextBox tbCoeff, TextBox tbValue, string line) : this(tbName, tbLB, tbUB, cbType, tbCoeff, tbValue)
        {
            string[] values = line.Split(Constants.IO_DELIMITER);
            if (values.Length != 5)
                throw new LinModelException("Insufficient values provided for a variable - line was: " + line);
            tbName.Dispatcher.Invoke(() =>
            {
                tbName.Text = values[0].Replace(":", "");
                double value;
                if (double.TryParse(values[1], NumberStyles.Any, Constants.FORMATTER, out value))
                    tbLB.Text = values[1];
                else
                    throw new LinModelException("Error parsing lower bound of variable - value provided: " + values[1]);
                if (double.TryParse(values[2], NumberStyles.Any, Constants.FORMATTER, out value))
                    tbUB.Text = values[2];
                else
                    throw new LinModelException("Error parsing upper bound of variable - value provided: " + values[2]);
                if (double.TryParse(values[3], NumberStyles.Any, Constants.FORMATTER, out value))
                    tbCoeff.Text = values[3];
                else
                    throw new LinModelException("Error parsing objective coefficient of variable - value provided: " + values[3]);
                int typeIndex = OrderedVariableTypeNames.IndexOf(values[4]);
                if (typeIndex >= 0)
                    cbType.SelectedIndex = typeIndex;
                else
                    throw new LinModelException("Error parsing type of variable - value provided: " + values[4]);
            });
        }

        #region View related fields

        /// <summary>
        /// UI-element of the corresponding information.
        /// </summary>
        private TextBox _textBoxName;
        /// <summary>
        /// UI-element of the corresponding information.
        /// </summary>
        private TextBox _textBoxLB;
        /// <summary>
        /// UI-element of the corresponding information.
        /// </summary>
        private TextBox _textBoxUB;
        /// <summary>
        /// UI-element of the corresponding information.
        /// </summary>
        private TextBox _textBoxCoeff;
        /// <summary>
        /// UI-element of the corresponding information.
        /// </summary>
        private TextBox _textBoxValue;
        /// <summary>
        /// UI-element of the corresponding information.
        /// </summary>
        private ComboBox _comboBoxType;

        #endregion

        #region Data retrieval

        /// <summary>
        /// The name of the variable.
        /// </summary>
        public string Name { get { string text = null; _textBoxName.Dispatcher.Invoke(() => { text = _textBoxName.Text; }); return text; } }
        /// <summary>
        /// The lower bound of the variable.
        /// </summary>
        public double LB
        {
            get
            {
                double value; string text = null; _textBoxLB.Dispatcher.Invoke(() => { text = _textBoxLB.Text; });
                if (double.TryParse(text, NumberStyles.Any, Constants.FORMATTER, out value)) return value;
                else if (string.IsNullOrWhiteSpace(text)) return 0;
                else throw new FormatException("Malformatted lower bound for variable " + Name + " (numbers should look like: \"" + Constants.EXAMPLE_NUMBER.ToString(Constants.FORMATTER) + "\")");
            }
        }
        /// <summary>
        /// The upper bound of the variable.
        /// </summary>
        public double UB
        {
            get
            {
                double value; string text = null; _textBoxUB.Dispatcher.Invoke(() => { text = _textBoxUB.Text; });
                if (double.TryParse(text, NumberStyles.Any, Constants.FORMATTER, out value)) return value;
                else if (string.IsNullOrWhiteSpace(text)) return 0;
                else throw new FormatException("Malformatted upper bound for variable " + Name + " (numbers should look like: \"" + Constants.EXAMPLE_NUMBER.ToString(Constants.FORMATTER) + "\")");
            }
        }
        /// <summary>
        /// The solution value of the variable.
        /// </summary>
        public double Value { set { _textBoxValue.Dispatcher.Invoke(() => { _textBoxValue.Text = value.ToString(Constants.FORMATTER); }); } }
        /// <summary>
        /// The coefficient of the variable in the objective.
        /// </summary>
        public double Coeff
        {
            get
            {
                double value; string text = null; _textBoxCoeff.Dispatcher.Invoke(() => { text = _textBoxCoeff.Text; });
                if (double.TryParse(text, NumberStyles.Any, Constants.FORMATTER, out value)) return value;
                else if (string.IsNullOrWhiteSpace(text)) return 0;
                else throw new FormatException("Malformatted objective coefficient for variable " + Name + " (numbers should look like: \"" + Constants.EXAMPLE_NUMBER.ToString(Constants.FORMATTER) + "\")");
            }
        }
        /// <summary>
        /// The type of the variable.
        /// </summary>
        public VariableType Type { get { int index = -1; _comboBoxType.Dispatcher.Invoke(() => { index = _comboBoxType.SelectedIndex; }); return (VariableType)Enum.GetValues(typeof(VariableType)).GetValue(index); } }

        #endregion

        #region I/O

        /// <summary>
        /// All variable type names in the right order.
        /// </summary>
        private static List<string> OrderedVariableTypeNames = Enum.GetNames(typeof(VariableType)).ToList();
        /// <summary>
        /// Puts all model-relevant information about this variable in one line and returns it.
        /// </summary>
        /// <returns>All information about the variable in one line.</returns>
        internal string ToLine()
        {
            return
                Name + ":" + Constants.IO_DELIMITER +
                LB.ToString(Constants.FORMATTER) + Constants.IO_DELIMITER +
                UB.ToString(Constants.FORMATTER) + Constants.IO_DELIMITER +
                Coeff.ToString(Constants.FORMATTER) + Constants.IO_DELIMITER +
                Type.ToString();
        }

        #endregion
    }

    /// <summary>
    /// Comprises one constraint.
    /// </summary>
    public class LinConstraint
    {
        /// <summary>
        /// Creates a new constraint with coefficients set for all given variables.
        /// </summary>
        /// <param name="tbName">The control containing the corresponding information.</param>
        /// <param name="tbRHS">The control containing the corresponding information.</param>
        /// <param name="cbType">The control containing the corresponding information.</param>
        /// <param name="variables">The variables already existing in the model.</param>
        /// <param name="variableCoeffControls">The controls containing the corresponding information.</param>
        public LinConstraint(TextBox tbName, ComboBox cbType, TextBox tbRHS, /*TextBox tbDual,*/ IEnumerable<LinVariable> variables, IEnumerable<TextBox> variableCoeffControls)
        {
            if (variables.Count() != variableCoeffControls.Count())
                throw new LinModelException("Variable count and coefficient control counts do not match!");
            _coefficientControls = variables.Zip(variableCoeffControls, (LinVariable variable, TextBox control) => { return new Tuple<LinVariable, TextBox>(variable, control); }).ToDictionary(k => k.Item1, v => v.Item2);
            _textBoxName = tbName;
            _comboBoxType = cbType;
            _textBoxRHS = tbRHS;
            // TODO support duals
            //_textBoxDual = tbDual;
        }

        /// <summary>
        /// Creates a new constraint with coefficients set for all given variables.
        /// </summary>
        /// <param name="tbName">The control containing the corresponding information.</param>
        /// <param name="tbRHS">The control containing the corresponding information.</param>
        /// <param name="cbType">The control containing the corresponding information.</param>
        /// <param name="variables">The variables already existing in the model.</param>
        /// <param name="variableCoeffControls">The controls containing the corresponding information.</param>
        /// <param name="line">A line containing the initial values.</param>
        public LinConstraint(TextBox tbName, ComboBox cbType, TextBox tbRHS, /*TextBox tbDual,*/ IEnumerable<LinVariable> variables, IEnumerable<TextBox> variableCoeffControls, string line) :
            this(tbName, cbType, tbRHS, variables, variableCoeffControls)
        {
            string[] values = line.Split(Constants.IO_DELIMITER);
            if (values.Length < 3)
                throw new LinModelException("Insufficient values provided for a constraint - line was: " + line);
            tbName.Dispatcher.Invoke(() =>
            {
                Dictionary<string, LinVariable> variablesByName = variables.ToDictionary(k => k.Name, v => v);
                tbName.Text = values[0].Replace(":", "");
                double value;
                int typeIndex = OrderedConstraintTypeNames.IndexOf(values[values.Length - 2]);
                if (typeIndex >= 0)
                    cbType.SelectedIndex = typeIndex;
                else
                    throw new LinModelException("Error parsing type of constraint - value provided: " + values[values.Length - 2]);
                if (double.TryParse(values[values.Length - 1], NumberStyles.Any, Constants.FORMATTER, out value))
                    tbRHS.Text = values[values.Length - 1];
                else
                    throw new LinModelException("Error parsing right hand side of constraint - value provided: " + values[values.Length - 1]);
                // Parse all coefficients - remove unnecessary values first
                values = values.Skip(1).Take(values.Length - 3).Where(v => v != "+").ToArray();
                if (values.Length % 2 == 1)
                    throw new LinModelException("Irregular constraint found - constraint was after removing '+' and additional fields: " + string.Join(" ", values));
                for (int i = 0; i + 1 < values.Length; i += 2)
                {
                    if (!variablesByName.ContainsKey(values[i + 1]))
                        throw new LinModelException("Unknown variable found in constraint: " + values[i + 1]);
                    if (double.TryParse(values[i], NumberStyles.Any, Constants.FORMATTER, out value))
                        _coefficientControls[variablesByName[values[i + 1]]].Text = values[i];
                    else
                        throw new LinModelException("Error parsing coefficient in constraint - value provided: " + values[i]);
                }
            });
        }

        /// <summary>
        /// Adds a variable to this constraint.
        /// </summary>
        /// <param name="variable">The variable to add.</param>
        /// <param name="coefficientControl">The control for inputting the coefficient.</param>
        public void AddVariable(LinVariable variable, TextBox coefficientControl) { _coefficientControls[variable] = coefficientControl; }

        #region View related fields

        /// <summary>
        /// Contains all controls for setting the coefficients through the GUI.
        /// </summary>
        private Dictionary<LinVariable, TextBox> _coefficientControls;
        /// <summary>
        /// UI-element of the corresponding information.
        /// </summary>
        private TextBox _textBoxName;
        /// <summary>
        /// UI-element of the corresponding information.
        /// </summary>
        private ComboBox _comboBoxType;
        /// <summary>
        /// UI-element of the corresponding information.
        /// </summary>
        private TextBox _textBoxRHS;
        /// <summary>
        /// UI-element of the corresponding information.
        /// </summary>
        // TODO support duals
        //private TextBox _textBoxDual;

        #endregion

        #region Data retrieval

        /// <summary>
        /// The name of the constraint.
        /// </summary>
        public string Name { get { string text = null; _textBoxName.Dispatcher.Invoke(() => { text = _textBoxName.Text; }); return text; } }
        /// <summary>
        /// The type of the constraint.
        /// </summary>
        public ConstraintType Type { get { int index = -1; _comboBoxType.Dispatcher.Invoke(() => { index = _comboBoxType.SelectedIndex; }); return (ConstraintType)Enum.GetValues(typeof(ConstraintType)).GetValue(index); } }
        /// <summary>
        /// The value of the right hand side.
        /// </summary>
        public double RHS
        {
            get
            {
                double value; string text = null; _textBoxRHS.Dispatcher.Invoke(() => { text = _textBoxRHS.Text; });
                if (double.TryParse(text, NumberStyles.Any, Constants.FORMATTER, out value)) return value;
                else if (string.IsNullOrWhiteSpace(text)) return 0;
                else throw new FormatException("Malformatted right hand side for constraint " + Name + " (numbers should look like: \"" + Constants.EXAMPLE_NUMBER.ToString(Constants.FORMATTER) + "\")");
            }
        }
        /// <summary>
        /// The dual value of the constraint.
        /// </summary>
        // TODO support duals
        //public double Dual { set { _textBoxDual.Dispatcher.Invoke(() => { _textBoxDual.Text = value.ToString(Constants.FORMATTER); }); } }
        /// <summary>
        /// Gets the coefficient of the given variable in this constraint.
        /// </summary>
        /// <param name="variable">The variable to lookup the coefficient for.</param>
        /// <returns>The coefficient of the given variable within this constraint.</returns>
        public double GetCoeff(LinVariable variable)
        {
            double value; string text = null; _coefficientControls[variable].Dispatcher.Invoke(() => { text = _coefficientControls[variable].Text; });
            if (double.TryParse(text, NumberStyles.Any, Constants.FORMATTER, out value)) return value;
            else if (string.IsNullOrWhiteSpace(text)) return 0;
            else throw new FormatException("Malformatted coefficient for variable " + variable.Name + " in constraint " + Name + " (numbers should look like: \"" + Constants.EXAMPLE_NUMBER.ToString(Constants.FORMATTER) + "\")");
        }

        #endregion

        #region I/O

        /// <summary>
        /// All constraint type names in the right order.
        /// </summary>
        private static List<string> OrderedConstraintTypeNames = Enum.GetValues(typeof(ConstraintType)).Cast<ConstraintType>().Select(t => Constants.ConvertConstraintTypeName(t)).ToList();
        /// <summary>
        /// Puts all model-relevant information about this constraint in one line and returns it.
        /// </summary>
        /// <param name="variablesInOrder">Contains all variables in the right order.</param>
        /// <returns>All information about the constraint in one line.</returns>
        internal string ToLine(IEnumerable<LinVariable> variablesInOrder)
        {
            return
                Name + ":" + Constants.IO_DELIMITER +
                string.Join(
                    " + ",
                    variablesInOrder.Select(v => GetCoeff(v).ToString(Constants.FORMATTER) + Constants.IO_DELIMITER + v.Name)) + Constants.IO_DELIMITER +
                Constants.ConvertConstraintTypeName(Type) + Constants.IO_DELIMITER +
                RHS.ToString(Constants.FORMATTER);
        }

        #endregion
    }

    #region Type definitions

    /// <summary>
    /// The type of the constraint.
    /// </summary>
    public enum ConstraintType
    {
        /// <summary>
        /// A lesser equals constraint (expression is lesser or equal to the right hand side).
        /// </summary>
        Le,
        /// <summary>
        /// A greater equals constraint (expression is greater or equal to the right hand side).
        /// </summary>
        Ge,
        /// <summary>
        /// An equals constraint (expression is equal to the right hand side).
        /// </summary>
        Eq,
    }
    /// <summary>
    /// The type of the variable.
    /// </summary>
    public enum VariableType
    {
        /// <summary>
        /// Defines a variable as continuous.
        /// </summary>
        Con,
        /// <summary>
        /// Defines a variable as integer.
        /// </summary>
        Int,
    }
    /// <summary>
    /// Indicates the optimization direction.
    /// </summary>
    public enum OptimizationDirection
    {
        /// <summary>
        /// Indicates that the objective will be maximized.
        /// </summary>
        Maximize,
        /// <summary>
        /// Indicates that the objective will be minimized.
        /// </summary>
        Minimize,
    }

    #endregion

    #region Exception definition

    /// <summary>
    /// An exception type that can be used to feedback errors during parsing linear model instances from file or GUI.
    /// </summary>
    public class LinModelException : ArgumentException
    {
        /// <summary>
        /// Creates a new instance of this exception.
        /// </summary>
        /// <param name="msg">The message to supply.</param>
        public LinModelException(string msg) : base(msg) { }
    }

    #endregion
}
