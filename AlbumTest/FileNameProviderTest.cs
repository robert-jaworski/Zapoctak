using AlbumLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using FileInfo = AlbumLibrary.FileInfo;

namespace AlbumTest {
	[TestClass]
	public class FileNameProviderTest {
		[TestMethod]
		[DataRow(1, "photos/{year}/{month}/{day}/{file:name}", "photos/2022/2/1/test.jpg", @"C:\testDir\test.jpg", null,
			"2022-02-01 13:37:21", "2022-02-01 15:23:56", null, null, null)]
		[DataRow(2, "photos/{h}/{h:exif}/{h:create}/{h:modify}/{file:name}", "photos/20/19/13/15/test.jpg", @"C:\testDir\test.jpg", "2022-01-31 19:44:07",
			"2022-02-01 13:37:21", "2022-02-01 15:23:56", null, null, "2022-01-31 20:44:07")]
		[DataRow(3, "photos/{YYYY}/{M#3}/{YY}{MM}{DD}-{hh}{mm}{ss}", "photos/2022/001/220131-204407.jpg", @"C:\testDir\test.jpg", "2022-01-31 19:44:07",
			"2022-02-01 13:37:21", "2022-02-01 15:23:56", null, null, "2022-01-31 20:44:07")]
		[DataRow(4, "photos/{device:name}/{device:manufacturer}-{device:model}-{file:name}", "photos/Test Camera/Test-Camera-test.jpg",
			@"C:\testDir\test.jpg", "2022-01-31 19:44:07", "2022-02-01 13:37:21", "2022-02-01 15:23:56", "Test", "Camera", "2022-01-31 20:44:07")]
		[DataRow(5, "photos/type-{file:ext#4}/{file:name}{noext}", "photos/type- jpg/test", @"C:\testDir\test.jpg", "2022-01-31 19:44:07", "2022-02-01 13:37:21",
			"2022-02-01 15:23:56", "Test", "Camera", "2022-01-31 20:44:07")]
		[DataRow(6, "photos/JPG/{file:name}{ext:.jpg}", "photos/JPG/test.jpg", @"C:\testDir\test.jpg", "2022-01-31 19:44:07", "2022-02-01 13:37:21",
			"2022-02-01 15:23:56", "Test", "Camera", "2022-01-31 20:44:07")]
		public void TemplateFileNameProvider_GetFileName(int id, string template, string expectedName, string path, string? exifDateTime,
			string fileCreation, string fileModification, string? manufacturer, string? model, string? overwriteDateTime) {
			Logger.LogMessage($"Testing {id}: {{0}} -> {expectedName}", template);

			var info = new FileInfo(path, exifDateTime is null ? null : DateTime.Parse(exifDateTime), DateTime.Parse(fileCreation),
				DateTime.Parse(fileModification), manufacturer, model) {
				OverwriteDateTime = overwriteDateTime is null ? null : DateTime.Parse(overwriteDateTime)
			};

			var fileNameProvider = new TemplateFileNameProvider(template);
			Assert.AreEqual(expectedName, fileNameProvider.GetFileName(info));
		}

		[TestMethod]
		[DataRow(1, "photos/JPG/{YYYY}/{file:name}{.:.jpg},photos/PNG/{file:name}{.:.png}", "photos/JPG/2022/test.jpg", @"C:\testDir\test.jpg",
			"2022-01-31 19:44:07", "2022-02-01 13:37:21", "2022-02-01 15:23:56", "Test", "Camera", "2022-01-31 20:44:07")]
		[DataRow(2, "photos/JPG/{file:name}{.:.jpg},photos/PNG/{file:name}{.:.png}", "photos/PNG/test.png", @"C:\testDir\test.png",
			"2022-01-31 19:44:07", "2022-02-01 13:37:21", "2022-02-01 15:23:56", "Microsoft", "Paint", "2022-01-31 20:44:07")]
		public void MultipleFileNameProvider_GetFileName(int id, string template, string expectedName, string path, string? exifDateTime,
			string fileCreation, string fileModification, string? manufacturer, string? model, string? overwriteDateTime) {
			Logger.LogMessage($"Testing {id}: {{0}} -> {expectedName}", template);

			var info = new FileInfo(path, exifDateTime is null ? null : DateTime.Parse(exifDateTime), DateTime.Parse(fileCreation),
				DateTime.Parse(fileModification), manufacturer, model) {
				OverwriteDateTime = overwriteDateTime is null ? null : DateTime.Parse(overwriteDateTime)
			};

			var fileNameProvider = new MultipleFileNameProvider(template.Split(',').Select(x => new TemplateFileNameProvider(x)));
			Assert.AreEqual(expectedName, fileNameProvider.GetFileName(info));
		}
	}
}
