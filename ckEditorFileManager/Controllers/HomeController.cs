using ckEditorFileManager.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ckEditorFileManager.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 首頁
        /// </summary>
        /// <returns>Index Page</returns>
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        /// <summary>
        /// CKEditor範例頁
        /// </summary>
        /// <returns>CKEditorSample Page</returns>
        public ActionResult CKEditorSample()
        {
            ViewBag.Title = "CKEditor範例";

            return View();
        }

        /// <summary>
        /// CKEditor 圖片檔案管理頁
        /// </summary>
        /// <param name="CKEditorFuncNum">CKEditor必要參數</param>
        /// <returns>CKEditorImageFileManager Page</returns>
        public ActionResult CKEditorImageFileManager(string CKEditorFuncNum)
        {
            CKEditorFuncNum = (CKEditorFuncNum == null) ? string.Empty : CKEditorFuncNum.Trim();
            ViewData["CKEditorFuncNum"] = CKEditorFuncNum;

            DirectoryInfo dirInfo = new DirectoryInfo(Server.MapPath("~/files/"));
            var searchFiles = dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories)
                .Where(o => o.Extension.ToLower().Equals(".jpg") || o.Extension.ToLower().Equals(".gif") || o.Extension.ToLower().Equals(".png"));

            List<string> fileDatas = new List<string>();
            fileDatas = searchFiles.OrderByDescending(o => o.LastWriteTime).Select(o =>
            {
                var fileName = Path.GetFileName(o.FullName);
                string fullFileName = string.Format("~/files/{0}", fileName);
                return fullFileName;
            }).ToList();

            return View(fileDatas);
        }

        /// <summary>
        /// CKEditor 影片檔案管理頁
        /// </summary>
        /// <param name="CKEditorFuncNum">CKEditor必要參數</param>
        /// <param name="videoType">影片種類(MP4,WebM)</param>
        /// <returns>CKEditorVideoFileManager Page</returns>
        public ActionResult CKEditorVideoFileManager(string CKEditorFuncNum, string videoType)
        {
            CKEditorFuncNum = (CKEditorFuncNum == null) ? string.Empty : CKEditorFuncNum.Trim();
            videoType = (videoType == null) ? string.Empty : videoType.Trim();
            VideoTypeEnum videoTypeValue;
            List<string> fileDatas = new List<string>();
            if (Enum.TryParse<VideoTypeEnum>(videoType, out videoTypeValue))
            {
                ViewData["CKEditorFuncNum"] = CKEditorFuncNum;
                ViewData["videoType"] = videoType;

                DirectoryInfo dirInfo = new DirectoryInfo(Server.MapPath("~/files/"));
                var searchFiles = dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories);
                switch (videoTypeValue)
                {
                    case VideoTypeEnum.Mp4:
                        searchFiles = searchFiles
                            .Where(o => o.Extension.ToLower().Equals(".mp4") || o.Extension.ToLower().Equals(".mov"));
                        break;
                    case VideoTypeEnum.WebM:
                        searchFiles = searchFiles
                            .Where(o => o.Extension.ToLower().Equals(".webm"));
                        break;
                }

                fileDatas = searchFiles.OrderByDescending(o => o.LastWriteTime).Select(o =>
                {
                    var fileName = Path.GetFileName(o.FullName);
                    string fullFileName = string.Format("~/files/{0}", fileName);
                    return fullFileName;
                }).ToList();
            }

            return View(fileDatas);
        }

        /// <summary>
        /// 檔案上傳
        /// </summary>
        /// <param name="file">檔案(可多個)</param>
        /// <returns>上傳結果(json資料格式)-配合[bootstrap-fileinput套件]</returns>
        public ActionResult CKEditorFileUpload(HttpPostedFileBase[] file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { error = "No file selected.", errorkeys = new string[0] });
            }

            string errorMessage = "You have faced errors in {0} files.";
            string root = Server.MapPath("~/files/");
            List<string> errorKeys = new List<string>();
            List<Tuple<string, string>> files = new List<Tuple<string, string>>();
            if (file != null && file.Count() > 0)
            {
                for (int i = 0; i < file.Count(); i++)
                {
                    var fileItem = file[i];
                    string fileExtension = Path.GetExtension(fileItem.FileName);
                    string fileName = Path.GetFileNameWithoutExtension(fileItem.FileName);

                    var checkFiles = Directory.GetFiles(root, fileItem.FileName);
                    if (checkFiles != null && checkFiles.Count() > 0)
                    {
                        Random random = new Random(DateTime.Now.Second);
                        var sno = random.Next(0, 10000);
                        fileName = string.Format("{0}_{1}_{2}", fileName, DateTime.Now.ToString("yyyyMMddHHmmss"), sno);
                    }

                    try
                    {
                        fileItem.SaveAs(Path.Combine(root, fileName + fileExtension));
                        files.Add(new Tuple<string, string>(Url.Content("~/files/"), fileName + fileExtension));
                    }
                    catch (Exception)
                    {
                        errorKeys.Add(i.ToString());
                        continue;
                    }
                }
            }

            //錯誤回傳格式 => {error: 'You have faced errors in 4 files.', errorkeys: [0, 3, 4, 5]}
            if (errorKeys.Count > 0)
            {
                return Json(new { error = string.Format(errorMessage, errorKeys.Count), errorkeys = errorKeys.ToArray() });
            }
            else
            {
                return Json(new { files = files.Select(o => new { @FilePath = o.Item1, @FileName = o.Item2 }).ToArray() });
            }
        }

        /// <summary>
        /// 使用檔名刪除檔案
        /// </summary>
        /// <param name="fileName">檔案名稱</param>
        /// <returns>刪除結果</returns>
        public ActionResult CKEditorFileDelete(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return Json(new { IsSuccess = false, Message = "File name must input." });

            string root = Server.MapPath("~/files/");
            DirectoryInfo dirInfo = new DirectoryInfo(root);
            var checkFiles = dirInfo.GetFiles(fileName);
            if (checkFiles != null && checkFiles.Count() > 0)
            {
                foreach (var item in checkFiles)
                {
                    try
                    {
                        var delFileName = Path.GetFileName(item.FullName);
                        if (delFileName.Equals(fileName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            item.Delete();
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                }
                return Json(new { IsSuccess = true, Message = string.Empty });
            }
            else
            {
                return Json(new { IsSuccess = false, Message = "Can't find file by file name." });
            }
        }
    }
}
