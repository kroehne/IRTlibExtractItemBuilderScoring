using System;
using System.IO;
using System.Collections.Generic;
using System.IO.Compression;
using System.Xml.Serialization;
using System.Text.Json; 
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Xml;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ExtractItemBuilderScoring
{
    class Program
    {
        static void Main(string[] args)
        {
            string targetDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string sourceDirectory = targetDirectory;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("IRTlib: ExtractItemBuilderScoring ({0})\n", typeof(Program).Assembly.GetName().Version.ToString());
            Console.ResetColor();
            Console.WriteLine("- Source Directory: {0}", sourceDirectory);
            Console.WriteLine("- Target Directory: {0}", targetDirectory);

            if (args.Length > 0)
                sourceDirectory = args[0];
              
            if (args.Length > 1)
                targetDirectory = args[1];

            string _pattern = "*.zip";
            List<string> _files = new List<string>();
            _files.AddRange(Directory.GetFiles(sourceDirectory, _pattern, SearchOption.AllDirectories));

            List<CodebookVariable> _cbVariables = new List<CodebookVariable>();

            // READ FILES

            foreach (var _file in _files)
            {
                // inspect archive 

                string _stimulus_file = "";
                string _itemscore_file = "";
                string _itemName = "";
                string _runtimeVersion = "";
                int _itemSizeWidth = -1;
                int _itemSizeHeight = -1;
                List<string> _itemTasks = new List<string>();

                
                using (ZipArchive archive = ZipFile.OpenRead(_file))
                {
                    Dictionary<string, string> _scoringResourcesCache = new Dictionary<string, string>();
                    Dictionary<string, string> inputValidationPattern = new Dictionary<string, string>();

                    _itemName = Path.GetFileName(_file).Replace(".zip", "");
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.Name == "stimulus.json")
                        {
                            // 9.4 and newer
                            _stimulus_file = "stimulus.json";
                        }
                        else if (entry.Name.EndsWith(".stimulus.json"))
                        {
                            _itemName = entry.Name.Replace(".stimulus.json", "");
                            _stimulus_file = entry.Name;
                        }
                        else if (entry.Name.EndsWith(".cbaitemscore"))
                        {
                            _itemscore_file = entry.Name;
                        }
                        else if (entry.Name.EndsWith(@".condition_dsl"))
                        {
                            using (var stream = entry.Open())
                            {
                                TextReader _tr = new StreamReader(stream);
                                string tmp = _tr.ReadToEnd();
                                tmp = tmp.Replace(Environment.NewLine, " ");
                                _scoringResourcesCache.Add(entry.Name.Replace(@"scoringResources/", ""), tmp);
                            }
                        }
                        else if (entry.Name.EndsWith(@".cbaml"))
                        {
                            using (var stream = entry.Open())
                            {
                                TextReader _tr = new StreamReader(stream);
                                string xml = _tr.ReadToEnd();
                                XmlReader rdr = XmlReader.Create(new System.IO.StringReader(xml));
                                while (rdr.Read())
                                {
                                    if (rdr.NodeType == XmlNodeType.Element)
                                    {
                                        string _id = rdr.GetAttribute("userDefinedId");
                                        string _type = rdr.GetAttribute("xsi:type");
                                        string _key =  _id;
                                        if (_type == "cbaml:SingleLineInputField" || _type == "cbaml:InputField")
                                        {
                                            string _inputValidationPattern = rdr.GetAttribute("inputValidationPattern");
                                            if (!inputValidationPattern.ContainsKey(_key))
                                                inputValidationPattern.Add(_key, _inputValidationPattern);
                                            else
                                            {
                                                Console.WriteLine("UserdefinedID expected only once. ");
                                            }
                                        }
                                    }
                                }

                            }

                        } else
                        {
                            // other files
                        } 
                    }

                    ZipArchiveEntry _stimulus = archive.GetEntry(_stimulus_file);
                    if (_stimulus != null)
                    {
                        using (var stream = _stimulus.Open())
                        {
                            TextReader _tr = new StreamReader(stream);
                            string jsonString = _tr.ReadToEnd();
                            var stimulus = JsonSerializer.Deserialize<ExtractItemBuilderScoring.IB_8_13_to_9_x.Stimulus>(jsonString);
                            _runtimeVersion = stimulus.runtimeCompatibilityVersion;
                            _itemSizeWidth = stimulus.itemWidth;
                            _itemSizeHeight = stimulus.itemHeight;
                            _itemTasks = stimulus.tasks;
                        }
                    }

                    ZipArchiveEntry _cbaitemscore = archive.GetEntry(_itemscore_file);
                    if (_cbaitemscore != null)
                    {
                        using (var stream = _cbaitemscore.Open())
                        {
                            XmlSerializer cbaitemscoreSerializer = new XmlSerializer(typeof(ibxml_8_13_to_9_x.softcon.de.Item));
                            var _cbaitemscoreobject = (ibxml_8_13_to_9_x.softcon.de.Item)cbaitemscoreSerializer.Deserialize(stream);

                            foreach (var _itemScoreList in _cbaitemscoreobject.ItemScoreList)
                            {
                                Dictionary<string, CodebookVariable> _vardict = new Dictionary<string, CodebookVariable>();
                                foreach (var _class in _itemScoreList.ClassList)
                                {
                                    var _variable = new CodebookVariable()
                                    {
                                        Task = _itemScoreList.Name,
                                        Class = _class.Name,
                                        ClassComment = _class.Comment,
                                        VariableValues = new List<CodebookValue>(),
                                        ProjectFile = System.IO.Path.GetFileName(_itemName),
                                        ProjectName = _itemName,
                                        VariableName = ""
                                    };

                                    _cbVariables.Add(_variable);
                                    _vardict.Add(_variable.Class, _variable);
                                }

                                foreach (var _hit in _itemScoreList.HitList)
                                {
                                    if (_hit.ClassProperty != "")
                                    {
                                        string _inputPattern = "";
                                        Match match = Regex.Match(_scoringResourcesCache[_hit.FileReference], @"result_text\(([^)]*)\)");
                                        if (match.Groups.Count > 1)
                                        {
                                            string _arguments = match.Groups[1].Value.Trim();
                                            string[] _argumentsSeparated = _arguments.Split(";");
                                            for (int i = 0; i < _argumentsSeparated.Length; i++)
                                            {
                                                if (_inputPattern.Length > 0)
                                                    _inputPattern += "; ";
                                                string _userDefinedID = _argumentsSeparated[i].Trim();
                                                if (inputValidationPattern.ContainsKey(_userDefinedID))
                                                    _inputPattern += _userDefinedID + "(" + inputValidationPattern[_userDefinedID] + ")";
                                                else
                                                    _inputPattern += _userDefinedID ;
                                            } 
                                        }
                                            
                                        _vardict[_hit.ClassProperty].VariableValues.Add(new CodebookValue
                                        {
                                            ConditionName = _hit.Name,
                                            ValueLabel = "",
                                            IntegerValue = -1,
                                            ConditionSyntax = _scoringResourcesCache[_hit.FileReference],
                                            ConditionType = "hit",
                                            UseResultText =  _scoringResourcesCache[_hit.FileReference].ToLower().Contains("result_text(") ? 1 : 0,
                                            InputValidation = _inputPattern
                                        });
                                    }
                                    else
                                    {

                                    }
                                }

                                foreach (var _miss in _itemScoreList.MissList)
                                {
                                    if (_miss.ClassProperty != "")
                                    {
                                        string _inputPattern = "";
                                        Match match = Regex.Match(_scoringResourcesCache[_miss.FileReference], @"result_text\(([^)]*)\)");
                                        for (int i = 0; i < match.Groups.Count; i++)
                                        {
                                            if (_inputPattern.Length > 0)
                                                _inputPattern += "; ";
                                           string _userDefinedID = match.Groups[1].Value.Trim();
                                            if (inputValidationPattern.ContainsKey(_userDefinedID))
                                                _inputPattern += _userDefinedID + "(" + inputValidationPattern[_userDefinedID] + ")";
                                            else
                                                _inputPattern += _userDefinedID + ")";
                                        }
                                        
                                        _vardict[_miss.ClassProperty].VariableValues.Add(new CodebookValue
                                        {
                                            ConditionName = _miss.Name,
                                            ValueLabel = "",
                                            IntegerValue = -1,
                                            ConditionSyntax = _scoringResourcesCache[_miss.FileReference],
                                            ConditionType = "miss",
                                            UseResultText = _scoringResourcesCache[_miss.FileReference].ToLower().Contains("result_text(") ? 1 : 0,
                                            InputValidation  = _inputPattern
                                        });
                                    }
                                    else
                                    {

                                    }
                                }
                            }
                              
                        }
                    }
 

                }
                 
            }

            // WRITE EXCEL SUMMARY

            IWorkbook _workbook = new XSSFWorkbook();
            ISheet _sheet = _workbook.CreateSheet("Scoring");

            IFont _font = _workbook.CreateFont();
            _font.FontName = "Calibri";
            _font.IsBold = true;
            _font.Color = IndexedColors.Orange.Index;

            ICellStyle _style = _workbook.CreateCellStyle();
            _style.SetFont(_font);
            _style.FillBackgroundColor = IndexedColors.Black.Index;
            _style.FillPattern = FillPattern.SolidForeground;

            IRow _headRow = _sheet.CreateRow(0);
            _headRow.CreateCell(0).SetCellValue("ProjectFile (CBA ItemBuilder File Name)");
            _headRow.CreateCell(1).SetCellValue("ProjectName (CBA ItemBuilder Project Name)");
            _headRow.CreateCell(2).SetCellValue("Task (Task Name / Entry Point)");
            _headRow.CreateCell(3).SetCellValue("Class (Variable Name)");
            _headRow.CreateCell(4).SetCellValue("ValueName (Hit Name or Miss Name)");
            _headRow.CreateCell(5).SetCellValue("VariableName (Variable Name)");
            _headRow.CreateCell(6).SetCellValue("VariableLabel (Description of Variable)");
            _headRow.CreateCell(7).SetCellValue("Type (Hit Name or Miss)");
            _headRow.CreateCell(8).SetCellValue("Syntax (Scoring Syntax)");
            _headRow.CreateCell(9).SetCellValue("ResultText");
            _headRow.CreateCell(10).SetCellValue("InputValidation (Regular Expressions for Inputs)");

            for (int h = 0; h <= 10; h++)
            {
                _headRow.Cells[h].CellStyle = _style;
                _sheet.AutoSizeColumn(h);
            }

            int _sheetItemScoreRow = 1;
            foreach (var v in _cbVariables)
            {
                foreach (var h in v.VariableValues)
                { 
                    IRow _row = _sheet.CreateRow(_sheetItemScoreRow);
                    _row.CreateCell(0).SetCellValue(v.ProjectFile + ".zip");
                    _row.CreateCell(1).SetCellValue(v.ProjectName);
                    _row.CreateCell(2).SetCellValue(v.Task); 
                    _row.CreateCell(3).SetCellValue(v.Class);
                    _row.CreateCell(4).SetCellValue(h.ConditionName);
                    _row.CreateCell(5).SetCellValue(v.VariableName); 
                    _row.CreateCell(6).SetCellValue(v.ClassComment); 
                    _row.CreateCell(7).SetCellValue(h.ConditionType);
                    _row.CreateCell(8).SetCellValue(h.ConditionSyntax); 
                    _row.CreateCell(9).SetCellValue(h.UseResultText);
                    _row.CreateCell(10).SetCellValue(h.InputValidation);
                    _sheetItemScoreRow += 1;

                }

                FileStream sw = File.Create(Path.Combine(targetDirectory, "ScoringSummary.xlsx"));
                _workbook.Write(sw);
                sw.Close();

            }

        }
    }

    public class CodebookVariable
    {
        public string ProjectFile { get; set; }
        public string ProjectName { get; set; }
        public string Task { get; set; }
        public string Class { get; set; }
        public string ClassComment { get; set; }
        public string VariableName { get; set; }

        public List<CodebookValue> VariableValues { get; set; }

    }

    public class CodebookValue
    {
        public string ConditionName { get; set; }
        public string ConditionType { get; set; } 
        public int IntegerValue { get; set; }
        public string ValueLabel { get; set; }
        public string ConditionSyntax { get; set; }
        public int UseResultText { get; set; }
        public string InputValidation { get; set; }
    }

}
