using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver
{
    class Program
    {

        static void Main(string[] args)
        {
            int[][] sudoku = new int[9][];
            sudoku[0] = new int[9] { 0, 0, 0, 4, 3, 0, 0, 0, 6 };
            sudoku[1] = new int[9] { 3, 8, 6, 9, 2, 0, 4, 5, 0 };
            sudoku[2] = new int[9] { 0, 5, 9, 7, 8, 0, 1, 2, 3 };
            sudoku[3] = new int[9] { 0, 0, 0, 0, 9, 0, 5, 7, 0 };
            sudoku[4] = new int[9] { 5, 6, 0, 0, 0, 0, 0, 1, 9 };
            sudoku[5] = new int[9] { 0, 0, 7, 0, 0, 3, 0, 0, 0 };
            sudoku[6] = new int[9] { 7, 3, 0, 0, 0, 9, 2, 0, 0 };
            sudoku[7] = new int[9] { 0, 4, 5, 0, 6, 2, 0, 0, 0 };
            sudoku[8] = new int[9] { 0, 0, 0, 0, 5, 7, 0, 0, 1 };
            sudoku = GetSolvedSudoku(sudoku);

            foreach (var arr in sudoku)
            {
                foreach (var number in arr)
                {
                    Console.Write(number + "|");
                }
                Console.WriteLine();
            }
        }


        /// <summary>
        /// function that finds the first empty cell in a matrix--loops from left top to right bottom
        /// cell is considred empty if the value less 0, matrix must be 9X9, if there is no empty cell returns -1,-1
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static (int row, int col) FirstEmptyCellPosition(int[][] matrix)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (matrix[i][j] == 0)
                        return (i, j);
                }
            }
            return (-1, -1);
        }

        /// <summary>
        /// return all numbers that can possibly be entered into the specified cell
        /// </summary>
        /// <returns></returns>
        public static List<int> GetAllAvailableNumbers(int[][] matrix, (int row, int col) coordinates)
        {
            List<int> unavailableNumbers = new List<int>();
            if (matrix[coordinates.row][coordinates.col] != 0)
            {
                return unavailableNumbers;
            }
            for (int i = 0; i < 9; i++)
            {
                unavailableNumbers.Add(matrix[coordinates.row][i]);
                unavailableNumbers.Add(matrix[i][coordinates.col]);
                unavailableNumbers.Add(matrix[(i % 3) + (((coordinates.row / 3) * 3))][(i / 3) + (((coordinates.col / 3) * 3))]);
            }

            return Enumerable.Range(1, 9).Where(n => !unavailableNumbers.Contains(n)).ToList();
        }

        /// <summary>
        /// takes a sudoku board of 9X9 and fills in all the numbers that only have one option, 
        /// does this recursivle until there is no more cells to fill
        /// </summary>
        /// <returns></returns>
        public static int[][] FillAllKnownNumbers(int[][] matrix)
        {
            //create a deep copy of the matrix
            int[][] filled = matrix.Select(x => x.Select(x => x).ToArray()).ToArray();

            bool matrixWasModified = false;
            do
            {
                matrixWasModified = false;
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        var avialableNumbers = GetAllAvailableNumbers(filled, (i, j));
                        if (avialableNumbers.Count == 1)
                        {
                            filled[i][j] = avialableNumbers.First();
                            matrixWasModified = true;
                        }
                    }
                }
            } while (matrixWasModified);

            return filled;
        }

        
        /// <summary>
        /// returns the solution to a 9X9 sudoku puzzel, this method assumes that the puzzle is solvable
        /// the sudoku should be a 9X9 jagged array, for empty celss the value should be 0.
        /// </summary>
        /// <param name="sudoku"></param>
        /// <returns></returns>
        public static int[][] GetSolvedSudoku(int[][] sudoku)
        {
            sudoku = FillAllKnownNumbers(sudoku);

            List<int> availableNumbersForFirstEmptyCell = GetAllAvailableNumbers(sudoku, FirstEmptyCellPosition(sudoku));
            Stack<int> availableNumbers = new Stack<int>(availableNumbersForFirstEmptyCell);
            SudokuLayer layer = new SudokuLayer
            {
                Sudoku = sudoku,
                AvailableNumbers = availableNumbers
            };

            Stack<SudokuLayer> layers = new Stack<SudokuLayer>();
            layers.Push(layer);

            while (true)
            {

                //make a deep copy of the board
                SudokuLayer sudokuLayer = new SudokuLayer();
                sudokuLayer.Sudoku = layers.Peek().Sudoku;
                //now the puzzly might be solved
                if (FirstEmptyCellPosition(sudokuLayer.Sudoku) == (-1, -1))
                {
                    break;
                }
                //it might not be solved but there might be no possible value
                if (layers.Peek().AvailableNumbers.Count == 0)
                {
                    layers.Pop();
                    continue;
                }
                //it might not be solved with a possilbe value to put into the empty cell
                //get the number to be inserted in the first empty cell 
                int numberToInsert = layers.Peek().AvailableNumbers.Pop();
                (int row, int col) firstEmptyCell = FirstEmptyCellPosition(sudokuLayer.Sudoku);
                sudokuLayer.Sudoku[firstEmptyCell.row][firstEmptyCell.col] = numberToInsert;
                sudokuLayer.Sudoku = FillAllKnownNumbers(sudokuLayer.Sudoku);
                try
                {

                    sudokuLayer.AvailableNumbers = new Stack<int>(GetAllAvailableNumbers(sudokuLayer.Sudoku, FirstEmptyCellPosition(sudokuLayer.Sudoku)));
                }
                catch
                {

                }
                layers.Push(sudokuLayer);
            }

            return layers.Peek().Sudoku;
        }
    }

    public class SudokuLayer
    {
        private int[][] _sudoku;

        public int[][] Sudoku
        {
            get
            {
                return _sudoku;
            }
            set
            {
                //deep copy the value
                _sudoku = value.Select(a => a.Select(n => n).ToArray()).ToArray();
            }
        }
        public Stack<int> AvailableNumbers { get; set; }
    }
}
