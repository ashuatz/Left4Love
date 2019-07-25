using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Linq;

#if UNITY_EDITOR

using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.Collections.Generic;

public class ExcelParser : Editor
{
    //Path : Assets 폴더에서 시작 
    private const string table_input_path = "/ExcelTable";
    private const string data_output_path = "/Resources/Table";
    
    //파일포맷 수정
    private static readonly string[] excel_file_extensions = { ".xls", ".xlsx" };
    private const string delimiter = "\t";

    private const string enum_table_file_name = "EnumTable";
    private const string world_level_table_file_name = "WorldLevelTable";

    //몇번째 줄부터 
    private const int start_line_index = 2;

    [MenuItem("Table/ExcelParser")]
    private static void ExcelParser_Default()
    {
        StringBuilder TotalData = new StringBuilder();

        DirectoryInfo D_Info = new DirectoryInfo(string.Format("{0}{1}", Application.dataPath, table_input_path));

        //현재 xls 파일만 parse 하는 로직이라 xlsx도 추가함
        FileInfo[] F_Info = D_Info.GetFiles("*" + excel_file_extensions[0]);
        //수정한 부분
        for (int i = 1; i < excel_file_extensions.Length; ++i)
        {
            int lastIndex = F_Info.Length;
            FileInfo[] addedF_Info = D_Info.GetFiles("*" + excel_file_extensions[i]);
            System.Array.Resize<FileInfo>(ref F_Info, F_Info.Length + addedF_Info.Length);
            System.Array.Copy(addedF_Info, 0, F_Info, lastIndex, addedF_Info.Length);
        }

        System.Action<StringBuilder, FileInfo, ISheet> CurrentAction = null;

        foreach (FileInfo info in F_Info)
        {
            if (info.Name.Contains(world_level_table_file_name))
                CurrentAction = MakeWorldLevelData;
            else
                CurrentAction = MakeSheetData;


            ISheet sheet;

            if (info.Extension.Equals(excel_file_extensions[0]))
            {
                HSSFWorkbook hssfwb;
                using (FileStream file = new FileStream(info.FullName, FileMode.Open, FileAccess.Read))
                {
                    hssfwb = new HSSFWorkbook(file);
                }

                for (int k = 0; k < hssfwb.NumberOfSheets; k++)
                {
                    sheet = hssfwb.GetSheetAt(k);

                    if (sheet.SheetName.Contains("Enum"))
                        MakeEnumData(TotalData, info, sheet);
                    else
                        CurrentAction(TotalData, info, sheet);
                }
            }
            else if(info.Extension.Equals(excel_file_extensions[1])) //xlsx
            {
                XSSFWorkbook xssfwb;
                using (FileStream file = new FileStream(info.FullName, FileMode.Open, FileAccess.Read))
                {
                    xssfwb = new XSSFWorkbook(file);
                }

                for (int k = 0; k < xssfwb.NumberOfSheets; k++)
                {
                    sheet = xssfwb.GetSheetAt(k);

                    if (sheet.SheetName.Contains("Enum"))
                        MakeEnumData(TotalData, info, sheet);
                    else
                        CurrentAction(TotalData, info, sheet);
                }
            }

        }

        AssetDatabase.Refresh();

        Debug.Log("Excel Parsing Done");

        EditorUtility.DisplayDialog(
              "Excel Parsing",
              "Done",
              "OK"
          );
    }

    #region EnumTableParser

    private static void MakeEnumData(StringBuilder TotalData, FileInfo info, ISheet sheet)
    {
        TotalData.Remove(0, TotalData.Length);

        IRow row;

        Dictionary<string, List<string>> enumClassDict = new Dictionary<string, List<string>>();
        
        string str = string.Empty;

        TotalData.Append(pre_enum_script);

        string lastName = string.Empty;
        for (int i = 0; i <= sheet.LastRowNum; i++)
        {
            row = sheet.GetRow(i);

            if (row == null)
                continue;

            if (row.GetCell(0) == null)
                continue;

            //사랑합니다 닷넷 4.x!!!!!!!!!!!!!!!!!!!!!!!!!!

            var enumName = row.GetCell(0).StringCellValue;
            if (lastName.Equals(enumName) == false && enumClassDict.ContainsKey(lastName))
            {
                enumClassDict[lastName].Add(enum_define_close);
            }
            if (!enumClassDict.TryGetValue(enumName, out var list))
            {
                list = new List<string>();
                enumClassDict.Add(enumName, list);
                list.Add(string.Format(enum_define_format, enumName));
                list.Add(enum_define_open);
            }


            List<string> enumElement = new List<string>();
            foreach (ICell cell in row)
            {
                str = string.Empty;

                switch (cell.CellType)
                {
                    case CellType.String:
                        str = cell.StringCellValue;
                        break;

                    case CellType.Numeric:
                        if (HSSFDateUtil.IsCellDateFormatted(cell))
                            str = cell.DateCellValue.ToString();
                        else
                            str = cell.NumericCellValue.ToString();
                        break;

                    case CellType.Formula:
                        switch (cell.CachedFormulaResultType) // =a1+b2 ..
                        {
                            case CellType.String:
                                str = cell.StringCellValue;
                                break;
                            case CellType.Numeric:
                                str = cell.NumericCellValue.ToString();
                                break;
                            default:
                                Debug.LogError(string.Format("[Parser]Invaild Type Exception : Cell type is {0}", cell.CachedFormulaResultType));
                                break;
                        }
                        break;

                    case CellType.Blank:
                        break;

                    default:
                        Debug.LogError(string.Format("[Parser]Out of case Exception : Cell type is {0}", cell.CellType));
                        break;
                }

                enumElement.Add(str);
            }

            enumClassDict[enumName].Add(string.Format(enum_define_element, enumElement[1], enumElement[2], enumElement.Count > 3 ? enumElement[3] : string.Empty));

            lastName = enumName;
        }
        

        foreach(var kvp in enumClassDict)
        {
            var e = kvp.Value.GetEnumerator();
            while (e.MoveNext())
            {
                TotalData.Append(e.Current);
            }
        }

        TotalData.Append(enum_define_close);

        TotalData.Append(post_enum_script);


        string OutputPath = Application.dataPath + ExcelParser.data_output_path + "/" + Path.GetFileNameWithoutExtension(sheet.SheetName) + ".cs";

        File.WriteAllText(OutputPath, TotalData.ToString());
    }

    private const string pre_enum_script =
@"//이 코드는 엑셀 파서에 의해 자동 생성됨. 
using System;

namespace Cubeat.DataTable
{
";

    private const string enum_define_format = "    public enum {0}";
    private const string enum_define_open = "\n    {\n";
    private const string enum_define_element = "        {0} = {1} , // {2} \n";
    private const string enum_define_close = "    }\n";
    private const string post_enum_script = @"}";
    #endregion

    #region LevelParser

    private static void MakeWorldLevelData(StringBuilder TotalData, FileInfo info, ISheet sheet)
    {
        TotalData.Remove(0, TotalData.Length);

        IRow row;


        Dictionary<int,List<string>> inverseList = new Dictionary<int, List<string>>();

        string str = string.Empty;

        for (int i = sheet.LastRowNum; i >= 0; --i)
        {
            row = sheet.GetRow(i);

            if (row == null)
                continue;

            if (row.GetCell(0) == null)
                continue;


            int ci = 0;
            foreach (ICell cell in row)
            {
                str = string.Empty;

                switch (cell.CellType)
                {
                    case CellType.String:
                        str = cell.StringCellValue;
                        break;

                    case CellType.Numeric:
                        if (HSSFDateUtil.IsCellDateFormatted(cell))
                            str = cell.DateCellValue.ToString();
                        else
                            str = cell.NumericCellValue.ToString();
                        break;

                    case CellType.Formula:
                        switch (cell.CachedFormulaResultType) // =a1+b2 ..
                        {
                            case CellType.String:
                                str = cell.StringCellValue;
                                break;
                            case CellType.Numeric:
                                str = cell.NumericCellValue.ToString();
                                break;
                            default:
                                Debug.LogError(string.Format("[Parser]Invaild Type Exception : Cell type is {0}", cell.CachedFormulaResultType));
                                break;
                        }
                        break;

                    case CellType.Blank:
                        break;

                    case CellType.Boolean:
                        str = cell.BooleanCellValue.ToString();
                        break;

                    default:
                        Debug.LogError(string.Format("[Parser]Out of case Exception : Cell type is {0}", cell.CellType));
                        break;
                }

                if (!inverseList.TryGetValue(ci, out var list))
                {
                    inverseList[ci] = new List<string>();
                }
                inverseList[ci].Add(str);

                //TotalData.Append(str);
                //TotalData.Append(delimiter);
                ci++;
            }
        }

        var keyList = inverseList.Keys.ToList();
        keyList.Reverse();
        foreach (var key in keyList)
        {
            for (int i = 0; i < inverseList[key].Count; ++i)
            {
                TotalData.Append(inverseList[key][i]);
                TotalData.Append(delimiter);
            }
        }


        string OutputPath = Application.dataPath + ExcelParser.data_output_path + "/Level/" + Path.GetFileNameWithoutExtension(sheet.SheetName.Split('_')[1]) + ".txt";

        File.WriteAllText(OutputPath, TotalData.ToString());
    }


    #endregion

    #region ExcelParser

    private static void MakeSheetData(StringBuilder TotalData, FileInfo info, ISheet sheet)
    {
        TotalData.Remove(0, TotalData.Length);

        IRow row;

        string str = string.Empty;

        MakeCSFile(sheet.SheetName.Remove(0, 2), sheet.GetRow(0));

        for (int i = start_line_index; i <= sheet.LastRowNum; i++)
        {
            row = sheet.GetRow(i);

            if (row == null)
                continue;

            if (row.GetCell(0) == null)
                continue;

            foreach (ICell cell in row)
            {
                str = string.Empty;

                switch (cell.CellType)
                {
                    case CellType.String:
                        str = cell.StringCellValue;
                        break;

                    case CellType.Numeric:
                        if (HSSFDateUtil.IsCellDateFormatted(cell))
                            str = cell.DateCellValue.ToString();
                        else
                            str = cell.NumericCellValue.ToString();
                        break;

                    case CellType.Formula:
                        switch (cell.CachedFormulaResultType) // =a1+b2 ..
                        {
                            case CellType.String:
                                str = cell.StringCellValue;
                                break;
                            case CellType.Numeric:
                                str = cell.NumericCellValue.ToString();
                                break;
                            default:
                                Debug.LogError(string.Format("[Parser]Invaild Type Exception : Cell type is {0}", cell.CachedFormulaResultType));
                                break;
                        }
                        break;

                    case CellType.Blank:
                        break;

                    case CellType.Boolean:
                        str = cell.BooleanCellValue.ToString();
                        break;

                    default:
                        Debug.LogError(string.Format("[Parser]Out of case Exception : Cell type is {0}", cell.CellType));
                        break;
                }
                TotalData.Append(str);
                TotalData.Append(delimiter); 
            }
            
            TotalData.Append("\n");

        }

        string OutputPath = Application.dataPath + ExcelParser.data_output_path + "/CSV/" + Path.GetFileNameWithoutExtension(sheet.SheetName.Split('_')[1]) + ".txt";

        File.WriteAllText(OutputPath, TotalData.ToString());
    }

    private static void MakeCSFile(string name, IRow defineRow)
    {
        StringBuilder fileData = new StringBuilder();
        fileData.Append(pre_script);
        var str = string.Format(script_class_define_format, name) + "{ \n";
        fileData.Append(str);

        foreach (ICell cell in defineRow)
        {
            if (cell.StringCellValue.Length <= 1)
                continue;

            int count = 0;
            string typeName = string.Empty;
            string variableName = GetVariableName(cell.StringCellValue, ref typeName, out count);

            //public string[] _name = new string[count]
            fileData.Append(string.Format(script_variable_define_format, typeName, variableName, GetInitializerFromType(typeName, count)) + "\n");
        }
        //public static <SomeThing> Load(string[] parts)
        fileData.Append(string.Format(script_function_start, name));
        fileData.Append(script_function_part_1);
        fileData.Append(script_function_part_2);
        fileData.Append(string.Format(script_function_class_initializer_format, name));

        foreach (ICell cell in defineRow)
        {
            if (cell.StringCellValue.Length <= 1)
                continue;

            int count = 0;
            string typeName = string.Empty;
            string variableName = GetVariableName(cell.StringCellValue, ref typeName, out count);
            string parser = GetParserFromType(typeName);

            if (typeName.Contains("["))
            {
                //doing
                //p.something[0] = parts[i++];
                //p.something[1] = parts[i++];
                //p.something[2] = parts[i++];
                
                for (int i = 0; i < count; ++i)
                {
                    fileData.Append(string.Format(script_parsing_board,
                        string.Format("{0}[{1}]", variableName, i),
                        parser) + "\n");
                }
            }
            else
            {
                fileData.Append(string.Format(script_parsing_board, variableName, parser) + "\n");
            }
        }

        fileData.Append(post_script);
        
        string path = Application.dataPath + data_output_path + "/" + Path.GetFileNameWithoutExtension(name) + ".cs";

        File.WriteAllText(path, fileData.ToString());

        AssetDatabase.Refresh();
    }

    #region For Auto Generate Script

    //i_something_name
    //^here
    private static string GetTypeForString(string _prefix)
    {
        switch (_prefix)
        {
            case "i": return "int";
            case "s": return "string";
            case "f": return "float";
            case "b": return "bool";

            default: return _prefix;
        }
    }

    //something;    or
    //something = new someType[count];
    private static string GetInitializerFromType(string type, int count)
    {
        if (type.Contains("["))
        {
            return string.Format(script_array_initializer_format, type.Split('[')[0], count);
        }
        else
        {
            return ";";
        }

    }

    private static string GetVariableName(string name, ref string typeName, out int count)
    {
        typeName = name.Split('_')[0];
        var prefixSize = typeName.Length;
        if (prefixSize == 1)
        {
            //i,f,s prefixes
            name = name.Remove(0, 1);
        }
        else
        {
            //enum
            name = name.Remove(0, prefixSize);
        }

        typeName = GetTypeForString(typeName);

        if (name.Contains("["))
        {
            typeName += "[]";
            count = int.Parse(name.Split('[')[1][0].ToString());
            name = name.Split('[')[0];
        }
        else
        {
            count = 0;
        }

        return name;
    }

    private static string GetParserFromType(string type)
    {
        switch (type)
        {
            case "int":
            case "int[]": return "int.Parse(parts[i++])";

            case "string":
            case "string[]": return "parts[i++]";

            case "float":
            case "float[]": return "float.Parse(parts[i++])";

            case "bool":
            case "bool[]": return "bool.Parse(parts[i++])";

            default:
                if (type.Contains("["))
                    type = type.Split('[')[0];

                return string.Format("({0})int.Parse(parts[i++])", type);
        }

    }

    private const string pre_script =
@"//이 코드는 엑셀 파서에 의해 자동 생성됨. 
using System;
using System.IO;

namespace Cubeat.DataTable
{
";
    private const string script_class_define_format =
        "    public class {0} ";
    private const string script_variable_define_format =
        "        public {0} {1}{2}";
    private const string script_function_start =
        "        public static {0} Load(string[] parts) ";
    private const string script_function_part_1 =
        "{\n";
    private const string script_function_part_2 =
        "            int i = 0;\n";
    private const string script_function_class_initializer_format =
        "            {0} p = new {0}();\n";

    private const string script_array_initializer_format =
        " = new {0}[{1}];";
    private const string script_parsing_board =
        "            p.{0} = {1};";

    private const string post_script =
@"
        return p;
        }
    }
}
";

    #endregion
    #endregion
}

#endif