using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEditor;
using UnityEngine;

namespace _TDS.Editor
{
    public static class HeroConfigExcelGenerator
    {
        [MenuItem("Tools/Config/Generate HeroConfig Excel")]
        public static void Generate()
        {
            string folder = "Assets/Excels";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string path = $"{folder}/HeroConfig.xlsx";

            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("heroes");

            // Header row
            IRow header = sheet.CreateRow(0);
            string[] columns = {
                "id", "name", "heroType", "attackRange", "attackCooldown",
                "damage", "critRate", "critDamage", "projectileAsset", "trajectoryType",
                "hitCount", "hitInterval", "projectileSpeed", "projectileSize",
                "spreadCount", "spreadAngleStep", "spreadDamageScale",
                "parallelCount", "parallelDamageScale",
                "explosiveRadius", "explosiveDamageScale"
            };

            for (int i = 0; i < columns.Length; i++)
            {
                header.CreateCell(i).SetCellValue(columns[i]);
            }

            // Hero data: id, name, heroType, attackRange, attackCooldown,
            //            damage, critRate, critDamage, projectileAsset, trajectoryType,
            //            hitCount, hitInterval, projectileSpeed, projectileSize,
            //            spreadCount, spreadAngleStep, spreadDamageScale,
            //            parallelCount, parallelDamageScale,
            //            explosiveRadius, explosiveDamageScale

            object[][] heroes = new object[][]
            {
                // Archer 1 - Single target, fast cooldown
                new object[] { 1001, "Archer 1", 0, 5.0, 1.0, 10, 0.1, 1.5, "Triangle", 0, 1, 0.1, 8.0, 1.0, 0, 0.0, 1.0, 0, 1.0, 0.0, 0.0 },
                // Archer 2 - Spread arrows
                new object[] { 1002, "Archer 2", 1, 4.5, 1.2, 8, 0.15, 1.8, "Triangle", 0, 1, 0.1, 7.0, 1.0, 3, 15.0, 0.7, 0, 1.0, 0.0, 0.0 },
                // Magic 1 - AoE explosion
                new object[] { 1003, "Magic 1", 2, 4.0, 1.5, 15, 0.05, 2.0, "Triangle", 0, 1, 0.1, 5.0, 1.5, 0, 0.0, 1.0, 0, 1.0, 2.0, 0.5 },
                // Magic 2 - Multi-hit parallel
                new object[] { 1004, "Magic 2", 3, 4.0, 1.8, 20, 0.05, 2.0, "Triangle", 0, 3, 0.3, 4.0, 1.2, 0, 0.0, 1.0, 2, 0.7, 0.0, 0.0 },
                // Buff Control - Stationary AoE
                new object[] { 1005, "Buff Control", 4, 3.0, 2.0, 5, 0.0, 1.0, "Triangle", 3, 1, 0.1, 0.0, 2.0, 0, 0.0, 1.0, 0, 1.0, 1.5, 0.3 },
            };

            for (int r = 0; r < heroes.Length; r++)
            {
                IRow row = sheet.CreateRow(r + 1);
                for (int c = 0; c < heroes[r].Length; c++)
                {
                    ICell cell = row.CreateCell(c);
                    object val = heroes[r][c];

                    if (val is int intVal)
                        cell.SetCellValue(intVal);
                    else if (val is double doubleVal)
                        cell.SetCellValue(doubleVal);
                    else if (val is float floatVal)
                        cell.SetCellValue((double)floatVal);
                    else if (val is string strVal)
                        cell.SetCellValue(strVal);
                }
            }

            // Auto-size columns
            for (int i = 0; i < columns.Length; i++)
            {
                sheet.AutoSizeColumn(i);
            }

            // Write file
            using (FileStream fs = File.Create(path))
            {
                workbook.Write(fs);
            }

            AssetDatabase.Refresh();
            Debug.Log($"[HeroConfig] Generated Excel at: {path}");
        }
    }
}
