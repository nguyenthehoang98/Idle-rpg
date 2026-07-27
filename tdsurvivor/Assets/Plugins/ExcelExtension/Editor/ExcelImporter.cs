using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using ExcelExtension;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

public class ExcelImporter : AssetPostprocessor
{
	public class ExcelAssetInfo
	{
		public Type AssetType { get; set; }
		public ExcelAssetAttribute Attribute { get; set; } 
	}

	static List<ExcelAssetInfo> cachedInfos = null; // Clear on compile.

	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		bool imported = false;
		
		foreach(string path in importedAssets)
		{
			if(Path.GetExtension(path) == ".xls" || Path.GetExtension(path) == ".xlsx") 
			{
				if(cachedInfos == null) cachedInfos = FindExcelAssetInfos();

				var excelName = Path.GetFileNameWithoutExtension(path);
				if(excelName.StartsWith("~$")) continue;
				excelName = RemoveNumberInString(excelName);

				ExcelAssetInfo info = cachedInfos.Find(i => i.AssetType.Name == excelName);

				if(info == null) continue;

				if(info.Attribute.IsAutoConvertExcel) ImportExcel(info);
				imported = true;
			}
		}

		if(imported) 
		{
			foreach (var cachedInfo in cachedInfos)
			{
				string fullPath = ConvertCShapePath(cachedInfo.Attribute.ConfigPath);
				
				if (File.Exists(fullPath))
				{
					string json = File.ReadAllText(fullPath);
					object asset = JsonUtility.FromJson(json, cachedInfo.AssetType);
					
					try
					{
						asset.GetType().GetMethod("OnCompleteImported").Invoke(asset, new object[0]);
					}
					catch (Exception e)
					{
						Debug.LogError(e);
					}
					
					json = JsonUtility.ToJson(asset);
					
					File.WriteAllText(ConvertCShapePath(fullPath), json);
				}
			}
			
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}

	public static string RemoveNumberInString(string str)
	{
		for (int i = 0; i < 10; i++)
		{
			str = str.Replace(i.ToString(), "");
		}

		return str;
	}

	static List<ExcelAssetInfo> FindExcelAssetInfos()
	{
		var list = new List<ExcelAssetInfo>();
		
		foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (assembly.FullName.StartsWith("Google")) continue;

			foreach (var type in assembly.GetTypes())
			{
				object[] attributes = type.GetCustomAttributes(typeof(ExcelAssetAttribute), false);

				if (attributes.Length == 0) continue;

				ExcelAssetAttribute attribute = (ExcelAssetAttribute)attributes[0];

				ExcelAssetInfo info = new ExcelAssetInfo()
				{
					AssetType = type,
					Attribute = attribute
				};

				list.Add(info);
			}
		}
		return list;
	}

	static UnityEngine.Object LoadOrCreateAsset(string assetPath, Type assetType)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(assetPath));

		var asset = AssetDatabase.LoadAssetAtPath(assetPath, assetType);
		if (asset == null)
		{
			asset = ScriptableObject.CreateInstance(assetType.Name);
			AssetDatabase.CreateAsset((ScriptableObject)asset, assetPath);
			asset.hideFlags = HideFlags.NotEditable;
		}

		return asset;
	}

	public static IWorkbook LoadBook(string excelPath)
	{
		using(FileStream stream = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			if (Path.GetExtension(excelPath) == ".xls") return new HSSFWorkbook(stream);
			else return new XSSFWorkbook(stream);
		}
	}

	static List<string> GetFieldNamesFromSheetHeader(ISheet sheet)
	{
		IRow headerRow = sheet.GetRow(0);

		var fieldNames = new List<string>();
		for (int i = 0; i < headerRow.LastCellNum; i++)
		{
			var cell = headerRow.GetCell(i);
			if(cell == null || cell.CellType == CellType.Blank) break;
			fieldNames.Add(cell.StringCellValue);
		}
		return fieldNames;
	}

	static object CellToFieldObject(ICell cell, FieldInfo fieldInfo)
	{
		switch (cell.CellType)
		{
			case CellType.Formula:
			case CellType.String:
				if (fieldInfo.FieldType.IsEnum)
				{
					return Enum.Parse(fieldInfo.FieldType, cell.StringCellValue);
				}

				if (fieldInfo.FieldType == typeof(int))
				{
					if (cell.CellType == CellType.Formula) 
						return (int) cell.NumericCellValue;
					return int.Parse(cell.StringCellValue);
				}

				if (fieldInfo.FieldType == typeof(float))
				{
					if (cell.CellType == CellType.Formula)
						return (float) cell.NumericCellValue;
					return float.Parse(cell.StringCellValue);
				}

				if (fieldInfo.FieldType == typeof(double))
				{
					if (cell.CellType == CellType.Formula) 
						return (double) cell.NumericCellValue;
					return double.Parse(cell.StringCellValue);
				}

				if (fieldInfo.FieldType == typeof(string))
				{
					return cell.StringCellValue;
				}

				if (fieldInfo.FieldType.IsArray || 
				    (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
				{
					string raw = cell.StringCellValue;

					string[] elements = raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
						.Select(e => e.Trim())
						.Where(e => !string.IsNullOrEmpty(e))
						.ToArray();

					Type elementType = fieldInfo.FieldType.IsArray
						? fieldInfo.FieldType.GetElementType()
						: fieldInfo.FieldType.GetGenericArguments()[0];

					// ===== Primitive =====
					if (elementType == typeof(string))
					{
						return CreateCollection(fieldInfo.FieldType, elements);
					}

					if (elementType == typeof(int))
					{
						int[] values = elements
							.Select(s => int.TryParse(s, out var n) ? (int?)n : null)
							.Where(n => n.HasValue)
							.Select(n => n.Value)
							.ToArray();
						return CreateCollection(fieldInfo.FieldType, values);
					}

					if (elementType == typeof(float))
					{
						float[] values = elements
							.Select(s => float.TryParse(s, out var n) ? (float?)n : null)
							.Where(n => n.HasValue)
							.Select(n => n.Value)
							.ToArray();
						return CreateCollection(fieldInfo.FieldType, values);
					}

					// ===== Object / Struct =====
					Array array = Array.CreateInstance(elementType, elements.Length);

					for (int i = 0; i < elements.Length; i++)
					{
						array.SetValue(JsonUtility.FromJson(elements[i], elementType), i);
					}

					return CreateCollection(fieldInfo.FieldType, array);
				}

				if (cell.CellType == CellType.Formula)
				{
					try
					{
						return JsonUtility.FromJson(cell.CellFormula, fieldInfo.FieldType);
					}
					catch (ArgumentException e)
					{
						Debug.LogError("parse formula: " + cell.CellFormula + ", field_type: " + fieldInfo.FieldType);
						throw e;
					}
				}
				
				return JsonUtility.FromJson(cell.StringCellValue, fieldInfo.FieldType);
				
			case CellType.Boolean:
				return cell.BooleanCellValue;
			case CellType.Numeric:
				return Convert.ChangeType(cell.NumericCellValue, fieldInfo.FieldType);
			default:
				if (fieldInfo.FieldType.IsValueType)
				{
					return Activator.CreateInstance(fieldInfo.FieldType);
				}

				return null;
		}
	}
	
	static object CreateCollection(Type collectionType, Array sourceArray)
	{
		if (collectionType.IsArray)
			return sourceArray;

		if (collectionType.IsGenericType &&
		    collectionType.GetGenericTypeDefinition() == typeof(List<>))
		{
			var list = (IList)Activator.CreateInstance(collectionType);
			foreach (var item in sourceArray)
				list.Add(item);
			return list;
		}

		throw new NotSupportedException($"Unsupported collection type: {collectionType}");
	}

	static object CreateEntityFromRow(IRow row, List<string> columnNames, Type entityType, string sheetName)
	{
		var entity = Activator.CreateInstance(entityType);

		for (int i = 0; i < columnNames.Count; i++)
		{
			FieldInfo entityField = entityType.GetField(
				columnNames[i],
				BindingFlags.Instance | BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic 
			);
			if (entityField == null) continue;

			ICell cell = row.GetCell(i);
			if (cell == null) continue;
			
			object fieldValue = CellToFieldObject(cell, entityField);

			try
			{
				entityField.SetValue(entity, fieldValue);
			}
			catch (Exception e)
			{
				Debug.LogError(e.StackTrace);
				Debug.LogError($"Raw data: {cell}, cell-type: {cell.CellType}, field-type: {entityField.FieldType}, field-name: {entityField.Name}");
				throw new Exception(string.Format("Invalid excel cell type at row {0}, column {1}, {2}, sheet", row.RowNum, cell.ColumnIndex, sheetName));
			}
		}
		return entity;
	}
	
	static object GetEntityListFromSheet(ISheet sheet, Type entityType)
	{
		List<string> excelColumnNames = GetFieldNamesFromSheetHeader(sheet);

		Type listType = typeof(List<>).MakeGenericType(entityType);
		MethodInfo listAddMethod = listType.GetMethod("Add", new Type[]{entityType});
		object list = Activator.CreateInstance(listType);

		// row of index 0 is header
		for (int i = 1; i <= sheet.LastRowNum; i++)
		{
			IRow row = sheet.GetRow(i);
			if(row == null) break;

			ICell entryCell = row.GetCell(0); 
			if(entryCell == null || entryCell.CellType == CellType.Blank) break;

			// skip comment row
			if(entryCell.CellType == CellType.String && entryCell.StringCellValue.StartsWith("#")) continue;

			var entity = CreateEntityFromRow(row, excelColumnNames, entityType, sheet.SheetName);
			listAddMethod.Invoke(list, new object[] { entity });
		}
		return list;
	}

	private static void ImportExcel(ExcelAssetInfo info)
	{
		string path = info.Attribute.ConfigPath;
		Type type = info.AssetType;
		object asset =  Activator.CreateInstance(type);
		IWorkbook book = LoadBook(info.Attribute.ExcelPath);

		var assetFields = info.AssetType.GetFields(
			BindingFlags.Instance | BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic
		);
		int sheetCount = 0;
		foreach (var assetField in assetFields)
		{
			ISheet sheet = book.GetSheet(assetField.Name);
			if(sheet == null) continue;
			
			Type fieldType = assetField.FieldType;
			if(! fieldType.IsGenericType || (fieldType.GetGenericTypeDefinition() != typeof(List<>))) continue;

			Type[] types = fieldType.GetGenericArguments();
			Type entityType = types[0];
			
			object entities = GetEntityListFromSheet(sheet, entityType);
			assetField.SetValue(asset, entities);
			sheetCount++;
		}

		try
		{
			asset.GetType().GetMethod("OnImported").Invoke(asset, new object[0]);
		}
		catch (Exception e)
		{
			Debug.LogError(e);
		}

		string json = JsonUtility.ToJson(asset);

		File.WriteAllText(ConvertCShapePath(path), json);
	}

	static string ConvertCShapePath(string filePath)
	{
		string dataPath = Application.dataPath;
		return Path.Combine(dataPath.Substring(0, dataPath.Length - 7), filePath); 
	}
}
