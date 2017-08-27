using HakeCommand.Framework;
using HakeCommand.Framework.Input;
using HakeCommand.Framework.Services.Environment;
using HakeCommand.Framework.Services.OutputEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HakeCommand.Commands
{
    public sealed class ItemInfo
    {
        public FileSystemInfo Item { get; }
        public ItemInfo(FileSystemInfo item)
        {
            Item = item;
        }

        public FileSystemInfo OnGetValue() => Item;
        public IOutputInfo OnWrite()
        {
            List<IEnumerable<string>> bodyContents = new List<IEnumerable<string>>();
            bodyContents.Add(new string[] { "Name", "Type", "Length", "Last Modified" });
            if (Item is DirectoryInfo dirInfo)
                bodyContents.Add(new string[] { dirInfo.Name, "Directory", "", dirInfo.LastWriteTime.ToString("yyyy/MM/dd hh:mm:ss") });
            else if (Item is FileInfo fileInfo)
                bodyContents.Add(new string[] { fileInfo.Name, "File", fileInfo.Length.ToString(), fileInfo.LastWriteTime.ToString("yyyy/MM/dd hh:mm:ss") });
            List<IOutputBody> bodies = OutputInfo.CreateBodies(bodyContents);
            bodies.Insert(1, OutputInfo.CreateColumnLineSeperator());
            return OutputInfo.Create(null, bodies, "");
        }
    }
    public sealed class ItemInfoList
    {
        public string Directory { get; }
        private IList<ItemInfo> items;

        public ItemInfoList(string directory, List<ItemInfo> items)
        {
            Directory = directory;
            this.items = items;
        }

        public IList<ItemInfo> OnGetValue() => items;
        public IOutputInfo OnWrite()
        {
            List<IEnumerable<string>> bodyContents = new List<IEnumerable<string>>();
            bodyContents.Add(new string[] { "Name", "Type", "Length", "Last Modified" });
            foreach (ItemInfo info in items)
            {
                if (info.Item is DirectoryInfo dirInfo)
                    bodyContents.Add(new string[] { dirInfo.Name, "Directory", "", dirInfo.LastWriteTime.ToString("yyyy/MM/dd hh:mm:ss") });
                else if (info.Item is FileInfo fileInfo)
                    bodyContents.Add(new string[] { fileInfo.Name, "File", fileInfo.Length.ToString(), fileInfo.LastWriteTime.ToString("yyyy/MM/dd hh:mm:ss") });
            }
            List<IOutputBody> bodies = OutputInfo.CreateBodies(bodyContents);
            bodies.Insert(1, OutputInfo.CreateColumnLineSeperator());
            return OutputInfo.Create($"Contents of : {Directory}\n", bodies, "");
        }
    }
    public sealed class ListContentCommands : CommandSet
    {
        [DescriptionAttribute("Get items of a specific directory")]
        [Command("ls")]
        public ItemInfoList ListContent([Path]DirectoryInfo path)
        {
            if (!path.Exists)
                SetExceptionAndThrow(new Exception($"directory does not exist: {path.FullName}"));

            var files = path.EnumerateFiles();
            var directories = path.EnumerateDirectories();
            List<ItemInfo> result = new List<ItemInfo>();
            foreach (FileSystemInfo info in directories)
                result.Add(new ItemInfo(info));
            foreach (FileSystemInfo info in files)
                result.Add(new ItemInfo(info));
            return new ItemInfoList(path.FullName, result);
        }

        [DescriptionAttribute("Show file contents")]
        [Command("cat")]
        public string ShowFileContent([Path]FileInfo file)
        {
            if (file == null)
            {
                string input = ReadLine("file path");
                file = new FileInfo(Path.Combine(Environment.WorkingDirectory.FullName, input));
            }
            if (!file.Exists)
                SetExceptionAndThrow(new Exception($"file does not exist: {file.FullName}"));

            Stream stream = file.OpenRead();
            StreamReader reader = new StreamReader(stream);
            string content = reader.ReadToEnd();
            reader.Dispose();
            stream.Dispose();
            return content;
        }

        [DescriptionAttribute("Show the specific object")]
        [Command("echo")]
        public object ShowContent(object content)
        {
            content = content ?? InputObject;
            Context.WriteResult = true;
            return content;
        }
    }
}
