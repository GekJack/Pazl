using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Xml;
namespace Pazl
{
    public class Pazl_resolve()
    {
        private static string sourceFilePath = "none";
        private static List<string> source_puzzle_pieces = new List<string>();
        private static List<string> puzzle = new List<string>();
        private static List<string> puzzle_temp = new List<string>();
        private static Dictionary<string, List<string>> source_puzzle_filtered_in_parts_to_connect = new Dictionary<string, List<string>>();
        private static Dictionary<int, List<string>> used_puzzle_parts = new Dictionary<int, List<string>>();
        public static int Main()
        {
            Console.WriteLine("Hi, this programme will help you solve a puzzle problem.");
            while (true)
            {
                Console.WriteLine("Write the path to the file that contains the puzzle pieces we need to put together.");
                while (true)
                {
                    Console.WriteLine("Write here:");
                    sourceFilePath = Console.ReadLine();
                    if (IsFileAccessible(sourceFilePath))
                    {
                        break;
                    }
                    Console.WriteLine("Wrong path to file");
                }
                Console.WriteLine("All right, I've got the file, I'll get to work");
                var res = CheckAllPiecesIsNumAndGetValues();
                if (res.testing)
                {
                    Console.WriteLine("All values correspond to the numbers");
                    break;
                }
                else
                {
                    Console.WriteLine($"Not correct {res.line}, check your file.(It must be a number and be 6 characters long)");
                    Thread.Sleep(5000);
                    Console.Clear();
                }
            }
            DictionaryGeneration();
            foreach (string start_piece in source_puzzle_pieces)
            {
                List<string> first = new List<string> {start_piece};
                used_puzzle_parts.Add(-1, first);
                AlgorithmFindPuzzle(start_piece, true, 0);
                AlgorithmFindPuzzle(start_piece, false, 0);
                if (puzzle_temp.Count != 0)
                {
                    HashSet<string> uniquePuzzles = new HashSet<string>(puzzle_temp);
                    string res = FindTheLongest(uniquePuzzles.ToList());
                    puzzle.Add(res);
                }
                used_puzzle_parts.Remove(-1);
            }
            HashSet<string> unique_final_puzzles = new HashSet<string>(puzzle);
            string final_result = FindTheLongest(unique_final_puzzles.ToList());
            Console.WriteLine(final_result);
            Console.ReadKey();
            return 0;
        }
        private static void ChangeKey(Dictionary<string, List<string>> dictionary, int num_rec, string old_key, bool return_key)
        {
            string newKey = $"{num_rec}_{old_key}";

            if (return_key)
            {
                if (dictionary.ContainsKey(newKey))
                {
                    List<string> list = dictionary[newKey];
                    List<string> newList = new List<string>(list);
                    dictionary[old_key] = newList;
                    dictionary.Remove(newKey);
                }
                else
                {
                    Console.WriteLine($"Ключ {newKey} не найден в словаре.");
                }
            }
            else
            {
                if (dictionary.ContainsKey(old_key))
                {
                    List<string> list = dictionary[old_key];
                    List<string> newList = new List<string>(list);
                    dictionary[newKey] = newList;
                    dictionary.Remove(old_key);
                }
                else
                {
                    Console.WriteLine($"Ключ {old_key} не найден в словаре.");
                }
            }
        }
        private static void AlgorithmFindPuzzle(string puzzle, bool isRightDirection, int num_rec)
        {
            string part_of_puzzle = isRightDirection ? puzzle.Substring(puzzle.Length - 2) : puzzle.Substring(0, 2);
            string directionKey = isRightDirection ? $"start_{part_of_puzzle}" : $"end_{part_of_puzzle}";

            List<string> puzzle_parts = new List<string>();
            if (source_puzzle_filtered_in_parts_to_connect.ContainsKey(directionKey))
            {
                List<string> piecesToRemove = new List<string>();
                puzzle_parts.AddRange(source_puzzle_filtered_in_parts_to_connect[directionKey]);
                foreach (string piece in puzzle_parts) {
                    bool allListsValid = true;
                    foreach (var kvp in used_puzzle_parts)
                    {
                        if (kvp.Key >= -1 && kvp.Key < num_rec)
                        {
                            if (kvp.Value.Contains(piece))
                            {
                                allListsValid = false;
                                break;
                            }
                        }
                    }
                    if (allListsValid)
                    {
                        if (!used_puzzle_parts.ContainsKey(num_rec))
                        {
                            used_puzzle_parts[num_rec] = new List<string>();
                        }
                        used_puzzle_parts[num_rec].Add(piece);
                    }
                    else
                    {
                        piecesToRemove.Add(piece);
                    }
                }
                ChangeKey(source_puzzle_filtered_in_parts_to_connect, num_rec, directionKey, false);
                foreach (string piece in piecesToRemove)
                {
                    puzzle_parts.Remove(piece);
                }
            }

            if (puzzle_parts.Count != 0)
            {
                foreach (string part in puzzle_parts)
                {
                    string new_puzzle = isRightDirection
                        ? ConnectingPuzzlePartRight(puzzle, part)
                        : ConnectingPuzzlePartLeft(puzzle, part);

                    puzzle_temp.Add(new_puzzle);
                    AlgorithmFindPuzzle(new_puzzle, true, num_rec + 1);
                    AlgorithmFindPuzzle(new_puzzle, false, num_rec + 1);
                }
                puzzle_temp.Remove(puzzle);
            }
            if (source_puzzle_filtered_in_parts_to_connect.ContainsKey(directionKey)) ChangeKey(source_puzzle_filtered_in_parts_to_connect, num_rec, directionKey, true);
            if (used_puzzle_parts.ContainsKey(num_rec)) used_puzzle_parts.Remove(num_rec);

        }

        private static void DictionaryGeneration()
        {
            string start = "start_";
            string end = "end_";
            foreach (string item in source_puzzle_pieces){
                string startPart = item.Substring(0, 2);
                string endPart = item.Substring(item.Length - 2, 2);
                string startKey = $"{start}{startPart}";
                string endKey = $"{end}{endPart}";
                if (source_puzzle_filtered_in_parts_to_connect.ContainsKey(startKey))
                {
                    source_puzzle_filtered_in_parts_to_connect[startKey].Add(item);
                }
                else
                {
                    source_puzzle_filtered_in_parts_to_connect[startKey] = new List<string> { item };
                }
                if (source_puzzle_filtered_in_parts_to_connect.ContainsKey(endKey))
                {
                    source_puzzle_filtered_in_parts_to_connect[endKey].Add(item);
                }
                else
                {
                    source_puzzle_filtered_in_parts_to_connect[endKey] = new List<string> { item };
                }
            }
        }
        private static (bool testing, string line) CheckAllPiecesIsNumAndGetValues()
        {
            (bool testing, string line) result = (false, "none");
            using (StreamReader sr = new StreamReader(sourceFilePath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if(int.TryParse(line,out int res))
                    {
                        result.testing = true;
                        if(!(line.Trim().Length == 6))
                        {
                            result.testing = false;
                            result.line = line;
                            source_puzzle_pieces.Clear();
                            break;
                        }
                        source_puzzle_pieces.Add(line);
                    }
                    else
                    {
                        result.testing = false;
                        result.line = line;
                        source_puzzle_pieces.Clear();
                        break;
                    }
                }
            }
            return result;
        }
        private static bool IsFileAccessible(string path)
        {
            try
            {
                using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private static string ConnectingPuzzlePartLeft(string main_part, string new_part)
        {
            string res_string = "";
            string start_of_main = main_part.Substring(0, 2);
            string end_of_new_part = new_part.Substring(new_part.Length - 2);
            if (end_of_new_part.Equals(start_of_main))
            {
                res_string = new_part.Substring(0, new_part.Length - 2) + main_part;
            }
            else
            {
                Console.WriteLine($"Error: {main_part} {new_part} wrong connection on the left");
                Environment.Exit(1);
            }
            return res_string;
        }
        private static string ConnectingPuzzlePartRight(string main_part, string new_part)
        {
            string res_string = "";
            string end_of_main = main_part.Substring(main_part.Length - 2);
            string start_of_new_part = new_part.Substring(0, 2);
            if (start_of_new_part.Equals(end_of_main))
            {
                res_string = main_part + new_part.Substring(2);
            }
            else
            {
                Console.WriteLine($"Error: {main_part} {new_part} wrong connection on the right");
                Environment.Exit(1);
            }
            return res_string;
        }
        private static string FindTheLongest(List<string> puzzles)
        {
            string the_longest_puzzle = puzzles[0];
            for (int i = 1; i < puzzles.Count; i++)
            {
                if (puzzles[i].Length > the_longest_puzzle.Length)
                {
                    the_longest_puzzle = puzzles[i];
                }
            }
            puzzles.Clear();
            return the_longest_puzzle;
        }
    }
}