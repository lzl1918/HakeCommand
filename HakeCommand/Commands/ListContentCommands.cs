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
    public sealed class ItemInfoList : IList<ItemInfo>, IEnumerable<ItemInfo>, IEnumerable, ICollection<ItemInfo>
    {
        public string Directory { get; }
        private IList<ItemInfo> items;

        public ItemInfoList(string directory, List<ItemInfo> items)
        {
            Directory = directory;
            this.items = items;
        }

        #region Implements of IList
        public ItemInfo this[int index] { get => items[index]; set => items[index] = value; }

        public int Count => items.Count;

        public bool IsReadOnly => items.IsReadOnly;

        public void Add(ItemInfo item)
        {
            items.Add(item);
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(ItemInfo item)
        {
            return items.Contains(item);
        }

        public void CopyTo(ItemInfo[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ItemInfo> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public int IndexOf(ItemInfo item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, ItemInfo item)
        {
            items.Insert(index, item);
        }

        public bool Remove(ItemInfo item)
        {
            return items.Remove(item);
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
        #endregion Implements of IList

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
        [Statement("Get items of a specific directory")]
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

        [Statement("Show file contents")]
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

        [Statement("Show the specific object")]
        [Command("echo")]
        public object ShowContent(object content)
        {
            content = content ?? InputObject;
            Context.WriteResult = true;
            return content;
        }
    }
}
