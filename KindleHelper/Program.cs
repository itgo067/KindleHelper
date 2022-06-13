using System.CommandLine;
using KindleHelper;
using KindleHelper.Models;
using KindleHelper.Utils;


namespace KindleHelper
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var root = new RootCommand();

            var outDirOption = new Option<DirectoryInfo?>(aliases: new string[] { "--output", "-o" }, description: "目录", getDefaultValue: () => { return new DirectoryInfo("./"); }) { Arity = ArgumentArity.ZeroOrOne, IsRequired = false };
            var domainOption = new Option<string?>(aliases: new string[] { "--domain" }, description: "国家地区代码 , cn, jp, com", getDefaultValue: () => { return "cn"; });
            var cookieOption = new Option<string?>(aliases: new string[] { "--cookie" }, description: "amazon cookie");
            var csrfTokenOption = new Option<string?>(aliases: new string[] { "--csrf-token" }, description: "amazon csrf token");
            var resumeOption = new Option<int>(aliases: new string[] { "--resume-from" }, description: " resume from the index if download filed");
            var cutLengthOption = new Option<int>(aliases: new string[] { "--cut-length" }, description: "truncate the file name", getDefaultValue: () => 100);
            var fileTypeOption = new Option<string?>(aliases: new string[] { "--filetype" }, description: "amazon file type , EBOK PDOC", getDefaultValue: () => { return "EBOK"; });

            root.Add(outDirOption);
            root.Add(domainOption);
            root.Add(cookieOption);
            root.Add(csrfTokenOption);
            root.Add(resumeOption);
            root.Add(cutLengthOption);
            root.Add(fileTypeOption);

            root.SetHandler(async (dir, domain, cookie, csrfToken, index, cutLength, fileType) =>
            {
                if (Directory.Exists(dir.FullName))
                {
                    Directory.CreateDirectory(dir.FullName);
                }

                if (!FileUtil.CheckCookieFile())
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(cookie))
                {
                    cookie = FileUtil.ReadCookieFromFile();
                    if (string.IsNullOrWhiteSpace(cookie))
                    {
                        Console.WriteLine("cookie 为空");
                        return;
                    }
                }

                KindleConst.COOKIE = cookie;

                var kindleDownload = new KindleDownload(csrfToken, domain, dir.FullName, cutLength);

                await kindleDownload.DownloadBooks(index, fileType);

            }, outDirOption, domainOption, cookieOption, csrfTokenOption, resumeOption, cutLengthOption, fileTypeOption

            );

            await root.InvokeAsync(args);


            
        }

        public void stringd () {
            
            }
    }
}



