using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinearModelControlLib
{
    /// <summary>
    /// Implements a twodimensional list.
    /// </summary>
    /// <typeparam name="T">The type of the elements contained in the list.</typeparam>
    public class List2D<T>
    {
        /// <summary>
        /// Creates a new twodimensional list.
        /// </summary>
        /// <param name="minRows">The minimal number of rows to start with.</param>
        /// <param name="minCols">The minimal number of columns to start with.</param>
        public List2D(int minRows, int minCols)
        {
            for (int i = 0; i < minRows; i++)
                for (int j = 0; j < minCols; j++)
                    this[i, j] = default(T);
            RowCount = minRows;
            ColCount = minCols;
        }

        /// <summary>
        /// The actual storage.
        /// </summary>
        private List<List<T>> _storage = new List<List<T>>();

        /// <summary>
        /// The number of rows in the list.
        /// </summary>
        public int RowCount { get; private set; }
        /// <summary>
        /// The number of columns in the list.
        /// </summary>
        public int ColCount { get; private set; }

        /// <summary>
        /// Gets or sets the element at the given position.
        /// </summary>
        /// <param name="i">The row index of the position.</param>
        /// <param name="j">The column index of the position.</param>
        /// <returns>The element at the position.</returns>
        public T this[int i, int j]
        {
            get
            {
                // Simply return the element at the position (exception will be thrown if out of bounds)
                return _storage[i][j];
            }
            set
            {
                // Add rows, if there are insufficient rows
                while (i >= RowCount)
                {
                    _storage.Add(Enumerable.Range(0, ColCount).Select(e => default(T)).ToList());
                    RowCount = _storage.Count;
                }
                // Add columns, if there are insufficient columns
                while (j >= ColCount)
                {
                    foreach (var row in _storage)
                        row.Add(default(T));
                    ColCount = _storage[0].Count;
                }
                // Set element
                _storage[i][j] = value;
            }
        }

        /// <summary>
        /// Gets all elements of a row.
        /// </summary>
        /// <param name="i">The index of the row.</param>
        /// <returns>The elements of the row.</returns>
        public IEnumerable<T> GetRowElements(int i) { return _storage[i]; }
        /// <summary>
        /// Gets all elements of a column.
        /// </summary>
        /// <param name="j">The index of the column.</param>
        /// <returns>The elements of the column.</returns>
        public IEnumerable<T> GetColumnElements(int j) { return Enumerable.Range(0, RowCount).Select(i => _storage[i][j]); }
        /// <summary>
        /// Inserts a row at the specified index.
        /// </summary>
        /// <param name="i">The index of the row.</param>
        /// <param name="row">The row to insert (has to have the exact amount of elements matching the number of columns).</param>
        public void InsertRow(int i, IEnumerable<T> row)
        {
            if (ColCount == 0)
                ColCount = row.Count();
            _storage.Insert(i, row.ToList());
            RowCount++;
            if (_storage[i].Count != ColCount)
                throw new ArgumentException("Invalid number of elements to insert!");
        }
        /// <summary>
        /// Inserts a column at the specified index.
        /// </summary>
        /// <param name="j">The index of the column.</param>
        /// <param name="column">The column to insert (has to have the exact amount of elements matching the number of rows).</param>
        public void InsertColumn(int j, IEnumerable<T> column)
        {
            if (RowCount == 0)
                RowCount = column.Count();
            int i = 0;
            foreach (var ele in column)
            {
                _storage[i].Insert(j, ele);
                i++;
            }
            ColCount++;
            if (i != RowCount)
                throw new ArgumentException("Invalid number of elements to insert!");
        }
        /// <summary>
        /// Removes the row at the given index.
        /// </summary>
        /// <param name="i">The index of the row.</param>
        public void RemoveRow(int i)
        {
            _storage.RemoveAt(i);
            RowCount--;
        }
        /// <summary>
        /// Removes the column at the given index.
        /// </summary>
        /// <param name="j">The column at the index.</param>
        public void RemoveColumn(int j)
        {
            foreach (var row in _storage)
                row.RemoveAt(j);
            ColCount--;
        }
    }
}
