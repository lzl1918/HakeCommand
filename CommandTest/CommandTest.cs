using HakeCommand.Framework;
using HakeCommand.Framework.Input.Internal;
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
            IInputCollection inputs;
            IInput command;
            string input = "test \"test abc ` a\" a;b;`\"c;\"123` \" c -Number 123 4` 54 -t";
            inputs = Input.Parse(input);
            Assert.AreEqual(input, inputs.Raw);
            Assert.AreEqual(1, inputs.Inputs.Count);
            command = inputs.Inputs[0];
            Assert.AreEqual("test", command.Name);
            Assert.AreEqual("123", command.Options["number"]);
            Assert.AreEqual(4, command.Arguments.Length);
            Assert.AreEqual("test abc  a", command.Arguments[0]);
            string[] arg1 = command.Arguments[1] as string[];
            Assert.AreNotEqual(null, arg1);
            Assert.AreEqual("\"c", arg1[2]);
            Assert.AreEqual("123 ", arg1[3]);
            Assert.AreEqual(4, arg1.Length);
            Assert.AreEqual(true, command.Options["t"]);
            Assert.AreEqual(2, command.Options.Count);

            input = "test -t";
            inputs = Input.Parse(input);
            Assert.AreEqual(input, inputs.Raw);
            Assert.AreEqual(1, inputs.Inputs.Count);
            command = inputs.Inputs[0];
            Assert.AreEqual(0, command.Arguments.Length);
            Assert.AreEqual(true, command.Options["t"]);

            input = "test -t ";
            inputs = Input.Parse(input);
            Assert.AreEqual(input, inputs.Raw);
            Assert.AreEqual(1, inputs.Inputs.Count);
            command = inputs.Inputs[0];
            Assert.AreEqual(0, command.Arguments.Length);
            Assert.AreEqual(true, command.Options["t"]);

            input = "test -t `\"ab;cd; - -n \"`012 34\"";
            inputs = Input.Parse(input);
            Assert.AreEqual(input, inputs.Raw);
            Assert.AreEqual(1, inputs.Inputs.Count);
            command = inputs.Inputs[0];
            Assert.AreEqual("test", command.Name);
            Assert.AreEqual("012 34", command.Options["n"]);
            Assert.AreEqual(0, command.Arguments.Length);
            arg1 = command.Options["t"] as string[];
            Assert.AreNotEqual(null, arg1);
            Assert.AreEqual("\"ab", arg1[0]);
            Assert.AreEqual(2, arg1.Length);
            Assert.AreEqual(2, command.Options.Count);

            input = "test | test -| t|v `\"t| sv -a|t a; b;c;|";
            inputs = Input.Parse(input);
            Assert.AreEqual(6, inputs.Inputs.Count);
            Assert.AreEqual("test", inputs.Inputs[0].Name);
            Assert.AreEqual("test", inputs.Inputs[1].Name);
            Assert.AreEqual("sv", inputs.Inputs[4].Name);
            command = inputs.Inputs[3];
            Assert.AreEqual("v", command.Name);
            Assert.AreEqual(1, command.Arguments.Length);
            Assert.AreEqual("\"t", command.Arguments[0]);
            Assert.AreEqual(true, inputs.Inputs[4].Options["a"]);
            Assert.AreEqual(2, inputs.Inputs[5].Arguments.Length);

            input = "test \"t\"| test \"t\";4 -test -test2 | test t;t;t| test";
            inputs = Input.Parse(input);
            Assert.AreEqual(4, inputs.Inputs.Count);
            Assert.AreEqual("test", inputs.Inputs[0].Name);
            Assert.AreEqual(1, inputs.Inputs[0].Arguments.Length);
            command = inputs.Inputs[1];
            Assert.AreEqual(1, command.Arguments.Length);
            Assert.AreEqual(true, command.Options["test"]);
            Assert.AreEqual(true, command.Options["test2"]);
            arg1 = command.Arguments[0] as string[];
            Assert.AreEqual("t", arg1[0]);
            Assert.AreEqual("4", arg1[1]);
            command = inputs.Inputs[2];
            Assert.AreEqual(1, command.Arguments.Length);
            arg1 = command.Arguments[0] as string[];
            Assert.AreEqual(3, arg1.Length);

            input = "test t;t;t`\";\"\";\"\"| test";
            inputs = Input.Parse(input);
            Assert.AreEqual(2, inputs.Inputs.Count);
            Assert.AreEqual("test", inputs.Inputs[0].Name);
            Assert.AreEqual(1, inputs.Inputs[0].Arguments.Length);
            command = inputs.Inputs[0];
            Assert.AreEqual(1, command.Arguments.Length);
            arg1 = command.Arguments[0] as string[];
            Assert.AreEqual(5, arg1.Length);
            Assert.AreEqual("t\"", arg1[2]);

            input = "test -v t -v c;t";
            inputs = Input.Parse(input);
            Assert.AreEqual(1, inputs.Inputs.Count);
            Assert.AreEqual("test", inputs.Inputs[0].Name);
            Assert.AreEqual(0, inputs.Inputs[0].Arguments.Length);
            command = inputs.Inputs[0];
            Assert.AreEqual(1, command.Options.Count);
            arg1 = command.Options["v"] as string[];
            Assert.AreEqual("c", arg1[0]);
            Assert.AreEqual("t", arg1[1]);

            input = "test -v t|c -v t`\"";
            inputs = Input.Parse(input);
            Assert.AreEqual(2, inputs.Inputs.Count);
            Assert.AreEqual("test", inputs.Inputs[0].Name);
            command = inputs.Inputs[0];
            Assert.AreEqual(1, command.Options.Count);
            Assert.AreEqual("t", command.Options["v"]);
            command = inputs.Inputs[1];
            Assert.AreEqual(1, command.Options.Count);
            Assert.AreEqual("t\"", command.Options["v"]);

            input = "test -v t;|c -v t`\"";
            inputs = Input.Parse(input);
            Assert.AreEqual(2, inputs.Inputs.Count);
            Assert.AreEqual("test", inputs.Inputs[0].Name);
            command = inputs.Inputs[0];
            Assert.AreEqual(1, command.Options.Count);
            Assert.AreEqual("t", command.Options["v"]);
            command = inputs.Inputs[1];
            Assert.AreEqual(1, command.Options.Count);
            Assert.AreEqual("t\"", command.Options["v"]);

            input = "test -v t;\"c -v t\"";
            inputs = Input.Parse(input);
            Assert.AreEqual(1, inputs.Inputs.Count);
            Assert.AreEqual("test", inputs.Inputs[0].Name);
            command = inputs.Inputs[0];
            Assert.AreEqual(1, command.Options.Count);
            arg1 = command.Options["v"] as string[];
            Assert.AreEqual("t", arg1[0]);
            Assert.AreEqual("c -v t", arg1[1]);

            input = "test -v t;`\"c";
            inputs = Input.Parse(input);
            Assert.AreEqual(1, inputs.Inputs.Count);
            Assert.AreEqual("test", inputs.Inputs[0].Name);
            command = inputs.Inputs[0];
            Assert.AreEqual(1, command.Options.Count);
            arg1 = command.Options["v"] as string[];
            Assert.AreEqual("t", arg1[0]);
            Assert.AreEqual("\"c", arg1[1]);
        }
    }
}
