using HakeCommand.Framework.Command;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CommandTest
{
    [TestClass]
    public class CommandTest
    {
        [TestMethod]
        public void TestParseCommand()
        {
            ICommand command;
            string input = @"test ""test abc \\ a"" a;b;\""c;""123\ "" c -Number 123 4\ 54 -t";
            command = InternalCommand.Parse(input);
            Assert.AreEqual(input, command.Raw);
            Assert.AreEqual("test", command.Command);
            Assert.AreEqual("123", command.Options["number"]);
            Assert.AreEqual(4, command.Arguments.Length);
            Assert.AreEqual("test abc \\ a", command.Arguments[0]);
            IList<string> arg1 = command.Arguments[1] as IList<string>;
            Assert.AreNotEqual(null, arg1);
            Assert.AreEqual("\"c", arg1[2]);
            Assert.AreEqual(4, arg1.Count);
            Assert.AreEqual(true, command.Options["t"]);
            Assert.AreEqual(2, command.Options.Count);

            input = @"test -t \""ab;cd; -n ""\012 34""";
            command = InternalCommand.Parse(input);
            Assert.AreEqual(input, command.Raw);
            Assert.AreEqual("test", command.Command);
            Assert.AreEqual("012 34", command.Options["n"]);
            Assert.AreEqual(0, command.Arguments.Length);
            arg1 = command.Options["t"] as IList<string>;
            Assert.AreNotEqual(null, arg1);
            Assert.AreEqual("\"ab", arg1[0]);
            Assert.AreEqual(2, arg1.Count);
            Assert.AreEqual(2, command.Options.Count);
        }
    }
}
