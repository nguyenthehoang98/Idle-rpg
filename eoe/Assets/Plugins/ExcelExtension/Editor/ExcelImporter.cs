using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using ExcelExtension;
using Newtonsoft.Json;
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
	static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
	{
		Converters = { new Vector3JsonConverter() }
	};

	private sealed class Vector3JsonConverter : JsonConverter
	{
		private struct Vector3Data
		{
			public float x;
			public float y;
			public float z;
		}

		public override bool CanConvert(Type objectType) => objectType == typeof(Vector3);

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			Vector3Data data = serializer.Deserialize<Vector3Data>(reader);
			return new Vector3(data.x, data.y, data.z);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			Vector3 vector = (Vector3)value;
			writer.WriteStartObject();
			writer.WritePropertyName("x");
			writer.WriteValue(vector.x);
			writer.WritePropertyName("y");
			writer.WriteValue(vector.y);
			writer.WritePropertyName("z");
			writer.WriteValue(vector.z);
			writer.WriteEndObject();
		}
	}

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
					object asset = JsonConvert.DeserializeObject(json, cachedInfo.AssetType, JsonSettings);
					
					try
					{
						asset.GetType().GetMethod("OnCompleteImported").Invoke(asset, new object[0]);
					}
					catch (Exception e)
					{
						Debug.LogError(e);
					}
					
					json = JsonConvert.SerializeObject(asset, Formatting.Indented, JsonSettings);
					
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

	static int GetHeaderRowCount(ISheet sheet)
	{
		for (int i = 0; i < sheet.NumMergedRegions; i++)
		{
			var region = sheet.GetMergedRegion(i);
			if (region.FirstRow == 0 && region.LastRow == 1) return 2;
		}

		return 1;
	}

	static string GetHeaderCellText(ISheet sheet, int rowIndex, int columnIndex)
	{
		IRow row = sheet.GetRow(rowIndex);
		ICell cell = row?.GetCell(columnIndex);
		if (cell != null && cell.CellType == CellType.String && !string.IsNullOrWhiteSpace(cell.StringCellValue))
			return cell.StringCellValue.Trim();

		// A horizontally merged group header only stores its value in the first cell.
		if (rowIndex == 0)
		{
			for (int i = 0; i < sheet.NumMergedRegions; i++)
			{
				var region = sheet.GetMergedRegion(i);
				if (region.FirstRow != 0 || region.LastRow != 0 ||
				    columnIndex < region.FirstColumn || columnIndex > region.LastColumn)
					continue;

				return sheet.GetRow(region.FirstRow)?.GetCell(region.FirstColumn)?.StringCellValue?.Trim();
			}
		}

		return null;
	}

	static List<string> GetFieldNamesFromSheetHeader(ISheet sheet)
	{
		int headerRowCount = GetHeaderRowCount(sheet);
		int lastCellNum = sheet.GetRow(0)?.LastCellNum ?? 0;
		if (headerRowCount == 2)
			lastCellNum = Math.Max(lastCellNum, sheet.GetRow(1)?.LastCellNum ?? 0);

		var fieldNames = new List<string>(lastCellNum);
		for (int i = 0; i < lastCellNum; i++)
		{
			string parentName = GetHeaderCellText(sheet, 0, i);
			string childName = headerRowCount == 2 ? GetHeaderCellText(sheet, 1, i) : null;
			string fieldName = string.IsNullOrEmpty(childName)
				? parentName
				: string.IsNullOrEmpty(parentName) ? childName : $"{parentName}.{childName}";

			fieldNames.Add(fieldName);
		}

		return fieldNames;
	}

	static object CellToFieldObject(ICell cell, FieldInfo fieldInfo)
	{
		switch (cell.CellType)
		{
			case CellType.Formula:
			case CellType.String:
				if (fieldInfo.FieldType == typeof(bool))
				{
					string value = cell.CellType == CellType.Formula
						? cell.CellFormula.Trim().TrimStart('=').Trim()
						: cell.StringCellValue.Trim();
					if (value.EndsWith("()")) value = value.Substring(0, value.Length - 2);
					return bool.Parse(value);
				}

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
					return float.Parse(cell.StringCellValue, NumberStyles.Float);
				}

				if (fieldInfo.FieldType == typeof(double))
				{
					if (cell.CellType == CellType.Formula) 
						return (double) cell.NumericCellValue;
					return double.Parse(cell.StringCellValue, NumberStyles.Number);
				}

				if (fieldInfo.FieldType == typeof(string))
				{
					return cell.StringCellValue;
				}

				if (fieldInfo.FieldType.IsArray || 
				    (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
				{
					string raw = cell.StringCellValue;
					Type elementType = fieldInfo.FieldType.IsArray
						? fieldInfo.FieldType.GetElementType()
						: fieldInfo.FieldType.GetGenericArguments()[0];

					if (!elementType.IsPrimitive && raw.TrimStart().StartsWith("["))
						return JsonConvert.DeserializeObject(raw, fieldInfo.FieldType, JsonSettings);

					string[] elements = raw
						.Trim('[', ']')
						.Split(',', StringSplitOptions.RemoveEmptyEntries)
						.Select(e => e.Trim())
						.ToArray();

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
						array.SetValue(JsonConvert.DeserializeObject(elements[i], elementType, JsonSettings), i);
					}

					return CreateCollection(fieldInfo.FieldType, array);
				}

				if (cell.CellType == CellType.Formula)
				{
					try
					{
						return JsonConvert.DeserializeObject(cell.CellFormula, fieldInfo.FieldType, JsonSettings);
					}
					catch (ArgumentException e)
					{
						Debug.LogError("parse formula: " + cell.CellFormula + ", field_type: " + fieldInfo.FieldType);
						throw e;
					}
				}
				
				return JsonConvert.DeserializeObject(cell.StringCellValue, fieldInfo.FieldType, JsonSettings);
				
			case CellType.Boolean:
				return cell.BooleanCellValue;
			case CellType.Numeric:
				if (fieldInfo.FieldType.IsEnum)
					return Enum.ToObject(fieldInfo.FieldType, Convert.ToInt32(cell.NumericCellValue));

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

	static bool HasCellValue(ICell cell)
	{
		return cell != null && cell.CellType != CellType.Blank &&
		       (cell.CellType != CellType.String || !string.IsNullOrWhiteSpace(cell.StringCellValue));
	}

	static bool HasNestedColumns(List<string> columnNames)
	{
		return columnNames.Any(name => !string.IsNullOrEmpty(name) && name.Contains('.'));
	}

	static bool RowHasNestedValues(IRow row, List<string> columnNames)
	{
		for (int i = 0; i < columnNames.Count; i++)
		{
			if (columnNames[i] != null && columnNames[i].Contains('.') && HasCellValue(row.GetCell(i)))
				return true;
		}

		return false;
	}

	static Type GetCollectionElementType(Type fieldType)
	{
		if (fieldType.IsArray) return fieldType.GetElementType();
		if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
			return fieldType.GetGenericArguments()[0];
		return null;
	}

	static void AppendNestedFields(object entity, IRow row, List<string> columnNames, Type entityType, string sheetName)
	{
		foreach (var group in columnNames
			.Select((name, index) => new { name, index })
			.Where(item => item.name != null && item.name.Contains('.'))
			.GroupBy(item => item.name.Substring(0, item.name.IndexOf('.'))))
		{
			FieldInfo collectionField = entityType.GetField(
				group.Key,
				BindingFlags.Instance | BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic);
			if (collectionField == null) continue;

			Type collectionType = collectionField.FieldType;
			Type nestedType = GetCollectionElementType(collectionType) ?? collectionType;
			object nestedValue = Activator.CreateInstance(nestedType);
			bool hasValue = false;

			foreach (var item in group)
			{
				ICell cell = row.GetCell(item.index);
				if (!HasCellValue(cell)) continue;

				string nestedFieldName = item.name.Substring(item.name.IndexOf('.') + 1);
				FieldInfo nestedField = nestedType.GetField(
					nestedFieldName,
					BindingFlags.Instance | BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic);
				if (nestedField == null) continue;

				try
				{
					nestedField.SetValue(nestedValue, CellToFieldObject(cell, nestedField));
					hasValue = true;
				}
				catch (Exception e)
				{
					Debug.LogError(e.StackTrace);
					throw new Exception($"Invalid nested field '{item.name}' at row {row.RowNum}, column {cell.ColumnIndex}, {sheetName}");
				}
			}

			if (collectionType.IsArray)
			{
				Array existing = collectionField.GetValue(entity) as Array;
				if (existing == null)
					collectionField.SetValue(entity, Array.CreateInstance(nestedType, 0));

				if (!hasValue) continue;

				existing = collectionField.GetValue(entity) as Array;
				Array values = Array.CreateInstance(nestedType, existing.Length + 1);
				Array.Copy(existing, values, existing.Length);
				values.SetValue(nestedValue, existing.Length);
				collectionField.SetValue(entity, values);
			}
			else if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(List<>))
			{
				IList values = collectionField.GetValue(entity) as IList;
				if (values == null)
				{
					values = (IList)Activator.CreateInstance(collectionType);
					collectionField.SetValue(entity, values);
				}

				if (hasValue) values.Add(nestedValue);
			}
			else if (hasValue)
			{
				collectionField.SetValue(entity, nestedValue);
			}
		}
	}

	static object CreateEntityFromRow(IRow row, List<string> columnNames, Type entityType, string sheetName)
	{
		var entity = Activator.CreateInstance(entityType);

		for (int i = 0; i < columnNames.Count; i++)
		{
			if (string.IsNullOrEmpty(columnNames[i]) || columnNames[i].Contains('.')) continue;

			FieldInfo entityField = entityType.GetField(
				columnNames[i],
				BindingFlags.Instance | BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic
			);
			if (entityField == null) continue;

			ICell cell = row.GetCell(i);
			if (!HasCellValue(cell)) continue;

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
		int headerRowCount = GetHeaderRowCount(sheet);
		bool hasNestedColumns = HasNestedColumns(excelColumnNames);

		Type listType = typeof(List<>).MakeGenericType(entityType);
		MethodInfo listAddMethod = listType.GetMethod("Add", new Type[] { entityType });
		IList list = (IList)Activator.CreateInstance(listType);
		object currentEntity = null;
		int currentEntityIndex = -1;

		for (int i = headerRowCount; i <= sheet.LastRowNum; i++)
		{
			IRow row = sheet.GetRow(i);
			if (row == null) continue;

			ICell entryCell = row.GetCell(0);
			bool hasEntry = HasCellValue(entryCell);
			if (!hasEntry)
			{
				if (!hasNestedColumns || currentEntity == null || !RowHasNestedValues(row, excelColumnNames)) break;

				AppendNestedFields(currentEntity, row, excelColumnNames, entityType, sheet.SheetName);
				list[currentEntityIndex] = currentEntity;
				continue;
			}

			// skip comment row
			if (entryCell.CellType == CellType.String && entryCell.StringCellValue.StartsWith("#")) continue;

			currentEntity = CreateEntityFromRow(row, excelColumnNames, entityType, sheet.SheetName);
			if (hasNestedColumns)
				AppendNestedFields(currentEntity, row, excelColumnNames, entityType, sheet.SheetName);

			listAddMethod.Invoke(list, new[] { currentEntity });
			currentEntityIndex = list.Count - 1;
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
		}

		try
		{
			asset.GetType().GetMethod("OnImported").Invoke(asset, new object[0]);
		}
		catch (Exception e)
		{
			Debug.LogError(e);
		}

		string json = JsonConvert.SerializeObject(asset, Formatting.Indented, JsonSettings);

		File.WriteAllText(ConvertCShapePath(path), json);
	}

	static string ConvertCShapePath(string filePath)
	{
		string dataPath = Application.dataPath;
		return Path.Combine(dataPath.Substring(0, dataPath.Length - 7), filePath); 
	}
}
