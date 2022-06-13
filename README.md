
# KindleHelper

> 下载亚马逊上你的 **电子书** 以及 **个人文档**

# 使用说明


## 使用

```bash
Usage:
  KindleHelper [options]

Options:
  -o, --output <output>        目录 [default: ./]
  --domain <domain>            国家地区代码 , cn, jp, com [default: cn]
  --cookie <cookie>            amazon cookie
  --csrf-token <csrf-token>    amazon csrf token
  --resume-from <resume-from>  resume from the index if download filed
  --cut-length <cut-length>    truncate the file name [default: 100]
  --filetype <filetype>        amazon file type , EBOK PDOC [default: EBOK]
  --version                    Show version information
  -?, -h, --help               Show help and usage information
```

## 可选配置说明

如果不想在 命令行中输入 --cookie， 那就可以利用 cookie.txt 来保存。

初次运行会在当前目录生成一个 `cookie.txt`,将浏览器 `cookie` 粘贴到 `cookie.txt` 中，重新运行程序即可，也可以直接在当前目录手动创建一个 `cookie.txt` 文件，将浏览器 `cookie` 粘贴到 `cookie.txt` 中，运行程序

### cookie 的获取

登录亚马逊后，F12 打开浏览器控制台，然后找到任意请求，将网络请求中 cookie 对应的值，复制到 `cookie.txt`

## 运行

```bash
./KindleHelper.py --domain cn 
```

# 感谢

[Kindle_download_helper-Python](https://github.com/yihong0618/Kindle_download_helper)