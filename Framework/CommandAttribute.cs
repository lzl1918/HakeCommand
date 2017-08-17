﻿using System;

namespace HakeCommand.Framework
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class CommandAttribute : Attribute
    {
        private readonly string name;

        public CommandAttribute(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get { return name; }
        }
    }


    public abstract class CommandSet
    {
    }
}
