using System;

namespace ExcelExtension
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class ExcelAssetAttribute : System.Attribute
	{
		public string ExcelPath { get; set; }
		public string ConfigPath { get; set; }
		public bool IsAutoConvertExcel { get; set; } = true;
	}
}