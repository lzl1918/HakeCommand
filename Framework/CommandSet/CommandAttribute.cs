using System;

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


    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class PathAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class DescriptionAttribute : Attribute
    {
        public string Description { get; }
        public DescriptionAttribute(string description)
        {
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }
    }

}
