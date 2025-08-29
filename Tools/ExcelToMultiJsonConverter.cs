using System.Collections.Generic;

namespace Gameplay.Script.Tools
{
#if UNITY_EDITOR
    using System.Data;
    using UnityEngine;
    using UnityEditor;
    using ExcelDataReader;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;

    public class ExcelToMultiJsonConverter : EditorWindow
    {
        [MenuItem("Tools/Excel/Convert to Multiple JSONs")]
        static void Convert()
        {
            string path = EditorUtility.OpenFilePanel("Select Excel File", "", "xlsx,xls");
            if (string.IsNullOrEmpty(path)) return;
        
            string outputDir = Path.Combine(Application.dataPath, "StreamingAssets", "Excel2Json");
            Directory.CreateDirectory(outputDir);
        
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true // 第一行作为列名
                    }
                });
                foreach (DataTable table in dataSet.Tables)
                {
                    var rows = new List<Dictionary<string, string>>();
                    int k = 0;
                    foreach (DataRow row in table.Rows)
                    {
                        k++;
                        if (k == 1)
                            continue;
                        var rowDict = new Dictionary<string, string>();
                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            string columnName = table.Columns[i].ColumnName;
                            rowDict[columnName] = row[i].ToString();
                        }
                        rows.Add(rowDict);
                    }
                
                    string json = JsonConvert.SerializeObject(rows, Formatting.Indented);
                    string outputPath = Path.Combine(outputDir, $"{table.TableName}.json");
                    File.WriteAllText(outputPath, json);
                }
            }
        
            AssetDatabase.Refresh();
            Debug.Log("转换完成！");
        }
    }
#endif
}